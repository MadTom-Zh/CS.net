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
    /// Interaction logic for ChatMsgItem.xaml
    /// </summary>
    public partial class ChatMsgItem : UserControl
    {
        public ChatMsgItem()
        {
            InitializeComponent();
        }

        public void SetLeft(BitmapImage icon, string charName, string msg)
        {
            imgLeft.Source = icon;
            imgRight.Source = null;
            tbChar.TextAlignment = TextAlignment.Left;
            tbMsg.TextAlignment = TextAlignment.Left;
            tbChar.Text = charName;
            tbMsg.Text = msg;
        }
        public void SetRight(BitmapImage icon, string charName, string msg)
        {
            imgLeft.Source = null;
            imgRight.Source =  icon;
            tbChar.TextAlignment = TextAlignment.Right;
            tbMsg.TextAlignment = TextAlignment.Right;
            tbChar.Text = charName;
            tbMsg.Text = msg;
        }
    }
}
