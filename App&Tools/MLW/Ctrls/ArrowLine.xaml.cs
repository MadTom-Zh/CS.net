using MadTomDev.UI;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace MLW_Succubus_Storys.Ctrls
{
    /// <summary>
    /// Interaction logic for ArrowLine.xaml
    /// </summary>
    public partial class ArrowLine : UserControl
    {
        public ArrowLine()
        {
            InitializeComponent();
        }

        public BtnNode btnStart, btnEnd;

        public Point lineEndPoint;
        public double lineThickness = 3;
        public double lineStartSpace = 0, lineEndSpace = 0;
        public bool lineHaveStartArrow = false, lineHaveEndArrow = true;
        public double lineStartArrowWidthMul = 3, lineEndArrowWidthMul = 3;
        public Brush lineBrush;
        public void SetBasicProperties(double lineThickness, double lineStartSpace, double lineEndSpace,
            bool lineHaveStartArrow, bool lineHaveEndArrow,
            double lineStartArrowWidthMul, double lineEndArrowWidthMul)
        {
            this.lineThickness = lineThickness;
            this.lineStartSpace = lineStartSpace;
            this.lineEndSpace = lineEndSpace;
            this.lineHaveStartArrow = lineHaveStartArrow;
            this.lineHaveEndArrow = lineHaveEndArrow;
            this.lineStartArrowWidthMul = lineStartArrowWidthMul;
            this.lineEndArrowWidthMul = lineEndArrowWidthMul;
        }

        public static double ang45 = Math.PI / 4;
        public static double ang30 = Math.PI / 6;
        public double cos30 = Math.Cos(ang30);
        public double sin45 = Math.Sin(ang45);
        public void ReDraw()
        {
            ReDraw(lineEndPoint);
        }
        public void ReDraw(Point endPoint)
        {
            lineEndPoint = endPoint;
            double lineLength
                = Math.Sqrt(endPoint.X * endPoint.X + endPoint.Y * endPoint.Y)
                    - lineStartSpace - lineEndSpace;
            if (lineLength < 0)
                throw new Exception("Length below zero!");


            // 偏角
            double ang = Math.Atan2(endPoint.Y, endPoint.X);
            // 计算真实起点和终点
            Point ptS, ptE;
            if (lineStartSpace != 0)
            {
                ptS = new Point(
                    lineStartSpace * Math.Cos(ang),
                    lineStartSpace * Math.Sin(ang));
            }
            else ptS = new Point(0, 0);
            if (lineEndSpace != 0)
            {
                ptE = new Point(
                    endPoint.X - lineEndSpace * Math.Cos(ang),
                    endPoint.Y - lineEndSpace * Math.Sin(ang));
            }
            else ptE = endPoint;

            // 如果有起始箭头
            List<Point> pathData = new List<Point>();
            double halfThickness = lineThickness / 2;
            double arrowWidthS = 0, arrowWidthE;
            Point ptS0 = new Point(ptS.X + halfThickness / sin45, ptS.Y);
            Point ptE0 = new Point(ptE.X - halfThickness / sin45, ptE.Y);
            Point ptS1, ptS2, ptE1, ptE2;
            if (lineHaveStartArrow)
            {
                arrowWidthS = lineThickness * lineStartArrowWidthMul;
                ptS1 = new Point(ptS.X + arrowWidthS, ptS.Y);
                pathData.Add(QuickGraphics.Rotate.RotatePoint_PI(ptS, ptS1, ang + ang30));
                ptS2 = new Point(ptS.X + arrowWidthS * cos30, ptS.Y + halfThickness);
                pathData.Add(QuickGraphics.Rotate.RotatePoint_PI(ptS, ptS2, ang));
            }
            else
            {
                pathData.Add(QuickGraphics.Rotate.RotatePoint_PI(ptS, ptS0, ang + ang30));
            }

            if (lineHaveEndArrow)
            {
                arrowWidthE = lineThickness * lineEndArrowWidthMul;
                ptE1 = new Point(ptE.X - arrowWidthE, ptE.Y);
                ptE2 = new Point(ptE.X - arrowWidthE * cos30, ptE.Y + halfThickness);
                pathData.Add(QuickGraphics.Rotate.RotatePoint_PI(ptE, ptE2, ang));
                pathData.Add(QuickGraphics.Rotate.RotatePoint_PI(ptE, ptE1, ang - ang30));
                pathData.Add(ptE);
                pathData.Add(QuickGraphics.Rotate.RotatePoint_PI(ptE, ptE1, ang + ang30));
                ptE2.Y = ptE.Y - halfThickness;
                pathData.Add(QuickGraphics.Rotate.RotatePoint_PI(ptE, ptE2, ang));
            }
            else
            {
                pathData.Add(QuickGraphics.Rotate.RotatePoint_PI(ptE, ptE0, ang - ang30));
                pathData.Add(ptE);
                pathData.Add(QuickGraphics.Rotate.RotatePoint_PI(ptE, ptE0, ang + ang30));
            }

            if (lineHaveStartArrow)
            {
                ptS2.Y = ptS1.Y - halfThickness;
                pathData.Add(QuickGraphics.Rotate.RotatePoint_PI(ptS, ptS2, ang));
                pathData.Add(QuickGraphics.Rotate.RotatePoint_PI(ptS, ptS1, ang - ang30));
            }
            else
            {
                pathData.Add(QuickGraphics.Rotate.RotatePoint_PI(ptS, ptS0, ang - ang30));
            }

            PathGeometry pg = new PathGeometry();
            PathFigure pf = new PathFigure();
            pf.StartPoint = ptS;
            for (int i = 0, iv = pathData.Count; i < iv; ++i)
                pf.Segments.Add(new LineSegment(pathData[i], true));
            pg.Figures.Add(pf);
            p.Data = pg;
            p.Fill = lineBrush;
        }
        public void ReDraw(Point endPoint, Brush brush)
        {
            SetBrush(brush);
            ReDraw(endPoint);
        }

        public void SetBrush(Brush brush)
        {
            lineBrush = brush;
        }
        public void SetShadowProperties(Color shadowColor, double shadowOpacity, double shadowBlurRadius)
        {
            this.shadowColor = shadowColor;
            this.shadowOpacity = shadowOpacity;
            this.shadowBlurRadius = shadowBlurRadius;
        }

        public Color shadowColor = Colors.LightCoral;
        public double shadowOpacity = 1;
        public double shadowBlurRadius = 10;


        internal bool isShadowOn = false;
        public void SetShadowOnOff(bool onOrOff)
        {
            if (onOrOff)
            {
                DropShadowEffect se = new DropShadowEffect()
                {
                    Color = shadowColor,
                    Opacity = shadowOpacity,
                    BlurRadius = shadowBlurRadius,
                    ShadowDepth = 0,
                    Direction = 0,
                };
                this.Effect = se;
                isShadowOn = true;
            }
            else
            {
                this.Effect = null;
                isShadowOn = false;
            }
        }

        public static SolidColorBrush arrowLineBrushDark = new SolidColorBrush(Colors.DimGray);
        public static SolidColorBrush arrowLineBrushInactive = new SolidColorBrush(Colors.LightGray);
        public static SolidColorBrush arrowLineBrushActive = new SolidColorBrush(Colors.Lime);
        public static SolidColorBrush arrowLineBrushNext = new SolidColorBrush(Colors.Aqua);
        public enum ArrowLineStyles
        {   Inactive, Active, Next,Dark }
        public ArrowLineStyles ArrowLineStyle
        {
            set
            {
                switch (value)
                {
                    case ArrowLineStyles.Dark:
                        SetBrush(arrowLineBrushDark);
                        ReDraw();
                        SetShadowOnOff(false);
                        break;
                    case ArrowLineStyles.Inactive:
                        SetBrush(arrowLineBrushInactive);
                        ReDraw();
                        SetShadowOnOff(true);
                        break;
                    case ArrowLineStyles.Active:
                        SetBrush(arrowLineBrushActive);
                        ReDraw();
                        SetShadowOnOff(true);
                        break;
                    case ArrowLineStyles.Next:
                        SetBrush(arrowLineBrushNext);
                        ReDraw();
                        SetShadowOnOff(true);
                        break;
                }
            }
        }
    }
}
