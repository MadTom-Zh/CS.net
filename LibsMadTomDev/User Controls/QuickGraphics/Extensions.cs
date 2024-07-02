using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MadTomDev.UI
{
    public static class Extensions
    {
        public static BitmapImage ToBitmapImage(this ImageSource img)
        {
            if (img is WriteableBitmap)
            {
                return ToBitmapImage((WriteableBitmap)img);
            }
            else
            {
                return (BitmapImage)img;
            }
        }
        public static BitmapImage ToBitmapImage(this WriteableBitmap img)
        {
            BitmapImage bmImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                encoder.Save(stream);
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = stream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }
        public static void SaveToImageFile(this FrameworkElement ui, string imgFileName)
        {
            using (FileStream stream = new FileStream(imgFileName, FileMode.Create))
            {

                RenderTargetBitmap bmp = new RenderTargetBitmap(
                    (int)(ui.ActualWidth + 0.5), (int)(ui.ActualHeight + 0.5),
                    96d, 96d, PixelFormats.Pbgra32);
                // 设置渲染区域，避免图像错位；
                ui.Arrange(new Rect(0,0, ui.ActualWidth, ui.ActualHeight));
                bmp.Render(ui);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(stream);
            }
        }
    }
}
