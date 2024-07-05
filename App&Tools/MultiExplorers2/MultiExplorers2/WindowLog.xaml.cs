using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for WindowLog.xaml
    /// </summary>
    public partial class WindowLog : Window
    {
        public WindowLog()
        {
            InitializeComponent();
        }

        Core core = Core.GetInstance();
        Logger logger = Logger.Instance;
        private void Window_Initialized(object sender, EventArgs e)
        {
            dataGrid.ItemsSource = logger.logItems;
            if (logger.logItems.Count > 0)
            {
                Logger.ItemViewModel lastItem = logger.logItems[logger.logItems.Count - 1];
                dataGrid.SelectedItem = dataGrid;
                dataGrid.ScrollIntoView(lastItem);
            }
        }

        private void btn_viewFiles_click(object sender, RoutedEventArgs e)
        {
            core.LogGeneral(core.GetLangTx("txLog_checkLogFiles"));
            Process.Start("Explorer.exe", logger.BaseDir);
        }

        private void btn_ok_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            core.LogGeneral(core.GetLangTx("txLog_closeWindowNClearCache"));
            logger.ClearLogItems();
        }

    }
}
