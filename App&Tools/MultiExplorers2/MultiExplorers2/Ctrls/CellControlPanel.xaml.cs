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

namespace MadTomDev.App.Ctrls
{
    /// <summary>
    /// Interaction logic for CellControlPanel.xaml
    /// </summary>
    public partial class CellControlPanel : UserControl
    {
        public CellControlPanel()
        {
            InitializeComponent();
        }
        public Setting.Layout.IntPair Position;
        Core core = Core.GetInstance();
        private void btn_removeRow_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            core.LayoutRemoveRow(this);
        }

        private void btn_removeCol_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            core.LayoutRemoveCol(this);
        }

        private void btn_addExplorer_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            core.LayoutAddExplorer(this);
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (this.ActualHeight > this.ActualWidth)
            //{
            //    this.Height = this.Width;
            //}
            //else
            //{
            //    this.Width = this.Height;
            //}
        }
    }
}
