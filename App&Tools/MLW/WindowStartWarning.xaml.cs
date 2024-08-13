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

namespace MLW_Succubus_Storys
{
    /// <summary>
    /// Interaction logic for WindowStartWarning.xaml
    /// </summary>
    public partial class WindowStartWarning : Window
    {
        public WindowStartWarning()
        {
            InitializeComponent();
        }
        public void TrySetLocalization()
        {
            Core.TrySetLocalTx(btnOk, "ok");
            Core.TrySetLocalTx(btnCancel, "cancel");
        }
        public string WarningText
        {
            set
            {
                Dispatcher.BeginInvoke(()=> 
                { tbText.Text = value; });
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
