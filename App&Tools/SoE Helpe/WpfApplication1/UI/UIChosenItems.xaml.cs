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
    /// UIChosenItems.xaml 的交互逻辑
    /// </summary>
    public partial class UIChosenItems : UserControl
    {
        public UIChosenItems()
        {
            InitializeComponent();
        }

        public List<UIBlockItem> ItemList
            = new List<UIBlockItem>();
        public List<ClassSoEMatrix> ItemDataList
        {
            get
            {
                List<ClassSoEMatrix> result = new List<ClassSoEMatrix>();
                for (int i = 0; i < ItemList.Count; i++)
                {
                    result.Add(ItemList[i].Mtx);
                }
                return result;
            }
        }
        public void ClearItems()
        {
            stackPanel_selectedItems.Children.Clear();
            ItemList.Clear();

            if (ItemCleaned != null)
            {
                ItemCleaned(this, new EventArgs());
            }
        }

        public void AddItem(UIBlockItem.ItemType itemType)
        {
            UIBlockItem newItem = new UIBlockItem();
            newItem.MouseLeftButtonUp += newItem_MouseLeftButtonUp;
            newItem.Init(itemType);
            stackPanel_selectedItems.Children.Add(newItem);
            ItemList.Add(newItem);

            if (ItemAdded != null)
            {
                ItemAdded(this, new EventArgs());
            }
        }

        void newItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UIBlockItem target = (UIBlockItem)sender;
            stackPanel_selectedItems.Children.Remove(target);
            ItemList.Remove(target);

            if (ItemRemoved != null)
            {
                ItemRemoved(target, new EventArgs());
            }
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            ClearItems();
        }

        public event EventHandler ItemAdded;
        public event EventHandler ItemRemoved;
        public event EventHandler ItemCleaned;
    }
}
