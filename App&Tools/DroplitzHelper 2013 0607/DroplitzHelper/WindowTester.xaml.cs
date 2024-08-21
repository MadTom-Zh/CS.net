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

namespace DroplitzHelper
{
    /// <summary>
    /// WindowTester.xaml 的交互逻辑
    /// </summary>
    public partial class WindowTester : Window
    {
        public WindowTester()
        {
            InitializeComponent();
            switchUI = switchData.UI;
            switchUI.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            switchUI.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            switchUI.Margin = new Thickness(5, 5, 0, 0);
            gridMain.Children.Add(switchUI);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Test();
        }

        Classes.Switch6Ways switchData = new Classes.Switch6Ways();
        UCs.UC_Switch switchUI;
        int testStep = -1;
        private void Test()
        {
            switch (testStep)
            {
                case -1:
                    switchData.MyType = Classes.Switch6Ways.Type.U;
                    break;
                case 0:
                    switchData.TurnWaysLeft(0);
                    break;
                case 1:
                    switchData.TurnWaysLeft(1);
                    break;
                case 2:
                    switchData.TurnWaysLeft(2);
                    break;
                case 3:
                    switchData.TurnWaysLeft(3);
                    break;
                case 4:
                    switchData.TurnWaysLeft(4);
                    break;
                case 5:
                    switchData.TurnWaysLeft(5);
                    break;
                case 6:
                    switchData.TurnWaysLeft(6);
                    break;
                default:
                    if (testStep > 6 && testStep < 20)
                    {
                        switchData.TurnWaysRight(2);
                    }
                    else
                    {
                        switchData.TurnWaysLeft(2);
                    }
                    break;
            }

            testStep++;
        }
    }
}
