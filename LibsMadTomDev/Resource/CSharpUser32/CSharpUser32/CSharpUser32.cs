using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Media.Imaging;

namespace MadTomDev.Resource
{
    public static class CSharpUser32
    {
        public class OuterWindow
        {
            private IntPtr hWnd;
            public OuterWindow(IntPtr hWnd)
            { this.hWnd = hWnd; }
            public int ZOrder
            { get => GetWindowZOrder(hWnd); }
            public string Text
            { get => GetWindowText(hWnd); }
            public System.Windows.Point Position
            {
                get
                {
                    Int32Rect rect = GetWindowPosiNSize(hWnd);
                    return new System.Windows.Point(rect.X, rect.Y);
                }
                set => SetWindowPosition(hWnd, value);
            }
            public System.Windows.Size Size
            {
                get
                {
                    Int32Rect rect = GetWindowPosiNSize(hWnd);
                    return new System.Windows.Size(rect.Width, rect.Height);
                }
                set => SetWindowSize(hWnd, value);
            }
            public bool TopMost
            {
                get => CheckWindowIsTopMost(hWnd);
                set => SetWindowTopMost(hWnd, value);
            }
            public double Opacity
            {
                get
                {
                    double result;
                    GetWindowOpacity(hWnd, out result);
                    return result;
                }
                set => SetWindowOpacity(hWnd, value);
            }
            public byte OpacityByte
            {
                get
                {
                    byte result;
                    GetWindowOpacityByte(hWnd, out result);
                    return result;
                }
                set => SetWindowOpacityByte(hWnd, value);
            }
            public void ShowWindow()
            { ShowWindow(hWnd); }
            public void HideWindow()
            { HideWindow(hWnd); }
            public WindowState WindowState
            {
                get => GetWindowState(hWnd);
                set => SetWindowState(hWnd, value);
            }
            public BitmapSource GetIcon(int width = 16)
            { return GetWindowIcon(hWnd, width); }

            public IntPtr[] ChildWindowsHandles
            { get => GetChildWindows(hWnd); }

            public bool WindowClosed_ListenerAdded
            { private set; get; } = false;
            public delegate void WindowClosedDelegate(IntPtr closedHWnd);
            public event WindowClosedDelegate WindowClosed;
            public void AddListener_WindowClosed()
            {
                if (WindowClosed_ListenerAdded)
                    return;
                var element = AutomationElement.FromHandle(hWnd);
                Automation.AddAutomationEventHandler(
                    WindowPattern.WindowClosedEvent, element,
                    TreeScope.Subtree, (s1, e1) =>
                    {
                        WindowClosed?.Invoke(hWnd);
                    });
                WindowClosed_ListenerAdded = true;
            }







            public static IntPtr[] GetAllTopWindowsHandles()
            {
                HashSet<IntPtr> result = new HashSet<IntPtr>();
                Process[] processlist = Process.GetProcesses();
                foreach (Process process in processlist)
                {
                    //if (!String.IsNullOrEmpty(process.MainWindowTitle))
                    //{
                    //    Console.WriteLine("Process: {0} ID: {1} Window title: {2}", process.ProcessName, process.Id, process.MainWindowTitle);
                    //}
                    result.Add(process.MainWindowHandle);
                }
                return result.ToArray();
            }

            #region wrap
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc2 lpEnumFunc, IntPtr lParam);
            private delegate bool EnumWindowsProc2(IntPtr hWnd, IntPtr lParam);
            private static HashSet<IntPtr> GetChildWindows_list;
            private static bool EnumChildWindow(IntPtr hWnd, IntPtr lParam)
            {
                GetChildWindows_list.Add(hWnd);
                return true;
            }
            #endregion
            public static IntPtr[] GetChildWindows(IntPtr parentWnd)
            {
                GetChildWindows_list = new HashSet<IntPtr>();
                GCHandle listHandle = GCHandle.Alloc(GetChildWindows_list);
                try
                {
                    EnumWindowsProc2 childProc = new EnumWindowsProc2(EnumChildWindow);
                    EnumChildWindows(parentWnd, childProc, GCHandle.ToIntPtr(listHandle));
                }
                finally
                {
                    if (listHandle.IsAllocated)
                        listHandle.Free();
                }
                return GetChildWindows_list.ToArray();
            }

            #region wrap
            private delegate bool EnumDesktopWindowsDelegate(IntPtr hWnd, int lParam);
            [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
                ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
            static extern bool EnumDesktopWindows(IntPtr hDesktop,
                EnumDesktopWindowsDelegate lpfn, IntPtr lParam);
            private delegate bool EnumDelegate(IntPtr hWnd, int lParam);
            private static bool EnumWindowsProc(IntPtr hWnd, int lParam)
            {
                getAllOpenWindowHandles_result.Add(hWnd);
                return true;
            }
            [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
                ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
            private static extern bool _EnumDesktopWindows(IntPtr hDesktop,
                EnumDelegate lpEnumCallbackFunction, IntPtr lParam);
            private static List<IntPtr> getAllOpenWindowHandles_result;

            [DllImport("user32.dll", EntryPoint = "WindowFromPoint")]
            public static extern IntPtr WindowFromPoint(int x, int y);
            #endregion
            public static IntPtr[] GetAllOpenWindowHandles()
            {
                //foreach (AutomationElement ae in AutomationElement.RootElement.FindAll(
                //    TreeScope.Children, Condition.TrueCondition))
                //{
                //    ae.
                //    name += ae.Current.Name + "/r/n";

                //}


                getAllOpenWindowHandles_result = new List<IntPtr>();
                EnumDelegate enumfunc = new EnumDelegate(EnumWindowsProc);
                IntPtr hDesktop = IntPtr.Zero; // current desktop
                bool success = _EnumDesktopWindows(hDesktop, enumfunc, IntPtr.Zero);

                if (success)
                {
                    return getAllOpenWindowHandles_result.ToArray();
                }
                else
                {
                    // Get the last Win32 error code
                    int errorCode = Marshal.GetLastWin32Error();

                    string errorMessage = String.Format(
                    "EnumDesktopWindows failed with code {0}.", errorCode);
                    throw new Exception(errorMessage);
                }
            }


            #region wrap
            [DllImport("user32.dll", EntryPoint = "GetTopWindow")]
            #endregion
            public static extern IntPtr GetTopWindow(IntPtr parentHWnd);
            public static IntPtr GetTopWindow()
            {
                return GetTopWindow(IntPtr.Zero);
            }
            #region wrap
            [DllImport("user32.dll")]
            private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
            const uint GW_HWNDPREV = 3;
            const uint GW_HWNDLAST = 1;
            #endregion

            public static int GetWindowZOrder(IntPtr hwnd)
            {
                var lowestHwnd = GetWindow(hwnd, GW_HWNDLAST);

                var z = 0;
                var hwndTmp = lowestHwnd;
                while (hwndTmp != IntPtr.Zero)
                {
                    if (hwnd == hwndTmp)
                    {
                        return z;
                    }

                    hwndTmp = GetWindow(hwndTmp, GW_HWNDPREV);
                    z++;
                }

                return int.MinValue;
            }

            //public static IntPtr GetTopWindow(IntPtr parentHWnd)
            //{
            //    return GetTopWindow(parentHWnd);
            //}

            #region wrap
            [DllImport("user32.dll", EntryPoint = "GetWindowText",
                ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
            private static extern int _GetWindowText(IntPtr hWnd,
                StringBuilder lpWindowText, int nMaxCount);
            #endregion
            public static string GetWindowText(IntPtr hWnd)
            {
                // for max filename length is 255
                StringBuilder title = new StringBuilder(255);
                int titleLength = _GetWindowText(hWnd, title, title.Capacity + 1);
                title.Length = titleLength;

                return title.ToString();
            }

            #region wrap
            [DllImport("user32.dll")]
            private static extern bool GetWindowRect(IntPtr hwnd, ref Rect wndRect);
            private struct Rect
            {
                public int Left { get; set; }
                public int Top { get; set; }
                public int Right { get; set; }
                public int Bottom { get; set; }
            }
            #endregion

            public static Int32Rect GetWindowPosiNSize(IntPtr hWnd)
            {
                Rect wndRect = new Rect();
                GetWindowRect(hWnd, ref wndRect);
                return new Int32Rect(
                    wndRect.Left, wndRect.Top,
                    (wndRect.Right - wndRect.Left),
                    (wndRect.Bottom - wndRect.Top));
            }


            #region wrap
            [DllImport("user32.dll")]
            private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
            private const UInt32 SWP_NOSIZE = 0x0001;
            private const UInt32 SWP_NOMOVE = 0x0002;
            private const UInt32 SWP_NOZORDER = 0x0004;
            private const UInt32 SWP_NOACTIVATE = 0x0010;
            private const UInt32 SWP_SHOWWINDOW = 0x0040;
            private const UInt32 SWP_HIDEWINDOW = 0x0080;
            private const UInt32 SWP_FRAMECHANGED = 0x0020;

            private static IntPtr HWND_BOTTOM = new IntPtr(1);
            private static IntPtr HWND_TOP = new IntPtr(0);
            private static IntPtr HWND_TOPMOST = new IntPtr(-1);
            private static IntPtr HWND_NOTOPMOST = new IntPtr(-2);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
            const int GWL_EXSTYLE = -20;
            const int WS_EX_TOPMOST = 0x0008;

            #endregion


            #region set window top-most, position
            public static bool CheckWindowIsTopMost(IntPtr hWnd)
            {
                int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
                return (exStyle & WS_EX_TOPMOST) == WS_EX_TOPMOST;
            }
            public static bool SetWindowTopMost(IntPtr hWnd, bool isTopMost)
            {
                // sometimes it will not work,
                // so you set it nothing first, then it will work...
                SetWindowPos(hWnd, IntPtr.Zero,
                    0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER);
                if (isTopMost)
                {
                    return SetWindowPos(hWnd, HWND_TOPMOST,
                       0, 0, 0, 0,
                        SWP_NOMOVE | SWP_NOSIZE);
                }
                else
                {
                    return SetWindowPos(hWnd, HWND_NOTOPMOST,
                        0, 0, 0, 0,
                        SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                }
            }
            public static bool SetWindowPosition(IntPtr hWnd, System.Windows.Point newPosi)
            { return SetWindowPosition(hWnd, (int)(newPosi.X + 0.5), (int)(newPosi.Y + 0.5)); }
            public static bool SetWindowPosition(IntPtr hWnd, int x, int y)
            {
                return SetWindowPos(hWnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
            }
            public static bool SetWindowSize(IntPtr hWnd, System.Windows.Size newSize)
            { return SetWindowSize(hWnd, (int)(newSize.Width + 0.5), (int)(newSize.Height + 0.5)); }
            public static bool SetWindowSize(IntPtr hWnd, int width, int height)
            {
                return SetWindowPos(hWnd, IntPtr.Zero, 0, 0, width, height, SWP_NOMOVE | SWP_NOZORDER | SWP_FRAMECHANGED | SWP_NOACTIVATE);
            }

            public static bool ShowWindow(IntPtr hWnd)
            {
                return SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }
            public static bool HideWindow(IntPtr hWnd)
            {
                return SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_HIDEWINDOW);
            }
            #endregion

            #region wrap
            [DllImport("user32.dll", SetLastError = true)]
            static extern bool GetLayeredWindowAttributes(IntPtr hwnd, out uint crKey, out byte bAlpha, out uint dwFlags);
            [DllImport("user32.dll")]
            private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
            [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
            private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

            //private const int GWL_EXSTYLE = -20;
            private const int WS_EX_LAYERED = 0x80000;
            private const int LWA_ALPHA = 0x2;
            private const int LWA_COLORKEY = 0x1;
            #endregion


            #region set window opacity
            public static bool GetWindowOpacityByte(IntPtr hWnd, out byte opacityByte)
            {
                uint cKey, uFlag = LWA_ALPHA;
                return GetLayeredWindowAttributes(hWnd, out cKey, out opacityByte, out uFlag);
            }
            public static bool GetWindowOpacity(IntPtr hWnd, out double opacity)
            {
                byte bAlpha;
                bool result = GetWindowOpacityByte(hWnd, out bAlpha);
                if (bAlpha == 0)
                    opacity = 1;
                else
                    opacity = (double)bAlpha / 255;
                return result;
            }


            public static bool SetWindowOpacityByte(IntPtr hWnd, byte newOpacityByte)
            {
                bool result = false;
                byte preAlpha;
                GetWindowOpacityByte(hWnd, out preAlpha);
                if (preAlpha == newOpacityByte)
                    return true;
                do
                {
                    SetWindowLong(hWnd, GWL_EXSTYLE, GetWindowLong(hWnd, GWL_EXSTYLE) ^ WS_EX_LAYERED);
                    result = SetLayeredWindowAttributes(hWnd, 0, newOpacityByte, LWA_ALPHA);
                    //SetWindowLong(hWnd, GWL_EXSTYLE, GetWindowLong(hWnd, GWL_EXSTYLE) ^ WS_EX_LAYERED);

                    GetWindowOpacityByte(hWnd, out preAlpha);
                }
                while (!result || preAlpha != newOpacityByte);
                return result;
            }
            public static bool SetWindowOpacity(IntPtr hWnd, double newOpacity)
            {
                if (newOpacity < 0) newOpacity = 0;
                else if (newOpacity > 1) newOpacity = 1;
                byte alpha = (byte)(newOpacity * 255);
                if (alpha < 1)
                    alpha = 1;
                return SetWindowOpacityByte(hWnd, alpha);
            }
            #endregion


            #region wrap
            [DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

            [DllImport("user32.dll", EntryPoint = "GetClassLong")]
            private static extern uint GetClassLong32(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
            private static extern IntPtr GetClassLong64(IntPtr hWnd, int nIndex);

            /// <summary>
            /// 64 bit version maybe loses significant 64-bit specific information
            /// </summary>
            private static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
            {
                if (IntPtr.Size == 4)
                    return new IntPtr((long)GetClassLong32(hWnd, nIndex));
                else
                    return GetClassLong64(hWnd, nIndex);
            }


            private static uint WM_GETICON = 0x007f;
            private static IntPtr ICON_SMALL2 = new IntPtr(2);
            private static IntPtr IDI_APPLICATION = new IntPtr(0x7F00);
            private static int GCL_HICON = -14;
            private static BitmapSource _GetWindowIcon(IntPtr hWnd, int width)
            {
                try
                {
                    IntPtr hIcon = default(IntPtr);
                    hIcon = SendMessage(hWnd, WM_GETICON, ICON_SMALL2, IntPtr.Zero);

                    if (hIcon == IntPtr.Zero)
                        hIcon = GetClassLongPtr(hWnd, GCL_HICON);

                    if (hIcon == IntPtr.Zero)
                        hIcon = LoadIcon(IntPtr.Zero, (IntPtr)0x7F00/*IDI_APPLICATION*/);

                    if (hIcon != IntPtr.Zero)
                        return UI.QuickGraphics.BitmapToBitmapSource(
                            new Bitmap(Icon.FromHandle(hIcon).ToBitmap(), width, width));
                    else
                        return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            #endregion

            #region get window icon
            public static BitmapSource GetWindowIcon(IntPtr hWnd, int size = 32)
            { return _GetWindowIcon(hWnd, size); }
            public static BitmapSource GetWindowIcon16(IntPtr hWnd)
            { return _GetWindowIcon(hWnd, 16); }
            public static BitmapSource GetWindowIcon32(IntPtr hWnd)
            { return _GetWindowIcon(hWnd, 32); }
            public static BitmapSource GetWindowIcon48(IntPtr hWnd)
            { return _GetWindowIcon(hWnd, 48); }
            public static BitmapSource GetWindowIcon64(IntPtr hWnd)
            { return _GetWindowIcon(hWnd, 64); }

            #endregion


            #region wrap
            [DllImport("user32.dll")]
            private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
            private const int SW_SHOWNORMAL = 1;
            private const int SW_SHOWMINIMIZED = 2;
            private const int SW_SHOWMAXIMIZED = 3;

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

            private struct WINDOWPLACEMENT
            {
                public int length;
                public int flags;
                public int showCmd;
                public System.Drawing.Point ptMinPosition;
                public System.Drawing.Point ptMaxPosition;
                public System.Drawing.Rectangle rcNormalPosition;
            }
            #endregion

            public static WindowState GetWindowState(IntPtr hWnd)
            {
                WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                GetWindowPlacement(hWnd, ref placement);
                switch (placement.showCmd)
                {
                    //case 1:
                    //    return FormWindowState.Normal;
                    case 2:
                        return WindowState.Minimized;
                    case 3:
                        return WindowState.Maximized;
                }
                return WindowState.Normal;
            }
            public static void SetWindowState(IntPtr hWnd, WindowState newFWState)
            {
                switch (newFWState)
                {
                    case WindowState.Minimized:
                        ShowWindowAsync(hWnd, SW_SHOWMINIMIZED);
                        break;
                    case WindowState.Maximized:
                        ShowWindowAsync(hWnd, SW_SHOWMAXIMIZED);
                        break;
                    case WindowState.Normal:
                        ShowWindowAsync(hWnd, SW_SHOWNORMAL);
                        break;
                }
            }





        }

        public class StandardDialogs
        {
            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct SHELLEXECUTEINFO
            {
                public int cbSize;
                public uint fMask;
                public IntPtr hwnd;
                [MarshalAs(UnmanagedType.LPTStr)]
                public string lpVerb;
                [MarshalAs(UnmanagedType.LPTStr)]
                public string lpFile;
                [MarshalAs(UnmanagedType.LPTStr)]
                public string lpParameters;
                [MarshalAs(UnmanagedType.LPTStr)]
                public string lpDirectory;
                public int nShow;
                public IntPtr hInstApp;
                public IntPtr lpIDList;
                [MarshalAs(UnmanagedType.LPTStr)]
                public string lpClass;
                public IntPtr hkeyClass;
                public uint dwHotKey;
                public IntPtr hIcon;
                public IntPtr hProcess;
            }

            private const int SW_SHOW = 5;
            private const uint SEE_MASK_INVOKEIDLIST = 12;
            public static bool ShowFileProperties(string filename)
            {
                SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
                info.cbSize = Marshal.SizeOf(info);
                info.lpVerb = "properties";
                info.lpFile = filename;
                info.nShow = SW_SHOW;
                info.fMask = SEE_MASK_INVOKEIDLIST;
                return ShellExecuteEx(ref info);
            }
        }
    }
}
