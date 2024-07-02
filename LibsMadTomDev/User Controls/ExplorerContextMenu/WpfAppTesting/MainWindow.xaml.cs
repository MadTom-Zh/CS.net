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

namespace WpfAppTesting
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

        private void btn1_click(object sender, RoutedEventArgs e)
        {
            //MadTomDev.UI.ExplorerContextMenu menu = new MadTomDev.UI.ExplorerContextMenu();

            //MadTomDev.UI.WindowMenuSettings testWnd = new MadTomDev.UI.WindowMenuSettings();
            //testWnd.Init(menu);
            //testWnd.ShowDialog();

            img.Source = MadTomDev.Common.IconHelper.Shell32Icons.GetInstance().GetCustomIcon(0);
        }

        private void btn2_click(object sender, RoutedEventArgs e)
        {
            //string testParent = "C:\\[files]\\abc";
            //HashSet<string> testFiles = new HashSet<string>();
            //testFiles.Add("C:\\[Files]\\abc\\a111");
            //testFiles.Add("C:\\[Files]\\abc\\a222");
            //string oCmd = "someCmd [parent] from [files] -t [date(HHmmss.fff)] saveTo \"[pc] [user] [date(yyyyMMdd HHmmss)].data\"";
            //string rCmd = MadTomDev.UI.EMItemModelExtensions.GetRealCommand(oCmd,testParent,testFiles);


            tb_date.Text = DateTime.Now.ToString(tb_format.Text);

            testUI.Value = DateTime.Now;
        }
    }
}
