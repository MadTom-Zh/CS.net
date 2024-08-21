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

namespace DroplitzHelper.UCs
{
    /// <summary>
    /// UC_Switch.xaml 的交互逻辑
    /// </summary>
    public partial class UC_Switch : UserControl
    {
        public UC_Switch()
        {
            InitializeComponent();
            Height = StaticHeight;
            Width = StaticWidth;
        }
        public static double StaticHeight = 80.0;
        public static double StaticWidth = 80.0;

        public Classes.Switch6Ways Switch6WData = null;
        public static Color Color_Locked = Colors.Black;
        public static Color Color_Blank = Colors.LightGreen;
        public static Color Color_Normal = Colors.LightBlue;
        public static Color Color_InTheWay_EndPoint = Colors.Red;
        public static Color Color_InTheWay_Pri = Colors.Orange;
        public static Color Color_InTheWay_Sec = Colors.Yellow;
        public void ReSetUI_Auto()
        {
            if (Switch6WData == null) return;
            rect1.Visibility = (Switch6WData.Ways[0] == true) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            rect2.Visibility = (Switch6WData.Ways[1] == true) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            rect3.Visibility = (Switch6WData.Ways[2] == true) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            rect4.Visibility = (Switch6WData.Ways[3] == true) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            rect5.Visibility = (Switch6WData.Ways[4] == true) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            rect6.Visibility = (Switch6WData.Ways[5] == true) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            switch (Switch6WData.MyType)
            {
                case Classes.Switch6Ways.Type.START:
                case Classes.Switch6Ways.Type.END:
                    ReSetUI_FrameColor(Color_Locked);
                    break;
                case Classes.Switch6Ways.Type.None:
                    ReSetUI_FrameColor(Color_Blank);
                    break;
                default:
                    ReSetUI_FrameColor(Color_Normal);
                    break;
            }
        }
        public void ReSetUI_FrameColor(Color newColor)
        {
            elli.Stroke = new SolidColorBrush(newColor);
        }
        public void ReSetUI_FrameColor_Normal()
        {
            ReSetUI_Auto();
        }
        public void ReSetUI_FrameColor_InOpenWayPri()
        {
            if(Switch6WData.MyType == Classes.Switch6Ways.Type.START
                || Switch6WData.MyType == Classes.Switch6Ways.Type.END)
            {
                return;
            }
            ReSetUI_FrameColor(Color_InTheWay_Pri);
        }
        public void ReSetUI_FrameColor_InOpenWaySec()
        {
            if (Switch6WData.MyType == Classes.Switch6Ways.Type.START
                || Switch6WData.MyType == Classes.Switch6Ways.Type.END)
            {
                return;
            }
            ReSetUI_FrameColor(Color_InTheWay_Sec);
        }
        public void ReSetUI_FrameColor_EndPoint()
        {
            if (Switch6WData.MyType == Classes.Switch6Ways.Type.START
                || Switch6WData.MyType == Classes.Switch6Ways.Type.END)
            {
                return;
            }
            ReSetUI_FrameColor(Color_InTheWay_EndPoint);
        }
    }
}
