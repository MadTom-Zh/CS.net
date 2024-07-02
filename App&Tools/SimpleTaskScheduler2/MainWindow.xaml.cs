using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            isIniting = true;
            dg_scheOneTime.ItemsSource = dg_scheOneTime_data;
            dg_scheEveryDay.ItemsSource = dg_scheEveryDay_data;
            dg_scheEveryWeek.ItemsSource = dg_scheEveryWeek_data;
            dg_scheEveryMonth.ItemsSource = dg_scheEveryMonth_data;
            dg_scheOtherInterval.ItemsSource = dg_scheOtherInterval_data;

            dg_taskList.ItemsSource = dg_taskList_data;

            core.ReInit(this);
            isIniting = false;
        }

        private bool isIniting = true;
        private Core core = Core.GetInstance();

        private int TabControl_SelectionChanged_preIndex = -1;
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl_SelectionChanged_preIndex != tabControl.SelectedIndex)
            {
                TabControl_SelectionChanged_preIndex = tabControl.SelectedIndex;
                if (TabControl_SelectionChanged_preIndex == 0)
                {
                    // tasks to ui
                    // no need
                }
                else
                {
                }

                if (TabControl_SelectionChanged_preIndex == 1)
                {
                    // schedules to ui
                    // no need
                    ScheRefreshAllNextTimes();
                }
                else
                {
                    // try save schedules
                    TryComfirmSaveScheChanged();
                }


                if (TabControl_SelectionChanged_preIndex == 2)
                {
                    // settings to ui
                    // no need
                }
                else
                {
                }

            }
        }



        #region task list


        public ObservableCollection<Data.VMDataTask> dg_taskList_data = new ObservableCollection<Data.VMDataTask>();

        private void dg_task_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (dg_taskList.SelectedItems.Count > 0)
            {
                List<Data.VMDataTask> tasksToDel = new List<Data.VMDataTask>();
                foreach (Data.VMDataTask t in dg_taskList.SelectedItems)
                    tasksToDel.Add(t);

                foreach (Data.VMDataTask t in tasksToDel)
                    dg_taskList_data.Remove(t);
            }
        }

        public void UpdateTasksCountDown()
        {
            if (dg_taskList_data.Count <= 0)
                return;

            Dispatcher.Invoke(() =>
            {
                foreach (Data.VMDataTask t in dg_taskList_data)
                {
                    t.UpdateCountDown();
                }
            });
        }


        #endregion


        #region scheduler

        public ObservableCollection<Data.VMDataSchedule> dg_scheOneTime_data = new ObservableCollection<Data.VMDataSchedule>();
        public ObservableCollection<Data.VMDataSchedule> dg_scheEveryDay_data = new ObservableCollection<Data.VMDataSchedule>();
        public ObservableCollection<Data.VMDataSchedule> dg_scheEveryWeek_data = new ObservableCollection<Data.VMDataSchedule>();
        public ObservableCollection<Data.VMDataSchedule> dg_scheEveryMonth_data = new ObservableCollection<Data.VMDataSchedule>();
        public ObservableCollection<Data.VMDataSchedule> dg_scheOtherInterval_data = new ObservableCollection<Data.VMDataSchedule>();

        public void ScheRefreshAllNextTimes()
        {
            Progress(dg_scheOneTime_data);
            Progress(dg_scheEveryDay_data);
            Progress(dg_scheEveryWeek_data);
            Progress(dg_scheEveryMonth_data);
            Progress(dg_scheOtherInterval_data);
            void Progress(ObservableCollection<Data.VMDataSchedule> list)
            {
                foreach (Data.VMDataSchedule i in list)
                {
                    i.UpdateNextTime();
                }
            }
        }


        private bool scheChanged = false;
        private void TryComfirmSaveScheChanged()
        {
            if (scheChanged)
            {
                if (MessageBox.Show(this, "Changes're not saved," + Environment.NewLine
                    + "would you like to save now?", "Not saved", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                    btn_scheApply_Click(null, null);
                else
                    btn_scheCancel_Click(null, null);
            }
        }


        private DataGrid curScheDataGrid = null;
        private void dg_sche_GotFocus(object sender, RoutedEventArgs e)
        {
            curScheDataGrid = (DataGrid)sender;
        }
        private void dg_sche_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btn_scheModify_Click(null, null);
        }


        private void dg_sche_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is DataGrid))
                return;

            DataGrid dg = (DataGrid)sender;
            ObservableCollection<Data.VMDataSchedule> vmData = (ObservableCollection<Data.VMDataSchedule>)dg.ItemsSource;

            int actionCode = 0; // 1-new, 2-modify, 3-delete, 4-moveUp, 5-moveDown
            switch (e.Key)
            {
                case Key.N: // new
                    actionCode = 1;
                    break;
                case Key.Apps: // modify
                    actionCode = 2;
                    break;
                case Key.Delete: // delete
                    actionCode = 3;
                    break;
                case Key.Up: // moveUp
                    actionCode = 4;
                    break;
                case Key.Down: // moveDown
                    actionCode = 5;
                    break;
            }

            if (actionCode > 0)
            {
                bool isHoldingShift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                bool isHoldingCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                switch (actionCode)
                {
                    case 1:
                        if (isHoldingCtrl)
                        {
                            btn_scheNew_Click(null, null);
                        }
                        break;
                    case 2:
                        btn_scheModify_Click(null, null);
                        break;
                    case 3:
                        btn_scheDelete_Click(null, null);
                        break;
                    case 4:
                        if (isHoldingShift)
                        {
                            DataGridScheMove(true, isHoldingCtrl);
                        }
                        break;
                    case 5:
                        if (isHoldingShift)
                        {
                            DataGridScheMove(false, isHoldingCtrl);
                        }
                        break;
                }
                e.Handled = true;
            }
        }


        #region buttons, new, modify, delete, moveUp, moveDown
        private void btn_scheNew_Click(object sender, RoutedEventArgs e)
        {
            scheChanged = true;
            WindowScheduleEditor win = new WindowScheduleEditor()
            { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner, };
            Data.VMDataSchedule data = new Data.VMDataSchedule() { NoTx = "*", Title = "[New Schedule]" };
            win.SetWindow(true, data);
            if (win.ShowDialog() == true)
            {
                win.GetUpdateData(ref data);
                switch (data.scheduleData.scheduleType)
                {
                    case UI.Class.ScheduleData.ScheduleTypes.Once:
                        dg_scheOneTime_data.Add(data);
                        break;
                    case UI.Class.ScheduleData.ScheduleTypes.EveryDay:
                        dg_scheEveryDay_data.Add(data);
                        break;
                    case UI.Class.ScheduleData.ScheduleTypes.EveryWeek:
                        dg_scheEveryWeek_data.Add(data);
                        break;
                    case UI.Class.ScheduleData.ScheduleTypes.EveryMonth:
                        dg_scheEveryMonth_data.Add(data);
                        break;
                    case UI.Class.ScheduleData.ScheduleTypes.OtherInterval:
                        dg_scheOtherInterval_data.Add(data);
                        break;
                }
            }
        }

        private void btn_scheModify_Click(object sender, RoutedEventArgs e)
        {
            if (curScheDataGrid == null
                || curScheDataGrid.SelectedItems.Count != 1)
                return;

            Data.VMDataSchedule data = (Data.VMDataSchedule)curScheDataGrid.SelectedItems[0];
            UI.Class.ScheduleData.ScheduleTypes oriScheType = data.scheduleData.scheduleType;

            WindowScheduleEditor win = new WindowScheduleEditor()
            { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner, };
            win.SetWindow(false, data);
            if (win.ShowDialog() == true)
            {
                scheChanged = true;
                win.GetUpdateData(ref data);
                if (!data.NoTx.EndsWith("*"))
                    data.NoTx += "*";
                if (data.scheduleData.scheduleType != oriScheType)
                {
                    ObservableCollection<Data.VMDataSchedule> source
                        = (ObservableCollection<Data.VMDataSchedule>)curScheDataGrid.ItemsSource;
                    source.Remove(data);
                    switch (data.scheduleData.scheduleType)
                    {
                        case UI.Class.ScheduleData.ScheduleTypes.Once:
                            dg_scheOneTime_data.Add(data);
                            break;
                        case UI.Class.ScheduleData.ScheduleTypes.EveryDay:
                            dg_scheEveryDay_data.Add(data);
                            break;
                        case UI.Class.ScheduleData.ScheduleTypes.EveryWeek:
                            dg_scheEveryWeek_data.Add(data);
                            break;
                        case UI.Class.ScheduleData.ScheduleTypes.EveryMonth:
                            dg_scheEveryMonth_data.Add(data);
                            break;
                        case UI.Class.ScheduleData.ScheduleTypes.OtherInterval:
                            dg_scheOtherInterval_data.Add(data);
                            break;
                    }
                }

            }

        }

        private void btn_scheDelete_Click(object sender, RoutedEventArgs e)
        {
            if (curScheDataGrid == null)
                return;

            scheChanged = true;
            List<Data.VMDataSchedule> itemsToDel = new List<Data.VMDataSchedule>();
            foreach (Data.VMDataSchedule i in curScheDataGrid.SelectedItems)
                itemsToDel.Add(i);
            ObservableCollection<Data.VMDataSchedule> source
                = (ObservableCollection<Data.VMDataSchedule>)curScheDataGrid.ItemsSource;
            foreach (Data.VMDataSchedule i in itemsToDel)
                source.Remove(i);
        }

        private void btn_scheMoveUp_Click(object sender, RoutedEventArgs e)
        {
            DataGridScheMove(true, false);
        }

        private void btn_scheMoveDown_Click(object sender, RoutedEventArgs e)
        {
            DataGridScheMove(false, false);
        }

        private void DataGridScheMove(bool upOrDown, bool toEnd)
        {
            if (curScheDataGrid == null
                || curScheDataGrid.SelectedItems.Count != 1)
                return;

            Data.VMDataSchedule item = (Data.VMDataSchedule)curScheDataGrid.SelectedItems[0];
            ObservableCollection<Data.VMDataSchedule> source
                = (ObservableCollection<Data.VMDataSchedule>)curScheDataGrid.ItemsSource;
            int curIdx = source.IndexOf(item);

            if (upOrDown)
            {
                if (curIdx > 0)
                {
                    scheChanged = true;
                    source.Remove(item);
                    if (toEnd)
                        source.Insert(0, item);
                    else
                        source.Insert(curIdx - 1, item);
                }
            }
            else
            {
                int maxV = source.Count - 1;
                if (curIdx >= 0 && curIdx < maxV)
                {
                    scheChanged = true;
                    source.Remove(item);
                    if (toEnd)
                        source.Add(item);
                    else
                        source.Insert(curIdx + 1, item);
                }
            }
        }


        private void btn_scheApply_Click(object sender, RoutedEventArgs e)
        {
            core.SaveAllSchedules();
            core.GenerateTasks(true);
            scheChanged = false;
            core.timer_selfCountdown = 0;
            tabControl.SelectedIndex = 0;
        }

        private void btn_scheCancel_Click(object sender, RoutedEventArgs e)
        {
            // reload
            core.ReLoadAllSchedules();
            ScheRefreshAllNextTimes();
            scheChanged = false;
        }

        #endregion

        #endregion


        #region settings

        public bool settingTimerInterval_userSetting = false;
        private void tb_settingTimerInterval_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isIniting)
                return;
            if (settingTimerInterval_userSetting)
                return;

            settingTimerInterval_userSetting = true;
            if (int.TryParse(tb_settingTimerInterval.Text, out int v))
            {
                if (v < 1) v = 1;
                else if (v > 600) v = 600;
            }
            else
            {
                v = 1;
            }
            sld_settingTimerInterval.Value = v;
            core.ChangeTimerInterval(v);
            settingTimerInterval_userSetting = false;

        }
        private void sld_settingTimerInterval_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isIniting)
                return;
            if (settingTimerInterval_userSetting)
                return;

            settingTimerInterval_userSetting = true;
            int newV = (int)sld_settingTimerInterval.Value;
            if (core.data.settingTimerIntervalSecs != newV)
            {
                tb_settingTimerInterval.Text = newV.ToString();
                core.ChangeTimerInterval(newV);
            }
            settingTimerInterval_userSetting = false;
        }


        public bool settingTaskListDays_userSetting = false;
        private void tb_settingTaskListDays_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isIniting)
                return;
            if (settingTaskListDays_userSetting)
                return;
            settingTaskListDays_userSetting = true;
            if (int.TryParse(tb_settingTaskListDays.Text, out int v))
            {
                if (v < 1) v = 1;
                else if (v > 400) v = 400;
            }
            else
            {
                v = 1;
            }
            sld_settingTaskListDays.Value = v;
            core.data.settingTaskScoutDays = v;
            settingTaskListDays_userSetting = false;
        }
        private void sld_settingTaskListDays_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isIniting)
                return;
            if (settingTaskListDays_userSetting)
                return;
            settingTaskListDays_userSetting = true;
            int newV = (int)sld_settingTaskListDays.Value;
            tb_settingTaskListDays.Text = newV.ToString();
            core.data.settingTaskScoutDays = newV;
            settingTaskListDays_userSetting = false;
        }



        #endregion


        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.Hide();
                this.Visibility = Visibility.Collapsed;
            }
        }
    }
}
