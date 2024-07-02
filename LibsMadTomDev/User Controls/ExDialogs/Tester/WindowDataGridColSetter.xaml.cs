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
using System.Windows.Shapes;

namespace Tester
{
    /// <summary>
    /// Interaction logic for WindowDataGridColSetter.xaml
    /// </summary>
    public partial class WindowDataGridColSetter : Window
    {
        public WindowDataGridColSetter()
        {
            InitializeComponent();
            btn_reload_Click(null, null);
        }

        private void btn_reload_Click(object sender, RoutedEventArgs e)
        {
            MadTomDev.UI.VM._DataGridColumnSetter dgcSetter = new MadTomDev.UI.VM._DataGridColumnSetter(dg);
            List<MadTomDev.UI.VM._DataGridColumnSetter.ColumnInfo> colInfoList
                = new List<MadTomDev.UI.VM._DataGridColumnSetter.ColumnInfo>();


            DataTemplate headerTemplete = new DataTemplate();



            FrameworkElementFactory container = new FrameworkElementFactory(typeof(StackPanel));
            container.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            FrameworkElementFactory a1 = new FrameworkElementFactory(typeof(TextBlock));
            FrameworkElementFactory a2 = new FrameworkElementFactory(typeof(TextBlock));
            container.AppendChild(a1);
            //a1.SetValue(TextBlock.TextProperty, "(o)");
            a1.SetBinding(TextBlock.TextProperty, new Binding("value"));
            container.AppendChild(a2);
            //Binding binding = new Binding();
            //binding.Path = new PropertyPath("MarketIndicator");
            //fef.SetBinding(CheckBox.ContentProperty, binding);
            //fef.SetValue(CheckBox.ForegroundProperty, Brushes.White);
            a2.SetBinding(TextBlock.TextProperty, new Binding("text"));
            headerTemplete.VisualTree = container;






            colInfoList.Add(new MadTomDev.UI.VM._DataGridColumnSetter.ColumnInfo()
            {
                columnType = MadTomDev.UI.VM._DataGridColumnSetter.ColumnTypes.Text,
                header = new dataGridHeaderVM() { text = "tx", value = "2", },
                headerTemplate = headerTemplete,
                //width = new DataGridLength(100),
                bingding = new Binding("text1"),
                //isReadOnly = false,
            });
            colInfoList.Add(new MadTomDev.UI.VM._DataGridColumnSetter.ColumnInfo()
            {
                columnType = MadTomDev.UI.VM._DataGridColumnSetter.ColumnTypes.CheckBox,
                header = "col2",
                //headerTemplate = null,
                //width = new DataGridLength(100),
                bingding = new Binding("ck1"),
                //isReadOnly = false,
            });
            colInfoList.Add(new MadTomDev.UI.VM._DataGridColumnSetter.ColumnInfo()
            {
                columnType = MadTomDev.UI.VM._DataGridColumnSetter.ColumnTypes.Hyperlink,
                header = "col3",
                //headerTemplate = null,
                //width = new DataGridLength(100),
                bingding = new Binding("lk1"),
                //isReadOnly = false,
            });

            dgcSetter.ReSetColumns(colInfoList);

            dg.ItemsSource = GetItemsSource();
        }

        ObservableCollection<dataGridItemVM> dataGridItemsSource;
        private ObservableCollection<dataGridItemVM> GetItemsSource()
        {
            dataGridItemsSource = new ObservableCollection<dataGridItemVM>();
            dataGridItemsSource.Add(new dataGridItemVM() { text1 = "t1", ck1 = false,lk1="abc1", });
            dataGridItemsSource.Add(new dataGridItemVM() { text1 = "t2", ck1 = true, lk1 = "abc2", });
            dataGridItemsSource.Add(new dataGridItemVM() { text1 = "t3", ck1 = false, lk1 = "abc3", });

            return dataGridItemsSource;
        }


        class dataGridHeaderVM
        {
            public string text { set; get; }
            public string value { set; get; }
        }

        class dataGridItemVM
        {
            public string text1 { set; get; }
            public bool ck1 { set; get; }
            public string lk1 { set; get; }
        }

        private void btn_test1_Click(object sender, RoutedEventArgs e)
        {
            ;
        }

    }
}
