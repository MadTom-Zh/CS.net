using MadTomDev.UI.Class;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace MadTomDev.App.SubWindows
{
    /// <summary>
    /// Interaction logic for WindowNoteColor.xaml
    /// </summary>
    public partial class WindowNoteColor : Window
    {
        public WindowNoteColor()
        {
            InitializeComponent();
        }
        public WindowNoteColor(ColorExpertCore core1,ColorExpertCore core2)
        {
            InitializeComponent();
            colorExpert1.Init(core1);
            colorExpert2.Init(core2);
        }

        public Color Color1
        {
            get => colorExpert1.WorkingColor;
            set => colorExpert1.WorkingColor = value;
        }
        public Color Color2
        {
            get => colorExpert2.WorkingColor;
            set => colorExpert2.WorkingColor = value;
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }
        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

    }
}
