using MadTomDev.App.Classes;
using MadTomDev.App.Ctrls;
using MadTomDev.App.VMs;
using MadTomDev.Common;
using MadTomDev.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for WindowCalculate.xaml
    /// </summary>
    public partial class WindowCalculator : Window
    {
        public WindowCalculator()
        {
            InitializeComponent();
        }
        private Core core = Core.Instance;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbv_sceneChain.Text = core.SceneFullPath;
            //grid_pre.ItemsSource = core.calculatedProcessingCombings;
        }


        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (tabMain.SelectedIndex)
            {
                default:
                case 0:
                    ClearQuantity();
                    graphAlpha_clear();
                    break;
                case 1:
                    // quantify
                    if (selectedProcessingCombing == null)
                    {
                        // clear
                        ClearQuantity();
                    }
                    else
                    {
                        // load
                        if (sp_qtP.Children.Count == 0)
                        {
                            FillQuantityPanel();
                        }
                        QuantityRefreshInfo();
                    }
                    break;
                case 2:
                    // graph alpha
                    if (selectedProcessingCombing == null)
                    {
                        graphAlpha_clear();
                    }
                    else
                    {
                        if (canvas_graphAlpha.Children.Count == 0)
                        {
                            if (fga != null)
                            {
                                fga.PanelTitleMouseDown -= Fga_PanelTitleMouseDown;
                                fga.PanelResizeHandleMouseDown -= Fga_PanelResizeHandleMouseDown;
                                //fga.Dispose();
                            }
                            fga = new FlowGraphAlpha(ref canvas_graphAlpha, selectedProcessingCombing.searchResult);
                            fga.PanelTitleMouseDown += Fga_PanelTitleMouseDown;
                            fga.PanelResizeHandleMouseDown += Fga_PanelResizeHandleMouseDown;
                        }
                        fga.UpdateQuantities();
                        fga.UpdateAllLinkLines();
                    }
                    break;
            }
        }


        #region window, mouse action

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (tabMain.SelectedIndex == 2)
            {
                if (movingOrResizingPanel != null)
                {
                    Point curMousePoint = Mouse.GetPosition(grid_graphAlpha);
                    if (movingOrResizing)
                    {
                        // move frame
                        MovePanelFrame(
                            panelFrameRectPositionStart.X + curMousePoint.X - panelMousePointStart.X,
                            panelFrameRectPositionStart.Y + curMousePoint.Y - panelMousePointStart.Y);
                    }
                    else
                    {
                        // resize frame
                        ResizePanelFrame(
                            curMousePoint.X - panelMousePointStart.X,
                            curMousePoint.Y - panelMousePointStart.Y);
                    }
                }
                else if (graphAlpha_moving)
                {
                    // moving graph alpha
                    Point curMousePoint = Mouse.GetPosition(grid_graphAlpha);
                    sv_graphAlpha.ScrollToHorizontalOffset(graphAlpha_movingStartScrollOffset.X + graphAlpha_movingStartMousePoint.X - curMousePoint.X);
                    sv_graphAlpha.ScrollToVerticalOffset(graphAlpha_movingStartScrollOffset.Y + graphAlpha_movingStartMousePoint.Y - curMousePoint.Y);
                }
            }
        }

        private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (tabMain.SelectedIndex == 2)
            {
                Point curMousePoint = Mouse.GetPosition(grid_graphAlpha);
                MoveHandleEnd(
                    curMousePoint.X - panelMousePointStart.X,
                    curMousePoint.Y - panelMousePointStart.Y);

                graphAlpha_moving = false;
                Cursor = Cursors.Arrow;
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (tabMain.SelectedIndex == 2)
            {
                Point curMousePoint = Mouse.GetPosition(grid_graphAlpha);
                MoveHandleEnd(
                    curMousePoint.X - panelMousePointStart.X,
                    curMousePoint.Y - panelMousePointStart.Y);
            }
        }
        #endregion


        #region precondition

        DataGridItemModelProcessingCombing? selectedProcessingCombing = null;

        private void tiq_targetProduct_IconDoubleClicked(object sender, EventArgs e)
        {
            WindowSelectThings selectWin = new WindowSelectThings()
            {
                Owner = this,
                DataGridSource = core.calculationThings,
            };
            if (selectWin.ShowDialog() == true
                && selectWin.SelectedThingIdList != null
                && selectWin.SelectedThingIdList.Count == 1)
            {
                //ClearAll();
                tiq_targetProduct.Item = core.FindThing(selectWin.SelectedThingIdList[0]);

                btn_search_Click(sender, new RoutedEventArgs());
            }
        }

        private Calculator calculator;
        private void btn_search_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
            if (tiq_targetProduct.Item == null)
            {
                MessageBox.Show(this, "No product selected.", "Stop", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            calculator = core.NewCalculate(tiq_targetProduct.Item);

            // fill pre wrapPanels
            ProcessingChains? pc = calculator.calculatedProcessingChains;
            FillPreWPanel(ref wp_preA, pc.Accessories.ToList<Things.ThingBase>(), Calculator.IngrediTypes.Accessory);
            FillPreWPanel(ref wp_preI, pc.Inputs.ToList<Things.ThingBase>(), Calculator.IngrediTypes.Input);
            FillPreWPanel(ref wp_preO, pc.Outputs.ToList<Things.ThingBase>(), Calculator.IngrediTypes.Output);
            FillPreWPanel(ref wp_preC, pc.EssentialChannels.Select(a => (Things.ThingBase)a).ToList(), Calculator.IngrediTypes.Channel);
            FillPreWPanel(ref wp_preP, pc.Processings.ToList<Things.ThingBase>(), Calculator.IngrediTypes.Procedure);

            // fill combination data grid
            dataGrid_pre.ItemsSource = calculator.calculatedProcessingCombings;





            this.Title = $"CC: {tiq_targetProduct.Item.name}";
        }
        private void ClearAll()
        {
            selectedProcessingCombing = null;

            // clear pre tab
            ClearPreWPanel(ref wp_preA);
            ClearPreWPanel(ref wp_preI);
            ClearPreWPanel(ref wp_preO);
            ClearPreWPanel(ref wp_preC);
            ClearPreWPanel(ref wp_preP);

            dataGrid_pre.ItemsSource = null;

            // clear quantify tab
            ClearQuantity();

            // clear graph tab
            graphAlpha_clear();

            this.Title = "CC: Calculator";
        }

        private void ClearPreWPanel(ref WrapPanel wPanel)
        {
            UIElement ui;
            for (int i = wPanel.Children.Count - 1; i >= 0; --i)
            {
                ui = wPanel.Children[i];
                if (ui is ThingWithLabel)
                {
                    ((ThingWithLabel)ui).CheckChanged -= PreIngredi_CheckChanged;
                }
                wPanel.Children.Remove(ui);
            }
        }

        private void FillPreWPanel(ref WrapPanel wPanel, List<Things.ThingBase> thingBaseList, Calculator.IngrediTypes ingrediType)
        {
            ThingWithLabel cb;
            Things.ThingBase thingBase;
            Channels.Channel channel;
            Recipes.Recipe recipe;
            string lb3Tx = $"/s{Environment.NewLine}/m{Environment.NewLine}/h";
            for (int i = 0, iv = thingBaseList.Count; i < iv; ++i)
            {
                thingBase = thingBaseList[i];
                cb = new ThingWithLabel()
                {
                    ThingBase = thingBase,
                    Tag = ingrediType,
                };
                switch (ingrediType)
                {
                    case Calculator.IngrediTypes.Accessory:
                    case Calculator.IngrediTypes.Input:
                    case Calculator.IngrediTypes.Output:
                        cb.TxLabel1 = cb.TxLabel2 = cb.TxLabel3 = "";// $"{Environment.NewLine}{Environment.NewLine}";
                        break;
                    case Calculator.IngrediTypes.Channel:
                        channel = (Channels.Channel)thingBase;
                        cb.TxLabel1 = "";
                        cb.TxLabel2 = GetSpeedTx(channel.speed);
                        cb.TxLabel3 = lb3Tx;
                        break;
                    case Calculator.IngrediTypes.Procedure:
                        recipe = (Recipes.Recipe)thingBase;
                        cb.TxLabel1 = $"{Environment.NewLine}{Environment.NewLine}Total";
                        cb.TxLabel2 = GetSpeedTx(GetSpeedTotal(recipe));
                        cb.TxLabel3 = lb3Tx;
                        break;
                }
                cb.CheckChanged += PreIngredi_CheckChanged;
                wPanel.Children.Add(cb);
            }
            string GetSpeedTx(double? speedBase)
            {
                if (speedBase == null)
                {
                    return $"0{Environment.NewLine}0{Environment.NewLine}0";
                }
                StringBuilder strBdr = new StringBuilder();
                strBdr.AppendLine(SimpleStringHelper.UnitsOfMeasure.GetShortString((decimal)speedBase, "", 1000, 2));
                strBdr.AppendLine(SimpleStringHelper.UnitsOfMeasure.GetShortString((decimal)speedBase * 60, "", 1000, 1));
                strBdr.Append(SimpleStringHelper.UnitsOfMeasure.GetShortString((decimal)speedBase * 3600, "", 1000, 1));
                return strBdr.ToString();
            }
            double GetSpeedTotal(Recipes.Recipe recipe)
            {
                double total = 0;
                foreach (Recipes.Recipe.PIOItem pio in recipe.outputs)
                {
                    if (pio.quantity == null)
                    {
                        throw ProcessingChains.SearchHelper.Error_Recipe_Output_Quantity_isNull(pio.item?.name, recipe.name);
                    }
                    total += pio.quantity.ValueCurrentInGeneral;
                }
                if (recipe.period == null)
                {
                    throw ProcessingChains.SearchHelper.Error_Recipe_Peroid_isNullOrZero(recipe.name);
                }
                return total / (double)recipe.period;
            }
        }

        private void PreIngredi_CheckChanged(object? sender, EventArgs e)
        {
            if (sender is not ThingWithLabel)
            {
                return;
            }
            ThingWithLabel twl = (ThingWithLabel)sender;
            if (twl.Tag is not Calculator.IngrediTypes)
            {
                return;
            }
            Calculator.IngrediTypes type = (Calculator.IngrediTypes)twl.Tag;
            if (twl.IsChecked)
            {
                calculator.AddFilter(twl.ThingBase, type);
            }
            else
            {
                calculator.RemoveFilter(twl.ThingBase, type);
            }
        }

        private void dataGrid_pre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearQuantity();
            graphAlpha_clear();

            if (dataGrid_pre.SelectedItem == null)
            {
                selectedProcessingCombing = null;
            }
            else
            {
                selectedProcessingCombing = (DataGridItemModelProcessingCombing)dataGrid_pre.SelectedItem;
            }
        }
        private void btn_cal_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProcessingCombing == null)
            {
                MessageBox.Show(this, "Select a process to continue.", "Whick process?", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            tabMain.SelectedIndex = 1;
        }

        private DateTime dataGrid_pre_PreviewMouseDownTime = DateTime.MinValue;
        private async void dataGrid_pre_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DateTime now = DateTime.Now;
            if ((now - dataGrid_pre_PreviewMouseDownTime).TotalMilliseconds <= Core.Instance.mouseDoubleClickInterval)
            {
                await Task.Delay(10);
                tabMain.SelectedIndex = 1;
            }
            else
            {
                dataGrid_pre_PreviewMouseDownTime = now;
            }
        }

        private async void dataGrid_pre_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && dataGrid_pre.SelectedItem != null)
            {
                await Task.Delay(10);
                tabMain.SelectedIndex = 2;
            }
        }


        #endregion


        #region quantity
        private void ClearQuantity()
        {
            tip_qt.Item = null;

            nud_qtSec.Value = 0;
            nud_qtMin.Value = 0;
            nud_qtHour.Value = 0;

            sp_qtP.Children.Clear();
            sp_qtA.Children.Clear();
            sp_qtI.Children.Clear();
            sp_qtO.Children.Clear();
        }
        private void FillQuantityPanel()
        {
            if (selectedProcessingCombing == null)
            {
                ClearQuantity();
                return;
            }
            tip_qt.Item = tiq_targetProduct.Item;

            int i, iv;
            ProcessingChains.ProcessingNode pNode;
            for (i = 0, iv = selectedProcessingCombing.searchResult.allProcesses.Count; i < iv; ++i)
            {
                pNode = selectedProcessingCombing.searchResult.allProcesses[i];
                if (pNode.recipe.processor == null)
                {
                    throw new Exception("Processor in process is null.");
                }
                sp_qtP.Children.Add(new ThingWithLabel()
                {
                    IsCheckable = false,
                    ThingBase = core.FindThing((Guid)pNode.recipe.processor),
                    TxLabel1 = "",
                    TxLabel2 = "",
                    TxLabel3 = "",
                });
            }
            FillPanelT(selectedProcessingCombing.searchResult.AllAccessories, ref sp_qtA);
            FillPanelH(selectedProcessingCombing.searchResult.allSources, ref sp_qtI);
            FillPanelH(selectedProcessingCombing.searchResult.allFinalProducts, ref sp_qtO);

            QuantityRefreshInfo();

            void FillPanelT(List<Things.Thing> dataList, ref StackPanel sPanel)
            {
                Things.Thing thing;
                for (i = 0, iv = dataList.Count; i < iv; ++i)
                {
                    thing = dataList[i];

                    sPanel.Children.Add(new ThingWithLabel()
                    {
                        IsCheckable = false,
                        ThingBase = thing,
                        TxLabel1 = "",
                        TxLabel2 = "",
                        TxLabel3 = "",
                    });
                }
            }
            void FillPanelH(List<ProcessingChains.ProcessingHead> dataList, ref StackPanel sPanel)
            {
                ProcessingChains.ProcessingHead head;
                for (i = 0, iv = dataList.Count; i < iv; ++i)
                {
                    head = dataList[i];

                    sPanel.Children.Add(new ThingWithLabel()
                    {
                        IsCheckable = false,
                        ThingBase = head.thing,
                        TxLabel1 = "",
                        TxLabel2 = "",
                        TxLabel3 = "",
                    });
                }
            }
        }
        private void QuantityRefreshInfo()
        {
            if (selectedProcessingCombing == null)
            {
                return;
            }
            int i, iv;
            ProcessingChains.ProcessingNode pNode;
            ThingWithLabel ui;
            if (sp_qtP.Children.Count > 0)
            {
                for (i = 0, iv = selectedProcessingCombing.searchResult.allProcesses.Count; i < iv; ++i)
                {
                    pNode = selectedProcessingCombing.searchResult.allProcesses[i];
                    ui = (ThingWithLabel)sp_qtP.Children[i];
                    ui.TxLabel2 = SimpleStringHelper.UnitsOfMeasure.GetShortString((decimal)pNode.calMultiple, "")
                        + Environment.NewLine
                        + "(" + Math.Ceiling(pNode.calMultiple) + ")";
                }
            }
            if (sp_qtA.Children.Count > 0)
            {
                Dictionary<Guid, double> counter = new Dictionary<Guid, double>();
                Things.Thing t;
                for (i = 0, iv = selectedProcessingCombing.searchResult.allProcesses.Count; i < iv; ++i)
                {
                    pNode = selectedProcessingCombing.searchResult.allProcesses[i];
                    foreach (Recipes.Recipe.PIOItem a in pNode.recipe.accessories)
                    {
                        if (a.item == null)
                        {
                            throw new Exception("Accessory is null.");
                        }
                        t = a.item;
                        if (counter.ContainsKey(t.id))
                        {
                            counter[t.id] += pNode.calMultiple;
                        }
                        else
                        {
                            counter.Add(t.id, pNode.calMultiple);
                        }
                    }
                }
                decimal count;
                for (i = 0, iv = sp_qtA.Children.Count; i < iv; ++i)
                {
                    ui = (ThingWithLabel)sp_qtA.Children[i];
                    if (ui.ThingBase == null)
                    {
                        continue;
                    }
                    if (counter.ContainsKey(ui.ThingBase.id))
                    {
                        count = (decimal)counter[ui.ThingBase.id];
                        ui.TxLabel2 = SimpleStringHelper.UnitsOfMeasure.GetShortString(count, "")
                        + Environment.NewLine
                        + "(" + Math.Ceiling(count) + ")";
                    }
                }
            }

            ProcessingChains.ProcessingHead pHead;
            UpdateInfo(selectedProcessingCombing.searchResult.allSources, ref sp_qtI);
            UpdateInfo(selectedProcessingCombing.searchResult.allFinalProducts, ref sp_qtO);

            void UpdateInfo(List<ProcessingChains.ProcessingHead> headList, ref StackPanel sPanel)
            {
                if (sPanel.Children.Count > 0)
                {
                    for (i = 0, iv = headList.Count; i < iv; ++i)
                    {
                        pHead = headList[i];
                        ui = (ThingWithLabel)sPanel.Children[i];
                        ui.TxLabel2 = SimpleStringHelper.UnitsOfMeasure.GetShortString((decimal)pHead.calSpeed, "")
                            + Environment.NewLine
                            + "(" + Math.Ceiling(pHead.calSpeed) + ")";
                    }
                }
            }
        }


        private async void quantity_nud_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (selectedProcessingCombing == null
                || sender is not NumericUpDown)
            {
                return;
            }
            if (e.Key == Key.Enter)
            {
                await Task.Delay(10);

                NumericUpDown ui = (NumericUpDown)sender;
                double baseMul = 1;
                decimal uiValue = ui.Value;
                if (ui == nud_qtMin)
                {
                    baseMul = 60;
                    nud_qtSec.Value = uiValue / 60;
                    nud_qtHour.Value = uiValue * 60;
                }
                else if (ui == nud_qtHour)
                {
                    baseMul = 3600;
                    nud_qtSec.Value = uiValue / 3600;
                    nud_qtMin.Value = uiValue / 60;
                }
                else
                {
                    nud_qtMin.Value = uiValue * 60;
                    nud_qtHour.Value = uiValue * 3600;
                }
                quantity_setNewQuantity((double)uiValue / baseMul);
            }
            else if (e.Key == Key.Escape)
            {
                NumericUpDown ui = (NumericUpDown)sender;
                double baseMul = 1;
                if (ui == nud_qtMin)
                {
                    baseMul = 60;
                }
                else if (ui == nud_qtHour)
                {
                    baseMul = 3600;
                }
                ui.Value = (decimal)(selectedProcessingCombing.searchResult.NewSpeed * baseMul);
            }
        }

        private void quantity_nud_LostFocus(object sender, RoutedEventArgs e)
        {
            if (selectedProcessingCombing == null
                || sender is not NumericUpDown)
            {
                return;
            }
            NumericUpDown ui = (NumericUpDown)sender;
            double baseMul = 1;
            if (ui == nud_qtMin)
            {
                baseMul = 60;
            }
            else if (ui == nud_qtHour)
            {
                baseMul = 3600;
            }
            quantity_setNewQuantity((double)ui.Value / baseMul);

        }

        private void quantity_nud_ValueChanged(NumericUpDown sender)
        {
            quantity_nud_LostFocus(sender, null);
        }

        private void quantity_setNewQuantity(double newSpeed)
        {
            if (selectedProcessingCombing == null)
            {
                return;
            }
            selectedProcessingCombing.searchResult.ReCalSpeed(newSpeed);
            QuantityRefreshInfo();
        }

        #endregion


        #region graph alpha

        private FlowGraphAlpha fga;

        private void graphAlpha_clear()
        {
            canvas_graphAlpha.Children.Clear();
        }

        #region move/resize panel
        private void Fga_PanelTitleMouseDown(FlowGraphAlpha fga, MovablePanel panel)
        {
            MovingOrResizingPanelStart(panel, true);
        }
        private void Fga_PanelResizeHandleMouseDown(FlowGraphAlpha fga, MovablePanel panel)
        {
            MovingOrResizingPanelStart(panel, false);
        }
        private void MovingOrResizingPanelStart(MovablePanel panel, bool isMovingOrResizing)
        {
            movingOrResizingPanel = panel;
            movingOrResizing = isMovingOrResizing;

            // add a frame to show new position
            panelFrameRect.Width = panel.ActualWidth * canvas_graphAlpha_curScaleValue;
            panelFrameRect.Height = panel.ActualHeight * canvas_graphAlpha_curScaleValue;


            panelFrameRectPositionStart = panel.TranslatePoint(new Point(), grid_graphAlpha);
            panelFrameRectSizeStart = new Size(panel.ActualWidth, panel.ActualHeight);

            MovePanelFrame(panelFrameRectPositionStart.X, panelFrameRectPositionStart.Y);
            panelMousePointStart = Mouse.GetPosition(grid_graphAlpha);
            grid_graphAlpha.Children.Add(panelFrameRect);
        }

        private MovablePanel? movingOrResizingPanel = null;
        private bool movingOrResizing = true;
        private Rectangle panelFrameRect = new Rectangle()
        {
            Stroke = Brushes.Gray,
            StrokeThickness = 1,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
        };
        private Point panelFrameRectPositionStart;
        private Size panelFrameRectSizeStart;
        private Point panelMousePointStart;

        private void MovePanelFrame(double? x, double? y)
        {
            if (x != null && y != null)
            {
                panelFrameRect.Margin = new Thickness((double)x, (double)y, 0, 0);
            }
            else if (x == null && y == null)
            {
                return;
            }
            else
            {
                Thickness oriMagin = panelFrameRect.Margin;
                if (x != null)
                {
                    panelFrameRect.Margin = new Thickness((double)x, oriMagin.Top, 0, 0);
                }
                else if (y != null)
                {
                    panelFrameRect.Margin = new Thickness(oriMagin.Left, (double)y, 0, 0);
                }
            }
        }
        private void ResizePanelFrame(double? offsetX, double? offsetY)
        {
            if (offsetX != null)
            {
                panelFrameRect.Width = panelFrameRectSizeStart.Width * canvas_graphAlpha_curScaleValue + (double)offsetX;
            }
            if (offsetY != null)
            {
                panelFrameRect.Height = panelFrameRectSizeStart.Height * canvas_graphAlpha_curScaleValue + (double)offsetY;
            }
        }

        private void MoveHandleEnd(double offsetX, double offsetY)
        {
            if (movingOrResizingPanel == null)
            {
                return;
            }

            // stop moving anima, set move
            MovablePanel targetPanel = movingOrResizingPanel;
            movingOrResizingPanel = null;

            offsetX /= canvas_graphAlpha_curScaleValue;
            offsetY /= canvas_graphAlpha_curScaleValue;
            // remove the frame
            grid_graphAlpha.Children.Remove(panelFrameRect);

            if (movingOrResizing)
            {
                // set new position
                FlowGraphAlpha.MovePanel(targetPanel, offsetX, offsetY);
            }
            else
            {
                // set new size
                FlowGraphAlpha.ResizePanel(targetPanel, offsetX, offsetY);
            }
            // re-layout
            //if (targetPanel is FlowGraphAlpha.ProcessPanel)
            //{
            //    ((FlowGraphAlpha.ProcessPanel)targetPanel).ReSetIOlayout();
            //}
            fga.SetAutoMargin(true);
        }

        #endregion

        #region zoom in/out

        private bool graphAlpha_moving = false;
        private Point graphAlpha_movingStartMousePoint;
        private Point graphAlpha_movingStartScrollOffset;
        private void canvas_graphAlpha_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            graphAlpha_moving = true;
            graphAlpha_movingStartMousePoint = Mouse.GetPosition(grid_graphAlpha);
            graphAlpha_movingStartScrollOffset = new Point(
                sv_graphAlpha.HorizontalOffset,
                sv_graphAlpha.VerticalOffset);
            Cursor = Cursors.Hand;
        }

        private void canvas_graphAlpha_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // do nothing
        }


        private void canvas_graphAlpha_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            canvas_graphAlpha_Zoom(Mouse.GetPosition(sv_graphAlpha), (double)e.Delta / 1200);
        }
        private double canvas_graphAlpha_curScaleValue = 1;
        private void canvas_graphAlpha_Zoom(Point? centerPoint = null, double scaleOffset = 0.1)
        {
            if (scaleOffset == 0)
            {
                return;
            }
            if (canvas_graphAlpha_curScaleValue < 0.2 && scaleOffset < 0)
            {
                return;
            }
            if (canvas_graphAlpha_curScaleValue >= 10 && scaleOffset > 0)
            {
                return;
            }
            double oriScaleValue = canvas_graphAlpha_curScaleValue;
            canvas_graphAlpha_curScaleValue += scaleOffset;

            Point oriScollOffset = new Point(
                sv_graphAlpha.HorizontalOffset,
                sv_graphAlpha.VerticalOffset);
            Point oriPtOnGraph = Mouse.GetPosition(bdr_graphAlpha);

            // 缩放到新比例
            ScaleTransform st;
            if (canvas_graphAlpha.RenderTransform is ScaleTransform)
            {
                st = (ScaleTransform)canvas_graphAlpha.RenderTransform;
            }
            else
            {
                st = new ScaleTransform();
                canvas_graphAlpha.RenderTransform = st;
            }
            st.ScaleY = st.ScaleX = canvas_graphAlpha_curScaleValue;
            bdr_grapnAlpha_syncSize();

            // 按照缩放控制点，重新挪动整图位置
            Point cp;
            if (centerPoint == null)
            {
                cp = new Point(
                    sv_graphAlpha.ActualWidth / 2,
                    sv_graphAlpha.ActualHeight / 2);
            }
            else
            {
                cp = (Point)centerPoint;
            }


            //await Task.Delay(50);

            double scollMultiple = canvas_graphAlpha_curScaleValue / oriScaleValue - 1;
            sv_graphAlpha.ScrollToHorizontalOffset(oriScollOffset.X + (oriPtOnGraph.X * scollMultiple));
            sv_graphAlpha.ScrollToVerticalOffset(oriScollOffset.Y + (oriPtOnGraph.Y * scollMultiple));
        }

        #endregion


        private void canvas_graphAlpha_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            bdr_grapnAlpha_syncSize();
        }
        private void bdr_grapnAlpha_syncSize()
        {
            bdr_graphAlpha.Width = canvas_graphAlpha.ActualWidth * canvas_graphAlpha_curScaleValue;
            bdr_graphAlpha.Height = canvas_graphAlpha.ActualHeight * canvas_graphAlpha_curScaleValue;
        }

        #region top buttons, zoom out/in, origin, save image
        private void btn_graphAlpha_zoomOut_Click(object sender, RoutedEventArgs e)
        {
            canvas_graphAlpha_Zoom(new Point(sv_graphAlpha.ActualWidth / 2, sv_graphAlpha.ActualHeight / 2), -0.1);
        }

        private void btn_graphAlpha_scaleOrigin_Click(object sender, RoutedEventArgs e)
        {
            canvas_graphAlpha_curScaleValue = 0.9;
            canvas_graphAlpha_Zoom(new Point(sv_graphAlpha.ActualWidth / 2, sv_graphAlpha.ActualHeight / 2), 0.1);
        }

        private void btn_graphAlpha_zoomIn_Click(object sender, RoutedEventArgs e)
        {
            canvas_graphAlpha_Zoom(new Point(sv_graphAlpha.ActualWidth / 2, sv_graphAlpha.ActualHeight / 2), 0.1);
        }

        private SaveFileDialog graphAlpha_saveToImageDialog;
        private void btn_graphAlpha_saveAsImage_Click(object sender, RoutedEventArgs e)
        {
            if (graphAlpha_saveToImageDialog == null)
            {
                graphAlpha_saveToImageDialog = new SaveFileDialog()
                {
                    Filter = "PNG|*.png|JPG|*.jpg|JPEG|*.jpeg",
                };
            }
            if (graphAlpha_saveToImageDialog.ShowDialog() == true)
            {
                bdr_graphAlpha.SaveToImageFile(graphAlpha_saveToImageDialog.FileName);
            }
        }
        #endregion

        #endregion



        private void btn_newCal_Click(object sender, RoutedEventArgs e)
        {
            core.mainWindow.btn_calculate_Click(sender, e);
        }


    }
}
