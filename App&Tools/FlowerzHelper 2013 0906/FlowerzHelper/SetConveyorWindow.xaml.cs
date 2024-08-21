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
using System.Windows.Shapes;

namespace FlowerzHelper
{
    /// <summary>
    /// SetConveyorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetConveyorWindow : Window
    {
        public SetConveyorWindow()
        {
            InitializeComponent();
            IconInit();
        }

        private void IconInit()
        {
            border_fb.Visibility = System.Windows.Visibility.Hidden;
            border_fcb.Visibility = System.Windows.Visibility.Hidden;
            border_tb.Visibility = System.Windows.Visibility.Hidden;

            fb_red.itemData = new Looper.Item(Looper.Item.Type.flowerSingle)
            {
                flowerMainColor = Looper.Item.COLOR_RED
            };
            fb_pink.itemData = new Looper.Item(Looper.Item.Type.flowerSingle)
            {
                flowerMainColor = Looper.Item.COLOR_MAGENTA
            };
            fb_yellow.itemData = new Looper.Item(Looper.Item.Type.flowerSingle)
            {
                flowerMainColor = Looper.Item.COLOR_YELLOW
            };
            fb_white.itemData = new Looper.Item(Looper.Item.Type.flowerSingle)
            {
                flowerMainColor = Looper.Item.COLOR_WHITE
            };
            fb_lightBlue.itemData = new Looper.Item(Looper.Item.Type.flowerSingle)
            {
                flowerMainColor = Looper.Item.COLOR_CYAN
            };
            fb_blue.itemData = new Looper.Item(Looper.Item.Type.flowerSingle)
            {
                flowerMainColor = Looper.Item.COLOR_BLUE
            };
            fb_red.UIRefresh();
            fb_pink.UIRefresh();
            fb_yellow.UIRefresh();
            fb_white.UIRefresh();
            fb_lightBlue.UIRefresh();
            fb_blue.UIRefresh();


            fcb_red.itemData = new Looper.Item(Looper.Item.Type.flowerDouble)
            {
                flowerMainColor = Looper.Item.COLOR_WHITE,
                flowerSubColor = Looper.Item.COLOR_RED
            };
            fcb_pink.itemData = new Looper.Item(Looper.Item.Type.flowerDouble)
            {
                flowerMainColor = Looper.Item.COLOR_WHITE,
                flowerSubColor = Looper.Item.COLOR_MAGENTA
            };
            fcb_yellow.itemData = new Looper.Item(Looper.Item.Type.flowerDouble)
            {
                flowerMainColor = Looper.Item.COLOR_WHITE,
                flowerSubColor = Looper.Item.COLOR_YELLOW
            };
            fcb_white.itemData = new Looper.Item(Looper.Item.Type.flowerDouble)
            {
                flowerMainColor = Looper.Item.COLOR_WHITE,
                flowerSubColor = Looper.Item.COLOR_WHITE
            };
            fcb_lightBlue.itemData = new Looper.Item(Looper.Item.Type.flowerDouble)
            {
                flowerMainColor = Looper.Item.COLOR_WHITE,
                flowerSubColor = Looper.Item.COLOR_CYAN
            };
            fcb_blue.itemData = new Looper.Item(Looper.Item.Type.flowerDouble)
            {
                flowerMainColor = Looper.Item.COLOR_WHITE,
                flowerSubColor = Looper.Item.COLOR_BLUE
            };
            fcb_red.UIRefresh();
            fcb_pink.UIRefresh();
            fcb_yellow.UIRefresh();
            fcb_white.UIRefresh();
            fcb_lightBlue.UIRefresh();
            fcb_blue.UIRefresh();


            tb_butterfly_red.itemData = new Looper.Item(Looper.Item.Type.toolButterfly)
            {
                butterflyColor = Looper.Item.COLOR_RED
            };
            tb_butterfly_pink.itemData = new Looper.Item(Looper.Item.Type.toolButterfly)
            {
                butterflyColor = Looper.Item.COLOR_MAGENTA
            };
            tb_butterfly_yellow.itemData = new Looper.Item(Looper.Item.Type.toolButterfly)
            {
                butterflyColor = Looper.Item.COLOR_YELLOW
            };
            tb_butterfly_white.itemData = new Looper.Item(Looper.Item.Type.toolButterfly)
            {
                butterflyColor = Looper.Item.COLOR_WHITE
            };
            tb_butterfly_lightBlue.itemData = new Looper.Item(Looper.Item.Type.toolButterfly)
            {
                butterflyColor = Looper.Item.COLOR_CYAN
            };
            tb_butterfly_blue.itemData = new Looper.Item(Looper.Item.Type.toolButterfly)
            {
                butterflyColor = Looper.Item.COLOR_BLUE
            };
            tb_butterfly_red.UIRefresh();
            tb_butterfly_pink.UIRefresh();
            tb_butterfly_yellow.UIRefresh();
            tb_butterfly_white.UIRefresh();
            tb_butterfly_lightBlue.UIRefresh();
            tb_butterfly_blue.UIRefresh();

            tb_shovel.itemData = new Looper.Item(Looper.Item.Type.toolShovel);
            tb_shovel.UIRefresh();
        }

        #region select icons
        private Color selectedFBColor = Colors.Black;
        private void fb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Ctrls.SubBlock.FlowerBlock target = (Ctrls.SubBlock.FlowerBlock)sender;
            string iconName = target.Name.ToLower();
            if (iconName.Contains("lightblue")) selectedFBColor = Looper.Item.COLOR_CYAN;
            else if (iconName.Contains("blue")) selectedFBColor = Looper.Item.COLOR_BLUE;
            else if (iconName.Contains("red")) selectedFBColor = Looper.Item.COLOR_RED;
            else if (iconName.Contains("white")) selectedFBColor = Looper.Item.COLOR_WHITE;
            else if (iconName.Contains("yellow")) selectedFBColor = Looper.Item.COLOR_YELLOW;
            else if (iconName.Contains("pink")) selectedFBColor = Looper.Item.COLOR_MAGENTA;

            border_fb.Visibility = System.Windows.Visibility.Visible;
            border_fb.Margin = target.Margin;
        }
        private Color selectedFCBColor = Colors.Black;
        private void fcb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Ctrls.SubBlock.FlowerBlock target = (Ctrls.SubBlock.FlowerBlock)sender;
            string iconName = target.Name.ToLower();
            if (iconName.Contains("lightblue")) selectedFCBColor = Looper.Item.COLOR_CYAN;
            else if (iconName.Contains("blue")) selectedFCBColor = Looper.Item.COLOR_BLUE;
            else if (iconName.Contains("red")) selectedFCBColor = Looper.Item.COLOR_RED;
            else if (iconName.Contains("white")) selectedFCBColor = Looper.Item.COLOR_WHITE;
            else if (iconName.Contains("yellow")) selectedFCBColor = Looper.Item.COLOR_YELLOW;
            else if (iconName.Contains("pink")) selectedFCBColor = Looper.Item.COLOR_MAGENTA;

            border_fcb.Visibility = System.Windows.Visibility.Visible;
            border_fcb.Margin = target.Margin;
        }
        bool selectedShovel = false;
        private Color selectedTBColor = Colors.Black;
        private void tb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Ctrls.SubBlock.ToolBlock target = (Ctrls.SubBlock.ToolBlock)sender;
            string iconName = target.Name.ToLower();
            selectedShovel = false;
            if (iconName.Contains("lightblue")) selectedTBColor = Looper.Item.COLOR_CYAN;
            else if (iconName.Contains("blue")) selectedTBColor = Looper.Item.COLOR_BLUE;
            else if (iconName.Contains("red")) selectedTBColor = Looper.Item.COLOR_RED;
            else if (iconName.Contains("white")) selectedTBColor = Looper.Item.COLOR_WHITE;
            else if (iconName.Contains("yellow")) selectedTBColor = Looper.Item.COLOR_YELLOW;
            else if (iconName.Contains("pink")) selectedTBColor = Looper.Item.COLOR_MAGENTA;
            else if (iconName.Contains("shovel")) selectedShovel = true;

            border_tb.Visibility = System.Windows.Visibility.Visible;
            border_tb.Margin = target.Margin;
        }
        #endregion

        private List<Looper.Item> _conveyorList;
        public List<Looper.Item> conveyorList
        {
            get
            {
                return _conveyorList;
            }
            set
            {
                _conveyorList = value;
                conveyorPanel.conveyorList = _conveyorList;
            }
        }

        private void button_coneyorItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_conveyorList.Count > 0)
            {
                if (((Button)sender).Name.ToLower().Contains("first"))
                {
                    _conveyorList.RemoveAt(0);
                }
                else
                {
                    _conveyorList.RemoveAt(_conveyorList.Count - 1);
                }
                conveyorPanel.PanelReLoad();
            }
        }

        private void button_coneyorItemClear_Click(object sender, RoutedEventArgs e)
        {
            _conveyorList.Clear();
            conveyorPanel.PanelReLoad();
        }

        private void button_coneyorAddFlowerSingle_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFBColor != Colors.Black)
            {
                Looper.Item newItem = new Looper.Item(Looper.Item.Type.flowerSingle)
                {
                    flowerMainColor = selectedFBColor
                };
                _conveyorList.Add(newItem);
                conveyorPanel.PanelReLoad();
            }
        }
        private void button_coneyorAddFlowerDouble_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFBColor != Colors.Black && selectedFCBColor != Colors.Black)
            {
                Looper.Item newItem = new Looper.Item(Looper.Item.Type.flowerDouble)
                {
                    flowerMainColor = selectedFBColor,
                    flowerSubColor = selectedFCBColor
                };
                _conveyorList.Add(newItem);
                conveyorPanel.PanelReLoad();
            }
        }
        private void button_coneyorAddTool_Click(object sender, RoutedEventArgs e)
        {
            if (selectedShovel == true)
            {
                Looper.Item newItem = new Looper.Item(Looper.Item.Type.toolShovel);
                _conveyorList.Add(newItem);
                conveyorPanel.PanelReLoad();
            }
            else if (selectedTBColor != Colors.Black)
            {
                Looper.Item newItem = new Looper.Item(Looper.Item.Type.toolButterfly)
                {
                    butterflyColor = selectedTBColor
                };
                _conveyorList.Add(newItem);
                conveyorPanel.PanelReLoad();
            }
        }

        private void button_ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            //this.Close();
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            //this.Close();
        }
    }
}
