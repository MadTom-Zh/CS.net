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
    /// Interaction logic for WindowScheduleEditor.xaml
    /// </summary>
    public partial class WindowScheduleEditor : Window
    {
        public WindowScheduleEditor()
        {
            InitializeComponent();
        }

        Data.VMDataSchedule data;
        public void SetWindow(bool newOrEdit, Data.VMDataSchedule data)
        {
            this.data = data.Clone();
            if (newOrEdit)
                Title = "Create Scheldule";
            else
                Title = "Modify Scheldule";

            tb_title.Text = data.Title;
            RefreshCycleDescription();
            tb_commands.Text = data.Cmd;
        }
        private void RefreshCycleDescription()
        {
            tb_cycleDescription.Text
                = "Next start at " + data.UpdateNextTime().ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine
                + data.scheduleData.GetCycleDescription();
        }
        public void GetUpdateData(ref Data.VMDataSchedule data)
        {
            data.Title = this.data.Title;
            data.scheduleData.title = this.data.scheduleData.title;
            data.scheduleData.startTime = this.data.scheduleData.startTime;
            data.scheduleData.startDaysInWeek = this.data.scheduleData.startDaysInWeek;
            data.scheduleData.startDaysInMonth = this.data.scheduleData.startDaysInMonth;
            data.scheduleData.scheduleType = this.data.scheduleData.scheduleType;
            data.scheduleData.otherInterval = this.data.scheduleData.otherInterval;
            data.UpdateNextTime();
            data.Cmd = tb_commands.Text;
        }

        private void btn_cycle_Click(object sender, RoutedEventArgs e)
        {
            data.scheduleData.title = tb_title.Text;
            UI.WindowScheduleMaker win = new UI.WindowScheduleMaker()
            { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner, };
            win.SetScheduleData(data.scheduleData);
            if (win.ShowDialog() == true)
            {
                win.GetScheduleData(ref data.scheduleData);
                tb_title.Text = data.Title;
                RefreshCycleDescription();
            }
        }
        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

    }
}
