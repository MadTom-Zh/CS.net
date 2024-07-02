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
    /// Interaction logic for ColorPanelSVH.xaml
    /// </summary>
    public partial class ColorPanelSVH : UserControl
    {
        public ColorPanelSVH()
        {
            InitializeComponent();
        }
        public void InitFromStaticCore()
        {
            Init(Class.ColorExpertCore.GetInstance());
        }
        public void Init(Class.ColorExpertCore core)
        {
            this.core = core;
            clrAdjuster = core.adjusterHSLnHSV;

            colorSlider.Init(clrAdjuster.HueSlideImage, true, clrAdjuster.Hue);
            img_cross.Source = QuickGraphics.Shape.Crosses.GetImgCrossC1().bitmapSource;

            core.WorkingColorChanged += clrMgr_WorkingColorChanged;
            core.WorkingColorIndexChanged += clrMgr_WorkingColorIndexChanged;

            colorSlider.SetSelectedValue = new Action<double>(SetHueFromColorSlider);

            IsEnabledChanged += (s2, e2) =>
            {
                if (this.IsEnabled)
                {
                    Color curColor = clrAdjuster.GetColor();
                    if (curColor != core.WorkingColor)
                        clrAdjuster.SetColor(core.WorkingColor);
                    _H = -1;
                    RefreshPanel();
                }
            };
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            img_cross.Source = QuickGraphics.Shape.Crosses.GetImgCrossC1().bitmapSource;
            grid_colorPanel_SizeChanged(null, null);
            RefreshPanel();
        }

        private Class.ColorExpertCore core;
        private Class.ColorExpertCore.AdjusterHSLnHSV clrAdjuster;


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
            if (changer != clrAdjuster)
                core.adjusterHSLnHSV.SetColor(core.WorkingColor);
            RefreshPanel();
        }


        private double _H = -1, _S = -1, _V = -1;
        private Color _colorH;

        public void RefreshPanel()
        {
            if (clrAdjuster == null)
                return;

            RefreshSelecterPictureNSlider();
            SetSlideBarsNTexts();

            _H = clrAdjuster.Hue;
            _S = clrAdjuster.HSV_Sat;
            _V = clrAdjuster.Value;
        }
        private void RefreshSelecterPictureNSlider()
        {
            if (_H != clrAdjuster.Hue)
            {
                RefreshSlidePosition();
                _colorH = clrAdjuster.GetColorHFromHSL();
                img_colorPanel.Source = clrAdjuster.GetColorPanelSV(_colorH);
                colorSlider.SetHandleColor(_colorH);
            }
            if (_S != clrAdjuster.HSV_Sat || _V != clrAdjuster.Value)
            {
                SetCrossPosi();
            }
        }
        private void SetCrossPosi()
        {
            if (clrAdjuster == null)
                return;

            int oPx = (int)(clrAdjuster.HSV_Sat * img_colorPanel.ActualWidth);
            int oPy = (int)((1 - clrAdjuster.Value) * img_colorPanel.ActualHeight);

            img_cross.Width = Math.Max(9, img_colorPanel.ActualWidth / 100);
            img_cross.Height = Math.Max(9, img_colorPanel.ActualHeight / 100);
            img_cross.Margin = new Thickness(
                bdr_img.Margin.Left + img_colorPanel.Margin.Left + oPx - img_cross.ActualWidth / 2,
                bdr_img.Margin.Top + img_colorPanel.Margin.Top + oPy - img_cross.ActualHeight / 2,
                0, 0);
        }
        private void RefreshSlidePosition()
        {
            if (core == null)
                return;

            colorSlider.SetHandlePosi(clrAdjuster.Hue / 360);
        }
        private bool isSettingSld = false;
        private void SetSlideBarsNTexts()
        {
            if (_H == clrAdjuster.Hue
                && _S == clrAdjuster.HSV_Sat
                && _V == clrAdjuster.Value)
                return;
            string hPerStr = clrAdjuster.Hue.ToString("0") + "°";
            string sPerStr = clrAdjuster.HSV_Sat.ToString("P0");
            string vPerStr = clrAdjuster.Value.ToString("P0");
            isSettingSld = true;
            if (_H != clrAdjuster.Hue)
            {
                sld_h.Value = clrAdjuster.Hue;
                _H = clrAdjuster.Hue;
            }
            if (_S != clrAdjuster.HSV_Sat)
            {
                sld_s.Value = clrAdjuster.HSV_Sat;
                _S = clrAdjuster.HSV_Sat;
            }
            if (_V != clrAdjuster.Lum)
            {
                sld_v.Value = clrAdjuster.Value;
                _V = clrAdjuster.Value;
            }
            isSettingSld = false;

            tb_h.Text = "H " + hPerStr;
            tb_s.Text = "S " + sPerStr;
            tb_v.Text = "V " + vPerStr;

            Color curColor = core.WorkingColor;
            StringBuilder strBdr = new StringBuilder();
            strBdr.Append(curColor.ToString());
            strBdr.AppendLine();
            strBdr.Append(hPerStr);
            strBdr.Append(", ");
            strBdr.Append(sPerStr);
            strBdr.Append(", ");
            strBdr.Append(vPerStr);
            tb_colorCode.Text = strBdr.ToString();
            tb_colorCode.Background = new SolidColorBrush(curColor);
            tb_colorCode.Foreground = new SolidColorBrush(core.GetBetterForeColor(curColor));
        }





        #region select S L form image panel
        private bool isSlidingGraph = false;
        private void bdr_img_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isSlidingGraph = true;
            SelectSL(Mouse.GetPosition(bdr_img));
        }
        private void bdr_img_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isSlidingGraph)
            {
                SelectSL(Mouse.GetPosition(bdr_img));
            }
        }
        private void bdr_img_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            isSlidingGraph = false;
        }
        private void bdr_img_MouseLeave(object sender, MouseEventArgs e)
        {
            isSlidingGraph = false;
        }

        private void SelectSL(Point mouseLocation)
        {
            clrAdjuster.HSV_Sat = (double)mouseLocation.X / img_colorPanel.ActualWidth;
            clrAdjuster.Value = 1 - (double)mouseLocation.Y / img_colorPanel.ActualHeight;

            clrAdjuster.SetBackWorkingColor();
            SetCrossPosi();
            SetSlideBarsNTexts();
        }

        #endregion

        private void sld_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isSettingSld)
                return;

            if (sender == sld_h)
                clrAdjuster.Hue = sld_h.Value;
            else if (sender == sld_v)
                clrAdjuster.Value = sld_v.Value;
            else if (sender == sld_s)
                clrAdjuster.HSV_Sat = sld_s.Value;

            clrAdjuster.SetBackWorkingColor();
        }


        private void SetHueFromColorSlider(double sldValue)
        {
            clrAdjuster.Hue = sldValue * 360;
            clrAdjuster.SetBackWorkingColor();
        }


        private void grid_colorPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            grid.RowDefinitions[0].Height = new GridLength(grid.ColumnDefinitions[0].ActualWidth);
            SetCrossPosi();
            colorSlider.SetHandlePosi(_V);
        }
    }
}
