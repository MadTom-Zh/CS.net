using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.IO;
using System.Web;
using System.Windows.Media.Imaging;

namespace Gunbook
{
    public class Class_booker
    {
        public class CatalogEntry
        {
            public Class_booker parent;
            private string _DirPath;
            public string DirPath
            {
                set
                {
                    _DirPath = value;
                    button.Click += Button_Click;
                    LoadAllPics();
                }
                get
                {
                    return _DirPath;
                }
            }

            private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                if (parent.CatalogEntry_Clicked != null)
                {
                    parent.CatalogEntry_Clicked(parent, new CatalogEntry_ClickedArgs(this));
                }
            }

            public string Text
            {
                set
                {
                    button.Content = value;
                }
            }
            public Button button = new Button()
            {
                Height = 40,
            };
            public class gunEntry
            {
                public CatalogEntry parent;
                public Image image = new Image();
                public FileInfo imageFileInfo
                {
                    set
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(value.FullName);
                        bitmap.EndInit();
                        image.Source = bitmap;
                        image.Width = bitmap.PixelWidth;
                        image.Height = bitmap.PixelHeight;
                    }
                }
            }


            public List<gunEntry> guns = new List<gunEntry>();
            private void LoadAllPics()
            {
                gunEntry gun;
                DirectoryInfo di = new DirectoryInfo(_DirPath);
                foreach (FileInfo fi in di.GetFiles("*.png"))
                {
                    gun = new gunEntry()
                    {
                        parent = this,
                        imageFileInfo = fi,
                    };
                    guns.Add(gun);
                }


            }
        }


        public List<CatalogEntry> catalog = new List<CatalogEntry>();
        public Class_booker(DirectoryInfo bookDir)
        {
            CatalogEntry cEntry;
            foreach (DirectoryInfo di in bookDir.GetDirectories())
            {
                cEntry = new CatalogEntry()
                {
                    parent = this,
                    Text = HttpUtility.UrlDecode(di.Name),
                    DirPath = di.FullName,
                };
                catalog.Add(cEntry);
            }
        }
        public class CatalogEntry_ClickedArgs : EventArgs
        {
            public CatalogEntry targetCatalogEntry;
            public CatalogEntry_ClickedArgs(CatalogEntry targetCatalogEntry)
            {
                this.targetCatalogEntry = targetCatalogEntry;
            }
        }
        public event EventHandler<CatalogEntry_ClickedArgs> CatalogEntry_Clicked;
    }
}
