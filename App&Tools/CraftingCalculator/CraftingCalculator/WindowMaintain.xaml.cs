using MadTomDev.App.Classes;
using MadTomDev.App.Ctrls;
using MadTomDev.App.VMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static MadTomDev.App.Classes.Recipes.Recipe;
using static MadTomDev.App.WindowMaintain;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
        }

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
                BitmapImage? testImg, testImgParent;
                Things.Thing? t = core.selectedMaintainThing.data, tp = core.selectedMaintainThing.dataParent;
                if (tp != null && t != null)
                {
                    tbv_nameThing.Text = tp.name;
                    tb_nameThing.Text = core.selectedMaintainThing.Name;

                    tbv_header_myId_thing.Text = t.id.ToString();
                    tbv_IdThing.Text = t.id.ToString();
                    tbv_parentIdThing.Text = tp.id.ToString();

                    cb_isDisabledThing.IsChecked = core.selectedMaintainThing.IsExcluded;
                    tb_unitThing.Text = t.unit == null ? tp.unit : t.unit;
                    tb_descriptionThing.Text = core.selectedMaintainThing.Description;
                    img_thing.Source = core.selectedMaintainThing.Image;
                    CreateUpdateThing_createOrUpdate = false;
                }
                else if (tp != null)
                {
                    tbv_nameThing.Clear();
                    tb_nameThing.Text = tp.name;

                    tbv_header_myId_thing.Text = "(inherited) " + tp.id.ToString();
                    tbv_parentIdThing.Text = tp.id.ToString();

                    cb_isDisabledThing.IsChecked = tp.isExcluded;
                    tb_unitThing.Text = tp.unit;
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
                    tb_unitThing.Text = t.unit;
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
            tb_unitThing.Clear();
            tb_descriptionThing.Clear();
            img_thing.Source = ImageIO.Image_Unknow;
        }
        #endregion


        #region thing, quick search by name
        private void tb_nameThing_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btn_searchNameThing_Click(sender, new RoutedEventArgs());
            }
        }
        private void tb_nameThing_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            searcherThingName.Reset();
        }

        private void btn_searchNameThing_Click(object sender, RoutedEventArgs e)
        {
            searcherThingName.SearchOrNext(tb_nameThing.Text);
        }
        private bool CheckThingNameNDescription(object helper, DataGridItemModelThing item)
        {
            SearchHelper<DataGridItemModelThing> h = (SearchHelper<DataGridItemModelThing>)helper;
            if (item.Name != null && h.Check(item.Name))
            {
                return true;
            }
            if (item.Description != null && h.Check(item.Description))
            {
                return true;
            }
            return false;
        }


        public class SearchHelper<T>
        {

            private DataGrid dataGrid;
            private TextBlock tbvCounter;
            private Common.SimpleStringHelper.Checker_starNQues checker = new Common.SimpleStringHelper.Checker_starNQues();
            List<T> result = new List<T>();
            int curIndex = -1;
            int curState = 0;
            public SearchHelper(DataGrid dataGrid, TextBlock tbvCounter)
            {
                this.dataGrid = dataGrid;
                this.tbvCounter = tbvCounter;
            }

            public Func<object, T, bool>? CheckItemFunc = null;
            public void SearchOrNext(string? searchingFor = null)
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
                        SelectAndViewDataGridItem(result[curIndex]);
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
                    checker.AddPattern($"*{searchingFor}*");
                    foreach (T t in dataGrid.ItemsSource)
                    {
                        if (CheckItemFunc != null && CheckItemFunc(this, t))
                        {
                            result.Add(t);
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
            private void SelectAndViewDataGridItem(T item)
            {
                dataGrid.SelectedItem = item;
                UI.VisualHelper.DataGrid.ScrollIntoView(dataGrid, null, item);
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
                        MessageBox.Show(this, errMsg, "Info", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        break;
                    case 1:
                        MessageBox.Show(this, errMsg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                }
                return;
            }
            if (core.CheckSelectedThingUsedByCannelOrRecipe(out Guid channelId,out Guid recipeId))
            {
                if (channelId != Guid.Empty)
                {
                    MessageBox.Show(this,
                        $"Cant delete, it's used by Channel[{core.FindChannel(channelId)?.name}]",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if(recipeId != Guid.Empty)
                {
                    MessageBox.Show(this,
                        $"Cant delete, it's used by Recipe[{core.FindRecipe(recipeId)?.name}]",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                return;
            }
            if (core.selectedMaintainThing == null)
            {
                throw new Exception("(Impossible) selected maintain thing is null.");
            }
            if (MessageBox.Show(this, $"Delete thing[{core.selectedMaintainThing.Name}],{Environment.NewLine
                + Environment.NewLine}Continue?!", "Warning",
                MessageBoxButton.YesNo, MessageBoxImage.Warning)
                == MessageBoxResult.Yes)
            {
                core.DeleteSelectedThing();
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
                        btn_createUpdateThing.Content = "Inherite";
                    }
                    else
                    {
                        btn_createUpdateThing.Content = "Create";
                    }
                }
                else
                {
                    btn_createUpdateThing.Content = "Update";
                }
            }
        }

        private bool btn_createUpdateThing_needNotSave = false;
        private void btn_createUpdateThing_Click(object sender, RoutedEventArgs e)
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

            Things.Thing newThing = new Things.Thing(core.thingsCurrent)
            {
                name = tb_nameThing.Text.Trim(),
                isExcluded = cb_isDisabledThing.IsChecked == true,
                unit = tb_unitThing.Text.Trim(),
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
            SystemSounds.Asterisk.Play();
            btn_createUpdateThing_needNotSave = true;
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
        private void dg_channels_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

                    tbv_header_myId_channel.Text = "(inherited) " + cp.id.ToString();
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
                    Item = foundThing,
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
                if (ui.Item == null)
                {
                    continue;
                }
                result.Add(new Channels.Channel.ContentItem()
                {
                    addOrRemove = true,
                    contentId = ui.Item.id,
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
        private void tb_nameChannel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btn_searchNameChannel_Click(sender, new RoutedEventArgs());
            }
        }
        private void tb_nameChannel_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            searcherChannelName.Reset();
        }

        private void btn_searchNameChannel_Click(object sender, RoutedEventArgs e)
        {
            searcherChannelName.SearchOrNext(tb_nameChannel.Text);
        }
        #endregion

        #region channel, quick search content
        private void tb_contentChannel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btn_searchContentChannel_Click(sender, new RoutedEventArgs());
            }
        }
        private void tb_contentChannel_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            searcherChannelContent.Reset();
        }
        private void btn_searchContentChannel_Click(object sender, RoutedEventArgs e)
        {
            searcherChannelContent.SearchOrNext(tb_contentChannel.Text);
        }

        private bool CheckChannelNameNDescription(object helper, DataGridItemModelChannel item)
        {
            SearchHelper<DataGridItemModelChannel> h = (SearchHelper<DataGridItemModelChannel>)helper;
            if (item.Name != null && h.Check(item.Name))
            {
                return true;
            }
            if (item.Description != null && h.Check(item.Description))
            {
                return true;
            }
            return false;
        }
        private bool CheckChannelContentNDescription(object helper, DataGridItemModelChannel item)
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
                    return true;
                }
            }
            return false;
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
        private void btn_addContentsChannel_Click(object sender, RoutedEventArgs e)
        {
            WindowSelectThings selectWin = new WindowSelectThings()
            {
                Owner = this,
                DataGridSource = core.maintainThings,
                IsMultiSelect = true,
            };
            if (selectWin.ShowDialog() == true
                && selectWin.SelectedThingIdList != null)
            {
                List<Guid> tmpList = new List<Guid>();
                tmpList.AddRange(selectWin.SelectedThingIdList);
                foreach (Guid existId in Channel_GetContentListFromWPanel().Select(a => a.contentId))
                {
                    if (tmpList.Contains(existId))
                    {
                        tmpList.Remove(existId);
                    }
                }
                Channel_FillWPanelItems(tmpList);
            }
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
                        MessageBox.Show(this, errMsg, "Info", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        break;
                    case 1:
                        MessageBox.Show(this, errMsg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                }
                return;
            }
            if (core.selectedMaintainChannel == null)
            {
                throw new Exception("(Impossible) selected maintain channel is null.");
            }
            if (MessageBox.Show(this, $"Delete channel[{core.selectedMaintainChannel.Name}],{Environment.NewLine
                + Environment.NewLine}Continue?!", "Warning",
                MessageBoxButton.YesNo, MessageBoxImage.Warning)
                == MessageBoxResult.Yes)
            {
                core.DeleteSelectedChannel();
            }
        }

        private void btn_createUpdateChannel_Click(object sender, RoutedEventArgs e)
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
                MessageBox.Show(this, "Channel must has a name.", "Stop", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            decimal testD;
            if (!decimal.TryParse(tb_speedChannel.Text, out testD))
            {
                MessageBox.Show(this, "speed is not a number.", "Stop", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

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
            SystemSounds.Asterisk.Play();
            btn_createUpdateChannel_needNotSave = true;
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
                        btn_createUpdateChannel.Content = "Inherite";
                    }
                    else
                    {
                        btn_createUpdateChannel.Content = "Create";
                    }
                }
                else
                {
                    btn_createUpdateChannel.Content = "Update";
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
                        tiq_processor.PIOItem = new Recipes.Recipe.PIOItem(foundProcessor, null);
                    }
                    else
                    {
                        tiq_processor.PIOItem = null;
                    }
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

                    tbv_header_myId_recipe.Text = "(inherited) " + rp.id.ToString();
                    tbv_parentIdRecipe.Text = rp.id.ToString();

                    cb_isDisabledRecipe.IsChecked = rp.isExcluded;
                    Things.Thing? processor = null;
                    if (rp.processor != null)
                    {
                        processor = core.FindThing((Guid)rp.processor, true);
                    }
                    if (processor != null)
                    {
                        tiq_processor.PIOItem = new Recipes.Recipe.PIOItem(processor, null);
                    }
                    else
                    {
                        tiq_processor.PIOItem = null;
                    }
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
                        tiq_processor.PIOItem = new Recipes.Recipe.PIOItem(processor, null);
                    }
                    else
                    {
                        tiq_processor.PIOItem = null;
                    }
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
            for (iv -= 2; iv >= 0; --iv)
            {
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
                { PIOItem = pioItem, };
                if (isAccessories)
                {
                    ui.IsInputEnabled = false;
                }
                ui.IconDoubleClicked += Ui_IconDoubleClicked;
                ui.CloseBtnClicked += Ui_CloseBtnClicked;
                sp.Children.Insert(j, ui);
            }
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
                newItem = ui.PIOItem.Clone();
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

            tiq_processor.PIOItem = null;
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

        private void Ui_IconDoubleClicked(object? sender, EventArgs e)
        {
            if (sender is not ThingInQuantity)
            {
                return;
            }

            WindowSelectThings windowSelectThing = new WindowSelectThings()
            {
                Owner = this,
                DataGridSource = core.maintainThings,
            };
            if (windowSelectThing.ShowDialog() == true
                && windowSelectThing.SelectedThingIdList != null
                && windowSelectThing.SelectedThingIdList.Count > 0)
            {
                Guid newThingId = (Guid)windowSelectThing.SelectedThingIdList[0];
                Things.Thing? nt = core.FindThing(newThingId, true);

                ThingInQuantity ui = (ThingInQuantity)sender;
                ui.Item = nt;
            }
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
        private void tb_nameRecipe_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btn_nameRecipeSearch_Click(sender, e);
            }
        }
        private void tb_nameRecipe_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            searcherRecipeName.Reset();
        }
        private void btn_nameRecipeSearch_Click(object sender, RoutedEventArgs e)
        {
            searcherRecipeName.SearchOrNext(tb_nameRecipe.Text);
        }
        private bool CheckRecipeNameNDescription(object helper, DataGridItemModelRecipe item)
        {
            SearchHelper<DataGridItemModelRecipe> h = (SearchHelper<DataGridItemModelRecipe>)helper;
            if (item.Name != null && h.Check(item.Name))
            {
                return true;
            }
            if (item.Description != null && h.Check(item.Description))
            {
                return true;
            }
            return false;
        }

        private void tb_ingredRecipe_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btn_ingredRecipeSearch_Click(sender, e);
            }
        }
        private void tb_ingredRecipe_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            searcherRecipeIngredi.Reset();
        }
        private void btn_ingredRecipeSearch_Click(object sender, RoutedEventArgs e)
        {
            searcherRecipeIngredi.SearchOrNext(tb_ingredRecipe.Text);
        }
        private bool CheckRecipeIngredi(object helper, DataGridItemModelRecipe item)
        {
            SearchHelper<DataGridItemModelRecipe> h = (SearchHelper<DataGridItemModelRecipe>)helper;
            List<Recipes.Recipe.PIOItem> fullList = new List<PIOItem>();
            fullList.AddRange(item.aList);
            fullList.AddRange(item.iList);
            fullList.AddRange(item.oList);
            foreach (Recipes.Recipe.PIOItem i in fullList)
            {
                if (i.thing != null
                    && i.thing.name != null
                    && h.Check(i.thing.name))
                {
                    return true;
                }
            }
            return false;
        }



        #endregion


        #region recipes, change processor, add/remove A, I, O

        private void tiq_processor_IconDoubleClicked(object sender, EventArgs e)
        {
            if (sender is not ThingInQuantity)
            {
                return;
            }

            WindowSelectThings windowSelectThing = new WindowSelectThings()
            {
                Owner = this,
                DataGridSource = core.maintainThings,
            };
            if (windowSelectThing.ShowDialog() == true
                && windowSelectThing.SelectedThingIdList != null
                && windowSelectThing.SelectedThingIdList.Count > 0)
            {
                Guid newThingId = (Guid)windowSelectThing.SelectedThingIdList[0];
                Things.Thing? nt = core.FindThing(newThingId);

                ThingInQuantity ui = (ThingInQuantity)sender;
                ui.Item = nt;

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
                        MessageBox.Show(this, errMsg, "Info", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        break;
                    case 1:
                        MessageBox.Show(this, errMsg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                }
                return;
            }
            if (core.selectedMaintainRecipe == null)
            {
                throw new Exception("(Impossible) selected maintain recipe is null.");
            }
            if (MessageBox.Show(this, $"Delete recipe[{core.selectedMaintainRecipe.Name}],{Environment.NewLine
                + Environment.NewLine}Continue?!", "Warning",
                MessageBoxButton.YesNo, MessageBoxImage.Warning)
                == MessageBoxResult.Yes)
            {
                core.DeleteSelectedRecipe();
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
                        btn_createOrUpdateRecipe.Content = "Inherite";
                    }
                    else
                    {
                        btn_createOrUpdateRecipe.Content = "Create";
                    }
                }
                else
                {
                    btn_createOrUpdateRecipe.Content = "Update";
                }
            }
        }

        private bool btn_createOrUpdateRecipe_needNotSave = false;
        private void btn_createOrUpdateRecipe_Click(object sender, RoutedEventArgs e)
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
                MessageBox.Show(this, "Recipe must has a name.", "Stop", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            decimal period;
            if (!decimal.TryParse(tb_periodRecipe.Text, out period) || period <= 0)
            {
                MessageBox.Show(this, "Recipe period must greator than zero.", "Stop", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            Recipes.Recipe newRecipe = new Recipes.Recipe(core.recipesCurrent)
            {
                name = tb_nameRecipe.Text,
                description = tb_descriptionRecipe.Text,
                isExcluded = cb_isDisabledRecipe.IsChecked == true,
                processor = tiq_processor.Item?.id,
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
            SystemSounds.Asterisk.Play();
            btn_createOrUpdateRecipe_needNotSave = true;
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
