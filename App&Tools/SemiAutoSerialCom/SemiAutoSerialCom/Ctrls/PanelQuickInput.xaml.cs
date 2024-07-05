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
    /// Interaction logic for PanelQuickInput.xaml
    /// </summary>
    public partial class PanelQuickInput : UserControl
    {
        public PanelQuickInput()
        {
            InitializeComponent();
        }

        public bool IsTopCoverOn
        {
            set
            {
                Dispatcher.Invoke(() =>
                {
                    rectTopCover.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                });
            }
            get
            {
                bool result = false;
                Dispatcher.Invoke(() =>
                {
                    result = rectTopCover.Visibility == Visibility.Visible;
                });
                return result;
            }
        }

        public string? QuickText
        {
            set
            {
                Dispatcher.Invoke(() =>
                {
                    tb.Text = value;
                });
            }
            get
            {
                string result = "";
                Dispatcher.Invoke(() =>
                {
                    result = tb.Text;
                });
                return result;
            }
        }

        public event Action<PanelQuickInput> btnSendClicked;
        public event Action<PanelQuickInput> btnRemoveClicked;
        private void btnSendClick(object sender, RoutedEventArgs e)
        {
            btnSendClicked?.Invoke(this);
        }
        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            btnRemoveClicked?.Invoke(this);
        }
    }
}
