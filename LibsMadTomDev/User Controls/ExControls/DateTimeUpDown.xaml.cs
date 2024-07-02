using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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

namespace MadTomDev.UI
{
    /// <summary>
    /// Interaction logic for DateTimeUpDown.xaml
    /// </summary>
    public partial class DateTimeUpDown : UserControl
    {
        /// <summary>
        /// 通过输入数字或按下方向光标开快速设定日期、时间，按住Shift后按上下可一次增减10；
        /// 注意不支持缩写格式字符串，即当格式字符串和显示字符串长度不匹配时，可能会出问题；
        /// 也不支持12小时制，建议只使用定长y M d H m s f这集中组合；
        /// 使用Ctrl-c复制选中字符串，或无选择时复制整串，使用Ctrl-v，尝试粘贴剪贴板时间；
        /// </summary>
        public DateTimeUpDown()
        {
            InitializeComponent();
            Value = DateTime.Now;
        }



        private string _DateTimeStringFormate = "yyyy-MM-dd HH:mm:ss.fff";
        public string DateTimeStringFormate
        {
            get => _DateTimeStringFormate;
            set
            {
                SetNumber_startIdx = 0;
                _DateTimeStringFormate = value;
                int s = tb.SelectionStart, l = tb.SelectionLength;
                tb.Text = _Value.ToString(value);
                tb.SelectionStart = s; tb.SelectionLength = l;
                SeekSegment();
            }
        }

        private DateTime _Value;
        public DateTime Value
        {
            get => _Value;
            set
            {
                //SetNumber_startIdx = 0;
                _Value = value;
                ValueChanged?.Invoke(this, new EventArgs());
                int s = tb.SelectionStart, l = tb.SelectionLength;
                tb.Text = value.ToString(_DateTimeStringFormate);
                tb.SelectionStart = s; tb.SelectionLength = l;
            }
        }



        //private DateTime _Value;
        //public DateTime Value
        //{
        //    get { return (DateTime)GetValue(ValueProperty); }
        //    set
        //    {
        //        SetValue(ValueProperty, value);
        //        _Value = value;
        //        ValueChanged?.Invoke(this, new EventArgs());
        //        int s = tb.SelectionStart, l = tb.SelectionLength;
        //        tb.Text = value.ToString(_DateTimeStringFormate);
        //        tb.SelectionStart = s; tb.SelectionLength = l;
        //    }
        //}
        //public static readonly new DependencyProperty ValueProperty =
        //    DependencyProperty.Register("Value", typeof(DateTime), typeof(DateTimeUpDown));//, new PropertyMetadata("2023-07-05"));




        public event EventHandler ValueChanged;

        private bool _IsReadonly = false;
        public bool IsReadonly
        {
            get => _IsReadonly;
            set
            {
                _IsReadonly = value;
               
            }
        }

        private void tb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isCtrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            if (isCtrlDown)
            {
                switch (e.Key)
                {
                    case Key.C:
                        if (tb.SelectionLength > 0)
                            Clipboard.SetText(tb.Text.Substring(tb.SelectionStart, tb.SelectionLength));
                        else
                            Clipboard.SetText(tb.Text);
                        break;
                    case Key.V:
                        if (Clipboard.ContainsText())
                        {
                            string tx = Clipboard.GetText();
                            if (DateTime.TryParse(tx, out DateTime v))
                            {
                                Value = v;
                            }
                        }
                        break;
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.D0:
                    case Key.NumPad0:
                        SetNumber(0);
                        e.Handled = true;
                        break;
                    case Key.D1:
                    case Key.NumPad1:
                        SetNumber(1);
                        e.Handled = true;
                        break;
                    case Key.D2:
                    case Key.NumPad2:
                        SetNumber(2);
                        e.Handled = true;
                        break;
                    case Key.D3:
                    case Key.NumPad3:
                        SetNumber(3);
                        e.Handled = true;
                        break;
                    case Key.D4:
                    case Key.NumPad4:
                        SetNumber(4);
                        e.Handled = true;
                        break;
                    case Key.D5:
                    case Key.NumPad5:
                        SetNumber(5);
                        e.Handled = true;
                        break;
                    case Key.D6:
                    case Key.NumPad6:
                        SetNumber(6);
                        e.Handled = true;
                        break;
                    case Key.D7:
                    case Key.NumPad7:
                        SetNumber(7);
                        e.Handled = true;
                        break;
                    case Key.D8:
                    case Key.NumPad8:
                        SetNumber(8);
                        e.Handled = true;
                        break;
                    case Key.D9:
                    case Key.NumPad9:
                        SetNumber(9);
                        e.Handled = true;
                        break;

                    case Key.Left:
                        SetDirection(0);
                        tb.SelectionStart = segmentStart;
                        e.Handled = true;
                        break;
                    case Key.Up:
                        SetDirection(1);
                        tb.SelectionStart = segmentStart;
                        e.Handled = true;
                        break;
                    case Key.Right:
                        SetDirection(2);
                        tb.SelectionStart = segmentStart;
                        e.Handled = true;
                        break;
                    case Key.Down:
                        SetDirection(3);
                        tb.SelectionStart = segmentStart;
                        e.Handled = true;
                        break;
                }
            }

        }

        private void tb_GotFocus(object sender, RoutedEventArgs e)
        {
            SetNumber_startIdx = 0;
            SeekSegment();
        }

        private void tb_LostFocus(object sender, RoutedEventArgs e)
        {
            SetNumber_startIdx = 0;
            rect.Width = 0;
        }
        private async void tb_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SetNumber_startIdx = 0;
            await Task.Delay(2);
            SeekSegment();
        }

        // 0-year 1-month 2-date 3-hour 4-minute 5-second 6-miliSecond
        private int segmentType = -1;
        private int segmentStart, segmentLength;
        public void SeekSegment()
        {
            int l = tb.Text.Length;
            if (l <= 0 || _DateTimeStringFormate.Length != l)
            {
                segmentType = -1;
            }
            else
            {
                int s = tb.SelectionStart;
                if (s == l)
                    --s;

                // 聚焦位置的字符，判断类型
                char sc = _DateTimeStringFormate[s];
                int t;
                segmentType = GetSegmentCharType(ref sc);

                int tInc = 1;
                bool leftE = false, rightE = false;
                if (segmentType < 0)
                {
                    while (true)
                    {
                        t = s + tInc;
                        if (t < l)
                        {
                            sc = _DateTimeStringFormate[t];
                            segmentType = GetSegmentCharType(ref sc);
                            if (segmentType >= 0)
                            {
                                s = t;
                                break;
                            }
                        }
                        else
                        {
                            rightE = true;
                        }

                        t = s - tInc;
                        if (t >= 0)
                        {
                            sc = _DateTimeStringFormate[t];
                            segmentType = GetSegmentCharType(ref sc);
                            if (segmentType >= 0)
                            {
                                s = t;
                                break;
                            }
                        }
                        else
                        {
                            leftE = true;
                        }
                        if (leftE && rightE)
                        {
                            segmentType = -1;
                            s = 0;
                            break;
                        }
                        ++tInc;
                    }
                }

                sc = _DateTimeStringFormate[s];

                // 寻找当前字符串的起始和长度
                int st = s;
                while (st >= 0 && _DateTimeStringFormate[st] == sc)
                    --st;
                ++st;
                if (st < 0)
                    st = 0;
                segmentStart = st;
                st = s + 1;
                while (st < l && _DateTimeStringFormate[st] == sc)
                    ++st;
                segmentLength = st - segmentStart;

                if (segmentLength > 0)
                {
                    double margLeft = 0;
                    if (segmentStart > 0)
                        margLeft = QuickGraphics.Text.MeasureWidth(tb.Text.Substring(0, segmentStart), tb);
                    rect.Width = QuickGraphics.Text.MeasureWidth(tb.Text.Substring(segmentStart, segmentLength), tb);
                    double fullTextWidth = QuickGraphics.Text.MeasureWidth(tb.Text, tb);
                    switch (tb.TextAlignment)
                    {
                        case TextAlignment.Left:
                        case TextAlignment.Justify:
                            rect.HorizontalAlignment = HorizontalAlignment.Left;
                            rect.Margin = new Thickness(margLeft + 3, 3, 0, 3);
                            break;
                        case TextAlignment.Center:
                            rect.HorizontalAlignment = HorizontalAlignment.Center;
                            double offset = (rect.Width - fullTextWidth) / 2;
                            rect.Margin = new Thickness(offset + margLeft , 3, -offset, 3);
                            break;
                        case TextAlignment.Right:
                            rect.HorizontalAlignment = HorizontalAlignment.Right;
                            rect.Margin = new Thickness(0, 3, fullTextWidth - rect.Width + margLeft + 3, 3);
                            break;
                    }
                }
                else
                {
                    rect.Width = 0;
                }
            }
        }
        private int GetSegmentCharType(ref char c)
        {
            switch (c)
            {
                case 'y': return 0;
                case 'M': return 1;
                case 'd': return 2;
                case 'H': return 3;
                case 'm': return 4;
                case 's': return 5;
                case 'f': return 6;
            }
            return -1;
        }


        public TextAlignment TextAlignment
        {
            get => tb.TextAlignment;
            set
            {
                tb.TextAlignment = value;
                SeekSegment();
            }
        }


        /// <summary>
        /// arrow key pressed, 0-left 1-up 2-right 3-down
        /// </summary>
        /// <param name="d"></param>
        private void SetDirection(int d)
        {
            SetNumber_startIdx = 0;

            int l = _DateTimeStringFormate.Length;
            if (l <= 0)
                return;

            SeekSegment();
            if (d == 0)
            {
                // left
                int s = segmentStart;
                if (s < 0)
                    return;
                char curC = _DateTimeStringFormate[s];
                int tPre = GetSegmentCharType(ref curC);
                int t;
                do
                {
                    if (--s < 0)
                        return;
                    curC = _DateTimeStringFormate[s];
                    t = GetSegmentCharType(ref curC);
                }
                while (t == tPre || t < 0);
                tb.SelectionStart = s;
                SeekSegment();
            }
            else if (d == 2)
            {
                // right
                int s = segmentStart + segmentLength - 1;
                if (s >= l)
                    return;
                char curC = _DateTimeStringFormate[s];
                int tPre = GetSegmentCharType(ref curC);
                int t;
                do
                {
                    if (++s >= l)
                        return;
                    curC = _DateTimeStringFormate[s];
                    t = GetSegmentCharType(ref curC);
                }
                while (t == tPre || t < 0);
                tb.SelectionStart = s;
                SeekSegment();
            }
            else if (d == 1 || d == 3)
            {
                bool upOrDown = d == 1;
                bool isShift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

                try
                {
                    switch (segmentType)
                    {
                        case 0: // year
                            if (upOrDown) Value = Value.AddYears(isShift ? 10 : 1);
                            else Value = Value.AddYears(isShift ? -10 : -1);
                            break;
                        case 1: // month
                            if (upOrDown) Value = Value.AddMonths(isShift ? 10 : 1);
                            else Value = Value.AddMonths(isShift ? -10 : -1);
                            break;
                        case 2: // date
                            if (upOrDown) Value = Value.AddDays(isShift ? 10 : 1);
                            else Value = Value.AddDays(isShift ? -10 : -1);
                            break;
                        case 3: // hour
                            if (upOrDown) Value = Value.AddHours(isShift ? 10 : 1);
                            else Value = Value.AddHours(isShift ? -10 : -1);
                            break;
                        case 4: // min
                            if (upOrDown) Value = Value.AddMinutes(isShift ? 10 : 1);
                            else Value = Value.AddMinutes(isShift ? -10 : -1);
                            break;
                        case 5: // sec
                            if (upOrDown) Value = Value.AddSeconds(isShift ? 10 : 1);
                            else Value = Value.AddSeconds(isShift ? -10 : -1);
                            break;
                        case 6: // miliSec
                            if (upOrDown) Value = Value.AddMilliseconds(isShift ? 10 : 1);
                            else Value = Value.AddMilliseconds(isShift ? -10 : -1);
                            break;
                    }
                }
                catch (Exception err)
                {
                    SystemSounds.Beep.Play();
                }
            }
        }


        private int SetNumber_startIdx = 0;
        private void SetNumber(int n, bool seekSegFirst = true)
        {
            if (seekSegFirst)
                SeekSegment();

            tb.SelectionLength = 0;
            switch (segmentType)
            {
                case 0: // year
                    if (SetNumber_startIdx < segmentLength)
                    {
                        Value = Value.AddYears(n * GetMultiple() - (SetNumber_startIdx == 0 ? Value.Year : 0));
                        if (SetNumber_startIdx >= (segmentLength - 1))
                        {
                            tb.SelectionStart = segmentStart;
                            SetNumber_startIdx = -1;
                        }
                        else
                        {
                            tb.SelectionStart = segmentStart + SetNumber_startIdx + 1;
                        }
                    }
                    break;
                case 1: // month
                    Value = Value.AddMonths(StepValue2(Value.Month, 1, 12));
                    break;
                case 2: // day
                    int days = DateTime.DaysInMonth(Value.Year, Value.Month);
                    Value = Value.AddDays(StepValue2(Value.Day, 1, days));
                    break;
                case 3: // hour
                    Value = Value.AddHours(StepValue2(Value.Hour, 0, 24));
                    break;
                case 4: // min
                    Value = Value.AddMinutes(StepValue2(Value.Minute, 0, 60));
                    break;
                case 5: // sec
                    Value = Value.AddSeconds(StepValue2(Value.Second, 0, 60));
                    break;
                case 6: // miliSec
                    if (SetNumber_startIdx < segmentLength)
                    {
                        Value = Value.AddMilliseconds(n * GetMultipleN(2 - SetNumber_startIdx)
                            - (SetNumber_startIdx == 0 ? Value.Millisecond : 0));
                        if (segmentLength - 1 <= SetNumber_startIdx)
                        {
                            tb.SelectionStart = segmentStart;
                            SetNumber_startIdx = -1;
                        }
                        else
                        {
                            tb.SelectionStart = segmentStart + SetNumber_startIdx + 1;
                        }
                    }
                    break;
            }
            ++SetNumber_startIdx;

            int StepValue2(int curValue, int minValue, int maxValue)
            {
                int result = 0, d1 = maxValue % 10, d2 = maxValue / 10;
                if (SetNumber_startIdx == 0)
                {
                    if (n == 0)
                    {
                        result = -(curValue / 10) * 10;
                        if ((curValue % 10) < minValue)
                            result += minValue;
                        tb.SelectionStart = segmentStart + SetNumber_startIdx + 1;
                    }
                    else
                    {
                        result = n * 10 - curValue;
                        if ((d1 == 0 && (d2 - 1) < n) || d2 < n)
                        {
                            tb.SelectionStart = segmentStart;
                            SetNumber_startIdx = -1;
                        }
                        else
                        {
                            tb.SelectionStart = segmentStart + SetNumber_startIdx + 1;
                        }
                    }
                }
                else if (SetNumber_startIdx == 1)
                {
                    if (curValue / 10 == d2 && d1 < n)
                        result = maxValue - curValue;
                    else
                        result = n - (curValue % 10);
                    tb.SelectionStart = segmentStart;
                    SetNumber_startIdx = -1;
                }
                return result;
            }

            int GetMultiple()
            {
                int result = 1;
                for (int i = 0, iv = segmentLength - SetNumber_startIdx - 1; i < iv; i++)
                    result *= 10;
                return result;
            }
            double GetMultipleN(int n)
            {
                if (n == 0)
                    return 1;

                int result = 1;
                if (n > 0)
                {
                    for (int i = 0; i < n; ++i)
                        result *= 10;
                }
                else // < 0
                {
                    for (int i = n; i < 0; ++i)
                        result /= 10;
                }
                return result;
            }
        }

    }
}
