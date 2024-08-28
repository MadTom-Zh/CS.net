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

namespace MadTomDev.Test.Ctrls
{
    /// <summary>
    /// Interaction logic for Shell32Icon.xaml
    /// </summary>
    public partial class IconInfo : UserControl
    {
        public IconInfo()
        {
            InitializeComponent();
        }

        public BitmapSource IconBig
        {
            set => img32.Source = value;
        }
        public BitmapSource IconSmall
        {
            set => img16.Source = value;
        }
        public string Text
        {
            set => tb.Text = value;
        }
    }
}
