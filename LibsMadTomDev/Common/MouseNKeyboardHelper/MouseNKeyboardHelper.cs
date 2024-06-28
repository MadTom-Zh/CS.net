using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace MadTomDev.Common
{
    public class MouseNKeyboardHelper
    {
        public static DragDropEffects GetDemandDragDropEffect(
            DragDropKeyStates keyState,
            DragDropEffects allowedEffects,
            DragDropEffects defaultDragDropEffect = DragDropEffects.Copy)
        {
            if (allowedEffects == DragDropEffects.None)
                return DragDropEffects.None;

            if ((keyState.HasFlag(DragDropKeyStates.ControlKey) && keyState.HasFlag(DragDropKeyStates.AltKey))
                && (allowedEffects & DragDropEffects.Link) == DragDropEffects.Link)
            {
                // KeyState 8 + 32 = CTL + ALT
                // Link drag-and-drop effect.
                return DragDropEffects.Link;
            }
            else if (keyState.HasFlag(DragDropKeyStates.AltKey)
                && (allowedEffects & DragDropEffects.Link) == DragDropEffects.Link)
            {
                // ALT KeyState for link.
                return DragDropEffects.Link;
            }
            else if (keyState.HasFlag(DragDropKeyStates.ShiftKey)
                && (allowedEffects & DragDropEffects.Move) == DragDropEffects.Move)
            {

                // SHIFT KeyState for move.
                return DragDropEffects.Move;
            }
            else if (keyState.HasFlag(DragDropKeyStates.ControlKey)
                && (allowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy)
            {

                // CTL KeyState for copy.
                return DragDropEffects.Copy;
            }
            //else if ((allowedEffects & DragDropEffects.Move) == DragDropEffects.Move)
            else if ((allowedEffects & defaultDragDropEffect) == defaultDragDropEffect)
            {
                // By default, the drop action should be move, if allowed.
                //return DragDropEffects.Move;

                // or not, by coder decision
                return defaultDragDropEffect;
            }
            else
            {
                return DragDropEffects.None;
            }
        }


        public static DragDropEffects GetDemandDragDropEffect(int keyState, DragDropEffects defaultDragDropEffect = DragDropEffects.Copy)
        {

            if ((keyState & (8 + 32)) == (8 + 32))
            {
                // KeyState 8 + 32 = CTL + ALT
                // Link drag-and-drop effect.
                return DragDropEffects.Link;
            }
            else if ((keyState & 32) == 32)
            {
                // ALT KeyState for link.
                return DragDropEffects.Link;
            }
            else if ((keyState & 4) == 4)
            {
                // SHIFT KeyState for move.
                return DragDropEffects.Move;
            }
            else if ((keyState & 8) == 8)
            {
                // CTL KeyState for copy.
                return DragDropEffects.Copy;
            }
            else
            {
                return defaultDragDropEffect;
            }
        }

        public static void DragFiles(UIElement ui, params string[] files)
        {
            DragDrop.DoDragDrop(ui, new DataObject(DataFormats.FileDrop, files),
                 DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
        }
        public static void DragFiles(UIElement ui, DragDropEffects ddEffects, params string[] files)
        {
            DragDrop.DoDragDrop(ui, new DataObject(DataFormats.FileDrop, files), ddEffects);
        }

        /// <summary>
        /// 检查是否可以将一批文件，投放到某个文件夹，通过条件为：
        /// “一批文件” 不 为空；
        /// 目标目录 不是 “一批文件”的子目录；
        /// “一批文件” 不是 目标目录的直接子集；
        /// 目标目录 不是 “一批文件”的其中一员；
        /// </summary>
        /// <param name="files"></param>
        /// <param name="targetDir"></param>
        /// <param name="withinSameDir"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool CheckFileDrops(string[] files, string targetDir, out bool sameDrive, out bool withinSameDir, out Exception error)
        {
            error = null;
            withinSameDir = false;
            sameDrive = false;
            if (files == null || files.Length == 0)
            {
                // nothing to drag
                error = new ArgumentNullException("No files.");
                return false;
            }
            sameDrive = true;
            string tarRoot = GetRoot(targetDir);
            foreach (string testF in files)
            {
                if (tarRoot != GetRoot(testF))
                    sameDrive = false;
                if (targetDir.StartsWith(testF + System.IO.Path.DirectorySeparatorChar))
                {
                    // parent drag into sub ?
                    error = new InvalidOperationException("Can't make parent the sub of its sub.");
                    return false;
                }
                if (System.IO.Path.GetDirectoryName(testF) == targetDir)
                {
                    // sub drag into its own parent ?
                    withinSameDir = true;
                    error = new InvalidOperationException("Drag to where its belong.");
                    return false;
                }
                if (testF == targetDir)
                {
                    // self drag into self ?
                    error = new InvalidOperationException("Can't make it both parent and sub.");
                    return false;
                }
            }
            string GetRoot(string path)
            {
                if (path.StartsWith("\\\\"))
                    return Data.Utilities.FilePath.GetUNCRoot(path);
                else
                    return path.Substring(0, 2);
            }
            return true;
        }


        /// <summary>
        /// 获取某个窗口的按键组合，如Ctrl+C；
        /// 使用时，监听KeyDown事件，按CommonKeyMeans即可获取常见命令组合键；
        /// </summary>
        public class KeyboardListener
        {
            private Window senderWindow = null;
            public KeyboardListener(Window window)
            {
                senderWindow = window;
                window.PreviewKeyDown += Window_PreviewKeyDown;
            }


            private bool _Listening = true;
            public bool Listening
            {
                get => _Listening;
                set
                {
                    if (_Listening == value)
                        return;

                    if (senderWindow != null)
                    {
                        senderWindow.PreviewKeyDown -= Window_PreviewKeyDown;
                        if (value)
                            senderWindow.PreviewKeyDown += Window_PreviewKeyDown;
                    }

                    _Listening = value;
                }
            }
            public bool ListeningSingleKey { get; set; } = true;
            public bool ListeningCombineKeys { get; set; } = true;

            public delegate void KeyDownDelegate(
                object sender, KeyEventArgs e,
                bool ctrl, bool shift, bool alt,
                CommonKeyMeans means);
            public event KeyDownDelegate KeyDown;


            private void AnaliseKeyEvent(object sender, KeyEventArgs e)
            {
                bool ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                bool shiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                bool altDown = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
                CommonKeyMeans means = CommonKeyMeans.Unknow;
                if (ListeningCombineKeys &&
                    (ctrlDown || shiftDown || altDown))
                {

                    if (ctrlDown && !shiftDown && !altDown)
                    {
                        switch (e.Key)
                        {
                            case Key.X:
                                means = CommonKeyMeans.Cut;
                                break;
                            case Key.C:
                                means = CommonKeyMeans.Copy;
                                break;
                            case Key.V:
                                means = CommonKeyMeans.Paste;
                                break;

                            case Key.N:
                                means = CommonKeyMeans.New;
                                break;
                            case Key.O:
                                means = CommonKeyMeans.Open;
                                break;
                            case Key.P:
                                means = CommonKeyMeans.Print;
                                break;

                            case Key.B:
                                means = CommonKeyMeans.Bold;
                                break;
                            case Key.I:
                                means = CommonKeyMeans.Italic;
                                break;
                            case Key.U:
                                means = CommonKeyMeans.UnderLine;
                                break;
                        }
                    }
                    else if (!ctrlDown && !shiftDown && altDown)
                    {
                        switch (e.Key)
                        {
                            case Key.F:
                                means = CommonKeyMeans.File;
                                break;
                        }
                    }
                    else if (ctrlDown && shiftDown && !altDown)
                    {
                        switch (e.Key)
                        {
                            case Key.S:
                                means = CommonKeyMeans.SaveAs;
                                break;
                        }
                    }
                }
                else if (!ListeningSingleKey)
                {
                    return;
                }

                KeyDown?.Invoke(sender, e, ctrlDown, shiftDown, altDown, means);
            }
            public enum CommonKeyMeans
            {
                Unknow,

                #region cut, copy, paste
                /// <summary>
                /// Ctrl + X
                /// </summary>
                Cut,
                /// <summary>
                /// Ctrl + C
                /// </summary>
                Copy,
                /// <summary>
                /// Ctrl + V
                /// </summary>
                Paste,
                /// <summary>
                /// Ctrl + N
                /// </summary>
                #endregion

                #region new, open, print
                New,
                /// <summary>
                /// Ctrl + O
                /// </summary>
                Open,
                /// <summary>
                /// Ctrl + P
                /// </summary>
                Print,
                #endregion

                #region bold, italic, underline
                /// <summary>
                /// Ctrl + B
                /// </summary>
                Bold,
                /// <summary>
                /// Ctrl + I
                /// </summary>
                Italic,
                /// <summary>
                /// Ctrl + U
                /// </summary>
                UnderLine,
                #endregion

                /// <summary>
                /// Alt + F
                /// </summary>
                File,

                /// <summary>
                /// Ctrl + Shift + S
                /// </summary>
                SaveAs,
            }

            private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
            {
                AnaliseKeyEvent(sender, e);
            }
        }


        /// <summary>
        /// 获取鼠标双击时间间隔(毫秒)；
        /// </summary>
        public static uint MouseDoubleClickTime
        {
            get
            {
                if (_MouseDoubleClickTimeNotInited)
                {
                    _MouseDoubleClickTime = GetDoubleClickTime();
                    _MouseDoubleClickTimeNotInited = false;
                }
                return _MouseDoubleClickTime;
            }
        }
        private static bool _MouseDoubleClickTimeNotInited = true;
        private static uint _MouseDoubleClickTime = 100;
        [DllImport("user32.dll")]
        private static extern uint GetDoubleClickTime();

    }
}
