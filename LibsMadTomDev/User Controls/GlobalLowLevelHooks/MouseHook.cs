using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace MadTomDev.UI
{
    // origin https://github.com/rvknth043/Global-Low-Level-Key-Board-And-Mouse-Hook

    /// <summary>
    /// Class for intercepting low level Windows mouse hooks.
    /// </summary>
    public class MouseHook : IDisposable
    {
        /// <summary>
        /// Internal callback processing function
        /// </summary>
        private delegate IntPtr MouseHookHandler(int nCode, IntPtr wParam, IntPtr lParam);
        private MouseHookHandler hookHandler;

        /// <summary>
        /// Function to be called when defined even occurs
        /// </summary>
        /// <param name="mouseStruct">MSLLHOOKSTRUCT mouse structure</param>
        public delegate void MouseHookCallback(MSLLHOOKSTRUCT mouseStruct);

        #region Events
        public event MouseHookCallback? LeftButtonDown;
        public event MouseHookCallback? LeftButtonUp;
        public event MouseHookCallback? RightButtonDown;
        public event MouseHookCallback? RightButtonUp;
        public event MouseHookCallback? MouseMove;
        public event MouseHookCallback? MouseWheel;
        /// <summary>
        /// 经测试，无论双击程序窗口还是窗外，均不触发；
        /// Posted when the user double-clicks the first or second X button while the cursor is in the nonclient area of a window.
        /// This message is posted to the window that contains the cursor.
        /// If a window has captured the mouse, this message is not posted.
        /// </summary>
        public event MouseHookCallback? DoubleClick_nonclient;
        /// <summary>
        /// 双击（两次同键，在双击时间内按下）
        /// mouseData = 0时 指示左键双击，1-右键双击，2-中键双击
        /// </summary>
        public event MouseHookCallback? DoubleClick;
        public event MouseHookCallback? MiddleButtonDown;
        public event MouseHookCallback? MiddleButtonUp;
        #endregion

        #region keyDown state
        public bool isLeftDown
        { private set; get; } = false;
        public bool isRightDown
        { private set; get; } = false;
        public bool isMiddleDown
        { private set; get; } = false;
        #endregion

        /// <summary>
        /// Low level mouse hook's ID
        /// </summary>
        private IntPtr hookID = IntPtr.Zero;

        /// <summary>
        /// Install low level mouse hook
        /// </summary>
        /// <param name="mouseHookCallbackFunc">Callback function</param>
        public MouseHook()
        {
            hookHandler = HookFunc;
            hookID = SetHook(hookHandler);
        }

        /// <summary>
        /// Destructor. Unhook current hook
        /// Remove low level mouse hook
        /// </summary>
        public void Dispose()
        {
            if (hookID == IntPtr.Zero)
                return;

            UnhookWindowsHookEx(hookID);
            hookID = IntPtr.Zero;
        }

        /// <summary>
        /// Sets hook and assigns its ID for tracking
        /// </summary>
        /// <param name="proc">Internal callback function</param>
        /// <returns>Hook ID</returns>
        private IntPtr SetHook(MouseHookHandler proc)
        {
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(module.ModuleName), 0);
        }

        /// <summary>
        /// Callback function
        /// </summary>
        private IntPtr HookFunc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // parse system messages
            if (nCode >= 0)
            {
                MouseMessages curMM = (MouseMessages)wParam;
                MSLLHOOKSTRUCT mhs = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                if (MouseMessages.WM_LBUTTONDOWN == curMM)
                {
                    isLeftDown = true;
                    LeftButtonDown?.Invoke(mhs);
                    CheckFireDoubleClick(curMM, mhs, 0);
                }
                if (MouseMessages.WM_LBUTTONUP == curMM)
                {
                    isLeftDown = false;
                    LeftButtonUp?.Invoke(mhs);
                }
                if (MouseMessages.WM_RBUTTONDOWN == curMM)
                {
                    isRightDown = true;
                    RightButtonDown?.Invoke(mhs);
                    CheckFireDoubleClick(curMM, mhs, 1);
                }
                if (MouseMessages.WM_RBUTTONUP == curMM)
                {
                    isRightDown = false;
                    RightButtonUp?.Invoke(mhs);
                }
                if (MouseMessages.WM_MOUSEMOVE == curMM)
                {
                    MouseMove?.Invoke(mhs);
                }
                if (MouseMessages.WM_MOUSEWHEEL == curMM)
                {
                    MouseWheel?.Invoke(mhs);
                }
                if (MouseMessages.WM_LBUTTONDBLCLK == curMM)
                {
                    DoubleClick_nonclient?.Invoke(mhs);
                }
                if (MouseMessages.WM_MBUTTONDOWN == curMM)
                {
                    isMiddleDown = true;
                    MiddleButtonDown?.Invoke(mhs);
                    CheckFireDoubleClick(curMM, mhs, 2);
                }
                if (MouseMessages.WM_MBUTTONUP == curMM)
                {
                    isMiddleDown = false;
                    MiddleButtonUp?.Invoke(mhs);
                }
            }
            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        private MouseMessages preMouseDownMsg = MouseMessages.WM_LBUTTONUP;
        private uint preMouseDownTime = 0;
        private void CheckFireDoubleClick(MouseMessages mm, MSLLHOOKSTRUCT mhs, int btnIdx)
        {
            if (DoubleClick == null)
                return;
            if ((btnIdx == 0 && (isRightDown || isMiddleDown))
                || (btnIdx == 1 && (isLeftDown || isMiddleDown))
                || (btnIdx == 2 && (isLeftDown || isRightDown)))
            {
                preMouseDownMsg = MouseMessages.WM_RBUTTONUP;
                return;
            }

            if (preMouseDownMsg == mm && (mhs.time - preMouseDownTime) <= DoubleClickTime)
            {
                mhs.mouseData = btnIdx;
                DoubleClick(mhs);
                preMouseDownMsg = MouseMessages.WM_RBUTTONUP;
            }
            else
            {
                preMouseDownMsg = mm;
                preMouseDownTime = mhs.time;
            }
        }

        #region WinAPI

        public uint _DoubleClickTime = 0;
        public uint DoubleClickTime
        {
            get
            {
                if (_DoubleClickTime == 0)
                {
                    _DoubleClickTime = GetDoubleClickTime();
                }
                return _DoubleClickTime;
            }
        }
        [DllImport("user32.dll")]
        static extern uint GetDoubleClickTime();


        private const int WH_MOUSE_LL = 14;

        private enum MouseMessages
        {
            WM_MOUSEMOVE = 0x0200,

            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_LBUTTONDBLCLK = 0x0203,

            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,

            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208,

            WM_MOUSEWHEEL = 0x020A,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
            public override string ToString()
            {
                return $"{x}, {y}";
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            /// <summary>
            /// wheel delta;
            /// push-up    7864320;
            /// pull-down -7864320;
            /// </summary>
            public int mouseData;
            /// <summary>
            /// The event-injected flags. 
            /// 0x00000001  LLMHF_INJECTED           Test the event-injected (from any process) flag.
            /// 0x00000002  LLMHF_LOWER_IL_INJECTED  Test the event-injected (from a process running at lower integrity level) flag.
            /// </summary>
            public uint flags;
            /// <summary>
            /// The time stamp for this message.
            /// 专用戳，无法通过new datetime转换为当前时间；
            /// </summary>
            public uint time;
            /// <summary>
            /// Additional information associated with the message.
            /// Minimum supported client	Windows 2000 Professional [desktop apps only]
            /// Minimum supported server	Windows 2000 Server [desktop apps only]
            /// Require Header	winuser.h (include Windows.h)
            /// </summary>
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            MouseHookHandler lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion
    }
}
