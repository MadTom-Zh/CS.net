using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MadTomDev.UI.Class
{
    public class ColorExpertCore
    {
        public ColorExpertCore() { Init(); }

        private static ColorExpertCore instance = null;
        public static ColorExpertCore GetInstance()
        {
            if (instance == null)
            {
                instance = new ColorExpertCore();
            }
            return instance;
        }
        private void Init()
        {
            workingColorList = new List<Color>(15);
            for (int i = workingColorList.Capacity; i > 0; i--)
                workingColorList.Add(Colors.White);

            adjusterHSLnHSV = new AdjusterHSLnHSV(this);
            adjusterCMYK = new AdjusterCMYK(this);
        }

        public delegate void WorkingColorIndexChangedDelegate(object sender, int workingColorIndex);
        public delegate void WorkingColorChangedDelegate(object sender, int workingColorIndex, object changer);
        public event WorkingColorIndexChangedDelegate WorkingColorIndexChanged;
        public event WorkingColorChangedDelegate WorkingColorChanged;
        public List<Color> workingColorList;
        private int _WorkingColorIndex = 0;
        public int WorkingColorIndex
        {
            get { return _WorkingColorIndex; }
            set
            {
                if (_WorkingColorIndex == value)
                    return;
                if (value < 0)
                    throw new Exception("Index should not be less than Zero.");
                else if (value >= workingColorList.Count)
                    throw new Exception("Index out of range.");
                _WorkingColorIndex = value;
                WorkingColorIndexChanged?.Invoke(this, _WorkingColorIndex);
            }
        }

        internal Color GetBetterForeColor(Color backColor)
        {
            return Color.FromArgb(
                255,
                (byte)((backColor.R + 128) % 256),
                (byte)((backColor.G + 128) % 256),
                (byte)((backColor.B + 128) % 256));
        }

        public Color WorkingColor
        { get => workingColorList[_WorkingColorIndex]; }

        public delegate void AlphaChangedDelegate(object sender, byte alpla);
        public event AlphaChangedDelegate AlphaChanged;
        public byte _Alpha = 255;
        public byte Alpha
        {
            get => _Alpha;
            set
            {
                if (_Alpha == value)
                    return;
                _Alpha = value;
                AlphaChanged?.Invoke(this, value);
                WorkingColorWithAlphaChanged?.Invoke(this, WorkingColorWithAlpha);
            }
        }
        public delegate void WorkingColorWithAlphaChangedDelegate(object sender, Color argb);
        public event WorkingColorWithAlphaChangedDelegate WorkingColorWithAlphaChanged;
        public Color WorkingColorWithAlpha
        { get => Color.FromArgb(_Alpha, WorkingColor.R, WorkingColor.G, WorkingColor.B); }
        public void SetWorkingColor(Color color, object changer)
        {
            //if (workingColorList[_WorkingColorIndex] == color)
            //    return;
            workingColorList[_WorkingColorIndex] = color;
            WorkingColorChanged?.Invoke(this, _WorkingColorIndex, changer);
            WorkingColorWithAlphaChanged?.Invoke(this, WorkingColorWithAlpha);
        }



        public interface IColorAdjuster
        {
            void SetBackWorkingColor(Color color);
            void SetBackWorkingColor();
            void SetColor(Color color);
            void SetColor();
            Color GetColor();
        }

        public AdjusterHSLnHSV adjusterHSLnHSV;
        public class AdjusterHSLnHSV : IColorAdjuster
        {
            public ColorExpertCore parent;
            public AdjusterHSLnHSV(ColorExpertCore parent)
            { this.parent = parent; }
            public void SetBackWorkingColor(Color color)
            { parent.SetWorkingColor(color, this); }
            public void SetBackWorkingColor()
            { parent.SetWorkingColor(GetColorFromHSL(), this); }

            #region UI images

            private BitmapSource _HueSlideImage = null;
            public BitmapSource HueSlideImage
            {
                get
                {
                    if (_HueSlideImage == null)
                    {
                        _HueSlideImage = QuickGraphics.Image.GetSlideImage(
                            new float[] { 0f, 0.16667f, 0.33333f, 0.5f, 0.66667f, 0.83333f, 1f },
                            new Color[]
                            { Color.FromArgb(255,255,0,0),Color.FromArgb(255,255,255,0),Color.FromArgb(255,0,255,0),
                            Color.FromArgb(255,0,255,255),Color.FromArgb(255,0,0,255),Color.FromArgb(255,255,0,255),
                            Color.FromArgb(255,255,0,0)});
                    }
                    return _HueSlideImage;
                }
            }

            private BitmapSource _ColorPanelImageRetangle = null;
            public BitmapSource ColorPanelImageRetangle
            {
                get
                {
                    if (_ColorPanelImageRetangle == null)
                    {
                        //// from top to above, change value to 127
                        //Color startColor, lastColor;
                        //Color[] bcArray = BrightColorArray;
                        //byte r, g, b;
                        //_ColorPanelImageRetangle = new Bitmap(bcArray.Length, 382); // height = 1 + 3*127
                        //for (int x = 0, xMax = bcArray.Length, yIdx, y, yMax = 128;
                        //    x < xMax; x++)
                        //{
                        //    startColor = bcArray[x];
                        //    lastColor = startColor;
                        //    _ColorPanelImageRetangle.SetPixel(x, 0, startColor);
                        //    for (y = 1; y < yMax; y++)
                        //    {
                        //        yIdx = y * 3;
                        //        r = (byte)(startColor.R + (float)y * (127 - startColor.R) / 128);
                        //        _ColorPanelImageRetangle.SetPixel(x, yIdx - 2, Color.FromArgb(r, lastColor.G, lastColor.B));
                        //        g = (byte)(startColor.G + (float)y * (127 - startColor.G) / 128);
                        //        _ColorPanelImageRetangle.SetPixel(x, yIdx - 1, Color.FromArgb(r, g, lastColor.B));
                        //        b = (byte)(startColor.B + (float)y * (127 - startColor.B) / 128);
                        //        lastColor = Color.FromArgb(r, g, b);
                        //        _ColorPanelImageRetangle.SetPixel(x, yIdx, lastColor);
                        //    }
                        //}

                        System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(1530, 382);
                        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                        {
                            QuickGraphics.Image.FillRectangleGradient_Bitmap(
                                g, new System.Drawing.Rectangle(new System.Drawing.Point(0, 0),
                                new System.Drawing.Size(255, 382)),
                                System.Drawing.Color.FromArgb(255, 0, 0), System.Drawing.Color.FromArgb(255, 255, 0),
                                System.Drawing.Color.FromArgb(127, 127, 127), System.Drawing.Color.FromArgb(127, 127, 127));
                            QuickGraphics.Image.FillRectangleGradient_Bitmap(
                                g, new System.Drawing.Rectangle(new System.Drawing.Point(255, 0),
                                new System.Drawing.Size(255, 382)),
                                System.Drawing.Color.FromArgb(255, 255, 0), System.Drawing.Color.FromArgb(0, 255, 0),
                                System.Drawing.Color.FromArgb(127, 127, 127), System.Drawing.Color.FromArgb(127, 127, 127));
                            QuickGraphics.Image.FillRectangleGradient_Bitmap(
                                g, new System.Drawing.Rectangle(new System.Drawing.Point(510, 0),
                                new System.Drawing.Size(255, 382)),
                                System.Drawing.Color.FromArgb(0, 255, 0), System.Drawing.Color.FromArgb(0, 255, 255),
                                System.Drawing.Color.FromArgb(127, 127, 127), System.Drawing.Color.FromArgb(127, 127, 127));
                            QuickGraphics.Image.FillRectangleGradient_Bitmap(
                                g, new System.Drawing.Rectangle(new System.Drawing.Point(765, 0),
                                new System.Drawing.Size(255, 382)),
                                System.Drawing.Color.FromArgb(0, 255, 255), System.Drawing.Color.FromArgb(0, 0, 255),
                                System.Drawing.Color.FromArgb(127, 127, 127), System.Drawing.Color.FromArgb(127, 127, 127));
                            QuickGraphics.Image.FillRectangleGradient_Bitmap(
                                g, new System.Drawing.Rectangle(new System.Drawing.Point(1020, 0),
                                new System.Drawing.Size(255, 382)),
                                System.Drawing.Color.FromArgb(0, 0, 255), System.Drawing.Color.FromArgb(255, 0, 255),
                                System.Drawing.Color.FromArgb(127, 127, 127), System.Drawing.Color.FromArgb(127, 127, 127));
                            QuickGraphics.Image.FillRectangleGradient_Bitmap(
                                g, new System.Drawing.Rectangle(new System.Drawing.Point(1275, 0),
                                new System.Drawing.Size(255, 382)),
                                System.Drawing.Color.FromArgb(255, 0, 255), System.Drawing.Color.FromArgb(255, 0, 0),
                                System.Drawing.Color.FromArgb(127, 127, 127), System.Drawing.Color.FromArgb(127, 127, 127));
                        }

                        _ColorPanelImageRetangle = QuickGraphics.BitmapToBitmapSource(bitmap);
                    }
                    return _ColorPanelImageRetangle;
                }
            }

            private BitmapSource _ColorPanelImageCircular = null;
            public BitmapSource ColorPanelImageCircular
            {
                get
                {
                    if (_ColorPanelImageCircular == null)
                    {
                        System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(500, 500);
                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, 500, 500);
                        using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bitmap))
                        {
                            System.Drawing.Drawing2D.GraphicsPath wheel_path = new System.Drawing.Drawing2D.GraphicsPath();
                            wheel_path.AddEllipse(rect);
                            wheel_path.Flatten();

                            float num_pts = (wheel_path.PointCount - 1) / 6;
                            System.Drawing.Color[] surround_colors = new System.Drawing.Color[wheel_path.PointCount];

                            int index = 0;
                            InterpolateColors(surround_colors, ref index,
                                1 * num_pts, 255, 255, 0, 0, 255, 255, 0, 255);
                            InterpolateColors(surround_colors, ref index,
                                2 * num_pts, 255, 255, 0, 255, 255, 0, 0, 255);
                            InterpolateColors(surround_colors, ref index,
                                3 * num_pts, 255, 0, 0, 255, 255, 0, 255, 255);
                            InterpolateColors(surround_colors, ref index,
                                4 * num_pts, 255, 0, 255, 255, 255, 0, 255, 0);
                            InterpolateColors(surround_colors, ref index,
                                5 * num_pts, 255, 0, 255, 0, 255, 255, 255, 0);
                            InterpolateColors(surround_colors, ref index,
                                wheel_path.PointCount, 255, 255, 255, 0, 255, 255, 0, 0);

                            using (System.Drawing.Drawing2D.PathGradientBrush path_brush =
                                new System.Drawing.Drawing2D.PathGradientBrush(wheel_path))
                            {
                                path_brush.CenterColor = System.Drawing.Color.FromArgb(127, 127, 127);
                                path_brush.SurroundColors = surround_colors;

                                gr.FillPath(path_brush, wheel_path);

                                // It looks better if we outline the wheel.
                                //using (Pen thick_pen = new Pen(outline_color, 2))
                                //{
                                //    gr.DrawPath(thick_pen, wheel_path);
                                //}
                            }
                        }
                        _ColorPanelImageCircular = QuickGraphics.BitmapToBitmapSource(bitmap);
                    }
                    return _ColorPanelImageCircular;
                }
            }

            public BitmapSource GetColorPanelSL(Color clrCenterRight)
            {
                System.Drawing.Bitmap newPic = new System.Drawing.Bitmap(500, 500);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newPic))
                {
                    QuickGraphics.Image.FillRectangleGradient_Bitmap(g,
                        new System.Drawing.Rectangle(0, 0, 500, 250),
                        System.Drawing.Color.White, System.Drawing.Color.White,
                        System.Drawing.Color.Gray, QuickGraphics.WinColorToColor(clrCenterRight));
                    QuickGraphics.Image.FillRectangleGradient_Bitmap(g,
                        new System.Drawing.Rectangle(0, 250, 500, 250),
                        System.Drawing.Color.Gray, QuickGraphics.WinColorToColor(clrCenterRight),
                        System.Drawing.Color.Black, System.Drawing.Color.Black);
                }
                return QuickGraphics.BitmapToBitmapSource(newPic);
            }
            public BitmapSource GetColorPanelSV(Color clrCenterRight)
            {
                System.Drawing.Bitmap newPic = new System.Drawing.Bitmap(500, 500);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newPic))
                {
                    QuickGraphics.Image.FillRectangleGradient_Bitmap(g,
                        new System.Drawing.Rectangle(0, 0, 500, 500),
                        System.Drawing.Color.White, QuickGraphics.WinColorToColor(clrCenterRight),
                        System.Drawing.Color.Black, System.Drawing.Color.Black);
                }
                return QuickGraphics.BitmapToBitmapSource(newPic);
            }



            // Fill in colors interpolating between the from and to values.
            private void InterpolateColors(System.Drawing.Color[] surround_colors,
                ref int index, float stop_pt,
                int from_a, int from_r, int from_g, int from_b,
                int to_a, int to_r, int to_g, int to_b)
            {
                int num_pts = (int)stop_pt - index;
                float a = from_a, r = from_r, g = from_g, b = from_b;
                float da = (to_a - from_a) / (num_pts - 1);
                float dr = (to_r - from_r) / (num_pts - 1);
                float dg = (to_g - from_g) / (num_pts - 1);
                float db = (to_b - from_b) / (num_pts - 1);

                for (int i = 0; i < num_pts; i++)
                {
                    surround_colors[index++] =
                        System.Drawing.Color.FromArgb((int)a, (int)r, (int)g, (int)b);
                    a += da;
                    r += dr;
                    g += dg;
                    b += db;
                }
            }

            #endregion


            #region UI operations

            private static double DoublePi = Math.PI * 2;
            public double circularColorRadius;
            public double retanglePointXfromCircle, retanglePointYfromCircle;
            public double originCirclePointX, originCirclePointY;
            public Color GetColorAt(double xPer, double yPer, bool isRetangle)
            {
                if (isRetangle)
                {
                    //int panelX = (int)(ColorPanelImageRetangle.Width * xPer);
                    //if (panelX < 0) panelX = 0;
                    //else if (panelX >= ColorPanelImageRetangle.Width) panelX = ColorPanelImageRetangle.Width - 1;
                    //int panelY = (int)(ColorPanelImageRetangle.Height * yPer);
                    //if (panelY < 0) panelY = 0;
                    //else if (panelY >= ColorPanelImageRetangle.Height) panelY = ColorPanelImageRetangle.Height - 1;

                    //return ColorPanelImageRetangle.GetPixel(panelX, panelY);

                    return parent.adjusterHSLnHSV.GetColorFromHSL(xPer * 360, 1 - yPer);
                }
                else
                {
                    originCirclePointX = xPer;
                    originCirclePointY = yPer;
                    circularColorRadius = Math.Sqrt(Math.Pow((xPer - 0.5), 2) + Math.Pow((yPer - 0.5), 2));
                    int retanY = circularColorRadius > 0.5 ? 0 : (int)((1 - (circularColorRadius / 0.5)) * ColorPanelImageRetangle.Height);


                    double v1x = 1 - 0.5;
                    double v1y = 0; //0.5 - 0.5;
                    double v2x = xPer - 0.5;
                    double v2y = yPer - 0.5;

                    if (v2x == 0 && v2y == 0)
                    {
                        //return ColorPanelImageRetangle.GetPixel(0, ColorPanelImageRetangle.Height - 1);
                        return parent.adjusterHSLnHSV.GetColorFromHSL(0, 0);
                    }

                    double angle = Math.Atan2(v1x, v1y) - Math.Atan2(v2x, v2y);

                    int retanX;
                    if (angle >= 0)
                        retanX = (int)((DoublePi - angle) / (DoublePi) * ColorPanelImageRetangle.Width);
                    else
                        retanX = (int)((-angle) / (DoublePi) * ColorPanelImageRetangle.Width);
                    retanglePointXfromCircle = (double)retanX / ColorPanelImageRetangle.Width;
                    retanglePointYfromCircle = (double)retanY / ColorPanelImageRetangle.Height;
                    //return ColorPanelImageRetangle.GetPixel(retanX, retanY);
                    return parent.adjusterHSLnHSV.GetColorFromHSL(retanglePointXfromCircle * 360, 1 - retanglePointYfromCircle);
                }
            }

            public Point GetCircularPointFromCurHS()
            {
                double radius = 0.5 - 0.5 * (1 - HSL_Sat);
                double angle = -(Hue / 360) * DoublePi;

                double x = 0.5 + Math.Cos(angle) * radius;
                double y = 0.5 + Math.Sin(angle) * radius;
                return new Point(x, y);
            }

            #endregion

            // how it works, read <<关于HSL,HSV体系转换的分析.docx>>

            //private static double QuarterPi = Math.PI / 4; // 45 deg
            private double _Hue = 0, _HSL_Sat = 1, _HSV_Sat = 0, _Lum = 0.5, _Value = 1.0;
            public double Hue  // x in RGB panel
            {
                get => _Hue;
                set
                {
                    if (_Hue == value) return;
                    if (value < 0) _Hue = 0;
                    else if (value >= 360) _Hue = 0;
                    else _Hue = value;
                }
            }
            public double HSL_Sat // y in RGB panel
            {
                get => _HSL_Sat;
                set
                {
                    if (_HSL_Sat == value) return;
                    if (value < 0) _HSL_Sat = 0;
                    else if (value > 1) _HSL_Sat = 1;
                    else _HSL_Sat = value;

                    UpdateSV_bySL();
                }
            }
            public double HSV_Sat
            {
                get => _HSV_Sat;
                set
                {
                    if (_HSV_Sat == value) return;
                    if (value < 0) _HSV_Sat = 0;
                    else if (value > 1) _HSV_Sat = 1;
                    else _HSV_Sat = value;

                    UpdateSL_bySV();
                }
            }
            public double Lum
            {
                get => _Lum;
                set
                {
                    if (_Lum == value) return;
                    if (value < 0) _Lum = 0;
                    else if (value > 1) _Lum = 1;
                    else _Lum = value;

                    UpdateSV_bySL();
                }
            }
            public double Value
            {
                get => _Value;
                set
                {
                    if (_Value == value) return;
                    if (value < 0) _Value = 0;
                    else if (value > 1) _Value = 1;
                    else _Value = value;

                    UpdateSL_bySV();
                }
            }
            private void UpdateSV_bySL()
            {
                double tmp1 = 2 * (1 - _Lum);
                if (_HSL_Sat > tmp1)
                {
                    // in triangle 4
                    _Value = 1;
                    _HSV_Sat = tmp1;
                }
                else
                {
                    // in triangle 1 2 3
                    _HSV_Sat = _HSL_Sat;
                    //_Value = _Lum + _HSL_Sat / 2;
                    _Value = 2 * _Lum / (2 - _HSL_Sat);
                    //if (_Value > 1)
                    //{
                    //    // seems impossible, but it happened
                    //    // like in triangle 4
                    //    _Value = 1;
                    //    _HSV_Sat = 2 * (1 - _Lum);
                    //}
                }
            }
            private void UpdateSL_bySV()
            {
                if (_Value <= _HSV_Sat)
                {
                    // in triangle 5
                    // change L, maybe S1 also
                    _Lum = _Value / 2;
                    //if (_HSL_Sat < _Value)
                    //    _HSL_Sat = _Lum;
                    _HSL_Sat = _HSV_Sat;
                }
                else
                {
                    // in both triangle 6 and 7
                    _HSL_Sat = _HSV_Sat;
                    //_Lum = _Value / (1 + _HSV_Sat);
                    _Lum = _Value - (_HSV_Sat / 2);
                }
            }


            public void SetColor(Color color)
            {
                double r = color.R / 255.0;
                double g = color.G / 255.0;
                double b = color.B / 255.0;

                double min, max, delta;

                min = Math.Min(r, g);
                min = Math.Min(min, b);

                max = Math.Max(r, g);
                max = Math.Max(max, b);

                _Lum = (max + min) / 2.0;
                delta = max - min;
                if (_Lum > 0.0)
                {
                    HSL_Sat = delta;

                    if (_HSL_Sat > 0.0)
                    {
                        _HSL_Sat /= (_Lum <= 0.5) ? (max + min) : (2.0 - max - min);
                    }
                }


                _Value = max;
                if (delta > 0)
                {
                    if (max > 0.0)
                    {
                        _HSV_Sat = (delta / max);

                        if (r >= max)                           // > is bogus, just keeps compilor happy
                            _Hue = (g - b) / delta;        // between yellow & magenta
                        else if (g >= max)
                            _Hue = 2.0 + (b - r) / delta;  // between cyan & yellow
                        else
                            _Hue = 4.0 + (r - g) / delta;  // between magenta & cyan

                        _Hue *= 60.0;                              // degrees


                        if (_Hue < 0.0)
                            _Hue += 360.0;
                    }
                    else
                    {
                        // if max is 0, then r = g = b = 0              
                        // s = 0, h is undefined
                        _HSV_Sat = 0.0;
                        //_Hue = 0;
                    }
                }
            }
            public void SetColor()
            { SetColor(parent.WorkingColor); }




            public Color GetColorFromHSL(double hue, double sat, double lum)
            {
                double v;
                double r, g, b;

                r = lum;   // default to gray
                g = lum;
                b = lum;
                v = (lum <= 0.5) ? (lum * (1.0 + sat)) : (lum + sat - lum * sat);

                if (v > 0)
                {
                    double m;
                    double sv;
                    int sextant;
                    double fract, vsf, mid1, mid2;



                    m = lum + lum - v;
                    sv = (v - m) / v;
                    hue /= 60;

                    sextant = (int)hue;
                    fract = hue - sextant;
                    vsf = v * sv * fract;
                    mid1 = m + vsf;
                    mid2 = v - vsf;

                    switch (sextant)
                    {
                        case 0:
                            r = v;
                            g = mid1;
                            b = m;
                            break;

                        case 1:
                            r = mid2;
                            g = v;
                            b = m;
                            break;

                        case 2:
                            r = m;
                            g = v;
                            b = mid1;
                            break;

                        case 3:
                            r = m;
                            g = mid2;
                            b = v;
                            break;

                        case 4:
                            r = mid1;
                            g = m;
                            b = v;
                            break;

                        case 5:
                            r = v;
                            g = m;
                            b = mid2;
                            break;
                    }
                }

                return Color.FromArgb(
                    255,
                    Convert.ToByte(r * 255.0f + 0.4),
                    Convert.ToByte(g * 255.0f + 0.4),
                    Convert.ToByte(b * 255.0f + 0.4));
            }
            public Color GetColorFromHSL(double hue, double sat)
            {
                return GetColorFromHSL(hue, sat, 0.5);
            }
            public Color GetColorFromHSL(double hue)
            {
                return GetColorFromHSL(hue, 1, 0.5);
            }
            public Color GetColorFromHSL()
            {
                return GetColorFromHSL(Hue, HSL_Sat, Lum);
            }
            public Color GetColorHFromHSL()
            {
                return GetColorFromHSL(Hue, 1, 0.5);
            }
            public Color GetColorHSFromHSL()
            {
                return GetColorFromHSL(Hue, HSL_Sat, 0.5);
            }



            public Color GetColorFromHSV(double hue, double sat, double value)
            {
                double r, g, b;

                if (sat <= 0.0)
                {       // < is bogus, just shuts up warnings
                    r = value;
                    g = value;
                    b = value;
                }
                else
                {
                    double hh, p, q, t, ff;
                    long i;
                    hh = hue;
                    if (hh >= 360.0) hh = 0.0;
                    hh /= 60.0;
                    i = (long)hh;
                    ff = hh - i;
                    p = value * (1.0 - sat);
                    q = value * (1.0 - (sat * ff));
                    t = value * (1.0 - (sat * (1.0 - ff)));

                    switch (i)
                    {
                        case 0:
                            r = value;
                            g = t;
                            b = p;
                            break;
                        case 1:
                            r = q;
                            g = value;
                            b = p;
                            break;
                        case 2:
                            r = p;
                            g = value;
                            b = t;
                            break;
                        case 3:
                            r = p;
                            g = q;
                            b = value;
                            break;
                        case 4:
                            r = t;
                            g = p;
                            b = value;
                            break;
                        case 5:
                        default:
                            r = value;
                            g = p;
                            b = q;
                            break;
                    }
                }
                return Color.FromArgb(
                    255,
                    (byte)(r * 255.0),
                    (byte)(g * 255.0),
                    (byte)(b * 255.0));
            }
            public Color GetColorFromHSV()
            {
                return GetColorFromHSV(Hue, HSV_Sat, Value);
            }
            public Color GetColorHFromHSV()
            {
                return GetColorFromHSV(Hue, 1, 1);
            }

            /// <summary>
            /// same result as GetColorFromHSL() and GetColorFromHSV();
            /// and, both GetColorFromHSL() or GetColorFromHSV() return same result;
            /// </summary>
            /// <returns></returns>
            public Color GetColor()
            {
                return GetColorFromHSL();
            }
        }


        public AdjusterCMYK adjusterCMYK;
        public class AdjusterCMYK : IColorAdjuster
        {
            public ColorExpertCore parent;
            public AdjusterCMYK(ColorExpertCore parent)
            { this.parent = parent; }


            public void SetBackWorkingColor(Color color)
            { parent.SetWorkingColor(color, this); }
            public void SetBackWorkingColor()
            { parent.SetWorkingColor(GetColor(), this); }

            private double _C = 0;
            private double _M = 0;
            private double _Y = 0;
            private double _K = 0;
            public double C
            {
                get => _C;
                set
                {
                    if (_C == value)
                        return;
                    if (value < 0) _C = 0;
                    else if (value > 1) _C = 1;
                    else _C = value;
                }
            }
            public double M
            {
                get => _M;
                set
                {
                    if (_M == value)
                        return;
                    if (value < 0) _M = 0;
                    else if (value > 1) _M = 1;
                    else _M = value;
                }
            }
            public double Y
            {
                get => _Y;
                set
                {
                    if (_Y == value)
                        return;
                    if (value < 0) _Y = 0;
                    else if (value > 1) _Y = 1;
                    else _Y = value;
                }
            }
            public double K
            {
                get => _K;
                set
                {
                    if (_K == value)
                        return;
                    if (value < 0) _K = 0;
                    else if (value > 1) _K = 1;
                    else _K = value;
                }
            }

            public void SetColor(Color color)
            {
                double r = (double)color.R / 255;
                double g = (double)color.G / 255;
                double b = (double)color.B / 255;
                double tmp = Math.Min(1 - r, 1 - g);
                K = Math.Min(tmp, 1 - b);
                double oneSubK = 1 - K;
                if (oneSubK == 0)
                {
                    C = 0;
                    M = 0;
                    Y = 0;
                }
                else
                {
                    C = (oneSubK - r) / oneSubK;
                    M = (oneSubK - g) / oneSubK;
                    Y = (oneSubK - b) / oneSubK;
                }
            }

            public void SetColor()
            { SetColor(parent.WorkingColor); }
            public Color GetColor()
            {
                byte r = (byte)(255.0 * (1 - C) * (1 - K));
                byte g = (byte)(255.0 * (1 - M) * (1 - K));
                byte b = (byte)(255.0 * (1 - Y) * (1 - K));

                return Color.FromArgb(255, r, g, b);
            }
        }

    }
}
