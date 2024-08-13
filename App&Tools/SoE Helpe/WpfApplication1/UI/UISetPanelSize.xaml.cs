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

namespace WpfApplication1.UI
{
    /// <summary>
    /// UISetPanelSize.xaml 的交互逻辑
    /// </summary>
    public partial class UISetPanelSize : UserControl
    {
        public UISetPanelSize()
        {
            InitializeComponent();
        }

        public event EventHandler PRowsChanged;
        public event EventHandler PColsChanged;

        private int _PRows = 4;
        public int PRows
        {
            set
            {
                if (_PRows != value)
                {
                    _PRows = value;
                    TextBox_Rows.Text = "" + _PRows;
                    if (PRowsChanged != null)
                    {
                        PRowsChanged(this, new EventArgs());
                    }
                }
            }
            get
            {
                return _PRows;
            }
        }
        private int _PCols = 4;
        public int PCols
        {
            set
            {
                if (_PCols != value)
                {
                    _PCols = value;
                    TextBox_Cols.Text = "" + _PCols;
                    if (PRowsChanged != null)
                    {
                        PColsChanged(this, new EventArgs());
                    }
                }
            }
            get
            {
                return _PCols;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //col dec
            if (_PCols > 2)
            {
                PCols = _PCols - 1;
                TextBox_Cols.Text = "" + _PCols;
            }
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //col inc
            if (_PCols < 16)
            {
                PCols = _PCols + 1;
                TextBox_Cols.Text = "" + _PCols;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //row dec
            if (_PRows > 2)
            {
                PRows = _PRows - 1;
                TextBox_Rows.Text = "" + _PRows;
            }
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            //row inc
            if (_PRows < 16)
            {
                PRows = _PRows + 1;
                TextBox_Rows.Text = "" + _PRows;
            }
        }

        //private void TextBox_Rows_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    try
        //    {
        //        int value = int.Parse(TextBox_Rows.Text);
        //        if (value > 2)
        //        {
        //            PRows = value;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        //private void TextBox_Cols_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    try
        //    {
        //        int value = int.Parse(TextBox_Cols.Text);
        //        if (value > 2)
        //        {
        //            PCols = value;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}
    }
}
