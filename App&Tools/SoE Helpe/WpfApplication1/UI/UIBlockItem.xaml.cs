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
    /// UIBlockItem.xaml 的交互逻辑
    /// </summary>
    public partial class UIBlockItem : UserControl
    {
        public UIBlockItem()
        {
            InitializeComponent();
        }

        private ClassSoEMatrix _Mtx;
        public ClassSoEMatrix Mtx
        {
            get
            {
                return _Mtx;
            }
            set
            {
                _Mtx = value;
                ReDrowItem();
            }
        }

        public enum ItemType
        {
            None,
            // HH
            //  HH
            LeftStairs,
            //  HH
            // HH
            RightStairs,
            //  H
            // HHH
            Pyra,
            // HHH
            // H
            LeftPin,
            // H
            // HHH
            RightPin,
            // HHHH
            Bar,
            // HH
            // HH
            BigBlock,
        }

        public static int BlockWidth = 50;
        private ItemType _MyType = ItemType.None;
        public ItemType MyType
        {
            get
            {
                return _MyType;
            }
        }
        public void Init(ItemType itemType)
        {
            _MyType = itemType;
            switch (itemType)
            {
                case ItemType.LeftStairs:
                    _Mtx = new ClassSoEMatrix(3, 2);
                    _Mtx.Data[0][0]
                        = _Mtx.Data[0][1]
                        = _Mtx.Data[1][1]
                        = _Mtx.Data[1][2]
                        = 1;
                    break;
                case ItemType.RightStairs:
                    _Mtx = new ClassSoEMatrix(3, 2);
                    _Mtx.Data[0][1]
                        = _Mtx.Data[0][2]
                        = _Mtx.Data[1][0]
                        = _Mtx.Data[1][1]
                        = 1;
                    break;
                case ItemType.Pyra:
                    _Mtx = new ClassSoEMatrix(3, 2);
                    _Mtx.Data[0][1]
                        = _Mtx.Data[1][0]
                        = _Mtx.Data[1][1]
                        = _Mtx.Data[1][2]
                        = 1;
                    break;
                case ItemType.LeftPin:
                    _Mtx = new ClassSoEMatrix(3, 2);
                    _Mtx.Data[0][0]
                        = _Mtx.Data[0][1]
                        = _Mtx.Data[0][2]
                        = _Mtx.Data[1][0]
                        = 1;
                    break;
                case ItemType.RightPin:
                    _Mtx = new ClassSoEMatrix(3, 2);
                    _Mtx.Data[0][0]
                        = _Mtx.Data[0][1]
                        = _Mtx.Data[0][2]
                        = _Mtx.Data[1][2]
                        = 1;
                    break;
                case ItemType.Bar:
                    _Mtx = new ClassSoEMatrix(4, 1);
                    _Mtx.Data[0][0]
                        = _Mtx.Data[0][1]
                        = _Mtx.Data[0][2]
                        = _Mtx.Data[0][3]
                        = 1;
                    break;
                case ItemType.BigBlock:
                    _Mtx = new ClassSoEMatrix(2, 2);
                    _Mtx.Data[0][0]
                        = _Mtx.Data[0][1]
                        = _Mtx.Data[1][0]
                        = _Mtx.Data[1][1]
                        = 1;
                    break;
            }
            this.Width = BlockWidth * _Mtx.Data[0].Length;
            this.Height = BlockWidth * _Mtx.Data.Length;
            _Mtx.MxTransposed += Mtx_MxTransposed;
            ReDrowItem();
        }
        void Mtx_MxTransposed(object sender, EventArgs e)
        {
            ReDrowItem();
        }

        // base width/hight 50pix
        private void ReDrowItem()
        {
            MainGrid.Children.Clear();
            Line line;
            SolidColorBrush br = new SolidColorBrush();
            br.Color = Colors.DarkGreen;
            int cols = Mtx.Data[0].Length;
            int rows = Mtx.Data.Length;
            for (int rowIdx = 0; rowIdx < rows; rowIdx++)
            {
                for (int colIdx = 0; colIdx < cols; colIdx++)
                {
                    if (Mtx.Data[rowIdx][colIdx] > 0)
                    {
                        if (colIdx < (cols - 1)
                            && Mtx.Data[rowIdx][colIdx + 1] > 0)
                        {
                            // draw right
                            // BlockWidth = 50
                            line = new Line();
                            line.Stroke = br;
                            line.StrokeThickness = BlockWidth - 5;
                            line.X1 = 2.5 + colIdx * BlockWidth;
                            line.X2 = 95 + colIdx * BlockWidth;
                            line.Y1 = 25 + rowIdx * BlockWidth;
                            line.Y2 = 25 + rowIdx * BlockWidth;
                            line.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                            line.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                            MainGrid.Children.Add(line);
                        }
                        if (rowIdx < (rows - 1)
                            && Mtx.Data[rowIdx + 1][colIdx] > 0)
                        {
                            // draw down
                            line = new Line();
                            line.Stroke = br;
                            line.StrokeThickness = BlockWidth - 5;
                            line.X1 = 25 + colIdx * BlockWidth;
                            line.X2 = 25 + colIdx * BlockWidth;
                            line.Y1 = 2.5 + rowIdx * BlockWidth;
                            line.Y2 = 95 + rowIdx * BlockWidth;
                            line.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                            line.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                            MainGrid.Children.Add(line);
                        }
                    }
                }
            }
        }

        //public bool CanClick = false;
        //public event EventHandler 
    }
}
