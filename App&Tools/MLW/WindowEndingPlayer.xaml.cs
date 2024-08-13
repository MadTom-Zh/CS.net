using MLW_Succubus_Storys.Classes;
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

namespace MLW_Succubus_Storys
{
    /// <summary>
    /// Interaction logic for WindowEndingPlayer.xaml
    /// </summary>
    public partial class WindowEndingPlayer : Window
    {
        public WindowEndingPlayer()
        {
            InitializeComponent();
        }

        public SuccuNode.EndingNode endingData;

        // 0 屏幕变黑
        // 1 图片出现，bgm开播
        // 2 第一文字出现
        // 【点击完成本区快，如果已经完成，则出现后续文字】
        // 【结束：最后文字出现后，再次点击】
        // 3 屏幕复原，bgm淡出，恢复之前的bgm

        private int state;
        private bool hadActivated = false;
        private int preBGMIndex;
        private async void Window_Activated(object sender, EventArgs e)
        {
            if (hadActivated)
                return;

            if (endingData == null)
            {
                MessageBox.Show("No ending data.");
                this.Close();
                return;
            }
            img.Source = endingData.image;
            await Task.Delay(50);
            hadActivated = true;

            BlackOut_playBGM();
        }
        private void SetTBSize()
        {
            tb.Width = this.ActualWidth * 4 / 5;
            tb.Height = this.ActualHeight * 1 / 3;
        }

        private async void BlackOut_playBGM()
        {
            // black out, play bg music
            await Task.Run(() =>
            {
                state = 0;
                preBGMIndex = Core.BGMIndex;
                if (endingData.isGoodOrBad)
                    Core.BGMPlay(4);
                else
                    Core.BGMPlay(5);
                // use 0.2s
                DateTime just = DateTime.Now;
                TimeSpan timePass;
                double opa;
                while (GetOpacityThis() < 1)
                {
                    Task.Delay(10).Wait();
                    timePass = DateTime.Now - just;
                    opa = timePass.TotalMilliseconds / 300; // 200
                    SetOpacityThis(opa);
                }
            });
            ShowBG();
        }

        private double GetOpacityThis()
        {
            double result = 0;
            Dispatcher.Invoke(() => { result = this.Opacity; });
            return result;
        }
        private void SetOpacityThis(double v)
        {
            Dispatcher.Invoke(() => { this.Opacity = v; });
        }
        private double GetOpacityImg()
        {
            double result = 0;
            Dispatcher.Invoke(() => { result = img.Opacity; });
            return result;
        }
        private void SetOpacityImg(double v)
        {
            Dispatcher.Invoke(() => { img.Opacity = v; });
        }
        private async void ShowBG()
        {
            // show bg image
           await  Task.Run(() =>
            {
                state = 1;
                // use 0.2s
                DateTime just = DateTime.Now;
                TimeSpan timePass;
                double opa;
                while (GetOpacityImg() < 1)
                {
                    Task.Delay(10).Wait();
                    timePass = DateTime.Now - just;
                    opa = timePass.TotalMilliseconds / 300; // 200
                    SetOpacityImg(opa);
                }

            });
            // show bg image
            state = 2;
            StartShowMsgAsync();
        }

        private bool showMsg_working = false;
        private int showMsg_msgIndex = 0;
        private Task StartShowMsgAsync()
        {
            return Task.Run(() =>
            {
                showMsg_working = true;
                string msg = endingData.msgs[showMsg_msgIndex++];
                string msgDsp = "";
                Dispatcher.Invoke(() =>
                {
                    SetTBSize();
                    tb.Text = ""; 
                });
                for (int i = 0, iv = msg.Length; i < iv; ++i)
                {
                    if (showMsg_endFlag)
                        break;
                    Task.Delay(15).Wait();
                    msgDsp += msg[i];
                    Dispatcher.Invoke(() => { tb.Text = msgDsp; });
                }
                if (showMsg_endFlag)
                    Dispatcher.Invoke(() => { tb.Text = msg; });
                showMsg_endFlag = false;
                showMsg_working = false;
            });
        }
        private bool showMsg_endFlag = false;
        public void EndShowMsg()
        {
            showMsg_endFlag = true;
        }
        private void Rectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (state != 2)
                return;

            if (showMsg_working)
            {
                EndShowMsg();
            }
            else
            {
                if (showMsg_msgIndex == endingData.msgs.Count)
                {
                    // to quit
                    tb.Text = "";
                    FakeOut();
                }
                else
                {
                    StartShowMsgAsync();
                }
            }
        }
        private void FakeOut()
        {
            Task.Run(() =>
            {
                // bg out, restore screen, play prev bg music
                state = 3;
                Core.BGMPlay(preBGMIndex);

                // use 0.2s
                DateTime just = DateTime.Now;
                TimeSpan timePass;
                double opa;
                while (GetOpacityImg() > 0)
                {
                    Task.Delay(10).Wait();
                    timePass = DateTime.Now - just;
                    opa = 1 - timePass.TotalMilliseconds / 300;
                    SetOpacityImg(opa);
                }
                just = DateTime.Now;
                while (GetOpacityThis() > 0)
                {
                    Task.Delay(10).Wait();
                    timePass = DateTime.Now - just;
                    opa = 1 - timePass.TotalMilliseconds / 200;
                    SetOpacityThis(opa);
                }
                Dispatcher.Invoke(() => { this.Close(); });
            });
        }
    }
}
