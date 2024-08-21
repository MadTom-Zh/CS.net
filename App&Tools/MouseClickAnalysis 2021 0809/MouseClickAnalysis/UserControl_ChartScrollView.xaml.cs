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
    /// UserControl_ChartScrollView.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_ChartScrollView : UserControl
    {
        public UserControl_ChartScrollView()
        {
            InitializeComponent();
        }

        List<UserControl_Chart> ucChartList
            = new List<UserControl_Chart>();
        public void ClearCharts()
        {
            gridChartContainer.Children.Clear();
            ucChartList.Clear();
            gridChartContainer.Height = 100;
        }

        public void AddChart(List<Class_MCDataAnalysts.CountDataItem> dataList)
        {
            UserControl_Chart ucChart = new UserControl_Chart();
            gridChartContainer.Children.Add(ucChart);

            ucChart.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            ucChart.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            ucChart.Height = UserControl_Chart.UI_HEIGHT;

            if (ucChartList.Count == 0)
                ucChart.Margin = new Thickness(0, 0, 0, 0);
            else
            {
                UserControl_Chart lastChartUC = ucChartList[ucChartList.Count - 1];
                ucChart.Margin = new Thickness(0,
                    lastChartUC.Margin.Top + UserControl_Chart.UI_HEIGHT - 1,
                    0, 0
                    );
            }

            ucChartList.Add(ucChart);
            ucChart.DataList = dataList;

            borderMainFrame.Height = ucChartList.Count * (UserControl_Chart.UI_HEIGHT - 1) + 5;
            gridChartContainer.Height = ucChartList.Count * (UserControl_Chart.UI_HEIGHT - 1) + 3;
            //gridChartContainer.Margin = new Thickness(0,0,0,0);
        }
    }
}
