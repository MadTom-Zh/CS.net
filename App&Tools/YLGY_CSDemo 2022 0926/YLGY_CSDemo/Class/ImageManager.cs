using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media.Imaging;
using System.IO;

namespace MadTomDev.Demo.Class
{
    internal class ImageManager
    {
        public ImageManager()
        {
            LoadImages();
        }

        public BitmapSource img_tileBase;
        public Dictionary<string, BitmapSource> imgDict_tilePaterns = new Dictionary<string, BitmapSource>();

        public void LoadImages()
        {
            img_tileBase = LoadImageFile(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "images/tileBase.png"));

            foreach (FileInfo fi in new DirectoryInfo(@"images\tilePatterns").GetFiles("*.png"))
            {
                imgDict_tilePaterns.Add(
                    fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length),
                    LoadImageFile(fi.FullName));
            }


        }
        private BitmapSource LoadImageFile(string file)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.UriSource = new Uri(file, UriKind.Absolute);
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}
