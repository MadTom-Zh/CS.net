using System;

using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Linq;

using ImageBox = System.Windows.Controls.Image;
using Cursors = System.Windows.Input.Cursors;
using Controls = System.Windows.Controls;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace MadTomDev.UIs
{
    public class ScreenSelectorSimple
    {
        private Window topWindow = null;
        private ImageBox imageBoxBG = null;
        private ImageBox imageBox = null;
        private Border bdrSlt = null;
        private Border bdrSlt_topLeft = null;
        private Border bdrSlt_topMiddle = null;
        private Border bdrSlt_topRight = null;
        private Border bdrSlt_left = null;
        private Border bdrSlt_right = null;
        private Border bdrSlt_buttomLeft = null;
        private Border bdrSlt_buttomMiddle = null;
        private Border bdrSlt_buttomRight = null;
        Bitmap screenBmp, partialBmp;
        public ScreenSelectorSimple()
        {
        }
        public void StartSelect()
        {
            ActionDone?.Invoke(this, ActionTypes.Msg, "Init screen shot.");
            int left = Screen.AllScreens.Min(screen => screen.Bounds.X);
            int top = Screen.AllScreens.Min(screen => screen.Bounds.Y);
            int right = Screen.AllScreens.Max(screen => screen.Bounds.X + screen.Bounds.Width);
            int bottom = Screen.AllScreens.Max(screen => screen.Bounds.Y + screen.Bounds.Height);
            int width = right - left;
            int height = bottom - top;

            if (imageBoxBG == null)
            {
                imageBoxBG = new ImageBox()
                {
                    Opacity = 0.5,
                    Stretch = Stretch.None,
                };
            }
            if (imageBox == null)
            {
                imageBox = new ImageBox()
                {
                    Stretch = Stretch.None,
                };
                imageBox.PreviewMouseDown += ImageBox_PreviewMouseDown;
            }

            screenBmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics bmpGraphics = Graphics.FromImage(screenBmp))
            {
                bmpGraphics.CopyFromScreen(left, top, 0, 0, new System.Drawing.Size(width, height));
                BitmapSource imageSource = Imaging.CreateBitmapSourceFromHBitmap(
                    screenBmp.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                imageBoxBG.Source = imageSource;
                imageBox.Source = imageSource;

                //imageBox.Source = imageBoxBG.Source;
            }


            if (topWindow == null)
            {
                ActionDone?.Invoke(this, ActionTypes.Msg, "Init select window.");
                SolidColorBrush bgBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                topWindow = new Window()
                {
                    Title = "Simple screen select window",
                    WindowStyle = WindowStyle.None,
                    Background = bgBrush,
                    BorderThickness = new Thickness(0, 0, 0, 0),

                    //Topmost = true,


                    Left = left,
                    Top = top,
                    Width = width,
                    Height = height,
                };
                topWindow.PreviewKeyDown += TopWindow_PreviewKeyDown;
                topWindow.PreviewMouseDown += TopWindow_PreviewMouseDown;
                topWindow.PreviewMouseMove += TopWindow_PreviewMouseMove;
                topWindow.PreviewMouseUp += TopWindow_PreviewMouseUp;

                // background
                Grid continer = new Grid();
                topWindow.Content = continer;
                continer.Children.Add(imageBoxBG);

                SolidColorBrush bdrSltBrush
                    = new SolidColorBrush(System.Windows.Media.Color.FromRgb(128, 0, 128));
                SolidColorBrush bdrSltHandleBrush
                    = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 64));
                bdrSlt = new Border()
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    BorderThickness = new Thickness(1),
                    BorderBrush = bdrSltBrush,
                    Visibility = Visibility.Collapsed,
                };
                bdrSlt.MouseEnter += Bdr_MouseEnter;
                bdrSlt.MouseLeave += Bdr_MouseLeave;
                continer.Children.Add(bdrSlt);
                bdrSlt.Child = imageBox;

                #region init all handles
                bdrSlt_topLeft = new Border()
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    BorderThickness = new Thickness(1),
                    BorderBrush = bdrSltHandleBrush,
                    Width = 4,
                    Height = 4,
                    Visibility = Visibility.Collapsed,
                };
                bdrSlt_topLeft.MouseEnter += Bdr_MouseEnter;
                bdrSlt_topLeft.MouseLeave += Bdr_MouseLeave;
                continer.Children.Add(bdrSlt_topLeft);
                bdrSlt_topMiddle = new Border()
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    BorderThickness = new Thickness(1),
                    BorderBrush = bdrSltHandleBrush,
                    Width = 4,
                    Height = 4,
                    Visibility = Visibility.Collapsed,
                };
                bdrSlt_topMiddle.MouseEnter += Bdr_MouseEnter;
                bdrSlt_topMiddle.MouseLeave += Bdr_MouseLeave;
                continer.Children.Add(bdrSlt_topMiddle);
                bdrSlt_topRight = new Border()
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    BorderThickness = new Thickness(1),
                    BorderBrush = bdrSltHandleBrush,
                    Width = 4,
                    Height = 4,
                    Visibility = Visibility.Collapsed,
                };
                bdrSlt_topRight.MouseEnter += Bdr_MouseEnter;
                bdrSlt_topRight.MouseLeave += Bdr_MouseLeave;
                continer.Children.Add(bdrSlt_topRight);

                bdrSlt_left = new Border()
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    BorderThickness = new Thickness(1),
                    BorderBrush = bdrSltHandleBrush,
                    Width = 4,
                    Height = 4,
                    Visibility = Visibility.Collapsed,
                };
                bdrSlt_left.MouseEnter += Bdr_MouseEnter;
                bdrSlt_left.MouseLeave += Bdr_MouseLeave;
                continer.Children.Add(bdrSlt_left);
                bdrSlt_right = new Border()
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    BorderThickness = new Thickness(1),
                    BorderBrush = bdrSltHandleBrush,
                    Width = 4,
                    Height = 4,
                    Visibility = Visibility.Collapsed,
                };
                bdrSlt_right.MouseEnter += Bdr_MouseEnter;
                bdrSlt_right.MouseLeave += Bdr_MouseLeave;
                continer.Children.Add(bdrSlt_right);

                bdrSlt_buttomLeft = new Border()
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    BorderThickness = new Thickness(1),
                    BorderBrush = bdrSltHandleBrush,
                    Width = 4,
                    Height = 4,
                    Visibility = Visibility.Collapsed,
                };
                bdrSlt_buttomLeft.MouseEnter += Bdr_MouseEnter;
                bdrSlt_buttomLeft.MouseLeave += Bdr_MouseLeave;
                continer.Children.Add(bdrSlt_buttomLeft);
                bdrSlt_buttomMiddle = new Border()
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    BorderThickness = new Thickness(1),
                    BorderBrush = bdrSltHandleBrush,
                    Width = 4,
                    Height = 4,
                    Visibility = Visibility.Collapsed,
                };
                bdrSlt_buttomMiddle.MouseEnter += Bdr_MouseEnter;
                bdrSlt_buttomMiddle.MouseLeave += Bdr_MouseLeave;
                continer.Children.Add(bdrSlt_buttomMiddle);
                bdrSlt_buttomRight = new Border()
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    BorderThickness = new Thickness(1),
                    BorderBrush = bdrSltHandleBrush,
                    Width = 4,
                    Height = 4,
                    Visibility = Visibility.Collapsed,
                };
                bdrSlt_buttomRight.MouseEnter += Bdr_MouseEnter;
                bdrSlt_buttomRight.MouseLeave += Bdr_MouseLeave;
                continer.Children.Add(bdrSlt_buttomRight);
                #endregion

            }

            ActionDone?.Invoke(this, ActionTypes.SelectStart, null);
            topWindow.Show();
        }

        public void Dispose()
        {
            topWindow.Close();
            screenBmp.Dispose();
            partialBmp.Dispose();
        }

        private void Bdr_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (selecting)
                return;

            curBdr = (Border)sender;
            if (curBdr == bdrSlt)
                topWindow.Cursor = Cursors.SizeAll;

            if (curBdr == bdrSlt_topLeft)
                topWindow.Cursor = Cursors.SizeNWSE;
            if (curBdr == bdrSlt_topMiddle)
                topWindow.Cursor = Cursors.SizeNS;
            if (curBdr == bdrSlt_topRight)
                topWindow.Cursor = Cursors.SizeNESW;

            if (curBdr == bdrSlt_left)
                topWindow.Cursor = Cursors.SizeWE;
            if (curBdr == bdrSlt_right)
                topWindow.Cursor = Cursors.SizeWE;

            if (curBdr == bdrSlt_buttomLeft)
                topWindow.Cursor = Cursors.SizeNESW;
            if (curBdr == bdrSlt_buttomMiddle)
                topWindow.Cursor = Cursors.SizeNS;
            if (curBdr == bdrSlt_buttomRight)
                topWindow.Cursor = Cursors.SizeNWSE;
        }
        private void Bdr_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (selecting)
                return;

            topWindow.Cursor = Cursors.Arrow;
            curBdr = null;
        }

        private Border curBdr;
        private Point StartMP;
        private Point StartSelectTopLeft = new Point();
        private Size StartSelectSize = new Size();
        private bool selecting = false;

        private bool initSelecting = true;
        private bool isMouseDown = false;
        private Point initSelectingMP1;
        private Point initSelectingMP2;
        private void TopWindow_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isMouseDown = true;
            if (initSelecting)
            {
                initSelectingMP1 = System.Windows.Input.Mouse.GetPosition(topWindow);
                return;
            }

            if (curBdr == null)
                return;

            selecting = true;
            StartMP = System.Windows.Input.Mouse.GetPosition(topWindow);
            StartSelectTopLeft = new Point(bdrSlt.Margin.Left, bdrSlt.Margin.Top);
            StartSelectSize = new Size(bdrSlt.ActualWidth, bdrSlt.ActualHeight);
        }
        private void TopWindow_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!isMouseDown)
                return;

            double offX;
            double offY;

            if (initSelecting)
            {
                initSelectingMP2 = System.Windows.Input.Mouse.GetPosition(topWindow);
                offX = initSelectingMP2.X - initSelectingMP1.X;
                offY = initSelectingMP2.Y - initSelectingMP1.Y;
                if (offX > 0 && offX < 3) offX = 3;
                else if (offX <= 0 && offX > -3) offX = -3;
                if (offY > 0 && offY < 3) offY = 3;
                else if (offY <= 0 && offY > -3) offY = -3;

                StartSelectTopLeft = new Point();
                StartSelectSize = new Size();
                if (offX > 0)
                {
                    StartSelectTopLeft.X = initSelectingMP1.X;
                    StartSelectSize.Width = offX;
                }
                else
                {
                    StartSelectTopLeft.X = initSelectingMP2.X;
                    StartSelectSize.Width = -offX;
                }
                if (offY > 0)
                {
                    StartSelectTopLeft.Y = initSelectingMP1.Y;
                    StartSelectSize.Height = offY;
                }
                else
                {
                    StartSelectTopLeft.Y = initSelectingMP2.Y;
                    StartSelectSize.Height = -offY;
                }
                if (bdrSlt.Visibility != Visibility.Visible)
                {
                    bdrSlt.Visibility = Visibility.Visible;
                    bdrSlt_topLeft.Visibility = Visibility.Visible;
                    bdrSlt_topMiddle.Visibility = Visibility.Visible;
                    bdrSlt_topRight.Visibility = Visibility.Visible;
                    bdrSlt_left.Visibility = Visibility.Visible;
                    bdrSlt_right.Visibility = Visibility.Visible;
                    bdrSlt_buttomLeft.Visibility = Visibility.Visible;
                    bdrSlt_buttomMiddle.Visibility = Visibility.Visible;
                    bdrSlt_buttomRight.Visibility = Visibility.Visible;
                }

                SizeSelectWindow(0, 0, 0, 0);

                return;
            }

            if (!selecting)
                return;

            Point curMP = System.Windows.Input.Mouse.GetPosition(topWindow);
            offX = curMP.X - StartMP.X;
            offY = curMP.Y - StartMP.Y;
            if (curBdr == bdrSlt)
                SizeSelectWindow(offX, offX, offY, offY);

            if (curBdr == bdrSlt_topLeft)
                SizeSelectWindow(offX, 0, offY, 0);
            if (curBdr == bdrSlt_topMiddle)
                SizeSelectWindow(0, 0, offY, 0);
            if (curBdr == bdrSlt_topRight)
                SizeSelectWindow(0, offX, offY, 0);

            if (curBdr == bdrSlt_left)
                SizeSelectWindow(offX, 0, 0, 0);
            if (curBdr == bdrSlt_right)
                SizeSelectWindow(0, offX, 0, 0);

            if (curBdr == bdrSlt_buttomLeft)
                SizeSelectWindow(offX, 0, 0, offY);
            if (curBdr == bdrSlt_buttomMiddle)
                SizeSelectWindow(0, 0, 0, offY);
            if (curBdr == bdrSlt_buttomRight)
                SizeSelectWindow(0, offX, 0, offY);
        }
        private void SizeSelectWindow(double offsetLeft, double offsetRight, double offsetTop, double offsetButtom)
        {
            if (offsetRight == 0 && offsetLeft != 0 && StartSelectSize.Width - offsetLeft < 1)
                offsetLeft = StartSelectSize.Width - 1;
            if (offsetLeft == 0 && offsetRight != 0 && StartSelectSize.Width + offsetRight < 1)
                offsetRight = 1 - StartSelectSize.Width;
            if (offsetButtom == 0 && offsetTop != 0 && StartSelectSize.Height - offsetTop < 1)
                offsetTop = StartSelectSize.Height - 1;
            if (offsetTop == 0 && offsetButtom != 0 && StartSelectSize.Height + offsetButtom < 1)
                offsetButtom = 1 - StartSelectSize.Height;

            double newLeft = StartSelectTopLeft.X + offsetLeft;
            double newTop = StartSelectTopLeft.Y + offsetTop;
            double newWidth = StartSelectSize.Width - offsetLeft + offsetRight;
            double newHeight = StartSelectSize.Height - offsetTop + offsetButtom;

            bdrSlt.Margin = new Thickness(newLeft, newTop, 0, 0);
            bdrSlt.Width = newWidth;
            bdrSlt.Height = newHeight;

            MakePartialBmp(
                (int)(newLeft + 0.5),
                (int)(newTop + 0.5),
                (int)(newWidth + 0.5),
                (int)(newHeight + 0.5));
            BitmapSource wndImg = Imaging.CreateBitmapSourceFromHBitmap(
                partialBmp.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            imageBox.Source = wndImg;

            double halfNewWidth = newWidth / 2;
            double halfNewHeight = newHeight / 2;
            double handleWidth = bdrSlt_topLeft.ActualWidth;
            double halfHandleWidth = handleWidth / 2;

            bdrSlt_topLeft.Margin = new Thickness(
                newLeft - handleWidth - 1,
                newTop - handleWidth - 1, 0, 0);
            bdrSlt_topMiddle.Margin = new Thickness(
                newLeft + halfNewWidth - halfHandleWidth,
                newTop - handleWidth - 1,
                0, 0);
            bdrSlt_topRight.Margin = new Thickness(
                newLeft + newWidth + 1,
                newTop - handleWidth - 1,
                0, 0);

            bdrSlt_left.Margin = new Thickness(
                newLeft - handleWidth - 1,
                newTop + halfNewHeight - halfHandleWidth,
                0, 0);
            bdrSlt_right.Margin = new Thickness(
                newLeft + newWidth + 1,
                newTop + halfNewHeight - halfHandleWidth,
                0, 0);

            bdrSlt_buttomLeft.Margin = new Thickness(
                newLeft - handleWidth - 1,
                newTop + newHeight + 1, 0, 0);
            bdrSlt_buttomMiddle.Margin = new Thickness(
                newLeft + halfNewWidth - halfHandleWidth,
                newTop + newHeight + 1,
                0, 0);
            bdrSlt_buttomRight.Margin = new Thickness(
                newLeft + newWidth + 1,
                newTop + newHeight + 1,
                0, 0);
        }
        private void MakePartialBmp(int left, int top, int width, int height)
        {
            partialBmp?.Dispose();
            partialBmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(partialBmp))
            {
                g.DrawImage(screenBmp,
                    new Rectangle(0, 0, width, height),
                    new Rectangle(left, top, width, height),
                    GraphicsUnit.Pixel
                    );
            }
        }
        private void TopWindow_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (initSelecting)
            {
                initSelecting = false;
            }
            isMouseDown = false;
            selecting = false;
        }


        private DateTime ImageBox_PreviewMouseDown_pre = DateTime.MinValue;
        private void ImageBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((DateTime.Now - ImageBox_PreviewMouseDown_pre).TotalMilliseconds <= SystemInformation.DoubleClickTime)
            {
                Confirm();
                ImageBox_PreviewMouseDown_pre = DateTime.MinValue;
                return;
            }
            ImageBox_PreviewMouseDown_pre = DateTime.Now;
        }
        private void TopWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.Enter:
                    Confirm();
                    break;
                case System.Windows.Input.Key.Escape:
                    Cancel();
                    break;
            }
        }

        public void Confirm()
        {
            //RenderTargetBitmap renderTargetBitmap =
            //    new RenderTargetBitmap(
            //        (int)(bdrSlt.ActualWidth + 0.5),
            //        (int)(bdrSlt.ActualHeight + 0.5),
            //        96, 96, PixelFormats.Pbgra32);
            //renderTargetBitmap.Render(imageBox);


            //PngBitmapEncoder pngImage = new PngBitmapEncoder();
            //pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            //using (Stream fileStream = File.Create(filePath))
            //{
            //    pngImage.Save(fileStream);
            //}


            topWindow.Hide();
            ActionDone?.Invoke(this, ActionTypes.SelectEnd, partialBmp);
        }
        public void Cancel()
        {
            topWindow.Hide();
            ActionDone?.Invoke(this, ActionTypes.SelectEnd, null);
        }

        public enum ActionTypes
        { None, Msg, SelectStart, SelectEnd }
        public delegate void ActionDoneDelegate(ScreenSelectorSimple sender, ActionTypes action, object outData);
        public event ActionDoneDelegate ActionDone;
    }
}
