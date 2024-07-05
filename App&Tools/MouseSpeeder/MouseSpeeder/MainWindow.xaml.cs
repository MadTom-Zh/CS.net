using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using SpeechLib;
using System.IO;
using System.ComponentModel;
using System.Windows.Interop;
using System.Diagnostics;

namespace MouseSpeeder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private bool isInit = true;
        private void Window_Initialized(object sender, EventArgs e)
        {

            int curMSpeed = GetMouseSpeed();
            sld_mSpeed.Value
                = sld_mSpeed.SelectionStart
                = sld_mSpeed.SelectionEnd
                = curMSpeed;

            LoadCfg();
            if (keySpeedDown == Key.None)
                _keySpeedDown = Key.F9;
            if (keySpeedUp == Key.None)
                _keySpeedUp = Key.F10;

            _proc = HookCallback;
            _hookID = SetHook(_proc);

            isInit = false;
            keySDTx = null;
            keySUTx = null;
        }

        #region global keyboard hook
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion


        #region key change speed
        private bool isSettingKeys = false;
        private void tb_down_GotFocus(object sender, RoutedEventArgs e)
        {
            isSettingKeys = true;
        }

        private void tb_down_LostFocus(object sender, RoutedEventArgs e)
        {
            isSettingKeys = false;
        }

        private void tb_up_GotFocus(object sender, RoutedEventArgs e)
        {
            isSettingKeys = true;
        }
        private void tb_up_LostFocus(object sender, RoutedEventArgs e)
        {
            isSettingKeys = false;
        }
        private void mainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            tb_focus.Focus();
        }

        private IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int keyPressed = (int)KeyInterop.KeyFromVirtualKey(Marshal.ReadInt32(lParam));
                Dispatcher.Invoke(() =>
                {
                    if (isSettingKeys)
                        return;

                    int test = (int)keySpeedDown;

                    if (keyPressed == (int)keySpeedDown)
                    {
                        sldChange(-1);
                    }
                    else if (keyPressed == (int)keySpeedUp)
                    {
                        sldChange(1);
                    }
                });
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        private void sldChange(int offset)
        {
            int newValue = (int)sld_mSpeed.Value + offset;
            if (newValue < sld_mSpeed.Minimum)
                sld_mSpeed.Value = sld_mSpeed.Minimum;
            else if (newValue > sld_mSpeed.Maximum)
                sld_mSpeed.Value = sld_mSpeed.Maximum;
            else
                sld_mSpeed.Value = newValue;
        }

        #endregion


        private void sld_mSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInit)
                return;

            int newMSpeed = (int)sld_mSpeed.Value;
            SetMouseSpeed(newMSpeed);

            if (cb_tts.IsChecked == true)
            {
                SpeekAsync(newMSpeed.ToString());
            }
        }


        #region tts
        private SpVoice voice = null;
        private string curSpeekingTx = null;
        private bool isSpeeking = false;
        private void SpeekAsync(string newTx)
        {
            if (newTx == curSpeekingTx)
                return;

            curSpeekingTx = newTx;
            if (isSpeeking)
                return;

            Task.Run(() =>
            {

                isSpeeking = true;

                if (voice == null)
                    voice = new SpVoice();

                string t2s;
                do
                {
                    t2s = curSpeekingTx;
                    voice.Speak(t2s);
                }
                while (curSpeekingTx != t2s);

                isSpeeking = false;
            });
        }
        #endregion


        #region get/set mouse speed
        [DllImport("user32.dll")]
        public static extern int SystemParametersInfo(int uAction, int uParam, IntPtr lpvParam, int fuWinIni);


        public const int SPI_GETMOUSESPEED = 112;
        public const int SPI_SETMOUSESPEED = 113;

        public static int GetMouseSpeed()
        {
            int intSpeed = 0;
            IntPtr ptr;
            ptr = Marshal.AllocCoTaskMem(4);
            SystemParametersInfo(SPI_GETMOUSESPEED, 0, ptr, 0);
            intSpeed = Marshal.ReadInt32(ptr);
            Marshal.FreeCoTaskMem(ptr);

            return intSpeed;
        }

        public static void SetMouseSpeed(int intSpeed)
        {
            IntPtr ptr = new IntPtr(intSpeed);

            int b = SystemParametersInfo(SPI_SETMOUSESPEED, 0, ptr, 0);

            if (b == 0)
            {
                Console.WriteLine("False");
            }
            else if (b == 1)
            {
                Console.WriteLine("true");
            }

        }
        #endregion


        #region hot keys ,mvvm like

        public event PropertyChangedEventHandler PropertyChanged;

        private Key _keySpeedDown = Key.None;
        private Key keySpeedDown
        {
            set
            {
                if (isInit)
                    return;
                if (_keySpeedDown == value)
                    return;
                if (value == _keySpeedUp)
                {
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }

                _keySpeedDown = value;
                keySDTx = null;
            }
            get => _keySpeedDown;
        }
        public string keySDTx
        {
            get => _keySpeedDown.ToString();
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("keySDTx"));
            }
        }


        private Key _keySpeedUp = Key.None;
        private Key keySpeedUp
        {
            set
            {
                if (isInit)
                    return;
                if (_keySpeedUp == value)
                    return;
                if (value == _keySpeedDown)
                {
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }

                _keySpeedUp = value;
                keySUTx = null;
            }
            get => _keySpeedUp;
        }
        public string keySUTx
        {
            get => _keySpeedUp.ToString();
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("keySUTx"));
            }
        }


        private void tb_down_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            keySpeedDown = e.Key;
        }

        private void tb_up_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            keySpeedUp = e.Key;
        }



        #endregion


        #region load/save cfgs


        private string cfgFile = null;


        private void LoadCfg()
        {
            if (cfgFile == null)
            {
                cfgFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                cfgFile = System.IO.Path.Combine(cfgFile, "MadTomDev");
                if (!Directory.Exists(cfgFile))
                    Directory.CreateDirectory(cfgFile);
                cfgFile = System.IO.Path.Combine(cfgFile, "MSpd.txt");
            }
            if (File.Exists(cfgFile))
            {
                object test;
                bool testBool;
                foreach (string l in File.ReadAllLines(cfgFile))
                {
                    if (l.StartsWith("Down:"))
                    {
                        if (Enum.TryParse(typeof(Key), l.Substring(5), out test))
                        {
                            keySpeedDown = (Key)test;
                        }
                    }
                    else if (l.StartsWith("Up:"))
                    {
                        if (Enum.TryParse(typeof(Key), l.Substring(3), out test))
                        {
                            keySpeedUp = (Key)test;
                        }
                    }
                    else if (l.StartsWith("isTTS:"))
                    {
                        if (bool.TryParse(l.Substring(6), out testBool))
                        {
                            cb_tts.IsChecked = testBool;
                        }
                    }
                }
            }
        }
        private void SaveCfg()
        {
            StringBuilder strBdr = new StringBuilder();
            strBdr.Append("Down:");
            strBdr.AppendLine(keySpeedDown.ToString());
            strBdr.Append("Up:");
            strBdr.AppendLine(keySpeedUp.ToString());
            strBdr.Append("isTTS:");
            strBdr.AppendLine(cb_tts.IsChecked.ToString());
            File.WriteAllText(cfgFile, strBdr.ToString());
        }
        #endregion

        private void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            SaveCfg();
            Task.Run(() => { UnhookWindowsHookEx(_hookID); });
        }

    }
}
