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
    /// Interaction logic for BtnSuccuEntry.xaml
    /// </summary>
    public partial class BtnSuccuEntry : UserControl
    {
        public BtnSuccuEntry()
        {
            InitializeComponent();
        }

        public ImageSource IconSuccu
        {
            set
            {
                Dispatcher.BeginInvoke(() =>
                { imgSucIcon.Source = value; });
            }
        }
        public string NameSuccu
        {
            get => tbSucName.Text;
            set
            {
                Dispatcher.BeginInvoke(() =>
                { tbSucName.Text = value; });
            }
        }
        public ImageSource IconMtl1
        {
            set
            {
                Dispatcher.BeginInvoke(() =>
                { imgMtl1.Source = value; });
            }
        }
        public ImageSource IconMtl2
        {
            set
            {
                Dispatcher.BeginInvoke(() =>
                { imgMtl2.Source = value; });
            }
        }
        public ImageSource IconMtl3
        {
            set
            {
                Dispatcher.BeginInvoke(() =>
                { imgMtl3.Source = value; });
            }
        }
        public Action<BtnSuccuEntry> actionEnter;

        private void Grid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            actionEnter?.Invoke(this);
        }
    }
}
