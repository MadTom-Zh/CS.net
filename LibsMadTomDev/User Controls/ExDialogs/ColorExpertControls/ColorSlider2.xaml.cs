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

namespace MadTomDev.UI.ColorExpertControls
{
    /// <summary>
    /// Interaction logic for ColorSlider2.xaml
    /// </summary>
    public partial class ColorSlider2 : UserControl
    {
        public ColorSlider2()
        {
            InitializeComponent();

            bdr_handle.PreviewMouseDown += Bdr_handle_PreviewMouseDown;
            bdr_handle.MouseLeave += Bdr_handle_MouseLeave;
            bdr_handle.PreviewMouseUp += Bdr_handle_PreviewMouseUp;
            bdr_handle.PreviewMouseMove += Bdr_handle_PreviewMouseMove;
            img_backImg.PreviewMouseDown += Img_backImg_PreviewMouseDown;
        }


        private bool isVertical;
        public void Init(BitmapSource stripeImage, bool isVertical = false, double initValue = 0.5)
        {
            this.isVertical = isVertical;
            if (isVertical)
            {
                this.MaxWidth = this.MinWidth = 40;
                bdr_handle.HorizontalAlignment = HorizontalAlignment.Stretch;
                bdr_handle.VerticalAlignment = VerticalAlignment.Top;
            }
            else
            {
                this.MaxHeight = this.MinHeight = 40;
                bdr_handle.HorizontalAlignment = HorizontalAlignment.Left;
                bdr_handle.VerticalAlignment = VerticalAlignment.Stretch;
            }
            ImageBrush bgBrush = new ImageBrush(stripeImage)
            {
                TileMode = TileMode.Tile,
                ViewportUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(0, 0, 40, 40),
            };
            grid.Background = bgBrush;
            SetHandlePosi(initValue);
            value = initValue;
        }

        public ImageSource StriptImage
        {
            get => img_backImg.Source;
            set => img_backImg.Source = value;
        }


        public bool IsSliding = false;
        private void Bdr_handle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Img_backImg_PreviewMouseDown(sender, e);
        }
        private void Bdr_handle_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            IsSliding = false;
        }

        private void Bdr_handle_MouseLeave(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            IsSliding = false;
        }
        private void Bdr_handle_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            if (IsSliding)
            {
                Point mPt = Mouse.GetPosition(img_backImg);
                SetHandlePosi(mPt, out value);
                SetValueFromSlider?.Invoke(value);
            }
        }
        private void Img_backImg_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            IsSliding = true;
            if (e.RightButton == MouseButtonState.Pressed)
            {
                SetHandlePosi(0.5);
                value = 0.5;
            }
            else
            {
                Point mPt = Mouse.GetPosition(img_backImg);
                SetHandlePosi(mPt, out value);
            }
            SetValueFromSlider?.Invoke(value);
        }

        public double value;

        public Action<double> SetValueFromSlider;

        public void SetHandlePosi(double value)
        {
            if (isVertical)
            {
                bdr_handle.Margin = new Thickness(
                    0,
                    img_backImg.ActualHeight * value
                        - bdr_handle.ActualHeight / 2,
                    0,
                    0);
            }
            else
            {
                bdr_handle.Margin = new Thickness(
                    img_backImg.ActualWidth * value
                        - bdr_handle.ActualWidth / 2,
                    0,
                    0,
                    0);
            }
        }
        private void SetHandlePosi(Point mousePosition, out double value)
        {
            if (isVertical)
            {
                value = (double)mousePosition.Y / img_backImg.ActualHeight;
            }
            else
            {
                value = (double)mousePosition.X / img_backImg.ActualWidth;
            }
            if (value < 0)
                value = 0;
            else if (value > 1)
                value = 1;
            SetHandlePosi(value);
        }


    }
}
