using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MouseClickAnalysis
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        System.Timers.Timer timer_checkVoidTime
            = new System.Timers.Timer(1000);
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer_checkVoidTime.Enabled = false;
            timer_checkVoidTime.Elapsed += timer_checkVoidTime_Elapsed;

            analysts.AnalysisStoped += analysts_AnalysisStoped;
            analysts.OneButtonRecStoped += analysts_OneButtonRecStoped;
        }


        void analysts_AnalysisStoped(object sender, EventArgs e)
        {
            timer_checkVoidTime.Stop();
            Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.SystemIdle,
                new DelegateSetCheckBoxCompleteAndMakeChart(SetCheckBoxCompleteAndMakeChart)
                );
        }
        private delegate void DelegateSetCheckBoxCompleteAndMakeChart();
        void SetCheckBoxCompleteAndMakeChart()
        {
            if (checkBox_Left.IsChecked != false)
                checkBox_Left.IsChecked = true;
            if (checkBox_Middle.IsChecked != false)
                checkBox_Middle.IsChecked = true;
            if (checkBox_Right.IsChecked != false)
                checkBox_Right.IsChecked = true;
            if (checkBox_X1.IsChecked != false)
                checkBox_X1.IsChecked = true;
            if (checkBox_X2.IsChecked != false)
                checkBox_X2.IsChecked = true;

            CallMakeCharts();
        }

        void timer_checkVoidTime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            analysts.CheckTimeVoid();
            //Dispatcher.Invoke(
            //    System.Windows.Threading.DispatcherPriority.SystemIdle,
            //    new DelegateSetAA(SetAA)
            //    );
        }
        //private delegate void DelegateSetAA();
        //void SetAA()
        //{
        //}

        Class_MCDataAnalysts analysts = new Class_MCDataAnalysts();

        int clickCount = 0;
        private void rectClickArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (timer_checkVoidTime.Enabled == false)
            {
                if (analysts.IsStoped == true) return;
                timer_checkVoidTime.Start();
            }
            object test = e.ButtonState;
            test = e.LeftButton;
            analysts.MCDataRec(e.ChangedButton);

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    if (checkBox_Left.IsChecked == false) checkBox_Left.IsChecked = null;
                    break;
                case MouseButton.Middle:
                    if (checkBox_Middle.IsChecked == false) checkBox_Middle.IsChecked = null;
                    break;
                case MouseButton.Right:
                    if (checkBox_Right.IsChecked == false) checkBox_Right.IsChecked = null;
                    break;
                case MouseButton.XButton1:
                    if (checkBox_X1.IsChecked == false) checkBox_X1.IsChecked = null;
                    break;
                case MouseButton.XButton2:
                    if (checkBox_X2.IsChecked == false) checkBox_X2.IsChecked = null;
                    break;
            }
            clickCount++;
            label_ClickCount.Content = clickCount.ToString("###,##0");
        }



        void analysts_OneButtonRecStoped(object sender, Class_MCDataAnalysts.OneButtonRecStopedArgs e)
        {
            //throw new NotImplementedException();
            switch (e.targetBtn)
            {
                case MouseButton.Left:
                    checkBox_Left.IsChecked = true;
                    break;
                case MouseButton.Middle:
                    checkBox_Middle.IsChecked = true;
                    break;
                case MouseButton.Right:
                    checkBox_Right.IsChecked = true;
                    break;
                case MouseButton.XButton1:
                    checkBox_X1.IsChecked = true;
                    break;
                case MouseButton.XButton2:
                    checkBox_X2.IsChecked = true;
                    break;
            }
        }

        //int tmp = 0;
        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            checkBox_Left.IsChecked = false;
            checkBox_Middle.IsChecked = false;
            checkBox_Right.IsChecked = false;
            checkBox_X1.IsChecked = false;
            checkBox_X2.IsChecked = false;

            analysts.ClearAndReset();
            CallClearCharts();

            clickCount = 0;
            label_ClickCount.Content = clickCount.ToString("###,##0");
        }

        private void buttonChartTip_Click(object sender, RoutedEventArgs e)
        {
            WindowChartTip tipWin = new WindowChartTip();
            tipWin.ShowDialog();
        }


        private void CallMakeCharts()
        {
            CallClearCharts();

            List<Class_MCDataAnalysts.CountDataItem> dataList;
            dataList = analysts.CopyAndGetCountDataList_ByMBtn(MouseButton.Left);
            if (dataList.Count > 0)
                UC_chartScrollView.AddChart(dataList);
            dataList = analysts.CopyAndGetCountDataList_ByMBtn(MouseButton.Middle);
            if (dataList.Count > 0)
                UC_chartScrollView.AddChart(dataList);
            dataList = analysts.CopyAndGetCountDataList_ByMBtn(MouseButton.Right);
            if (dataList.Count > 0)
                UC_chartScrollView.AddChart(dataList);
            dataList = analysts.CopyAndGetCountDataList_ByMBtn(MouseButton.XButton1);
            if (dataList.Count > 0)
                UC_chartScrollView.AddChart(dataList);
            dataList = analysts.CopyAndGetCountDataList_ByMBtn(MouseButton.XButton2);
            if (dataList.Count > 0)
                UC_chartScrollView.AddChart(dataList);
        }

        private void CallClearCharts()
        {
            UC_chartScrollView.ClearCharts();
        }
    }
}
