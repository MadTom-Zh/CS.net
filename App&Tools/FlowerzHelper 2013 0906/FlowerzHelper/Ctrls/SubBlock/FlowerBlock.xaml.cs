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
    /// FlowerBlock.xaml 的交互逻辑
    /// </summary>
    public partial class FlowerBlock : UserControl
    {
        public FlowerBlock()
        {
            InitializeComponent();
            InvisibleAll();
        }

        private void InvisibleAll()
        {
            polygonStone.Visibility = System.Windows.Visibility.Hidden;
            polygonMain.Visibility = System.Windows.Visibility.Hidden;
            ellipseSub.Visibility = System.Windows.Visibility.Hidden;
            ellipseSubWFrame.Visibility = System.Windows.Visibility.Hidden;
        }

        //private Looper.Item _itemData;
        public Looper.Item itemData;

        public void UIRefresh()
        {
            InvisibleAll();

            if (itemData == null)
            {
            }
            else if (itemData.myType == Looper.Item.Type.stone)
            {
                polygonStone.Visibility = System.Windows.Visibility.Visible;
            }
            else if (itemData.myType == Looper.Item.Type.flowerSingle)
            {
                polygonMain.Fill = new SolidColorBrush(itemData.flowerMainColor);
                polygonMain.Visibility = System.Windows.Visibility.Visible;
            }
            else if (itemData.myType == Looper.Item.Type.flowerDouble)
            {
                polygonMain.Fill = new SolidColorBrush(itemData.flowerMainColor);
                ellipseSub.Fill = new SolidColorBrush(itemData.flowerSubColor);
                polygonMain.Visibility = System.Windows.Visibility.Visible;
                ellipseSub.Visibility = System.Windows.Visibility.Visible;
                ellipseSubWFrame.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
