using MadTomDev.UI;
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

namespace Tester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }


        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        MadTomDev.UI.ScreenSelectorSimple sss;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            sss = new ScreenSelectorSimple();
            Int32Rect rect = new Int32Rect(100, 100, 500, 500);
            //sss.SetSelectedArea(rect);
            //if (sss.ShowDialog(rect) == true)
            if (sss.ShowDialog() == true)
            {
                img.Source = sss.SelectedImage;
            }
        }
    }
}
