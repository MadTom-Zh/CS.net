using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

namespace MadTomDev.UI.ColorExpertControls
{
    /// <summary>
    /// Interaction logic for ColorPanelScreenPixPicker.xaml
    /// </summary>
    public partial class ColorPanelScreenPixPicker : UserControl
    {
        public Class.ColorExpertCore core;
        public ColorPanelScreenPixPicker()
        {
            InitializeComponent();
            //txLabel_tubeToTip">Tube... *Tip
            //ResourceDictionary rd = Application.Current.Resources;
            //if (!rd.Contains("txLabel_tubeToTip"))
            //    rd.Add("txLabel_tubeToTip", "Tube... *Tip");
        }
        public void InitFromStaticCore()
        {
            Init(Class.ColorExpertCore.GetInstance());
        }
        public void Init(Class.ColorExpertCore core)
        {
            this.core = core;

            core.WorkingColorChanged += ClrMgr_WorkingColorChanged;
            core.WorkingColorIndexChanged += ClrMgr_WorkingColorIndexChanged;

            RefreshPanel();

            IsEnabledChanged += (s1, e1) =>
            {
                if (IsEnabled)
                    RefreshPanel();
            };
        }

        private void ClrMgr_WorkingColorChanged(object sender, int workingColorIndex, object changer)
        {
            if (IsEnabled)
                RefreshPanel();
        }
        private void ClrMgr_WorkingColorIndexChanged(object sender, int workingColorIndex)
        {
            if (IsEnabled)
                RefreshPanel();
        }

        private void RefreshPanel()
        {
            if (isPicking)
                return;

            StringBuilder strBdr = new StringBuilder();
            strBdr.Append("X: , Y:");
            Color curClr = core.WorkingColor;
            RefreshPanel_appendColorTx(strBdr, curClr);
            tb.Text = strBdr.ToString();
            tb.Background = new SolidColorBrush(curClr);
            tb.Foreground = new SolidColorBrush(core.GetBetterForeColor(curClr));
        }
        private void RefreshPanel_appendColorTx(StringBuilder strBdr, Color clr)
        {
            strBdr.AppendLine();
            strBdr.Append(clr.ToString());
            strBdr.AppendLine();
            strBdr.Append(clr.R.ToString("0"));
            strBdr.Append(", ");
            strBdr.Append(clr.G.ToString("0"));
            strBdr.Append(", ");
            strBdr.Append(clr.B.ToString("0"));
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            StartPicking();
        }

        private bool isPicking = false;
        private Timer pickingTimer;
        private KeyboardHook keyboardHook = null;
        private MouseHook mouseHook = null;
        private void StartPicking()
        {
            if (pickingTimer != null)
                return;

            this.IsEnabled = false;
            btn.IsEnabled = false;
            isPicking = true;

            keyboardHook = new KeyboardHook();
            keyboardHook.KeyDown += KeyboardHook_KeyDown;
            mouseHook = new MouseHook();
            mouseHook.MouseMove += MouseHook_MouseMove;
            mouseHook.LeftButtonDown += MouseHook_LeftButtonDown;

            pickingTimer = new Timer(new TimerCallback(Timer_Tick));
            pickingTimer.Change(0, 50);
            this.IsEnabled = true;
        }

        private void MouseHook_LeftButtonDown(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            EndPicking();
        }
        private void KeyboardHook_KeyDown(KeyboardHook.VKeys key)
        {
            if (key == KeyboardHook.VKeys.KEY_T)
                EndPicking();
        }
        private void MouseHook_MouseMove(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            cursor = new Point(mouseStruct.pt.x, mouseStruct.pt.y);
        }

        public void EndPicking()
        {
            if (pickingTimer == null)
                return;

            keyboardHook.KeyDown -= KeyboardHook_KeyDown;
            keyboardHook.Dispose();
            mouseHook.MouseMove -= MouseHook_MouseMove;
            mouseHook.LeftButtonDown -= MouseHook_LeftButtonDown;
            mouseHook.Dispose();



            core.SetWorkingColor(selectedColor, this);
            isPicking = false;
            btn.IsEnabled = true;

            pickingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            pickingTimer.Dispose();
            pickingTimer = null;
        }

        private void HookManager_MouseDown(object sender, MouseEventArgs e)
        {
            EndPicking();
        }


        private Point cursor;
        private Color selectedColor;
        private void Timer_Tick(object sender)
        {
            Dispatcher.Invoke(() =>
            {
                BitmapSource snapShot = QuickGraphics.Screen.GetSnapAt(cursor, 5, 5);
                img.Source = snapShot;

                selectedColor = QuickGraphics.Image.GetColor(snapShot, 2, 2);

                tb.Background = new SolidColorBrush(selectedColor);
                tb.Foreground = new SolidColorBrush(core.GetBetterForeColor(selectedColor));

                StringBuilder strBdr = new StringBuilder();
                strBdr.Append("X:");
                strBdr.Append(cursor.X.ToString("0"));
                strBdr.Append(" , Y:");
                strBdr.Append(cursor.Y.ToString("0"));
                RefreshPanel_appendColorTx(strBdr, selectedColor);

                tb.Text = strBdr.ToString();
            });
        }


    }
}
