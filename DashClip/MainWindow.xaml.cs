using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DashClip
{
    public class ClipboardItem
    {
        public string Type { get; set; }
        public string Text { get; set; }
        public string App { get; set; }
        public DateTime Added { get; set; }
        public string Attachment { get; set; }
        public ImageSource Image { get; set; }

        public string Icon {
            get {
                return Image != null ? "resources/image32.png" : "resources/clipboard32.png";
            }
        }

        public string Preview {
            get {
                if (Text == null) return "";
                return Regex.Replace(Text, @"\s+", " ");
            }
        }

        public string FormattedTimestamp {
            get {
                return Added.ToString(Added.Date != DateTime.Now.Date ? "d" : "t");
            }
        }
    }

    public partial class MainWindow : Window
    {
        string dbPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\cliphistory.db";
        ObservableCollection<ClipboardItem> clipboardItems = new ObservableCollection<ClipboardItem>();
        CollectionViewSource filteredClipboardItems = new CollectionViewSource();
        TaskbarIcon trayIcon = new TaskbarIcon();

        public MainWindow() {
            InitializeComponent();
            Topmost = true;

            trayIcon.IconSource = Icon;
            trayIcon.TrayLeftMouseUp += onClickTrayIcon;
            
            filteredClipboardItems.Source = clipboardItems;
            listResults.ItemsSource = filteredClipboardItems.View;

            using (var db = new LiteDB.LiteDatabase(dbPath)) {
                var storage = db.GetCollection<ClipboardItem>("items");
                var items = storage.Find(LiteDB.Query.All("Added", LiteDB.Query.Descending), 0, 50);

                foreach (var item in items)
                    clipboardItems.Add(item);
            }

            var clipboard = new ClipboardMonitor();
            clipboard.ClipboardContentChanged += OnClipboardChanged;
        }

        private void onClickTrayIcon(object sender, RoutedEventArgs e) {
            WindowState = WindowState == WindowState.Minimized ? WindowState.Normal : WindowState.Minimized;
        }

        void OnClipboardChanged(object sender, EventArgs e) {
            var window = new ActiveWindow();

            var clipboardItem = new ClipboardItem {
                App = window.GetAppName(),
                Text = Clipboard.GetText(),
                Added = DateTime.Now
            };

            if (Clipboard.ContainsImage()) {
                clipboardItem.Type = "image";
                clipboardItem.Attachment = Guid.NewGuid().ToString();

                BitmapSource bmpSrc = Clipboard.GetImage();
                clipboardItem.Image = bmpSrc;
            }
            else if (Clipboard.ContainsText()) {
                clipboardItem.Type = "text";

                foreach (var item in clipboardItems)
                    if (item.Text == clipboardItem.Text) return;

                using (var db = new LiteDB.LiteDatabase(dbPath)) {
                    var storage = db.GetCollection<ClipboardItem>("items");
                    storage.Insert(clipboardItem);
                }
            } else return;

            clipboardItems.Insert(0, clipboardItem);
        }

        private void onSearchChanged(object sender, TextChangedEventArgs e) {
            filteredClipboardItems.View.Filter = item => {
                var clipItem = item as ClipboardItem;
                return clipItem.Text.ToLower().Contains(textSearch.Text.ToLower());
            };
        }

        private void onResultDoubleClick(object sender, MouseButtonEventArgs e) {
            this.Hide();
            var clipItem = listResults.SelectedItem as ClipboardItem;
            Clipboard.SetText(clipItem.Text);

            if (clipItem.Attachment != null)
                Clipboard.SetImage(clipItem.Image as BitmapSource);

            ActiveWindow.Paste();
            this.Show();
        }

        private void onStateChanged(object sender, EventArgs e) {
            if (WindowState == WindowState.Minimized) {
                this.ShowInTaskbar = false;
                trayIcon.ShowBalloonTip("DashClip", "DashClip is running in the background", BalloonIcon.Info);
            }
            else if (WindowState == WindowState.Normal) {
                ShowInTaskbar = true;
            }
        }
    }
}
