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


namespace FlowerzHelper.Ctrls
{
    /// <summary>
    /// GardenPanel.xaml 的交互逻辑
    /// </summary>
    public partial class GardenPanel : UserControl
    {
        public GardenPanel()
        {
            InitializeComponent();
        }

        private Looper.Item[][] _dataMatrix = null;
        public Looper.Item[][] dataMatrix
        {
            get
            {
                return _dataMatrix;
            }
            set
            {
                _dataMatrix = value;
                GrdSize = new Looper.IntSize(_dataMatrix[0].Length, _dataMatrix.Length);

                DragGrid();
                DragBlocks();
            }
        }

        private Point startPoint;
        private Point endPoint;
        private double stepLength;

        private Looper.IntSize _grdSize;
        private Looper.IntSize GrdSize
        {
            set
            {
                _grdSize = value;
                //_looper.gardenSize = _grdSize;

                if (_grdSize.Width > _grdSize.Height)
                {
                    stepLength = this.Width / _grdSize.Width;
                    startPoint = new Point(0.0, (this.Height - stepLength * _grdSize.Height) / 2);
                    endPoint = new Point(this.Width, (this.Height + stepLength * _grdSize.Height) / 2);
                }
                else if (_grdSize.Width == _grdSize.Height)
                {
                    stepLength = this.Width / _grdSize.Width;
                    endPoint = new Point(0, 0);
                    endPoint = new Point(this.Width, this.Height);
                }
                else
                {
                    stepLength = this.Width / _grdSize.Height;
                    startPoint = new Point((this.Width - stepLength * _grdSize.Width) / 2, 0.0);
                    endPoint = new Point((this.Width + stepLength * _grdSize.Width) / 2, this.Height);
                }

                DragGrid();
                DragBlocks();
            }
            get
            {
                return _grdSize;
            }
        }

        private List<Line> gridLines = new List<Line>();
        public void DragGrid()
        {
            for (int i = gridLines.Count - 1; i >= 0; i--)
            {
                gridWindow.Children.Remove(gridLines[i]);
                gridLines.RemoveAt(i);
            }

            Line newLint;
            double newLine_StrokeThickness1 = 1;
            double newLine_StrokeThickness4 = 4;
            Brush bs_gray = new SolidColorBrush(Colors.Gray);
            Brush bs_red = new SolidColorBrush(Colors.Red);

            // inner frames
            for (int i = 1; i < _grdSize.Height; i++)
            {
                newLint = new Line();
                newLint.StrokeThickness = newLine_StrokeThickness1;
                newLint.Stroke = bs_gray;
                newLint.X1 = startPoint.X;
                newLint.Y1 = startPoint.Y + i * stepLength;
                newLint.X2 = endPoint.X;
                newLint.Y2 = newLint.Y1;
                gridLines.Add(newLint);
                gridWindow.Children.Add(newLint);
            }
            for (int i = 1; i < _grdSize.Width; i++)
            {
                newLint = new Line();
                newLint.StrokeThickness = newLine_StrokeThickness1;
                newLint.Stroke = bs_gray;
                newLint.X1 = startPoint.X + i * stepLength;
                newLint.Y1 = startPoint.Y;
                newLint.X2 = newLint.X1;
                newLint.Y2 = endPoint.Y;
                gridLines.Add(newLint);
                gridWindow.Children.Add(newLint);
            }

            // outer frame
            newLint = new Line();
            newLint.StrokeThickness = newLine_StrokeThickness4;
            newLint.Stroke = bs_red;
            newLint.X1 = startPoint.X;
            newLint.Y1 = startPoint.Y;
            newLint.X2 = endPoint.X;
            newLint.Y2 = startPoint.Y;
            gridLines.Add(newLint);
            gridWindow.Children.Add(newLint);

            newLint = new Line();
            newLint.StrokeThickness = newLine_StrokeThickness4;
            newLint.Stroke = bs_red;
            newLint.X1 = endPoint.X;
            newLint.Y1 = startPoint.Y;
            newLint.X2 = endPoint.X;
            newLint.Y2 = endPoint.Y;
            gridLines.Add(newLint);
            gridWindow.Children.Add(newLint);

            newLint = new Line();
            newLint.StrokeThickness = newLine_StrokeThickness4;
            newLint.Stroke = bs_red;
            newLint.X1 = endPoint.X;
            newLint.Y1 = endPoint.Y;
            newLint.X2 = startPoint.X;
            newLint.Y2 = endPoint.Y;
            gridLines.Add(newLint);
            gridWindow.Children.Add(newLint);

            newLint = new Line();
            newLint.StrokeThickness = newLine_StrokeThickness4;
            newLint.Stroke = bs_red;
            newLint.X1 = startPoint.X;
            newLint.Y1 = endPoint.Y;
            newLint.X2 = startPoint.X;
            newLint.Y2 = startPoint.Y;
            gridLines.Add(newLint);
            gridWindow.Children.Add(newLint);

        }

        private List<SubBlock.FlowerBlock> gridBlocks = new List<SubBlock.FlowerBlock>();
        public void DragBlocks()
        {
            for (int i = gridBlocks.Count - 1; i >= 0; i--)
            {
                gridWindow.Children.Remove(gridBlocks[i]);
                gridBlocks.RemoveAt(i);
            }

            Looper.Item item;
            SubBlock.FlowerBlock ui;
            for (int i = _dataMatrix.Length - 1; i >= 0; i--)
            {
                for (int j = _dataMatrix[i].Length - 1; j >= 0; j--)
                {
                    item = _dataMatrix[i][j];
                    if (item != null)
                    {
                        ui = CreateBlock(item, j, i);
                        gridBlocks.Add(ui);
                        gridWindow.Children.Add(ui);
                    }
                }
            }
        }
        private SubBlock.FlowerBlock CreateBlock(Looper.Item itemData, int x, int y)
        {
            SubBlock.FlowerBlock result = new SubBlock.FlowerBlock();
            result.itemData = itemData;
            itemData.UIFlower = result;
            result.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            result.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            double scall = stepLength / result.Width;
            result.RenderTransform
                = new ScaleTransform(scall, scall);

            result.Margin = new Thickness(startPoint.X + x * stepLength, startPoint.Y + y * stepLength, 0, 0);
            result.UIRefresh();

            return result;
        }

        private bool canUserCtrl = true;
        public bool CanUserCtrl
        {
            get
            {
                return canUserCtrl;
            }
            set
            {
                canUserCtrl = value;
                if (canUserCtrl == true)
                {
                    gridWindow.Opacity = 1;
                }
                else gridWindow.Opacity = 0.6;
            }
        }

        #region mouse position
        private bool isMouseOver = false;
        private Point mousePosi;
        private void gridWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            if (canUserCtrl == true && _dataMatrix != null)
            {
                isMouseOver = true;
                gridWindow.Focus();
            }
        }
        private void gridWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            isMouseOver = false;
        }
        private void gridWindow_MouseMove(object sender, MouseEventArgs e)
        {
            mousePosi = e.GetPosition(gridWindow);
        }
        #endregion

        private void gridWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // ` [ ]
            //MessageBox.Show(e.Key.ToString());


            if (canUserCtrl == false)
            {
                return;
            }
            if (isMouseOver == false)
            {
                return;
            }
            if (_dataMatrix == null)
            {
                return;
            }

            //bool ctrlDowned = ((Control.ModifierKeys & Keys.Control) == Keys.Control);
            bool ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            switch (e.Key)
            {
                case Key.Oem3:
                case Key.D0:
                case Key.NumPad0:
                    SetBlock_stone(GetPosiByMousePosi());
                    break;
                case Key.D1:
                case Key.NumPad1:
                    if (ctrlDown == true) SetBlock_flowerSubColor(Looper.Item.COLOR_RED, GetPosiByMousePosi());
                    else SetBlock_flowerMainColor(Looper.Item.COLOR_RED, GetPosiByMousePosi());
                    break;
                case Key.D2:
                case Key.NumPad2:
                    if (ctrlDown == true) SetBlock_flowerSubColor(Looper.Item.COLOR_MAGENTA, GetPosiByMousePosi());
                    else SetBlock_flowerMainColor(Looper.Item.COLOR_MAGENTA, GetPosiByMousePosi());
                    break;
                case Key.D3:
                case Key.NumPad3:
                    if (ctrlDown == true) SetBlock_flowerSubColor(Looper.Item.COLOR_YELLOW, GetPosiByMousePosi());
                    else SetBlock_flowerMainColor(Looper.Item.COLOR_YELLOW, GetPosiByMousePosi());
                    break;
                case Key.D4:
                case Key.NumPad4:
                    if (ctrlDown == true) SetBlock_flowerSubColor(Looper.Item.COLOR_WHITE, GetPosiByMousePosi());
                    else SetBlock_flowerMainColor(Looper.Item.COLOR_WHITE, GetPosiByMousePosi());
                    break;
                case Key.D5:
                case Key.NumPad5:
                    if (ctrlDown == true) SetBlock_flowerSubColor(Looper.Item.COLOR_CYAN, GetPosiByMousePosi());
                    else SetBlock_flowerMainColor(Looper.Item.COLOR_CYAN, GetPosiByMousePosi());
                    break;
                case Key.D6:
                case Key.NumPad6:
                    if (ctrlDown == true) SetBlock_flowerSubColor(Looper.Item.COLOR_BLUE, GetPosiByMousePosi());
                    else SetBlock_flowerMainColor(Looper.Item.COLOR_BLUE, GetPosiByMousePosi());
                    break;
                case Key.Delete:
                case Key.Back:
                    SetBlock_singleOrGround(GetPosiByMousePosi());
                    break;
                case Key.Oem4:
                    if (ctrlDown == true) SetBlock_flowerSubColor(false, GetPosiByMousePosi());
                    else SetBlock_flowerMainColor(false, GetPosiByMousePosi());
                    break;
                case Key.Oem6:
                    if (ctrlDown == true) SetBlock_flowerSubColor(true, GetPosiByMousePosi());
                    else SetBlock_flowerMainColor(true, GetPosiByMousePosi());
                    break;
            }



            e.Handled = true;
        }

        private Looper.IntPoint GetPosiByMousePosi()
        {
            return new Looper.IntPoint(
                (int)((mousePosi.X - startPoint.X) / stepLength),
                (int)((mousePosi.Y - startPoint.Y) / stepLength)
                );
        }
        private void SetBlock_stone(Looper.IntPoint posi)
        {
            Looper.Item item = _dataMatrix[posi.Y][posi.X];
            if (item == null)
            {
                item = new Looper.Item(Looper.Item.Type.stone);
                SubBlock.FlowerBlock ui = CreateBlock(item, posi.X, posi.Y);
                _dataMatrix[posi.Y][posi.X] = item;
                gridBlocks.Add(ui);
                gridWindow.Children.Add(ui);
            }
            else
            {
                item.myType = Looper.Item.Type.stone;
                item.UIFlower.UIRefresh();
            }
        }
        private void SetBlock_flowerMainColor(Color mainColor, Looper.IntPoint posi)
        {
            Looper.Item item = _dataMatrix[posi.Y][posi.X];
            if (item == null)
            {
                item = new Looper.Item(Looper.Item.Type.flowerSingle);
                item.flowerMainColor = mainColor;
                SubBlock.FlowerBlock ui = CreateBlock(item, posi.X, posi.Y);
                _dataMatrix[posi.Y][posi.X] = item;
                gridBlocks.Add(ui);
                gridWindow.Children.Add(ui);
            }
            else
            {
                if (item.myType != Looper.Item.Type.flowerSingle && item.myType != Looper.Item.Type.flowerDouble)
                {
                    item.myType = Looper.Item.Type.flowerSingle;
                }
                item.flowerMainColor = mainColor;
                item.UIFlower.UIRefresh();
            }
        }
        private void SetBlock_flowerMainColor(bool toSetNxtColor, Looper.IntPoint posi)
        {
            Looper.Item item = _dataMatrix[posi.Y][posi.X];
            if (item != null &&
                (item.myType == Looper.Item.Type.flowerSingle || item.myType == Looper.Item.Type.flowerDouble))
            {
                if (toSetNxtColor == true)
                    SetBlock_flowerMainColor(Looper.Item.GetNxtColor(item.flowerMainColor), posi);
                else
                    SetBlock_flowerMainColor(Looper.Item.GetPrvColor(item.flowerMainColor), posi);
            }
        }
        private void SetBlock_flowerSubColor(Color subColor, Looper.IntPoint posi)
        {
            Looper.Item item = _dataMatrix[posi.Y][posi.X];
            if (item != null)
            {
                if (item.myType == Looper.Item.Type.flowerSingle || item.myType == Looper.Item.Type.flowerDouble)
                {
                    item.myType = Looper.Item.Type.flowerDouble;
                    item.flowerSubColor = subColor;
                    item.UIFlower.UIRefresh();
                }
            }
        }
        private void SetBlock_flowerSubColor(bool toSetNxtColor, Looper.IntPoint posi)
        {
            Looper.Item item = _dataMatrix[posi.Y][posi.X];
            if (item != null && item.myType == Looper.Item.Type.flowerDouble)
            {
                if (toSetNxtColor == true)
                    SetBlock_flowerSubColor(Looper.Item.GetNxtColor(item.flowerSubColor), posi);
                else
                    SetBlock_flowerSubColor(Looper.Item.GetPrvColor(item.flowerSubColor), posi);
            }
        }
        private void SetBlock_singleOrGround(Looper.IntPoint posi)
        {
            Looper.Item item = _dataMatrix[posi.Y][posi.X];
            if (item != null)
            {
                if (item.myType == Looper.Item.Type.flowerDouble)
                {
                    item.myType = Looper.Item.Type.flowerSingle;
                    item.UIFlower.UIRefresh();
                }
                else
                {
                    gridWindow.Children.Remove(item.UIFlower);
                    gridBlocks.Remove(item.UIFlower);
                    _dataMatrix[posi.Y][posi.X] = null;
                }
            }
        }

        public Looper.IntPoint PutPoint;
        public event EventHandler CallPut;
        private void gridWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CallPut!= null)
            {
                PutPoint = GetPosiByMousePosi();
                CallPut(this, null);
            }
        }

        private List<Line> markLines = new List<Line>();
        public void SetXMark(Looper.IntPoint point)
        {
            Line newLint;
            double newLine_StrokeThickness7 = 7;
            Brush bs_darkOrange = new SolidColorBrush(Colors.DarkOrange);

            newLint = new Line();
            newLint.StrokeThickness = newLine_StrokeThickness7;
            newLint.Stroke = bs_darkOrange;
            newLint.X1 = startPoint.X + point.X * stepLength;
            newLint.Y1 = startPoint.Y + point.Y * stepLength;
            newLint.X2 = startPoint.X + (point.X + 1) * stepLength;
            newLint.Y2 = startPoint.Y + (point.Y + 1) * stepLength;
            markLines.Add(newLint);
            gridWindow.Children.Add(newLint);

            newLint = new Line();
            newLint.StrokeThickness = newLine_StrokeThickness7;
            newLint.Stroke = bs_darkOrange;
            newLint.X1 = startPoint.X + (point.X + 1) * stepLength;
            newLint.Y1 = startPoint.Y + point.Y * stepLength;
            newLint.X2 = startPoint.X + point.X * stepLength;
            newLint.Y2 = startPoint.Y + (point.Y + 1) * stepLength;
            markLines.Add(newLint);
            gridWindow.Children.Add(newLint);
        }
        public void DelXMarks()
        {
            for (int i = markLines.Count - 1; i >= 0; i--)
            {
                gridWindow.Children.Remove(markLines[i]);
                markLines.RemoveAt(i);
            }
        }

    }
}
