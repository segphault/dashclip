using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace DashClip
{
    public sealed class ClipboardMonitor : IDisposable
    {
        // Taken from here:
        // http://codereview.stackexchange.com/questions/115417/monitoring-the-clipboard

        private static class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AddClipboardFormatListener(IntPtr hwnd);


            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

            public const int WM_CLIPBOARDUPDATE = 0x031D;
            public static IntPtr HWND_MESSAGE = new IntPtr(-3);
        }

        private HwndSource hwndSource = new HwndSource(0, 0, 0, 0, 0, 0, 0, null, NativeMethods.HWND_MESSAGE);
        private bool disposed = false;

        public ClipboardMonitor()
        {
            hwndSource.AddHook(WndProc);
            NativeMethods.AddClipboardFormatListener(hwndSource.Handle);
        }

        public void Dispose()
        {
            if (disposed) return;

            NativeMethods.RemoveClipboardFormatListener(hwndSource.Handle);
            hwndSource.RemoveHook(WndProc);
            hwndSource.Dispose();
            disposed = true;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_CLIPBOARDUPDATE)
                ClipboardContentChanged?.Invoke(this, EventArgs.Empty);

            return IntPtr.Zero;
        }

        public event EventHandler ClipboardContentChanged;
    }
}
