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

namespace MLW_Succubus_Storys.Ctrls
{
    /// <summary>
    /// Interaction logic for TabHeader.xaml
    /// </summary>
    public partial class TabHeader : UserControl
    {
        public TabHeader()
        {
            InitializeComponent();
        }

        public ImageSource Icon
        {
            set
            {
                Dispatcher.BeginInvoke(() =>
                { imgIcon.Source = value; });
            }
        }
        public string Text
        {
            get => tbText.Text;
            set
            {
                Dispatcher.BeginInvoke(() =>
                { tbText.Text = value; });
            }
        }

        public Action<TabHeader> actionClose;
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            actionClose?.Invoke(this);
        }
    }
}
