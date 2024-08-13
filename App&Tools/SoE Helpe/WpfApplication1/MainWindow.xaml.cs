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

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region set mother panel size
        private void uiSetPanelSize_Loaded(object sender, RoutedEventArgs e)
        {
            uiSetPanelSize.PRowsChanged += uiSetPanelSize_PRowsChanged;
            uiSetPanelSize.PColsChanged += uiSetPanelSize_PColsChanged;
        }
        void uiSetPanelSize_PColsChanged(object sender, EventArgs e)
        {
            uiMotherPanel.ReInit(uiSetPanelSize.PRows, uiSetPanelSize.PCols);
            Check2Start();
        }
        void uiSetPanelSize_PRowsChanged(object sender, EventArgs e)
        {
            uiMotherPanel.ReInit(uiSetPanelSize.PRows, uiSetPanelSize.PCols);
            Check2Start();
        }
        #endregion

        #region set items
        private void uiSetItems_Loaded(object sender, RoutedEventArgs e)
        {
            uiSetItems.InitItems();
            uiSetItems.ItemClicked += uiSetItems_ItemClicked;
        }
        void uiSetItems_ItemClicked(object sender, EventArgs e)
        {
            UI.UIBlockItem target = (UI.UIBlockItem)sender;
            uiChosenItems.AddItem(target.MyType);
        }
        private void Window_KeyDown_1(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D1:
                case Key.NumPad1:
                    uiChosenItems.AddItem(uiSetItems.ItemList[0].MyType);
                    break;
                case Key.D2:
                case Key.NumPad2:
                    uiChosenItems.AddItem(uiSetItems.ItemList[1].MyType);
                    break;
                case Key.D3:
                case Key.NumPad3:
                    uiChosenItems.AddItem(uiSetItems.ItemList[2].MyType);
                    break;
                case Key.D4:
                case Key.NumPad4:
                    uiChosenItems.AddItem(uiSetItems.ItemList[3].MyType);
                    break;
                case Key.D5:
                case Key.NumPad5:
                    uiChosenItems.AddItem(uiSetItems.ItemList[4].MyType);
                    break;
                case Key.D6:
                case Key.NumPad6:
                    uiChosenItems.AddItem(uiSetItems.ItemList[5].MyType);
                    break;
                case Key.D7:
                case Key.NumPad7:
                    uiChosenItems.AddItem(uiSetItems.ItemList[6].MyType);
                    break;
            }
        }
        #endregion

        #region choose item
        private void uiChosenItems_Loaded(object sender, RoutedEventArgs e)
        {
            uiChosenItems.ItemAdded += uiChosenItems_ItemAdded;
            uiChosenItems.ItemCleaned += uiChosenItems_ItemCleaned;
            uiChosenItems.ItemRemoved += uiChosenItems_ItemRemoved;
        }
        void uiChosenItems_ItemRemoved(object sender, EventArgs e)
        {
            uiMotherPanel.ReDraw();
            Check2Start();
        }
        void uiChosenItems_ItemCleaned(object sender, EventArgs e)
        {
            uiMotherPanel.ReDraw();
            Check2Start();
        }
        void uiChosenItems_ItemAdded(object sender, EventArgs e)
        {
            uiMotherPanel.ReDraw();
            Check2Start();
        }
        #endregion

        private void Check2Start()
        {
            int vancans = uiMotherPanel.Mtx.Data.Length * uiMotherPanel.Mtx.Data[0].Length;
            textBox_panelVacans.Text
                = "" + vancans;
            int blocks = 0;
            foreach (UI.UIBlockItem citems in uiChosenItems.ItemList)
            {
                blocks += citems.Mtx.Blocks;
            }
            textBox_spareBlocks.Text
                 = "" + blocks;

            if (vancans > blocks)
            {
                textBox_spareBlocks.Background = new SolidColorBrush(Colors.Yellow);
            }
            else if (vancans < blocks)
            {
                textBox_spareBlocks.Background = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                // start
                ClassJigsaw helper = new ClassJigsaw(uiMotherPanel.Mtx, uiChosenItems.ItemDataList);
                if (helper.Start() == true)
                {
                    textBox_spareBlocks.Background = new SolidColorBrush(Colors.LightGreen);
                    uiMotherPanel.SetItems(helper.ResultInfoList);
                }
                else
                {
                    textBox_spareBlocks.Background = new SolidColorBrush(Colors.OrangeRed);
                }
            }
        }

        private void button_help_Click(object sender, RoutedEventArgs e)
        {
            (new AboutBox()).ShowDialog();
        }
    }
}
