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
    /// Interaction logic for WindowInputBox.xaml
    /// </summary>
    public partial class WindowInputBox : Window
    {
        public WindowInputBox()
        {
            InitializeComponent();

            tb_input.Focus();
            tb_input.SelectAll();
        }

        public string InputText
        {
            get => tb_input.Text;
            set => tb_input.Text = value;
        }


        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btn_ok_Click(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                btn_cancel_Click(sender, e);
            }
        }
    }
}
