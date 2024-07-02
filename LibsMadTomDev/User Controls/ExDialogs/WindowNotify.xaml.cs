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
using System.Windows.Shapes;

namespace MadTomDev.UI
{
    /// <summary>
    /// Interaction logic for WindowNotify.xaml
    /// </summary>
    public partial class WindowNotify : Window
    {
        public WindowNotify()
        {
            InitializeComponent();
        }
        public new string Title
        {
            get => base.Title;
            set => Dispatcher.Invoke(() => { base.Title = value; });
        }

        public string Text
        {
            get => tb.Text;
            set => Dispatcher.Invoke(() => { tb.Text = value; });
        }
    }
}
