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

namespace MadTomDev.UI
{
    /// <summary>
    /// Interaction logic for WindowScheduleMaker.xaml
    /// </summary>
    public partial class WindowScheduleMaker : Window
    {
        public WindowScheduleMaker()
        {
            InitializeComponent();
        }

        private bool isSetting = false;
        public void SetScheduleData(Class.ScheduleData data)
        {
            isSetting = true;
            tb_title.Text = data.title;
            dtud_startDate.Value = data.startTime;
            dtud_startTime.Value = data.startTime;
            CollapseAllPanels();
            rb_once.IsChecked = false;
            rb_everyDay.IsChecked = false;
            rb_everyWeek.IsChecked = false;
            rb_everyMonth.IsChecked = false;
            rb_otherInterval.IsChecked = false;
            switch (data.scheduleType)
            {
                case Class.ScheduleData.ScheduleTypes.Once:
                case Class.ScheduleData.ScheduleTypes.EveryDay:
                    if (data.scheduleType == Class.ScheduleData.ScheduleTypes.Once)
                    {
                        panel_startDate.Visibility = Visibility.Visible;
                        rb_once.IsChecked = true;
                    }
                    else
                    {
                        rb_everyDay.IsChecked = true;
                    }
                    isSettingStartDateTime = false;
                    break;
                case Class.ScheduleData.ScheduleTypes.EveryWeek:
                    rb_everyWeek.IsChecked = true;
                    UnCheckAllWeekDays();
                    CheckWeekDaysFromData(data);
                    panel_everyWeek.Visibility = Visibility.Visible;
                    break;
                case Class.ScheduleData.ScheduleTypes.EveryMonth:
                    rb_everyMonth.IsChecked = true;
                    UnCheckAllMonthDays();
                    CheckMonthDaysFromData(data);
                    panel_everyMonth.Visibility = Visibility.Visible;
                    break;
                case Class.ScheduleData.ScheduleTypes.OtherInterval:
                    rb_otherInterval.IsChecked = true;
                    nud_otherIntervals_days.Value = (int)data.otherInterval.TotalDays;
                    dtud_otherIntervals_time.Value = new DateTime(2023, 1, 1) + data.otherInterval;
                    panel_otherInterval.Visibility = Visibility.Visible;
                    panel_startDate.Visibility = Visibility.Visible;
                    break;
            }
            isSetting = false;
        }
        private void UnCheckAllWeekDays()
        {
            for (int i = 0; i < 7; ++i)
            {
                ((CheckBox)panel_everyWeek.Children[i]).IsChecked = false;
            }
        }
        private void CheckWeekDaysFromData(Class.ScheduleData data)
        {
            Class.ScheduleData.SortAndRemoveDuplis(ref data.startDaysInWeek);
            foreach (int day in data.startDaysInWeek)
            {
                if (day < 0 || day > 6)
                    continue;
                if (day == 0)
                {
                    ((CheckBox)panel_everyWeek.Children[6]).IsChecked = true;
                }
                else
                {
                    ((CheckBox)panel_everyWeek.Children[day - 1]).IsChecked = true;
                }
            }
        }
        private void SetWeekDaysToData(ref Class.ScheduleData data)
        {
            data.startDaysInWeek.Clear();
            CheckBox cb = (CheckBox)panel_everyWeek.Children[6];
            if (cb.IsChecked == true)
            {
                data.startDaysInWeek.Add(0);
            }
            for (int i = 0; i < 6; ++i)
            {
                if (((CheckBox)panel_everyWeek.Children[i]).IsChecked == true)
                    data.startDaysInWeek.Add(i + 1);
            }
        }
        private void UnCheckAllMonthDays()
        {
            for (int i = 0; i < 31; ++i)
            {
                ((CheckBox)panel_everyMonth.Children[i]).IsChecked = false;
            }
        }
        private void CheckMonthDaysFromData(Class.ScheduleData data)
        {
            Class.ScheduleData.SortAndRemoveDuplis(ref data.startDaysInMonth);
            foreach (int day in data.startDaysInMonth)
            {
                if (day < 0 || day > 31)
                    continue;
                ((CheckBox)panel_everyMonth.Children[day - 1]).IsChecked = true;
            }
        }
        private void SetMonthDaysToData(ref Class.ScheduleData data)
        {
            data.startDaysInMonth.Clear();
            for (int i = 0; i < 31; ++i)
            {
                if (((CheckBox)panel_everyMonth.Children[i]).IsChecked == true)
                    data.startDaysInMonth.Add(i + 1);
            }
        }

        public Class.ScheduleData GetScheduleData()
        {
            Class.ScheduleData result = new Class.ScheduleData();
            GetScheduleData(ref result);
            return result;
        }
        public void GetScheduleData(ref Class.ScheduleData dataToUpdate)
        {
            dataToUpdate.title = tb_title.Text;
            DateTime date = dtud_startDate.Value;
            DateTime time = dtud_startTime.Value;
            dataToUpdate.startTime = new DateTime(
                date.Year, date.Month, date.Day,
                time.Hour,time.Minute,time.Second);
            if (rb_once.IsChecked == true)
            {
                dataToUpdate.scheduleType = Class.ScheduleData.ScheduleTypes.Once;

            }
            else if (rb_everyDay.IsChecked == true)
            {
                dataToUpdate.scheduleType = Class.ScheduleData.ScheduleTypes.EveryDay;
            }
            else if (rb_everyWeek.IsChecked == true)
            {
                dataToUpdate.scheduleType = Class.ScheduleData.ScheduleTypes.EveryWeek;
                SetWeekDaysToData(ref dataToUpdate);
            }
            else if (rb_everyMonth.IsChecked == true)
            {
                dataToUpdate.scheduleType = Class.ScheduleData.ScheduleTypes.EveryMonth;
                SetMonthDaysToData(ref dataToUpdate);
            }
            else if (rb_otherInterval.IsChecked == true)
            {
                dataToUpdate.scheduleType = Class.ScheduleData.ScheduleTypes.OtherInterval;
                time = dtud_otherIntervals_time.Value;
                dataToUpdate.otherInterval = new TimeSpan(
                    (int)nud_otherIntervals_days.Value,
                    time.Hour, time.Minute, time.Second);
            }
        }

        private void rb_Checked(object sender, RoutedEventArgs e)
        {
            if (isSetting)
                return;
            if (!(sender is RadioButton))
                return;

            CollapseAllPanels();
            RadioButton rb = (RadioButton)sender;
            if (rb == rb_once)
            {
                panel_startDate.Visibility = Visibility.Visible;
            }
            else if (rb == rb_everyWeek)
            {
                panel_everyWeek.Visibility = Visibility.Visible;
            }
            else if (rb == rb_everyMonth)
            {
                panel_everyMonth.Visibility = Visibility.Visible;
            }
            else if (rb == rb_otherInterval)
            {
                panel_otherInterval.Visibility = Visibility.Visible;
                panel_startDate.Visibility = Visibility.Visible;
            }
        }
        private void CollapseAllPanels()
        {
            panel_everyWeek.Visibility = Visibility.Collapsed;
            panel_everyMonth.Visibility = Visibility.Collapsed;
            panel_otherInterval.Visibility = Visibility.Collapsed;
            panel_startDate.Visibility = Visibility.Collapsed;
        }

        private bool isSettingStartDateTime = false;
        private void dtud_startDate_ValueChanged(object sender, EventArgs e)
        {
            if (isSettingStartDateTime)
                return;
            isSettingStartDateTime = true;
            dtud_startTime.Value = dtud_startDate.Value;
            isSettingStartDateTime = false;
        }
        private void dtud_startTime_ValueChanged(object sender, EventArgs e)
        {
            if (isSettingStartDateTime)
                return;
            isSettingStartDateTime = true;
            dtud_startDate.Value = dtud_startTime.Value;
            isSettingStartDateTime = false;
        }



        public Class.ScheduleData resultData;
        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            resultData = GetScheduleData();
            DialogResult = true;
            Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

    }
}
