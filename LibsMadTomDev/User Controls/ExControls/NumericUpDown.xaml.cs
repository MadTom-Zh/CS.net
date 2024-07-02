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
using System.Threading;

namespace MadTomDev.UI
{
    /// <summary>
    /// Interaction logic for NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        /// <summary>
        /// 数字输入框，默认范围0-100
        /// +可显示为百分比，可设置固定增量；
        /// +鼠标点击上下箭头调整，按住快速调整；
        /// +键盘上下调整，按住快速调整；
        /// +键盘直接输入数字，回车确认，esc取消；
        /// ++编辑时变黄，有误时变红；
        /// </summary>
        public NumericUpDown()
        {
            InitializeComponent();
            DataContext = this;
            ReSetText();
        }

        public delegate void ValueChangedDelegate(NumericUpDown sender);
        public event ValueChangedDelegate ValueChanged;


        private decimal _Value = 0;
        [Description("控件的数值"), Category("Common")]

        public decimal Value
        {
            get => _Value;
            set
            {
                if (value < Minimum)
                    if (_Value != Minimum)
                        _Value = Minimum;
                    else return;
                else if (value > Maximum)
                    if (_Value != Maximum)
                        _Value = Maximum;
                    else return;
                else
                    _Value = value;

                if (_Increment > 0)
                {
                    decimal rem = _Value % _Increment;
                    if (rem > 0)
                    {
                        if (rem >= _Increment / 2)
                        {
                            _Value += _Increment - rem;
                        }
                        else
                        {
                            _Value -= rem;
                        }
                    }
                }

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
            }
            else
            {
                if (_ThousandsSeparator)
                    format.Append("##,###,###,###,###,###,###,###,###,##0");
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
            tb.Text = Value.ToString(format.ToString());
            tb.SelectionStart = tb.Text.Length;
        }

        private decimal _Increment = 1;
        [Description("用上、下调整数值时的增量"), Category("Common")]
        public decimal Increment
        {
            get => _Increment;
            set
            {
                if (_Increment == value)
                    return;
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("增量不可小于等于零");
                _Increment = value;
            }
        }

        private decimal _Maximum = 100;
        [Description("限定最大值"), Category("Common")]
        public decimal Maximum
        {
            get => _Maximum;
            set
            {
                if (_Maximum == value)
                    return;
                if (value < Minimum)
                    throw new ArgumentOutOfRangeException("最大值不能小于最小值");

                _Maximum = value;
                if (Value > _Maximum)
                    Value = _Maximum;
            }
        }
        private decimal _Minimum = 0;
        [Description("限定最小值"), Category("Common")]
        public decimal Minimum
        {
            get => _Minimum;
            set
            {
                if (_Minimum == value)
                    return;
                if (value > Maximum)
                    throw new ArgumentOutOfRangeException("最小值不能大于最大值");

                _Minimum = value;
                if (Value < _Minimum)
                    Value = _Minimum;
            }
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
                if (value <= 0)
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

        #region button down to change fast

        private Timer timer = null;
        private bool btnDownIsDown;
        private void TimerReStart()
        {
            if (timer == null)
            {
                timer = new Timer(TimerTick);
            }
            // 模拟键盘的按下，键盘按下后第一次触发260ms后开始循环，间隔为30ms
            timer.Change(260, 30);
        }
        private void TimerTick(object s)
        {
            if (btnDownIsDown)
            {
                if (_Value <= _Minimum)
                    TimerStop();
                else
                {
                    Dispatcher.Invoke(() => ValueDec());
                }
            }
            else
            {
                if (_Value >= _Maximum)
                    TimerStop();
                else
                {
                    Dispatcher.Invoke(() => ValueInc());
                }
            }
        }
        private void TimerStop()
        {
            timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }
        private void btn_up_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ValueInc();
            if (_Value >= _Maximum)
                return;
            btnDownIsDown = false;
            TimerReStart();
        }
        private void btn_down_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ValueDec();
            if (_Value <= _Minimum)
                return;
            btnDownIsDown = true;
            TimerReStart();
        }
        private void btn_up_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        { TimerStop(); }
        private void btn_up_MouseLeave(object sender, MouseEventArgs e)
        { TimerStop(); }
        private void btn_down_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        { TimerStop(); }
        private void btn_down_MouseLeave(object sender, MouseEventArgs e)
        { TimerStop(); }

        #endregion


        private void ValueInc()
        {
            Value += Increment;
        }
        private void ValueDec()
        {
            Value -= Increment;
        }

        private void tb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    ValueInc();
                    tb.Background = tbBGBrushPre;
                    e.Handled = true;
                    break;
                case Key.Down:
                    ValueDec();
                    tb.Background = tbBGBrushPre;
                    e.Handled = true;
                    break;
                case Key.Enter:
                    if (!tbValueErr)
                    {
                        _Value = tbValue;
                        tb_LostFocus(null, null);
                        ValueChanged?.Invoke(this);
                    }
                    e.Handled = true;
                    break;
                case Key.Escape:
                    tb_LostFocus(null, null);
                    break;
                    //case Key.NumPad0:
                    //case Key.NumPad1:
                    //case Key.NumPad2:
                    //case Key.NumPad3:
                    //case Key.NumPad4:
                    //case Key.NumPad5:
                    //case Key.NumPad6:
                    //case Key.NumPad7:
                    //case Key.NumPad8:
                    //case Key.NumPad9:
                    //case Key.D0:
                    //case Key.D1:
                    //case Key.D2:
                    //case Key.D3:
                    //case Key.D4:
                    //case Key.D5:
                    //case Key.D6:
                    //case Key.D7:
                    //case Key.D8:
                    //case Key.D9:
                    //case Key.Decimal:
                    //case Key.Back:
                    //case Key.Delete:
                    //case Key.Left:
                    //case Key.Right:
                    //    // allow user to input
                    //    break;
                    //default:
                    //    e.Handled = true;
                    //    break;
            }
        }

        private Brush tbBGBrushPre = SystemColors.WindowTextBrush;
        private Brush tbBGBrushEdit, tbBGBrushErr;
        private decimal tbValue;
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

            if (decimal.TryParse(tx, out tbValue))
            {
                if (_AsPercentage)
                    tbValue /= 100;
                if (tbValue < _Minimum || tbValue > _Maximum)
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

    }
}
