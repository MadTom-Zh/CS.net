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
    /// Interaction logic for CharValue.xaml
    /// </summary>
    public partial class CharValue : UserControl
    {
        public CharValue()
        {
            InitializeComponent();
        }

        public string TextChar
        { set => tb_char.Text = value; }
        public string TextValue
        { set => tb_value.Text = value; }
        public bool IsUIChar
        {
            set
            {
                if (value)
                {
                    bdr_char.Width = 22;
                    bdr_value.Width = 46;
                }
                else
                {
                    bdr_char.Width = 60;
                    bdr_value.Width = 40;
                }
            }
        }

        private SolidColorBrush backgroundBrush = SystemColors.MenuHighlightBrush;
        public void SetSelected(bool isSelected)
        {
            if (isSelected)
                sPanel.Background = backgroundBrush;
            else
                sPanel.Background = null;
        }
        //public void SetUIChar()
        //{
        //    bdr_char.Width = 22;
        //    bdr_value.Width = 46;
        //}
        //public void SetUIKeyBoard()
        //{
        //    bdr_char.Width = 110;
        //    bdr_value.Width = 40;
        //}
    }
}
