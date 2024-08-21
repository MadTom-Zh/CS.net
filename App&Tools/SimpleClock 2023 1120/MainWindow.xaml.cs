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
using System.Windows.Navigation;
using System.Windows.Shapes;

using MadTomDev.Common;

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            mTimer.Tick += MTimer_Tick;
            mTimer.LoopType = MillisecondTimer.LoopTypes.Wait4ms;
            mTimer.Interval = MillisecondTimer.Intervals.ms500;
        }

        private MillisecondTimer mTimer = MillisecondTimer.New();
        private int day, hour, minute, second;
        private void MTimer_Tick(MillisecondTimer sender, DateTime tickTime, bool is500ms, bool is1000ms)
        {
            if (isClosing)
                return;

            if (is1000ms)
            {
                hour = tickTime.Hour;
                minute = tickTime.Minute;
                second = tickTime.Second;
                SetTime();

                if (day != tickTime.Day)
                {
                    day = tickTime.Day;
                    SetDate(tickTime);
                }
            }
            else if (is500ms)
            {
                SetTime(false);
            }
        }
        private SolidColorBrush btnBorderBrush_Ctrl = SystemColors.ControlDarkDarkBrush;
        private SolidColorBrush btnBorderBrush_Red = new SolidColorBrush(Colors.Red);


        private void SetDate(DateTime date)
        {
            Dispatcher.Invoke(() =>
            {
                tb_date.Text = date.ToString("yyyy-MM-dd");
                btn_w1.BorderBrush = btnBorderBrush_Ctrl;
                btn_w2.BorderBrush = btnBorderBrush_Ctrl;
                btn_w3.BorderBrush = btnBorderBrush_Ctrl;
                btn_w4.BorderBrush = btnBorderBrush_Ctrl;
                btn_w5.BorderBrush = btnBorderBrush_Ctrl;
                btn_w6.BorderBrush = btnBorderBrush_Ctrl;
                btn_w7.BorderBrush = btnBorderBrush_Ctrl;
                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        btn_w1.BorderBrush = btnBorderBrush_Red; break;
                    case DayOfWeek.Tuesday:
                        btn_w2.BorderBrush = btnBorderBrush_Red; break;
                    case DayOfWeek.Wednesday:
                        btn_w3.BorderBrush = btnBorderBrush_Red; break;
                    case DayOfWeek.Thursday:
                        btn_w4.BorderBrush = btnBorderBrush_Red; break;
                    case DayOfWeek.Friday:
                        btn_w5.BorderBrush = btnBorderBrush_Red; break;
                    case DayOfWeek.Saturday:
                        btn_w6.BorderBrush = btnBorderBrush_Red; break;
                    case DayOfWeek.Sunday:
                        btn_w7.BorderBrush = btnBorderBrush_Red; break;
                }
            });
        }
        private StringBuilder timeStrBdr = new StringBuilder();
        private void SetTime(bool withColon = true)
        {
            Dispatcher.Invoke(() =>
            {
                timeStrBdr.Clear();
                if (withColon)
                {
                    timeStrBdr.Append(hour.ToString("00"));
                    timeStrBdr.Append(":");
                    timeStrBdr.Append(minute.ToString("00"));
                    timeStrBdr.Append(":");
                    timeStrBdr.Append(second.ToString("00"));

                    pb_cycle.Value = (hour * 3600 + minute * 60 + second) / 86400d;
                }
                else
                {
                    timeStrBdr.Append(hour.ToString("00"));
                    timeStrBdr.Append(" ");
                    timeStrBdr.Append(minute.ToString("00"));
                    timeStrBdr.Append(" ");
                    timeStrBdr.Append(second.ToString("00"));
                }

                tb_time.Text = timeStrBdr.ToString();
            });
        }

        private bool isClosing = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;
            mTimer.Dispose();
        }
    }
}
