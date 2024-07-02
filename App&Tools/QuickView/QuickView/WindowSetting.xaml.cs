using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for WindowSetting.xaml
    /// </summary>
    public partial class WindowSetting : Window
    {
        public WindowSetting()
        {
            InitializeComponent();
        }

        private Settings setting = Settings.GetInstance();
        private void Window_Initialized(object sender, EventArgs e)
        {
            cb_loopSubDirs.IsChecked = setting.CmnLoopSubs;
            cb_errMsgBox.IsChecked = setting.CmnErrorMsgBox;

            bdr_bgColor.Background = new SolidColorBrush(setting.BG);

            keyLeft = setting.ViewPrevKey;
            keyRight = setting.ViewNextKey;
            keyDown = setting.Sort0Key;
            keyUp = setting.Sort1Key;
            tb_dir0.Text = setting.Sort0DirName;
            tb_dir1.Text = setting.Sort1DirName;

            thumCount = setting.ViewThumbnailCount;
            historyCount = setting.ViewHistoryCount;
            stillWaitMin = setting.StillWaitMin;

            tb_blockFiles.Text = setting.BlockFiles;
            tb_limitMinFileSize.Text = setting.LimitMinFileSize.ToString();
        }


        System.Windows.Forms.ColorDialog clrDlg = null;
        private void btn_bgColor_Click(object sender, RoutedEventArgs e)
        {
            if (clrDlg == null)
            {
                Color oClr = setting.BG;
                System.Drawing.Color dClr = System.Drawing.Color.FromArgb(
                    oClr.A, oClr.R, oClr.G, oClr.B);

                clrDlg = new System.Windows.Forms.ColorDialog()
                { Color = dClr, };
                if (clrDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    dClr = clrDlg.Color;
                    Color nClr = Color.FromArgb(dClr.A, dClr.R, dClr.G, dClr.B);
                    bdr_bgColor.Background = new SolidColorBrush(nClr);
                }
            }
        }

        #region ui-value linkage
        private Key _keyLeft;
        private Key keyLeft
        {
            get => _keyLeft;
            set
            {
                _keyLeft = value;
                tb_keyLeft.Text = value.ToString();
            }
        }
        private Key _keyRight;
        private Key keyRight
        {
            get => _keyRight;
            set
            {
                _keyRight = value;
                tb_keyRight.Text = value.ToString();
            }
        }
        private Key _keyDown;
        private Key keyDown
        {
            get => _keyDown;
            set
            {
                _keyDown = value;
                tb_keyDown.Text = value.ToString();
            }
        }
        private Key _keyUp;
        private Key keyUp
        {
            get => _keyUp;
            set
            {
                _keyUp = value;
                tb_keyUp.Text = value.ToString();
            }
        }
        private int thumCount
        {
            get => (int)sld_thumCount.Value;
            set
            {
                sld_thumCount.Value = value;
                tb_thumCount.Text = value.ToString();
            }
        }
        private int historyCount
        {
            get => (int)sld_historyCount.Value;
            set
            {
                sld_historyCount.Value = value;
                tb_historyCount.Text = value.ToString();
            }
        }
        private int stillWaitMin
        {
            get => (int)sld_stillWaitMin.Value;
            set
            {
                sld_stillWaitMin.Value = value;
                tb_stillWaitMin.Text = value.ToString();
            }
        }
        #endregion

        #region set key
        private bool isSetinKeyLeft = false;
        private void btn_keyLeft_Click(object sender, RoutedEventArgs e)
        {
            isSetinKeyLeft = true;
            tb_keyLeft.Text = "[press a key]";
            tb_keyLeft.SelectAll();
            tb_keyLeft.Focus();
        }
        private void tb_keyLeft_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (isSetinKeyLeft)
            {
                isSetinKeyLeft = false;
                keyLeft = e.Key;
            }
        }


        private bool isSetinKeyRight = false;
        private void btn_keyRight_Click(object sender, RoutedEventArgs e)
        {
            isSetinKeyRight = true;
            tb_keyRight.Text = "[press a key]";
            tb_keyRight.SelectAll();
            tb_keyRight.Focus();
        }
        private void tb_keyRight_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (isSetinKeyRight)
            {
                isSetinKeyRight = false;
                keyRight = e.Key;
            }
        }


        private bool isSetinKeyDown = false;
        private void btn_keyDown_Click(object sender, RoutedEventArgs e)
        {
            isSetinKeyDown = true;
            tb_keyDown.Text = "[press a key]";
            tb_keyDown.SelectAll();
            tb_keyDown.Focus();
        }
        private void tb_keyDown_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (isSetinKeyDown)
            {
                isSetinKeyDown = false;
                keyDown = e.Key;
            }
        }


        private bool isSetinKeyUp = false;
        private void btn_keyUp_Click(object sender, RoutedEventArgs e)
        {
            isSetinKeyUp = true;
            tb_keyUp.Text = "[press a key]";
            tb_keyUp.SelectAll();
            tb_keyUp.Focus();
        }
        private void tb_keyUp_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (isSetinKeyUp)
            {
                isSetinKeyUp = false;
                keyUp = e.Key;
            }
        }
        #endregion


        private void sld_thumCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (tb_thumCount == null)
                return;
            tb_thumCount.Text = ((int)sld_thumCount.Value).ToString();
        }

        private void sld_historyCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (tb_historyCount == null)
                return;
            tb_historyCount.Text = ((int)sld_historyCount.Value).ToString();
        }
        private void sld_stillWaitMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (tb_stillWaitMin == null)
                return;
            tb_stillWaitMin.Text = ((int)sld_stillWaitMin.Value).ToString();
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            // check first;
            if (!float.TryParse(tb_limitMinFileSize.Text, out float minFileSize))
            {
                MessageBox.Show("Odd limit min file size.");
                return;
            }

            // set value;
            setting.CmnLoopSubs = cb_loopSubDirs.IsChecked == true;
            setting.CmnErrorMsgBox = cb_errMsgBox.IsChecked == true;

            setting.BG = ((SolidColorBrush)bdr_bgColor.Background).Color;

            setting.ViewPrevKey = keyLeft;
            setting.ViewNextKey = keyRight;
            setting.Sort0Key = keyDown;
            setting.Sort1Key = keyUp;
            setting.Sort0DirName = tb_dir0.Text;
            setting.Sort1DirName = tb_dir1.Text;

            setting.ViewThumbnailCount = thumCount;
            setting.ViewHistoryCount = historyCount;
            setting.StillWaitMin = stillWaitMin;

            setting.BlockFiles = tb_blockFiles.Text;
            setting.LimitMinFileSize = minFileSize;

            setting.Save();
            this.Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
