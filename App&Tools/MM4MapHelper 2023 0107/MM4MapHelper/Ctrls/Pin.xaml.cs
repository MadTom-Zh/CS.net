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

namespace MadTomDev.App.Ctrls
{
    /// <summary>
    /// Interaction logic for Pin.xaml
    /// </summary>
    public partial class Pin : UserControl
    {
        public Pin()
        {
            InitializeComponent();
        }

        public bool IsHighLighted
        {
            get => img_c2.Visibility == Visibility.Visible;
            set
            {
                if (value)
                {
                    img_c2.Visibility = Visibility.Visible;
                    img_c1.Visibility = Visibility.Collapsed;
                }
                else
                {
                    img_c1.Visibility = Visibility.Visible;
                    img_c2.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
