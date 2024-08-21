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

namespace FlowerzHelper.Ctrls
{
    /// <summary>
    /// ConveyorPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ConveyorPanel : UserControl
    {
        public ConveyorPanel()
        {
            InitializeComponent();
        }

        private List<Looper.Item> _conveyorList;
        public List<Looper.Item> conveyorList
        {
            set
            {
                _conveyorList = value;
                if (_conveyorList.Count == 0)
                {
                    PanelClear();
                }
                else
                {
                    PanelReLoad();
                }
            }
            get
            {
                return _conveyorList;
            }
        }

        public void PanelReLoad()
        {
            // clean and reInit
            PanelClear();
            Looper.Item item;
            for (int i = 0; i < _conveyorList.Count; i++)
            {
                item = _conveyorList[i];
                item.UIFlower = null;
                item.UITool = null;

                if (item.myType == Looper.Item.Type.toolButterfly || item.myType == Looper.Item.Type.toolShovel)
                {
                    CreateTBlock(item);
                }
                else if (item.myType == Looper.Item.Type.flowerSingle || item.myType == Looper.Item.Type.flowerDouble)
                {
                    CreateFBlock(item);
                }
            }

            // layout
            double startTop = 5;
            double startLeft = 5;
            double stepLength = 190 / (_conveyorList.Count + 1);
            for (int i = _conveyorList.Count - 1; i >= 0; i--)
            {
                item = _conveyorList[i];
                if (item.UIFlower != null)
                {
                    item.UIFlower.Margin
                        = new Thickness(startLeft + (i + 1) * stepLength - 15, startTop, 0, 0);
                    gridWindow.Children.Add(item.UIFlower);
                }
                if (item.UITool != null)
                {
                    item.UITool.Margin
                        = new Thickness(startLeft + (i + 1) * stepLength - 15, startTop, 0, 0);
                    gridWindow.Children.Add(item.UITool);
                }
            }
        }
        private void PanelClear()
        {
            gridWindow.Children.RemoveRange(0, gridWindow.Children.Count);
        }
        private SubBlock.FlowerBlock CreateFBlock(Looper.Item itemData)
        {
            SubBlock.FlowerBlock result = new SubBlock.FlowerBlock();
            result.itemData = itemData;
            result.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            result.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            result.UIRefresh();
            itemData.UIFlower = result;
            return result;
        }
        private SubBlock.ToolBlock CreateTBlock(Looper.Item itemData)
        {
            SubBlock.ToolBlock result = new SubBlock.ToolBlock();
            result.itemData = itemData;
            result.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            result.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            result.UIRefresh();
            itemData.UITool = result;
            return result;
        }
    }
}
