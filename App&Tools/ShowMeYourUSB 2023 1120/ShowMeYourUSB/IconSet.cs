using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MadTomDev.App
{
    class IconSet
    {
        private IconSet() { }
        private static IconSet instance;
        public static IconSet GetInstance()
        {
            if (instance == null)
            {
                instance = new IconSet();
            }
            return instance;
        }

        public BitmapSource IconDriveOffline
        {
            get => iconS32.GetIcon(10, false);
        }
        //public BitmapSource _IconNotify = null;
        public System.Drawing.Bitmap GetIconNotify()
        {
            //_IconNotify = new System.Drawing.Bitmap(stream);
            System.Drawing.Bitmap bBase = GetBitmap(iconS32.GetIcon(53, false));
            System.Drawing.Bitmap bFore = GetBitmap(iconS32.GetIcon(22, false));

            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bBase))
            {
                //g.DrawImage(bFore, new System.Drawing.Rectangle(0, 4, 12, 12));
                g.DrawImage(bFore, new System.Drawing.Rectangle(0, 0, 16, 16));
            }

            return bBase;
        }
        private System.Drawing.Bitmap IconNotifyWithNum_old;
        internal System.Drawing.Bitmap GetIconNotifyWithNum(int num)
        {
            IconNotifyWithNum_old?.Dispose();

            System.Drawing.Bitmap imgBase = GetIconNotify();
            System.Drawing.Font font = new System.Drawing.Font("consolas", 9);
            string strNum = num.ToString();
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(imgBase))
            {
                System.Drawing.SizeF strSize = g.MeasureString(strNum, font);
                using (System.Drawing.Brush strBrush = new System.Drawing.SolidBrush(System.Drawing.Color.BlueViolet))
                {
                    g.DrawString(num.ToString(), font, strBrush,
                        new System.Drawing.PointF(
                            imgBase.Width - strSize.Width,
                            imgBase.Height - strSize.Height)
                        );
                }
            }
            IconNotifyWithNum_old = imgBase;
            return imgBase;
        }
        public System.Drawing.Bitmap GetBitmap(BitmapSource bs)
        {
            // alpha changed to black...

            //System.Drawing.Bitmap bitmap;
            //using (MemoryStream outStream = new MemoryStream())
            //{
            //    BitmapEncoder enc = new BmpBitmapEncoder();

            //    enc.Frames.Add(BitmapFrame.Create(bs));
            //    enc.Save(outStream);
            //    bitmap = new System.Drawing.Bitmap(outStream);
            //}
            //return bitmap;


            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(
                bs.PixelWidth,
                bs.PixelHeight,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(
                new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            bs.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        public BitmapSource GetDriveIcon(string driveName)
        {
            return iconFS.GetIcon(driveName, true, true);
        }

        private Common.IconHelper.FileSystem iconFS = Common.IconHelper.FileSystem.Instance;
        private Common.IconHelper.Shell32Icons iconS32 = Common.IconHelper.Shell32Icons.Instance;

    }
}
