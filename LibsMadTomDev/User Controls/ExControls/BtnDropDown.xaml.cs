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

namespace MadTomDev.UI
{
    /// <summary>
    /// Interaction logic for BtnDropDown.xaml
    /// </summary>
    public partial class BtnDropDown : UserControl
    {
        public BtnDropDown()
        {
            InitializeComponent();
            btn_dropDown.Content = GraphResource.PathArrowSmallRight;
        }

        public string Text
        {
            get => tb.Text;
            set
            {
                tb.Text = value;
            }
        }

        public async void GetBtnWidth()
        {
            int countOut = 20;
            while (--countOut >0)
            {
                if (this.ActualWidth > 0)
                {
                    _BtnWidth = this.ActualWidth;
                    break;
                }
                else
                    await Task.Delay(20);
            }
        }
        private double _BtnWidth;
        public double BtnWidth
        { get => _BtnWidth; }

        public Action<BtnDropDown> ActionClick, ActionDropDownClick;

        private void btn_main_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ActionClick?.Invoke(this);
        }

        private void btn_dropDown_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            ActionDropDownClick?.Invoke(this);
        }
        public void SetDropDownArrow(bool isDownOrRight)
        {
            if (isDownOrRight)
                btn_dropDown.Content = GraphResource.PathArrowSmallDown;
            else
                btn_dropDown.Content = GraphResource.PathArrowSmallRight;
        }
    }
}
