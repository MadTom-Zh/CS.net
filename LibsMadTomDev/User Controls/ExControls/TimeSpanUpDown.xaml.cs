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
    /// Interaction logic for TimeSpanUpDown.xaml
    /// </summary>
    public partial class TimeSpanUpDown : UserControl
    {
        public TimeSpanUpDown()
        {
            InitializeComponent();
            Value = new TimeSpan();
        }


        private string _TimeSpanStringFormate = @"\ ddd\.\ hh\:mm\:ss\.\ fff";
        private string TimeSpanStringFormate
        {
            get => _TimeSpanStringFormate;
            set
            {
                SetNumber_startIdx = 0;
                _TimeSpanStringFormate = value;
                int s = tb.SelectionStart, l = tb.SelectionLength;
                tb.Text = _Value.ToString(value);
                tb.SelectionStart = s; tb.SelectionLength = l;
                SeekSegment();
            }
        }

        private TimeSpan _Value;
        public TimeSpan Value
        {
            get => _Value;
            set
            {
                //SetNumber_startIdx = 0;
                _Value = value;
                ValueChanged?.Invoke(this, new EventArgs());
                int s = tb.SelectionStart, l = tb.SelectionLength;
                if (value < TimeSpan.Zero)
                    _TimeSpanStringFormate = @"\-ddd\.\ hh\:mm\:ss\.\ fff";
                else
                    _TimeSpanStringFormate = @"\ ddd\.\ hh\:mm\:ss\.\ fff";
                tb.Text = value.ToString(_TimeSpanStringFormate);
                tb.SelectionStart = s; tb.SelectionLength = l;
            }
        }

        private bool _CanValueNegative = true;
        public bool CanValueNegative
        {
            get => _CanValueNegative;
            set
            {
                if (_CanValueNegative == value)
                    return;
                _CanValueNegative = value;
                if (!value && _Value < TimeSpan.Zero)
                {
                    Value = TimeSpan.Zero;
                }
            }
        }


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
                            if (TimeSpan.TryParse(tx, out TimeSpan v))
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
            if (l <= 0 || _TimeSpanStringFormate.Length < l)
            {
                segmentType = -1;
            }
            else
            {
                int s = tb.SelectionStart;
                if (s == l)
                    --s;

                // 聚焦位置的字符，判断类型
                string matchString = _TimeSpanStringFormate.Replace("\\", "");
                string addingString = "";
                for (int i = 0, iv = l - matchString.Length; i < iv; ++i)
                {
                    addingString += "d";
                }
                if (addingString.Length > 0)
                {
                    matchString = matchString.Substring(0, 1) + addingString + matchString.Substring(1);
                }
                char sc = matchString[s];
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
                            sc = matchString[t];
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
                            sc = matchString[t];
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

                sc = matchString[s];

                // 寻找当前字符串的起始和长度
                int st = s;
                while (st >= 0 && matchString[st] == sc)
                    --st;
                ++st;
                if (st < 0)
                    st = 0;
                segmentStart = st;
                st = s + 1;
                while (st < l && matchString[st] == sc)
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
                            rect.Margin = new Thickness(offset + margLeft, 3, -offset, 3);
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
                //case 'y': return 0;
                //case 'M': return 1;
                case 'd': return 2;
                //case 'H': return 3;
                case 'h': return 3;
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

            int l = _TimeSpanStringFormate.Length;
            if (l <= 0)
                return;

            SeekSegment();
            if (d == 0)
            {
                // left
                int s = segmentStart;
                if (s < 0)
                    return;
                char curC = _TimeSpanStringFormate[s];
                int tPre = GetSegmentCharType(ref curC);
                int t;
                do
                {
                    if (--s < 0)
                        return;
                    curC = _TimeSpanStringFormate[s];
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
                char curC = _TimeSpanStringFormate[s];
                int tPre = GetSegmentCharType(ref curC);
                int t;
                do
                {
                    if (++s >= l)
                        return;
                    curC = _TimeSpanStringFormate[s];
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
                    TimeSpan addTime;
                    switch (segmentType)
                    {
                        case 2: // date
                            addTime = new TimeSpan(24, 0, 0);
                            if (upOrDown) Value = Value.Add(isShift ? 10 * addTime : addTime);
                            else Value = Value.Add(isShift ? -10 * addTime : -addTime);
                            break;
                        case 3: // hour
                            addTime = new TimeSpan(1, 0, 0);
                            if (upOrDown) Value = Value.Add(isShift ? 10 * addTime : addTime);
                            else Value = Value.Add(isShift ? -10 * addTime : -addTime);
                            break;
                        case 4: // min
                            addTime = new TimeSpan(0, 1, 0);
                            if (upOrDown) Value = Value.Add(isShift ? 10 * addTime : addTime);
                            else Value = Value.Add(isShift ? -10 * addTime : -addTime);
                            break;
                        case 5: // sec
                            addTime = new TimeSpan(0, 0, 1);
                            if (upOrDown) Value = Value.Add(isShift ? 10 * addTime : addTime);
                            else Value = Value.Add(isShift ? -10 * addTime : -addTime);
                            break;
                        case 6: // miliSec
                            addTime = new TimeSpan(0, 0, 0, 0, 1);
                            if (upOrDown) Value = Value.Add(isShift ? 10 * addTime : addTime);
                            else Value = Value.Add(isShift ? -10 * addTime : -addTime);
                            break;
                    }
                    if (!_CanValueNegative && _Value < TimeSpan.Zero)
                        _Value = TimeSpan.Zero;
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
            TimeSpan addTime;
            switch (segmentType)
            {
                case 2: // day
                    if (SetNumber_startIdx < segmentLength)
                    {
                        addTime = new TimeSpan(n * GetMultiple() - (SetNumber_startIdx == 0 ? (int)Value.TotalDays : 0), 0, 0, 0);
                        if(_Value < TimeSpan.Zero)
                            Value = _Value.Add(-addTime);
                        else
                            Value = _Value.Add(addTime);

                        if (Value.TotalDays >= 1000)
                            Value.Add(new TimeSpan(-1000, 0, 0, 0));
                        else if (Value.TotalDays <= -1000)
                            Value.Add(new TimeSpan(1000, 0, 0, 0));
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
                case 3: // hour
                    addTime = new TimeSpan(StepValue2(Value.Hours, 0, 24), 0, 0); 
                    if (_Value < TimeSpan.Zero)
                        Value = _Value.Add(-addTime);
                    else
                        Value = _Value.Add(addTime);
                    break;
                case 4: // min
                    addTime = new TimeSpan(0, StepValue2(Value.Minutes, 0, 60), 0);
                    if (_Value < TimeSpan.Zero)
                        Value = _Value.Add(-addTime);
                    else
                        Value = _Value.Add(addTime);
                    break;
                case 5: // sec
                    addTime = new TimeSpan(0, 0, StepValue2(Value.Seconds, 0, 60));
                    if (_Value < TimeSpan.Zero)
                        Value = _Value.Add(-addTime);
                    else
                        Value = _Value.Add(addTime);
                    break;
                case 6: // miliSec
                    if (SetNumber_startIdx < segmentLength)
                    {
                        addTime = new TimeSpan(0, 0, 0, 0, n * (int)GetMultipleN(2 - SetNumber_startIdx)
                            - (SetNumber_startIdx == 0 ? Value.Milliseconds : 0));
                        if (_Value < TimeSpan.Zero)
                            Value = _Value.Add(-addTime);
                        else
                            Value = _Value.Add(addTime);
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
