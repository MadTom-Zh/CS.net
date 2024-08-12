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
    /// Interaction logic for ButtonCom.xaml
    /// </summary>
    public partial class ButtonCom : UserControl
    {
        public ButtonCom()
        {
            InitializeComponent();
        }

        public string Text
        {
            get => tb.Text;
            set
            {
                Dispatcher.BeginInvoke(()=> 
                {
                    tb.Text = value;
                });
            }
        }
    }
}
