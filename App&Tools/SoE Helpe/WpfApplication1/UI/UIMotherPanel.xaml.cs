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

namespace WpfApplication1.UI
{
    /// <summary>
    /// UIMotherPanel.xaml 的交互逻辑
    /// </summary>
    public partial class UIMotherPanel : UserControl
    {
        public UIMotherPanel()
        {
            InitializeComponent();
        }
        private void MainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            ReInit(4, 4);
        }

        private ClassSoEMatrix _Mtx;
        public ClassSoEMatrix Mtx
        {
            get
            {
                return _Mtx;
            }
        }

        public void ReInit(int cols, int rows)
        {
            _Mtx = new ClassSoEMatrix(cols, rows);
            ReDraw();
        }

        public void ReDraw()
        {
            int cols = _Mtx.Width;
            int rows = _Mtx.Height;

            MainGrid.Children.Clear();
            MainGrid.Width = cols * UIBlockItem.BlockWidth;
            MainGrid.Height = rows * UIBlockItem.BlockWidth;

            Line line;
            SolidColorBrush br = new SolidColorBrush(Colors.LightPink);
            for (int i = 1; i < cols; i++)
            {
                line = new Line();
                line.X1 = i * UIBlockItem.BlockWidth;
                line.Y1 = 0;
                line.X2 = i * UIBlockItem.BlockWidth;
                line.Y2 = rows * UIBlockItem.BlockWidth;
                line.Stroke = br;
                line.StrokeThickness = 2;
                line.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                line.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                MainGrid.Children.Add(line);
            }
            for (int i = 1; i < rows; i++)
            {
                line = new Line();
                line.X1 = 0;
                line.Y1 = i * UIBlockItem.BlockWidth;
                line.X2 = cols * UIBlockItem.BlockWidth;
                line.Y2 = i * UIBlockItem.BlockWidth;
                line.Stroke = br;
                line.StrokeThickness = 2;
                line.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                line.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                MainGrid.Children.Add(line);
            }
        }

        public void SetItems(List<ClassJigsaw.PieceInfo> items)
        {
            UIBlockItem ui;
            foreach(ClassJigsaw.PieceInfo item in items)
            {
                ui = new UIBlockItem();
                ui.Mtx = item.item;
                ui.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                ui.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                ui.Margin = new Thickness(item.startPoint.X * UIBlockItem.BlockWidth,
                    item.startPoint.Y * UIBlockItem.BlockWidth,
                    0, 0);
                MainGrid.Children.Add(ui);
            }
        }
    }
}
