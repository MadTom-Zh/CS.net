using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace MadTomDev.UI
{
    public static class Extensions
    {
        /*
        public static void Update(this UIElement ui)
        {
            ui.Dispatcher.Invoke(()=> { });
        }*/
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
        public static BitmapImage ToBitmapImage(this FrameworkElement ui)
        {
            RenderTargetBitmap bmpCopied = new RenderTargetBitmap(
                (int)(ui.ActualWidth + 0.5),
                (int)(ui.ActualHeight + 0.5), 96, 96,
                PixelFormats.Default);
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                VisualBrush visualBrush = new VisualBrush(ui);
                drawingContext.DrawRectangle(
                    visualBrush, null,
                    new Rect(new System.Windows.Point(),
                    new System.Windows.Size(ui.ActualWidth, ui.ActualHeight)));
            }
            bmpCopied.Render(drawingVisual);


            BitmapImage bmp = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmpCopied));
                encoder.Save(stream);
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bmp.StreamSource = new MemoryStream(stream.ToArray()); //stream;
                bmp.EndInit();
                bmp.Freeze();
            }
            return bmp;
        }
        public static void SaveToImageFile(this FrameworkElement ui, string imgFileName)
        {
            using (FileStream stream = new FileStream(imgFileName, FileMode.Create))
            {

                RenderTargetBitmap bmp = new RenderTargetBitmap(
                    (int)(ui.ActualWidth + 0.5), (int)(ui.ActualHeight + 0.5),
                    96d, 96d, PixelFormats.Pbgra32);
                // 设置渲染区域，避免图像错位；
                ui.Arrange(new Rect(0, 0, ui.ActualWidth, ui.ActualHeight));
                bmp.Render(ui);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(stream);
            }
        }


        #region create cursor from bitmap image
        //Source: https://toxigon.com/create-cursor-from-bitmap-wpf
        [StructLayout(LayoutKind.Sequential)]
        private struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }


        //[DllImport("user32.dll")]
        //private static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll")]
        private static extern SafeIconHandle CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetIconInfo(IntPtr hIcon, out IconInfo pIconInfo);

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        protected class SafeIconHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeIconHandle() : base(true)
            {
            }
            protected override bool ReleaseHandle()
            {
                return DestroyIcon(handle);
            }
        }

        #endregion


        public static Cursor CreateCursor(this BitmapImage bitmap, int xHotspot, int yHotspot)
        {
            // Create a MemoryStream from the BitmapImage
            MemoryStream ms = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(ms);
            ms.Seek(0, SeekOrigin.Begin);

            // Create a Bitmap from the MemoryStream
            Bitmap bmp = new Bitmap(ms);

            // Create an IconInfo structure
            IconInfo iconInfo = new IconInfo();
            iconInfo.fIcon = false;
            iconInfo.xHotspot = xHotspot;
            iconInfo.yHotspot = yHotspot;
            iconInfo.hbmMask = bmp.GetHbitmap();
            iconInfo.hbmColor = bmp.GetHbitmap();

            // Create the cursor
            //IntPtr hCursor = CreateIconIndirect(ref iconInfo);
            SafeIconHandle hCursor = CreateIconIndirect(ref iconInfo);



            //return CursorInteropHelper.Create(new SafeHandle(hCursor, true));
            return CursorInteropHelper.Create(hCursor);
        }
    }
}
