using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

namespace MadTomDev.UI
{
    /// <summary>
    /// Interaction logic for WindowHelper.xaml
    /// </summary>
    public partial class WindowHelper : Window
    {
        public WindowHelper()
        {
            InitializeComponent();

            isIniting = true;
            InitMsg();

            isIniting = false;
            RegenCmdMsg();
            RegenCmdDelayStart();
        }


        #region init


        private bool isIniting = true;
        private void InitMsg()
        {
            panel_msgImages.Children.Clear();
            Type mImgType = typeof(MessageBoxImage);
            RadioButton rb;
            HashSet<string> imgFlags = new HashSet<string>();
            string flag;
            Thickness rbMargin = new Thickness(8, 0, 0, 0);
            foreach (MessageBoxImage i in mImgType.GetEnumValues())
            {
                flag = i.ToString();
                if (imgFlags.Contains(flag))
                    continue;

                imgFlags.Add(flag);
                rb = new RadioButton() { Content = flag, Tag = i, Margin = rbMargin, };
                rb.Checked += Rb_Checked;
                panel_msgImages.Children.Add(rb);
            }
            ((RadioButton)panel_msgImages.Children[0]).IsChecked = true;

            panel_msgButtons.Children.Clear();
            Type mBtnType = typeof(MessageBoxButton);
            foreach (MessageBoxButton b in mBtnType.GetEnumValues())
            {
                rb = new RadioButton() { Content = b.ToString(), Tag = b, Margin = rbMargin, };
                rb.Checked += Rb_Checked;
                panel_msgButtons.Children.Add(rb);
            }
            ((RadioButton)panel_msgButtons.Children[0]).IsChecked = true;
        }


        #endregion


        #region message box

        private void Rb_Checked(object sender, RoutedEventArgs e)
        { RegenCmdMsg(); }
        private void tb_msgTitle_TextChanged(object sender, TextChangedEventArgs e)
        { RegenCmdMsg(); }
        private void tb_msgContent_TextChanged(object sender, TextChangedEventArgs e)
        { RegenCmdMsg(); }
        private void cb_msgTopMost_CheckChanged(object sender, RoutedEventArgs e)
        { RegenCmdMsg(); }
        private void RegenCmdMsg()
        {
            if (isIniting)
                return;

            MessageBoxImage img = MessageBoxImage.None;
            foreach (RadioButton rb in panel_msgImages.Children)
            {
                if (rb.IsChecked == true)
                {
                    img = (MessageBoxImage)rb.Tag;
                    break;
                }
            }
            MessageBoxButton btn = MessageBoxButton.OK;
            foreach (RadioButton rb in panel_msgButtons.Children)
            {
                if (rb.IsChecked == true)
                {
                    btn = (MessageBoxButton)rb.Tag;
                    break;
                }
            }
            tb_msgCmd.Text = Core.GetCmdMsg(
                tb_msgTitle.Text, tb_msgContent.Text.Replace(Environment.NewLine, "\\r"),
                img, btn, cb_msgTopMost.IsChecked == true);
        }

        private void btn_msgStart_Click(object sender, RoutedEventArgs e)
        {
            TestCmd(tb_msgCmd.Text);
        }
        private void TestCmd(string cmd)
        {
            using (Process p = new Process())
            {
                p.StartInfo = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                };
                p.Start();

                p.StandardInput.WriteLine(cmd);

                p.StandardInput.Flush();
                p.StandardInput.Close();

                p.WaitForExit();
                p.Close();
            }
        }


        #endregion


        #region delay start


        private void tsud_delayStartFix_ValueChanged(object sender, EventArgs e)
        { RegenCmdDelayStart(); }
        private void tsud_delayStartRand_ValueChanged(object sender, EventArgs e)
        { RegenCmdDelayStart(); }
        private void cb_delayStartShowCmdWindow_CheckChangeded(object sender, RoutedEventArgs e)
        { RegenCmdDelayStart(); }
        private void tb_delayStartCmdsToStart_TextChanged(object sender, TextChangedEventArgs e)
        { RegenCmdDelayStart(); }
        private void RegenCmdDelayStart()
        {
            if (isIniting)
                return;

            tb_delayStartCmd.Text = Core.GetCmdDelayStart(
                tb_delayStartTitle.Text,
                tsud_delayStartFix.Value,
                tsud_delayStartRand.Value,
                cb_delayStartShowCmdWindow.IsChecked == true,
                tb_delayStartCmdsToStart.Text);
        }

        private void btn_delayStartStart_Click(object sender, RoutedEventArgs e)
        {
            TestCmd(tb_delayStartCmd.Text);
        }

        #endregion
    }
}
