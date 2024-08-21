using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlowerzHelper.Ctrls.SubBlock
{
    /// <summary>
    /// ToolBlock.xaml 的交互逻辑
    /// </summary>
    public partial class ToolBlock : UserControl
    {
        public ToolBlock()
        {
            InitializeComponent();
            InvisibleAll();
        }

        private void InvisibleAll()
        {
            imageShovel.Visibility = System.Windows.Visibility.Hidden;
            polygonButterfly.Visibility = System.Windows.Visibility.Hidden;
        }

        //private Looper.Item _itemData;
        public Looper.Item itemData;

        public void UIRefresh()
        {
            InvisibleAll();

            if (itemData == null)
            {
            }
            else if (itemData.myType == Looper.Item.Type.toolButterfly)
            {
                polygonButterfly.Fill = new SolidColorBrush(itemData.butterflyColor);
                polygonButterfly.Visibility = System.Windows.Visibility.Visible;
            }
            else if (itemData.myType == Looper.Item.Type.toolShovel)
            {
                imageShovel.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
