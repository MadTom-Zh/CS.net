using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MouseClickAnalysis
{
    /// <summary>
    /// UserControl_Chart.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_Chart : UserControl
    {
        public static int UI_HEIGHT = 150;

        public UserControl_Chart()
        {
            InitializeComponent();
        }

        List<Class_MCDataAnalysts.CountDataItem> _dataList;
        public List<Class_MCDataAnalysts.CountDataItem> DataList
        {
            set
            {
                _dataList = value;
                ReDraw();
            }
        }

        private void ReDraw()
        {
            if (this.IsLoaded == false)
            {
                return;
            }
            ClearGs();
            if (_dataList != null)
            {
                ReCollectBaseInfo();
                Drag_Ruler();
                Drag_BarsAndTitle();
            }
        }

        private void ClearGs()
        {
            gridBarsAndTitle.Children.Clear();
            gridRuler.Children.Clear();
        }

        private double width_BarsArea;
        private double width_Bar;
        private double height_BarsArea;
        private double height_Bar_value1;

        private double width_OneMark;

        private double averageMDCountValue;

        private double leftOffSet_Ruler;
        private double width_Ruler_MarkBig;
        private double width_Ruler_MarkSmall;
        private double height_Ruler_MarkBig;
        private double height_Ruler_MarkSmall;

        private int countRulerMarkMax = 1000;

        private int fontSize_RulerLabel = 9;
        private int fontSize_TitleLabel = 13;

        // 重新收集画图区域尺寸信息
        private void ReCollectBaseInfo()
        {
            width_BarsArea = gridBarsAndTitle.ActualWidth;

            width_OneMark = width_BarsArea / countRulerMarkMax;

            width_Bar = width_OneMark * 1.4;
            height_BarsArea = gridBarsAndTitle.ActualHeight;



            averageMDCountValue = 0;
            foreach (Class_MCDataAnalysts.CountDataItem cdItem in _dataList)
            {
                averageMDCountValue += cdItem.MBDownCount;
            }
            averageMDCountValue /= 1.0 * _dataList.Count;

            height_Bar_value1 = height_BarsArea / averageMDCountValue;

            leftOffSet_Ruler = gridRuler.ActualWidth - gridBarsAndTitle.ActualWidth;
            if (leftOffSet_Ruler <= 0)
            {
                throw new Exception("Ruler Area is Not Wide Enough!");
            }
            width_Ruler_MarkBig = width_OneMark * 0.8;
            width_Ruler_MarkSmall = width_OneMark * 0.5;
            height_Ruler_MarkBig = gridRuler.ActualHeight - 1 - fontSize_RulerLabel;
            height_Ruler_MarkSmall = height_Ruler_MarkBig / 2;


        }

        public static Color Color_Ruler_MarkBar = Colors.DarkRed;
        private void Drag_Ruler()
        {
            int interval_Small = 10;
            int interval_Big = 0;

            while (interval_Big < (fontSize_RulerLabel * 5) || ((interval_Big / interval_Small) % 5) != 0)
            {
                interval_Big++;
            }

            Rectangle markBar_Small;
            for (int dragMaxCount = countRulerMarkMax / interval_Small; dragMaxCount >= 0; dragMaxCount--)
            {
                // drag small bars
                markBar_Small = new Rectangle();
                gridRuler.Children.Add(markBar_Small);
                markBar_Small.Width = width_Ruler_MarkSmall;
                markBar_Small.Height = height_Ruler_MarkSmall;

                markBar_Small.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                markBar_Small.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

                markBar_Small.Fill = new SolidColorBrush(Color_Ruler_MarkBar);

                markBar_Small.Margin = new Thickness(
                    width_OneMark * dragMaxCount * interval_Small - (width_Ruler_MarkSmall - width_OneMark) / 2 + leftOffSet_Ruler,
                    0, 0, 0);
            }

            Rectangle markBar_Big;
            Label labelMark;
            for (int dragMaxCount = countRulerMarkMax / interval_Big; dragMaxCount >= 0; dragMaxCount--)
            {
                // drag big bars, labels
                markBar_Big = new Rectangle();
                gridRuler.Children.Add(markBar_Big);
                markBar_Big.Width = width_Ruler_MarkBig;
                markBar_Big.Height = height_Ruler_MarkBig;

                markBar_Big.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                markBar_Big.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

                markBar_Big.Fill = new SolidColorBrush(Color_Ruler_MarkBar);

                markBar_Big.Margin = new Thickness(
                    width_OneMark * dragMaxCount * interval_Big - (width_Ruler_MarkBig - width_OneMark) / 2 + leftOffSet_Ruler,
                    0, 0, 0);


                labelMark = new Label();
                gridRuler.Children.Add(labelMark);

                labelMark.FontSize = fontSize_RulerLabel;
                labelMark.Content = (dragMaxCount * interval_Big).ToString("###,##0");

                labelMark.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                labelMark.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

                labelMark.Margin = new Thickness(
                    width_OneMark * dragMaxCount * interval_Big + width_Ruler_MarkBig - (width_Ruler_MarkBig - width_OneMark) / 2 + leftOffSet_Ruler,
                    0, 0, 0);
            }

            // mouse button label

            Label labelTitle = new Label();
            gridBarsAndTitle.Children.Add(labelTitle);

            labelTitle.FontSize = fontSize_TitleLabel;
            switch (_dataList[0].mBtn)
            {
                case MouseButton.Left:
                    labelTitle.Content = "左";
                    break;
                case MouseButton.Middle:
                    labelTitle.Content = "中";
                    break;
                case MouseButton.Right:
                    labelTitle.Content = "右";
                    break;
                case MouseButton.XButton1:
                    labelTitle.Content = "X1";
                    break;
                case MouseButton.XButton2:
                    labelTitle.Content = "X2";
                    break;
            }

            labelTitle.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            labelTitle.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            labelTitle.Margin = new Thickness(0, 0, 0, 0);
        }
        public static Color Color_BarValue_MultiDot2 = Colors.Blue;
        public static Color Color_BarValue_Multi1 = Colors.Green;
        public static Color Color_BarValue_Multi5 = Colors.Red;
        private void Drag_BarsAndTitle()
        {
            Rectangle bar;
            Class_MCDataAnalysts.CountDataItem cdItem;
            for (int i = 0; i < _dataList.Count; i++)
            {
                cdItem = _dataList[i];
                bar = new Rectangle();
                gridBarsAndTitle.Children.Add(bar);

                #region set bar size, position, color
                bar.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                bar.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

                bar.Width = width_Bar;
                if (cdItem.MBDownCount > averageMDCountValue)
                {
                    bar.Height = cdItem.MBDownCount * 0.2 * height_Bar_value1;
                    bar.Fill = new SolidColorBrush(Color_BarValue_MultiDot2);
                }
                else if (cdItem.MBDownCount * 5 < averageMDCountValue)
                {
                    bar.Height = cdItem.MBDownCount * 5 * height_Bar_value1;
                    bar.Fill = new SolidColorBrush(Color_BarValue_Multi5);
                }
                else
                {
                    bar.Height = cdItem.MBDownCount * height_Bar_value1;
                    bar.Fill = new SolidColorBrush(Color_BarValue_Multi1);
                }

                bar.Margin = new Thickness(width_OneMark * cdItem.timeLengthMS - (width_Bar - width_OneMark) / 2,
                    0, 0, 0);
                #endregion
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReDraw();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ReDraw();
        }
    }
}
