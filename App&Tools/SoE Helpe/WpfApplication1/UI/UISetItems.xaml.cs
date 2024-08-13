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

namespace WpfApplication1.UI
{
    /// <summary>
    /// UISetItems.xaml 的交互逻辑
    /// </summary>
    public partial class UISetItems : UserControl
    {
        public UISetItems()
        {
            InitializeComponent();
        }

        public List<UIBlockItem> ItemList
            = new List<UIBlockItem>();
        public void InitItems()
        {
            MainGrid.Children.Clear();
            int baseLeft = 10;
            //int baseTop = 10;
            int curLeft = baseLeft;

            ItemList.Clear();

            UIBlockItem item = new UIBlockItem();
            item.MouseLeftButtonUp += item_MouseLeftButtonUp;
            item.Init(UIBlockItem.ItemType.LeftStairs);
            item.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            item.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            item.Margin = new Thickness(curLeft, (MainGrid.Height - item.Height) / 2, 0, 0);
            curLeft += (int)item.Width + 5;
            MainGrid.Children.Add(item);
            ItemList.Add(item);

            item = new UIBlockItem();
            item.MouseLeftButtonUp += item_MouseLeftButtonUp;
            item.Init(UIBlockItem.ItemType.RightStairs);
            item.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            item.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            item.Margin = new Thickness(curLeft, (MainGrid.Height - item.Height) / 2, 0, 0);
            curLeft += (int)item.Width + 5;
            MainGrid.Children.Add(item);
            ItemList.Add(item);

            item = new UIBlockItem();
            item.MouseLeftButtonUp += item_MouseLeftButtonUp;
            item.Init(UIBlockItem.ItemType.Pyra);
            item.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            item.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            item.Margin = new Thickness(curLeft, (MainGrid.Height - item.Height) / 2, 0, 0);
            curLeft += (int)item.Width + 5;
            MainGrid.Children.Add(item);
            ItemList.Add(item);

            item = new UIBlockItem();
            item.MouseLeftButtonUp += item_MouseLeftButtonUp;
            item.Init(UIBlockItem.ItemType.LeftPin);
            item.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            item.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            item.Margin = new Thickness(curLeft, (MainGrid.Height - item.Height) / 2, 0, 0);
            curLeft += (int)item.Width + 5;
            MainGrid.Children.Add(item);
            ItemList.Add(item);

            item = new UIBlockItem();
            item.MouseLeftButtonUp += item_MouseLeftButtonUp;
            item.Init(UIBlockItem.ItemType.RightPin);
            item.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            item.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            item.Margin = new Thickness(curLeft, (MainGrid.Height - item.Height) / 2, 0, 0);
            curLeft += (int)item.Width + 5;
            MainGrid.Children.Add(item);
            ItemList.Add(item);

            item = new UIBlockItem();
            item.MouseLeftButtonUp += item_MouseLeftButtonUp;
            item.Init(UIBlockItem.ItemType.BigBlock);
            item.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            item.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            item.Margin = new Thickness(curLeft, (MainGrid.Height - item.Height) / 2, 0, 0);
            curLeft += (int)item.Width + 5;
            MainGrid.Children.Add(item);
            ItemList.Add(item);

            item = new UIBlockItem();
            item.MouseLeftButtonUp += item_MouseLeftButtonUp;
            item.Init(UIBlockItem.ItemType.Bar);
            item.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            item.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            item.Margin = new Thickness(curLeft, (MainGrid.Height - item.Height) / 2, 0, 0);
            curLeft += (int)item.Width + 5;
            MainGrid.Children.Add(item);
            ItemList.Add(item);

            MainGrid.Width = curLeft;
        }

        void item_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ItemClicked != null)
            {
                ItemClicked(sender, e);
            }
        }
        public event EventHandler ItemClicked;
    }
}
