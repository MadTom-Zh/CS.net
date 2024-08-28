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
    /// Interaction logic for ColorNameARGB.xaml
    /// </summary>
    public partial class ColorNameARGB : UserControl
    {
        public ColorNameARGB()
        {
            InitializeComponent();
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, new RoutedEventArgs());
        }
        public event RoutedEventHandler Click;

        public string Text
        {
            set => tb.Text = value;
            get => tb.Text;
        }
        public Brush Brush
        {
            set => bdrColor.Background = value;
        }
        public Color Color
        {
            get
            {
                if (bdrColor.Background == null)
                    return Colors.Transparent;
                else
                    return ((SolidColorBrush)bdrColor.Background).Color;
            }
        }
    }
}
