using MadTomDev.App.Classes;
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

using Console = MadTomDev.App.Classes.Console;

namespace MadTomDev.App.Ctrls
{
    /// <summary>
    /// Interaction logic for TabItemHeader.xaml
    /// </summary>
    public partial class TabItemHeader : UserControl
    {
        public TabItemHeader()
        {
            InitializeComponent();
        }

        private Console _Console;
        public Console Console
        {
            set
            {
                _Console = value;
            }
        }
        private Settings setting = Core.Instance.setting;




        public void SetTextUp(string tx)
        {
            Dispatcher.BeginInvoke(() =>
            {
                tb1.Text = tx;
            });
        }
        public void SetTextDown(string tx)
        {
            Dispatcher.BeginInvoke(() =>
            {
                tb2.Text = tx;
            });
        }
        public string GetHeaderText()
        {
            return tb1.Text + " " + tb2.Text;
        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Console.tabPanel.TrySaveCurProfileFromUI();
            Core.Instance.CloseConsole(_Console);
        }
    }
}
