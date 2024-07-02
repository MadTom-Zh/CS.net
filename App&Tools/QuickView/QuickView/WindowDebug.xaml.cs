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

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for WindowDebug.xaml
    /// </summary>
    public partial class WindowDebug : Window
    {
        public WindowDebug()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            tb.Clear();
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tb.LineCount > tb.MaxLines - 10)
            {
                Button_Click(null, null);
            }
        }

        public void AppendLine(string tx)
        {
            tb.AppendText(Environment.NewLine + tx);
            tb.ScrollToEnd();
        }
        public void AppendText(string tx)
        {
            tb.AppendText(tx);
            tb.ScrollToEnd();
        }
    }
}
