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

using System.Threading;

namespace FlowerzHelper.Ctrls
{
    /// <summary>
    /// CentralPanel.xaml 的交互逻辑
    /// </summary>
    public partial class CentralPanel : UserControl
    {
        public CentralPanel()
        {
            InitializeComponent();

            //looper.LoopingPorgressChanged += looper_LoopingPorgressChanged;
            looper.LoopingPorgressStart += looper_LoopingPorgressStart;
            looper.LoopingPorgressEnd += looper_LoopingPorgressEnd;
            looper.LoopingPorgressAbort += looper_LoopingPorgressAbort;

            gardenInit.CallPut += gardenInit_CallPut;

            IconInit();
        }


        public static double HEIGHT = 950;
        public static double WIDTH = 900;

        private Looper looper = new Looper();

        private void button_setGSize_Click(object sender, RoutedEventArgs e)
        {
            looper.gardenSize = new Looper.IntSize(int.Parse(textBox_GWidth.Text), int.Parse(textBox_GHeight.Text));
            gardenInit.dataMatrix = looper.gardenItemMatrix;
            CheckEnableLoopButton();
        }


        #region coneyor functions
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

        private void button_coneyorItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (looper.conveyor.Count > 0)
            {
                if (((Button)sender).Name.ToLower().Contains("first"))
                {
                    looper.conveyor.RemoveAt(0);
                }
                else
                {
                    looper.conveyor.RemoveAt(looper.conveyor.Count - 1);
                }
                conveyorPanel.PanelReLoad();
                CheckEnableLoopButton();
            }
        }

        private void button_coneyorAddFlowerSingle_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFBColor != Colors.Black)
            {
                Looper.Item newItem = new Looper.Item(Looper.Item.Type.flowerSingle)
                {
                    flowerMainColor = selectedFBColor
                };
                looper.conveyor.Add(newItem);
                conveyorPanel.conveyorList = looper.conveyor;
                //conveyorPanel.PanelReLoad();
                CheckEnableLoopButton();
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
                looper.conveyor.Add(newItem);
                conveyorPanel.conveyorList = looper.conveyor;
                //conveyorPanel.PanelReLoad();
                CheckEnableLoopButton();
            }
        }
        private void button_coneyorAddTool_Click(object sender, RoutedEventArgs e)
        {
            if (selectedShovel == true)
            {
                Looper.Item newItem = new Looper.Item(Looper.Item.Type.toolShovel);
                looper.conveyor.Add(newItem);
                conveyorPanel.conveyorList = looper.conveyor;
                //conveyorPanel.PanelReLoad();
            }
            else if (selectedTBColor != Colors.Black)
            {
                Looper.Item newItem = new Looper.Item(Looper.Item.Type.toolButterfly)
                {
                    butterflyColor = selectedTBColor
                };
                looper.conveyor.Add(newItem);
                conveyorPanel.conveyorList = looper.conveyor;
                //conveyorPanel.PanelReLoad();
                CheckEnableLoopButton();
            }
        }
        #endregion


        //private void button_setConveyor_Click(object sender, RoutedEventArgs e)
        //{
        //    SetConveyorWindow cWin = new SetConveyorWindow();

        //    List<Looper.Item> conveyorClone = new List<Looper.Item>();
        //    conveyorClone.AddRange(looper.conveyor);

        //    cWin.conveyorList = conveyorClone;
        //    cWin.Closed += cWin_Closed;
        //    cWin.ShowDialog();
        //}
        //void cWin_Closed(object sender, EventArgs e)
        //{
        //    SetConveyorWindow target = (SetConveyorWindow)sender;
        //    if (target.DialogResult == true)
        //    {
        //        looper.conveyor = target.conveyorList;
        //        conveyorPanel.conveyorList = looper.conveyor;
        //        conveyorPanel.PanelReLoad();
        //        CheckEnableLoopButton();
        //    }
        //}
        private void CheckEnableLoopButton()
        {
            if (looper.conveyor.Count > 0
                && looper.gardenSize.Width > 0
                && looper.gardenSize.Height > 0)
            {
                button_loop.Content = "  Loop";
                button_loop.IsEnabled = true;
            }
            else
            {
                button_loop.Content = "  Impossible";
                button_loop.IsEnabled = false;
            }
        }


        void gardenInit_CallPut(object sender, EventArgs e)
        {
            if (looper.conveyor.Count > 0)
            {
                Looper.Item takenItem = looper.conveyor[0];
                looper.conveyor.RemoveAt(0);
                Looper.Item targetItem = looper.gardenItemMatrix[gardenInit.PutPoint.Y][gardenInit.PutPoint.X];
                if (targetItem != null)
                {
                    if (targetItem.myType == Looper.Item.Type.flowerSingle
                        || targetItem.myType == Looper.Item.Type.flowerDouble)
                    {
                        if (takenItem.myType == Looper.Item.Type.toolButterfly)
                        {
                            targetItem.myType = Looper.Item.Type.flowerSingle;
                            targetItem.flowerMainColor = takenItem.butterflyColor;
                        }
                        else if (takenItem.myType == Looper.Item.Type.toolShovel)
                        {
                            looper.conveyor.Insert(0, targetItem.Clone(true));
                            looper.gardenItemMatrix[gardenInit.PutPoint.Y][gardenInit.PutPoint.X] = null;
                        }
                        else
                        {
                            looper.conveyor.Insert(0, takenItem);
                        }
                    }
                    else
                    {
                        looper.conveyor.Insert(0, takenItem);
                    }
                }
                else
                {
                    if (takenItem.myType == Looper.Item.Type.flowerSingle
                            || takenItem.myType == Looper.Item.Type.flowerDouble)
                    {
                        looper.gardenItemMatrix[gardenInit.PutPoint.Y][gardenInit.PutPoint.X]
                            = takenItem.Clone(true);
                    }
                    else if (takenItem.myType == Looper.Item.Type.toolShovel)
                    {
                    }
                    else
                    {
                        looper.conveyor.Insert(0, takenItem);
                    }
                }
            }
            looper.Loop_CheckAndShapeMatrix();
            conveyorPanel.conveyorList = looper.conveyor;
            gardenInit.dataMatrix = looper.gardenItemMatrix;
            CheckEnableLoopButton();
        }

        Thread threadLooping;
        Thread threadLoopingLabel;
        private void button_loop_Click(object sender, RoutedEventArgs e)
        {
            //new Thread(() =>
            //{
            //    this.Dispatcher.Invoke(new Action(() =>
            //    {
            //        looper.Loop();
            //        looper.loopingProgress
            //    }));
            //}).Start();

            ResultClear();
            button_loopAbort.IsEnabled = true;
            button_loop.IsEnabled = false;

            threadLooping = new Thread(new ThreadStart(ThreadLoop));
            threadLoopingLabel = new Thread(new ThreadStart(ThreadLoopLabel));
            ThreadWorking = true;
            threadLooping.Start();
            threadLoopingLabel.Start();
        }

        private bool ThreadWorking = true;
        private delegate void DelegateSetLoopingProgressLabel();
        void ThreadLoop()
        {
            looper.Loop();
        }
        void ThreadLoopLabel()
        {
            while (ThreadWorking)
            {
                Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.SystemIdle,
                    new DelegateSetLoopingProgressLabel(SetLoopingProgressButtonLabel)
                    );
            }
            Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.SystemIdle,
                new DelegateSetLoopingProgressLabel(SetLoopingProgressButtonLabel)
                );
        }

        int looper_LoopingPorgressChanged_bar = 0; // 0 1 2
        void SetLoopingProgressButtonLabel()
        {
            if (looper.isLoopingAbort)
            {
                button_loop.Content = "  Loop";
                button_loop.IsEnabled = true;
                return;
            }
            else if (looper.loopingProgress.Count == 0)
            {
                button_loop.Content = "  Done";
                button_loop.IsEnabled = true;
                return;
            }

            try
            {
                string label = "  Looping..";
                looper_LoopingPorgressChanged_bar++;
                looper_LoopingPorgressChanged_bar %= 3;
                if (looper_LoopingPorgressChanged_bar == 0) label += "-";
                else if (looper_LoopingPorgressChanged_bar == 1) label += "\\";
                else if (looper_LoopingPorgressChanged_bar == 2) label += "/";

                label += "..";

                int curValue;
                for (int i = 0; i < looper.loopingProgress.Count; i++)
                {
                    if (i != 0) label += ",";
                    curValue = looper.loopingProgress[i];
                    if (curValue > 999) curValue = 999;
                    label += "" + curValue.ToString("000");
                }

                button_loop.Content = label;
                button_loop.UpdateLayout();
            }
            catch { }
        }

        private delegate void DelegateResultShow();
        void looper_LoopingPorgressEnd(object sender, EventArgs e)
        {
            ThreadWorking = false;

            Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.SystemIdle,
                new DelegateResultShow(ResultShow)
                );
        }
        void looper_LoopingPorgressStart(object sender, EventArgs e)
        {
            ThreadWorking = true;
        }
        void looper_LoopingPorgressAbort(object sender, EventArgs e)
        {
            ThreadWorking = false;
        }

        private void button_loopAbort_Click(object sender, RoutedEventArgs e)
        {
            looper.LoopAbort();
            button_loop.IsEnabled = true;
        }

        //void looper_LoopingPorgressChanged(object sender, EventArgs e)
        //{

        //}

        List<GardenPanel> gPanelList_step = new List<GardenPanel>();
        List<GardenPanel> gPanelList_result = new List<GardenPanel>();
        private void ResultShow()
        {
            GardenPanel gp;
            Looper.Result result;
            for (int i = 0; i < looper.finalResult.Count; i++)
            {
                result = looper.finalResult[i];

                gp = new GardenPanel();
                gp.Focusable = false;
                gp.CanUserCtrl = false;
                gp.dataMatrix = Looper.CloneMatrix(looper.gardenItemMatrix);
                gp.SetXMark(result.firstPosition);

                gp.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                gp.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                gp.Margin = new Thickness(0, i * 300, 0, 0);

                gPanelList_step.Add(gp);
                gridStepOne.Children.Add(gp);

                gp = new GardenPanel();
                gp.Focusable = false;
                gp.CanUserCtrl = false;
                gp.dataMatrix = Looper.CloneMatrix(result.resultMap);

                gp.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                gp.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                gp.Margin = new Thickness(0, i * 300, 0, 0);

                gPanelList_result.Add(gp);
                gridResult.Children.Add(gp);
            }
        }
        private void ResultClear()
        {
            for (int i = gPanelList_step.Count - 1; i >= 0; i--)
            {
                gridStepOne.Children.Remove(gPanelList_step[i]);
                gPanelList_step.RemoveAt(i);
            }
            for (int i = gPanelList_result.Count - 1; i >= 0; i--)
            {
                gridResult.Children.Remove(gPanelList_result[i]);
                gPanelList_result.RemoveAt(i);
            }
        }

        private void button_help_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWin = new AboutWindow();
            aboutWin.ShowDialog();
        }
    }
}
