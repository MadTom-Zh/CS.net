using MadTomDev.App.Classes;
using MadTomDev.Common;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            core.mainWindow = this;
            treeView.ItemsSource = core.sceneMgr.treeViewSource;
            core.sceneMgr.ReLoadSceneTreeRoots();

            // try load language
            core.LoadSelectedLanguage();
            core.TrySetLanguage(this);
        }
        Core core = Core.Instance;
        SceneMgr sceneMgr = Core.Instance.sceneMgr;


        #region reload scene tree, load node
        private void btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            ClearUI();
            core.sceneMgr.ReLoadSceneTreeRoots();
        }
        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            tbv_name.Visibility = Visibility.Visible;
            tb_name.Visibility = Visibility.Collapsed;
            tbv_description.Visibility = Visibility.Visible;
            tb_description.Visibility = Visibility.Collapsed;
            object tNode = treeView.SelectedItem;
            MadTomDev.UI.VisualHelper.TreeView.BringIntoView(treeView, tNode);
            if (tNode is VMs.TreeViewNodeModelScene)
            {
                // load info to ui
                core.sceneMgr.selectedTreeViewNode = (VMs.TreeViewNodeModelScene)tNode;
                img.Source = core.sceneMgr.selectedTreeViewNode.Icon;
                tbv_name.Text = core.sceneMgr.selectedTreeViewNode.Text;
                tbv_description.Text = core.sceneMgr.selectedTreeViewNode.Description;
            }
            else
            {
                // clear ui
                core.sceneMgr.selectedTreeViewNode = null;
                ClearUI();
            }
        }
        private void ClearUI()
        {
            img.Source = ImageIO.Image_Unknow;
            tbv_name.Text = "[No name]";
            tbv_description.Text = "[No description]";

            tb_quickSearch.Clear();
            tb_quickSearch_preText = null;
            tbv_quickSearchCount.Text = "";
        }
        #endregion


        #region change scene image

        private void rect_img_PreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
        private void rect_img_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private bool isNewImg = false;
        private void rect_img_PreviewDrop(object sender, DragEventArgs e)
        {
            if (sceneMgr.selectedTreeViewNode == null)
            {
                MessageBox.Show(this,
                    (string)Application.Current.TryFindResource("lb_winLauncher_msgBox_selectAScene_content"),
                    (string)Application.Current.TryFindResource("lb_winLauncher_msgBox_selectAScene_title"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BitmapSource? bs = MadTomDev.UI.QuickGraphics.Image.FromDragDrop(e.Data);
            if (bs != null)
            {
                img.Source = bs;
            }
            else
            {
                img.Source = ImageIO.Image_Unknow;
            }
            isNewImg = true;
            btn_save.Visibility = Visibility.Visible;
        }

        private void rect_img_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            rect_img.Focus();
        }

        private void rect_img_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                img.Source = ImageIO.Image_Unknow;
                isNewImg = true;
                btn_save.Visibility = Visibility.Visible;
            }
            else if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control)
                && e.Key == Key.V)
            {
                BitmapSource? bs = UI.QuickGraphics.Image.FromClipboard();
                if (bs != null)
                {
                    img.Source = bs;
                }
                else
                {
                    img.Source = ImageIO.Image_Unknow;
                }
                isNewImg = true;
                btn_save.Visibility = Visibility.Visible;
            }
        }
        #endregion


        #region edit scene name n' description
        private DateTime tbv_name_PreviewMouseUpTime = DateTime.MinValue;
        private void tbv_name_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (core.sceneMgr.selectedTreeViewNode == null)
            {
                return;
            }
            DateTime now = DateTime.Now;
            if ((now - tbv_name_PreviewMouseUpTime).TotalMilliseconds <= core.mouseDoubleClickInterval)
            {
                tb_name.Text = tbv_name.Text;
                tb_name.Visibility = Visibility.Visible;
                tbv_name.Visibility = Visibility.Hidden;
                btn_save.Visibility = Visibility.Visible;
                tb_name.Focus();
            }
            else
            {
                tbv_name_PreviewMouseUpTime = now;
            }
        }


        private DateTime tbv_description_PreviewMouseUpTime = DateTime.MinValue;

        private void tbv_description_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (core.sceneMgr.selectedTreeViewNode == null)
            {
                return;
            }
            DateTime now = DateTime.Now;
            if ((now - tbv_description_PreviewMouseUpTime).TotalMilliseconds <= core.mouseDoubleClickInterval)
            {
                tb_description.Text = tbv_description.Text;
                tb_description.Visibility = Visibility.Visible;
                tbv_description.Visibility = Visibility.Hidden;
                btn_save.Visibility = Visibility.Visible;
                tb_description.Focus();
            }
            else
            {
                tbv_description_PreviewMouseUpTime = now;
            }
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            if (sceneMgr.selectedTreeViewNode == null)
            {
                return;
            }
            bool toSave = false;
            bool changeName = false;
            if (tb_name.Visibility == Visibility.Visible
                && tb_name.Text != tbv_name.Text)
            {
                sceneMgr.selectedTreeViewNode.Text = tb_name.Text;
                tbv_name.Text = tb_name.Text;
                changeName = true;
                toSave = true;
            }
            if (tb_description.Visibility == Visibility.Visible
                && tb_description.Text != tbv_description.Text)
            {
                sceneMgr.selectedTreeViewNode.Description = tb_description.Text;
                tbv_description.Text = tb_description.Text;
                toSave = true;
            }
            if (changeName)
            {
                sceneMgr.ChangeSceneDirName(sceneMgr.selectedTreeViewNode, sceneMgr.selectedTreeViewNode.Text);
            }
            if (toSave)
            {
                sceneMgr.UpdateSceneInfo(sceneMgr.selectedTreeViewNode);
            }
            tbv_name.Visibility = Visibility.Visible;
            tbv_description.Visibility = Visibility.Visible;
            tb_name.Visibility = Visibility.Hidden;
            tb_description.Visibility = Visibility.Hidden;

            if (isNewImg)
            {
                if (img.Source == ImageIO.Image_Unknow)
                {
                    ImageIO.SetSceneCover(null);
                }
                else
                {
                    ImageIO.SetSceneCover(img.Source);
                }
                sceneMgr.selectedTreeViewNode.Icon = (BitmapSource)img.Source;
                isNewImg = false;
            }

            btn_save.Visibility = Visibility.Hidden;
        }
        #endregion


        #region quick search
        private string? tb_quickSearch_preText = null;
        private void tb_quickSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btn_quickSearch_Click(sender, new RoutedEventArgs());
            }
        }
        SimpleStringHelper.Checker_starNQues quickSearch_checker = new SimpleStringHelper.Checker_starNQues();
        private List<VMs.TreeViewNodeModelScene> quickSearchResult = new List<VMs.TreeViewNodeModelScene>();
        private int quickSearchResult_curIndex = -1;
        private void btn_quickSearch_Click(object sender, RoutedEventArgs e)
        {
            if (tb_quickSearch_preText == tb_quickSearch.Text)
            {
                // focus next
                ++quickSearchResult_curIndex;
                if (quickSearchResult_curIndex >= quickSearchResult.Count)
                {
                    quickSearchResult_curIndex = 0;
                }

                if (quickSearchResult.Count > 0)
                {
                    SelectAndViewTreeNode(quickSearchResult[quickSearchResult_curIndex]);
                }
                else
                {
                    quickSearchResult_curIndex = -1;
                }
                tbv_quickSearchCount.Text = $"{quickSearchResult_curIndex + 1} / {quickSearchResult.Count}";
            }
            else
            {
                quickSearchResult.Clear();
                quickSearchResult_curIndex = -1;
                quickSearch_checker.ClearPatterns();
                quickSearch_checker.AddPattern($"*{tb_quickSearch.Text}*");
                foreach (VMs.TreeViewNodeModelScene treeNode in core.sceneMgr.treeViewSource)
                {
                    QuickSearchLoop(treeNode);
                }
                tb_quickSearch_preText = tb_quickSearch.Text;

                // focus first(next)
                btn_quickSearch_Click(sender, new RoutedEventArgs());
            }
        }
        private void QuickSearchLoop(VMs.TreeViewNodeModelScene treeNode)
        {
            if (quickSearch_checker.Check(treeNode.Text))
            {
                quickSearchResult.Add(treeNode);
            }
            else if (quickSearch_checker.Check(treeNode.Description))
            {
                quickSearchResult.Add(treeNode);
            }

            if (treeNode.HasLoadingLabelNode())
            {
                // load subs
                core.sceneMgr.LoadSubScenes(ref treeNode);
                treeNode.RemoveLoadingLabelNodes();
            }

            foreach (object node in treeNode.Children)
            {
                if (node is VMs.TreeViewNodeModelScene)
                {
                    QuickSearchLoop((VMs.TreeViewNodeModelScene)node);
                }
            }
        }
        private void SelectAndViewTreeNode(VMs.TreeViewNodeModelScene treeNode)
        {
            // expand to item
            List<VMs.TreeViewNodeModelScene> nodeChain = core.sceneMgr.GetSceneChain(treeNode);
            for (int i = 0, iv = nodeChain.Count; i < iv; ++i)
            {
                nodeChain[i].IsExpanded = true;
            }

            // select item
            treeNode.IsSelected = true;
            UI.VisualHelper.TreeView.BringIntoView(treeView, treeNode);
        }
        #endregion


        #region create scene, peer or leaf, delete scene
        private void btn_createPeer_Click(object sender, RoutedEventArgs e)
        {
            CreateScene(tb_quickSearch.Text, false);
        }
        private void btn_createLeaf_Click(object sender, RoutedEventArgs e)
        {
            CreateScene(tb_quickSearch.Text, true);
        }
        private void CreateScene(string name, bool isLeaf)
        {
            try
            {
                VMs.TreeViewNodeModelScene newNode;
                VMs.TreeViewNodeModelScene? parent = null;
                if (core.sceneMgr.selectedTreeViewNode != null)
                {
                    if (isLeaf)
                    {
                        parent = core.sceneMgr.selectedTreeViewNode;
                    }
                    else
                    {
                        parent = core.sceneMgr.selectedTreeViewNode.parent == null ? null : (VMs.TreeViewNodeModelScene)core.sceneMgr.selectedTreeViewNode.parent;
                    }
                }
                newNode = core.sceneMgr.CreateScene(parent, name);
                SelectAndViewTreeNode(newNode);
            }
            catch (Exception err)
            {
                MessageBox.Show(this, err.Message,
                    (string)Application.Current.TryFindResource("lb_winLauncher_msgBox_error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {
            if (core.sceneMgr.selectedTreeViewNode == null)
            {
                MessageBox.Show(this,
                    (string)Application.Current.TryFindResource("lb_winLauncher_msgBox_selectAScene_toDelete_content"),
                    (string)Application.Current.TryFindResource("lb_winLauncher_msgBox_selectAScene_toDelete_title"),
                    MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            if (MessageBox.Show(this,
                    string.Format((string)Application.Current.TryFindResource("lb_winLauncher_msgBox_deleteScene_content"),
                        core.sceneMgr.selectedTreeViewNode.Text,
                        Environment.NewLine,
                        Environment.NewLine + Environment.NewLine),
                    (string)Application.Current.TryFindResource("lb_winLauncher_msgBox_deleteScene_title"),
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No)
                == MessageBoxResult.Yes)
            {
                core.sceneMgr.DeleteScene(core.sceneMgr.selectedTreeViewNode);
            }
        }
        #endregion


        #region maintain, calculate
        private async void btn_maintain_Click(object sender, RoutedEventArgs e)
        {
            if (core.sceneMgr.selectedTreeViewNode == null)
            {
                MessageBox.Show(this,
                    (string)Application.Current.TryFindResource("lb_winLauncher_msgBox_selectAScene_toMaintain_content"),
                    (string)Application.Current.TryFindResource("lb_winLauncher_msgBox_selectAScene_toMaintain_title"),
                    MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            await core.SetCursorWait(this);
            WindowMaintain winMaintain = new WindowMaintain();
            winMaintain.Closed += (s1, e1) =>
            {
                this.Show();
            };
            await core.SetCursorArrow(this);
            this.Hide();
            winMaintain.Show();
        }

        private List<Window> allCalWindows = new List<Window>();
        public async void btn_calculateAuto_Click(object sender, RoutedEventArgs e)
        {
            if (core.sceneMgr.selectedTreeViewNode == null)
            {
                MessageBox.Show(this,
                    (string)Application.Current.TryFindResource("lb_winLauncher_msgBox_selectAScene_toCalculate_content"),
                    (string)Application.Current.TryFindResource("lb_winLauncher_msgBox_selectAScene_toCalculate_title"),
                    MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            await core.SetCursorWait(this);
            core.ReInitCalculation(core.sceneMgr.selectedTreeViewNode);
            WindowCalculatorAuto winMaintain = new WindowCalculatorAuto();
            if (allCalWindows.Count > 0)
            {
                winMaintain.WindowStartupLocation = WindowStartupLocation.Manual;
            }
            winMaintain.Closed += (s1, e1) =>
            {
                if (s1 is not WindowCalculatorAuto)
                {
                    throw new Exception("What just closed, is not a auto-calculator window!");
                }
                allCalWindows.Remove((Window)s1);
                if (allCalWindows.Count == 0)
                {
                    this.Show();
                }
            };
            allCalWindows.Add(winMaintain);


            await core.SetCursorArrow(this);
            this.Hide();

            winMaintain.Show();
        }

        private async void btn_calculateManu_Click(object sender, RoutedEventArgs e)
        {
            if (core.sceneMgr.selectedTreeViewNode == null)
            {
                MessageBox.Show(this,
                    (string)Application.Current.TryFindResource("lb_winLauncher_msgBox_selectAScene_toCalculate_content"),
                    (string)Application.Current.TryFindResource("lb_winLauncher_msgBox_selectAScene_toCalculate_title"),
                    MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            await core.SetCursorWait(this);
            core.ReInitCalculation(core.sceneMgr.selectedTreeViewNode);
            WindowCalculatorManu winMaintain = new WindowCalculatorManu();
            if (allCalWindows.Count > 0)
            {
                winMaintain.WindowStartupLocation = WindowStartupLocation.Manual;
            }
            winMaintain.Closed += (s1, e1) =>
            {
                if (s1 is not WindowCalculatorManu)
                {
                    throw new Exception("What just closed, is not a manu-calculator window!");
                }
                allCalWindows.Remove((Window)s1);
                if (allCalWindows.Count == 0)
                {
                    this.Show();
                }
            };
            allCalWindows.Add(winMaintain);

            await core.SetCursorArrow(this);
            this.Hide();

            winMaintain.Show();
        }

        #endregion

        #region change language
        private void btn_language_Click(object sender, RoutedEventArgs e)
        {
            cm_languages.IsOpen = true;
        }
        private void cm_languages_Opened(object sender, RoutedEventArgs e)
        {
            foreach (Control c in cm_languages.Items)
            {
                if (c is MenuItem)
                {
                    ((MenuItem)c).Click -= Mi_Click;
                }
            }
            cm_languages.Items.Clear();

            string[] langs = core.GetLanguageList();
            if (langs == null || langs.Length == 0)
            {
                e.Handled = true;
                cm_languages.Items.Add("[No language file.]");
                return;
            }
            MenuItem mi;
            foreach (string lang in langs)
            {
                mi = new MenuItem()
                {
                    Header = lang,
                    IsCheckable = true,
                    IsChecked = lang == core.SelectedLanguage,
                };
                mi.Click += Mi_Click;
                cm_languages.Items.Add(mi);
            }
        }

        private void Mi_Click(object sender, RoutedEventArgs e)
        {
            MenuItem sUi = (MenuItem)sender;
            // it checked/de-checked, then fired click;
            if (!sUi.IsChecked)
            {
                return;
            }

            // set new language
            core.TrySetLanguage(this, sUi.Header?.ToString());
        }
        #endregion

    }
}
