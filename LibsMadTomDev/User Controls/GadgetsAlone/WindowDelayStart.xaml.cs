using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using System.Windows.Shapes;

namespace MadTomDev.UI
{
    /// <summary>
    /// Interaction logic for WindowDelayStart.xaml
    /// </summary>
    public partial class WindowDelayStart : Window
    {
        public WindowDelayStart()
        {
            InitializeComponent();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            isClosing = true;
            this.Close();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;
        }

        private Timer timer;
        private bool isClosing = false;
        public void SetNStart(string title, TimeSpan tsFix, TimeSpan tsRand, bool showCmdWnd, string[] cmds)
        {
            tb_title.Text = title;
            if (cmds == null || cmds.Length == 0)
            {
                MessageBox.Show($"No cmd in ({title})");
                this.Close();
                return;
            }
            this.cmds = cmds;
            this.showCmdWnd = showCmdWnd;
            StringBuilder strBdr = new StringBuilder();
            foreach (string c in cmds)
            {
                strBdr.AppendLine(c);
            }
            tb_cmds.Text = strBdr.ToString();

            timeUp = DateTime.Now;
            Random rand = new Random((int)timeUp.Ticks);
            tsTotal = new TimeSpan(tsFix.Ticks + rand.NextInt64(tsRand.Ticks));
            pb_countDown.Maximum = tsTotal.Ticks;
            timeStart = timeUp + tsTotal;
            UpdateCountDownProgress(timeUp);

            timer = new Timer(new TimerCallback(timerTick));
            timer.Change(321, 321);
        }
        private DateTime timeUp, timeStart = DateTime.MaxValue;
        private TimeSpan tsTotal;
        private bool showCmdWnd = true;
        private string[] cmds;
        private void timerTick(object s)
        {
            if (isClosing)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                return;
            }
            DateTime now = DateTime.Now;
            UpdateCountDownProgress(now);
            if (timeStart <= now)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);

                // start
                using (Process p = new Process())
                {
                    p.StartInfo = new ProcessStartInfo()
                    {
                        FileName = "cmd.exe",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        //RedirectStandardOutput = true,
                        //RedirectStandardError = true,
                        CreateNoWindow = !showCmdWnd,
                    };
                    p.Start();

                    foreach (string l in cmds)
                    {
                        p.StandardInput.WriteLine(l);
                    }
                    p.StandardInput.Flush();
                    p.StandardInput.Close();


                    p.WaitForExit();
                    p.Close();
                }

                Dispatcher.Invoke(() =>
                { Close(); });
            }
        }


        private void UpdateCountDownProgress(DateTime now)
        {
            TimeSpan tsLeft = timeStart - now;
            Dispatcher.Invoke(() =>
            {
                pb_countDown.Value = tsTotal.Ticks - tsLeft.Ticks;
                if (tsLeft <= TimeSpan.Zero)
                    tb_countDown.Text = "Start";
                else
                    tb_countDown.Text = tsLeft.ToString(@"ddd\.\ hh\:mm\:ss\.\ fff");
            });
        }
    }
}
