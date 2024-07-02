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
    /// Interaction logic for ColorPanelCommon.xaml
    /// </summary>
    public partial class ColorPanelCommon : UserControl
    {


        //public new bool IsEnabled
        //{
        //    get
        //    {
        //        return (bool)GetValue(IsEnabledProperty);
        //    }
        //    set
        //    {
        //        SetValue(IsEnabledProperty, value);
        //        colorSlider.IsEnabled = value;
        //        if (value)
        //        {
        //            clrAdjuster.SetColor(clrMgr.WorkingColor);
        //            RefreshWholePanel();
        //        }
        //    }
        //}

        //// Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty IsEnabledProperty =
        //    DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(ColorPanelCommon), new PropertyMetadata(true));

        private Class.ColorExpertCore core;
        private Class.ColorExpertCore.AdjusterHSLnHSV clrAdjuster;
        public ColorPanelCommon()
        {
            InitializeComponent();
            //"txLabel_rectangle" > Rectangle </sys:String >
            //"txLabel_cricular" > Circular </sys:String >
            //ResourceDictionary rd = Application.Current.Resources;
            //if (!rd.Contains("txLabel_rectangle"))
            //    rd.Add("txLabel_rectangle", "Rectangle");
            //if (!rd.Contains("txLabel_cricular"))
            //    rd.Add("txLabel_cricular", "Circular");
        }
        private bool isNotInited = true;
        public void InitFromStaticCore()
        {
            Init(Class.ColorExpertCore.GetInstance());
        }
        public void Init(Class.ColorExpertCore core)
        {
            this.core = core;
            core.WorkingColorChanged -= ColorMgr_WorkingColorChanged;
            core.WorkingColorIndexChanged -= ColorMgr_WorkingColorIndexChanged;

            clrAdjuster = core.adjusterHSLnHSV;
            colorSlider.Init(
                QuickGraphics.Image.GetSlideImage(
                    new float[] { 0f, 0.5f, 1f },
                    new Color[] { Colors.White, core.adjusterHSLnHSV.GetColorHSFromHSL(), Colors.Black }
                    ));

            // init cross
            img_cross.Source = QuickGraphics.Shape.Crosses.GetImgCrossC1().bitmapSource;

            clrAdjuster.SetColor(core.WorkingColor);
            core.WorkingColorChanged += ColorMgr_WorkingColorChanged;
            core.WorkingColorIndexChanged += ColorMgr_WorkingColorIndexChanged;

            colorSlider.SetSelectedValue = new Action<double>(SetLumFromSlider);

            IsEnabledChanged += (s2, e2) =>
            {
                if (this.IsEnabled)
                {
                    Color curColor = clrAdjuster.GetColor();
                    if (curColor != core.WorkingColor)
                        clrAdjuster.SetColor(core.WorkingColor);
                    _H = -1;
                    RefreshWholePanel();
                }
            };

            isNotInited = false;

            TabControl_SizeChanged(null, null);
            tab_img_SelectionChanged(null, null);
            RefreshWholePanel();
        }
        private void SetLumFromSlider(double newLum)
        {
            clrAdjuster.Lum = 1 - newLum;
            clrAdjuster.SetBackWorkingColor();
        }

        //private void UserControl_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if (isNotInited) return;
        //}


        private void ColorMgr_WorkingColorIndexChanged(object sender, int workingColorIndex)
        {
            if (!IsEnabled)
                return;
            clrAdjuster.SetColor(core.WorkingColor);
            RefreshWholePanel();
        }

        private void ColorMgr_WorkingColorChanged(object sender, int workingColorIndex, object changer)
        {
            if (!IsEnabled)
                return;
            if (changer != clrAdjuster)
            {
                clrAdjuster.SetColor(core.WorkingColor);
            }
            RefreshWholePanel();
        }

        #region set panel from core vars

        private double _H = -1, _S = -1, _L = -1;
        private Color _clr;
        public void RefreshWholePanel()
        {
            SetCrossPosi();

            if (_H != clrAdjuster.Hue
                || _S != clrAdjuster.HSL_Sat)
            {
                colorSlider.StripeImage = QuickGraphics.Image.GetSlideImage(
                    new float[] { 0f, 0.5f, 1f }, new Color[] { Colors.White, clrAdjuster.GetColorHSFromHSL(), Colors.Black });
            }
            if (_H != clrAdjuster.Hue
                || _S != clrAdjuster.HSL_Sat
                || _L != clrAdjuster.Lum)
            {
                _clr = clrAdjuster.GetColorFromHSL();
                RefreshSlider();
                RefreshTextBar();
            }

            _H = clrAdjuster.Hue;
            _S = clrAdjuster.HSL_Sat;
            _L = clrAdjuster.Lum;
        }
        private void RefreshSlider()
        {
            colorSlider.SetHandleColor(_clr);
            colorSlider.SetHandlePosi(1 - clrAdjuster.Lum);
        }
        private void RefreshTextBar()
        {
            tb_colorCode.Background = new SolidColorBrush(_clr);
            tb_colorCode.Foreground = new SolidColorBrush(core.GetBetterForeColor(_clr));

            StringBuilder clrTxBdr = new StringBuilder();
            clrTxBdr.Append(_clr.ToString());
            clrTxBdr.AppendLine();
            clrTxBdr.Append(_clr.R.ToString());
            clrTxBdr.Append(", ");
            clrTxBdr.Append(_clr.G.ToString());
            clrTxBdr.Append(", ");
            clrTxBdr.Append(_clr.B.ToString());
            tb_colorCode.Text = clrTxBdr.ToString();
        }

        #endregion



        public bool IsRectImg = true;
        private void tab_img_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isNotInited) return;
            IsRectImg = tab_img.SelectedIndex == 0;
            if (IsRectImg)
            {
                img_colorPanel.Source = clrAdjuster.ColorPanelImageRetangle;
            }
            else
            {
                img_colorPanel.Source = clrAdjuster.ColorPanelImageCircular;
            }
            SetCrossPosi();
        }

        #region select color from image

        public bool IsSliding = false;
        private void bdr_img_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            IsSliding = true;
            PickGraphPixAndSetSelectedColor(Mouse.GetPosition(bdr_img));
        }

        private void bdr_img_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (IsSliding)
                PickGraphPixAndSetSelectedColor(Mouse.GetPosition(bdr_img));
        }

        private void bdr_img_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            IsSliding = false;
        }
        private void bdr_img_MouseLeave(object sender, MouseEventArgs e)
        {
            IsSliding = false;
        }
        private void PickGraphPixAndSetSelectedColor(Point mousePosi)
        {
            double xPer = (double)mousePosi.X / bdr_img.ActualWidth;
            if (xPer < 0) xPer = 0;
            else if (xPer > 1) xPer = 1;
            double yPer = (double)mousePosi.Y / bdr_img.ActualHeight;
            if (yPer < 0) yPer = 0;
            else if (yPer > 1) yPer = 1;

            // draw a cross on panel
            if (IsRectImg)
            {
                clrAdjuster.Hue = xPer * 360;
                clrAdjuster.HSL_Sat = 1 - yPer;
            }
            else
            {
                // for getting ui vars
                Color colorHS = clrAdjuster.GetColorAt(xPer, yPer, IsRectImg);

                clrAdjuster.Hue = clrAdjuster.retanglePointXfromCircle * 360;
                clrAdjuster.HSL_Sat = 1 - clrAdjuster.retanglePointYfromCircle;
            }

            // after color changed, the callback event will refresh the whole panel
            //SetCrossPosi();
            clrAdjuster.SetBackWorkingColor();
        }


        private void SetCrossPosi()
        {
            if (isNotInited) return;
            double xPer, yPer;
            //draw a cross around the PickGraphIndicatorPoint
            if (IsRectImg)
            {
                xPer = clrAdjuster.Hue / 360;
                yPer = 1 - clrAdjuster.HSL_Sat;
            }
            else
            {
                Point cirPoint = clrAdjuster.GetCircularPointFromCurHS();
                xPer = cirPoint.X;
                yPer = cirPoint.Y;
            }
            img_cross.Width = Math.Max(9, img_colorPanel.ActualWidth / 100);
            img_cross.Height = Math.Max(9, img_colorPanel.ActualHeight / 100);
            img_cross.Margin = new Thickness(
                bdr_img.Margin.Left + img_colorPanel.Margin.Left + xPer * img_colorPanel.ActualWidth - img_cross.ActualWidth / 2,
                bdr_img.Margin.Top + img_colorPanel.Margin.Top + yPer * img_colorPanel.ActualHeight - img_cross.ActualHeight / 2,
                0, 0);
        }


        #endregion




        private void TabControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            grid.RowDefinitions[0].Height = new GridLength(grid.ColumnDefinitions[0].ActualWidth + 23);
            SetCrossPosi();
            colorSlider.SetHandlePosi(1 - _L);
        }
    }
}
