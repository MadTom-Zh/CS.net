using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MadTomDev.App
{
    public class StaticResource
    {
        public static BitmapSource UIIconExplorer32
        {
            get => Core.GetInstance().iconS32.GetIcon(15);
        }
        public static BitmapSource UIIconExplorer16
        {
            get => Core.GetInstance().iconS32.GetIcon(15, false);
        }
        public static BitmapSource UIIconRecycleBinEmpty32
        {
            get => Core.GetInstance().iconS32.GetIcon(101);
        }
        public static BitmapSource UIIconRecycleBinEmpty16
        {
            get => Core.GetInstance().iconS32.GetIcon(101, false);
        }
        public static BitmapSource UIIconRecycleBinFull32
        {
            get => Core.GetInstance().iconS32.GetIcon(102);
        }
        public static BitmapSource UIIconRecycleBinFull16
        {
            get => Core.GetInstance().iconS32.GetIcon(102, false);
        }
        public static BitmapSource UIIconNetwork16
        {
            get => Core.GetInstance().iconS32.GetIcon(13, false);
        }
        public static BitmapSource UIIconDiskTaskStates32
        {
            get => Core.GetInstance().iconS32.GetIcon(165);
        }
        public static BitmapSource UIIconSetting32
        {
            get => Core.GetInstance().iconS32.GetIcon(316);
        }
        public static BitmapSource UIIconHelp32
        {
            get => Core.GetInstance().iconS32.GetIcon(154);
        }
        public static BitmapSource UIIconInfo32
        {
            get => Core.GetInstance().iconS32.GetIcon(77);
        }
        public static BitmapSource SysIconInfo16
        {
            get => Core.GetInstance().iconSys.dictIconImage16["Asterisk"];
        }
        public static BitmapSource SysIconWarning16
        {
            get => Core.GetInstance().iconSys.dictIconImage16["Warning"];
        }
        public static BitmapSource SysIconError16
        {
            get => Core.GetInstance().iconSys.dictIconImage16["Error"];
        }

        private static BitmapSource _UIIconSearchRightToLeft16;
        public static BitmapSource UIIconSearchRightToLeft16
        {
            get
            {
                if (_UIIconSearchRightToLeft16 == null)
                {
                    _UIIconSearchRightToLeft16
                        = UI.QuickGraphics.Mirror.MirrorImageSource.LeftToRight(Core.GetInstance().iconS32.GetIcon(22, false));
                }
                return _UIIconSearchRightToLeft16;
            }
        }

        public static BitmapSource UIIconStyle16
        {
            get => Core.GetInstance().iconS32.GetIcon(141, false);
        }
        public static BitmapSource UIIconRefresh16
        {
            get => Core.GetInstance().iconS32.GetIcon(238, false);
        }
        public static BitmapSource UIIconHost16
        {
            get => Core.GetInstance().iconS32.GetIcon(17, false);
        }
        public static BitmapSource UIIconNetDir16
        {
            get => Core.GetInstance().iconS32.GetIcon(275, false);
        }

        public static BitmapSource UIIconHostShare16
        {
            get => Core.GetInstance().iconS32.GetIcon(275, false);
        }
        public static BitmapSource UIIconStop16
        {
            get => Core.GetInstance().iconS32.GetIcon(109, false);
        }

        private static BitmapSource _UIIconPlus32;
        public static BitmapSource UIIconPlus32
        {
            get
            {
                if (_UIIconPlus32 == null)
                {
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri("pack://application:,,/Images/Plus32.png");
                    img.EndInit();
                    _UIIconPlus32 = img;
                }
                return _UIIconPlus32;
            }
        }
        private static BitmapSource _UIIconArrowRight32;
        public static BitmapSource UIIconArrowRight32
        {
            get
            {
                if (_UIIconArrowRight32 == null)
                {
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri("pack://application:,,/Images/ArrowRight32.png");
                    img.EndInit();
                    _UIIconArrowRight32 = img;
                }
                return _UIIconArrowRight32;
            }
        }


        public static Polygon ShapeTriangleSmallDown
        {
            get
            {
                PointCollection points = new PointCollection();
                points.Add(new System.Windows.Point(0, 0));
                points.Add(new System.Windows.Point(3, 3));
                points.Add(new System.Windows.Point(6, 0));
                Polygon result = new Polygon()
                {
                    Points = points,
                    Fill = new SolidColorBrush(Colors.Black),
                };
                return result;
            }
        }
        public static Path PathArrowSmallDown
        {
            get
            {
                Geometry geo = Geometry.Parse("M 0,0 L 3,3 L 6,0");
                Path result = new Path()
                {
                    Data = geo,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1,
                };
                return result;
            }
        }
        public static Path PathArrowSmallRight
        {
            get
            {
                Geometry geo = Geometry.Parse("M 0,0 L 3,3 L 0,6");
                Path result = new Path()
                {
                    Data = geo,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1,
                };
                return result;
            }
        }

        public static Path NewPathCross100
        {
            get
            {
                PathFigure pthFigure = new PathFigure();
                pthFigure.StartPoint = new System.Windows.Point(0, 0);

                System.Windows.Point Point2 = new System.Windows.Point(100, 100);
                System.Windows.Point Point3 = new System.Windows.Point(0, 100);
                System.Windows.Point Point4 = new System.Windows.Point(100, 0);

                PolyLineSegment plineSeg = new PolyLineSegment();
                plineSeg.Points.Add(Point2);
                plineSeg.Points.Add(Point3);
                plineSeg.Points.Add(Point4);

                PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
                myPathSegmentCollection.Add(plineSeg);

                pthFigure.Segments = myPathSegmentCollection;

                PathFigureCollection pthFigureCollection = new PathFigureCollection();
                pthFigureCollection.Add(pthFigure);

                PathGeometry pthGeometry = new PathGeometry();
                pthGeometry.Figures = pthFigureCollection;

                Path _PathCross100 = new Path()
                {
                    Stroke = new SolidColorBrush(Colors.Gray),
                    StrokeThickness = 1,
                    Data = pthGeometry,
                    //Fill = new SolidColorBrush(Colors.Yellow),
                };

                return _PathCross100;
            }
        }
    }
}
