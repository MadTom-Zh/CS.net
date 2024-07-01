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

namespace tester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            dd= MadTomDev.Resource.DriveDetector.GetInstance();
            dd.DrivePluged += Dd_DrivePluged;
        }

        private void Dd_DrivePluged(string driveName, bool plugedInOrOut)
        {
            string inOrOutStr = plugedInOrOut ? "in" : "out";
            Dispatcher.Invoke(()=> { MessageBox.Show($"{driveName}, {inOrOutStr}", "info"); });
            
        }

        MadTomDev.Resource.DriveDetector dd;
    }
}
