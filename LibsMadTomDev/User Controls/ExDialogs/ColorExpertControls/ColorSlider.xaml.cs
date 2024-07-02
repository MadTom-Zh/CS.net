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
    /// Interaction logic for ColorSlider.xaml
    /// </summary>
    public partial class ColorSlider : UserControl
    {
        public ColorSlider()
        {
            InitializeComponent();

            bdr_handle.PreviewMouseDown += Bdr_handle_PreviewMouseDown;
            bdr_handle.MouseLeave += Bdr_handle_MouseLeave;
            bdr_handle.PreviewMouseUp += Bdr_handle_PreviewMouseUp;
            bdr_handle.PreviewMouseMove += Bdr_handle_PreviewMouseMove;

            bdr_backStripe.PreviewMouseDown += Bdr_backStripe_PreviewMouseDown;
            bdr_backStripe.PreviewMouseUp += Bdr_backStripe_PreviewMouseUp;
        }


        private bool isVertical;
        private bool dontReach_One;
        public void Init(BitmapSource stripeImage, bool isVertical = true, double initValue = 0.5,bool dontReach_One = true)
        {
            this.isVertical = isVertical;
            this.dontReach_One = dontReach_One;
            if (isVertical)
            {
                this.MaxWidth = this.MinWidth = 50;
                bdr_backStripe.Margin = new Thickness(12, 16, 12, 16);
                bdr_handle.Width = 50;
                bdr_handle.HorizontalAlignment = HorizontalAlignment.Center;
                bdr_handle.VerticalAlignment = VerticalAlignment.Top;
            }
            else
            {
                this.MaxHeight = this.MinHeight = 50;
                bdr_backStripe.Margin = new Thickness(16, 12, 16, 12);
                bdr_handle.Height = 50;
                bdr_handle.HorizontalAlignment = HorizontalAlignment.Left;
                bdr_handle.VerticalAlignment = VerticalAlignment.Center;
            }
            StripeImage = stripeImage;
            SetHandlePosi(initValue);
            value = initValue;
        }
        public ImageSource StripeImage
        {
            get => img_backStripe.Source;
            set => img_backStripe.Source = value;
        }
        public void SetHandleColor(Color clr)
        {
            bdr_handle.Background = new SolidColorBrush(clr);
        }
        public void SetHandleWidth(double width)
        {
            bdr_handle.Width = width;
        }
        public void SetHandlePosi(double value)
        {
            if (isVertical)
            {
                bdr_handle.Margin = new Thickness(
                    0,
                    bdr_backStripe.Margin.Top
                        + bdr_backStripe.ActualHeight * value
                        - bdr_handle.ActualHeight / 2,
                    0,
                    0);
            }
            else
            {
                bdr_handle.Margin = new Thickness(
                    bdr_backStripe.Margin.Left
                        + bdr_backStripe.ActualWidth * value
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
                value = (double)mousePosition.Y / bdr_backStripe.ActualHeight;
            }
            else
            {
                value = (double)mousePosition.X / bdr_backStripe.ActualWidth;
            }
            if (value < 0)
                value = 0;
            else if (value > 1)
                value = 1;
            SetHandlePosi(value);
        }

        public double value;



        public bool IsSliding = false;
        private void Bdr_handle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Bdr_backStripe_PreviewMouseDown(sender, e);
        }
        private void Bdr_handle_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            IsSliding = false;
        }
        private void Bdr_backStripe_PreviewMouseUp(object sender, MouseButtonEventArgs e)
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
                Point mPt = Mouse.GetPosition(bdr_backStripe);
                SetHandlePosi(mPt, out value);
                if (dontReach_One && value == 1)
                    value = 0.9999;
                SetSelectedValue?.Invoke(value);
            }
        }




        private void Bdr_backStripe_PreviewMouseDown(object sender, MouseButtonEventArgs e)
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
                Point mPt = Mouse.GetPosition(bdr_backStripe);
                SetHandlePosi(mPt, out value);
            }
            if (dontReach_One && value == 1)
                value = 0.9999;
            SetSelectedValue?.Invoke(value);
        }

        public Action<double> SetSelectedValue;


    }
}
