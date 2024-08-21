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

namespace DroplitzHelper
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            mainPanel.SizeChanged += mainPanel_SizeChanged;
            mainPanel.SPMgrComputingStart += mainPanel_SPMgrComputingStart;
            mainPanel.SPMgrComputingEnd += mainPanel_SPMgrComputingEnd;
        }
        void mainPanel_SPMgrComputingStart(object sender, EventArgs e)
        {
            border.BorderBrush = new SolidColorBrush(Colors.Red);
        }
        void mainPanel_SPMgrComputingEnd(object sender, EventArgs e)
        {
            border.BorderBrush = new SolidColorBrush(Colors.Green);
        }

        void mainPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            textBlock_taps.Text = mainPanel.TapCount.ToString();
            textBlock_rowsMain.Text = mainPanel.RowCount.ToString();
        }

        private void button_tapReduc_Click(object sender, RoutedEventArgs e)
        {
            mainPanel.IncreaseTap(-1);
        }
        private void button_tapIncre_Click(object sender, RoutedEventArgs e)
        {
            mainPanel.IncreaseTap(1);
        }

        private void button_rowReduc_Click(object sender, RoutedEventArgs e)
        {
            mainPanel.IncreaseRow(-1);
        }
        private void button_rowIncre_Click(object sender, RoutedEventArgs e)
        {
            mainPanel.IncreaseRow(1);
        }

        private void Window_KeyDown_1(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1:
                    button_about_Click(this, new RoutedEventArgs());
                    break;
                case Key.F5:
                    // 计算长链接
                    mainPanel.SPMgrReCalculateWays(true);
                    break;
                case Key.F6:
                    // 计算简链接
                    mainPanel.SPMgrReCalculateWays(false);
                    break;
                default:
                    mainPanel.TrySendKey(e.Key, Mouse.GetPosition(mainPanel.gridContainer));
                    break;
            }
        }
        private void button_about_Click(object sender, RoutedEventArgs e)
        {
            WindowAbout aboutWin = new WindowAbout();
            aboutWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            aboutWin.ShowDialog();
        }
        private void Window_MouseRightButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            mainPanel.TrySendMiceDown(false, Mouse.GetPosition(mainPanel.gridContainer));
        }

        private void Window_PreviewMouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            mainPanel.TrySendMiceDown(true, Mouse.GetPosition(mainPanel.gridContainer));
        }
    }
}
