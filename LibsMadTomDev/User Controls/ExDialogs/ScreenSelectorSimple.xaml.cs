using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MadTomDev.UI
{
    /// <summary>
    /// Interaction logic for ScreenSelectorSimple.xaml
    /// </summary>
    public partial class ScreenSelectorSimple : Window
    {
        /// <summary>
        /// 
        /// </summary>
        public ScreenSelectorSimple()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 主屏幕的左上角为(0,0)，需要默认选择的位置，请按所有屏幕的全图的位置来设定；
        /// 如主屏左侧为附屏，设定附屏的窗口位置时，其x坐标必定为负值；
        /// </summary>
        /// <param name="defaultSelectArea"></param>
        /// <returns></returns>
        public bool? ShowDialog(Int32Rect defaultSelectArea)
        {
            SetSelectedArea(defaultSelectArea);
            return base.ShowDialog();
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            img_bg.Source = QuickGraphics.Screen.GetAllInOneScreenShot(out System.Drawing.Rectangle aioBounds);
            this.Left = aioBounds.X;
            this.Top = aioBounds.Y;
            this.Width = aioBounds.Width;
            this.Height = aioBounds.Height;
            ShowSelectedArea();
        }

        public void SetSelectedArea(Int32Rect area)
        {
            selectedArea = area;
            if (area != Int32Rect.Empty)
            {
                stateCode = 1;
            }
        }
        private void ShowSelectedArea()
        {
            if (selectedArea.Width < minimumLength
                || selectedArea.Height < minimumLength)
            {
                selectedArea = Int32Rect.Empty;
            }

            if (selectedArea == Int32Rect.Empty)
            {
                // not selected
                rect_grayTop.Margin = new Thickness(0);
                rect_grayLeft.Visibility = Visibility.Collapsed;
                rect_grayRight.Visibility = Visibility.Collapsed;
                rect_grayBottom.Visibility = Visibility.Collapsed;
                SetHandlesVisible(false);
            }
            else
            {
                double areaBtmToBtm = this.ActualHeight - selectedArea.Y - selectedArea.Height;
                rect_grayTop.Margin = new Thickness(
                    0,
                    0,
                    0,
                    this.ActualHeight - selectedArea.Y);
                rect_grayLeft.Margin = new Thickness(
                    0,
                    selectedArea.Y,
                    this.ActualWidth - selectedArea.X,
                    areaBtmToBtm);
                rect_grayRight.Margin = new Thickness(
                    selectedArea.X + selectedArea.Width,
                    selectedArea.Y,
                    0,
                    areaBtmToBtm);
                rect_grayBottom.Margin = new Thickness(
                    0,
                    selectedArea.Y + selectedArea.Height,
                    0,
                    0);

                rect_grayLeft.Visibility = Visibility.Visible;
                rect_grayRight.Visibility = Visibility.Visible;
                rect_grayBottom.Visibility = Visibility.Visible;

                double topPosi = selectedArea.Y - bdr_handleTopLeft.ActualHeight - 2;
                double leftPosi = selectedArea.X - bdr_handleTopLeft.ActualWidth - 2;
                double hCenterPosi = selectedArea.X + selectedArea.Width / 2 - bdr_handleTopCenter.ActualWidth / 2;
                double rightPosi = selectedArea.X + selectedArea.Width + 2;
                double vCenterPosi = selectedArea.Y + selectedArea.Height / 2 - bdr_handleMidLeft.ActualHeight / 2;
                double btmPosi = selectedArea.Y + selectedArea.Height + 2;
                bdr_handleTopLeft.Margin = new Thickness(leftPosi, selectedArea.Y - 8, 0, 0);
                bdr_handleTopCenter.Margin = new Thickness(hCenterPosi, topPosi, 0, 0);
                bdr_handleTopRight.Margin = new Thickness(rightPosi, topPosi, 0, 0);
                bdr_handleMidLeft.Margin = new Thickness(leftPosi, vCenterPosi, 0, 0);
                bdr_handleMidRight.Margin = new Thickness(rightPosi, vCenterPosi, 0, 0);
                bdr_handleBottomLeft.Margin = new Thickness(leftPosi, btmPosi, 0, 0);
                bdr_handleBottomCenter.Margin = new Thickness(hCenterPosi, btmPosi, 0, 0);
                bdr_handleBottomRight.Margin = new Thickness(rightPosi, btmPosi, 0, 0);

                SetHandlesVisible(true);
            }
        }
        private void SetHandlesVisible(bool visible)
        {
            Visibility value = visible ? Visibility.Visible : Visibility.Collapsed;

            bdr_handleTopLeft.Visibility = value;
            bdr_handleTopCenter.Visibility = value;
            bdr_handleTopRight.Visibility = value;
            bdr_handleMidLeft.Visibility = value;
            bdr_handleMidRight.Visibility = value;
            bdr_handleBottomLeft.Visibility = value;
            bdr_handleBottomCenter.Visibility = value;
            bdr_handleBottomRight.Visibility = value;
        }

        private int minimumLength = 3;
        private Int32Rect selectedArea = Int32Rect.Empty;

        /// <summary>
        /// 用于决定用户操作产生的效果
        /// 0 - 用户通过鼠标拖拽，确定初始选区；或退出选择；
        /// 1 - 选区已经确定，用户移动选区，或调整大小；返回上一状态，或确认选择；
        /// </summary>
        private int stateCode = 0;
        /// <summary>
        /// 调整选区大小时，确定用户按住的是哪个手柄
        /// -1 - no effect area
        /// 0 - top left     1 - top center     2 - top right
        /// 3 - middle left                     4 - middle right
        /// 5 - bottom left  6 - bottom center  7 - bottom right
        /// 8 - selection area
        /// </summary>
        private int selectedHandleIndex;

        private bool isAreaMouseLBDown = false;
        private DateTime areaMouseLBDownTime = DateTime.MinValue;
        private Point areaMouseDownPoint;
        private Int32Rect areaMouseDownSelectedRect;
        private void rect_touchArea_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            areaMouseDownPoint = Mouse.GetPosition(this);
            switch (stateCode)
            {
                case 0:
                    if (e.ChangedButton == MouseButton.Right)
                    {
                        this.DialogResult = false;
                        this.Close();
                        return;
                    }
                    else
                    {
                        isAreaMouseLBDown = true;
                        areaMouseLBDownTime = DateTime.Now;
                    }
                    break;
                case 1:
                    if (e.ChangedButton == MouseButton.Right)
                    {
                        selectedArea = Int32Rect.Empty;
                        ShowSelectedArea();
                        stateCode = 0;
                        areaMouseLBDownTime = DateTime.MinValue;
                    }
                    else
                    {
                        DateTime now = DateTime.Now;
                        if (selectedHandleIndex == 8
                            && (now - areaMouseLBDownTime).TotalMilliseconds <= DoubleClickTime)
                        {
                            this.DialogResult = true;
                            this.Close();
                            return;
                        }
                        isAreaMouseLBDown = true;
                        areaMouseLBDownTime = DateTime.Now;
                        areaMouseDownSelectedRect = selectedArea;
                    }
                    break;
            }
        }
        [DllImport("user32.dll")]
        static extern uint GetDoubleClickTime();
        private uint _DoubleClickTime = 0;
        private uint DoubleClickTime
        {
            get
            {
                if (_DoubleClickTime == 0)
                {
                    _DoubleClickTime = GetDoubleClickTime();
                }
                return _DoubleClickTime;
            }
        }


        public bool CheckMouseInSelectArea(Point mousePt)
        {
            if (mousePt.X >= selectedArea.X
                && mousePt.Y >= selectedArea.Y)
            {
                if (mousePt.X - selectedArea.X <= selectedArea.Width
                    && mousePt.Y - selectedArea.Y <= selectedArea.Height)
                {
                    return true;
                }
            }
            return false;
        }
        public bool CheckMouseInHandle(Border handle, Point mousePt)
        {
            Thickness handleMargin = handle.Margin;
            if (mousePt.X >= handleMargin.Left
                && mousePt.Y >= handleMargin.Top)
            {
                if (mousePt.X - handleMargin.Left <= handle.ActualWidth
                    && mousePt.Y - handleMargin.Top <= handle.ActualHeight)
                {
                    return true;
                }
            }
            return false;
        }

        private void rect_touchArea_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (isAreaMouseLBDown)
            {
                if (stateCode == 0)
                {
                    stateCode = 1;
                }
            }
            isAreaMouseLBDown = false;
        }
        private void rect_touchArea_MouseLeave(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            isAreaMouseLBDown = false;
        }

        private void rect_touchArea_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            Point curPt = Mouse.GetPosition(this);
            switch (stateCode)
            {
                case 0:
                    if (isAreaMouseLBDown)
                    {
                        double x1, x2, y1, y2;
                        if (curPt.X < areaMouseDownPoint.X)
                        {
                            x1 = curPt.X;
                            x2 = areaMouseDownPoint.X;
                        }
                        else
                        {
                            x1 = areaMouseDownPoint.X;
                            x2 = curPt.X;
                        }
                        if (curPt.Y < areaMouseDownPoint.Y)
                        {
                            y1 = curPt.Y;
                            y2 = areaMouseDownPoint.Y;
                        }
                        else
                        {
                            y1 = areaMouseDownPoint.Y;
                            y2 = curPt.Y;
                        }

                        if (x2 - x1 < minimumLength)
                            x2 = x1 + minimumLength;
                        if (y2 - y1 < minimumLength)
                            y2 = y1 + minimumLength;

                        selectedArea = new Int32Rect(
                            (int)(x1 + 0.5), (int)(y1 + 0.5),
                            (int)(x2 - x1 + 0.5), (int)(y2 - y1 + 0.5));
                        ShowSelectedArea();
                    }
                    break;
                case 1:
                    if (isAreaMouseLBDown)
                    {
                        double offX = curPt.X - areaMouseDownPoint.X,
                            offY = curPt.Y - areaMouseDownPoint.Y;
                        switch (selectedHandleIndex)
                        {
                            case 0:
                                selectedArea = GetNewRect(
                                    areaMouseDownSelectedRect,
                                    offX, offY, -offX, -offY);
                                break;
                            case 1:
                                selectedArea = GetNewRect(
                                    areaMouseDownSelectedRect,
                                    0, offY, 0, -offY);
                                break;
                            case 2:
                                selectedArea = GetNewRect(
                                    areaMouseDownSelectedRect,
                                    0, offY, offX, -offY);
                                break;
                            case 3:
                                selectedArea = GetNewRect(
                                    areaMouseDownSelectedRect,
                                    offX, 0, -offX, 0);
                                break;
                            case 4:
                                selectedArea = GetNewRect(
                                    areaMouseDownSelectedRect,
                                    0, 0, offX, 0);
                                break;
                            case 5:
                                selectedArea = GetNewRect(
                                    areaMouseDownSelectedRect,
                                    offX, 0, -offX, offY);
                                break;
                            case 6:
                                selectedArea = GetNewRect(
                                    areaMouseDownSelectedRect,
                                    0, 0, 0, offY);
                                break;
                            case 7:
                                selectedArea = GetNewRect(
                                    areaMouseDownSelectedRect,
                                    0, 0, offX, offY);
                                break;
                            case 8:
                                selectedArea = GetNewRect(
                                    areaMouseDownSelectedRect,
                                    offX, offY, 0, 0);
                                break;
                            default:
                            case -1:
                                break;
                        }
                        if (selectedHandleIndex >= 0)
                        {
                            if (areaMouseDownSelectedRect.Width < minimumLength)
                                areaMouseDownSelectedRect.Width = minimumLength;
                            if (areaMouseDownSelectedRect.Height < minimumLength)
                                areaMouseDownSelectedRect.Height = minimumLength;
                            ShowSelectedArea();
                        }
                    }
                    else
                    {
                        if (CheckMouseInSelectArea(curPt))
                        {
                            selectedHandleIndex = 8;
                            rect_touchArea.Cursor = Cursors.SizeAll;
                        }
                        else if (CheckMouseInHandle(bdr_handleTopLeft, curPt))
                        {
                            selectedHandleIndex = 0;
                            rect_touchArea.Cursor = Cursors.SizeNWSE;
                        }
                        else if (CheckMouseInHandle(bdr_handleTopCenter, curPt))
                        {
                            selectedHandleIndex = 1;
                            rect_touchArea.Cursor = Cursors.SizeNS;
                        }
                        else if (CheckMouseInHandle(bdr_handleTopRight, curPt))
                        {
                            selectedHandleIndex = 2;
                            rect_touchArea.Cursor = Cursors.SizeNESW;
                        }
                        else if (CheckMouseInHandle(bdr_handleMidLeft, curPt))
                        {
                            selectedHandleIndex = 3;
                            rect_touchArea.Cursor = Cursors.SizeWE;
                        }
                        else if (CheckMouseInHandle(bdr_handleMidRight, curPt))
                        {
                            selectedHandleIndex = 4;
                            rect_touchArea.Cursor = Cursors.SizeWE;
                        }
                        else if (CheckMouseInHandle(bdr_handleBottomLeft, curPt))
                        {
                            selectedHandleIndex = 5;
                            rect_touchArea.Cursor = Cursors.SizeNESW;
                        }
                        else if (CheckMouseInHandle(bdr_handleBottomCenter, curPt))
                        {
                            selectedHandleIndex = 6;
                            rect_touchArea.Cursor = Cursors.SizeNS;
                        }
                        else if (CheckMouseInHandle(bdr_handleBottomRight, curPt))
                        {
                            selectedHandleIndex = 7;
                            rect_touchArea.Cursor = Cursors.SizeNWSE;
                        }
                        else
                        {
                            selectedHandleIndex = -1;
                            rect_touchArea.Cursor = Cursors.Arrow;
                        }
                    }
                    break;
            }
        }
        private Int32Rect GetNewRect(Int32Rect oriRect, double offX, double offY, double offWidth, double offHeight)
        {
            double x, y, newWidth, newHeight;
            Cal(oriRect.X, oriRect.Width, offX, offWidth, out x, out newWidth);
            Cal(oriRect.Y, oriRect.Height, offY, offHeight, out y, out newHeight);

            void Cal(int basePosi, int baseSize, double offPosi, double offSize, out double posi, out double size)
            {
                if (baseSize + offSize >= 0)
                {
                    posi = basePosi + offPosi;
                    size = baseSize + offSize;
                }
                else
                {
                    if (offPosi == 0)
                    {
                        posi = basePosi + baseSize + offSize;
                    }
                    else
                    {
                        posi = basePosi + baseSize;
                    }
                    size = -baseSize - offSize;
                }
                if (size < minimumLength)
                {
                    double inc = minimumLength - size;
                    posi -= inc;
                    size += inc;
                }
            }

            return new Int32Rect(
                (int)(x + 0.5), (int)(y + 0.5),
                (int)(newWidth + 0.5), (int)(newHeight + 0.5));
        }

        public BitmapSource SelectedImage
        {
            get
            {
                if (selectedArea != Int32Rect.Empty)
                {
                    return QuickGraphics.ChopImage((BitmapSource)img_bg.Source,
                        selectedArea.X,
                        (int)(selectedArea.Y + 0.5),
                        (int)(this.ActualWidth - selectedArea.X - selectedArea.Width + 0.5),
                        (int)(this.ActualHeight - selectedArea.Y - selectedArea.Height + 0.5));
                }
                else
                {
                    return null;
                }
            }
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            switch (e.Key)
            {
                case Key.Return:
                    if (stateCode == 1)
                    {
                        this.DialogResult = true;
                        this.Close();
                    }
                    break;
                case Key.Escape:
                    if (stateCode == 1)
                    {
                        selectedArea = Int32Rect.Empty;
                        ShowSelectedArea();
                        stateCode = 0;
                    }
                    else
                    {
                        this.DialogResult = false;
                        this.Close();
                    }
                    break;
            }
        }
    }
}
