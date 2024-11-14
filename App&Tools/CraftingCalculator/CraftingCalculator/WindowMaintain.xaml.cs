using MadTomDev.App.Classes;
using MadTomDev.App.Ctrls;
using MadTomDev.App.VMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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
using static MadTomDev.App.Classes.Recipes.Recipe;
using MadTomDev.App.SubWindows;

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for WindowMaintain.xaml
    /// </summary>
    public partial class WindowMaintain : Window
    {
        public WindowMaintain()
        {
            InitializeComponent();
        }
        Core core = Core.Instance;
        SearchHelper<DataGridItemModelThing> searcherThingName;
        SearchHelper<DataGridItemModelChannel> searcherChannelName;
        SearchHelper<DataGridItemModelChannel> searcherChannelContent;
        SearchHelper<DataGridItemModelRecipe> searcherRecipeName;
        SearchHelper<DataGridItemModelRecipe> searcherRecipeIngredi;
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tba_animaInfo.SetReady();

            await core.SetCursorWait(this);

            core.ReInitMaintaince(core.sceneMgr.selectedTreeViewNode);
            dg_things.ItemsSource = core.maintainThings;
            dg_channels.ItemsSource = core.maintainChannels;
            dg_recipes.ItemsSource = core.maintainRecipes;
            tbv_loading.Visibility = Visibility.Collapsed;

            tbv_sceneChain.Text = core.SceneFullPath;

            searcherThingName = new SearchHelper<DataGridItemModelThing>(dg_things, tbv_nameThingSearchCount);
            searcherThingName.CheckItemFunc = CheckThingNameNDescription;
            searcherChannelName = new SearchHelper<DataGridItemModelChannel>(dg_channels, tbv_nameChannelSearchCount);
            searcherChannelName.CheckItemFunc = CheckChannelNameNDescription;
            searcherChannelContent = new SearchHelper<DataGridItemModelChannel>(dg_channels, tbv_contentChannelSearchCount);
            searcherChannelContent.CheckItemFunc = CheckChannelContentNDescription;
            searcherRecipeName = new SearchHelper<DataGridItemModelRecipe>(dg_recipes, tbv_nameRecipeSearchCount);
            searcherRecipeName.CheckItemFunc = CheckRecipeNameNDescription;
            searcherRecipeIngredi = new SearchHelper<DataGridItemModelRecipe>(dg_recipes, tbv_ingredRecipeSearchCount);
            searcherRecipeIngredi.CheckItemFunc = CheckRecipeIngredi;

            await core.SetCursorArrow(this);
        }

        #region window, mouse actions
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (recipeIngredient_dragingUI != null)
            {
                FrameworkElement? dragingFE = UI.VisualHelper.StackPanel.GetItemUI(
                    recipeIngredient_dragingUI_container,
                    Mouse.GetPosition(recipeIngredient_dragingUI_container));
                if (dragingFE is not Rectangle)
                {
                    return;
                }
                ThingInQuantity? ui = null;
                if (dragingFE.Parent is Grid)
                {
                    Grid testGrid = (Grid)dragingFE.Parent;
                    if (testGrid.Parent is Border)
                    {
                        Border testBdr = (Border)testGrid.Parent;
                        if (testBdr.Parent is Grid)
                        {
                            testGrid = (Grid)testBdr.Parent;
                            if (testGrid.Parent is ThingInQuantity)
                            {
                                ui = (ThingInQuantity)testGrid.Parent;
                            }
                        }
                    }
                }
                if (ui == null)
                {
                    return;
                }

                if (ui == recipeIngredient_dragingUI)
                {
                    return;
                }

                int myPosi = recipeIngredient_dragingUI_container.Children.IndexOf(recipeIngredient_dragingUI);
                int newPosi = recipeIngredient_dragingUI_container.Children.IndexOf(ui);
                recipeIngredient_dragingUI_container.Children.Remove(recipeIngredient_dragingUI);

                recipeIngredient_dragingUI_container.Children.Insert(newPosi, recipeIngredient_dragingUI);

            }
        }


        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (recipeIngredient_dragingUI_container != null)
            {
                recipeIngredient_dragingUI_container.Cursor = Cursors.Arrow;
            }
            recipeIngredient_dragingUI_container = null;
            recipeIngredient_dragingUI = null;
        }
        #endregion


        #region Animations

        private void AnimaSaved()
        {
            tba_animaInfo.Background = Brushes.LimeGreen;
            tba_animaInfo.SetText("  " + (string)Application.Current.Resources["lb_winMaintain_animaInfo_saved"] + "  ");
        }
        private void AnimaDeleted()
        {
            tba_animaInfo.Background = Brushes.OrangeRed;
            tba_animaInfo.SetText("  " + (string)Application.Current.Resources["lb_winMaintain_animaInfo_deleted"] + "  ");
        }

        #endregion


        #region tab things

        #region things, select thing, load info to UI
        private void dg_things_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_things.SelectedItem == null)
            {
                core.selectedMaintainThing = null;
            }
            else
            {
                core.selectedMaintainThing = (DataGridItemModelThing)dg_things.SelectedItem;
                dg_things.ScrollIntoView(dg_things.SelectedItem);
            }

            searcherThingName.SkipOrReset();

            tbv_nameThing.Clear();
            tbv_header_myId_thing.Text = "";
            tbv_parentIdThing.Clear();
            tbv_IdThing.Clear();


            if (core.selectedMaintainThing == null)
            {
                if (img_thing.Source != null && img_thing.Source != ImageIO.Image_Unknow)
                {
                    imgThing_newImg = true;
                }
                CreateUpdateThing_createOrUpdate = true;
            }
            else
            {
                imgThing_newImg = false;
                Things.Thing? t = core.selectedMaintainThing.data, tp = core.selectedMaintainThing.dataParent;
                if (tp != null && t != null)
                {
                    tbv_nameThing.Text = tp.name;
                    tb_nameThing.Text = core.selectedMaintainThing.Name;

                    tbv_header_myId_thing.Text = t.id.ToString();
                    tbv_IdThing.Text = t.id.ToString();
                    tbv_parentIdThing.Text = tp.id.ToString();

                    cb_isDisabledThing.IsChecked = core.selectedMaintainThing.IsExcluded;
                    tb_descriptionThing.Text = core.selectedMaintainThing.Description;
                    img_thing.Source = core.selectedMaintainThing.Image;
                    CreateUpdateThing_createOrUpdate = false;
                }
                else if (tp != null)
                {
                    tbv_nameThing.Clear();
                    tb_nameThing.Text = tp.name;

                    tbv_header_myId_thing.Text =
                        (string)Application.Current.TryFindResource("lb_winMaintain_preIdInherite")
                        + " " + tp.id.ToString();
                    tbv_parentIdThing.Text = tp.id.ToString();

                    cb_isDisabledThing.IsChecked = tp.isExcluded;
                    tb_descriptionThing.Text = tp.description;
                    img_thing.Source = core.selectedMaintainThing.Image;
                    CreateUpdateThing_createOrUpdate = true;
                }
                else // if (t != null)
                {
                    tbv_nameThing.Clear();
                    tb_nameThing.Text = t.name;

                    tbv_header_myId_thing.Text = t.id.ToString();
                    tbv_IdThing.Text = t.id.ToString();

                    cb_isDisabledThing.IsChecked = t.isExcluded;
                    tb_descriptionThing.Text = t.description;
                    img_thing.Source = core.selectedMaintainThing.Image;
                    CreateUpdateThing_createOrUpdate = false;
                }
            }
        }

        private void btn_deSelectThing_Click(object sender, RoutedEventArgs e)
        {
            dg_things.SelectedItem = null;
        }
        private void btn_clearUIThing_Click(object sender, RoutedEventArgs e)
        {
            tb_nameThing.Clear();

            //tbv_header_myId_thing.Text = "";

            cb_isDisabledThing.IsChecked = false;
            tb_descriptionThing.Clear();
            img_thing.Source = ImageIO.Image_Unknow;
        }
        #endregion


        #region thing, quick search by name
        private bool tb_nameThing_ignoreKeyUpOnce = false;
        private void tb_nameThing_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tb_nameThing_ignoreKeyUpOnce = true;
                btn_searchNameThing_Click(sender, new RoutedEventArgs());
            }
        }
        private void tb_nameThing_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (tb_nameThing_ignoreKeyUpOnce)
            {
                tb_nameThing_ignoreKeyUpOnce = false;
                return;
            }
            searcherThingName.Reset();
        }

        private async void btn_searchNameThing_Click(object sender, RoutedEventArgs e)
        {
            await core.SetCursorWait(this);
            searcherThingName.SearchOrNext(tb_nameThing.Text);
            core.SetCursorArrow(this);
        }
        private int CheckThingNameNDescription(object helper, DataGridItemModelThing item, object? missing)
        {
            SearchHelper<DataGridItemModelThing> h = (SearchHelper<DataGridItemModelThing>)helper;
            if (item.Name != null && h.Check(item.Name))
            {
                return 1;
            }
            if (item.Description != null && h.Check(item.Description))
            {
                return 2;
            }
            return 0;
        }


        public class SearchHelper<T>
        {

            private DataGrid? dataGrid = null;
            private WrapPanel? wrapPanel_channels = null;
            private Type channelUIItemType;
            private TextBlock tbvCounter;
            private Common.SimpleStringHelper.Checker_starNQues checker = new Common.SimpleStringHelper.Checker_starNQues();
            List<object> result = new List<object>();
            int curIndex = -1;
            int curState = 0;
            public SearchHelper(DataGrid dataGrid, TextBlock tbvCounter)
            {
                this.dataGrid = dataGrid;
                this.tbvCounter = tbvCounter;
            }
            public SearchHelper(WrapPanel wpChannels, Type channelUIItemType, TextBlock tbvCounter)
            {
                this.wrapPanel_channels = wpChannels;
                this.channelUIItemType = channelUIItemType;
                this.tbvCounter = tbvCounter;
            }

            public Func<object, T, object?, int> CheckItemFunc;
            public Func<object, Type, UIElement, int> CheckChannelFunc;
            public void SearchOrNext(string? searchingFor = null, bool fuzzySearch = true, object? arg1 = null)
            {
                if (curState > 0)
                {
                    // focus next
                    ++curState;
                    ++curIndex;
                    if (curIndex >= result.Count)
                    {
                        curIndex = 0;
                    }

                    if (result.Count > 0)
                    {
                        if (dataGrid != null)
                        {
                            SelectAndViewDataGridItem(result[curIndex]);
                        }
                        else if (wrapPanel_channels != null)
                        {
                            object item = result[curIndex];
                            if (item != null)
                            {
                                SelectAndViewWrapPanel_channels_Item((Interfaces.SelectableUIItem)item);
                            }
                        }
                    }
                    else
                    {
                        curIndex = -1;
                    }
                    tbvCounter.Text = $"{curIndex + 1} / {result.Count}";
                }
                else // curState <=0
                {
                    if (string.IsNullOrWhiteSpace(searchingFor))
                    {
                        return;
                    }
                    result.Clear();
                    curIndex = -1;
                    curState = 1;
                    checker.ClearPatterns();
                    if (fuzzySearch)
                    {
                        while (searchingFor.Contains("  "))
                        {
                            searchingFor = searchingFor.Replace("  ", " ");
                        }
                        checker.AddPattern($"*{searchingFor.Replace(" ", "* *")}*");
                    }
                    else
                    {
                        checker.AddPattern($"*{searchingFor}*");
                    }
                    if (CheckItemFunc != null)
                    {
                        List<object> result_tmp1 = new List<object>();
                        List<object> result_tmp2 = new List<object>();
                        if (dataGrid != null)
                        {
                            if (dataGrid.ItemsSource != null)
                            {
                                foreach (T t in dataGrid.ItemsSource)
                                {
                                    switch (CheckItemFunc(this, t, arg1))
                                    {
                                        case 1:
                                            result_tmp1.Add(t);
                                            break;
                                        case 2:
                                            result_tmp2.Add(t);
                                            break;
                                    }
                                }
                            }
                        }
                        else if (wrapPanel_channels != null)
                        {
                            foreach (UIElement item in wrapPanel_channels.Children)
                            {
                                switch (CheckChannelFunc(this, channelUIItemType, item))
                                {
                                    case 1:
                                        result_tmp1.Add(item);
                                        break;
                                    case 2:
                                        result_tmp2.Add(item);
                                        break;
                                }
                            }
                        }
                        int i, iv;
                        for (i = 0, iv = result_tmp1.Count; i < iv; ++i)
                        {
                            result.Add(result_tmp1[i]);
                        }
                        for (i = 0, iv = result_tmp2.Count; i < iv; ++i)
                        {
                            result.Add(result_tmp2[i]);
                        }
                    }
                    // focus first(next)
                    SearchOrNext();
                }
            }
            public bool Check(string sample)
            {
                return checker.Check(sample);
            }
            private void SelectAndViewDataGridItem(object item)
            {
                if (dataGrid == null)
                {
                    return;
                }
                dataGrid.SelectedItem = item;
                UI.VisualHelper.DataGrid.ScrollIntoView(dataGrid, null, item);
            }

            Interfaces.SelectableUIItem? wrapPanel_channels_preSelectedItem = null;
            private void SelectAndViewWrapPanel_channels_Item(Interfaces.SelectableUIItem item)
            {
                if (wrapPanel_channels == null)
                {
                    return;
                }
                // select item
                if (wrapPanel_channels_preSelectedItem != null)
                {
                    wrapPanel_channels_preSelectedItem.IsSelected = false;
                }
                item.IsSelected = true;
                wrapPanel_channels_preSelectedItem = item;

                // scroll into view
                UI.VisualHelper.WrapPanel.ScrollIntoView(wrapPanel_channels, (UIElement)item);
            }

            public void SkipOrReset()
            {
                --curState;
                tbvCounter.Text = "";
            }
            public void Reset()
            {
                curState = 0;
                tbvCounter.Text = "";
            }
        }

        #endregion

        #region thing, change image
        private void rect_imgThingDropZone_PreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void rect_imgThingDropZone_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private bool imgThing_newImg = false;
        private void rect_imgThingDropZone_PreviewDrop(object sender, DragEventArgs e)
        {
            BitmapSource? bs = UI.QuickGraphics.Image.FromDragDrop(e.Data);
            if (bs != null)
            {
                img_thing.Source = bs;
            }
            else
            {
                img_thing.Source = ImageIO.Image_Unknow;
            }
            imgThing_newImg = true;
        }


        private void rect_imgThingDropZone_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            rect_imgThingDropZone.Focus();
        }
        private void rect_imgThingDropZone_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                img_thing.Source = ImageIO.Image_Unknow;
                imgThing_newImg = true;
            }
            else if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control)
                && e.Key == Key.V)
            {
                BitmapSource? bs = UI.QuickGraphics.Image.FromClipboard();
                if (bs != null)
                {
                    img_thing.Source = bs;
                }
                else
                {
                    img_thing.Source = ImageIO.Image_Unknow;
                }
                imgThing_newImg = true;
            }
        }
        #endregion


        #region thing, delete, create or update
        private void btn_deleteThing_Click(object sender, RoutedEventArgs e)
        {
            if (!core.DeleteSelectedThingCheck(out string? errMsg, out int errLv))
            {
                switch (errLv)
                {
                    case 0:
                        MessageBox.Show(this, errMsg,
                            (string)Application.Current.TryFindResource("lb_winMaintain_info"),
                            MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        break;
                    case 1:
                        MessageBox.Show(this, errMsg,
                            (string)Application.Current.TryFindResource("lb_winMaintain_warning"),
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                }
                return;
            }
            if (core.CheckSelectedThingUsedByCannelOrRecipe(out Guid channelId, out Guid recipeId))
            {
                if (channelId != Guid.Empty)
                {
                    MessageBox.Show(this,
                        string.Format((string)Application.Current.TryFindResource("lb_winMaintain_msgBox_cantDelUsedByChannel_content"),
                            core.FindChannel(channelId)?.name),
                        (string)Application.Current.TryFindResource("lb_winMaintain_warning"),
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (recipeId != Guid.Empty)
                {
                    MessageBox.Show(this,
                        string.Format((string)Application.Current.TryFindResource("lb_winMaintain_msgBox_cantDelUsedByRecipe_content"),
                            core.FindRecipe(recipeId)?.name),
                        (string)Application.Current.TryFindResource("lb_winMaintain_warning"),
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                return;
            }
            if (core.selectedMaintainThing == null)
            {
                throw new Exception("(Impossible) selected maintain thing is null.");
            }
            if (MessageBox.Show(this,
                string.Format(
                    (string)Application.Current.TryFindResource("lb_winMaintain_msgBox_confirmToDeleteThing_content"),
                    core.selectedMaintainThing.Name,
                    Environment.NewLine + Environment.NewLine),
                (string)Application.Current.TryFindResource("lb_winMaintain_warning"),
                MessageBoxButton.YesNo, MessageBoxImage.Warning)
                == MessageBoxResult.Yes)
            {
                core.DeleteSelectedThing();
                AnimaDeleted();
            }
        }

        private bool _CreateUpdateThing_createOrUpdate = true;
        private bool CreateUpdateThing_createOrUpdate
        {
            get => _CreateUpdateThing_createOrUpdate;
            set
            {
                _CreateUpdateThing_createOrUpdate = value;
                if (value)
                {
                    if (core.selectedMaintainThing != null && core.selectedMaintainThing.dataParent != null)
                    {
                        btn_createUpdateThing.Content = (string)Application.Current.Resources["lb_winMaintain_inherite"];
                    }
                    else
                    {
                        btn_createUpdateThing.Content = (string)Application.Current.Resources["lb_winMaintain_create"];
                    }
                }
                else
                {
                    btn_createUpdateThing.Content = (string)Application.Current.Resources["lb_winMaintain_update"];
                }
            }
        }

        private bool btn_createUpdateThing_needNotSave = false;
        private async void btn_createUpdateThing_Click(object sender, RoutedEventArgs e)
        {
            if (btn_createUpdateThing_needNotSave)
            {
                return;
            }
            if (core.thingsCurrent == null)
            {
                throw new Exception("(Impossible) No current thing.");
            }
            if (string.IsNullOrWhiteSpace(tb_nameThing.Text))
            {
                MessageBox.Show(this, "Thing must has a name.", "Stop", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            await core.SetCursorWait(this);
            Things.Thing newThing = new Things.Thing(core.thingsCurrent)
            {
                name = tb_nameThing.Text.Trim(),
                isExcluded = cb_isDisabledThing.IsChecked == true,
                description = tb_descriptionThing.Text,
                // image
            };

            ImageSource? newImg = null;
            if (imgThing_newImg)
            {
                newImg = img_thing.Source;
                imgThing_newImg = false;
            }

            core.CreateNewOrUpdateThing(newThing, newImg, out DataGridItemModelThing? newVM);
            if (newVM != null)
            {
                dg_things.SelectedItem = newVM;
                dg_things.ScrollIntoView(newVM);
            }
            core.SetCursorArrow(this);
            SystemSounds.Asterisk.Play();
            btn_createUpdateThing_needNotSave = true;
            AnimaSaved();
        }
        private void btn_createUpdateThing_LostFocus(object sender, RoutedEventArgs e)
        {
            if (btn_createUpdateThing_needNotSave)
            {
                btn_createUpdateThing_needNotSave = false;
            }
        }
        #endregion

        #endregion






        #region tab channels

        #region channel, select channel, load info
        private async void dg_channels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_channels.SelectedItem == null)
            {
                core.selectedMaintainChannel = null;
            }
            else
            {
                core.selectedMaintainChannel = (DataGridItemModelChannel)dg_channels.SelectedItem;
                dg_channels.ScrollIntoView(dg_channels.SelectedItem);
            }

            searcherChannelName.SkipOrReset();
            searcherChannelContent.SkipOrReset();

            tbv_nameChannel.Clear();
            tbv_header_myId_channel.Text = "";
            tbv_IdChannel.Clear();
            tbv_parentIdChannel.Clear();


            if (core.selectedMaintainChannel == null)
            {
                if (img_channel.Source != null && img_channel.Source != ImageIO.Image_Unknow)
                {
                    imgChannel_newImg = true;
                }
                CreateUpdateChannel_createOrUpdate = true;
            }
            else
            {
                await core.SetCursorWait(this);
                imgChannel_newImg = false;

                // clear content warping panel
                wPanel_contents.Children.Clear();

                Channels.Channel? c = core.selectedMaintainChannel.data, cp = core.selectedMaintainChannel.dataParent;
                if (cp != null && c != null)
                {
                    tbv_nameChannel.Text = cp.name;
                    tb_nameChannel.Text = core.selectedMaintainChannel.Name;
                    tb_contentChannel.Text = core.selectedMaintainChannel.ContentListTx;

                    tbv_header_myId_channel.Text = c.id.ToString();
                    tbv_IdChannel.Text = c.id.ToString();
                    tbv_parentIdChannel.Text = cp.id.ToString();

                    CreateUpdateChannel_createOrUpdate = false;
                }
                else if (cp != null)
                {
                    tbv_nameChannel.Clear();
                    tb_nameChannel.Text = cp.name;
                    tb_contentChannel.Text = cp.ContentListTx;

                    tbv_header_myId_channel.Text =
                        (string)Application.Current.TryFindResource("lb_winMaintain_preIdInherite")
                        + " " + cp.id.ToString();
                    tbv_parentIdChannel.Text = cp.id.ToString();

                    CreateUpdateChannel_createOrUpdate = true;
                }
                else // if (c != null)
                {
                    tbv_nameChannel.Clear();
                    tb_nameChannel.Text = c.name;
                    tb_contentChannel.Text = c.ContentListTx;

                    tbv_header_myId_channel.Text = c.id.ToString();
                    tbv_IdChannel.Text = c.id.ToString();

                    CreateUpdateChannel_createOrUpdate = false;
                }

                cb_isDisabledChannel.IsChecked = core.selectedMaintainChannel.IsExcluded;
                tb_speedChannel.Text = core.selectedMaintainChannel.Speed.ToString();
                Channel_FillWPanelItems(core.selectedMaintainChannel.contentList);
                tb_descriptionChannel.Text = core.selectedMaintainChannel.Description;
                img_channel.Source = core.selectedMaintainChannel.Image;

                await core.SetCursorArrow(this);
            }
        }
        private void Channel_FillWPanelItems(List<Channels.Channel.ContentItem> list)
        {
            Channels.Channel.ContentItem curItem;
            List<Guid> idList = new List<Guid>();
            for (int i = 0, iv = list.Count; i < iv; ++i)
            {
                curItem = list[i];
                if (curItem.addOrRemove == false)
                {
                    continue;
                }
                idList.Add(curItem.contentId);
            }
            Channel_FillWPanelItems(idList);
        }
        private void Channel_FillWPanelItems(List<Guid> idList)
        {
            Things.Thing? foundThing;
            ThingInQuantity newUI;
            Thickness marg = new Thickness(0, 0, 16, 0);
            for (int i = 0, iv = idList.Count; i < iv; ++i)
            {
                foundThing = core.FindThing(idList[i]);
                if (foundThing == null)
                {
                    continue;
                }
                newUI = new ThingInQuantity()
                {
                    IsInputEnabled = false,
                    Width = 200,
                    ThingId = foundThing.id,
                    Margin = marg,
                };
                newUI.CloseBtnClicked += ChannelContentItem_CloseBtnClicked;
                wPanel_contents.Children.Add(newUI);
            }
        }

        private void ChannelContentItem_CloseBtnClicked(object? sender, EventArgs e)
        {
            if (sender is not ThingInQuantity)
            {
                return;
            }
            ThingInQuantity ui = (ThingInQuantity)sender;
            if (ui.Parent != wPanel_contents)
            {
                return;
            }
            wPanel_contents.Children.Remove(ui);
        }

        private List<Channels.Channel.ContentItem> Channel_GetContentListFromWPanel()
        {
            List<Channels.Channel.ContentItem> result = new List<Channels.Channel.ContentItem>();
            ThingInQuantity ui;
            for (int i = 0, iv = wPanel_contents.Children.Count; i < iv; ++i)
            {
                if (wPanel_contents.Children[i] is not ThingInQuantity)
                {
                    continue;
                }
                ui = (ThingInQuantity)wPanel_contents.Children[i];
                if (ui.ThingId == Guid.Empty)
                {
                    continue;
                }
                result.Add(new Channels.Channel.ContentItem()
                {
                    addOrRemove = true,
                    contentId = ui.ThingId,
                });
            }
            return result;
        }

        private void btn_deSelectChannel_Click(object sender, RoutedEventArgs e)
        {
            dg_channels.SelectedItem = null;
        }

        private void btn_clearUIChannel_Click(object sender, RoutedEventArgs e)
        {
            tb_nameChannel.Clear();
            tbv_nameChannel.Clear();
            tb_contentChannel.Clear();

            tbv_nameChannelSearchCount.Text = "";
            tbv_contentChannelSearchCount.Text = "";

            //tbv_header_myId_channel.Text = "";

            cb_isDisabledChannel.IsEnabled = false;
            tb_speedChannel.Clear();
            tb_descriptionChannel.Clear();
            img_channel.Source = ImageIO.Image_Unknow;

            wPanel_contents.Children.Clear();
        }
        #endregion


        #region channel, quick search

        #region channel, quick search name
        private bool tb_nameChannel_ignoreKeyUpOnce = false;
        private void tb_nameChannel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tb_nameChannel_ignoreKeyUpOnce = true;
                btn_searchNameChannel_Click(sender, new RoutedEventArgs());
            }
        }
        private void tb_nameChannel_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (tb_nameChannel_ignoreKeyUpOnce)
            {
                tb_nameChannel_ignoreKeyUpOnce = false;
                return;
            }
            searcherChannelName.Reset();
        }

        private async void btn_searchNameChannel_Click(object sender, RoutedEventArgs e)
        {
            await core.SetCursorWait(this);
            searcherChannelName.SearchOrNext(tb_nameChannel.Text);
            core.SetCursorArrow(this);
        }
        #endregion

        #region channel, quick search content
        private bool tb_contentChannel_ignoreKeyUpOnce = false;
        private void tb_contentChannel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tb_contentChannel_ignoreKeyUpOnce = true;
                btn_searchContentChannel_Click(sender, new RoutedEventArgs());
            }
        }
        private void tb_contentChannel_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (tb_contentChannel_ignoreKeyUpOnce)
            {
                tb_contentChannel_ignoreKeyUpOnce = false;
                return;
            }
            searcherChannelContent.Reset();
        }
        private async void btn_searchContentChannel_Click(object sender, RoutedEventArgs e)
        {
            await core.SetCursorWait(this);
            searcherChannelContent.SearchOrNext(tb_contentChannel.Text);
            await core.SetCursorArrow(this);
        }

        private int CheckChannelNameNDescription(object helper, DataGridItemModelChannel item, object? missing)
        {
            SearchHelper<DataGridItemModelChannel> h = (SearchHelper<DataGridItemModelChannel>)helper;
            if (item.Name != null && h.Check(item.Name))
            {
                return 1;
            }
            if (item.Description != null && h.Check(item.Description))
            {
                return 2;
            }
            return 0;
        }
        private int CheckChannelContentNDescription(object helper, DataGridItemModelChannel item, object? missing)
        {
            SearchHelper<DataGridItemModelChannel> h = (SearchHelper<DataGridItemModelChannel>)helper;
            Things.Thing? foundThing;
            foreach (Channels.Channel.ContentItem i in item.contentList)
            {
                if (i.addOrRemove == false)
                {
                    continue;
                }
                foundThing = core.FindThing(i.contentId);
                if (foundThing == null)
                {
                    continue;
                }
                if (foundThing.name != null && h.Check(foundThing.name))
                {
                    return 1;
                }
            }
            return 0;
        }
        #endregion

        #endregion


        #region channel, change picture
        private void rect_imgChannelDropZone_PreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
        private void rect_imgChannelDropZone_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private bool imgChannel_newImg = false;
        private void rect_imgChannelDropZone_PreviewDrop(object sender, DragEventArgs e)
        {
            BitmapSource? bs = UI.QuickGraphics.Image.FromDragDrop(e.Data);
            if (bs != null)
            {
                img_channel.Source = bs;
            }
            else
            {
                img_channel.Source = ImageIO.Image_Unknow;
            }
            imgChannel_newImg = true;
        }

        private void rect_imgChannelDropZone_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            rect_imgChannelDropZone.Focus();
        }

        private void rect_imgChannelDropZone_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                img_channel.Source = ImageIO.Image_Unknow;
                imgChannel_newImg = true;
            }
            else if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control)
                && e.Key == Key.V)
            {
                BitmapSource? bs = UI.QuickGraphics.Image.FromClipboard();
                if (bs != null)
                {
                    img_channel.Source = bs;
                }
                else
                {
                    img_channel.Source = ImageIO.Image_Unknow;
                }
                imgChannel_newImg = true;
            }
        }
        #endregion

        #region channel, add/remove contents
        private async void btn_addContentsChannel_Click(object sender, RoutedEventArgs e)
        {
            await core.SetCursorWait(this);
            WindowSelectThings? win = core.ShowSelectThingsWin(this, true);
            if (win != null && win.SelectedThingIdList != null)
            {
                List<Guid> tmpList = new List<Guid>();
                tmpList.AddRange(win.SelectedThingIdList);
                foreach (Guid existId in Channel_GetContentListFromWPanel().Select(a => a.contentId))
                {
                    if (tmpList.Contains(existId))
                    {
                        tmpList.Remove(existId);
                    }
                }
                Channel_FillWPanelItems(tmpList);
            }
            await core.SetCursorArrow(this);
        }

        #endregion

        #region channel, delete, create or update
        private void btn_deleteChannel_Click(object sender, RoutedEventArgs e)
        {
            if (!core.DeleteSelectedChannelCheck(out string? errMsg, out int errLv))
            {
                switch (errLv)
                {
                    case 0:
                        MessageBox.Show(this, errMsg,
                        (string)Application.Current.TryFindResource("lb_winMaintain_info"),
                            MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        break;
                    case 1:
                        MessageBox.Show(this, errMsg,
                        (string)Application.Current.TryFindResource("lb_winMaintain_warning"),
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                }
                return;
            }
            if (core.selectedMaintainChannel == null)
            {
                throw new Exception("(Impossible) selected maintain channel is null.");
            }
            if (MessageBox.Show(this,
                    string.Format(
                        (string)Application.Current.TryFindResource("lb_winMaintain_msgBox_confirmToDeleteChannel_content"),
                        core.selectedMaintainChannel.Name,
                        Environment.NewLine + Environment.NewLine
                        ),
                    (string)Application.Current.TryFindResource("lb_winMaintain_warning"),
                MessageBoxButton.YesNo, MessageBoxImage.Warning)
                == MessageBoxResult.Yes)
            {
                core.DeleteSelectedChannel();
                AnimaDeleted();
            }
        }

        private async void btn_createUpdateChannel_Click(object sender, RoutedEventArgs e)
        {
            if (btn_createUpdateChannel_needNotSave)
            {
                return;
            }
            if (core.channelsCurrent == null)
            {
                throw new Exception("(Impossible) No current channel.");
            }
            if (string.IsNullOrWhiteSpace(tb_nameChannel.Text))
            {
                MessageBox.Show(this,
                    (string)Application.Current.TryFindResource("lb_winMaintain_msgBox_channelMustHasAName_content"),
                    (string)Application.Current.TryFindResource("lb_winMaintain_msgBox_channelMustHasAName_title"),
                    MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            decimal testD;
            if (!decimal.TryParse(tb_speedChannel.Text, out testD))
            {
                MessageBox.Show(this,
                    (string)Application.Current.TryFindResource("lb_winMaintain_msgBox_channelMustHasSpeed_content"),
                    (string)Application.Current.TryFindResource("lb_winMaintain_msgBox_channelMustHasSpeed_title"),
                    MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            await core.SetCursorWait(this);
            Channels.Channel newChannel = new Channels.Channel(core.channelsCurrent)
            {
                name = tb_nameChannel.Text,
                isExcluded = cb_isDisabledChannel.IsChecked == true,
                speed = testD,
                description = tb_descriptionChannel.Text,
                // image
                // contents
            };


            ImageSource? newImg = null;
            if (imgChannel_newImg)
            {
                newImg = img_channel.Source;
                imgChannel_newImg = false;
            }
            newChannel.contentList = Channel_GetContentListFromWPanel();




            core.CreateNewOrUpdateChannel(newChannel, newImg, out DataGridItemModelChannel? newVM);
            if (newVM != null)
            {
                dg_channels.SelectedItem = newVM;
                dg_channels.ScrollIntoView(newVM);
            }
            await core.SetCursorArrow(this);
            SystemSounds.Asterisk.Play();
            btn_createUpdateChannel_needNotSave = true;
            AnimaSaved();
        }

        private void btn_createUpdateChannel_LostFocus(object sender, RoutedEventArgs e)
        {
            if (btn_createUpdateChannel_needNotSave)
            {
                btn_createUpdateChannel_needNotSave = false;
            }
        }
        private bool _CreateUpdateChannel_createOrUpdate = true;
        private bool CreateUpdateChannel_createOrUpdate
        {
            get => _CreateUpdateChannel_createOrUpdate;
            set
            {
                _CreateUpdateChannel_createOrUpdate = value;
                if (value)
                {
                    if (core.selectedMaintainChannel != null && core.selectedMaintainChannel.dataParent != null)
                    {
                        btn_createUpdateChannel.Content = (string)Application.Current.Resources["lb_winMaintain_inherite"];
                    }
                    else
                    {
                        btn_createUpdateChannel.Content = (string)Application.Current.Resources["lb_winMaintain_create"];
                    }
                }
                else
                {
                    btn_createUpdateChannel.Content = (string)Application.Current.Resources["lb_winMaintain_update"];
                }
            }
        }
        private bool btn_createUpdateChannel_needNotSave = false;

        #endregion



        #endregion





        #region tab recipies


        #region recipes, select recipe
        private void dg_recipes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_recipes.SelectedItem == null)
            {
                core.selectedMaintainRecipe = null;
            }
            else
            {
                core.selectedMaintainRecipe = (DataGridItemModelRecipe)dg_recipes.SelectedItem;
                dg_recipes.ScrollIntoView(dg_recipes.SelectedItem);
            }

            searcherRecipeName.SkipOrReset();
            searcherRecipeIngredi.SkipOrReset();

            tbv_nameRecipe.Clear();
            tbv_header_myId_recipe.Text = "";
            tbv_idRecipe.Clear();
            tbv_parentIdRecipe.Clear();


            if (core.selectedMaintainRecipe == null)
            {
                if (img_recipe.Source != null && img_recipe.Source != ImageIO.Image_Unknow)
                {
                    imgRecipe_newImg = true;
                }
                CreateUpdateRecipe_createOrUpdate = true;
            }
            else
            {
                imgRecipe_newImg = false;

                Recipe_ClearSPanelItems(ref sp_ARecipe);
                Recipe_ClearSPanelItems(ref sp_IRecipe);
                Recipe_ClearSPanelItems(ref sp_ORecipe);

                BitmapImage? testImg, testImgParent;
                Recipes.Recipe? r = core.selectedMaintainRecipe.data, rp = core.selectedMaintainRecipe.dataParent;
                if (rp != null && r != null)
                {
                    tbv_nameRecipe.Text = rp.name;
                    tb_nameRecipe.Text = core.selectedMaintainRecipe.Name;

                    tbv_header_myId_recipe.Text = r.id.ToString();
                    tbv_idRecipe.Text = r.id.ToString();
                    tbv_parentIdRecipe.Text = rp.id.ToString();

                    cb_isDisabledRecipe.IsChecked = core.selectedMaintainRecipe.IsExcluded;

                    Guid pid = Guid.Empty;
                    if (r.processor != null)
                    {
                        pid = (Guid)r.processor;
                    }
                    else if (rp.processor != null)
                    {
                        pid = (Guid)rp.processor;
                    }
                    Things.Thing? foundProcessor = core.FindThing(pid);
                    if (foundProcessor != null)
                    {
                        tiq_processor.ThingId = foundProcessor.id;
                    }
                    else
                    {
                        tiq_processor.ThingId = Guid.Empty;
                    }
                    tiq_processor.SetQuantity(null);
                    tb_periodRecipe.Text = r.period.ToString();

                    // load items
                    Recipe_FillSPanelItems(ref sp_ARecipe, core.selectedMaintainRecipe.aList, true);
                    Recipe_FillSPanelItems(ref sp_IRecipe, core.selectedMaintainRecipe.iList);
                    Recipe_FillSPanelItems(ref sp_ORecipe, core.selectedMaintainRecipe.oList);

                    tb_descriptionRecipe.Text = core.selectedMaintainRecipe.Description;
                    img_recipe.Source = core.selectedMaintainRecipe.Image;
                    CreateUpdateRecipe_createOrUpdate = false;
                }
                else if (rp != null)
                {
                    tbv_nameRecipe.Clear();
                    tb_nameRecipe.Text = rp.name;

                    tbv_header_myId_recipe.Text =
                        (string)Application.Current.TryFindResource("lb_winMaintain_preIdInherite")
                        + " " + rp.id.ToString();
                    tbv_parentIdRecipe.Text = rp.id.ToString();

                    cb_isDisabledRecipe.IsChecked = rp.isExcluded;
                    Things.Thing? processor = null;
                    if (rp.processor != null)
                    {
                        processor = core.FindThing((Guid)rp.processor, true);
                    }
                    if (processor != null)
                    {
                        tiq_processor.ThingId = processor.id;
                    }
                    else
                    {
                        tiq_processor.ThingId = Guid.Empty;
                    }
                    tiq_processor.SetQuantity(null);
                    tb_periodRecipe.Text = rp.period.ToString();

                    Recipe_FillSPanelItems(ref sp_ARecipe, core.selectedMaintainRecipe.aList, true);
                    Recipe_FillSPanelItems(ref sp_IRecipe, core.selectedMaintainRecipe.iList);
                    Recipe_FillSPanelItems(ref sp_ORecipe, core.selectedMaintainRecipe.oList);
                    tb_descriptionRecipe.Text = rp.description;

                    img_recipe.Source = core.selectedMaintainRecipe.Image;
                    CreateUpdateRecipe_createOrUpdate = true;
                }
                else // if (r != null)
                {
                    tbv_nameRecipe.Clear();
                    tb_nameRecipe.Text = r.name;

                    tbv_header_myId_recipe.Text = r.id.ToString();
                    tbv_idRecipe.Text = r.id.ToString();

                    cb_isDisabledRecipe.IsChecked = r.isExcluded;
                    Things.Thing? processor = null;
                    if (r.processor != null)
                    {
                        processor = core.FindThing((Guid)r.processor, true);
                    }
                    if (processor != null)
                    {
                        tiq_processor.ThingId = processor.id;
                    }
                    else
                    {
                        tiq_processor.ThingId = Guid.Empty;
                    }
                    tiq_processor.SetQuantity(null);
                    tb_periodRecipe.Text = r.period.ToString();

                    Recipe_FillSPanelItems(ref sp_ARecipe, core.selectedMaintainRecipe.aList, true);
                    Recipe_FillSPanelItems(ref sp_IRecipe, core.selectedMaintainRecipe.iList);
                    Recipe_FillSPanelItems(ref sp_ORecipe, core.selectedMaintainRecipe.oList);
                    tb_descriptionRecipe.Text = r.description;

                    img_recipe.Source = core.selectedMaintainRecipe.Image;
                    CreateUpdateRecipe_createOrUpdate = false;
                }
            }
        }

        private void Recipe_ClearSPanelItems(ref StackPanel sp)
        {
            int iv = sp.Children.Count;
            if (iv <= 1)
            {
                return;
            }
            ThingInQuantity ui;
            for (iv -= 2; iv >= 0; --iv)
            {
                if (sp.Children[iv] is ThingInQuantity)
                {
                    ui = (ThingInQuantity)sp.Children[iv];
                    ui.IconDoubleClicked -= Ui_IconDoubleClicked;
                    ui.CloseBtnClicked -= Ui_CloseBtnClicked;
                    ui.IconDragStart -= Ui_IconDragStart;
                }
                sp.Children.RemoveAt(iv);
            }
        }
        private void Recipe_FillSPanelItems(ref StackPanel sp, List<Recipes.Recipe.PIOItem> list, bool isAccessories = false)
        {
            Recipes.Recipe.PIOItem pioItem;
            ThingInQuantity ui;
            for (int i = 0, iv = list.Count, j = sp.Children.Count - 1; i < iv; ++i, ++j)
            {
                pioItem = list[i];

                // refresh info
                if (pioItem.thing != null)
                {
                    pioItem.thing = core.FindThing(pioItem.thing.id);
                }

                ui = new ThingInQuantity()
                { ThingId = pioItem.thing == null ? Guid.Empty : pioItem.thing.id };
                ui.SetQuantity(pioItem.quantity);
                if (isAccessories)
                {
                    ui.IsInputEnabled = false;
                }
                ui.IconDoubleClicked += Ui_IconDoubleClicked;
                ui.CloseBtnClicked += Ui_CloseBtnClicked;
                ui.IconDragStart += Ui_IconDragStart;
                sp.Children.Insert(j, ui);
            }
        }
        private StackPanel? recipeIngredient_dragingUI_container = null;
        private ThingInQuantity? recipeIngredient_dragingUI = null;
        private void Ui_IconDragStart(object? sender, EventArgs e)
        {
            if (sender is not ThingInQuantity)
            {
                return;
            }
            recipeIngredient_dragingUI = (ThingInQuantity)sender;
            if (recipeIngredient_dragingUI.Parent is not StackPanel)
            {
                recipeIngredient_dragingUI = null;
                return;
            }
            recipeIngredient_dragingUI_container = (StackPanel)recipeIngredient_dragingUI.Parent;
            recipeIngredient_dragingUI_container.Cursor = Cursors.Hand;
        }

        private List<Recipes.Recipe.PIOItem> Recipe_GetItemsFromSPanel(ref StackPanel sp, bool ignoreQuantity = false)
        {
            List<Recipes.Recipe.PIOItem> result = new List<Recipes.Recipe.PIOItem>();
            if (sp == null)
            {
                return result;
            }
            UIElement uiRaw;
            ThingInQuantity ui;
            Recipes.Recipe.PIOItem newItem;
            for (int i = 0, iv = sp.Children.Count - 1; i < iv; ++i)
            {
                uiRaw = sp.Children[i];
                if (uiRaw is not ThingInQuantity)
                {
                    continue;
                }
                ui = (ThingInQuantity)uiRaw;
                if (ui.ThingId == Guid.Empty)
                {
                    continue;
                }
                newItem = new PIOItem(core.FindThing(ui.ThingId), ui.GetQuantityCopy());
                if (ignoreQuantity && newItem.quantity != null)
                {
                    newItem.quantity.Clear();
                }
                result.Add(newItem);
            }
            return result;
        }



        private void btn_deselectRecipe_Click(object sender, RoutedEventArgs e)
        {
            dg_recipes.SelectedItem = null;
        }

        private void btn_clearUIRecipe_Click(object sender, RoutedEventArgs e)
        {
            tb_nameRecipe.Clear();
            //tb_ingredRecipe.Clear();
            //tb_typeRecipe.Clear();

            //tbv_header_myId_recipe.Text = "";

            cb_isDisabledRecipe.IsChecked = false;

            tiq_processor.ThingId = Guid.Empty;
            tiq_processor.SetQuantity(null);
            tb_periodRecipe.Clear();

            // clear items
            Recipe_ClearSPanelItems(ref sp_ARecipe);
            Recipe_ClearSPanelItems(ref sp_IRecipe);
            Recipe_ClearSPanelItems(ref sp_ORecipe);

            tb_descriptionRecipe.Clear();
            img_recipe.Source = ImageIO.Image_Unknow;
        }
        #endregion


        #region recipe, change/remove accessories, ingredients

        private async void Ui_IconDoubleClicked(object? sender, EventArgs e)
        {
            if (sender is not ThingInQuantity)
            {
                return;
            }

            await core.SetCursorWait(this);
            WindowSelectThings? win = core.ShowSelectThingsWin(this);
            if (win != null
                && win.SelectedThingIdList != null
                && win.SelectedThingIdList.Count > 0)
            {
                Guid newThingId = (Guid)win.SelectedThingIdList[0];
                Things.Thing? nt = core.FindThing(newThingId, true);

                ThingInQuantity ui = (ThingInQuantity)sender;
                ui.ThingId = nt == null ? Guid.Empty : nt.id;
            }
            await core.SetCursorArrow(this);
        }
        private void Ui_CloseBtnClicked(object? sender, EventArgs e)
        {
            if (sender is not ThingInQuantity)
            {
                return;
            }
            ThingInQuantity ui = (ThingInQuantity)sender;
            if (ui.Parent is not StackPanel)
            {
                throw new Exception($"Ctrls:ThingInQuantity from unknow container [{ui.Parent.ToString()}]");
            }

            // remove
            ui.IconDoubleClicked -= Ui_IconDoubleClicked;
            ui.CloseBtnClicked -= Ui_CloseBtnClicked;
            ((StackPanel)ui.Parent).Children.Remove(ui);

        }
        #endregion


        #region recipes, quick search, by recipe-name, by ingredient-name, or by ingredient-type
        private bool tb_nameRecipe_ignoreKeyUpOnce = false;
        private void tb_nameRecipe_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tb_nameRecipe_ignoreKeyUpOnce = true;
                btn_nameRecipeSearch_Click(sender, e);
            }
        }
        private void tb_nameRecipe_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (tb_nameRecipe_ignoreKeyUpOnce)
            {
                tb_nameRecipe_ignoreKeyUpOnce = false;
                return;
            }
            searcherRecipeName.Reset();
        }
        private void btn_nameRecipeSearch_Click(object sender, RoutedEventArgs e)
        {
            searcherRecipeName.SearchOrNext(tb_nameRecipe.Text);
        }
        private int CheckRecipeNameNDescription(object helper, DataGridItemModelRecipe item, object? missing)
        {
            SearchHelper<DataGridItemModelRecipe> h = (SearchHelper<DataGridItemModelRecipe>)helper;
            if (item.Name != null && h.Check(item.Name))
            {
                return 1;
            }
            if (item.Description != null && h.Check(item.Description))
            {
                return 2;
            }
            return 0;
        }

        private bool tb_ingredRecipe_ignoreKeyUpOnce = false;
        private void tb_ingredRecipe_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tb_ingredRecipe_ignoreKeyUpOnce = true;
                btn_ingredRecipeSearch_Click(sender, e);
            }
        }
        private void tb_ingredRecipe_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (tb_ingredRecipe_ignoreKeyUpOnce)
            {
                tb_ingredRecipe_ignoreKeyUpOnce = false;
                return;
            }
            searcherRecipeIngredi.Reset();
        }
        private void rb_ingredRecipeSearch_rangChanged(object sender, RoutedEventArgs e)
        {
            searcherRecipeIngredi?.Reset();
        }
        private void btn_ingredRecipeSearch_Click(object sender, RoutedEventArgs e)
        {
            int arg1 = 0;
            if (rb_ingredRecipeSearch_input.IsChecked == true) arg1 = 1;
            else if (rb_ingredRecipeSearch_output.IsChecked == true) arg1 = 2;
            else if (rb_ingredRecipeSearch_processor.IsChecked == true) arg1 = 3;
            else if (rb_ingredRecipeSearch_accessory.IsChecked == true) arg1 = 4;
            searcherRecipeIngredi.SearchOrNext(tb_ingredRecipe.Text, true, arg1);
        }
        private int CheckRecipeIngredi(object helper, DataGridItemModelRecipe item, object arg1)
        {
            SearchHelper<DataGridItemModelRecipe> h = (SearchHelper<DataGridItemModelRecipe>)helper;
            List<Recipes.Recipe.PIOItem> fullList = new List<PIOItem>();
            if (arg1 is int)
            {
                switch ((int)arg1)
                {
                    case 0:
                        fullList.AddRange(item.iList);
                        fullList.AddRange(item.oList);
                        if (item.data is not null && item.data.processor is not null)
                        {
                            Things.Thing? pro = core.FindThing(item.data.processor.Value);
                            if (pro is not null)
                            {
                                fullList.Add(new PIOItem(pro, null));
                            }
                        }
                        fullList.AddRange(item.aList);
                        break;
                    case 1:
                        fullList.AddRange(item.iList);
                        break;
                    case 2:
                        fullList.AddRange(item.oList);
                        break;
                    case 3:
                        if (item.data is not null && item.data.processor is not null)
                        {
                            Things.Thing? pro = core.FindThing(item.data.processor.Value);
                            if (pro is not null)
                            {
                                fullList.Add(new PIOItem(pro, null));
                            }
                        }
                        break;
                    case 4:
                        fullList.AddRange(item.aList);
                        break;
                }
            }
            foreach (Recipes.Recipe.PIOItem i in fullList)
            {
                if (i.thing != null
                    && i.thing.name != null
                    && h.Check(i.thing.name))
                {
                    return 1;
                }
            }
            return 0;
        }



        #endregion


        #region recipes, change processor, add/remove A, I, O

        private async void tiq_processor_IconDoubleClicked(object sender, EventArgs e)
        {
            if (sender is not ThingInQuantity)
            {
                return;
            }

            await core.SetCursorWait(this);
            WindowSelectThings? win = core.ShowSelectThingsWin(this);
            if (win != null
                && win.SelectedThingIdList != null
                && win.SelectedThingIdList.Count > 0)
            {
                Guid newThingId = (Guid)win.SelectedThingIdList[0];
                Things.Thing? nt = core.FindThing(newThingId);

                ThingInQuantity ui = (ThingInQuantity)sender;
                if (nt.isExcluded == true)
                {
                    MessageBox.Show(this, "This machine has been disabled.", "No-can-do", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ui.ThingId = Guid.Empty;
                    img_recipe.Source = ImageIO.Image_Unknow;
                    tb_nameRecipe.Text = "";
                }
                else
                {
                    ui.ThingId = nt == null ? Guid.Empty : nt.id;

                    // auto change recipe image as processor image
                    BitmapImage? testImg = ImageIO.GetOut(nt);
                    if (testImg != null)
                    {
                        img_recipe.Source = testImg;
                        imgRecipe_newImg = true;
                    }
                    else
                    {
                        img_recipe.Source = ImageIO.Image_Unknow;
                    }

                    // auto set recipe name, if empty
                    if (nt != null && string.IsNullOrWhiteSpace(tb_nameRecipe.Text))
                    {
                        tb_nameRecipe.Text = nt.name;
                    }
                }
            }
            await core.SetCursorArrow(this);
        }

        private void btn_addARecipe_Click(object sender, RoutedEventArgs e)
        {
            AddRecipeUI(ref sp_ARecipe, true);
        }
        private void btn_addIRecipe_Click(object sender, RoutedEventArgs e)
        {
            AddRecipeUI(ref sp_IRecipe);
        }
        private void btn_addORecipe_Click(object sender, RoutedEventArgs e)
        {
            AddRecipeUI(ref sp_ORecipe);
        }
        private void AddRecipeUI(ref StackPanel sp, bool isAccessories = false)
        {
            ThingInQuantity newUI = new ThingInQuantity()
            {
                IsInputEnabled = !isAccessories,
            };
            newUI.IconDoubleClicked += Ui_IconDoubleClicked;
            newUI.CloseBtnClicked += Ui_CloseBtnClicked;
            newUI.IconDragStart += Ui_IconDragStart;
            sp.Children.Insert(sp.Children.Count - 1, newUI);
        }
        #endregion



        #region recipes change recipe picture
        private void rect_imgRecipeDropZone_PreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
        private void rect_imgRecipeDropZone_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private bool imgRecipe_newImg = false;
        private void rect_imgRecipeDropZone_PreviewDrop(object sender, DragEventArgs e)
        {
            BitmapSource? bs = UI.QuickGraphics.Image.FromDragDrop(e.Data);
            if (bs != null)
            {
                img_recipe.Source = bs;
            }
            else
            {
                img_recipe.Source = ImageIO.Image_Unknow;
            }
            imgRecipe_newImg = true;
        }
        private void rect_imgRecipeDropZone_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            rect_imgRecipeDropZone.Focus();
        }

        private void rect_imgRecipeDropZone_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                img_recipe.Source = ImageIO.Image_Unknow;
                imgRecipe_newImg = true;
            }
            else if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control)
                && e.Key == Key.V)
            {
                BitmapSource? bs = UI.QuickGraphics.Image.FromClipboard();
                if (bs != null)
                {
                    img_recipe.Source = bs;
                }
                else
                {
                    img_recipe.Source = ImageIO.Image_Unknow;
                }
                imgRecipe_newImg = true;
            }
        }
        #endregion


        #region recipes, delete, create or update
        private void btn_deleteRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (!core.DeleteSelectedRecipeCheck(out string? errMsg, out int errLv))
            {
                switch (errLv)
                {
                    case 0:
                        MessageBox.Show(this, errMsg,
                            (string)Application.Current.TryFindResource("lb_winMaintain_info"),
                            MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        break;
                    case 1:
                        MessageBox.Show(this, errMsg,
                            (string)Application.Current.TryFindResource("lb_winMaintain_warning"),
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                }
                return;
            }
            if (core.selectedMaintainRecipe == null)
            {
                throw new Exception("(Impossible) selected maintain recipe is null.");
            }
            if (MessageBox.Show(this,
                string.Format(
                    (string)Application.Current.TryFindResource("lb_winMaintain_msgBox_confirmToDeleteRecipe_content"),
                    core.selectedMaintainRecipe.Name,
                    Environment.NewLine + Environment.NewLine
                    ),
                (string)Application.Current.TryFindResource("lb_winMaintain_warning"),
                MessageBoxButton.YesNo, MessageBoxImage.Warning)
                == MessageBoxResult.Yes)
            {
                core.DeleteSelectedRecipe();
                AnimaDeleted();
            }
        }

        private bool _CreateUpdateRecipe_createOrUpdate = true;
        private bool CreateUpdateRecipe_createOrUpdate
        {
            get => _CreateUpdateRecipe_createOrUpdate;
            set
            {
                _CreateUpdateRecipe_createOrUpdate = value;
                if (value)
                {
                    if (core.selectedMaintainRecipe != null && core.selectedMaintainRecipe.dataParent != null)
                    {
                        btn_createOrUpdateRecipe.Content = (string)Application.Current.Resources["lb_winMaintain_inherite"];
                    }
                    else
                    {
                        btn_createOrUpdateRecipe.Content = (string)Application.Current.Resources["lb_winMaintain_create"];
                    }
                }
                else
                {
                    btn_createOrUpdateRecipe.Content = (string)Application.Current.Resources["lb_winMaintain_update"];
                }
            }
        }

        private bool btn_createOrUpdateRecipe_needNotSave = false;
        private async void btn_createOrUpdateRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (btn_createOrUpdateRecipe_needNotSave)
            {
                return;
            }
            if (core.recipesCurrent == null)
            {
                throw new Exception("(Impossible) No current recipe.");
            }
            if (string.IsNullOrWhiteSpace(tb_nameRecipe.Text))
            {
                MessageBox.Show(this,
                    (string)Application.Current.TryFindResource("lb_winMaintain_msgBox_recipeMustHasAName_content"),
                    (string)Application.Current.TryFindResource("lb_winMaintain_msgBox_recipeMustHasAName_title"),
                    MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            decimal period;
            if (!decimal.TryParse(tb_periodRecipe.Text, out period) || period <= 0)
            {
                MessageBox.Show(this,
                    (string)Application.Current.TryFindResource("lb_winMaintain_msgBox_periodMustGreatorThanZero_content"),
                    (string)Application.Current.TryFindResource("lb_winMaintain_msgBox_periodMustGreatorThanZero_title"),
                    MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            await core.SetCursorWait(this);
            Recipes.Recipe newRecipe = new Recipes.Recipe(core.recipesCurrent)
            {
                name = tb_nameRecipe.Text,
                description = tb_descriptionRecipe.Text,
                isExcluded = cb_isDisabledRecipe.IsChecked == true,
                processor = tiq_processor.ThingId == Guid.Empty ? null : tiq_processor.ThingId,
                accessories = Recipe_GetItemsFromSPanel(ref sp_ARecipe, true),
                period = period,
                inputs = Recipe_GetItemsFromSPanel(ref sp_IRecipe),
                outputs = Recipe_GetItemsFromSPanel(ref sp_ORecipe),
                // image
            };

            ImageSource? newImg = null;
            if (imgRecipe_newImg)
            {
                newImg = img_recipe.Source;
                imgRecipe_newImg = false;
            }

            core.CreateNewOrUpdateRecipe(newRecipe, newImg, out DataGridItemModelRecipe? newVM);
            if (newVM != null)
            {
                dg_recipes.SelectedItem = newVM;
                dg_recipes.ScrollIntoView(newVM);
            }
            await core.SetCursorArrow(this);
            SystemSounds.Asterisk.Play();
            btn_createOrUpdateRecipe_needNotSave = true;
            AnimaSaved();
        }
        private void btn_createOrUpdateRecipe_LostFocus(object sender, RoutedEventArgs e)
        {
            if (btn_createOrUpdateRecipe_needNotSave)
            {
                btn_createOrUpdateRecipe_needNotSave = false;
            }
        }



        #endregion

        #endregion

    }
}
