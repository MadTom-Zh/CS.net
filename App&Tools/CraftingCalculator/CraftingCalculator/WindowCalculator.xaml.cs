using MadTomDev.App.Classes;
using MadTomDev.App.Ctrls;
using MadTomDev.App.VMs;
using MadTomDev.Common;
using MadTomDev.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using static MadTomDev.App.Classes.Recipes;

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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await core.SetCursorWait(this);

            tbv_sceneChain.Text = core.SceneFullPath;
            //grid_pre.ItemsSource = core.calculatedProcessingCombings;

            LoadPreFilterSettings();
            FillPreFilterSPanels();

            tabMain.SelectedIndex = 1;

            await Task.Delay(1000);
            await core.SetCursorArrow(this);
        }

        private int tabMain_SelectionIndexPre = 1;
        private async void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabMain_SelectionIndexPre == 0
                && tabMain.SelectedIndex != 0)
            {
                TrySavePreFilterSettings();
            }
            
            switch (tabMain.SelectedIndex)
            {
                default:
                case 0:
                    break;
                case 1:
                    //ClearQuantity();
                    //graphAlpha_clear();
                    break;
                case 2:
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
                case 3:
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
                            await core.SetCursorWait(this);
                            fga = new FlowGraphAlpha(ref canvas_graphAlpha, selectedProcessingCombing.searchResult);
                            fga.PanelTitleMouseDown += Fga_PanelTitleMouseDown;
                            fga.PanelResizeHandleMouseDown += Fga_PanelResizeHandleMouseDown;
                            await core.SetCursorArrow(this,1);
                        }
                        fga.UpdateQuantities();
                        fga.UpdateAllLinkLines();
                    }
                    break;
            }
            tabMain_SelectionIndexPre = tabMain.SelectedIndex;
        }


        #region window, mouse action

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (tabMain.SelectedIndex == 3)
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
            if (tabMain.SelectedIndex == 3)
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


        #region pre-filters
        private void FillPreFilterSPanels()
        {

            #region 按处理器排序
            List<DataGridItemModelRecipe> tmpRList = new List<DataGridItemModelRecipe>();
            int i;
            DataGridItemModelRecipe testR;
            bool isInsert;
            foreach (DataGridItemModelRecipe r in core.calculationRecipes)
            {
                if (r.data == null)
                {
                    continue;
                }
                isInsert = false;
                for (i = tmpRList.Count - 1; i >= 0; --i)
                {
                    testR = tmpRList[i];
                    if (testR.data == null)
                    {
                        continue;
                    }
                    if (testR.data.processor == r.data.processor)
                    {
                        if (i == tmpRList.Count - 1)
                        {
                            tmpRList.Add(r);
                        }
                        else
                        {
                            tmpRList.Insert(i+1,r);
                        }
                        isInsert = true;
                        break;
                    }
                }
                if (isInsert)
                {
                    continue;
                }
                tmpRList.Add(r);
            }

            FillFilterSPanel(ref sp_pfP, tmpRList.Select(a => a.Id).ToList(), Calculator.IngrediTypes.Procedure);
            List<Guid> tmpIdList = new List<Guid>();
            foreach (DataGridItemModelRecipe r in core.calculationRecipes)
            {
                if (r.aList != null)
                {
                    foreach (Recipe.PIOItem a in r.aList)
                    {
                        if (a.thing != null
                            && !tmpIdList.Contains(a.thing.id))
                        {
                            tmpIdList.Add(a.thing.id);
                        }
                    }
                }
            }
            #endregion

            FillFilterSPanel(ref sp_pfA, tmpIdList, Calculator.IngrediTypes.Accessory);
            FillFilterSPanel(ref sp_pfC, core.calculationChannels.Select(a => a.Id).ToList(), Calculator.IngrediTypes.Channel);
            tmpIdList = core.calculationThings.Select(a => a.Id).ToList();
            FillFilterSPanel(ref sp_pfI, tmpIdList, Calculator.IngrediTypes.Input);
            FillFilterSPanel(ref sp_pfO, tmpIdList, Calculator.IngrediTypes.Output);
        }
        private void FillFilterSPanel(ref StackPanel sPanel, List<Guid> tIdList, Calculator.IngrediTypes ingrediType)
        {
            ThingWithLabel cb;
            Guid curId;
            Things.ThingBase? curThingBase;
            Channels.Channel channel;
            Recipes.Recipe recipe;
            string lb3Tx = $"/s{Environment.NewLine}/m{Environment.NewLine}/h";
            HashSet<Guid> filterList;
            switch (ingrediType)
            {
                case Calculator.IngrediTypes.Accessory:
                    filterList = preFilters_accessories;
                    break;
                case Calculator.IngrediTypes.Channel:
                    filterList = preFilters_channels;
                    break;
                case Calculator.IngrediTypes.Input:
                    filterList = preFilters_inputs;
                    break;
                case Calculator.IngrediTypes.Output:
                    filterList = preFilters_outputs;
                    break;
                default:
                case Calculator.IngrediTypes.Procedure:
                    filterList = preFilters_procedures;
                    break;
            }
            for (int i = 0, iv = tIdList.Count; i < iv; ++i)
            {
                curId = tIdList[i];
                switch (ingrediType)
                {
                    case Calculator.IngrediTypes.Accessory:
                    case Calculator.IngrediTypes.Input:
                    case Calculator.IngrediTypes.Output:
                        curThingBase = core.FindThing(curId);
                        cb = new ThingWithLabel()
                        {
                            ThingBase = curThingBase,
                            Tag = ingrediType,
                            IsChecked = filterList.Contains(curId),
                        };
                        cb.CheckChanged += FilterIngredi_CheckChanged;
                        sPanel.Children.Add(cb);
                        cb.TxLabel1 = cb.TxLabel2 = cb.TxLabel3 = "";// $"{Environment.NewLine}{Environment.NewLine}";
                        break;
                    case Calculator.IngrediTypes.Channel:
                        channel = core.FindChannel(curId);
                        cb = new ThingWithLabel()
                        {
                            ThingBase = channel,
                            Tag = ingrediType,
                            IsChecked = filterList.Contains(curId),
                        };
                        cb.CheckChanged += FilterIngredi_CheckChanged;
                        sPanel.Children.Add(cb);
                        cb.TxLabel1 = "";
                        cb.TxLabel2 = GetSpeedTx(channel.speed);
                        cb.TxLabel3 = lb3Tx;
                        break;
                    case Calculator.IngrediTypes.Procedure:
                        recipe = core.FindRecipe(curId);
                        cb = new ThingWithLabel()
                        {
                            ThingBase = recipe,
                            Tag = ingrediType,
                            IsChecked = filterList.Contains(curId),
                        };

                        sPanel.Children.Add(cb);

                        cb.CheckChanged += FilterIngredi_CheckChanged;
                        cb.TxLabel1 = $"{Environment.NewLine}{Environment.NewLine}Total";
                        cb.TxLabel2 = GetSpeedTx(GetSpeedTotal(recipe));
                        cb.TxLabel3 = lb3Tx;
                        break;
                }
            }
            string GetSpeedTx(decimal? speedBase)
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
            decimal GetSpeedTotal(Recipes.Recipe recipe)
            {
                decimal total = 0;
                foreach (Recipes.Recipe.PIOItem pio in recipe.outputs)
                {
                    if (pio.quantity == null)
                    {
                        throw ProcessingChains.SearchHelper.Error_Recipe_Output_Quantity_isNull(pio.thing?.name, recipe.name);
                    }
                    total += pio.quantity.ValueCurrentInGeneral;
                }
                if (recipe.period == null)
                {
                    throw ProcessingChains.SearchHelper.Error_Recipe_Peroid_isNullOrZero(recipe.name);
                }
                return total / (decimal)recipe.period;
            }
        }
        private void FilterIngredi_CheckChanged(object? sender, EventArgs e)
        {
            if (sender == null
                || sender is not ThingWithLabel)
            {
                return;
            }
            ThingWithLabel ui = (ThingWithLabel)sender;
            DependencyObject uiParent = ui.Parent;
            HashSet<Guid> idList;
            if (uiParent == sp_pfP)
            {
                idList = preFilters_procedures;
            }
            else if (uiParent == sp_pfA)
            {
                idList = preFilters_accessories;
            }
            else if (uiParent == sp_pfC)
            {
                idList = preFilters_channels;
            }
            else if (uiParent == sp_pfI)
            {
                idList = preFilters_inputs;
            }
            else if (uiParent == sp_pfO)
            {
                idList = preFilters_outputs;
            }
            else
            {
                throw new Exception("Unknow parent!");
            }

            if (ui.ThingBase == null)
            {
                throw new Exception("No data set to UI.");
            }
            Guid tId = ui.ThingBase.id;
            if (ui.IsChecked)
            {
                if (!idList.Contains(tId))
                {
                    idList.Add(tId);
                }
            }
            else
            {
                if (idList.Contains(tId))
                {
                    idList.Remove(tId);
                }
            }
            needSavePreFilterSettings = true;
        }

        private HashSet<Guid> preFilters_procedures = new HashSet<Guid>();
        private HashSet<Guid> preFilters_accessories = new HashSet<Guid>();
        private HashSet<Guid> preFilters_channels = new HashSet<Guid>();
        private HashSet<Guid> preFilters_inputs = new HashSet<Guid>();
        private HashSet<Guid> preFilters_outputs = new HashSet<Guid>();
        private void LoadPreFilterSettings()
        {
            string file = System.IO. Path.Combine(
                core.sceneMgr. selectedTreeViewNode.GetDirFullName(), 
                SceneMgr.FILENAME_PREFILTERLISTS);
            if (!File.Exists(file))
            {
                return;
            }

            int flagIdx = -1;
            string? curLine;
            Guid testId;
            using (FileStream fs = new FileStream(file,FileMode.Open))
            using (StreamReader sr = new StreamReader(fs))
            {
                while (!sr.EndOfStream)
                {
                    curLine = sr.ReadLine();
                    if (curLine == null)
                    {
                        continue;
                    }

                    if (curLine == SceneMgr.FLAG_PROCEDURES)
                    {
                        flagIdx = 0;
                    }
                    else if (curLine == SceneMgr.FLAG_ACCESSORIES)
                    {
                        flagIdx = 1;
                    }
                    else if (curLine == SceneMgr.FLAG_CHANNELS)
                    {
                        flagIdx = 2;
                    }
                    else if (curLine == SceneMgr.FLAG_INPUTS)
                    {
                        flagIdx = 3;
                    }
                    else if (curLine == SceneMgr.FLAG_OUTPUTS)
                    {
                        flagIdx = 4;
                    }
                    else
                    {
                        if (Guid.TryParse(curLine, out testId))
                        {
                            switch (flagIdx)
                            {
                                case 0:
                                    preFilters_procedures.Add(testId);
                                    break;
                                case 1:
                                    preFilters_accessories.Add(testId);
                                    break;
                                case 2:
                                    preFilters_channels.Add(testId);
                                    break;
                                case 3:
                                    preFilters_inputs.Add(testId);
                                    break;
                                case 4:
                                    preFilters_outputs.Add(testId);
                                    break;
                            }
                        }
                    }
                }
            }
        }
        private bool needSavePreFilterSettings = false;
        private void TrySavePreFilterSettings()
        {
            if (!needSavePreFilterSettings)
            {
                return;
            }

            string file = System.IO.Path.Combine(
                core.sceneMgr.selectedTreeViewNode.GetDirFullName(),
                SceneMgr.FILENAME_PREFILTERLISTS);
            using (FileStream fs = new FileStream(file, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                WriteBlock(sw, SceneMgr.FLAG_PROCEDURES, preFilters_procedures);
                WriteBlock(sw, SceneMgr.FLAG_ACCESSORIES, preFilters_accessories);
                WriteBlock(sw, SceneMgr.FLAG_CHANNELS, preFilters_channels);
                WriteBlock(sw, SceneMgr.FLAG_INPUTS, preFilters_inputs);
                WriteBlock(sw, SceneMgr.FLAG_OUTPUTS, preFilters_outputs);
            }

            void WriteBlock(StreamWriter sw, string flag, HashSet<Guid> idList)
            {
                sw.WriteLine(flag);
                foreach (Guid id in idList)
                {
                    sw.WriteLine(id.ToString());
                }
            }
        }
        #endregion


        #region precondition

        DataGridItemModelProcessingCombing? selectedProcessingCombing = null;

        private async void tiq_targetProduct_IconDoubleClicked(object sender, EventArgs e)
        {
            if (core.calculationChannels.Count <= 0)
            {
                MessageBox.Show(this,
                    (string)Application.Current.TryFindResource("lb_winCalculator_msgBox_noChannel_content"),
                    (string)Application.Current.TryFindResource("lb_winCalculator_msgBox_noChannel_title"),
                    MessageBoxButton.OK,MessageBoxImage.Asterisk);
                return;
            }

            await core.SetCursorWait(this);
            WindowSelectThings? win = core.ShowSelectThingsWin(this);
            if (win != null
                && win.SelectedThingIdList != null
                && win.SelectedThingIdList.Count == 1)
            {
                ClearAll();
                tiq_targetProduct.ThingId = win.SelectedThingIdList[0];

                //先填充选择面板，随后显示搜索按钮；

                ProcessingChains.SearchHelper.GetRelatedIdList(tiq_targetProduct.ThingId,
                    out List<Guid> recipeIdList, out List<Guid> accessoryIdList,
                    out List<Guid> inputIdList, out List<Guid> outputIdList);

                // fill pre wrapPanels
                FillPreWPanel(ref wp_preA, accessoryIdList, Calculator.IngrediTypes.Accessory);
                FillPreWPanel(ref wp_preI, inputIdList, Calculator.IngrediTypes.Input);
                FillPreWPanel(ref wp_preO, outputIdList, Calculator.IngrediTypes.Output);
                FillPreWPanel(ref wp_preC, core.calculationChannels.Select(a => a.Id).ToList(), Calculator.IngrediTypes.Channel);
                FillPreWPanel(ref wp_preP, recipeIdList, Calculator.IngrediTypes.Procedure);

                btn_search.IsEnabled = true;

                expd_preP.IsExpanded = true;
                expd_preA.IsExpanded = true;
                expd_preC.IsExpanded = true;
                expd_preI.IsExpanded = true;
                expd_preO.IsExpanded = false;
            }
            await core.SetCursorArrow(this);
        }

        private Calculator calculator;
        private async void btn_search_Click(object sender, RoutedEventArgs e)
        {
            if (tiq_targetProduct.ThingId == Guid.Empty)
            {
                MessageBox.Show(this,
                    (string)Application.Current.TryFindResource("lb_winCalculator_msgBox_noProductSelected_content"),
                    (string)Application.Current.TryFindResource("lb_winCalculator_msgBox_noProductSelected_title"),
                    MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            await core.SetCursorWait(this);

            calculator = core.NewCalculate(tiq_targetProduct.ThingId,
                GetPreWPanelDeselectedThingIdList(ref wp_preP),
                GetPreWPanelDeselectedThingIdList(ref wp_preA),
                GetPreWPanelDeselectedThingIdList(ref wp_preC),
                GetPreWPanelDeselectedThingIdList(ref wp_preI),
                GetPreWPanelDeselectedThingIdList(ref wp_preO));


            // fill combination data grid
            dataGrid_pre.ItemsSource = calculator.calculatedProcessingCombings;

            if (calculator.calculatedProcessingCombings == null
                || calculator.calculatedProcessingCombings.Count == 0)
            {
                MessageBox.Show(this,
                    (string)Application.Current.TryFindResource("lb_winCalculator_msgBox_searchNoResult_content"),
                    (string)Application.Current.TryFindResource("lb_winCalculator_msgBox_searchNoResult_title"),
                    MessageBoxButton.OK,MessageBoxImage.Information);
            }


            this.Title = $"CC: {core.FindThing(tiq_targetProduct.ThingId)?.name}";

            btn_search.IsEnabled = false;

            await core.SetCursorArrow(this);
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

            this.Title = (string)Application.Current.TryFindResource("lb_winCalculator_title");

            calculator?.Dispose();
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
        private List<Guid> GetPreWPanelDeselectedThingIdList(ref WrapPanel wPanel)
        {
            List<Guid> result = new List<Guid>();
            UIElement ui;
            ThingWithLabel twl;
            for (int i = wPanel.Children.Count - 1; i >= 0; --i)
            {
                ui = wPanel.Children[i];
                if (ui is ThingWithLabel)
                {
                    twl = (ThingWithLabel)ui;
                    if (twl.IsChecked && twl.ThingBase != null)
                    {
                        result.Add(twl.ThingBase.id);
                    }
                }
            }
            return result;
        }

        private void FillPreWPanel(ref WrapPanel wPanel, List<Guid> thingIdList, Calculator.IngrediTypes ingrediType)
        {
            ThingWithLabel cb;
            Guid curId;
            Things.ThingBase? curThingBase;
            Channels.Channel channel;
            Recipes.Recipe recipe;
            string lb3Tx = $"/s{Environment.NewLine}/m{Environment.NewLine}/h";

            HashSet<Guid> preFilterList;
            switch (ingrediType)
            {
                case Calculator.IngrediTypes.Accessory:
                    preFilterList = preFilters_accessories;
                    break;
                case Calculator.IngrediTypes.Channel:
                    preFilterList = preFilters_channels;
                    break;
                case Calculator.IngrediTypes.Input:
                    preFilterList = preFilters_inputs;
                    break;
                case Calculator.IngrediTypes.Output:
                    preFilterList = preFilters_outputs;
                    break;
                default:
                case Calculator.IngrediTypes.Procedure:
                    preFilterList = preFilters_procedures;
                    break;
            }

            for (int i = 0, iv = thingIdList.Count; i < iv; ++i)
            {
                curId = thingIdList[i];
                switch (ingrediType)
                {
                    case Calculator.IngrediTypes.Accessory:
                    case Calculator.IngrediTypes.Input:
                    case Calculator.IngrediTypes.Output:
                        curThingBase = core.FindThing(curId);
                        cb = new ThingWithLabel()
                        {
                            ThingBase = curThingBase,
                            Tag = ingrediType,
                            IsChecked = preFilterList.Contains(curId),
                        };
                        cb.CheckChanged += PreIngredi_CheckChanged;
                        wPanel.Children.Add(cb);
                        cb.TxLabel1 = cb.TxLabel2 = cb.TxLabel3 = "";// $"{Environment.NewLine}{Environment.NewLine}";
                        break;
                    case Calculator.IngrediTypes.Channel:
                        channel = core.FindChannel(curId);
                        cb = new ThingWithLabel()
                        {
                            ThingBase = channel,
                            Tag = ingrediType,
                            IsChecked = preFilterList.Contains(curId),
                        };
                        cb.CheckChanged += PreIngredi_CheckChanged;
                        wPanel.Children.Add(cb);
                        cb.TxLabel1 = "";
                        cb.TxLabel2 = GetSpeedTx(channel.speed);
                        cb.TxLabel3 = lb3Tx;
                        break;
                    case Calculator.IngrediTypes.Procedure:
                        recipe = core.FindRecipe(curId);
                        cb = new ThingWithLabel()
                        {
                            ThingBase = recipe,
                            Tag = ingrediType,
                            IsChecked = preFilterList.Contains(curId),
                        };

                        wPanel.Children.Add(cb);

                        cb.CheckChanged += PreIngredi_CheckChanged;
                        cb.TxLabel1 = $"{Environment.NewLine}{Environment.NewLine}Total";
                        cb.TxLabel2 = GetSpeedTx(GetSpeedTotal(recipe));
                        cb.TxLabel3 = lb3Tx;
                        break;
                }
            }
            string GetSpeedTx(decimal? speedBase)
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
            decimal GetSpeedTotal(Recipes.Recipe recipe)
            {
                decimal total = 0;
                foreach (Recipes.Recipe.PIOItem pio in recipe.outputs)
                {
                    if (pio.quantity == null)
                    {
                        throw ProcessingChains.SearchHelper.Error_Recipe_Output_Quantity_isNull(pio.thing?.name, recipe.name);
                    }
                    total += pio.quantity.ValueCurrentInGeneral;
                }
                if (recipe.period == null)
                {
                    throw ProcessingChains.SearchHelper.Error_Recipe_Peroid_isNullOrZero(recipe.name);
                }
                return total / (decimal)recipe.period;
            }
        }

        private void PreIngredi_CheckChanged(object? sender, EventArgs e)
        {
            //if (sender is not ThingWithLabel)
            //{
            //    return;
            //}
            //ThingWithLabel twl = (ThingWithLabel)sender;
            //if (twl.Tag is not Calculator.IngrediTypes)
            //{
            //    return;
            //}
            //Calculator.IngrediTypes type = (Calculator.IngrediTypes)twl.Tag;
            //if (twl.IsChecked)
            //{
            //    calculator.AddFilter(twl.ThingBase, type);
            //}
            //else
            //{
            //    calculator.RemoveFilter(twl.ThingBase, type);
            //}

            btn_search.IsEnabled = true;
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
                MessageBox.Show(this,
                    (string)Application.Current.TryFindResource("lb_winCalculator_msgBox_selectAProcessToContinue_content"),
                    (string)Application.Current.TryFindResource("lb_winCalculator_msgBox_selectAProcessToContinue_title"),
                    MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            tabMain.SelectedIndex = 1;
        }

        private DateTime dataGrid_pre_PreviewMouseDownTime = DateTime.MinValue;
        private object dataGrid_pre_selectedItemPre = null;
        private async void dataGrid_pre_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DateTime now = DateTime.Now;
            if ((now - dataGrid_pre_PreviewMouseDownTime).TotalMilliseconds <= Core.Instance.mouseDoubleClickInterval)
            {
                dataGrid_pre_selectProcessNShowQuantity();
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
                dataGrid_pre_selectProcessNShowQuantity();
            }
        }
        private async void dataGrid_pre_selectProcessNShowQuantity()
        {
            if (dataGrid_pre_selectedItemPre != dataGrid_pre.SelectedItem)
            {
                ClearQuantity();
                graphAlpha_clear();
            }
            await Task.Delay(10);
            tabMain.SelectedIndex = 2;
            dataGrid_pre_selectedItemPre = dataGrid_pre.SelectedItem;
        }


        #endregion


        #region quantity
        private void ClearQuantity()
        {
            tip_qt.ThingId = Guid.Empty;

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
            tip_qt.ThingId = tiq_targetProduct.ThingId;

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

        struct CounterItem
        {
            public Guid id;
            public decimal count;
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
                List<CounterItem> counter = new List<CounterItem>();
                Things.Thing t;
                for (i = 0, iv = selectedProcessingCombing.searchResult.allProcesses.Count; i < iv; ++i)
                {
                    pNode = selectedProcessingCombing.searchResult.allProcesses[i];
                    foreach (Recipes.Recipe.PIOItem a in pNode.recipe.accessories)
                    {
                        if (a.thing == null)
                        {
                            throw new Exception("Accessory is null.");
                        }
                        t = a.thing;
                        //if (counter.ContainsKey(t.id))
                        //{
                        //    counter[t.id] += pNode.calMultiple;
                        //}
                        //else
                        //{
                        counter.Add(new CounterItem()
                        {
                            id = t.id,
                            count = pNode.calMultiple,
                        });
                        //}
                    }
                }
                decimal count;
                CounterItem ci;
                for (i = 0, iv = sp_qtA.Children.Count; i < iv; ++i)
                {
                    ui = (ThingWithLabel)sp_qtA.Children[i];
                    if (ui.ThingBase == null)
                    {
                        continue;
                    }
                    ci = counter[i];
                    if (ci.id == ui.ThingBase.id)
                    {
                        ui.TxLabel2 = SimpleStringHelper.UnitsOfMeasure.GetShortString(ci.count, "")
                            + Environment.NewLine
                            + "(" + Math.Ceiling(ci.count) + ")";
                    }
                }
            }

            ProcessingChains.ProcessingHead pHead;
            UpdateInfo(selectedProcessingCombing.searchResult.allSources, ref sp_qtI);
            UpdateInfo(selectedProcessingCombing.searchResult.allFinalProducts, ref sp_qtO);

            QuantityRefreshNumberUDs();
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
        private void QuantityRefreshNumberUDs()
        {
            quantity_nud_codeSetting = true;
            if (selectedProcessingCombing == null)
            {
                nud_qtSec.Value = 0;
                nud_qtMin.Value = 0;
                nud_qtHour.Value = 0;
            }
            else
            {
                nud_qtSec.Value = selectedProcessingCombing.searchResult.NewSpeed;
                nud_qtMin.Value = selectedProcessingCombing.searchResult.NewSpeed * 60;
                nud_qtHour.Value = selectedProcessingCombing.searchResult.NewSpeed * 3600;
            }
            quantity_nud_codeSetting = false;
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
                decimal baseMul = 1;
                decimal uiValue = ui.Value;
                if (ui == nud_qtMin)
                {
                    baseMul = 60;
                }
                else if (ui == nud_qtHour)
                {
                    baseMul = 3600;
                }
                quantity_setNewQuantity(uiValue / baseMul);
            }
            else if (e.Key == Key.Escape)
            {
                NumericUpDown ui = (NumericUpDown)sender;
                decimal baseMul = 1;
                if (ui == nud_qtMin)
                {
                    baseMul = 60;
                }
                else if (ui == nud_qtHour)
                {
                    baseMul = 3600;
                }
                quantity_nud_codeSetting = true;
                ui.Value = selectedProcessingCombing.searchResult.NewSpeed * baseMul;
                quantity_nud_codeSetting = false;
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
            decimal baseMul = 1;
            if (ui == nud_qtMin)
            {
                baseMul = 60;
            }
            else if (ui == nud_qtHour)
            {
                baseMul = 3600;
            }
            quantity_setNewQuantity(ui.Value / baseMul);
        }

        private bool quantity_nud_codeSetting = false;
        private void quantity_nud_ValueChanged(NumericUpDown sender)
        {
            if (quantity_nud_codeSetting)
            {
                return;
            }
            quantity_nud_LostFocus(sender, null);
        }

        private void quantity_setNewQuantity(decimal newSpeed)
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
