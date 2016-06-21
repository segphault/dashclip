# DashClip

DashClip is a clipboard history manager for Windows built with C# and WPF. It records a log of content copied to the clipboard so that you can easily go back and paste items that were previously in the clipboard history.

# Limitations

* It currently captures plain text only, without formatting
* Although it captures images, they are not stored in the database and don't persist across runs

## Third-party Components

* Icons from the [Icons8](https://icons8.com) collection (CC-BY-ND)
* Tray icon library [WPF NotifyIcon](http://www.hardcodet.net/wpf-notifyicon) (CPOL)
* Data persistence with [LiteDB](http://www.litedb.org/) (MIT)