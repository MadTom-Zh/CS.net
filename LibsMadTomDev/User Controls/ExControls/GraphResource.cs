using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MadTomDev.UI
{
    public class GraphResource
    {
        // Because Polygon inherits from FrameworkElement, you can not use it twice. 
        //private static Polygon ShapeTriangleSmallDown
        public static Polygon ShapeTriangleSmallDown
        {
            get
            {
                PointCollection points = new PointCollection();
                points.Add(new System.Windows.Point(0, 0));
                points.Add(new System.Windows.Point(3, 3));
                points.Add(new System.Windows.Point(6, 0));
                return new Polygon()
                {
                    Points = points,
                    Fill = new SolidColorBrush(Colors.Black),
                };
            }
        }

        public static Path PathArrowSmallDown
        {
            get
            {
                Geometry geo = Geometry.Parse("M 0,0 L 3,3 L 6,0");
                return new Path()
                {
                    Data = geo,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1,
                };
            }
        }

        public static Path PathArrowSmallRight
        {
            get
            {
                Geometry geo = Geometry.Parse("M 0,0 L 3,3 L 0,6");
                return new Path()
                {
                    Data = geo,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1,
                };
            }
        }
        public static Path PathArrowSmallDoubleRight
        {
            get
            {
                Geometry geo = Geometry.Parse("M 0,0 L 3,3 L 0,6 M 2,0 L 5,3 L 2,6");
                return new Path()
                {
                    Data = geo,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1,
                };
            }
        }

        private static Common.IconHelper.Shell32Icons icon32 = null;
        public static BitmapSource UIIconRefresh16
        {
            get
            {
                if (icon32 == null)
                    icon32 = Common.IconHelper.Shell32Icons.Instance;
                return icon32.GetIcon(238, false);
            }
        }
        public static BitmapSource UIIconGoto16
        {
            get
            {
                if (icon32 == null)
                    icon32 = Common.IconHelper.Shell32Icons.Instance;
                return icon32.GetIcon(299, false);
            }
        }
    }
}
