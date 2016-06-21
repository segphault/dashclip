using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DashClip
{
    public class ActiveWindow
    {
        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr window, EnumWindowsProc callback, IntPtr i);

        private static bool EnumWindow(IntPtr handle, IntPtr pointer) {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            List<IntPtr> list = gch.Target as List<IntPtr>;

            if (list == null)
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");

            list.Add(handle);
            return true;
        }

        public static List<IntPtr> GetChildWindows(IntPtr parent) {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);

            try {
                EnumWindowsProc childProc = new EnumWindowsProc(EnumWindow);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }

            return result;
        }

        Process windowProcess;
        
        public ActiveWindow() {
            var window = GetForegroundWindow();
            windowProcess = GetWindowProcess(window);

            if (windowProcess.ProcessName == "ApplicationFrameHost")
                windowProcess = GetWindowProcess(GetChildWindows(window)[0]);
        }

        public Process GetWindowProcess(IntPtr w) {
            uint procId;
            GetWindowThreadProcessId(w, out procId);
            return Process.GetProcessById((int)procId);
        }

        public string GetAppName() {
            if (windowProcess == null) return null;
            return windowProcess.MainModule.FileVersionInfo.ProductName;
        }

        public Icon GetAppIcon() {
            return windowProcess != null ?
                   Icon.ExtractAssociatedIcon(windowProcess.MainModule.FileName) : null;
        }

        public static void Paste() {
            keybd_event((byte)0x11, 0, 0, 0);
            keybd_event((byte)0x56, 0, 0, 0);

            keybd_event((byte)0x11, 0, (byte)0x0002, 0);
            keybd_event((byte)0x56, 0, (byte)0x0002, 0);
        }
    }
}
