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

using System.IO;

namespace Gunbook
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadPictures();
            ReSize_wrapPanel_main(new Size(scrollViewer_main.Width, scrollViewer_main.Height));
        }

        private void scrollViewer_main_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReSize_wrapPanel_main(e.NewSize);
        }
        private void ReSize_wrapPanel_main(Size parentSize)
        {
            double newWidth = scrollViewer_main.ActualWidth - 30;
            wrapPanel_main.Width = newWidth;
            int items = wrapPanel_main.Children.Count;

            if (items == 0)
            {
                wrapPanel_main.Height = 30;
            }
            else
            {
                Image ctrl = (Image)wrapPanel_main.Children[0];
                int rowItemCount = (int)(newWidth / ctrl.Width);
                int rowCount = wrapPanel_main.Children.Count / rowItemCount + ((wrapPanel_main.Children.Count % rowItemCount) > 0 ? 1 : 0);
                wrapPanel_main.Height = (ctrl.Height * rowCount) + 30;
            }
        }


        private Class_booker booker;
        private void ReloadPictures()
        {
            booker = new Class_booker(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\Book"));
            booker.CatalogEntry_Clicked += Booker_CatalogEntry_Clicked;

            stackPanel_catalog.Children.Clear();
            foreach (Class_booker.CatalogEntry ce in booker.catalog)
            {
                stackPanel_catalog.Children.Add(ce.button);
            }
        }

        private void Booker_CatalogEntry_Clicked(object sender, Class_booker.CatalogEntry_ClickedArgs e)
        {
            wrapPanel_main.Children.Clear();
            foreach (Class_booker.CatalogEntry.gunEntry ge in e.targetCatalogEntry.guns)
            {
                wrapPanel_main.Children.Add(ge.image);
            }



            ReSize_wrapPanel_main(new Size(scrollViewer_main.Width, scrollViewer_main.Height));
        }
    }
}
