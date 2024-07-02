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

namespace MadTomDev.UI.ColorExpertControls
{
    /// <summary>
    /// Interaction logic for ColorCode.xaml
    /// </summary>
    public partial class ColorCode : UserControl
    {
        public ColorCode()
        {
            InitializeComponent();
        }

        private Class.ColorExpertCore core;
        public void InitFromStaticCore()
        {
            Init(Class.ColorExpertCore.GetInstance());
        }
        public void Init(Class.ColorExpertCore core)
        {
            this.core = core;


            core.WorkingColorChanged += clrMgr_WorkingColorChanged;
            core.WorkingColorIndexChanged += clrMgr_WorkingColorIndexChanged;

            colorSlider_3bit.Init(QuickGraphics.Image.GetColorStrip_3bit(false), false);
            colorSlider_6bit.Init(QuickGraphics.Image.GetColorStrip_6bit(false), false);
            colorSlider_9bit.Init(QuickGraphics.Image.GetColorStrip_9bit(false), false);
            colorSlider_12bit.Init(QuickGraphics.Image.GetColorStrip_12bit(false), false);

            colorSlider_3bit.SetHandleWidth(20);
            colorSlider_6bit.SetHandleWidth(20);
            colorSlider_9bit.SetHandleWidth(20);
            colorSlider_12bit.SetHandleWidth(20);

            colorSlider_3bit.SetSelectedValue = new Action<double>(SetColorFromSlider3bit);
            colorSlider_6bit.SetSelectedValue = new Action<double>(SetColorFromSlider6bit);
            colorSlider_9bit.SetSelectedValue = new Action<double>(SetColorFromSlider9bit);
            colorSlider_12bit.SetSelectedValue = new Action<double>(SetColorFromSlider12bit);

            IsEnabledChanged += (s2, e2) =>
            {
                if (this.IsEnabled)
                {
                    RefreshPanel();
                }
            };
            RefreshPanel();
        }
        private void clrMgr_WorkingColorIndexChanged(object sender, int workingColorIndex)
        {
            if (!IsEnabled)
                return;
            RefreshPanel();
        }

        private void clrMgr_WorkingColorChanged(object sender, int workingColorIndex, object changer)
        {
            if (!IsEnabled)
                return;
            if (changer != this)
                RefreshPanel();
        }


        public void RefreshPanel()
        {
            _SetSliders(core.WorkingColor);
            _SetTextBox(core.WorkingColor);
        }
        private void _SetSliders(Color clr)
        {
            cb_3bit.IsChecked = false;
            cb_6bit.IsChecked = false;
            cb_9bit.IsChecked = false;
            cb_12bit.IsChecked = false;

            int[] v4 = QuickGraphics.Image.values_4bit;
            int[] v3 = QuickGraphics.Image.values_3bit;
            int[] v2 = QuickGraphics.Image.values_2bit;
            int[] v1 = QuickGraphics.Image.values_1bit;
            double sdValue;
            if (v1.Contains(clr.R) && v1.Contains(clr.G) && v1.Contains(clr.B))
            {
                if (SetColorFromSlider_actSldIdx == 0)
                {
                    colorSlider_3bit.SetHandleColor(clr);
                    cb_3bit.IsChecked = true;
                }
                else
                {
                    sdValue = GetSdValue(v1);
                    colorSlider_3bit.SetHandlePosi(sdValue);
                    if (sdValue >= 0)
                    {
                        cb_3bit.IsChecked = true;
                        colorSlider_3bit.SetHandleColor(clr);
                    }
                }
            }
            if (v2.Contains(clr.R) && v2.Contains(clr.G) && v2.Contains(clr.B))
            {
                if (SetColorFromSlider_actSldIdx == 1)
                {
                    colorSlider_6bit.SetHandleColor(clr);
                    cb_6bit.IsChecked = true;
                }
                else
                {
                    sdValue = GetSdValue(v2);
                    colorSlider_6bit.SetHandlePosi(sdValue);
                    if (sdValue >= 0)
                    {
                        cb_6bit.IsChecked = true;
                        colorSlider_6bit.SetHandleColor(clr);
                    }
                }
            }
            if (v3.Contains(clr.R) && v3.Contains(clr.G) && v3.Contains(clr.B))
            {
                if (SetColorFromSlider_actSldIdx == 2)
                {
                    colorSlider_9bit.SetHandleColor(clr);
                    cb_9bit.IsChecked = true;
                }
                else
                {
                    sdValue = GetSdValue(v3);
                    colorSlider_9bit.SetHandlePosi(sdValue);
                    if (sdValue >= 0)
                    {
                        cb_9bit.IsChecked = true;
                        colorSlider_9bit.SetHandleColor(clr);
                    }
                }
            }
            if (v4.Contains(clr.R) && v4.Contains(clr.G) && v4.Contains(clr.B))
            {
                if (SetColorFromSlider_actSldIdx == 3)
                {
                    colorSlider_12bit.SetHandleColor(clr);
                    cb_12bit.IsChecked = true;
                }
                else
                {
                    sdValue = GetSdValue(v4);
                    colorSlider_12bit.SetHandlePosi(sdValue);
                    if (sdValue >= 0)
                    {
                        cb_12bit.IsChecked = true;
                        colorSlider_12bit.SetHandleColor(clr);
                    }
                }
            }

            double GetSdValue(int[] va)
            {
                int len = va.Length;
                int maxi = len - 1;
                int ri = Array.IndexOf(va, clr.R);
                int gi = Array.IndexOf(va, clr.G);
                int bi = Array.IndexOf(va, clr.B);
                double rdi = ri, gdi = gi, bdi = bi;
                double result = -1;
                if (ri == maxi && gi == 0)
                {
                    result = (5d + (1 - bdi / len)) / 6;
                }
                else if (gi == 0 && bi == maxi)
                {
                    result = (4d + (rdi + 1) / len) / 6;
                }
                else if (ri == 0 && bi == maxi)
                {
                    result = (3d + (1 - gdi / len)) / 6;
                }
                else if (ri == 0 && gi == maxi)
                {
                    result = (2d + (bdi + 1) / len) / 6;
                }
                else if (gi == maxi && bi == 0)
                {
                    result = (1d + (1 - rdi / len)) / 6;
                }
                else if (ri == maxi && bi == 0)
                {
                    result = ((gdi + 1) / len) / 6;
                }
                else result = -1;

                if (result == 1)
                    result = 0;
                return result;// + (1d / (6 * len));
            }
        }
        private void _SetTextBox(Color clr)
        {
            tb_clr.Background = new SolidColorBrush(clr);
            tb_clr_isProgSetting = true;
            if (tb_clr_skipOnce)
                tb_clr_skipOnce = false;
            else
                tb_clr.Text = clr.ToString();
            tb_clr_isProgSetting = false;
            tb_clr.Foreground = new SolidColorBrush(core.GetBetterForeColor(clr));
        }

        private void SetColorFromSlider3bit(double sValue)
        { SetColorFromSlider(0, sValue); }
        private void SetColorFromSlider6bit(double sValue)
        { SetColorFromSlider(1, sValue); }
        private void SetColorFromSlider9bit(double sValue)
        { SetColorFromSlider(2, sValue); }
        private void SetColorFromSlider12bit(double sValue)
        { SetColorFromSlider(3, sValue); }

        private static double[] sValuePoints = new double[] { 0, 0.166667, 0.333333, 0.5, 0.666667, 0.833333, 1 };
        private int SetColorFromSlider_actSldIdx = -1;
        private void SetColorFromSlider(int sldIdx, double sValue)
        {
            SetColorFromSlider_actSldIdx = sldIdx;
            byte r = 0, g = 0, b = 0;
            if (sValue < sValuePoints[1])
            {
                r = 255; b = 0;
                g = GetRoundValue(sldIdx, sValue * 6 * 255, true);
            }
            else if (sValue < sValuePoints[2])
            {
                g = 255; b = 0;
                r = GetRoundValue(sldIdx, (sValuePoints[2] - sValue) * 6 * 255, false);
            }
            else if (sValue < sValuePoints[3])
            {
                r = 0; g = 255;
                b = GetRoundValue(sldIdx, (sValue - sValuePoints[2]) * 6 * 255, true);
            }
            else if (sValue < sValuePoints[4])
            {
                r = 0; b = 255;  //
                g = GetRoundValue(sldIdx, (sValuePoints[4] - sValue) * 6 * 255, false);
            }
            else if (sValue < sValuePoints[5])
            {
                g = 0; b = 255;  //
                r = GetRoundValue(sldIdx, (sValue - sValuePoints[4]) * 6 * 255, true);
            }
            else if (sValue < sValuePoints[6])
            {
                r = 255; g = 0;
                b = GetRoundValue(sldIdx, (1 - sValue) * 6 * 255, false);
            }
            else
            {
                return;
            }

            Color newColor = Color.FromArgb(255, r, g, b);
            core.SetWorkingColor(newColor, this);
            RefreshPanel();
            SetColorFromSlider_actSldIdx = -1;

            byte GetRoundValue(int sldIdx, double value, bool smallFirst)
            {
                int[] v;
                switch (sldIdx)
                {
                    default:
                    case 0:
                        v = QuickGraphics.Image.values_1bit;
                        break;
                    case 1:
                        v = QuickGraphics.Image.values_2bit;
                        break;
                    case 2:
                        v = QuickGraphics.Image.values_3bit;
                        break;
                    case 3:
                        v = QuickGraphics.Image.values_4bit;
                        break;
                }
                int curV;
                double lastInc = -1;
                for (int i = 0, iv = v.Length; i < iv; ++i)
                {
                    curV = v[i];

                    if (curV == value)
                    {
                        return (byte)curV;
                    }
                    else if (curV < value)
                    {
                        lastInc = value - curV;
                    }
                    else
                    {
                        if (smallFirst)
                            return (byte)v[i - 1];
                        else
                            return (byte)v[i];
                    }
                }
                return 255;
            }
        }

        private bool tb_clr_isProgSetting = false;
        private bool tb_clr_skipOnce = false;
        private void tb_clr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tb_clr_isProgSetting)
                return;

            try
            {
                object testClr = ColorConverter.ConvertFromString(tb_clr.Text);
                if (testClr != null && testClr is Color)
                {
                    Color newColor = (Color)testClr;
                    core.SetWorkingColor(newColor, this);
                    tb_clr_skipOnce = true;
                    RefreshPanel();
                }
            }
            catch (Exception) { }
        }
    }
}
