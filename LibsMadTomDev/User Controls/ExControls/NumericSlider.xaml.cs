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

using System.ComponentModel;

namespace MadTomDev.UI
{
    /// <summary>
    /// Interaction logic for NumericSlider.xaml
    /// </summary>
    public partial class NumericSlider : UserControl
    {
        /// <summary>
        /// 数字滑块，默认范围0-10
        /// +可显示为百分比；
        /// +可设置布局方向；
        /// +键盘上下调整，按住快速调整；
        /// +键盘直接输入数字，回车确认，esc取消；
        /// ++编辑时变黄，有误时变红；
        /// </summary>
        public NumericSlider()
        {
            InitializeComponent();
            ReSetText();
        }

        public enum Directions
        { LeftRight, RightLeft, UpDown, DownUp, }
        private Directions _Direction = Directions.LeftRight;

        [Description("控件的排列方向"), Category("Appearance")]
        public Directions Direction
        {
            get => _Direction;
            set
            {
                if (_Direction == value)
                    return;

                _Direction = value;
                grid.Children.Remove(tb);
                grid.Children.Remove(sld);
                grid.RowDefinitions.Clear();
                grid.ColumnDefinitions.Clear();
                switch (_Direction)
                {
                    case Directions.LeftRight:
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                        grid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Auto);
                        grid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                        Grid.SetRow(tb, 0);
                        Grid.SetColumn(tb, 0);
                        grid.Children.Add(tb);
                        sld.Orientation = Orientation.Horizontal;
                        sld.Margin = new Thickness(1, 0, 0, 0);
                        sld.HorizontalAlignment = HorizontalAlignment.Stretch;
                        sld.VerticalAlignment = VerticalAlignment.Center;
                        Grid.SetRow(sld, 0);
                        Grid.SetColumn(sld, 1);
                        grid.Children.Add(sld);
                        break;
                    case Directions.RightLeft:
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                        grid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                        grid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Auto);
                        sld.Orientation = Orientation.Horizontal;
                        sld.Margin = new Thickness(0, 0, 1, 0);
                        sld.HorizontalAlignment = HorizontalAlignment.Stretch;
                        sld.VerticalAlignment = VerticalAlignment.Center;
                        Grid.SetRow(sld, 0);
                        Grid.SetColumn(sld, 0);
                        grid.Children.Add(sld);
                        Grid.SetRow(tb, 0);
                        Grid.SetColumn(tb, 1);
                        grid.Children.Add(tb);
                        break;
                    case Directions.DownUp:
                        grid.RowDefinitions.Add(new RowDefinition());
                        grid.RowDefinitions.Add(new RowDefinition());
                        grid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
                        grid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Auto);
                        sld.Orientation = Orientation.Vertical;
                        sld.Margin = new Thickness(0, 0, 0, 1);
                        sld.HorizontalAlignment = HorizontalAlignment.Center;
                        sld.VerticalAlignment = VerticalAlignment.Stretch;
                        Grid.SetRow(sld, 0);
                        Grid.SetColumn(sld, 0);
                        grid.Children.Add(sld);
                        Grid.SetRow(tb, 1);
                        Grid.SetColumn(tb, 0);
                        grid.Children.Add(tb);
                        break;
                    case Directions.UpDown:
                        grid.RowDefinitions.Add(new RowDefinition());
                        grid.RowDefinitions.Add(new RowDefinition());
                        grid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Auto);
                        grid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                        sld.Margin = new Thickness(0, 1, 0, 0);
                        sld.HorizontalAlignment = HorizontalAlignment.Center;
                        sld.VerticalAlignment = VerticalAlignment.Stretch;
                        Grid.SetRow(tb, 0);
                        Grid.SetColumn(tb, 0);
                        grid.Children.Add(tb);
                        sld.Orientation = Orientation.Vertical;
                        Grid.SetRow(sld, 1);
                        Grid.SetColumn(sld, 0);
                        grid.Children.Add(sld);
                        break;
                }
            }
        }


        public delegate void ValueChangedDelegate(NumericSlider sender);
        public event ValueChangedDelegate ValueChanged;

        [Description("控件的数值"), Category("Common")]
        public double Value
        {
            get => sld.Value;
            set
            {
                if (value <= Minimum)
                {
                    if (Value == Minimum)
                        return;
                }
                else if (Maximum <= value)
                {
                    if (Value == Maximum)
                        return;
                }

                sld.Value = value;

                ReSetText();
                ValueChanged?.Invoke(this);
            }
        }


        private void ReSetText()
        {
            StringBuilder format = new StringBuilder();
            if (_AsPercentage)
            {
                format.Append("P");
                if (_DecimalPlaces > 0)
                    format.Append(_DecimalPlaces);
                else
                    format.Append("0");
            }
            else
            {
                if (_ThousandsSeparator)
                    format.Append("###,###,###,###,###,###,###,##0");
                else
                    format.Append("############################0");
                if (_DecimalPlaces > 0)
                {
                    format.Append(".");
                    for (int i = 0; i < _DecimalPlaces; i++)
                    {
                        format.Append("0");
                    }
                }
            }
            tb.Text = sld.Value.ToString(format.ToString());
            tb.SelectionStart = tb.Text.Length;
        }

        [Description("用滑块调整数值时的小增量"), Category("Common")]
        public double SmallChange
        {
            get => sld.SmallChange;
            set => sld.SmallChange = value;
        }


        [Description("用滑槽调整数值时的大增量"), Category("Common")]
        public double LargeChange
        {
            get => sld.LargeChange;
            set => sld.LargeChange = value;
        }

        [Description("限定最大值"), Category("Common")]
        public double Maximum
        {
            get => sld.Maximum;
            set => sld.Maximum = value;
        }

        [Description("限定最小值"), Category("Common")]
        public double Minimum
        {
            get => sld.Minimum;
            set => sld.Minimum = value;
        }

        private bool _AsPercentage = false;
        [Description("显示为百分比"), Category("Common")]
        public bool AsPercentage
        {
            get => _AsPercentage;
            set
            {
                if (_AsPercentage == value)
                    return;
                _AsPercentage = value;
                ReSetText();
            }
        }
        private int _DecimalPlaces = 0;
        [Description("小数点后位数"), Category("Common")]
        public int DecimalPlaces
        {
            get => _DecimalPlaces;
            set
            {
                if (_DecimalPlaces == value)
                    return;
                if (value < 0)
                    throw new ArgumentOutOfRangeException("小数位数不可小于零");
                _DecimalPlaces = value;
            }
        }
        private bool _ThousandsSeparator = false;
        [Description("设置千分位的逗号"), Category("Common")]
        public bool ThousandsSeparator
        {
            get => _ThousandsSeparator;
            set
            {
                if (_ThousandsSeparator == value)
                    return;
                _ThousandsSeparator = value;
                ReSetText();
            }
        }

        private void sld_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ReSetText();
            ValueChanged?.Invoke(this);
        }

        private void tb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    Value += LargeChange;
                    tb.Background = tbBGBrushPre;
                    e.Handled = true;
                    break;
                case Key.Down:
                    Value -= LargeChange;
                    tb.Background = tbBGBrushPre;
                    e.Handled = true;
                    break;
                case Key.Enter:
                    if (!tbValueErr)
                    {
                        Value = tbValue;
                        tb_LostFocus(null, null);
                        ValueChanged?.Invoke(this);
                    }
                    e.Handled = true;
                    break;
                case Key.Escape:
                    tb_LostFocus(null, null);
                    break;
            }
        }



        private Brush tbBGBrushPre = SystemColors.WindowTextBrush;
        private Brush tbBGBrushEdit, tbBGBrushErr;
        private double tbValue;
        private bool tbValueErr;
        private void tb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tbBGBrushPre == SystemColors.WindowTextBrush)
            {
                tbBGBrushPre = tb.Background;
                tbBGBrushEdit = new SolidColorBrush(Colors.Yellow);
                tbBGBrushErr = new SolidColorBrush(Colors.Orange);
            }
            tb.Background = tbBGBrushEdit;
        }
        private void tb_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (tb.IsFocused)
            {
                tb_GotFocus(null, null);
                tb_TextChanged(null, null);
            }
        }

        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            string tx;
            if (_AsPercentage && tb.Text.EndsWith("%"))
            {
                tx = tb.Text.Substring(0, tb.Text.Length - 1);
            }
            else tx = tb.Text;

            if (double.TryParse(tx, out tbValue))
            {
                if (_AsPercentage)
                    tbValue /= 100;
                if (tbValue < Minimum || tbValue > Maximum)
                {
                    tbValueErr = true;
                    tb.Background = tbBGBrushErr;
                }
                else
                {
                    tbValueErr = false;
                    tb.Background = tbBGBrushEdit;
                }
            }
            else
            {
                tbValueErr = true;
                tb.Background = tbBGBrushErr;
            }
        }


        private void tb_LostFocus(object sender, RoutedEventArgs e)
        {
            ReSetText();
            tb.Background = tbBGBrushPre;
        }

        public void FocusTB()
        {
            tb.Focus();
        }
    }
}
