using System;
using System.Collections.Generic;
using System.IO;
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

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for TitledMedia.xaml
    /// </summary>
    public partial class TitledMedia : UserControl
    {
        public TitledMedia()
        {
            InitializeComponent();
        }

        public TextBlock TextBlock
        { get => textBlock; }
        public MediaElement MediaElement
        { get => mediaElement; }

        private FileInfo _MediaFile;
        public FileInfo MediaFile
        {
            get => _MediaFile;
            internal set
            {
                _MediaFile = value;
                if (_MediaFile != null)
                {
                    textBlock.Text = _MediaFile.Name;
                    mediaElement.Source = new Uri(_MediaFile.FullName);
                    mediaElement.MediaEnded += MediaElement_MediaEnded;
                }
                else
                {
                    textBlock.Text = "---";
                    mediaElement.Source = null;
                    mediaElement.MediaEnded -= MediaElement_MediaEnded;
                }
            }
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = TimeSpan.FromMilliseconds(1);
            mediaElement.Play();
        }

        internal void Pause()
        {
            mediaElement.Pause();
        }

        internal void Resume()
        {
            mediaElement.Play();
        }
        internal void Close()
        {
            mediaElement.Stop();
            mediaElement.Close();
        }

        public string Title
        {
            get => textBlock.Text;
            set
            {
                textBlock.Text = value;
                if (string.IsNullOrWhiteSpace(value))
                    textBlock.Width = double.NaN;
            }
        }
        public Visibility TitleVisibility
        {
            get => textBlock.Visibility;
            set => textBlock.Visibility = value;
        }

        public void SetSize(Size newSize)
        {
            this.Margin = new Thickness(0);
            mediaElement.Stretch = Stretch.Uniform;
            this.Width = newSize.Width;
            mediaElement.Width = double.NaN;
            this.Height = newSize.Height;
            mediaElement.Height = double.NaN;
            textBlock.Width = newSize.Width;
        }
        public void SetZoomOrigin(Size windowSize)
        {
            mediaElement.Stretch = Stretch.None;
            this.Width = mediaElement.NaturalVideoWidth;
            textBlock.Width = double.NaN;
            this.Height = mediaElement.NaturalVideoHeight;// + grid.RowDefinitions[1].ActualHeight;

            double horSpace = (windowSize.Width - this.Width) / 2;
            double verSpace = (windowSize.Height - this.Height) / 2;
            this.Margin = new Thickness(horSpace, verSpace, horSpace, verSpace);
        }
        public async void SetZoomFull(Size windowSize)
        {
            mediaElement.Stretch = Stretch.Uniform;

            double eWidth = mediaElement.NaturalVideoWidth;
            double eHeight = (mediaElement.NaturalVideoHeight);// + grid.RowDefinitions[1].ActualHeight);
            if (eWidth == 0 || eHeight == 0)
                return;

            double whWin = windowSize.Width / windowSize.Height;
            double whEle = eWidth / eHeight;

            double horSpace;
            double verSpace;
            if (whWin < whEle)
            {
                this.Width = windowSize.Width;
                this.Height = windowSize.Width * (eHeight / eWidth);

                horSpace = 0;
                verSpace = (windowSize.Height - this.Height) / 2;
            }
            else
            {
                this.Width = windowSize.Height * (eWidth / eHeight);
                this.Height = windowSize.Height;

                horSpace = (windowSize.Width - this.Width) / 2;
                verSpace = 0;
            }
            this.Margin = new Thickness(horSpace, verSpace, horSpace, verSpace);
        }
        public void SetZoom(Size windowSize, Point mPoint, double zoomMultiple)
        {
            if (mediaElement.ActualWidth == 0 || mediaElement.ActualHeight == 0)
                return;

            mediaElement.Stretch = Stretch.Uniform;
            this.Width = double.NaN;
            this.Height = double.NaN;
            Thickness actMargin = this.Margin;
            if (actMargin.Left == 0 && actMargin.Top == 0
                && actMargin.Right == 0 && actMargin.Bottom == 0)
            {
                double horSpace = (windowSize.Width - this.ActualWidth) / 2;
                double verSpace = (windowSize.Height - this.ActualHeight) / 2;
                actMargin = new Thickness(horSpace, verSpace, horSpace, verSpace);
            }
            //actMargin.Bottom += grid.RowDefinitions[1].ActualHeight;

            //double zRatioX = (mPoint.X - windowSize.Width / 2
            //    + mediaElement.ActualWidth / 2
            //    + actMargin.Right - actMargin.Left)
            //        / mediaElement.ActualWidth;
            //double zRatioY = (mPoint.Y - windowSize.Height / 2
            //    + mediaElement.ActualHeight / 2
            //    //+ grid.RowDefinitions[1].ActualHeight / 2
            //    + actMargin.Bottom - actMargin.Top)
            //        / mediaElement.ActualHeight;

            double zRatioX;
            if (textBlock.ActualWidth > mediaElement.ActualWidth)
                zRatioX = (mPoint.X - actMargin.Left
                    - (textBlock.ActualWidth - mediaElement.ActualWidth) / 2)
                        / mediaElement.ActualWidth;
            else
                zRatioX = (mPoint.X - actMargin.Left) / mediaElement.ActualWidth;
            double zRatioY = (mPoint.Y - actMargin.Top) / mediaElement.ActualHeight;
            double zDecHor, zDecVer, zDecLeft, zDecTop, zDecRight, zDecBottom;
            zDecHor = mediaElement.ActualWidth * (zoomMultiple - 1);
            zDecVer = mediaElement.ActualHeight * (zoomMultiple - 1);
            zDecLeft = zDecHor * zRatioX;
            zDecRight = zDecHor - zDecLeft;
            zDecTop = zDecVer * zRatioY;
            zDecBottom = zDecVer - zDecTop;

            this.Margin = new Thickness(
                actMargin.Left - zDecLeft,
                actMargin.Top - zDecTop,
                actMargin.Right - zDecRight,
                actMargin.Bottom - zDecBottom);// - grid.RowDefinitions[1].ActualHeight);
        }

        public void Move(Thickness oriMargin, Size windowSize, Point oriPoint, Point newPoint)
        {
            Thickness actMargin;
            if (oriMargin.Left == 0 && oriMargin.Top == 0
                && oriMargin.Right == 0 && oriMargin.Bottom == 0)
            {
                double horSpace = (windowSize.Width - this.ActualHeight) / 2;
                double verSpace = (windowSize.Height - this.ActualHeight) / 2;
                actMargin = new Thickness(horSpace, verSpace, horSpace, verSpace);
            }
            else
            {
                actMargin = oriMargin;
            }
            double offX = newPoint.X - oriPoint.X;
            double offY = newPoint.Y - oriPoint.Y;

            this.Margin = new Thickness(
                actMargin.Left + offX,
                actMargin.Top + offY,
                actMargin.Right - offX,
                actMargin.Bottom - offY);
        }
                
        public void SetTitleColor(Brush brush)
        {
            textBlock.Foreground = brush;
        }
    }
}
