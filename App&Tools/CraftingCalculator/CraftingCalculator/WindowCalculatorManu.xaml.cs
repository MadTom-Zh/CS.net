using MadTomDev.App.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
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

using MadTomDev.Common;
using MadTomDev.App.Ctrls;

using MadTomDev.App.SubWindows;
using System.IO;
using static MadTomDev.App.Classes.FlowGraphAlpha;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Cursors = System.Windows.Input.Cursors;
using DragDropEffects = System.Windows.DragDropEffects;
using MessageBox = System.Windows.MessageBox;

using HorizontalAlignment = System.Windows.HorizontalAlignment;
using static MadTomDev.App.Classes.FlowGraphAlpha_Manu;
using MadTomDev.UI;
using System.Drawing;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Brushes = System.Windows.Media.Brushes;
using Size = System.Windows.Size;
using System.CodeDom;
using static System.Windows.Forms.LinkLabel;
using static MadTomDev.App.Classes.Recipes;
using MadTomDev.UI.Class;

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for WindowCalculatorManu.xaml
    /// </summary>
    public partial class WindowCalculatorManu : Window
    {
        public WindowCalculatorManu()
        {
            InitializeComponent();
        }

        Core core = Core.Instance;
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await core.SetCursorWait(this);

            tbv_sceneChain.Text = core.SceneFullPath;

            // graph
            graphFGAM = new FlowGraphAlpha_Manu(ref canvas_graph);
            graphFGAM.PanelTitleMouseDown += graphFGAM_PanelTitleMouseDown;
            graphFGAM.PanelResizeHandleMouseDown += graphFGAM_PanelResizeHandleMouseDown;

            // load graph tool bar, channel btns
            ReloadGraphToolBarSelection();
            ReLoadChannelButtons();
            ReSetChannelButtonsWidth();

            await core.SetCursorArrow(this);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (GraphNeedSave)
            {
                if (MessageBox.Show(this,
                    "Do you want to save" +
                    " before exit?", "Graph changed",
                    MessageBoxButton.YesNo, MessageBoxImage.Question)
                     == MessageBoxResult.Yes)
                {
                    btn_graphSave_Click(sender, new RoutedEventArgs());
                }
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Back:
                    {
                        if (tabControl.SelectedIndex == 0)
                        {
                            // recipe view history, go back
                            List<ViewHistory.Item> history = recipeViewHistory.Read_History();
                            if (history.Count > 0 && recipeViewHistoryOffset < history.Count)
                            {
                                ViewHistory.Item recent = history[recipeViewHistoryOffset++];
                                bool tagValue = (bool)recent.tag;
                                lookingFor_product_orSource = tagValue;
                                RecipeReloadAll(Guid.Parse(recent.Name), tagValue);
                            }
                        }
                    }
                    break;
                case Key.S:
                    {
                        if (tabControl.SelectedIndex != 1)
                        {
                            break;
                        }
                        if (Keyboard.IsKeyDown(Key.LeftCtrl)
                            || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            if (Keyboard.IsKeyDown(Key.LeftAlt)
                                || Keyboard.IsKeyDown(Key.RightAlt))
                            {
                                btn_graphSaveAs_Click(sender, e);
                            }
                            else
                            {
                                btn_graphSave_Click(sender, e);
                            }
                        }
                    }
                    break;
            }
        }


        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (tabControl.SelectedIndex)
            {
                case 0: // recipe
                    {
                        tabControl_SelectionChanged_quitGraph(true);
                        btn_simSpeedReset_Click(tabControl, new RoutedEventArgs());
                    }
                    break;
                case 1: // graph
                    {
                        // reload
                        tabControl_SelectionChanged_quitGraph(false);
                        btn_simSpeedReset_Click(tabControl, new RoutedEventArgs());

                        Graph_ReloadFileList();
                    }
                    break;
                case 2: // sim speed
                    {
                        tabControl_SelectionChanged_quitGraph(true);

                        btn_simSpeedReset_Click(tabControl, new RoutedEventArgs());
                    }
                    break;
            }
        }
        private void tabControl_SelectionChanged_quitGraph(bool isQuit)
        {
            if (btn_arrow.IsChecked != true)
            {
                btn_arrow_Click(tabControl, new RoutedEventArgs());
            }
            sp_graphToolbar.IsEnabled = !isQuit;
        }



        #region window mouse events, move canvas, zoom
        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point curMousePoint = Mouse.GetPosition(grid_graph);
            if (movingOrResizingPanel is not null)
            {
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
            else if (graph_moving)
            {
                // moving graph alpha
                sv_graph.ScrollToHorizontalOffset(graph_startScrollOffset.X + graph_startMousePoint.X - curMousePoint.X);
                sv_graph.ScrollToVerticalOffset(graph_startScrollOffset.Y + graph_startMousePoint.Y - curMousePoint.Y);
            }
            else if (graph_selecting)
            {
                graphFGAM_SelectNodesSizing(curMousePoint);
            }
        }
        private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseOperateEnd();
        }
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseOperateEnd();
        }
        private void MouseOperateEnd()
        {
            Point curMousePoint = Mouse.GetPosition(grid_graph);

            MoveHandleEnd(
                curMousePoint.X - panelMousePointStart.X,
                curMousePoint.Y - panelMousePointStart.Y);
            graph_moving = false;

            if (graph_selecting)
            {
                graphFGAM_SelectNodesEnd(curMousePoint);
                graph_selecting = false;
            }
            if (graphFGAM.notePanel_withLMD is not null)
            {
                NoteLineEnd(curMousePoint);
            }
            Cursor = Cursors.Arrow;
        }
        #endregion


        #region tab recipes

        private bool? lookingFor_product_orSource = null;
        private ViewHistory recipeViewHistory = new ViewHistory(20, "/", false);
        private int recipeViewHistoryOffset = 0;
        private void tiq_lookingFor_IconDoubleClicked(object sender, EventArgs e)
        {
            WindowSelectThings? win = core.ShowSelectThingsWin(this);

            if (win != null
                && win.SelectedThingIdList != null
                && win.SelectedThingIdList.Count == 1)
            {
                tiq_lookingFor.ThingId = win.SelectedThingIdList[0];
                lookingFor_product_orSource = true;
                if (RecipeReloadAll(win.SelectedThingIdList[0], true)
                    == false)
                {
                    lookingFor_product_orSource = false;
                    RecipeReloadAll(win.SelectedThingIdList[0], false);
                }
                recipeViewHistory.Write_History(
                    win.SelectedThingIdList[0].ToString(),
                    null,
                    lookingFor_product_orSource);
                recipeViewHistoryOffset = 0;
            }
        }

        private bool RecipeReloadAll(Guid thingId, bool isProduction)
        {
            // clear panels
            ClearSPRecipesInputs();
            ClearSPRecipes();
            ClearSPRecipesOutputs();

            // load
            List<Recipes.Recipe> foundRecipes;
            if (isProduction)
            {
                foundRecipes = core.FindRecips_byOutputs(new Guid[] { thingId });
            }
            else
            {
                foundRecipes = core.FindRecipes_byInputs(new Guid[] { thingId });
            }
            if (foundRecipes.Count == 0)
            {
                return false;
            }

            // sort
            Recipes.Recipe r1, r2;
            for (int i = 0, j, iv = foundRecipes.Count - 1, jv = iv + 1; i < iv; ++i)
            {
                r1 = foundRecipes[i];
                for (j = i + 1; j < jv; ++j)
                {
                    r2 = foundRecipes[j];
                    if (r1.processor == r2.processor)
                    {
                        if (j == i + 1)
                        {
                            break;
                        }

                        foundRecipes.RemoveAt(j);
                        foundRecipes.Insert(i + 1, r2);
                        break;
                    }
                }
            }
            // fill ui
            foreach (Recipes.Recipe r in foundRecipes)
            {
                ThingWithLabel ui = new ThingWithLabel()
                {
                    ThingBase = r,
                    TxLabel1 = "",
                    TxLabel2 = "",
                    TxLabel3 = "",
                    //TxLabel3 = "A:" + r.accessories.Count.ToString() + Environment.NewLine
                    //            + "I:" + r.inputs.Count.ToString() + Environment.NewLine
                    //            + "O:" + r.outputs.Count.ToString(),
                    IsCheckable = false,
                };
                if (r.outputs.Count > 0)
                {
                    Recipes.Recipe.PIOItem outItem = r.outputs[0];
                    if (outItem.quantity != null)
                    {
                        ui.TxLabel3 = ThingWithLabel.GetCommonSpeed(r.GetBaseSpeed_perSec(false, 0));
                    }
                }
                ui.PreviewMouseLeftButtonDown += UiRecipe_PreviewMouseLeftButtonDown;
                sp_recipes.Children.Add(ui);

                TryAddRecipeInputOutputItem(r, true);
                TryAddRecipeInputOutputItem(r, false);
            }
            return true;
        }


        private void ClearSPRecipes()
        {
            recipeHighlight = null;

            foreach (UIElement u in sp_recipes.Children)
            {
                if (u is not ThingWithLabel)
                {
                    continue;
                }
                ((ThingWithLabel)u).PreviewMouseLeftButtonDown -= UiRecipe_PreviewMouseLeftButtonDown;
            }
            sp_recipes.Children.Clear();
        }
        private void ClearSPRecipesInputs()
        {
            recipeHighlightInputs.Clear();

            foreach (UIElement u in sp_recipeInputs.Children)
            {
                if (u is not ThingWithLabel)
                {
                    continue;
                }
                ((ThingWithLabel)u).PreviewMouseDoubleClick -= UiRecipeInput_PreviewMouseDoubleClick;
            }
            sp_recipeInputs.Children.Clear();
        }
        private void ClearSPRecipesOutputs()
        {
            recipeHighlightOutputs.Clear();

            foreach (UIElement u in sp_recipeOutputs.Children)
            {
                if (u is not ThingWithLabel)
                {
                    continue;
                }
                ((ThingWithLabel)u).PreviewMouseDoubleClick -= UiRecipeOutput_PreviewMouseDoubleClick;
            }
            sp_recipeOutputs.Children.Clear();
        }

        private ThingWithLabel? recipeHighlight = null;
        private void UiRecipe_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ThingWithLabel)
            {
                return;
            }
            ThingWithLabel recipeUI = (ThingWithLabel)sender;
            // set background
            if (recipeHighlight != null)
            {
                if (recipeHighlight != recipeUI)
                {
                    recipeHighlight.BackgroundColor = Colors.Transparent;
                    recipeUI.BackgroundColor = Core.ColorProcedure;
                }
            }
            else
            {
                recipeUI.BackgroundColor = Core.ColorProcedure;
            }
            recipeHighlight = recipeUI;

            // set input ui
            if (recipeUI.ThingBase != null)
            {
                RecipeGroupItems((Recipes.Recipe)recipeUI.ThingBase, true);
            }
            else
            {
                RecipeGroupItems(null, true);
            }

            // set output ui
            if (recipeUI.ThingBase != null)
            {
                RecipeGroupItems((Recipes.Recipe)recipeUI.ThingBase, false);
            }
            else
            {
                RecipeGroupItems(null, false);
            }
        }
        private List<ThingWithLabel> recipeHighlightInputs = new List<ThingWithLabel>();
        private List<ThingWithLabel> recipeHighlightOutputs = new List<ThingWithLabel>();
        private void RecipeGroupItems(Recipes.Recipe? recipe, bool inputsOrOutputs)
        {
            List<Ctrls.ThingWithLabel> uiHighlightList;
            StackPanel spContainer;
            if (inputsOrOutputs)
            {
                uiHighlightList = recipeHighlightInputs;
                spContainer = sp_recipeInputs;
            }
            else
            {
                uiHighlightList = recipeHighlightOutputs;
                spContainer = sp_recipeOutputs;
            }

            if (recipe == null)
            {
                foreach (Ctrls.ThingWithLabel ui in uiHighlightList)
                {
                    ui.BackgroundColor = Colors.Transparent;
                }
                return;
            }
            List<Recipes.Recipe.PIOItem> items;
            if (inputsOrOutputs)
            {
                items = recipe.inputs;
            }
            else
            {
                items = recipe.outputs;
            }


            // group n' set background
            Recipes.Recipe.PIOItem curItem;
            int oriUIdx;
            List<Ctrls.ThingWithLabel> curHighlightUIList = new List<Ctrls.ThingWithLabel>();
            for (int i = 0, iv = items.Count; i < iv; ++i)
            {
                curItem = items[i];
                if (curItem.thing == null)
                {
                    continue;
                }

                Ctrls.ThingWithLabel? foundUI = SpFindElement(curItem.thing.id, out oriUIdx);
                if (oriUIdx < 0)
                {
                    continue;
                }

                if (i < oriUIdx)
                {
                    spContainer.Children.RemoveAt(oriUIdx);
                    spContainer.Children.Insert(i, foundUI);
                }

                if (foundUI != null)
                {
                    if (uiHighlightList.Contains(foundUI))
                    {
                        uiHighlightList.Remove(foundUI);
                    }
                    else
                    {
                        foundUI.BackgroundColor = inputsOrOutputs ? Core.ColorInput : Core.ColorOutput;
                    }
                    curHighlightUIList.Add(foundUI);
                }
            }
            foreach (Ctrls.ThingWithLabel ui in uiHighlightList)
            {
                ui.BackgroundColor = Colors.Transparent;
            }
            uiHighlightList.Clear();
            uiHighlightList.AddRange(curHighlightUIList);
            curHighlightUIList.Clear();


            Ctrls.ThingWithLabel? SpFindElement(Guid itmeId, out int childIndex)
            {
                childIndex = -1;
                UIElement u;
                Ctrls.ThingWithLabel ui;
                for (int i = 0, iv = spContainer.Children.Count; i < iv; ++i)
                {
                    u = spContainer.Children[i];
                    if (u is not Ctrls.ThingWithLabel)
                    {
                        continue;
                    }

                    ui = (Ctrls.ThingWithLabel)u;
                    if (ui.ThingBase != null && ui.ThingBase.id == itmeId)
                    {
                        childIndex = i;
                        return ui;
                    }
                }
                return null;
            }
        }

        private void TryAddRecipeInputOutputItem(Recipes.Recipe recipe, bool inputsOrOutputs)
        {
            List<Recipes.Recipe.PIOItem> items;
            StackPanel spContainer;
            if (inputsOrOutputs)
            {
                items = recipe.inputs;
                spContainer = sp_recipeInputs;
            }
            else
            {
                items = recipe.outputs;
                spContainer = sp_recipeOutputs;
            }
            bool exists;
            Ctrls.ThingWithLabel ui;
            foreach (Recipes.Recipe.PIOItem item in items)
            {
                exists = false;
                foreach (UIElement u in spContainer.Children)
                {
                    if (u is not Ctrls.ThingWithLabel)
                    {
                        continue;
                    }
                    ui = (Ctrls.ThingWithLabel)u;
                    if (ui.ThingBase?.id == item.thing?.id)
                    {
                        exists = true;
                        break;
                    }
                }
                if (exists)
                {
                    continue;
                }

                ui = new Ctrls.ThingWithLabel()
                {
                    ThingBase = item.thing,
                    IsNumberLabelsVisible = false,
                    IsCheckable = false,
                };
                if (inputsOrOutputs)
                {
                    ui.PreviewMouseDoubleClick += UiRecipeInput_PreviewMouseDoubleClick;
                }
                else
                {
                    ui.PreviewMouseDoubleClick += UiRecipeOutput_PreviewMouseDoubleClick;
                }
                spContainer.Children.Add(ui);
            }
        }

        private void UiRecipeInput_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ThingWithLabel)
            {
                return;
            }
            ThingWithLabel ui = (ThingWithLabel)sender;
            if (ui.ThingBase == null)
            {
                return;
            }
            if (lookingFor_product_orSource == true
                && tiq_lookingFor.ThingId == ui.ThingBase.id)
            {
                return;
            }

            tiq_lookingFor.ThingId = ui.ThingBase.id;
            lookingFor_product_orSource = true;
            RecipeReloadAll(ui.ThingBase.id, true);
            recipeViewHistory.Write_History(ui.ThingBase.id.ToString(), null, true);
            recipeViewHistoryOffset = 0;
        }
        private void UiRecipeOutput_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ThingWithLabel)
            {
                return;
            }
            ThingWithLabel ui = (ThingWithLabel)sender;
            if (ui.ThingBase == null)
            {
                return;
            }
            if (lookingFor_product_orSource == false
                && tiq_lookingFor.ThingId == ui.ThingBase.id)
            {
                return;
            }

            tiq_lookingFor.ThingId = ui.ThingBase.id;
            lookingFor_product_orSource = false;
            RecipeReloadAll(ui.ThingBase.id, false);
            recipeViewHistory.Write_History(ui.ThingBase.id.ToString(), null, false);
            recipeViewHistoryOffset = 0;
        }

        #endregion


        #region graphs in canvas

        FlowGraphAlpha_Manu graphFGAM;

        #region drag recipe from
        private Point? recipeMouseDownPoint;
        private ThingWithLabel? recipeDragUi = null;
        private void sp_recipes_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mp = Mouse.GetPosition(sp_recipes);
            FrameworkElement fe = UI.VisualHelper.StackPanel.GetItemUI(sp_recipes, typeof(ThingWithLabel), mp);
            if (fe is not null)
            {
                recipeMouseDownPoint = mp;
                recipeDragUi = (ThingWithLabel)fe;
            }
            else
            {
                recipeMouseDownPoint = null;
                recipeDragUi = null;
            }
        }

        private void sp_recipes_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            recipeMouseDownPoint = null;
            recipeDragUi = null;
        }
        private void sp_recipes_MouseLeave(object sender, MouseEventArgs e)
        {
            recipeMouseDownPoint = null;
            recipeDragUi = null;
        }

        private void sp_recipes_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (recipeMouseDownPoint is null
                || recipeDragUi is null
                || recipeDragUi.ThingBase is null)
            {
                return;
            }

            Point strMP = recipeMouseDownPoint.Value;
            Point curMP = Mouse.GetPosition(sp_recipes);
            if (Math.Abs(curMP.X - strMP.X) > 3 || Math.Abs(curMP.Y - strMP.Y) > 3)
            {
                Recipes.Recipe recipe = (Recipes.Recipe)recipeDragUi.ThingBase;
                MouseNKeyboardHelper.DragObject(recipeDragUi, DragDropEffects.Link, typeof(Recipes.Recipe).ToString(), recipe);
            }
        }
        #endregion

        #region drop recipe to
        private void sv_graph_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string dataFormat = typeof(Recipes.Recipe).ToString();
            if (!e.Data.GetDataPresent(dataFormat))
            {
                return;
            }

            // if already has a recipe with same outputs
            Recipes.Recipe recipe = (Recipes.Recipe)e.Data.GetData(dataFormat);
            if (graphFGAM.CheckHasRecipe_withSameOutputs(recipe, out Recipes.Recipe? existRecipe))
            {
                if (MessageBox.Show(this,
                    $"Already has a recipe[{existRecipe?.name}] with same outputs.{Environment.NewLine}Continue adding?", "Note",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return;
                }
            }
            else if (graphFGAM.CheckHasRecipe(recipe))
            {
                if (MessageBox.Show(this,
                    $"Already has the recipe[{recipe?.name}].{Environment.NewLine}Continue adding?", "Note",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return;
                }
            }

            Point mp = e.GetPosition(canvas_graph);
            graphFGAM.ManuAddPPanel(recipe, mp.X, mp.Y);
            GraphNeedSave = true;
        }
        #endregion


        #region canvas scall, move, selecting nodes


        private double canvas_graph_curScaleValue = 1;
        private void canvas_graph_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            if (linkMode_firstPointChecked)
            {
                return;
            }
            canvas_graph_Zoom(Mouse.GetPosition(sv_graph), (double)e.Delta / 1200);
        }
        private bool canvas_graph_Zooming = false;
        private double canvas_graph_Zoom_Min = 0.2;
        private double canvas_graph_Zoom_Max = 10;
        private void canvas_graph_Zoom(Point? centerPoint = null, double scaleOffset = 0.1)
        {
            if (canvas_graph_Zooming || scaleOffset == 0)
            {
                return;
            }
            if (canvas_graph_curScaleValue <= canvas_graph_Zoom_Min && scaleOffset < 0)
            {
                return;
            }
            if (canvas_graph_curScaleValue >= canvas_graph_Zoom_Max && scaleOffset > 0)
            {
                return;
            }
            canvas_graph_Zooming = true;

            double oriScaleValue = canvas_graph_curScaleValue;
            canvas_graph_curScaleValue += scaleOffset;
            if (canvas_graph_curScaleValue < canvas_graph_Zoom_Min)
            {
                canvas_graph_curScaleValue = canvas_graph_Zoom_Min;
            }
            else if (canvas_graph_Zoom_Max < canvas_graph_curScaleValue)
            {
                canvas_graph_curScaleValue = canvas_graph_Zoom_Max;
            }

            Point oriScollOffset = new Point(
                sv_graph.HorizontalOffset,
                sv_graph.VerticalOffset);
            Point oriPtOnGraph = Mouse.GetPosition(bdr_graph);

            // 缩放到新比例
            ScaleTransform st;
            if (canvas_graph.RenderTransform is ScaleTransform)
            {
                st = (ScaleTransform)canvas_graph.RenderTransform;
            }
            else
            {
                st = new ScaleTransform();
                canvas_graph.RenderTransform = st;
            }
            st.ScaleY = st.ScaleX = canvas_graph_curScaleValue;
            bdr_graph_syncSize();

            // 按照缩放控制点，重新挪动整图位置
            Point cp;
            if (centerPoint == null)
            {
                cp = new Point(
                    sv_graph.ActualWidth / 2,
                    sv_graph.ActualHeight / 2);
            }
            else
            {
                cp = (Point)centerPoint;
            }


            //await Task.Delay(50);

            double scollMultiple = canvas_graph_curScaleValue / oriScaleValue - 1;
            sv_graph.ScrollToHorizontalOffset(oriScollOffset.X + (oriPtOnGraph.X * scollMultiple));
            sv_graph.ScrollToVerticalOffset(oriScollOffset.Y + (oriPtOnGraph.Y * scollMultiple));

            canvas_graph_Zooming = false;
        }

        private void canvas_graph_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            bdr_graph_syncSize();
        }
        private void bdr_graph_syncSize()
        {
            bdr_graph.Width = canvas_graph.ActualWidth * canvas_graph_curScaleValue + 1;
            bdr_graph.Height = canvas_graph.ActualHeight * canvas_graph_curScaleValue + 1;
        }

        private bool graph_moving = false;
        private Point graph_startMousePoint;
        private Point graph_startScrollOffset;

        private bool graph_selecting = false;
        /// <summary>
        /// selecting mode, 0-reSelect, 1-add, 2-remove
        /// </summary>
        private int graph_selectingMode = 0;
        private Rectangle selectingFrameRect = new Rectangle()
        {
            Stroke = Brushes.DarkOrange,
            StrokeThickness = 2,
            Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(20, 255, 128, 00)),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
        };

        private void canvas_graph_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (grid_graph.Children.Contains(CanvasMouseLMBDown_LinkMode_visualLine))
            {
                return;
            }
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                graph_moving = true;
                graph_startMousePoint = Mouse.GetPosition(grid_graph);
                graph_startScrollOffset = new Point(
                    sv_graph.HorizontalOffset,
                    sv_graph.VerticalOffset);
                Cursor = Cursors.Hand;
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                SelectedUIInfo curUIInfo = new SelectedUIInfo();
                graphFGAM.GetUIInfo(ref curUIInfo, canvas_graph, Mouse.GetPosition(canvas_graph));
                if (curUIInfo.notePanel is not null
                    && curUIInfo.notePanel.LineHandleIsMouseOver)
                {
                    // start line a note
                }
                else
                {
                    // 2024 1111
                    // 如果鼠标在节点的标题上，则不进行选择（进行移动）
                    bool movinePanels = false;
                    if (curUIInfo.ioPanel is not null
                        && selectedIOPanels.Contains(curUIInfo.ioPanel)
                        && curUIInfo.ioPanel.CheckMouseOnTitle())
                    {
                        movinePanels = true;
                    }
                    else if (curUIInfo.processPanel is not null
                        && selectedProcessPanels.Contains(curUIInfo.processPanel)
                        && curUIInfo.processPanel.CheckMouseOnTitle())
                    {
                        movinePanels = true;
                    }
                    else if (curUIInfo.notePanel is not null
                        && selectedNotePanels.Contains(curUIInfo.notePanel)
                        && curUIInfo.notePanel.CheckMouseOnTitle())
                    {
                        movinePanels = true;
                    }

                    if (movinePanels == false)
                    {
                        graph_selecting = true;
                        graph_startMousePoint = Mouse.GetPosition(grid_graph);
                        graphFGAM_SelectNodesStart();
                    }
                }
            }
        }
        private void canvas_graph_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CanvasMouseLMBDown_NormalMode();
            CanvasMouseLMBDown_LinkMode();
        }

        private void canvas_graph_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

        }
        private void canvas_graph_PreviewMouseMove(object sender, MouseEventArgs e)
        {
        }
        private void grid_graph_MouseMove(object sender, MouseEventArgs e)
        {
            CanvasMouseMove_NormalMode();
            CanvasMouseMove_LinkMode();
        }


        private void graphFGAM_SelectNodesStart()
        {
            bool isShift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            //bool isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            bool isAlt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);

            if (isShift && isAlt)
            {
                GraphDeselectAll();
                graph_selectingMode = 0;
            }
            else if (isShift)
            {
                graph_selectingMode = 1;
            }
            else if (isAlt)
            {
                graph_selectingMode = 2;
            }
            else
            {
                GraphDeselectAll();
                graph_selectingMode = 0;
            }

            selectingFrameRect.Width = 0;
            selectingFrameRect.Height = 0;
            if (grid_graph.Children.Contains(selectingFrameRect) == false)
            {
                grid_graph.Children.Add(selectingFrameRect);
            }
        }
        private void graphFGAM_SelectNodesSizing(Point curMousePoint)
        {
            graphFGAM_setSelectingFrameRect(ref curMousePoint);
            graphFGAM_SelectNodes();
        }
        private void graphFGAM_setSelectingFrameRect(ref Point curMousePoint)
        {
            double x, y, w, h;
            if (graph_startMousePoint.X < curMousePoint.X)
            {
                x = graph_startMousePoint.X;
                w = curMousePoint.X - x;
            }
            else
            {
                x = curMousePoint.X;
                w = graph_startMousePoint.X - x;
            }
            if (graph_startMousePoint.Y < curMousePoint.Y)
            {
                y = graph_startMousePoint.Y;
                h = curMousePoint.Y - y;
            }
            else
            {
                y = curMousePoint.Y;
                h = graph_startMousePoint.Y - y;
            }

            //w *= canvas_graph_curScaleValue;
            //h *= canvas_graph_curScaleValue;
            selectingFrameRect.Margin = new Thickness(x, y, 0, 0);
            selectingFrameRect.Width = w;
            selectingFrameRect.Height = h;
        }
        private void graphFGAM_SelectNodes()
        {
            // in range selectingFrameRect
            double x, y, w, h;
            Thickness frameMargin = selectingFrameRect.Margin;
            Point oPoint = new Point();
            Point bPoint = bdr_graph.TranslatePoint(oPoint, sv_graph);
            x = frameMargin.Left - bPoint.X;
            x /= canvas_graph_curScaleValue;
            y = frameMargin.Top - bPoint.Y;
            y /= canvas_graph_curScaleValue;

            w = selectingFrameRect.ActualWidth / canvas_graph_curScaleValue;
            h = selectingFrameRect.ActualHeight / canvas_graph_curScaleValue;

            RectangleF selectingRange = new RectangleF((float)x, (float)y, (float)w, (float)h);

            QuickCheckNCalculate.GraphRelations gr = QuickCheckNCalculate.CheckRectangleRelation(
                selectingRange, graphFGAM.inputPanel.GetBoundsRect());
            if (gr != QuickCheckNCalculate.GraphRelations.Outside)
            {
                GraphSelectIOPanel(graphFGAM.inputPanel, graph_selectingMode != 2);
            }
            else if (graph_selectingMode == 0)
            {
                GraphSelectIOPanel(graphFGAM.inputPanel, false);
            }
            gr = QuickCheckNCalculate.CheckRectangleRelation(
                selectingRange, graphFGAM.outputPanel.GetBoundsRect());
            if (gr != QuickCheckNCalculate.GraphRelations.Outside)
            {
                GraphSelectIOPanel(graphFGAM.outputPanel, graph_selectingMode != 2);
            }
            else if (graph_selectingMode == 0)
            {
                GraphSelectIOPanel(graphFGAM.outputPanel, false);
            }

            foreach (ProcessPanel pPanel in graphFGAM.processPanelList)
            {
                gr = QuickCheckNCalculate.CheckRectangleRelation(
                    selectingRange, pPanel.GetMainPanelBoundsRect());
                if (gr != QuickCheckNCalculate.GraphRelations.Outside)
                {
                    GraphSelectProcessPanel(pPanel, graph_selectingMode != 2);
                }
                else if (graph_selectingMode == 0)
                {
                    GraphSelectProcessPanel(pPanel, false);
                }
            }
            double lineCenterXOffset = graphFGAM.basicConfig.lineDotDiameter * 1.5;
            foreach (LinkLineV2 lLine in graphFGAM.linkLineList)
            {
                gr = QuickCheckNCalculate.CheckRectangleRelation(
                    selectingRange, lLine.GetMainPanelBoundsRect());
                if (gr != QuickCheckNCalculate.GraphRelations.Outside)
                {
                    GraphSelectLinkLine(lLine, graph_selectingMode != 2);
                }
                else if (graph_selectingMode == 0)
                {
                    GraphSelectLinkLine(lLine, false);
                }
            }
            foreach (FlowGraphAlpha_Manu.NotePanel nPanel in graphFGAM.notePanelList)
            {
                gr = QuickCheckNCalculate.CheckRectangleRelation(
                    selectingRange, nPanel.GetBoundsRect());
                if (gr != QuickCheckNCalculate.GraphRelations.Outside)
                {
                    GraphSelectNotePanel(nPanel, graph_selectingMode != 2);
                }
                else if (graph_selectingMode == 0)
                {
                    GraphSelectNotePanel(nPanel, false);
                }
            }

        }
        private void graphFGAM_SelectNodesEnd(Point curMousePoint)
        {
            if (graph_selecting == false)
            {
                return;
            }

            graphFGAM_setSelectingFrameRect(ref curMousePoint);
            graphFGAM_SelectNodes();
            grid_graph.Children.Remove(selectingFrameRect);
        }

        #endregion


        #region move/resize panel
        private void graphFGAM_PanelTitleMouseDown(FlowGraphAlpha fga, MovablePanel panel)
        {
            if (GraphWorkMode == GraphWorkModes.Normal)
            {
                MovingOrResizingPanelStart(panel, true);
            }
        }
        private void graphFGAM_PanelResizeHandleMouseDown(FlowGraphAlpha fga, MovablePanel panel)
        {
            if (GraphWorkMode == GraphWorkModes.Normal)
            {
                MovingOrResizingPanelStart(panel, false);
            }
        }
        private void MovingOrResizingPanelStart(MovablePanel panel, bool isMovingOrResizing)
        {
            movingOrResizingPanel = panel;
            movingOrResizing = isMovingOrResizing;

            // add a frame to show new position
            if (isMovingOrResizing)
            {
                double fx1 = double.MaxValue, fy1 = double.MaxValue,
                    fx2 = double.MinValue, fy2 = double.MinValue;
                foreach (IOPanel ioPanel in selectedIOPanels)
                {
                    GetXY(ioPanel, ref fx1, ref fy1, ref fx2, ref fy2);
                }
                foreach (ProcessPanel pPanel in selectedProcessPanels)
                {
                    GetXY(pPanel, ref fx1, ref fy1, ref fx2, ref fy2);
                }
                foreach (NotePanel nPanel in selectedNotePanels)
                {
                    GetXY(nPanel, ref fx1, ref fy1, ref fx2, ref fy2);
                }
                GetXY(panel, ref fx1, ref fy1, ref fx2, ref fy2);
                //panelFrameRect.Width = panel.ActualWidth * canvas_graph_curScaleValue;
                //panelFrameRect.Height = panel.ActualHeight * canvas_graph_curScaleValue;
                double fw = fx2 - fx1, fh = fy2 - fy1;
                panelFrameRect.Width = (fw) * canvas_graph_curScaleValue;
                panelFrameRect.Height = (fh) * canvas_graph_curScaleValue;

                //panelFrameRectPositionStart = panel.TranslatePoint(new Point(), grid_graph);
                //panelFrameRectSizeStart = new Size(panel.ActualWidth, panel.ActualHeight);
                panelFrameRectPositionStart = panel.TranslatePoint(new Point(fx1 - panel.X, fy1 - panel.Y), grid_graph);
                //panelFrameRectPositionStart = grid_graph.TranslatePoint(new Point(fx1, fy1), bdr_graph);
                //panelFrameRectPositionStart = bdr_graph.TranslatePoint(new Point(fx1, fy1), grid_graph);
                panelFrameRectSizeStart = new Size(fw, fh);
            }
            else
            {
                panelFrameRect.Width = panel.ActualWidth * canvas_graph_curScaleValue;
                panelFrameRect.Height = panel.ActualHeight * canvas_graph_curScaleValue;

                panelFrameRectPositionStart = panel.TranslatePoint(new Point(), grid_graph);
                panelFrameRectSizeStart = new Size(panel.ActualWidth, panel.ActualHeight);
            }



            MovePanelFrame(panelFrameRectPositionStart.X, panelFrameRectPositionStart.Y);
            panelMousePointStart = Mouse.GetPosition(grid_graph);
            if (grid_graph.Children.Contains(panelFrameRect) == false)
            {
                grid_graph.Children.Add(panelFrameRect);
            }
            GraphNeedSave = true;

            void GetXY(MovablePanel mPanel, ref double xMin, ref double yMin, ref double xMax, ref double yMax)
            {
                double pX = mPanel.X,
                pY = mPanel.Y;
                if (xMin > pX)
                {
                    xMin = pX;
                }
                if (yMin > pY)
                {
                    yMin = pY;
                }
                double pX2 = pX + mPanel.ActualWidth,
                 pY2 = pY + mPanel.ActualHeight;
                if (xMax < pX2)
                {
                    xMax = pX2;
                }
                if (yMax < pY2)
                {
                    yMax = pY2;
                }
            }
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
                panelFrameRect.Width = panelFrameRectSizeStart.Width * canvas_graph_curScaleValue + (double)offsetX;
            }
            if (offsetY != null)
            {
                panelFrameRect.Height = panelFrameRectSizeStart.Height * canvas_graph_curScaleValue + (double)offsetY;
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

            offsetX /= canvas_graph_curScaleValue;
            offsetY /= canvas_graph_curScaleValue;
            // remove the frame
            grid_graph.Children.Remove(panelFrameRect);

            if (movingOrResizing)
            {
                // set new position
                //FlowGraphAlpha.MovePanel(targetPanel, offsetX, offsetY);
                List<MovablePanel> panels = new List<MovablePanel>();
                panels.AddRange(selectedIOPanels);
                panels.AddRange(selectedProcessPanels);
                panels.AddRange(selectedNotePanels);
                if (panels.Contains(targetPanel) == false)
                {
                    panels.Add(targetPanel);
                }
                FlowGraphAlpha.MovePanels(panels, offsetX, offsetY);
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
            graphFGAM.SetAutoMargin(true);
        }

        private async void NoteLineEnd(Point mPt)
        {
            NotePanel? np = graphFGAM.notePanel_withLMD;
            if (np is null)
            {
                return;
            }
            graphFGAM.GetUIInfo(ref normalMode_cursorUIinfo, canvas_graph, Mouse.GetPosition(canvas_graph));

            graphFGAM.notePanel_withLMD = null;

            //UIElement? optUi, Point? optRefPt
            Point refPt = normalMode_cursorUIinfo.mousePoint;
            if (normalMode_cursorUIinfo.subPanel is not null)
            {
                // sub panel, maybe relate to ioPanel or processPanel
                if (normalMode_cursorUIinfo.ioPanel is not null)
                {
                    RectangleF subPanenBR
                        = normalMode_cursorUIinfo.ioPanel.GetSubPanelBoundsRect(
                            normalMode_cursorUIinfo.ioPanel.IndexOfHead(
                                (ProcessingChains.ProcessingHead)normalMode_cursorUIinfo.subPanel.Tag));
                    refPt.X -= subPanenBR.X + subPanenBR.Width / 2;
                    refPt.Y -= subPanenBR.Y + subPanenBR.Height / 2;
                    np.SetLineEnd(normalMode_cursorUIinfo.subPanel, refPt);
                }
                else
                {
                    refPt.X -= GetCenterX(normalMode_cursorUIinfo.subPanel);
                    refPt.Y -= GetCenterY(normalMode_cursorUIinfo.subPanel);
                    np.SetLineEnd(normalMode_cursorUIinfo.subPanel, refPt);
                }
            }
            else if (normalMode_cursorUIinfo.ioPanel is not null)
            {
                refPt.X -= normalMode_cursorUIinfo.ioPanel.CenterX;
                refPt.Y -= normalMode_cursorUIinfo.ioPanel.CenterY;
                np.SetLineEnd(normalMode_cursorUIinfo.ioPanel, refPt);

            }
            else if (normalMode_cursorUIinfo.processPanel is not null)
            {
                refPt.X -= normalMode_cursorUIinfo.processPanel.CenterX;
                refPt.Y -= normalMode_cursorUIinfo.processPanel.CenterY;
                np.SetLineEnd(normalMode_cursorUIinfo.processPanel, refPt);
            }
            else if (normalMode_cursorUIinfo.linkLine is not null
                && normalMode_cursorUIinfo.linkLine_OnPanel)
            {
                refPt.X -= normalMode_cursorUIinfo.linkLine.CenterX;
                refPt.Y -= normalMode_cursorUIinfo.linkLine.CenterY;
                np.SetLineEnd(normalMode_cursorUIinfo.linkLine, refPt);
            }
            else if (normalMode_cursorUIinfo.notePanel is not null)
            {
                refPt.X -= normalMode_cursorUIinfo.notePanel.CenterX;
                refPt.Y -= normalMode_cursorUIinfo.notePanel.CenterY;
                np.SetLineEnd(normalMode_cursorUIinfo.notePanel, refPt);
            }

            if (np.lineHandle is not null)
            {
                await Task.Delay(60);
                np.lineHandle.Visibility = Visibility.Visible;
                np.Update();
            }


            double GetCenterX(ThingWithLabel subPanel)
            {
                return Canvas.GetLeft(subPanel) + subPanel.ActualWidth / 2;
            }
            double GetCenterY(ThingWithLabel subPanel)
            {
                return Canvas.GetTop(subPanel) + subPanel.ActualHeight / 2;
            }
        }

        #endregion


        #region graph tool bar

        private List<Guid> checkedChannelIdList = new List<Guid>();
        private int checkedChannelBtnWidth = 8;
        private void ReloadGraphToolBarSelection()
        {
            checkedChannelIdList.Clear();
            string file = System.IO.Path.Combine(
                core.sceneMgr.selectedTreeViewNode.GetDirFullName(),
                SceneMgr.FILENAME_FGAM_COLLAPSED_CHANNELS);
            if (!File.Exists(file))
            {
                return;
            }
            Guid testId;
            foreach (string line in File.ReadAllLines(file))
            {
                if (Guid.TryParse(line, out testId))
                {
                    checkedChannelIdList.Add(testId);
                }
            }
        }
        private void SaveGraphToolBarSelection()
        {
            string file = System.IO.Path.Combine(
                core.sceneMgr.selectedTreeViewNode.GetDirFullName(),
                SceneMgr.FILENAME_FGAM_COLLAPSED_CHANNELS);
            using (FileStream fs = File.OpenWrite(file))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                foreach (Guid id in checkedChannelIdList)
                {
                    sw.WriteLine(id.ToString());
                }
                sw.Flush();
                fs.Flush();
            }
        }
        private void ReLoadChannelButtons()
        {
            // remove ori buttons
            for (int i = sp_graphToolbar.Children.Count - 1; 2 <= i; --i)
            {
                if (sp_graphToolbar.Children[i] is ThingWithLabel)
                {
                    ((ThingWithLabel)sp_graphToolbar.Children[i]).PreviewMouseLeftButtonDown
                        -= ChannelBtn_PreviewMouseLeftButtonDown;
                }

                sp_graphToolbar.Children.RemoveAt(i);
            }

            // load new buttons
            ThingWithLabel newBtn;
            foreach (VMs.DataGridItemModelChannel c in Core.Instance.calculationChannels)
            {
                newBtn = new ThingWithLabel()
                {
                    IsCheckable = false,
                    IsSelectable = true,
                    Tag = c.data,
                    ThingBase = c.data,
                    TxLabel1 = "",
                    TxLabel2 = "",
                };
                newBtn.TxLabel3 = ThingWithLabel.GetCommonSpeed(c.data?.speed);
                newBtn.PreviewMouseLeftButtonDown += ChannelBtn_PreviewMouseLeftButtonDown;

                sp_graphToolbar.Children.Add(newBtn);
            }
        }
        private void ReSetChannelButtonsWidth()
        {
            ThingWithLabel btn;
            Channels.Channel channel;
            Guid foundId;
            for (int i = 2, iv = sp_graphToolbar.Children.Count; i < iv; ++i)
            {
                if (sp_graphToolbar.Children[i] is not ThingWithLabel)
                {
                    continue;
                }
                btn = (ThingWithLabel)sp_graphToolbar.Children[i];
                if (btn.Tag is not Channels.Channel)
                {
                    continue;
                }
                channel = (Channels.Channel)btn.Tag;
                foundId = checkedChannelIdList.Find(a => a == channel.id);
                if (foundId != Guid.Empty)
                {
                    btn.MaxWidth = checkedChannelBtnWidth;
                }
                else
                {
                    btn.MaxWidth = double.MaxValue;
                }
            }
        }
        private void ChannelButtonsUnselecteAll()
        {
            ThingWithLabel btn;
            for (int i = 2, iv = sp_graphToolbar.Children.Count; i < iv; ++i)
            {
                if (sp_graphToolbar.Children[i] is not ThingWithLabel)
                {
                    continue;
                }
                btn = (ThingWithLabel)sp_graphToolbar.Children[i];
                btn.IsSelected = false;
            }
        }

        private void ChannelBtn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ThingWithLabel)
            {
                return;
            }
            // visual
            btn_arrow.IsChecked = false;
            GraphDeselectAll();
            ChannelButtonsUnselecteAll();
            ThingWithLabel btn = ((ThingWithLabel)sender);
            btn.IsSelected = true;

            // chose to link mode
            if (btn.Tag is not Channels.Channel)
            {
                return;
            }
            GraphWorkMode = GraphWorkModes.Link;
            selectedLinkChannel = (Channels.Channel)btn.Tag;
        }


        private void btn_arrow_Click(object sender, RoutedEventArgs e)
        {
            btn_arrow.IsChecked = true;
            CanvasMouseDown_clearFlags();
            ChannelButtonsUnselecteAll();

            // normal mode, move, zoom, resize;
            GraphWorkMode = GraphWorkModes.Normal;
        }

        private void btn_settingChannels_Click(object sender, RoutedEventArgs e)
        {
            WindowSelectChannels channelsWin = new WindowSelectChannels()
            {
                Owner = this,
                IsMultiSelect = true,
                Channels = core.channelsFinal?.list,
            };
            channelsWin.SetSelection(checkedChannelIdList);

            if (channelsWin.ShowDialog() == true)
            {
                checkedChannelIdList = channelsWin.CheckedChannelIdList;

                // set buttons width
                ReSetChannelButtonsWidth();
                SaveGraphToolBarSelection();
            }
        }

        #endregion


        #region link nodes, delete link, delete node, set multiple, 

        public enum GraphWorkModes
        { Normal, Link, }
        private GraphWorkModes GraphWorkMode = GraphWorkModes.Normal;

        private void CanvasMouseLMBDown_NormalMode()
        {
            if (GraphWorkMode != GraphWorkModes.Normal)
            {
                return;
            }

            graphFGAM.GetUIInfo(ref linkMode_cursorUIinfo, canvas_graph, Mouse.GetPosition(canvas_graph));

            // for sim speed
            TryGetSimSpeedRef();

            bool isShiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool isAltDown = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);

            if (linkMode_cursorUIinfo.HasControls == false)
            {
                if (!isShiftDown && !isAltDown)
                {
                    GraphDeselectAll();
                }
                return;
            }

            // 2024 1111
            // 如果鼠标在节点的标题上，则不进行选择（进行移动）
            if (linkMode_cursorUIinfo.ioPanel is not null)
            {
                if (isShiftDown)
                {
                    GraphSelectIOPanel(linkMode_cursorUIinfo.ioPanel, true);
                }
                else if (isAltDown)
                {
                    GraphSelectIOPanel(linkMode_cursorUIinfo.ioPanel, false);
                }
                else
                {
                    // 2024 1111
                    // 检查是否为移动节点操作
                    if (linkMode_cursorUIinfo.ioPanel.CheckMouseOnTitle() == false)
                    {
                        GraphDeselectAll();
                        GraphSelectIOPanel(linkMode_cursorUIinfo.ioPanel);
                    }
                }
            }
            else if (linkMode_cursorUIinfo.processPanel is not null)
            {
                if (isShiftDown)
                {
                    GraphSelectProcessPanel(linkMode_cursorUIinfo.processPanel, true);
                }
                else if (isAltDown)
                {
                    GraphSelectProcessPanel(linkMode_cursorUIinfo.processPanel, false);
                }
                else
                {
                    // 2024 1111
                    // 检查是否为移动节点操作
                    if (linkMode_cursorUIinfo.processPanel.CheckMouseOnTitle() == false)
                    {
                        GraphDeselectAll();
                        GraphSelectProcessPanel(linkMode_cursorUIinfo.processPanel);
                    }
                }
            }
            else if (linkMode_cursorUIinfo.linkLine is not null)
            {
                if (isShiftDown)
                {
                    GraphSelectLinkLine(linkMode_cursorUIinfo.linkLine, true);
                }
                else if (isAltDown)
                {
                    GraphSelectLinkLine(linkMode_cursorUIinfo.linkLine, false);
                }
                else
                {
                    GraphDeselectAll();
                    GraphSelectLinkLine(linkMode_cursorUIinfo.linkLine);
                }
            }
            else if (linkMode_cursorUIinfo.notePanel is not null)
            {
                if (isShiftDown)
                {
                    GraphSelectNotePanel(linkMode_cursorUIinfo.notePanel, true);
                }
                else if (isAltDown)
                {
                    GraphSelectNotePanel(linkMode_cursorUIinfo.notePanel, false);
                }
                else
                {
                    if (linkMode_cursorUIinfo.notePanel.CheckMouseOnTitle() == false)
                    {
                        GraphDeselectAll();
                        GraphSelectNotePanel(linkMode_cursorUIinfo.notePanel);
                    }
                }
            }
        }

        private Channels.Channel selectedLinkChannel;

        private bool linkMode_firstPointChecked = false;

        private FlowGraphAlpha_Manu.SelectedUIInfo linkMode_cursorUIinfo = new FlowGraphAlpha_Manu.SelectedUIInfo();
        private FlowGraphAlpha_Manu.SelectedUIInfo linkMode_cursorUIinfo_last = new FlowGraphAlpha_Manu.SelectedUIInfo();
        private FlowGraphAlpha_Manu.SelectedUIInfo linkMode_firstUIinfo = new FlowGraphAlpha_Manu.SelectedUIInfo();
        private FlowGraphAlpha_Manu.SelectedUIInfo linkMode_secondUIinfo = new FlowGraphAlpha_Manu.SelectedUIInfo();

        private bool linkMode_firstNSecond_canLink = false;
        private Things.Thing? linkMode_linkOnThing = null;
        private decimal linkMode_linkSpeed = 0;

        private void CanvasMouseDown_clearFlags()
        {
            linkMode_firstPointChecked = false;
            linkMode_firstUIinfo.Clear();
            linkMode_secondUIinfo.Clear();
            linkMode_firstNSecond_canLink = false;
        }

        private bool CanvasMouseMove_LinkMode_isBusy = false;
        private void CanvasMouseMove_LinkMode()
        {
            if (CanvasMouseMove_LinkMode_isBusy)
            {
                return;
            }
            if (GraphWorkMode != GraphWorkModes.Link)
            {
                return;
            }
            CanvasMouseMove_LinkMode_isBusy = true;
            graphFGAM.GetUIInfo(ref linkMode_cursorUIinfo, canvas_graph, Mouse.GetPosition(canvas_graph));
            if ((linkMode_cursorUIinfo.ioPanel is not null && linkMode_cursorUIinfo.ioPanel == linkMode_firstUIinfo.ioPanel)
                || (linkMode_cursorUIinfo.processPanel is not null && linkMode_cursorUIinfo.processPanel == linkMode_firstUIinfo.processPanel))
            {
            }
            else if (!linkMode_firstPointChecked)
            {
                CanvasMouseMove_LinkMode_RestoreSelectVisuals(true);
                linkMode_firstUIinfo.SetInfoFrom(linkMode_cursorUIinfo);
                if (linkMode_firstUIinfo.processPanel is not null) // linkMode_firstIOPanel != null | || linkMode_firstLinkLine != null)
                {
                    // set visual selected
                    if (linkMode_firstUIinfo.subPanel is not null
                        && linkMode_firstUIinfo.subPanel_isInOrOut == false
                        && linkMode_firstUIinfo.subPanel.ThingBase is not null)
                    {
                        linkMode_firstUIinfo.subPanel.IsSelected = true;
                    }
                    else
                    {
                        linkMode_firstUIinfo.processPanel.IsSelected = true;
                    }
                }
            }
            else
            {
                // for the second item
                CanvasMouseLMBDown_LinkMode_VisualLineUpdate();
                linkMode_firstNSecond_canLink = false;
                CanvasMouseMove_LinkMode_RestoreSelectVisuals(false);
                linkMode_secondUIinfo.SetInfoFrom(linkMode_cursorUIinfo);
                if (linkMode_secondUIinfo.processPanel != null) // linkMode_secondIOPanel != null ||  || linkMode_secondLinkLine != null)
                {
                    // set visual selected
                    if (graphFGAM.CheckCanLink(
                        linkMode_firstUIinfo, linkMode_secondUIinfo,
                        out linkMode_linkOnThing, out linkMode_linkSpeed))
                    {
                        linkMode_firstNSecond_canLink = true;
                    }
                    if (linkMode_secondUIinfo.subPanel is not null && linkMode_secondUIinfo.subPanel.ThingBase is not null)
                    {
                        if (linkMode_firstNSecond_canLink == false)
                        {
                            CanvasMouseMove_LinkMode_SetSelectedItemVisual(linkMode_secondUIinfo.subPanel, true);
                        }
                        linkMode_secondUIinfo.subPanel.IsSelected = true;
                    }
                    else
                    {
                        if (linkMode_firstNSecond_canLink == false)
                        {
                            CanvasMouseMove_LinkMode_SetSelectedItemVisual(linkMode_secondUIinfo.processPanel, true);
                        }
                        linkMode_secondUIinfo.processPanel.IsSelected = true;
                    }
                }
            }

            CanvasMouseMove_LinkMode_isBusy = false;

        }
        private void CanvasMouseLMBDown_LinkMode()
        {
            if (GraphWorkMode != GraphWorkModes.Link)
            {
                return;
            }

            if (!linkMode_firstPointChecked)
            {
                // first item, set visual line
                if (linkMode_firstUIinfo.ioPanel is not null)
                {
                }
                else if ((linkMode_firstUIinfo.subPanel != null && linkMode_firstUIinfo.subPanel.IsSelected)
                    || (linkMode_firstUIinfo.processPanel != null && linkMode_firstUIinfo.processPanel.IsSelected)
                    )
                {
                    if (linkMode_firstUIinfo.subPanel is not null
                        && linkMode_firstUIinfo.subPanel.ThingBase is not null
                        && linkMode_firstUIinfo.subPanel_isInOrOut)
                    {
                    }
                    else
                    {
                        // 检查连接的端口物品是否受到当前通道支持；
                        if (linkMode_firstUIinfo.subPanel is not null)
                        {
                            if (linkMode_firstUIinfo.subPanel.ThingBase is not null
                                && TryFindCurChannelContent(linkMode_firstUIinfo.subPanel.ThingBase.id) is not null)
                            {
                                linkMode_firstPointChecked = true;
                                CanvasMouseLMBDown_LinkMode_visualLine_startPoint = Mouse.GetPosition(grid_graph);
                                CanvasMouseLMBDown_LinkMode_VisualLineStart();
                            }
                        }
                        else if (linkMode_firstUIinfo.processPanel is not null)
                        {
                            foreach (Recipes.Recipe.PIOItem pio in linkMode_firstUIinfo.processPanel.processNode.recipe.outputs)
                            {
                                if (pio.thing is not null
                                    && TryFindCurChannelContent(pio.thing.id) is not null)
                                {
                                    linkMode_firstPointChecked = true;
                                    CanvasMouseLMBDown_LinkMode_visualLine_startPoint = Mouse.GetPosition(grid_graph);
                                    CanvasMouseLMBDown_LinkMode_VisualLineStart();
                                    break;
                                }
                            }
                        }
                        Channels.Channel.ContentItem? TryFindCurChannelContent(Guid thingId)
                        {
                            Channels.Channel.ContentItem transItem
                                = selectedLinkChannel.contentList.Find(a => a.contentId == thingId);
                            if (transItem.contentId == Guid.Empty)
                            {
                                return null;
                            }
                            return transItem;
                        }
                    }
                }
            }
            else
            {
                // second item, remove visual
                CanvasMouseLMBDown_LinkMode_VisualLineEnd();
                CanvasMouseMove_LinkMode_RestoreSelectVisuals(true);
                CanvasMouseMove_LinkMode_RestoreSelectVisuals(false);

                // add link
                if (linkMode_firstNSecond_canLink
                    && linkMode_firstUIinfo.processPanel is not null
                    && linkMode_secondUIinfo.processPanel is not null
                    && linkMode_linkOnThing is not null)
                {
                    graphFGAM.ManuAddLinkLine(
                        linkMode_firstUIinfo.processPanel.processNode,
                        linkMode_secondUIinfo.processPanel.processNode,
                        selectedLinkChannel,
                        linkMode_linkOnThing,
                        linkMode_linkSpeed);
                    GraphNeedSave = true;
                }

                CanvasMouseDown_clearFlags();
            }
        }

        private void CanvasMouseMove_LinkMode_SetSelectedItemVisual(UIElement ui, bool useDisabledColor)
        {
            if (ui is ThingWithLabel)
            {
                ((ThingWithLabel)ui).selectedBdrColor = useDisabledColor ? Colors.DimGray : Colors.Orange;

            }
            else if (ui is FlowGraphAlpha.LinkLine)
            {
                ((FlowGraphAlpha.LinkLine)ui).selectedShadowEffect
                    = useDisabledColor ? ShadowEffect_DimGray : ShadowEffect_DarkOrange;
            }
            else // MovablePanel
            {
                ((MovablePanel)ui).selectedShadowEffect
                    = useDisabledColor ? ShadowEffect_DimGray : ShadowEffect_DarkOrange;
            }
        }
        private void CanvasMouseMove_LinkMode_RestoreSelectVisuals(bool firstOrSecond)
        {
            if (firstOrSecond)
            {
                if (linkMode_firstUIinfo.subPanel != null) linkMode_firstUIinfo.subPanel.IsSelected = false;
                if (linkMode_firstUIinfo.ioPanel != null) linkMode_firstUIinfo.ioPanel.IsSelected = false;
                if (linkMode_firstUIinfo.processPanel != null) linkMode_firstUIinfo.processPanel.IsSelected = false;
                if (linkMode_firstUIinfo.linkLine != null) linkMode_firstUIinfo.linkLine.IsSelected = false;
            }
            else
            {
                if (linkMode_secondUIinfo.subPanel != null)
                {
                    CanvasMouseMove_LinkMode_SetSelectedItemVisual(linkMode_secondUIinfo.subPanel, false);
                    linkMode_secondUIinfo.subPanel.IsSelected = false;
                }
                if (linkMode_secondUIinfo.ioPanel != null)
                {
                    CanvasMouseMove_LinkMode_SetSelectedItemVisual(linkMode_secondUIinfo.ioPanel, false);
                    linkMode_secondUIinfo.ioPanel.IsSelected = false;
                }
                if (linkMode_secondUIinfo.processPanel != null)
                {
                    CanvasMouseMove_LinkMode_SetSelectedItemVisual(linkMode_secondUIinfo.processPanel, false);
                    linkMode_secondUIinfo.processPanel.IsSelected = false;
                }
                if (linkMode_secondUIinfo.linkLine != null)
                {
                    CanvasMouseMove_LinkMode_SetSelectedItemVisual(linkMode_secondUIinfo.linkLine, false);
                    linkMode_secondUIinfo.linkLine.IsSelected = false;
                }
            }
        }
        private Point CanvasMouseLMBDown_LinkMode_visualLine_startPoint;
        FlowGraphAlpha.LinkLine CanvasMouseLMBDown_LinkMode_visualLine;


        private void CanvasMouseLMBDown_LinkMode_VisualLineStart()
        {
            movingOrResizing = false;
            movingOrResizingPanel = null;
            graph_moving = false;
            graph_selecting = false;

            if (CanvasMouseLMBDown_LinkMode_visualLine is null)
            {
                CanvasMouseLMBDown_LinkMode_visualLine = new LinkLine(graphFGAM)
                { LineColorBrush = Brushes.Orange, };
            }
            //switch (colorState)
            //{
            //    case null:
            //    case 0:
            //        CanvasMouseLMBDown_LinkMode_visualLine.LineColorBrush = Brushes.Orange;
            //        break;
            //    case 1:
            //        CanvasMouseLMBDown_LinkMode_visualLine.LineColorBrush = Brushes.DarkRed;
            //        break;
            //}
            CanvasMouseLMBDown_LinkMode_VisualLineUpdate();
            grid_graph.Children.Add(CanvasMouseLMBDown_LinkMode_visualLine);
        }
        private void CanvasMouseLMBDown_LinkMode_VisualLineUpdate()
        {
            if (CanvasMouseLMBDown_LinkMode_visualLine is not null)
            {
                Point curMP = Mouse.GetPosition(grid_graph);
                curMP.X -= 2;
                CanvasMouseLMBDown_LinkMode_visualLine.RedrawLine(
                    CanvasMouseLMBDown_LinkMode_visualLine_startPoint,
                    curMP,
                    8);
            }
        }
        private void CanvasMouseLMBDown_LinkMode_VisualLineEnd()
        {
            if (CanvasMouseLMBDown_LinkMode_visualLine is not null)
            {
                grid_graph.Children.Remove(CanvasMouseLMBDown_LinkMode_visualLine);
            }
        }

        private FlowGraphAlpha_Manu.SelectedUIInfo normalMode_cursorUIinfo = new FlowGraphAlpha_Manu.SelectedUIInfo();
        private IOPanel? notePanel_lineToIOPanel = null;
        private ProcessPanel? notePanel_lineToPPanel = null;
        private LinkLineV2? notePanel_lineToLine = null;
        private NotePanel? notePanel_lineToNPanel = null;
        private bool CanvasMouseMove_NormalMode_isBusy = false;
        private void CanvasMouseMove_NormalMode()
        {
            if (CanvasMouseMove_NormalMode_isBusy)
            {
                return;
            }
            if (GraphWorkMode != GraphWorkModes.Normal
                || graphFGAM is null)
            {
                return;
            }
            CanvasMouseMove_NormalMode_isBusy = true;
            if (graphFGAM.notePanel_withLMD is not null)
            {
                graphFGAM.GetUIInfo(ref normalMode_cursorUIinfo, canvas_graph, Mouse.GetPosition(canvas_graph));
                NotePanel np = graphFGAM.notePanel_withLMD;
                if (np.lineHandle is not null
                    && np.lineHandle.Visibility == Visibility.Visible)
                {
                    this.Cursor = np.lineHandle.Cursor;
                    np.lineHandle.Visibility = Visibility.Collapsed;
                }
                Point refPt = new Point(
                    normalMode_cursorUIinfo.mousePoint.X - np.CenterX,
                    normalMode_cursorUIinfo.mousePoint.Y - np.CenterY);
                graphFGAM.notePanel_withLMD.SetLineEnd(null, refPt);


                #region select visual
                if (normalMode_cursorUIinfo.ioPanel is not null)
                {
                    if (notePanel_lineToIOPanel is not null)
                    {
                        if (notePanel_lineToIOPanel == normalMode_cursorUIinfo.ioPanel)
                        {
                            // do nothing
                        }
                        else
                        {
                            notePanel_lineToIOPanel.IsSelected = false;
                        }
                    }
                    notePanel_lineToIOPanel = normalMode_cursorUIinfo.ioPanel;
                    notePanel_lineToIOPanel.IsSelected = true;
                    if (notePanel_lineToPPanel is not null)
                    {
                        notePanel_lineToPPanel.IsSelected = false;
                        notePanel_lineToPPanel = null;
                    }
                    if (notePanel_lineToLine is not null)
                    {
                        notePanel_lineToLine.IsSelected = false;
                        notePanel_lineToLine = null;
                    }
                    if (notePanel_lineToNPanel is not null
                        && notePanel_lineToNPanel != np)
                    {
                        notePanel_lineToNPanel.IsSelected = false;
                        notePanel_lineToNPanel = null;
                    }
                }
                else if (normalMode_cursorUIinfo.processPanel is not null)
                {
                    if (notePanel_lineToIOPanel is not null)
                    {
                        notePanel_lineToIOPanel.IsSelected = false;
                        notePanel_lineToIOPanel = null;
                    }
                    if (notePanel_lineToPPanel is not null)
                    {
                        if (notePanel_lineToPPanel == normalMode_cursorUIinfo.processPanel)
                        {
                            // do nothing
                        }
                        else
                        {
                            notePanel_lineToPPanel.IsSelected = false;
                        }
                    }
                    notePanel_lineToPPanel = normalMode_cursorUIinfo.processPanel;
                    notePanel_lineToPPanel.IsSelected = true;
                    if (notePanel_lineToLine is not null)
                    {
                        notePanel_lineToLine.IsSelected = false;
                        notePanel_lineToLine = null;
                    }
                    if (notePanel_lineToNPanel is not null
                        && notePanel_lineToNPanel != np)
                    {
                        notePanel_lineToNPanel.IsSelected = false;
                        notePanel_lineToNPanel = null;
                    }
                }
                else if (normalMode_cursorUIinfo.linkLine is not null
                    && normalMode_cursorUIinfo.linkLine_OnPanel)
                {
                    if (notePanel_lineToIOPanel is not null)
                    {
                        notePanel_lineToIOPanel.IsSelected = false;
                        notePanel_lineToIOPanel = null;
                    }
                    if (notePanel_lineToPPanel is not null)
                    {
                        notePanel_lineToPPanel.IsSelected = false;
                        notePanel_lineToPPanel = null;
                    }
                    if (notePanel_lineToLine is not null)
                    {
                        if (notePanel_lineToLine == normalMode_cursorUIinfo.linkLine)
                        {
                            // do nothing
                        }
                        else
                        {
                            notePanel_lineToLine.IsSelected = false;
                        }
                    }
                    notePanel_lineToLine = normalMode_cursorUIinfo.linkLine;
                    notePanel_lineToLine.IsSelected = true;
                    if (notePanel_lineToNPanel is not null
                        && notePanel_lineToNPanel != np)
                    {
                        notePanel_lineToNPanel.IsSelected = false;
                        notePanel_lineToNPanel = null;
                    }
                }
                else if (normalMode_cursorUIinfo.notePanel_onHandle == false
                    && normalMode_cursorUIinfo.notePanel is not null)
                {

                    if (notePanel_lineToIOPanel is not null)
                    {
                        notePanel_lineToIOPanel.IsSelected = false;
                        notePanel_lineToIOPanel = null;
                    }
                    if (notePanel_lineToPPanel is not null)
                    {
                        notePanel_lineToPPanel.IsSelected = false;
                        notePanel_lineToPPanel = null;
                    }
                    if (notePanel_lineToLine is not null)
                    {
                        notePanel_lineToLine.IsSelected = false;
                        notePanel_lineToLine = null;
                    }
                    if (notePanel_lineToNPanel is not null)
                    {
                        if (notePanel_lineToNPanel == normalMode_cursorUIinfo.notePanel)
                        {
                            // do nothing
                        }
                        else if (notePanel_lineToNPanel != np)
                        {
                            notePanel_lineToNPanel.IsSelected = false;
                        }
                    }
                    notePanel_lineToNPanel = normalMode_cursorUIinfo.notePanel;
                    notePanel_lineToNPanel.IsSelected = true;
                }
                else
                {
                    if (notePanel_lineToIOPanel is not null)
                    {
                        notePanel_lineToIOPanel.IsSelected = false;
                        notePanel_lineToIOPanel = null;
                    }
                    if (notePanel_lineToPPanel is not null)
                    {
                        notePanel_lineToPPanel.IsSelected = false;
                        notePanel_lineToPPanel = null;
                    }
                    if (notePanel_lineToLine is not null)
                    {
                        notePanel_lineToLine.IsSelected = false;
                        notePanel_lineToLine = null;
                    }
                    if (notePanel_lineToNPanel is not null
                        && notePanel_lineToNPanel != np)
                    {
                        notePanel_lineToNPanel.IsSelected = false;
                        notePanel_lineToNPanel = null;
                    }
                }
                #endregion
            }
            CanvasMouseMove_NormalMode_isBusy = false;
        }




        #endregion


        #region note, line to



        #endregion



        #endregion



        #region context menu



        #region select, multi select

        private List<FlowGraphAlpha.IOPanel> selectedIOPanels = new List<FlowGraphAlpha.IOPanel>();
        private List<FlowGraphAlpha_Manu.LinkLineV2> selectedLinkLines = new List<FlowGraphAlpha_Manu.LinkLineV2>();
        private List<FlowGraphAlpha.ProcessPanel> selectedProcessPanels = new List<FlowGraphAlpha.ProcessPanel>();
        private List<FlowGraphAlpha_Manu.NotePanel> selectedNotePanels = new List<FlowGraphAlpha_Manu.NotePanel>();

        private void GraphSelectIOPanel(FlowGraphAlpha.IOPanel IOPanel, bool addOrRemove = true)
        {
            if (addOrRemove)
            {
                if (selectedIOPanels.Contains(IOPanel) == false)
                {
                    selectedIOPanels.Add(IOPanel);
                }
            }
            else
            {
                if (selectedIOPanels.Contains(IOPanel))
                {
                    selectedIOPanels.Remove(IOPanel);
                }
            }
            IOPanel.IsSelected = addOrRemove;
        }
        private void GraphSelectProcessPanel(FlowGraphAlpha.ProcessPanel pPanel, bool addOrRemove = true)
        {
            if (addOrRemove)
            {
                if (selectedProcessPanels.Contains(pPanel) == false)
                {
                    selectedProcessPanels.Add(pPanel);
                }
            }
            else
            {
                if (selectedProcessPanels.Contains(pPanel))
                {
                    selectedProcessPanels.Remove(pPanel);
                }
            }
            pPanel.IsSelected = addOrRemove;
        }
        private void GraphSelectLinkLine(FlowGraphAlpha_Manu.LinkLineV2 line, bool addOrRemove = true)
        {
            if (addOrRemove)
            {
                if (selectedLinkLines.Contains(line) == false)
                {
                    selectedLinkLines.Add(line);
                }
            }
            else
            {
                if (selectedLinkLines.Contains(line))
                {
                    selectedLinkLines.Remove(line);
                }
            }
            line.IsSelected = addOrRemove;
        }
        private void GraphSelectNotePanel(FlowGraphAlpha_Manu.NotePanel nPanel, bool addOrRemove = true)
        {
            if (addOrRemove)
            {
                if (selectedNotePanels.Contains(nPanel) == false)
                {
                    selectedNotePanels.Add(nPanel);
                }
            }
            else
            {
                if (selectedNotePanels.Contains(nPanel))
                {
                    selectedNotePanels.Remove(nPanel);
                }
            }
            nPanel.IsSelected = addOrRemove;
        }

        #endregion

        #region de-select
        private void GraphDeselectAll()
        {
            GraphDeselectIOPanels();
            GraphDeselectProcessPanels();
            GraphDeselectLinkLines();
            GraphDeselectNotePanels();
        }
        private void GraphDeselectIOPanels()
        {
            // set visual
            foreach (IOPanel p in selectedIOPanels)
            {
                p.IsSelected = false;
            }
            selectedIOPanels.Clear();
        }
        private void GraphDeselectProcessPanels()
        {
            foreach (ProcessPanel p in selectedProcessPanels)
            {
                p.IsSelected = false;
            }
            selectedProcessPanels.Clear();
        }
        private void GraphDeselectLinkLines()
        {
            foreach (LinkLine l in selectedLinkLines)
            {
                l.IsSelected = false;
            }
            selectedLinkLines.Clear();
        }
        private void GraphDeselectNotePanels()
        {
            foreach (FlowGraphAlpha_Manu.NotePanel n in selectedNotePanels)
            {
                n.IsSelected = false;
            }
            selectedNotePanels.Clear();
        }
        #endregion


        private void canvas_graph_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (GraphWorkMode != GraphWorkModes.Normal)
            {
                e.Handled = true;
                return;
            }
            bool canAddNote = false;
            bool canSetNote = false;

            bool canSetmultiple = false;
            bool canDelNodes = false;
            bool canDelLines = false;
            bool onlyOneLine = false;
            bool canAutoFinish = false;

            graphFGAM.GetUIInfo(ref linkMode_cursorUIinfo, canvas_graph, Mouse.GetPosition(canvas_graph));
            if (linkMode_cursorUIinfo.ioPanel == null
                && linkMode_cursorUIinfo.processPanel == null
                && linkMode_cursorUIinfo.linkLine == null
                && linkMode_cursorUIinfo.notePanel == null)
            {
                GraphDeselectAll();

                canAddNote = true;
                canAutoFinish = true;
            }
            else if (linkMode_cursorUIinfo.ioPanel != null)
            {
                if (selectedIOPanels.Contains(linkMode_cursorUIinfo.ioPanel) == false)
                {
                    GraphDeselectAll();
                }
                if (!linkMode_cursorUIinfo.ioPanel.IsSelected)
                {
                    GraphSelectIOPanel(linkMode_cursorUIinfo.ioPanel);
                }

                canDelLines = true;
            }
            else if (linkMode_cursorUIinfo.processPanel != null)
            {
                if (selectedProcessPanels.Contains(linkMode_cursorUIinfo.processPanel) == false)
                {
                    GraphDeselectAll();
                }
                if (!linkMode_cursorUIinfo.processPanel.IsSelected)
                {
                    GraphSelectProcessPanel(linkMode_cursorUIinfo.processPanel);
                }
                canSetmultiple = selectedProcessPanels.Count == 1;
                canDelNodes = true;
                canDelLines = true;
                canAutoFinish = true;

                linkMode_cursorUIinfo_last = linkMode_cursorUIinfo;
            }
            else if (linkMode_cursorUIinfo.linkLine != null)
            {
                if (selectedLinkLines.Contains(linkMode_cursorUIinfo.linkLine) == false)
                {
                    GraphDeselectAll();
                }
                if (!linkMode_cursorUIinfo.linkLine.IsSelected)
                {
                    GraphSelectLinkLine(linkMode_cursorUIinfo.linkLine);
                }

                canDelLines = true;
                onlyOneLine = selectedLinkLines.Count == 1;
                canAutoFinish = true;
            }
            else if (linkMode_cursorUIinfo.notePanel is not null)
            {
                if (selectedNotePanels.Contains(linkMode_cursorUIinfo.notePanel) == false)
                {
                    GraphDeselectAll();
                }
                if (!linkMode_cursorUIinfo.notePanel.IsSelected)
                {
                    GraphSelectNotePanel(linkMode_cursorUIinfo.notePanel);
                }
                canDelNodes = true;
            }
            else
            {
                e.Handled = true;
            }

            if (selectedIOPanels.Count == 0
                && selectedProcessPanels.Count == 0
                && selectedLinkLines.Count == 0
                && selectedNotePanels.Count > 0)
            {
                canSetNote = true;
            }

            btn_addNote.Visibility = canAddNote ? Visibility.Visible : Visibility.Collapsed;
            btn_setNoteColor.Visibility = canSetNote ? Visibility.Visible : Visibility.Collapsed;
            btn_setNoteFont.Visibility = canSetNote ? Visibility.Visible : Visibility.Collapsed;

            btn_setmultiple.IsEnabled = canSetmultiple;
            btn_deleteNode.IsEnabled = canDelNodes;
            btn_deleteLink.IsEnabled = canDelLines;
            if (onlyOneLine && linkMode_cursorUIinfo.linkLine is not null)
            {
                btn_setmultiple.IsEnabled = true;

                Guid tId = linkMode_cursorUIinfo.linkLine.link.thing.id;
                btn_linkMatchSource.IsEnabled
                    = linkMode_cursorUIinfo.linkLine.link.nodePrev.FindNextLinks(tId).Count == 1;
                btn_linkMatchTarget.IsEnabled
                    = linkMode_cursorUIinfo.linkLine.link.nodeNext.FindPrevLinks(tId).Count == 1;
            }
            else
            {
                btn_linkMatchSource.IsEnabled = false;
                btn_linkMatchTarget.IsEnabled = false;
            }
            btn_autoFinish.IsEnabled = canAutoFinish;
        }

        private void btn_addNote_Click(object sender, RoutedEventArgs e)
        {
            graphFGAM.ManuAddNote(
                linkMode_cursorUIinfo.mousePoint.X,
                linkMode_cursorUIinfo.mousePoint.Y);
            GraphNeedSave = true;
        }

        private ColorExpertCore? colorCore1 = null;
        private ColorExpertCore? colorCore2 = null;
        private void btn_setNoteColor_Click(object sender, RoutedEventArgs e)
        {
            if (selectedNotePanels.Count == 0)
            {
                return;
            }
            NotePanel nPanel = selectedNotePanels[0];
            if (colorCore1 is null)
            {
                colorCore1 = new ColorExpertCore();
                colorCore2 = new ColorExpertCore();
            }
            WindowNoteColor winNC = new WindowNoteColor(colorCore1, colorCore2)
            {
                Owner = this,
                Color1 = nPanel.BackgroundColor,
                Color2 = nPanel.ForegroundColor,
            };
            if (winNC.ShowDialog() == true)
            {
                foreach (NotePanel n in selectedNotePanels)
                {
                    n.BackgroundColor = winNC.Color1;
                    n.ForegroundColor = winNC.Color2;
                }
            }
        }
        private void btn_setNoteFont_Click(object sender, RoutedEventArgs e)
        {
            if (selectedNotePanels.Count == 0)
            {
                return;
            }
            NotePanel nPanel = selectedNotePanels[0];
            FontDialog fd = new FontDialog()
            {
                Owner = this,
                SettingFontFamily = nPanel.FontFamily,
                SettingFontSize = nPanel.FontSize,
                SettingFontWeight = nPanel.FontWeight,
                SettingFontStyle = nPanel.FontStyle,
                SettingTextDecoration = nPanel.TextDecoration,
            };
            if (fd.ShowDialog() == true)
            {
                foreach (NotePanel n in selectedNotePanels)
                {
                    n.FontFamily = fd.SettingFontFamily;
                    n.FontSize = fd.SettingFontSize;
                    n.FontWeight = fd.SettingFontWeight;
                    n.FontStyle = fd.SettingFontStyle;
                    n.TextDecoration = fd.SettingTextDecoration;
                }
            }
        }

        private void btn_setMultiple_Click(object sender, RoutedEventArgs e)
        {
            if (linkMode_cursorUIinfo.processPanel is null
                && linkMode_cursorUIinfo.linkLine is null)
            {
                throw new Exception("Can only set process-panel or Linkline.");
            }

            if (linkMode_cursorUIinfo.processPanel is not null)
            {
                WindowNodeSpeed winNS = new WindowNodeSpeed(graphFGAM, linkMode_cursorUIinfo.processPanel)
                { Owner = this, };
                if (winNS.ShowDialog() == true)
                {
                    GraphNeedSave = true;
                }
            }
            else if (linkMode_cursorUIinfo.linkLine is not null)
            {
                WindowLinkSpeed winLS = new WindowLinkSpeed(graphFGAM, linkMode_cursorUIinfo.linkLine)
                { Owner = this, };
                if (winLS.ShowDialog() == true)
                {
                    GraphNeedSave = true;
                }
            }
        }

        private void btn_deleteNode_Click(object sender, RoutedEventArgs e)
        {
            bool hadDeleted = false;
            HashSet<ProcessingChains.ProcessingNodeBase> optNodes_tmp = new HashSet<ProcessingChains.ProcessingNodeBase>();
            HashSet<ProcessingChains.ProcessingNodeBase> optNodes = new HashSet<ProcessingChains.ProcessingNodeBase>();
            foreach (ProcessPanel pPanel in selectedProcessPanels)
            {
                SetOptNodes_fromPPanel(pPanel);
                graphFGAM.ManuRemoveProcessPanel(pPanel);
                hadDeleted = true;
            }
            RemoveRemovedNodes();
            UpdateNodeLinks();
            foreach (FlowGraphAlpha_Manu.NotePanel nPanel in selectedNotePanels)
            {
                graphFGAM.ManuRemoveNote(nPanel);
                hadDeleted = true;
            }


            GraphDeselectProcessPanels();
            GraphDeselectNotePanels();
            if (hadDeleted)
            {
                GraphNeedSave = true;
            }

            void SetOptNodes_fromPPanel(ProcessPanel pPanel)
            {
                foreach (ProcessingChains.ProcessingLink l in pPanel.processNode.linksPrev)
                {
                    if (optNodes_tmp.Contains(l.nodePrev) == false)
                    {
                        optNodes_tmp.Add(l.nodePrev);
                    }
                }
                foreach (ProcessingChains.ProcessingLink l in pPanel.processNode.linksNext)
                {
                    if (optNodes_tmp.Contains(l.nodeNext) == false)
                    {
                        optNodes_tmp.Add(l.nodeNext);
                    }
                }
            }
            void RemoveRemovedNodes()
            {
                foreach (ProcessingChains.ProcessingNode pNode in graphFGAM.processPanelList.Select(a => a.processNode))
                {
                    if (optNodes_tmp.Contains(pNode))
                    {
                        optNodes.Add(pNode);
                    }
                }
                optNodes_tmp.Clear();
            }
            void UpdateNodeLinks()
            {
                foreach (ProcessingChains.ProcessingNodeBase nBase in optNodes)
                {
                    foreach (ProcessingChains.ProcessingLink l in nBase.linksPrev)
                    {
                        if (l.ui is LinkLineV2)
                        {
                            ((LinkLineV2)l.ui).Update();
                        }
                    }
                    foreach (ProcessingChains.ProcessingLink l in nBase.linksNext)
                    {
                        if (l.ui is LinkLineV2)
                        {
                            ((LinkLineV2)l.ui).Update();
                        }
                    }
                }
            }
        }

        private void btn_deleteLink_Click(object sender, RoutedEventArgs e)
        {
            bool hadDeleted = false;
            foreach (LinkLineV2 l in selectedLinkLines)
            {
                graphFGAM.ManuRemoveLinkLine(l);
                hadDeleted = true;
            }
            if (hadDeleted == false && selectedProcessPanels.Count == 1)
            {
                graphFGAM.ManuRemoveLinkLines(selectedProcessPanels[0]);
                hadDeleted = true;
            }
            if (hadDeleted == false && selectedIOPanels.Count == 1)
            {
                graphFGAM.ManuRemoveLinkLines(selectedIOPanels[0]);
                hadDeleted = true;
            }

            GraphDeselectLinkLines();
            if (hadDeleted)
            {
                GraphNeedSave = true;
            }
        }
        private void btn_linkMatchSource_Click(object sender, RoutedEventArgs e)
        {
            if (selectedIOPanels.Count > 0
                || selectedProcessPanels.Count > 0
                || selectedLinkLines.Count != 1)
            {
                return;
            }
            ProcessingChains.ProcessingLink link = selectedLinkLines[0].link;
            LinkSpeed.OptNodeInfo onInfo = new LinkSpeed.OptNodeInfo(link, true);
            decimal nodeSpeed = (onInfo.headSpeed_perSec < 0) ? onInfo.portSpeed_perSec : onInfo.headSpeed_perSec;
            if (Math.Abs(link.GetBaseSpeed() - nodeSpeed) < ProcessingChains.aboutZero)
            {
                return;
            }
            graphFGAM.SetLinkBaseSpeed(selectedLinkLines[0], nodeSpeed);
        }

        private void btn_linkMatchTarget_Click(object sender, RoutedEventArgs e)
        {
            if (selectedIOPanels.Count > 0
                || selectedProcessPanels.Count > 0
                || selectedLinkLines.Count != 1)
            {
                return;
            }
            ProcessingChains.ProcessingLink link = selectedLinkLines[0].link;
            LinkSpeed.OptNodeInfo onInfo = new LinkSpeed.OptNodeInfo(link, false);
            decimal nodeSpeed = (onInfo.headSpeed_perSec < 0) ? onInfo.portSpeed_perSec : onInfo.headSpeed_perSec;
            if (Math.Abs(link.GetBaseSpeed() - nodeSpeed) < ProcessingChains.aboutZero)
            {
                return;
            }
            graphFGAM.SetLinkBaseSpeed(selectedLinkLines[0], nodeSpeed);
        }

        private void btn_autoFinish_Click(object sender, RoutedEventArgs e)
        {
            // 2024 1021
            // 清空输入输出
            // 先关联计算数量，如果出现不能联通的多个图，则提示并中断；
            // 如果出现相同的输入和输出，则询问是否使用输出作为输入；
            // 如果完全没问题，则给出提示，描述前面加上[Complete]；

            graphFGAM.ManuRemoveLinkLines(graphFGAM.inputPanel);
            graphFGAM.ManuRemoveLinkLines(graphFGAM.outputPanel);
            graphFGAM.inputPanel.ClearHeadList();
            graphFGAM.outputPanel.ClearHeadList();

            // 2024 1023
            // 如果所有节点的连线速度都正确，则跳过速度设定步骤；
            bool needSyncSpeeds = false;
            foreach (ProcessPanel pPanel in graphFGAM.processPanelList)
            {
                if (needSyncSpeeds)
                {
                    break;
                }
                for (int i = pPanel.processNode.recipe.inputs.Count - 1; i >= 0; --i)
                {
                    if (CheckNodePortsSpeed(pPanel.processNode, true, i) == false)
                    {
                        needSyncSpeeds = true;
                        break;
                    }
                }
                if (needSyncSpeeds)
                {
                    break;
                }
                for (int i = pPanel.processNode.recipe.outputs.Count - 1; i >= 0; --i)
                {
                    if (CheckNodePortsSpeed(pPanel.processNode, false, i) == false)
                    {
                        needSyncSpeeds = true;
                        break;
                    }
                }
            }
            bool CheckNodePortsSpeed(ProcessingChains.ProcessingNode pNode, bool inOrOut, int portIndex)
            {
                List<Recipes.Recipe.PIOItem> portItemList;
                if (inOrOut)
                {
                    portItemList = pNode.recipe.inputs;
                }
                else
                {
                    portItemList = pNode.recipe.outputs;
                }
                decimal portSpeed;
                decimal? linksSpeed;
                for (int i = 0, iv = portItemList.Count; i < iv; ++i)
                {
                    portSpeed = pNode.GetPortSpeed_perSec(inOrOut, i);
                    linksSpeed = pNode.GetPortLinksSpeed_perSec(inOrOut, i);
                    if (linksSpeed is null)
                    {
                        continue;
                    }
                    if (ProcessingChains.aboutZero <= Math.Abs(portSpeed - linksSpeed.Value))
                    {
                        return false;
                    }
                }
                return true;
            }

            if (needSyncSpeeds)
            {
                ProcessPanel startPPanel;
                if (linkMode_cursorUIinfo_last is null
                    || linkMode_cursorUIinfo_last.processPanel is null)
                {
                    if (graphFGAM.processPanelList.Count > 0)
                    {
                        startPPanel = graphFGAM.processPanelList[0];
                    }
                    else
                    {
                        MessageBox.Show(this,
                            "No graph node.", "Info",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                else
                {
                    startPPanel = linkMode_cursorUIinfo_last.processPanel;
                }
                graphFGAM.SetGraphBaseSpeed(
                    startPPanel, null,
                    out List<ProcessPanel> unsetPPanels,
                    out List<LinkLineV2> unsetLinkLines);

                if (unsetPPanels.Count > 0 || unsetLinkLines.Count > 0)
                {
                    MessageBox.Show(this,
                        $"Nodes[{unsetPPanels.Count}] n' links[{unsetLinkLines.Count}] are not set.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }
            }

            ProcessingChains.ResultHelper sr = graphFGAM.searchResult;
            sr.allSources.Clear();
            sr.allFinalProducts.Clear();
            sr.allProcesses = graphFGAM.processPanelList.Select(a => a.processNode).ToList();
            sr.allLinks = graphFGAM.linkLineList.Select(a => a.link).ToList();
            ReSetChannelData();
            ProcessingChains.ResultHelper.LinkLeftAllSources(sr);
            ProcessingChains.ResultHelper.LinkRightAllProducts(sr);
            ProcessingChains.ResultHelper.MergeSameHeads(ref sr.allSources);
            ProcessingChains.ResultHelper.MergeSameHeads(ref sr.allFinalProducts);

            bool toReduceSources = false;
            foreach (ProcessingChains.ProcessingHead hOut in sr.allFinalProducts)
            {
                foreach (ProcessingChains.ProcessingHead hIn in sr.allSources)
                {
                    if (hOut.thing != hIn.thing)
                    {
                        continue;
                    }
                    if (MessageBox.Show(this,
                        $"Some products are same as source{Environment.NewLine}, do you want to use product to reduce sources?",
                        "Product as source.",
                        MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                    {
                        toReduceSources = true;
                    }
                    break;
                }
            }
            if (toReduceSources)
            {
                ProcessingChains.ResultHelper.ReduceAllInputsFromProducts(sr);
            }

            // sync ui and data
            graphFGAM.SyncIOPanels();

            string flag = (string)Application.Current.Resources["lb_winCalculatorManu_graphComplete"];
            if (tb_graphDescription.Text.StartsWith(flag) == false)
            {
                tb_graphDescription.Text = flag + " " + tb_graphDescription.Text;
            }
            GraphNeedSave = true;
        }
        private void ReSetChannelData()
        {
            HashSet<Channels.Channel> cSet = graphFGAM.searchResult.allUsedChannels;
            cSet.Clear();

            foreach (Channels.Channel c in graphFGAM.linkLineList.Select(a => a.link.channel))
            {
                cSet.Add(c);
            }
            foreach (UIElement u in sp_graphToolbar.Children)
            {
                if (u is not ThingWithLabel)
                {
                    continue;
                }
                ThingWithLabel ui = (ThingWithLabel)u;
                if (ui.Tag is not Channels.Channel)
                {
                    continue;
                }
                cSet.Add((Channels.Channel)ui.Tag);
            }
        }

        #endregion



        #region tab graphs

        private bool _GraphNeedSave = false;
        private bool GraphNeedSave
        {
            get => _GraphNeedSave;
            set
            {
                if (btn_graphSave is null
                    || _GraphNeedSave == value)
                {
                    return;
                }
                _GraphNeedSave = value;
                btn_graphSave.IsEnabled = value;
            }
        }
        private void tb_graphDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            GraphNeedSave = true;
        }

        private void btn_graphSave_Click(object sender, RoutedEventArgs e)
        {
            if (graphList_selectedItem is null)
            {
                btn_graphSaveAs_Click(sender, e);
                return;
            }
            if (GraphNeedSave == false)
            {
                return;
            }

            graphFGAM.SaveGraph(graphList_selectedItem.TxName, tb_graphDescription.Text, true);
            GraphNeedSave = false;
            graphList_selectedItem.TxDescription = tb_graphDescription.Text;
            tba_saved.Background = Brushes.Lime;
            tba_saved.SetText((string)Application.Current.Resources["lb_winCalculatorManu_graphSaved"]);
        }

        private void btn_graphSaveAs_Click(object sender, RoutedEventArgs e)
        {
            UI.InputWindow.InputOption preSet = new UI.InputWindow.InputOption()
            { title = "Graph Name", DataType = UI.InputWindow.DataTypes.String, };
            if (graphList_selectedItem is not null)
            {
                preSet.defaultValue = graphList_selectedItem.TxName;
            }
            UI.InputWindow inputWin = new UI.InputWindow()
            { Owner = this, Title = "Input a name for this Graph.", };
            inputWin.ReSetInutOptions(preSet);
            if (inputWin.ShowDialog() == true)
            {
                // try save to file
                try
                {
                    graphFGAM.SaveGraph(inputWin.UserData[0].valueString, tb_graphDescription.Text, false);
                    tba_saved.Background = Brushes.Lime;
                    tba_saved.SetText((string)Application.Current.Resources["lb_winCalculatorManu_graphSaved"]);
                    Graph_ReloadFileList();
                    ThingWithLabel curItem;
                    foreach (UIElement u in sp_graphList.Children)
                    {
                        if (u is not ThingWithLabel)
                        {
                            continue;
                        }
                        curItem = (ThingWithLabel)u;
                        if (curItem.TxName == inputWin.UserData[0].valueString)
                        {
                            curItem.IsSelected = true;
                            curItem.TxDescription = tb_graphDescription.Text;
                            graphList_selectedItem = curItem;
                        }
                        else
                        {
                            curItem.IsSelected = false;
                        }
                    }
                    tbv_graphName.Text = inputWin.UserData[0].valueString;
                }
                catch (Exception err)
                {
                    MessageBox.Show(this,
                        err.Message, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private ThingWithLabel? graphList_selectedItem = null;
        private async void btn_graphDelete_Click(object sender, RoutedEventArgs e)
        {
            if (graphList_selectedItem is null)
            {
                return;
            }
            if (MessageBox.Show(this, "Do you want to delete this graph?", "To delete",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }
            await core.SetCursorWait(this);
            try
            {
                graphFGAM.DeleteGraph(graphList_selectedItem.TxName);
                Graph_RemoveFileItem(graphList_selectedItem.TxName);
                tba_saved.Background = Brushes.OrangeRed;
                tba_saved.SetText((string)Application.Current.Resources["lb_winCalculatorManu_graphDeleted"]);
            }
            catch (Exception err)
            {
                MessageBox.Show(this,
                    err.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                core.SetCursorArrow(this, 0);
            }
        }

        private void btn_graphClear_Click(object sender, RoutedEventArgs e)
        {
            // deselect graph
            if (graphList_selectedItem is not null)
            {
                graphList_selectedItem.IsSelected = false;
                graphList_selectedItem = null;
                tbv_graphName.Text = "";
                tb_graphDescription.Text = "";
            }

            // clear nodes
            graphFGAM.ManuRemoveAllNotes();
            for (int i = graphFGAM.processPanelList.Count - 1; i >= 0; --i)
            {
                graphFGAM.ManuRemoveProcessPanel(graphFGAM.processPanelList[i]);
            }
            graphFGAM.inputPanel.ClearHeadList();
            graphFGAM.outputPanel.ClearHeadList();
        }
        private void Graph_ReloadFileList()
        {
            Graph_ClearFileItems();
            ThingWithLabel ui;
            foreach (FileInfo f in graphFGAM.GetGraphFiles())
            {
                ui = new ThingWithLabel()
                {
                    Tag = f,
                    Icon = ImageIO.Image_XmlFile,
                    TxName = f.Name.Substring(0, f.Name.Length - 4),
                    TxDescription = "",
                    IsSelectable = true,
                    IsNumberLabelsVisible = false,
                    IsCheckable = false,
                };
                if (graphList_selectedItem is not null
                    && ((FileInfo)graphList_selectedItem.Tag).Name == f.Name)
                {
                    ui.IsSelected = true;
                }
                ui.PreviewMouseDoubleClick += GraphFileItem_PreviewMouseDoubleClick;
                sp_graphList.Children.Add(ui);
            }
        }
        private void Graph_ClearFileItems()
        {
            foreach (UIElement u in sp_graphList.Children)
            {
                if (u is not ThingWithLabel)
                {
                    continue;
                }
                ((ThingWithLabel)u).PreviewMouseDoubleClick -= GraphFileItem_PreviewMouseDoubleClick;
            }
            sp_graphList.Children.Clear();
        }
        private void Graph_RemoveFileItem(string graphName)
        {
            ThingWithLabel ui;
            foreach (UIElement u in sp_graphList.Children)
            {
                if (u is not ThingWithLabel)
                {
                    continue;
                }
                ui = (ThingWithLabel)u;
                if (ui.TxName != graphName)
                {
                    continue;
                }
                ui.PreviewMouseDoubleClick -= GraphFileItem_PreviewMouseDoubleClick;
                if (graphList_selectedItem is not null
                    && ui == graphList_selectedItem)
                {
                    tbv_graphName.Text = "";
                    tb_graphDescription.Text = "";
                    graphList_selectedItem = null;
                }
                sp_graphList.Children.Remove(ui);
                break;
            }
        }

        private void GraphFileItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ThingWithLabel)
            {
                return;
            }
            if (sender == graphList_selectedItem
                && GraphNeedSave == false)
            {
                return;
            }
            if (graphList_selectedItem is not null)
            {
                graphList_selectedItem.IsSelected = false;
            }

            ThingWithLabel curItem = (ThingWithLabel)sender;
            graphFGAM.ReLoadGraph(curItem.TxName);
            tbv_graphName.Text = curItem.TxName;
            tb_graphDescription.Text = graphFGAM.ReLoadGraph_description;
            curItem.TxDescription = graphFGAM.ReLoadGraph_description;
            //curItem.IsSelected = true;
            graphList_selectedItem = curItem;
            GraphNeedSave = false;
            ThingWithLabel selectTestItem;
            foreach (UIElement u in sp_graphList.Children)
            {
                if (u is not ThingWithLabel)
                {
                    continue;
                }
                selectTestItem = (ThingWithLabel)u;
                selectTestItem.IsSelected = selectTestItem == curItem;
            }

            tba_saved.Background = Brushes.Aqua;
            tba_saved.SetText((string)Application.Current.Resources["lb_winCalculatorManu_graphLoaded"]);
        }




        #endregion


        #region tab sim speed
        // change to normal mode (not link mode) first;
        // disable all channel btns;
        // change to other tab, reset;

        #region nud speed ui
        private UI.NumericUpDown? nud_ssCurrent = null;
        private void nud_ssSec_ValueChanged(UI.NumericUpDown sender)
        {
            if (nud_ssCurrent != sender)
            {
                return;
            }
            decimal ups = sender.Value;
            nud_ssMin.Value = ups * 60;
            nud_ssHour.Value = sender.Value * 3600;
            SetSimSpeed(ups);
        }
        private void nud_ssSec_GotFocus(object sender, RoutedEventArgs e)
        {
            nud_ssCurrent = nud_ssSec;
        }
        private void nud_ssSec_LostFocus(object sender, RoutedEventArgs e)
        {
            nud_ssCurrent = null;
        }


        private void nud_ssMin_ValueChanged(UI.NumericUpDown sender)
        {
            if (nud_ssCurrent != sender)
            {
                return;
            }
            decimal ups = sender.Value / 60;
            nud_ssSec.Value = ups;
            nud_ssHour.Value = sender.Value * 60;
            SetSimSpeed(ups);
        }
        private void nud_ssMin_GotFocus(object sender, RoutedEventArgs e)
        {
            nud_ssCurrent = nud_ssMin;
        }
        private void nud_ssMin_LostFocus(object sender, RoutedEventArgs e)
        {
            nud_ssCurrent = null;
        }


        private void nud_ssHour_ValueChanged(UI.NumericUpDown sender)
        {
            if (nud_ssCurrent != sender)
            {
                return;
            }
            decimal ups = sender.Value / 3600;
            nud_ssSec.Value = ups;
            nud_ssMin.Value = sender.Value / 60;
            SetSimSpeed(ups);
        }
        private void nud_ssHour_GotFocus(object sender, RoutedEventArgs e)
        {
            nud_ssCurrent = nud_ssHour;
        }
        private void nud_ssHour_LostFocus(object sender, RoutedEventArgs e)
        {
            nud_ssCurrent = null;
        }

        #endregion
        private void SetSimSpeed(decimal calMultiple)
        {
            if (simSpeedBaseValue <= 0)
            {
                return;
            }
            decimal multiple = calMultiple / simSpeedBaseValue;
            graphFGAM.SetNewCallSpeeds(multiple);
        }

        private SelectedUIInfo simSpeedRefUIInfo;
        private decimal simSpeedBaseValue = -1;
        private void TryGetSimSpeedRef()
        {
            if (tabControl.SelectedIndex != 2)
            {
                // not in sim speed
                return;
            }
            decimal calValue = -1;
            if (linkMode_cursorUIinfo.HasControls == false)
            {
                simSpeedBaseValue = -1;
            }
            else
            {
                simSpeedRefUIInfo = new SelectedUIInfo(linkMode_cursorUIinfo);
                if (simSpeedRefUIInfo.subPanel is not null)
                {
                    // parent might be p-panel or io-panel
                    if (simSpeedRefUIInfo.ioPanel is not null
                        && simSpeedRefUIInfo.subPanel.Tag is ProcessingChains.ProcessingHead)
                    {
                        ProcessingChains.ProcessingHead head = (ProcessingChains.ProcessingHead)simSpeedRefUIInfo.subPanel.Tag;
                        simSpeedBaseValue = head.baseQuantity;
                        calValue = head.calQuantity;
                    }
                    else if (simSpeedRefUIInfo.processPanel is not null
                        && simSpeedRefUIInfo.subPanel.Tag is Recipes.Recipe.PIOItem)
                    {
                        Recipes.Recipe.PIOItem pio = (Recipes.Recipe.PIOItem)simSpeedRefUIInfo.subPanel.Tag;
                        ProcessingChains.ProcessingNode pNode = simSpeedRefUIInfo.processPanel.processNode;
                        if (pio.quantity is not null
                            && pNode.recipe.period is not null)
                        {
                            decimal v = pio.quantity.ValueCurrentInGeneral / pNode.recipe.period.Value;
                            simSpeedBaseValue = v * pNode.baseQuantity;
                            calValue = v * pNode.calQuantity;
                        }
                    }
                }
                else if (simSpeedRefUIInfo.processPanel is not null)
                {
                    ProcessingChains.ProcessingNode pNode = simSpeedRefUIInfo.processPanel.processNode;
                    simSpeedBaseValue = pNode.calQuantity;
                    calValue = pNode.calQuantity;
                }
                else if (simSpeedRefUIInfo.linkLine is not null)
                {
                    ProcessingChains.ProcessingLink link = simSpeedRefUIInfo.linkLine.link;
                    simSpeedBaseValue = link.baseQuantity;
                    calValue = link.calQuantity;
                }
            }

            bool canSimSpeed = calValue >= 0;
            nud_ssSec.IsEnabled = canSimSpeed;
            nud_ssMin.IsEnabled = canSimSpeed;
            nud_ssHour.IsEnabled = canSimSpeed;
            if (calValue < 0)
            {
                nud_ssSec.Value = 0;
                nud_ssMin.Value = 0;
                nud_ssHour.Value = 0;
            }
            else
            {
                nud_ssSec.Value = calValue;
                nud_ssMin.Value = calValue * 60;
                nud_ssHour.Value = calValue * 3600;
            }
        }

        private void btn_simSpeedSetToBase_Click(object sender, RoutedEventArgs e)
        {
            if (graphFGAM is null
                || graphFGAM.processPanelList.Count == 0)
            {
                return;
            }
            ProcessingChains.ProcessingNode pNode = graphFGAM.processPanelList[0].processNode;
            if (Math.Abs(pNode.baseQuantity - pNode.calQuantity) < ProcessingChains.aboutZero)
            {
                return;
            }
            graphFGAM.SetBaseSpeeds(pNode.calQuantity / pNode.baseQuantity);
            GraphNeedSave = true;
        }

        private void btn_simSpeedReset_Click(object sender, RoutedEventArgs e)
        {
            nud_ssCurrent = null;
            simSpeedBaseValue = -1;
            nud_ssSec.Value = 0;
            nud_ssMin.Value = 0;
            nud_ssHour.Value = 0;
            // re set init speed;
            graphFGAM?.ResetCalSpeeds();
        }



        #endregion

    }
}
