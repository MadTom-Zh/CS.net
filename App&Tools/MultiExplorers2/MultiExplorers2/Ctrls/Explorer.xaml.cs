using MadTomDev.Common;
using MadTomDev.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static MadTomDev.Common.ViewHistory;
using static MadTomDev.UI.VisualHelper;
using static System.Formats.Asn1.AsnWriter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using Binding = System.Windows.Data.Binding;
using Control = System.Windows.Controls.Control;
using Cursors = System.Windows.Input.Cursors;
using DataFormats = System.Windows.DataFormats;
using DataObject = System.Windows.DataObject;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Orientation = System.Windows.Controls.Orientation;
using ProgressBar = System.Windows.Controls.ProgressBar;
using UserControl = System.Windows.Controls.UserControl;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace MadTomDev.App.Ctrls
{
    /// <summary>
    /// Interaction logic for Explorer.xaml
    /// </summary>
    public partial class Explorer : UserControl
    {
        public Explorer()
        {
            InitializeComponent();
        }

        Core core = Core.GetInstance();
        private ObservableCollection<object> treeViewNodes = new ObservableCollection<object>();
        public VM.TreeViewModelContainer treeViewRoot_quickAccess;
        public VM.TreeViewModelContainer treeViewRoot_thisPC;
        public VM.TreeViewModelContainer treeViewRoot_network;

        private ViewHistory viewHistory = new ViewHistory(16, System.IO.Path.PathSeparator.ToString());

        public void Init()
        {
            // StyleApply();

            #region init quick access links (one time)

            treeViewRoot_quickAccess = new VM.TreeViewModelContainer(null)
            {
                Icon = core.iconS32.GetIcon(320, false),
                Text = "Quick Access",
                Children = new ObservableCollection<object>(),
            };
            string qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = core.iconS32.GetIcon(34, false),
                link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
            });
            qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            qaDirPath = System.IO.Path.GetDirectoryName(qaDirPath);
            string test1 = System.IO.Path.Combine(qaDirPath, "Download"),
                test2 = System.IO.Path.Combine(qaDirPath, "Downloads");
            if (Directory.Exists(test1))
                qaDirPath = test1;
            else if (Directory.Exists(test2))
                qaDirPath = test2;
            else
                qaDirPath = null;
            if (qaDirPath != null)
            {
                treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
                {
                    Text = System.IO.Path.GetFileName(qaDirPath),
                    Icon = core.iconS32.GetIcon(122, false),
                    link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
                });
            }
            qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = core.iconS32.GetIcon(1, false),
                link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
            });
            qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = core.iconS32.GetIcon(324, false),
                link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
            });
            qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = core.iconS32.GetIcon(116, false),
                link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
            });
            qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = core.iconS32.GetIcon(115, false),
                link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
            });
            qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.Favorites);
            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = core.iconS32.GetIcon(86, false),
                link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
            });
            qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.Templates);
            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = core.iconS32.GetIcon(114, false),
                link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
            });
            qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = core.iconS32.GetIcon(89, false),
                link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
            });
            qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = core.iconS32.GetIcon(90, false),
                link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
            });
            //treeViewRoot_quickAccess_add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            //treeViewRoot_quickAccess_add(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            //treeViewRoot_quickAccess_add(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            //treeViewRoot_quickAccess_add(Environment.GetFolderPath(Environment.SpecialFolder.Favorites));
            //treeViewRoot_quickAccess_add(Environment.GetFolderPath(Environment.SpecialFolder.History));
            //treeViewRoot_quickAccess_add(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos));
            //treeViewRoot_quickAccess_add(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
            //treeViewRoot_quickAccess_add(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
            //treeViewRoot_quickAccess_add(Environment.GetFolderPath(Environment.SpecialFolder.Recent));
            //treeViewRoot_quickAccess_add(Environment.GetFolderPath(Environment.SpecialFolder.Programs));
            //treeViewRoot_quickAccess_add(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
            //treeViewRoot_quickAccess_add(Environment.GetFolderPath(Environment.SpecialFolder.System));
            //treeViewRoot_quickAccess_add(Environment.GetFolderPath(Environment.SpecialFolder.Templates));

            #endregion

            treeViewNodes.Add(treeViewRoot_quickAccess);

            treeViewRoot_thisPC = new VM.TreeViewModelContainer(null)
            {
                Icon = core.iconS32.GetIcon(15, false),
                Text = "This PC",
                Children = new ObservableCollection<object>(),
            };
            treeViewRoot_thisPC.AddLoadingLabelNode();
            treeViewRoot_thisPC.ActionExpandedChanged = TreeNodeExpendChangedHandler;
            treeViewNodes.Add(treeViewRoot_thisPC);

            treeViewRoot_network = new VM.TreeViewModelContainer(null)
            {
                Icon = core.iconS32.GetIcon(13, false),
                Text = "Network",
                Children = new ObservableCollection<object>(),
            };
            treeViewRoot_network.AddLoadingLabelNode();
            treeViewRoot_network.ActionExpandedChanged = TreeNodeExpendChangedHandler;
            treeViewNodes.Add(treeViewRoot_network);

            treeView.ItemsSource = treeViewNodes;

            core.ReloadDirComplete += Core_ReloadDirComplete;
            core.ReloadHostsComplete += Core_ReloadHostsComplete;
            core.FileRenamed += Core_FileRenamed;
            core.FilesCopyed += Core_FilesCopyed;
            core.FilesCut += Core_FilesCut;

            dataGrid.ItemsSource = dataGridItemSource;
            core.iconFS.IconGot += IconFS_IconGot;

            navigateBar.ActionGotoURL = NavigateToFromNB;
            navigateBar.FuncGetSubList = GetURLSubList;

            viewHistory.HistoryWriten += (s1) => { ResetHistoryBtns(); };
            viewHistory.EntryGotten += (s2, e2) => { ResetHistoryBtns(); };
            void ResetHistoryBtns()
            {
                btn_back.IsEnabled = viewHistory.CanGetPrev;
                btn_fore.IsEnabled = viewHistory.CanGetNext;
                btn_dropDown.IsEnabled = viewHistory.Count > 1;
            }
        }

        private void UserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            core.currentExplorer = this;
        }






        #region top left buttons

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            if (viewHistory.CanGetPrev)
            {
                ViewHistory.Item vhItem = viewHistory.Get_Entry(-1);
                NavigateTo(vhItem.fullName);
            }
        }

        private void btn_fore_Click(object sender, RoutedEventArgs e)
        {
            if (viewHistory.CanGetNext)
            {
                ViewHistory.Item vhItem = viewHistory.Get_Entry(1);
                NavigateTo(vhItem.fullName);
            }
        }

        private void btn_dropDown_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (cMenu_historyDropDown.IsOpen)
            {
                cMenu_historyDropDown.IsOpen = false;
            }
            else
            {
                cMenu_historyDropDown.Items.Clear();

                if (viewHistory.Count > 0)
                {
                    List<ViewHistory.Item> items = viewHistory.Read_All();
                    ViewHistory.Item vhItem, vhCurent = viewHistory.Read();
                    MenuItem mItem;
                    for (int i = items.Count - 1; i >= 0; --i)
                    {
                        vhItem = items[i];
                        mItem = new MenuItem()
                        {
                            Icon = new Image() { Source = vhItem.image },
                            Header = vhItem.label,
                            Tag = vhItem,
                        };
                        if (vhItem.fullName == vhCurent.fullName)
                        {
                            mItem.FontWeight = FontWeights.Bold;
                        }
                        mItem.Click += (s1, e1) =>
                        {
                            MenuItem mi = (MenuItem)s1;
                            ViewHistory.Item vh = (ViewHistory.Item)mi.Tag;
                            isNavigatingFromHistory = true;
                            NavigateTo(vh.fullName);
                        };
                        cMenu_historyDropDown.Items.Add(mItem);
                    }
                    cMenu_historyDropDown.PlacementTarget = btn_dropDown;
                    cMenu_historyDropDown.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                    cMenu_historyDropDown.IsOpen = true;
                }
            }
        }


        private void btn_up_Click(object sender, RoutedEventArgs e)
        {
            object tnTest = treeView.SelectedItem;
            if (tnTest != null)
            {
                VM.TreeViewModelContainer tnCtn = (VM.TreeViewModelContainer)tnTest;
                if (tnCtn != null)
                {
                    ((UI.VMBase.TreeViewNodeModelBase)tnCtn.parent).IsSelected = true;
                }
            }
        }


        private void btn_style_Click(object sender, RoutedEventArgs e)
        {
            WindowExplorerBG eBG = new WindowExplorerBG();
            eBG.Init(this);
            eBG.ShowDialog();
            // close app to save to current, or update to exist layouts
        }

        #region style get set (give up)

        //private FontFamily styleFontFamily;
        //private FontWeight styleFontWeight;
        //private double styleFontSize;
        //private btush
        //private void StyleApply()
        //{
        //}
        //internal Brush GetAllBorderBrush()
        //{
        //    throw new NotImplementedException();
        //}

        //internal Brush GetFontBrush()
        //{
        //    throw new NotImplementedException();
        //}

        //internal void SetAllBorderBrush(Brush newBrush)
        //{
        //    throw new NotImplementedException();
        //}

        //internal void SetFontBrush(Brush newBrush)
        //{
        //    throw new NotImplementedException();
        //}

        //internal void SetFontSize(double value)
        //{
        //    throw new NotImplementedException();
        //}

        //internal void SetFontWeight(FontWeight fontWeight)
        //{
        //    throw new NotImplementedException();
        //}

        //internal void SetFontFamily(FontFamily fontFamily)
        //{
        //    throw new NotImplementedException();
        //}

        //internal double GetFontSize()
        //{
        //    throw new NotImplementedException();
        //}

        //internal FontWeight GetFontWeight()
        //{
        //    throw new NotImplementedException();
        //}

        //internal FontFamily GetFontFamily()
        //{
        //    throw new NotImplementedException();
        //}
        #endregion

        #endregion



        #region navigating bar

        private bool isNavigating = false;
        private bool isNavigatingFromHistory = false;
        internal void NavigateTo(string url)
        {
            // 显示忙碌光标，阻止用户额外操作（以及其他情况）

            if (string.IsNullOrWhiteSpace(url))
            {
                treeViewRoot_thisPC.IsSelected = true;
                core.ReloadDisksAsync();
                UI.VisualHelper.TreeView.BringIntoView(treeView, treeViewRoot_thisPC);
            }
            else
            {
                isNavigating = true;
                if (url == treeViewRoot_quickAccess.Text)
                {
                    treeViewRoot_quickAccess.IsSelected = true;
                    isNavigating = false;
                }
                else if (url == treeViewRoot_thisPC.Text)
                {
                    treeViewRoot_thisPC.IsSelected = true;
                    isNavigating = false;
                }
                else if (url == treeViewRoot_network.Text)
                {
                    treeViewRoot_network.IsSelected = true;
                    isNavigating = false;
                }
                else
                {
                    navigatingSubPath.Clear();
                    navigatingSubPath.AddRange(url.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries));
                    if (url.StartsWith("\\\\"))
                    {
                        navigatingParentNode = treeViewRoot_network;
                    }
                    else
                    {
                        navigatingParentNode = treeViewRoot_thisPC;
                    }

                    // 启动顶部遮罩
                    SetBusy(true, true);
                    NavigateTo_loop();
                }
            }
        }
        private void NavigateToFromNB(string nbUrl)
        {
            if (nbUrl == treeViewRoot_quickAccess.Text)
            {
                treeViewRoot_quickAccess.IsSelected = true;
                UI.VisualHelper.TreeView.BringIntoView(treeView, treeViewRoot_quickAccess);
            }
            else if (nbUrl == treeViewRoot_thisPC.Text)
            {
                treeViewRoot_thisPC.IsSelected = true;
                core.ReloadDisksAsync();
                UI.VisualHelper.TreeView.BringIntoView(treeView, treeViewRoot_thisPC);
            }
            else if (nbUrl == treeViewRoot_network.Text)
            {
                treeViewRoot_network.IsSelected = true;
                core.ReloadHostsAsync();
                UI.VisualHelper.TreeView.BringIntoView(treeView, treeViewRoot_network);
            }
            else
            {
                if (!string.IsNullOrEmpty(nbUrl) && !nbUrl.StartsWith("\\\\")
                    && nbUrl.Contains(navigateBar.PathSeparator))
                {
                    while (nbUrl.StartsWith("\\"))
                        nbUrl = nbUrl.Substring(1);
                    int psIdx = nbUrl.IndexOf(navigateBar.PathSeparator);
                    if (psIdx > 0)
                    {
                        string startStr = nbUrl.Substring(0, psIdx);

                        if (startStr == treeViewRoot_quickAccess.Text
                            || startStr == treeViewRoot_thisPC.Text
                            || startStr == treeViewRoot_network.Text)
                        {
                            nbUrl = nbUrl.Substring(psIdx + 1);
                        }
                    }
                }
                NavigateTo(nbUrl);
            }
        }
        private object navigatingParentNode;
        private List<string> navigatingSubPath = new List<string>();
        private async void NavigateTo_loop()
        {
            if (TreeViewCheckNodeHasLoadingLabel(navigatingParentNode))
            {
                await TreeViewRequestSubContentsAsync(navigatingParentNode);
                SetBusy(false, false);
            }
            else
            {
                ((UI.VMBase.TreeViewNodeModelBase)navigatingParentNode).IsExpanded = true;
                object subNode = TreeNodeFindSubNode(navigatingParentNode, navigatingSubPath[0]);

                // 检查子节点的路径是否物理存在，不存在时，在父节点中删除这个节点，并设置subNode = null
                if (subNode is VM.TreeViewModelDir)
                {
                    VM.TreeViewModelDir tnDir = (VM.TreeViewModelDir)subNode;
                    if (!Directory.Exists(tnDir.dirInfo.fullName))
                    {
                        VM.TreeViewModelPhysical tnParent = (VM.TreeViewModelPhysical)navigatingParentNode;
                        tnParent.Children.Remove(subNode);
                        IOInfoShadow io = tnParent.fullContent.Find(d => d.fullName == tnDir.dirInfo.fullName);
                        if (io != null)
                            tnParent.fullContent.Remove(io);
                        subNode = null;
                    }
                }
                else if (subNode is VM.TreeViewModelDisk)
                {
                    VM.TreeViewModelDisk tnDisk = (VM.TreeViewModelDisk)subNode;
                    if (!Directory.Exists(tnDisk.diskInfo.name))
                    {
                        subNode = null;
                        treeViewRoot_thisPC.Children.Remove(subNode);
                    }
                }
                else if (subNode is VM.TreeViewModelHost)
                {
                    VM.TreeViewModelHost tnHost = (VM.TreeViewModelHost)subNode;
                    //if (!Directory.Exists(tnDisk.diskInfo.name))
                    //{
                    //    subNode = null;
                    //    treeViewRoot_thisPC.Children.Remove(subNode);
                    //}
                }

                if (subNode == null)
                {
                    // path break
                    isNavigating = false;
                    UI.VMBase.TreeViewNodeModelBase parNodeBase
                        = (UI.VMBase.TreeViewNodeModelBase)navigatingParentNode;
                    parNodeBase.IsSelected = true;
                    UI.VisualHelper.TreeView.BringIntoView(treeView, parNodeBase);
                    NavigateBarSync();

                    MessageBox.Show(
                        core.GetLangTx("txMsg_cantFindInPathContent", navigatingSubPath[0], GetTreeNodeURL(navigatingParentNode)),
                        core.GetLangTx("txMsg_cantFindPath"), MessageBoxButton.OK, MessageBoxImage.Warning);


                    // 关闭顶部遮罩
                    SetBusy(false, false);
                    SetFocuseByLastFocused();

                    // 2022-10-12 到达后，再读取一次路径；
                    ReloadPhycalPath(GetTreeNodeURL());
                }
                else
                {
                    if (navigatingSubPath.Count == 1)
                    {
                        // found
                        isNavigating = false;
                        UI.VMBase.TreeViewNodeModelBase subNodeBase
                            = (UI.VMBase.TreeViewNodeModelBase)subNode;
                        subNodeBase.IsSelected = true;
                        TreeViewRequestSubContentsAsync(subNode);

                        DispatcherOperation dispatcherOperation
                            = Dispatcher.BeginInvoke(() =>
                        {
                            UI.VisualHelper.TreeView.BringIntoView(treeView, subNodeBase);
                        }, DispatcherPriority.Background);


                        // 当点击了连接之后，虽然转到了目标地点，但如果直接使用同步地址栏的话，会去将之前点击的连接节点同步进去                        
                        NavigateBarSync(subNodeBase);

                        // 关闭顶部遮罩
                        SetBusy(false, false);
                        SetFocuseByLastFocused();

                        // 2022-10-12 到达后，再读取一次路径；
                        ReloadPhycalPath(GetTreeNodeURL());
                    }
                    else
                    {
                        navigatingSubPath.RemoveAt(0);
                        navigatingParentNode = subNode;
                        NavigateTo_loop();
                    }
                }
            }
            void ReloadPhycalPath(string testPath)
            {
                if (string.IsNullOrWhiteSpace(testPath))
                    return;

                if (testPath.Length > 2 && (testPath[1] == ':'))
                    core.ReloadDirAsync(testPath);
                else if (testPath.StartsWith("\\\\") && Utilities.Checker.CheckIsHostRootOrSubPath(testPath))
                    core.ReloadDirAsync(testPath);
            }
        }
        public string GetTreeNodeURL(object treeNode)
        {
            if (treeNode == null)
            {
                return null;
            }
            else if (treeNode == treeViewRoot_thisPC
                || treeNode == treeViewRoot_quickAccess
                || treeNode == treeViewRoot_network)
            {
                return ((VM.TreeViewModelContainer)treeNode).Text;
            }
            else if (treeNode is VM.TreeViewModelDisk)
            {
                return ((VM.TreeViewModelDisk)treeNode).diskInfo.name;
            }
            else if (treeNode is VM.TreeViewModelDir)
            {
                return ((VM.TreeViewModelDir)treeNode).dirInfo.fullName;
            }
            else if (treeNode is VM.TreeViewModelHost)
            {
                return ((VM.TreeViewModelHost)treeNode).GetFullPath();
            }
            return null;
        }
        public string GetTreeNodeURL()
        {
            string result = null;
            Dispatcher.Invoke(() =>
            {
                result = GetTreeNodeURL(treeView.SelectedItem);
            });
            return result;
        }
        public string GetDataGridItemURL(object dgItem)
        {
            string result = null;
            Dispatcher.Invoke(() =>
            {
                if (dgItem is VM.DataGridRowModle_disk)
                {
                    VM.DataGridRowModle_disk disk = (VM.DataGridRowModle_disk)dgItem;
                    result = disk.diskInfo.name;
                }
                else if (dgItem is VM.DataGridRowModle_dirNFile)
                {
                    VM.DataGridRowModle_dirNFile dir = (VM.DataGridRowModle_dirNFile)dgItem;
                    result = dir.iois.fullName;
                }
                else if (dgItem is VM.DataGridRowModle_host)
                {
                    VM.DataGridRowModle_host host = (VM.DataGridRowModle_host)dgItem;
                    result = "\\\\" + host.name;
                }
                else if (dgItem is VM.DataGridRowModle_link)
                {
                    VM.DataGridRowModle_link link = (VM.DataGridRowModle_link)dgItem;
                    result = link.link;
                }
            });
            return result;
        }

        private List<UI.NavigateBar.NodeData> GetURLSubList(string parentPath)
        {
            List<UI.NavigateBar.NodeData> result = new List<UI.NavigateBar.NodeData>();
            if (parentPath == null)
            {
                result.Add(Navigate_GetBtnData(treeViewRoot_quickAccess));
                result.Add(Navigate_GetBtnData(treeViewRoot_thisPC));
                result.Add(Navigate_GetBtnData(treeViewRoot_network));
                return result;
            }
            object parentTN;
            if (parentPath == treeViewRoot_quickAccess.Text)
                parentTN = treeViewRoot_quickAccess;
            else if (parentPath == treeViewRoot_thisPC.Text)
                parentTN = treeViewRoot_thisPC;
            else if (parentPath == treeViewRoot_network.Text)
                parentTN = treeViewRoot_network;
            else
                parentTN = FindTreeNode(parentPath.Substring(parentPath.IndexOf("\\") + 1));

            if (parentTN == null)
                return null;

            VM.TreeViewModelContainer containerTN = (VM.TreeViewModelContainer)parentTN;
            if (containerTN != null)
            {
                UI.NavigateBar.NodeData nd;
                VM.TreeViewModelDisk tnDisk;
                foreach (object s in containerTN.Children)
                {
                    result.Add(Navigate_GetBtnData(s));
                }
            }
            return result;
        }

        private void NavigateBarSync(object treeNode, bool addHistory = true)
        {
            if (treeNode == null)
            {
                navigateBar.CutNodeAt(0);
                return;
            }

            List<object> nodeChain = UI.VisualHelper.TreeView.GetNodeChain(treeView, treeNode);
            List<UI.BtnDropDown> btnChain = navigateBar.GetBtnChain();
            object curNode;
            UI.BtnDropDown curBtn;
            for (int i = 0, iv = nodeChain.Count, j = btnChain.Count; i < iv; ++i)
            {
                if (i < j)
                {
                    curBtn = btnChain[i];
                    if (curBtn.Tag != nodeChain[i])
                    {
                        navigateBar.CutNodeAt(i);
                        navigateBar.PushNode(Navigate_GetBtnData(nodeChain[i]));
                        j = i + 1;
                    }
                }
                else
                {
                    navigateBar.PushNode(Navigate_GetBtnData(nodeChain[i]));
                }
            }
            navigateBar.Icon = navigateBar.GetLastBtnIcon();
            navigateBar.TextboxURL = GetTreeNodeURL(treeNode);
            if (addHistory)
                navigateBar.AddCurURLToHistory();
        }
        private void NavigateBarSync(bool addHistory = true)
        {
            NavigateBarSync(treeView.SelectedItem, addHistory);
        }
        private UI.NavigateBar.NodeData Navigate_GetBtnData(object treeNode)
        {
            UI.NavigateBar.NodeData nd = new UI.NavigateBar.NodeData()
            { fullPath = treeView_GetFullPath(treeNode), tag = treeNode, icon = ((VM.TreeViewModelContainer)treeNode).Icon };
            if (treeNode is VM.TreeViewModelDisk)
            {
                string driveName = ((VM.TreeViewModelDisk)treeNode).diskInfo.name;
                nd.text = driveName;
                nd.nodeText = driveName.Substring(0, 2);
            }
            else
            {
                nd.nodeText = ((VM.TreeViewModelContainer)treeNode).Text;
                nd.text = nd.nodeText;
            }
            return nd;
        }


        #endregion




        #region 处理treeview事件，选中，展开，折叠

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // 将子内容显示到datagrid上
            // 显示路径到地址栏

            Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
            {
                object selectedTN = treeView.SelectedItem;
                if (selectedTN is VM.TreeViewModelLink)
                {
                    NavigateTo(((VM.TreeViewModelLink)selectedTN).link.fullName);
                }
                else
                {
                    DataGridItemClear(true);

                    bool hasDataGridFilled = false;

                    if (selectedTN == treeViewRoot_quickAccess)
                    {
                        dataGrid_setColumns_links();
                        DataGridItemAdd_afterTreeViewRootSelect(treeViewRoot_quickAccess.Children);
                        hasDataGridFilled = true;
                    }
                    else if (selectedTN == treeViewRoot_thisPC)
                    {
                        dataGrid_setColumns_disks();
                        DataGridItemAdd_afterTreeViewRootSelect(treeViewRoot_thisPC.Children);
                        //hasDataGridFilled = !(treeViewRoot_thisPC.Children.Count == 1 && TreeViewCheckNodeHasLoadingLabel(treeViewRoot_thisPC));
                    }
                    else if (selectedTN == treeViewRoot_network)
                    {
                        dataGrid_setColumns_hosts();
                        DataGridItemAdd_afterTreeViewRootSelect(treeViewRoot_network.Children);
                        hasDataGridFilled = !(treeViewRoot_network.Children.Count == 1 && TreeViewCheckNodeHasLoadingLabel(treeViewRoot_network));
                    }
                    else if (selectedTN is VM.TreeViewModelHost)
                    {
                        dataGrid_setColumns_hostDirs();
                        VM.TreeViewModelHost hostNode = (VM.TreeViewModelHost)selectedTN;
                        DataGridItemAdd_afterTreeViewRootSelect(hostNode.Children);
                        //hasDataGridFilled = !(hostNode.Children.Count == 1 && TreeViewCheckNodeHasLoadingLabel(hostNode));
                    }
                    else if (selectedTN is VM.TreeViewModelDisk)
                    {
                        // 当磁盘路径不存在时（不大可能），则向上到此电脑；
                        VM.TreeViewModelDisk tnDisk = (VM.TreeViewModelDisk)selectedTN;
                        if (!Directory.Exists(tnDisk.diskInfo.name))
                        {
                            ShowPathNotExists(tnDisk.diskInfo.name);
                            treeViewRoot_thisPC.Children.Remove(selectedTN);
                            treeViewRoot_thisPC.IsSelected = true;
                            //dataGrid_setColumns_disks();
                            //DataGridItemAdd(treeViewRoot_thisPC.Children);
                        }
                        else
                        {
                            dataGrid_setColumns_dirsNfiles();
                            DataGridItemAdd(((VM.TreeViewModelDisk)selectedTN).fullContent, false);
                        }
                        //hasDataGridFilled = !(tnDisk.Children.Count == 1 && TreeViewCheckNodeHasLoadingLabel(tnDisk));
                    }
                    else if (selectedTN is VM.TreeViewModelDir)
                    {
                        // 当文件夹路径不存在时，删除这个节点，并向上寻找，重新选定到可用的节点；
                        VM.TreeViewModelDir tnDir = (VM.TreeViewModelDir)selectedTN;

                        if (!Directory.Exists(tnDir.dirInfo.fullName))
                        {
                            ShowPathNotExists(tnDir.dirInfo.fullName);
                            VM.TreeViewModelDir tnDirParent;
                            VM.TreeViewModelDisk tnDiskParent;
                            IOInfoShadow io;
                            while (true)
                            {
                                if (tnDir.parent != null)
                                {
                                    if (tnDir.parent is VM.TreeViewModelDir)
                                    {
                                        tnDirParent = (VM.TreeViewModelDir)tnDir.parent;
                                        tnDirParent.Children.Remove(tnDir);
                                        io = tnDirParent.fullContent.Find(d => d.fullName == tnDir.dirInfo.fullName);
                                        if (io != null)
                                            tnDirParent.fullContent.Remove(io);
                                        if (Directory.Exists(tnDirParent.dirInfo.fullName))
                                        {
                                            tnDirParent.IsSelected = true;
                                            break;
                                        }
                                        else
                                        {
                                            tnDir = tnDirParent;
                                            continue;
                                        }
                                    }
                                    else if (tnDir.parent is VM.TreeViewModelDisk)
                                    {
                                        tnDiskParent = (VM.TreeViewModelDisk)tnDir.parent;
                                        if (Directory.Exists(tnDiskParent.diskInfo.name))
                                        {
                                            tnDiskParent.IsSelected = true;
                                        }
                                        else
                                        {
                                            treeViewRoot_thisPC.Children.Remove(tnDiskParent);
                                            treeViewRoot_thisPC.IsSelected = true;
                                        }
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            dataGrid_setColumns_dirsNfiles();
                            DataGridItemAdd(((VM.TreeViewModelDir)selectedTN).fullContent, false);
                        }
                        //hasDataGridFilled = !(tnDir.Children.Count == 1 && TreeViewCheckNodeHasLoadingLabel(tnDir));
                    }
                    DataGridHideLoadingLabel();

                    bool isErrorAccess = false;
                    string msg1 = null;
                    string strMissing = null;
                    if (!hasDataGridFilled && selectedTN != treeViewRoot_quickAccess)
                    {
                        if (TreeViewCheckNodeHasLoadingLabel(selectedTN))
                        {
                            TreeViewRequestSubContentsAsync(selectedTN);
                        }
                        else if (selectedTN is VM.TreeViewModelPhysical)
                        {
                            VM.TreeViewModelPhysical tNode = (VM.TreeViewModelPhysical)selectedTN;
                            DirectoryInfo testDI = new DirectoryInfo(tNode.GetFullPath());
                            try
                            {
                                if (testDI.Exists && testDI.EnumerateFiles().Any())
                                {
                                    TreeViewRequestSubContentsAsync(selectedTN);
                                }
                            }
                            catch (Exception err)
                            {
                                isErrorAccess = true;
                                if (tNode.Children != null)
                                {
                                    tNode.Children.Clear();
                                }
                                core.LogError(err);
                                msg1 = err.Message;
                            }
                        }
                    }


                    NavigateBarSync();

                    // show states
                    if (isErrorAccess)
                    {
                        SetStates(ref msg1, ref strMissing, ref strMissing, ref strMissing, ref strMissing, ref strMissing);
                    }
                    else
                    {
                        SetStatesSelection();
                    }
                }
                core.FsWatchingRemoveUnwanted();

                object tnSelected = treeView.SelectedItem;
                if (!isNavigating && !isNavigatingFromHistory && tnSelected != null)
                {
                    if (tnSelected == treeViewRoot_quickAccess)
                        viewHistory.Write_History(treeViewRoot_quickAccess.Text, treeViewRoot_quickAccess.Icon, treeViewRoot_quickAccess, treeViewRoot_quickAccess.Text);
                    else if (tnSelected == treeViewRoot_thisPC)
                        viewHistory.Write_History(treeViewRoot_thisPC.Text, treeViewRoot_thisPC.Icon, treeViewRoot_thisPC, treeViewRoot_thisPC.Text);
                    else if (tnSelected == treeViewRoot_network)
                        viewHistory.Write_History(treeViewRoot_network.Text, treeViewRoot_network.Icon, treeViewRoot_network, treeViewRoot_network.Text);
                    else if (tnSelected is VM.TreeViewModelDisk)
                    {
                        VM.TreeViewModelDisk tnDisk = (VM.TreeViewModelDisk)tnSelected;
                        viewHistory.Write_History(tnDisk.diskInfo.name, tnDisk.Icon, tnDisk, tnDisk.Text);
                    }
                    else if (tnSelected is VM.TreeViewModelHost)
                    {
                        VM.TreeViewModelHost tnHost = (VM.TreeViewModelHost)tnSelected;
                        viewHistory.Write_History(tnHost.hostName, tnHost.Icon, tnHost, tnHost.Text);
                    }
                    else if (tnSelected is VM.TreeViewModelDir)
                    {
                        VM.TreeViewModelDir tnDir = (VM.TreeViewModelDir)tnSelected;
                        viewHistory.Write_History(tnDir.dirInfo.fullName, tnDir.Icon, tnDir, tnDir.Text);
                    }
                    isNavigatingFromHistory = false;
                }
                if (treeView.SelectedItem != null)
                {
                    VM.TreeViewModelContainer tnContainer = (VM.TreeViewModelContainer)treeView.SelectedItem;
                    if (tnContainer != null && tnContainer.parent != null)
                        btn_up.IsEnabled = true;
                    else
                        btn_up.IsEnabled = false;
                }
                else
                {
                    btn_up.IsEnabled = false;
                }
            });
            void ShowPathNotExists(string notExistsPath)
            {
                MessageBox.Show(core.GetLangTx("txMsg_pathNotExist", notExistsPath),
                    core.GetLangTx("txMsg_stop"), MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }
        private bool TreeViewCheckNodeHasLoadingLabel(object treeNode)
        {
            VM.TreeViewModelContainer testTN = (VM.TreeViewModelContainer)treeNode;
            if (testTN != null)
            {
                if (testTN.Children == null)
                    return false;
                return testTN.Children.Count == 1
                        && ((UI.VMBase.TreeViewNodeModelBase)testTN.Children[0]).Text
                            == VM.TreeViewModelPhysical.TxLoading;
            }
            return false;
        }
        private Task TreeViewRequestSubContentsAsync(object parentTreeNode)
        {
            if (parentTreeNode == treeViewRoot_thisPC)
            {
                return core.ReloadDisksAsync();
            }
            else if (parentTreeNode == treeViewRoot_network)
            {
                //return core.ReloadHostsAsync();
                return Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        //treeViewRoot_network.RemoveLoadingLabelNode();

                        //VM.TreeViewModelHost foundHostNode = null;
                        //string nameLower = navigatingSubPath[0].ToLower();
                        //foreach (VM.TreeViewModelHost hNode in treeViewRoot_network.Children)
                        //{
                        //    if (hNode.Text.ToLower() == nameLower)
                        //        foundHostNode = hNode;
                        //}
                        //if (foundHostNode == null)
                        //{
                        //    foundHostNode = new VM.TreeViewModelHost(treeViewRoot_network)
                        //    {
                        //        hostName = navigatingSubPath[0],
                        //        Icon = core.iconS32.GetIcon(17, false),
                        //        Text = navigatingSubPath[0],
                        //    };
                        //    foundHostNode.AddLoadingLabelNode();
                        //    treeViewRoot_network.Children.Add(foundHostNode);
                        //}
                        List<string> fhList = new List<string>();
                        fhList.Add(navigatingSubPath[0]);
                        core.ReloadHostsComplete_fakeCall(fhList);
                    });
                });
            }
            else if (parentTreeNode is VM.TreeViewModelHost)
            {
                return core.ReloadHostAsync(((VM.TreeViewModelHost)parentTreeNode).hostName);
            }
            else if (parentTreeNode is VM.TreeViewModelDisk)
            {
                return core.ReloadDirAsync(((VM.TreeViewModelDisk)parentTreeNode).diskInfo.name);
            }
            else //if (parentTreeNode is VM.TreeViewModelDir)
            {
                return core.ReloadDirAsync(((VM.TreeViewModelDir)parentTreeNode).dirInfo.fullName);
            }
        }

        public void TreeNodeExpendChangedHandler(object node)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
            {
                bool isExpended = ((UI.VMBase.TreeViewNodeModelBase)node).IsExpanded;
                if (isExpended)
                {
                    if (node == treeViewRoot_network)
                    {
                        core.ReloadHostsAsync();
                    }
                    else if (TreeViewCheckNodeHasLoadingLabel(node))
                    {
                        TreeViewRequestSubContentsAsync(node);
                    }
                }
                else
                {

                    //// 节点收起后，是否需要回收其中节点资源？
                    //// 仅回收当前explorer的资源，如果当前路径是收起节点的子路径，则不回收
                    //return;
                    //if (!TreeNodeCheckIsLowerNode(node, treeView.SelectedItem))
                    //{

                    // 2022-10-04 
                    // 网络节点不清理已有的主机节点；
                    if (node != treeViewRoot_quickAccess
                        && node != treeViewRoot_network)
                    {
                        // 因为被折叠的节点会被自动选中，所以无论如何，都可以清理其子节点；

                        VM.TreeViewModelContainer containerNode = (VM.TreeViewModelContainer)node;
                        containerNode.Children.Clear();
                        containerNode.AddLoadingLabelNode();
                    }
                    //}
                }
            });
        }
        private bool TreeNodeCheckIsLowerNode(object parentTN, object testLowerTN)
        {
            if (parentTN == null || testLowerTN == null || parentTN == testLowerTN)
                return false;

            VM.TreeViewModelContainer testContainer;
            object subNode = testLowerTN;
            while (true)
            {
                testContainer = (VM.TreeViewModelContainer)subNode;
                if (testContainer == null)
                    return false;
                else if (testContainer == parentTN)
                    return true;
                else
                {
                    subNode = testContainer.parent;
                }
            }
        }

        private object TreeNodeFindSubNode(object parentTN, string subNodeName)
        {
            if (parentTN == treeViewRoot_thisPC)
            {
                return GetAllTreeNodesDisk().Find(d => d.diskInfo.name.Substring(0, 1).ToLower() == subNodeName.Substring(0, 1).ToLower());
            }
            else if (parentTN == treeViewRoot_network)
            {
                return GetAllTreeNodesHost().Find(h => h.hostName.ToLower() == subNodeName.ToLower());
            }
            else if (parentTN is VM.TreeViewModelDisk
                || parentTN is VM.TreeViewModelHost
                || parentTN is VM.TreeViewModelDir)
            {
                return GetAllTreeNodesDir(parentTN).Find(d => d.dirInfo.name.ToLower() == subNodeName.ToLower());
            }
            return null;
        }

        private string treeView_GetFullPath(object treeNode)
        {
            if (treeNode is VM.TreeViewModelDisk)
            {
                VM.TreeViewModelDisk tn = (VM.TreeViewModelDisk)treeNode;
                return tn.diskInfo.name;
            }
            else if (treeNode is VM.TreeViewModelDir)
            {
                VM.TreeViewModelDir tn = (VM.TreeViewModelDir)treeNode;
                return tn.dirInfo.fullName;
            }
            else if (treeNode is VM.TreeViewModelHost)
            {
                VM.TreeViewModelHost tn = (VM.TreeViewModelHost)treeNode;
                return tn.GetFullPath();
            }
            return null;
        }


        #endregion




        #region 收到core广播数据

        private void Core_ReloadDirComplete(Core sender, bool? nullFull_trueAdd_falseRemove,
            string basePath, List<Core.ReloadItemInfo> dirs, List<Core.ReloadItemInfo> files)
        {
            Dispatcher.Invoke(async () =>
            {
                bool isLocalOrNetwork = (basePath == null || basePath[1] == ':');
                object foundNode = FindTreeNode(basePath);

                bool qol_Load_DGSelection_isLoaded = false;

                if (foundNode == null)
                {
                    // 定位受影响的 树表节点，如果没有基础路径，则定位到 pc 或 网络
                    if (string.IsNullOrEmpty(basePath))
                        foundNode = isLocalOrNetwork ? treeViewRoot_thisPC : treeViewRoot_network;
                    //else if(Data.Utilities.Checker.un)
                }
                if (foundNode != null)
                {
                    // 已经定位到 树表节点， 如果是自循环导航，则进一步导航，或定位后更新地址
                    if (nullFull_trueAdd_falseRemove == null)
                    {
                        TreeNodesSync(foundNode, dirs, files);
                        if (isNavigating && foundNode == navigatingParentNode)
                            NavigateTo_loop();
                        if (basePath == GetTreeNodeURL())
                            NavigateBarSync();
                    }
                    else if (nullFull_trueAdd_falseRemove == true)
                    {
                        TreeNodesAdd(foundNode, dirs, files);
                    }
                    else
                    {
                        TreeNodesRemove(foundNode, dirs, files);
                    }
                    TreeNodesRemoveLoading(foundNode);
                }

                if (foundNode != null && foundNode == treeView.SelectedItem)
                {
                    // 受影响 树表节点 正好是当前选中的，则将树节点的内容更新到列表
                    DataGridShowLoadingLabel();
                    if (foundNode == treeViewRoot_thisPC)
                    {
                        dataGrid_setColumns_disks();
                    }
                    //else if (foundNode == treeViewRoot_network)
                    //{
                    //    dataGrid_setColumns_hosts();
                    //}
                    else if (foundNode is VM.TreeViewModelHost)
                    {
                        dataGrid_setColumns_hostDirs();
                    }
                    else
                    {
                        dataGrid_setColumns_dirsNfiles();
                    }
                    DataGridItemSync(foundNode);

                    // 列头状态会变为已经排序，但下面内容还是没有排序的；
                    //dataGrid.Columns[0].SortMemberPath = dataGrid_colSortMemberPath_nameForSorting;
                    //dataGrid.Columns[0].SortDirection = ListSortDirection.Ascending;

                    DataGridHideLoadingLabel();

                    // show states
                    SetStatesSelection();
                }

                // 如果受影响节点的 父节点 是 当前选中节点，则需要更新列表
                object foundNodeParent = FindTreeNode(System.IO.Path.GetDirectoryName(basePath));
                if (foundNodeParent != null && foundNodeParent == treeView.SelectedItem)
                {
                    qol_Load_DGSelection();
                    qol_Load_DGSelection_isLoaded = true;

                    VM.TreeViewModelPhysical tnPhy = (VM.TreeViewModelPhysical)foundNodeParent;
                    if (nullFull_trueAdd_falseRemove == true)
                    {
                        // 基准路径目标被添加
                        IOInfoShadow newIO = null;
                        try
                        {
                            newIO = new IOInfoShadow(basePath);
                        }
                        catch (Exception err)
                        {
                            core.LogError(err);
                            core.ReloadDirComplete_removeIOs_fakeCall(basePath);
                        }

                        if (newIO != null)
                        {

                            #region 添加树节点项目
                            if (tnPhy.fullContent != null)
                            {
                                IOInfoShadow foundIO = tnPhy.fullContent.Find(io => io.fullName == newIO.fullName);
                                if (foundIO == null)
                                    tnPhy.fullContent.Add(newIO);
                            }
                            bool isNotExists = true;
                            if (!newIO.wasFile)
                            {
                                VM.TreeViewModelDir tnDir = null;
                                if (tnPhy.Children != null)
                                {
                                    foreach (object o in tnPhy.Children)
                                    {
                                        if (o is VM.TreeViewModelDir)
                                        {
                                            tnDir = (VM.TreeViewModelDir)o;
                                            if (tnDir.dirInfo.fullName == newIO.fullName)
                                            {
                                                isNotExists = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (isNotExists)
                                {
                                    // add
                                    if (tnPhy.Children == null)
                                        tnPhy.Children = new ObservableCollection<object>();
                                    tnPhy.Children.Add(new VM.TreeViewModelDir(tnPhy, newIO));
                                }
                                else
                                {
                                    // update
                                    tnDir.UpdateFrom(newIO);
                                }
                            }
                            #endregion

                            #region 添加列表项目
                            VM.DataGridRowModle_dirNFile curDGItem = null;
                            isNotExists = true;
                            foreach (object o in dataGridItemSource)
                            {
                                if (o is VM.DataGridRowModle_dirNFile)
                                {
                                    curDGItem = (VM.DataGridRowModle_dirNFile)o;
                                    if (curDGItem.iois.fullName == basePath)
                                    {
                                        isNotExists = false;
                                        break;
                                    }
                                }
                            }
                            List<object> animaList = new List<object>();
                            if (isNotExists)
                            {
                                // add
                                VM.DataGridRowModle_dirNFile newDGItem = new VM.DataGridRowModle_dirNFile(newIO);
                                dataGridItemSource.Add(newDGItem);
                                animaList.Add(newDGItem);
                                // 如果是使用新建功能增加的项目，则开始重命名这个项目；
                                if (!TryRenameNewFile())
                                {
                                    // select files
                                    dataGrid.SelectedItem = newDGItem;
                                }
                            }
                            else
                            {
                                // update
                                curDGItem.UpdateFrom(newIO);
                                animaList.Add(curDGItem);
                            }
                            await dataGrid_delayRefreshAsync();

                            // 粘贴文件后，一批文件中是一个一个创建的；
                            TrySelectCopiedCutFiles(basePath);

                            // 动画
                            if (isNotExists)
                                dataGrid_doAnimateAsync(
                                    dataGrid_doAnimate_getVisiblePosis(animaList),
                                    dataGrid_doAnimate_colorAdded);
                            else
                                dataGrid_doAnimateAsync(
                                    dataGrid_doAnimate_getVisiblePosis(animaList),
                                    dataGrid_doAnimate_colorUpdated);
                            #endregion

                        }
                    }
                    else if (nullFull_trueAdd_falseRemove == false)
                    {
                        // 基准路径目标被移除
                        // 清理树节点
                        IOInfoShadow curIO;
                        for (int i = 0, iv = tnPhy.fullContent.Count; i < iv; ++i)
                        {
                            curIO = tnPhy.fullContent[i];
                            if (curIO.fullName == basePath)
                            {
                                tnPhy.fullContent.Remove(curIO);
                                break;
                            }
                        }
                        object curItem;
                        VM.TreeViewModelDir curItemDir;
                        if (tnPhy.Children != null)
                        {
                            for (int i = 0, iv = tnPhy.Children.Count; i < iv; ++i)
                            {
                                curItem = tnPhy.Children[i];
                                if (curItem is VM.TreeViewModelDir)
                                {
                                    curItemDir = (VM.TreeViewModelDir)curItem;
                                    if (curItemDir.dirInfo.fullName == basePath)
                                    {
                                        tnPhy.Children.Remove(curItem);
                                        break;
                                    }
                                }
                            }
                        }

                        // 清理列表中的项目
                        VM.DataGridRowModle_dirNFile curDGItemDir;
                        for (int i = 0, iv = dataGridItemSource.Count; i < iv; ++i)
                        {
                            curItem = dataGridItemSource[i];
                            if (curItem is VM.DataGridRowModle_dirNFile)
                            {
                                curDGItemDir = (VM.DataGridRowModle_dirNFile)curItem;
                                if (curDGItemDir.iois.fullName == basePath)
                                {
                                    dataGridItemSource.Remove(curItem);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // not possible 因为找不到基准节点，所以无法将附加内容添加进去
                    }
                }
                if (qol_Load_DGSelection_isLoaded)
                {
                    qol_Set_DGSelection();
                }
                SetFocuseByLastFocused();
            });
        }
        private void Core_FileRenamed(Core sender, string oldFullName, string newName)
        {
            Dispatcher.Invoke(() =>
            {
                bool isLocalOrNetwork = (oldFullName == null || oldFullName[1] == ':');
                string newFullName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(oldFullName), newName);
                string newExt = System.IO.Path.GetExtension(newFullName);
                VM.DataGridRowModle_dirNFile dgItem;

                // 重命名的是文件夹，当这个文件夹出现在树表中时，
                // 更新这个树节点的名称和路径和子节点的名称和路径；
                // 如果当前选择路径中有这个节点，则更新导航栏对应信息；
                // 当被修改节点的父节点是该节点时，更新列表中被改名项目的名称和路径；
                object renamedNode = FindTreeNode(oldFullName);
                object renamedNodeParent = FindTreeNode(System.IO.Path.GetDirectoryName(oldFullName));
                if (renamedNode != null)
                {
                    TreeNodesRename(renamedNode, newName);

                    List<object> curPathNodeChain = UI.VisualHelper.TreeView.GetNodeChain(treeView, treeView.SelectedItem);
                    string oldFullNameWithSep = oldFullName + System.IO.Path.DirectorySeparatorChar;
                    string newFullNameWithSep = newFullName + System.IO.Path.DirectorySeparatorChar;
                    if (curPathNodeChain.Contains(renamedNode))
                    {
                        // update url
                        navigateBar.SetFileRename(oldFullName, newName);

                        // update path, in list view?
                        foreach (object dgO in dataGridItemSource)
                        {
                            if (dgO is VM.DataGridRowModle_dirNFile)
                            {
                                dgItem = (VM.DataGridRowModle_dirNFile)dgO;
                                if (dgItem.iois.fullName.StartsWith(oldFullNameWithSep))
                                {
                                    dgItem.iois.fullName = dgItem.iois.fullName.Replace(oldFullNameWithSep, newFullNameWithSep);
                                    //dgItem.iois.name = System.IO.Path.GetFileName(dgItem.iois.fullName);
                                    dgItem.iois.directoryName = System.IO.Path.GetDirectoryName(dgItem.iois.fullName);
                                    //dgItem.iois.extension = System.IO.Path.GetExtension(dgItem.iois.fullName);
                                }
                                //else if (dgItem.iois.fullName == oldFullName)
                                //{
                                //    dgItem.iois.fullName = newFullName;
                                //    dgItem.iois.name = newName;
                                //    dgItem.iois.extension = System.IO.Path.GetExtension(newFullName);
                                //}
                            }
                        }
                    }
                    else
                    {
                        HandleRenameInDataGrid();
                    }
                }

                // 重命名的是文件，检查这个节点的父节点是否在树表中出现；
                // 如果出现在树表，则更新其中这个文件的名称和路径；
                // 如果这个父节点是当前选中节点，则更新列表中对应文件；

                if (renamedNodeParent != null)
                {
                    IOInfoShadow fileIO;
                    if (renamedNodeParent is VM.TreeViewModelDisk)
                    {
                        VM.TreeViewModelDisk parentDisk = (VM.TreeViewModelDisk)renamedNodeParent;
                        HandleOldFile(parentDisk.fullContent);
                    }
                    else if (renamedNodeParent is VM.TreeViewModelDir)
                    {
                        VM.TreeViewModelDir parentDir = (VM.TreeViewModelDir)renamedNodeParent;
                        HandleOldFile(parentDir.fullContent);
                    }
                    void HandleOldFile(List<IOInfoShadow> fullContent)
                    {
                        for (int i = 0, iv = fullContent.Count; i < iv; ++i)
                        {
                            fileIO = fullContent[i];
                            if (fileIO.fullName == oldFullName)
                            {
                                fileIO.fullName = newFullName;
                                fileIO.name = newName;
                                fileIO.extension = newExt;
                                break;
                            }
                        }
                    }
                    HandleRenameInDataGrid();
                }
                void HandleRenameInDataGrid()
                {
                    UI.VMBase.TreeViewNodeModelBase renamedNodeBase = (UI.VMBase.TreeViewNodeModelBase)renamedNode;
                    List<object> animaListUpdate = new List<object>();
                    if (renamedNodeParent == treeView.SelectedItem
                        || (renamedNodeBase != null && renamedNodeBase.parent == treeView.SelectedItem))
                    {
                        // update list view(item)
                        foreach (object dgO in dataGridItemSource)
                        {
                            if (dgO is VM.DataGridRowModle_dirNFile)
                            {
                                dgItem = (VM.DataGridRowModle_dirNFile)dgO;
                                //因为ioinfo和treeviewNode的完整列表就是同一个对象，所以iois已经变更过了；
                                if (dgItem.iois.fullName == newFullName)
                                {
                                    dgItem.name = dgItem.iois.name;
                                    dgItem.fileType = dgItem.iois.extension;
                                    //dgItem.iois.fullName = newFullName;
                                    //dgItem.iois.name = newName;
                                    //dgItem.iois.extension = newExt;
                                    if (dgItem.iois.wasFile
                                        && dgItem.iois.extension != System.IO.Path.GetExtension(oldFullName))
                                    {
                                        dgItem.icon = core.iconFS.GetIcon(newFullName, true, false);
                                    }
                                    // 记录名称，用于色块动画
                                    animaListUpdate.Add(dgItem);
                                    break;
                                }
                            }
                        }
                    }
                    dataGrid_delayRefreshAsync();

                    // 动画
                    dataGrid_doAnimateAsync(
                        dataGrid_doAnimate_getVisiblePosis(animaListUpdate),
                        dataGrid_doAnimate_colorUpdated);
                }
                SetFocuseByLastFocused();
            });
        }





        private DateTime dataGrid_delayRefresh_exeTime = DateTime.MinValue;
        private bool dataGrid_delayRefresh_isExecuting = false;
        private Task dataGrid_delayRefreshAsync()
        {
            DateTime now = DateTime.Now;
            dataGrid_delayRefresh_exeTime = now.AddMilliseconds(350);
            if (dataGrid_delayRefresh_isExecuting)
            {
                return Task.Run(() =>
                {
                    while (dataGrid_delayRefresh_isExecuting)
                    {
                        Task.Delay(10).Wait();
                    }
                });
            }
            else
            {
                dataGrid_delayRefresh_isExecuting = true;
                return Task.Run(() =>
                {
                    while ((dataGrid_delayRefresh_exeTime - DateTime.Now).TotalMilliseconds > 0)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            dataGrid.Items.Refresh();
                        });
                        Task.Delay(300).Wait();
                    }
                    dataGrid_delayRefresh_isExecuting = false;
                });
            }
        }


        private void Core_ReloadHostsComplete(Core sender, List<string> hostList)
        {
            Dispatcher.Invoke(() =>
            {
                TreeNodesRemoveLoading(treeViewRoot_network);
                List<object> hosts = new List<object>();
                VM.TreeViewModelHost newHostNode;
                foreach (string h in hostList)
                {
                    newHostNode = new VM.TreeViewModelHost(treeViewRoot_network)
                    {
                        hostName = h,
                        Text = h,
                        Icon = core.iconS32.GetIcon(17, false),
                    };
                    newHostNode.AddLoadingLabelNode();
                    newHostNode.ActionExpandedChanged = TreeNodeExpendChangedHandler;

                    hosts.Add(newHostNode);
                }
                TreeNodesSync(treeViewRoot_network, hosts);
                if (isNavigating && navigatingSubPath.Count > 0)
                {
                    string curHostName = navigatingSubPath[0];
                    VM.TreeViewModelHost foundHost = (VM.TreeViewModelHost)(hosts.Find(h => ((VM.TreeViewModelHost)h).Text == curHostName));
                    if (foundHost != null)
                    {
                        //navigatingSubPath.RemoveAt(0);
                        //navigatingParentNode = foundHost;
                        NavigateTo_loop();
                    }
                }


                if (treeView.SelectedItem == treeViewRoot_network)
                {
                    DataGridShowLoadingLabel();
                    dataGrid_setColumns_hosts();
                    DataGridItemSync(treeViewRoot_network);
                    DataGridHideLoadingLabel();

                    // show states
                    SetStatesSelection();
                }

                SetFocuseByLastFocused();
            });
        }

        private Dictionary<string, BitmapSource> IconFS_IconGot_buffer = new Dictionary<string, BitmapSource>();
        private bool IconFS_IconGot_isBusy = false;
        private async void IconFS_IconGot(IconHelper.FileSystem sender, string path, string ext, bool smallIcon, bool isDirectory, BitmapSource icon)
        {
            lock (IconFS_IconGot_buffer)
            {
                if (!IconFS_IconGot_buffer.ContainsKey(path))
                {
                    IconFS_IconGot_buffer.Add(path, icon);
                }
            }
            await Task.Delay(100);
            if (IconFS_IconGot_isBusy)
                return;

            IconFS_IconGot_isBusy = true;
            await Task.Run(() =>
            {
                Dictionary<string, BitmapSource> cache = new Dictionary<string, BitmapSource>();
                lock (IconFS_IconGot_buffer)
                {
                    foreach (string key in IconFS_IconGot_buffer.Keys)
                    {
                        cache.Add(key, IconFS_IconGot_buffer[key]);
                    }
                    IconFS_IconGot_buffer.Clear();
                }
                Dispatcher.Invoke(() =>
                {
                    if (dataGridItemSource.Count <= 0)
                        return;

                    object test = dataGridItemSource[0];
                    if (test is VM.DataGridRowModle_dirNFile)
                    {
                        List<VM.DataGridRowModle_dirNFile> dgDFList = GetAllDataGridDirNFile();
                        VM.DataGridRowModle_dirNFile foundDGItem;
                        foreach (string key in cache.Keys)
                        {
                            foundDGItem = dgDFList.Find(df => df.iois.fullName == key);
                            if (foundDGItem != null)
                            {
                                foundDGItem.icon = cache[key];
                                //await Task.Delay(1);
                            }
                        }
                        dataGrid_delayRefreshAsync();
                    }
                });
            });
            IconFS_IconGot_isBusy = false;
        }




        #endregion


        #region 操纵treeview同步、增加、删除节点

        private object FindTreeNode(string fullPath)
        {
            if (fullPath == null)
                return null;
            if (fullPath[1] == ':')
            {
                return FindTreeNode(treeViewRoot_thisPC, fullPath);
            }
            else
            {
                //network
                return FindTreeNode(treeViewRoot_network, fullPath);
            }
        }
        private object FindTreeNode(object parentNode, string relaPath)
        {
            if (TreeNodeHaveLoading(parentNode))
                return null;

            while (relaPath.StartsWith("\\"))
                relaPath = relaPath.Substring(1);
            while (relaPath.EndsWith("\\"))
                relaPath = relaPath.Substring(0, relaPath.Length - 1);

            int idxS = relaPath.IndexOf("\\");
            string curName;
            if (idxS > 0)
            {
                curName = relaPath.Substring(0, idxS);
            }
            else
            {
                curName = relaPath;
            }

            if (parentNode is VM.TreeViewModelDir)
            {
                VM.TreeViewModelDir parentNodeDir = (VM.TreeViewModelDir)parentNode;
                if (parentNodeDir.Children != null)
                {
                    foreach (object child in parentNodeDir.Children)
                    {
                        if (((UI.VMBase.TreeViewNodeModelBase)child).Text == curName)
                        {
                            if (idxS < 0)
                                return child;
                            else
                                return FindTreeNode(child, relaPath.Substring(idxS + 1));
                        }
                    }
                }
            }
            else if (parentNode is VM.TreeViewModelDisk)
            {
                VM.TreeViewModelDisk parentNodeDisk = (VM.TreeViewModelDisk)parentNode;
                if (parentNodeDisk.Children != null)
                {
                    foreach (object child in parentNodeDisk.Children)
                    {
                        if (((UI.VMBase.TreeViewNodeModelBase)child).Text == curName)
                        {
                            if (idxS < 0)
                                return child;
                            else
                                return FindTreeNode(child, relaPath.Substring(idxS + 1));
                        }
                    }
                }
            }
            else if (parentNode is VM.TreeViewModelHost)
            {
                VM.TreeViewModelHost parentNodeHost = (VM.TreeViewModelHost)parentNode;
                if (parentNodeHost.Children != null)
                {
                    foreach (object child in parentNodeHost.Children)
                    {
                        if (((UI.VMBase.TreeViewNodeModelBase)child).Text == curName)
                        {
                            if (idxS < 0)
                                return child;
                            else
                                return FindTreeNode(child, relaPath.Substring(idxS + 1));
                        }
                    }
                }
            }
            else if (parentNode == treeViewRoot_thisPC)
            {
                if (treeViewRoot_thisPC.Children != null)
                {
                    foreach (VM.TreeViewModelDisk disk in treeViewRoot_thisPC.Children)
                    {
                        if (disk.diskInfo.name.Substring(0, 2) == curName)
                        {
                            if (idxS < 0)
                                return disk;
                            else
                                return FindTreeNode(disk, relaPath.Substring(idxS + 1));
                        }
                    }
                }
            }
            else if (parentNode == treeViewRoot_network)
            {
                if (treeViewRoot_network.Children != null)
                {
                    foreach (VM.TreeViewModelHost host in treeViewRoot_network.Children)
                    {
                        if (host.hostName == curName)
                        {
                            if (idxS < 0)
                                return host;
                            else
                                return FindTreeNode(host, relaPath.Substring(idxS + 1));
                        }
                    }
                }
            }
            return null;
        }


        private List<VM.TreeViewModelLink> GetAllTreeNodesLink()
        {
            List<VM.TreeViewModelLink> result = new List<VM.TreeViewModelLink>();
            foreach (VM.TreeViewModelLink o in treeViewRoot_quickAccess.Children)
            {
                //if (o is VM.TreeViewModelLink)
                result.Add(o);
            }
            return result;
        }
        private List<VM.TreeViewModelDisk> GetAllTreeNodesDisk()
        {
            List<VM.TreeViewModelDisk> result = new List<VM.TreeViewModelDisk>();
            foreach (object o in treeViewRoot_thisPC.Children)
            {
                if (o is VM.TreeViewModelDisk)
                    result.Add((VM.TreeViewModelDisk)o);
            }
            return result;
        }
        private List<VM.TreeViewModelDisk> GetAllTreeNodesDisk(IEnumerable<object> oriList)
        {
            List<VM.TreeViewModelDisk> result = new List<VM.TreeViewModelDisk>();
            foreach (object o in oriList)
            {
                if (o is VM.TreeViewModelDisk)
                    result.Add((VM.TreeViewModelDisk)o);
            }
            return result;
        }
        private List<VM.TreeViewModelHost> GetAllTreeNodesHost()
        {
            List<VM.TreeViewModelHost> result = new List<VM.TreeViewModelHost>();
            foreach (object o in treeViewRoot_network.Children)
            {
                if (o is VM.TreeViewModelHost)
                    result.Add((VM.TreeViewModelHost)o);
            }
            return result;
        }
        private List<VM.TreeViewModelHost> GetAllTreeNodesHost(IEnumerable<object> oriList)
        {
            List<VM.TreeViewModelHost> result = new List<VM.TreeViewModelHost>();
            if (oriList != null)
            {
                foreach (object o in oriList)
                    result.Add((VM.TreeViewModelHost)o);
            }
            return result;
        }
        private List<VM.TreeViewModelDir> GetAllTreeNodesDir(object parentTreeNode)
        {
            List<VM.TreeViewModelDir> result = new List<VM.TreeViewModelDir>();
            VM.TreeViewModelContainer tnContainer = (VM.TreeViewModelContainer)parentTreeNode;
            if (tnContainer.Children != null)
            {
                foreach (object o in tnContainer.Children)
                {
                    if (o is VM.TreeViewModelDir)
                        result.Add((VM.TreeViewModelDir)o);
                }
            }
            return result;
        }
        private List<VM.TreeViewModelDir> GetAllTreeNodesDir(IEnumerable<object> oriList)
        {
            List<VM.TreeViewModelDir> result = new List<VM.TreeViewModelDir>();
            if (oriList != null)
            {
                foreach (object o in oriList)
                {
                    if (o is VM.TreeViewModelDir)
                        result.Add((VM.TreeViewModelDir)o);
                }
            }
            return result;
        }

        private bool TreeNodeHaveLoading(object parentNode)
        {
            return ((VM.TreeViewModelContainer)parentNode).HaveLoadingLabelNode();
        }
        private void TreeNodesRemoveLoading(object parentNode)
        {
            ((VM.TreeViewModelContainer)parentNode).RemoveLoadingLabelNode();
        }
        /// <summary>
        /// 不可转换文件清单；
        /// 将core广播来的列表，转化为treeview直接可用的节点列表
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="infoList"></param>
        /// <returns>按父节点类型，返回
        /// TreeViewModelDisk列表，
        /// TreeViewModelHost列表，
        /// TreeViewModelDir列表；
        /// 如果是文件列表，则返回IOInfoShadow列表</returns>
        private List<object> GetNodes(object parentNode, List<Core.ReloadItemInfo> infoList)
        {
            List<object> result = new List<object>();
            if (parentNode == treeViewRoot_thisPC)
            {
                // disks
                VM.TreeViewModelDisk vm;
                foreach (Core.ReloadItemInfo rld in infoList)
                {
                    vm = new VM.TreeViewModelDisk(parentNode)
                    {
                        diskInfo = new DriveInfoShadow(new DriveInfo(rld.Name)),
                        Text = rld.Name,
                    };
                    if (rld.Err != null)
                    {
                        vm.Icon = core.iconS32.GetIcon(53, false);
                        vm.error = rld.Err;
                    }
                    else
                    {
                        vm.Icon = core.iconFS.GetIcon(rld.Name, true, true);
                    }
                    if (rld.hasSubDir)
                    {
                        vm.AddLoadingLabelNode();
                    }
                    result.Add(vm);
                }
            }
            else if (parentNode == treeViewRoot_network)
            {
                // hosts
                VM.TreeViewModelHost vm;
                foreach (Core.ReloadItemInfo rld in infoList)
                {
                    vm = new VM.TreeViewModelHost(parentNode)
                    {
                        hostName = rld.Name,
                        Icon = core.iconS32.GetIcon(17, false),
                        Text = rld.Name,
                    };
                    vm.AddLoadingLabelNode();
                    result.Add(vm);
                }
            }
            else if (parentNode is VM.TreeViewModelHost)
            {
                // net-dir
                VM.TreeViewModelDir vm;
                foreach (Core.ReloadItemInfo rld in infoList)
                {
                    result.Add(new VM.TreeViewModelDir(parentNode, rld));
                }
            }
            else
            {
                // just dir or files
                VM.TreeViewModelDir vm;
                if (infoList != null)
                {
                    foreach (Core.ReloadItemInfo rld in infoList)
                    {
                        result.Add(new VM.TreeViewModelDir(parentNode, rld));
                    }
                }
            }
            return result;
        }
        private List<IOInfoShadow> GetFiles(List<Core.ReloadItemInfo> infoList)
        {
            if (infoList == null)
                return null;
            List<IOInfoShadow> result = new List<IOInfoShadow>();
            foreach (Core.ReloadItemInfo rii in infoList)
                result.Add(rii.ioInfo);
            return result;
        }

        /// <summary>
        /// 同步（树）节点，和输入清单对比，如果原树表存在同名，则更新，如原树不存在而输入存在，则新增；如果原树存在但输入不存在，则删除；
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="newDirNodes"></param>
        /// <param name="newFileNodes"></param>
        private void TreeNodesSync(object parentNode, List<object> newDirNodes, List<IOInfoShadow> newFileNodes = null)
        {
            if (parentNode == treeViewRoot_thisPC)
            {
                List<VM.TreeViewModelDisk> curDiskList = GetAllTreeNodesDisk();
                List<VM.TreeViewModelDisk> newDiskList = GetAllTreeNodesDisk(newDirNodes);
                VM.TreeViewModelDisk foundDisk;
                foreach (VM.TreeViewModelDisk newDisk in newDiskList)
                {
                    foundDisk = curDiskList.Find(d => d.diskInfo.volumeLabel == newDisk.diskInfo.volumeLabel);
                    if (foundDisk == null)
                    {
                        treeViewRoot_thisPC.Children.Add(newDisk);
                        curDiskList.Add(newDisk);
                        newDisk.ActionExpandedChanged = TreeNodeExpendChangedHandler;
                    }
                    else
                    {
                        foundDisk.UpdateFrom(newDisk);
                    }
                }
                foreach (VM.TreeViewModelDisk lostDisk in curDiskList)
                {
                    foundDisk = newDiskList.Find(d => d.diskInfo.volumeLabel == lostDisk.diskInfo.volumeLabel);
                    if (foundDisk == null)
                    {
                        treeViewRoot_thisPC.Children.Remove(lostDisk);
                    }
                }
            }
            else if (parentNode == treeViewRoot_network)
            {
                List<VM.TreeViewModelHost> curHostList = GetAllTreeNodesHost();
                List<VM.TreeViewModelHost> newHostList = GetAllTreeNodesHost(newDirNodes);
                VM.TreeViewModelHost foundHost;
                foreach (VM.TreeViewModelHost newHost in newHostList)
                {
                    foundHost = curHostList.Find(h => h.hostName.ToLower() == newHost.hostName.ToLower());
                    if (foundHost == null)
                    {
                        treeViewRoot_network.Children.Add(newHost);
                        curHostList.Add(newHost);
                    }
                    else
                    {
                        foundHost.UpdateFrom(newHost);
                    }
                }

                // 2022-10-04
                // 因为获取所有主机的功能有时候会直接跳出，所以这里不清理已经存在的主机

                //foreach (VM.TreeViewModelHost lostHost in curHostList)
                //{
                //    foundHost = newHostList.Find(h => h.hostName == lostHost.hostName);
                //    if (foundHost == null)
                //    {
                //        treeViewRoot_network.Children.Remove(lostHost);
                //    }
                //}
            }
            else if (parentNode is VM.TreeViewModelDisk)
            {
                SyncDirsNFiles(parentNode, GetAllTreeNodesDir(newDirNodes), newFileNodes);
            }
            else if (parentNode is VM.TreeViewModelHost)
            {
                SyncDirsNFiles(parentNode, GetAllTreeNodesDir(newDirNodes), newFileNodes);
            }
            else if (parentNode is VM.TreeViewModelDir)
            {
                SyncDirsNFiles(parentNode, GetAllTreeNodesDir(newDirNodes), newFileNodes);
            }
            void SyncDirsNFiles(object parentNode, List<VM.TreeViewModelDir> newDirList, List<IOInfoShadow> fileList)
            {
                VM.TreeViewModelPhysical parentNodeDir = (VM.TreeViewModelPhysical)parentNode;
                List<VM.TreeViewModelDir> curDirtList = GetAllTreeNodesDir(parentNode);
                VM.TreeViewModelDir foundDir;
                foreach (VM.TreeViewModelDir newDir in newDirList)
                {
                    if (newDir.dirInfo.wasExists)
                    {
                        foundDir = curDirtList.Find(d => d.dirInfo.name == newDir.dirInfo.name);
                        if (foundDir == null)
                        {
                            if (parentNodeDir.Children == null)
                                parentNodeDir.Children = new ObservableCollection<object>();
                            parentNodeDir.Children.Add(newDir);
                            curDirtList.Add(newDir);
                            newDir.ActionExpandedChanged = TreeNodeExpendChangedHandler;
                        }
                        else
                        {
                            foundDir.UpdateFrom(newDir);
                        }
                    }
                }
                foreach (VM.TreeViewModelDir lostDir in curDirtList)
                {
                    foundDir = newDirList.Find(d => d.dirInfo.name == lostDir.dirInfo.name);
                    if (foundDir == null)
                    {
                        parentNodeDir.Children.Remove(lostDir);
                    }
                }
                List<IOInfoShadow> newFullContent = new List<IOInfoShadow>();
                newFullContent.AddRange(newDirList.Select(d => d.dirInfo));
                if (fileList != null)
                    newFullContent.AddRange(fileList);
                IOInfoShadow foundIO;
                foreach (IOInfoShadow newIO in newFullContent)
                {
                    foundIO = parentNodeDir.fullContent.Find(io => io.name.ToLower() == newIO.name.ToLower());
                    if (foundIO == null)
                    {
                        parentNodeDir.fullContent.Add(newIO);
                    }
                    else
                    {
                        foundIO.UpdateFrom(newIO);
                    }
                }
                IOInfoShadow curIO;
                for (int i = 0, iv = parentNodeDir.fullContent.Count; i < iv; i++)
                {
                    curIO = parentNodeDir.fullContent[i];
                    foundIO = newFullContent.Find(io => io.name.ToLower() == curIO.name.ToLower());
                    if (foundIO == null)
                    {
                        parentNodeDir.fullContent.Remove(curIO);
                        i--; iv--;
                    }
                }
            }
        }
        private void TreeNodesSync(object parentNode, List<Core.ReloadItemInfo> dirInfoList, List<Core.ReloadItemInfo> fileInfoList)
        {
            TreeNodesSync(parentNode, GetNodes(parentNode, dirInfoList), GetFiles(fileInfoList));
        }

        private void TreeNodesRename(object renamedNode, string newName)
        {
            if (renamedNode is VM.TreeViewModelDir)
            {
                VM.TreeViewModelDir tvDir = (VM.TreeViewModelDir)renamedNode;
                string oldDirName = tvDir.dirInfo.fullName;

                // rename current node
                tvDir.dirInfo.name = newName;
                string newDirName = System.IO.Path.Combine(tvDir.dirInfo.directoryName, newName);
                tvDir.dirInfo.fullName = newDirName;
                tvDir.dirInfo.extension = System.IO.Path.GetExtension(tvDir.dirInfo.fullName);
                tvDir.Text = newName;

                // update all sub-contents
                IOInfoShadow subIo;
                for (int i = 0, iv = tvDir.fullContent.Count; i < iv; ++i)
                {
                    subIo = tvDir.fullContent[i];
                    subIo.name = newName;
                    subIo.fullName = System.IO.Path.Combine(subIo.directoryName, newName);
                    subIo.extension = System.IO.Path.GetExtension(subIo.fullName);
                }

                // if sub nodes loaded, update them too
                if (tvDir.Children != null)
                {
                    if (tvDir.Children.Count == 1 && tvDir.Children[0] is UI.VMBase.TreeViewNodeModelBase)
                    {
                        // loading
                    }
                    else
                    {
                        foreach (VM.TreeViewModelDir subDir in tvDir.Children)
                        {
                            RenameDirLoop(subDir, oldDirName, newDirName);
                        }
                    }
                }
            }
            else
            {
                ;
            }

            void RenameDirLoop(VM.TreeViewModelDir subTN, string oldDirName, string newDirName)
            {
                subTN.dirInfo.fullName = subTN.dirInfo.fullName.Replace(oldDirName, newDirName);
                foreach (VM.TreeViewModelDir iSubDir in subTN.Children)
                {
                    RenameDirLoop(iSubDir, oldDirName, newDirName);
                }
            }
        }

        /// <summary>
        /// 删除（树）节点，如发现同名，则删除
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="dirInfoList"></param>
        /// <param name="fileInfoList"></param>
        private void TreeNodesRemove(object parentNode, List<Core.ReloadItemInfo> dirInfoList, List<Core.ReloadItemInfo> fileInfoList)
        {
            Core.ReloadItemInfo foundInfo;
            if (parentNode == treeViewRoot_thisPC)
            {
                List<VM.TreeViewModelDisk> curDiskList = GetAllTreeNodesDisk();
                VM.TreeViewModelDisk foundDisk;
                foreach (Core.ReloadItemInfo lostDisk in dirInfoList)
                {
                    foundDisk = curDiskList.Find(d => d.diskInfo.name[0] == lostDisk.Name[0]);
                    if (foundDisk != null)
                    {
                        treeViewRoot_thisPC.Children.Remove(foundDisk);
                    }
                }
            }
            else if (parentNode == treeViewRoot_network)
            {
                List<VM.TreeViewModelHost> curHostList = GetAllTreeNodesHost();
                VM.TreeViewModelHost foundHost;
                foreach (Core.ReloadItemInfo lostHost in dirInfoList)
                {
                    foundHost = curHostList.Find(h => h.hostName == lostHost.Name);
                    if (foundHost != null)
                    {
                        treeViewRoot_network.Children.Remove(foundHost);
                    }
                }
            }
            else if (parentNode is VM.TreeViewModelDisk)
            {
                RemoveNodes(parentNode, dirInfoList, fileInfoList);
            }
            else if (parentNode is VM.TreeViewModelHost)
            {
                RemoveNodes(parentNode, dirInfoList, null);
            }
            else if (parentNode is VM.TreeViewModelDir)
            {
                RemoveNodes(parentNode, dirInfoList, fileInfoList);
            }

            void RemoveNodes(object parentNode, List<Core.ReloadItemInfo> dirInfoList, List<Core.ReloadItemInfo> fileInfoList)
            {
                VM.TreeViewModelPhysical parentNodeDir = (VM.TreeViewModelPhysical)parentNode;
                List<VM.TreeViewModelDir> curDirList = GetAllTreeNodesDir(parentNodeDir.Children);
                VM.TreeViewModelDir foundDir;
                IOInfoShadow foundIO;

                if (dirInfoList == null && fileInfoList == null)
                {
                    if (parentNode is VM.TreeViewModelDisk)
                    {
                        treeViewRoot_thisPC.Children.Remove(parentNode);
                    }
                    else if (parentNode is VM.TreeViewModelDir && parentNodeDir.parent != null)
                    {
                        VM.TreeViewModelDir parDir = (VM.TreeViewModelDir)parentNode;
                        VM.TreeViewModelPhysical parParNode = (VM.TreeViewModelPhysical)parDir.parent;
                        parParNode.Children.Remove(parentNode);
                        IOInfoShadow io = parParNode.fullContent.Find(io => io.fullName == parDir.dirInfo.fullName);
                        if (io != null)
                            parParNode.fullContent.Remove(io);
                    }
                }
                else if (dirInfoList != null)
                {
                    foreach (Core.ReloadItemInfo lostDir in dirInfoList)
                    {
                        foundDir = curDirList.Find(d => d.dirInfo.name.ToLower() == lostDir.ioInfo.name.ToLower());
                        if (foundDir != null)
                        {
                            parentNodeDir.Children.Remove(foundDir);
                        }
                        foundIO = parentNodeDir.fullContent.Find(io => io.name.ToLower() == lostDir.ioInfo.name.ToLower());
                        if (foundIO != null)
                        {
                            parentNodeDir.fullContent.Remove(foundIO);
                        }
                    }
                }
                if (fileInfoList != null)
                {
                    foreach (Core.ReloadItemInfo lostFile in fileInfoList)
                    {
                        foundIO = parentNodeDir.fullContent.Find(io => io.name.ToLower() == lostFile.ioInfo.name.ToLower());
                        if (foundIO != null)
                        {
                            parentNodeDir.fullContent.Remove(foundIO);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 新增/更新（树）节点，如果不存在则新增，存在则更新；
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="newDirNodes"></param>
        /// <param name="newFileNodes"></param>
        private void TreeNodesAdd(object parentNode, List<object> newDirNodes, List<IOInfoShadow> newFileNodes)
        {
            if (parentNode == treeViewRoot_thisPC)
            {
                List<VM.TreeViewModelDisk> curDiskList = GetAllTreeNodesDisk();
                VM.TreeViewModelDisk foundDisk;
                foreach (VM.TreeViewModelDisk newDisk in newDirNodes)
                {
                    foundDisk = curDiskList.Find(d => d.diskInfo.volumeLabel == newDisk.diskInfo.volumeLabel);
                    if (foundDisk == null)
                    {
                        treeViewRoot_thisPC.Children.Add(newDisk);
                        newDisk.ActionExpandedChanged = TreeNodeExpendChangedHandler;
                    }
                    else
                    {
                        foundDisk.UpdateFrom(newDisk);
                    }
                }
            }
            else if (parentNode == treeViewRoot_network)
            {
                List<VM.TreeViewModelHost> curHostList = GetAllTreeNodesHost();
                VM.TreeViewModelHost foundHost;
                foreach (VM.TreeViewModelHost newHost in newDirNodes)
                {
                    foundHost = curHostList.Find(d => d.hostName == newHost.hostName);
                    if (foundHost == null)
                    {
                        treeViewRoot_thisPC.Children.Add(newHost);
                        newHost.ActionExpandedChanged = TreeNodeExpendChangedHandler;
                    }
                    else
                    {
                        foundHost.UpdateFrom(newHost);
                    }
                }
            }
            else if (parentNode is VM.TreeViewModelHost)
            {
                AddNodes(parentNode, GetAllTreeNodesDir(newDirNodes), newFileNodes);
            }
            else if (parentNode is VM.TreeViewModelDisk)
            {
                AddNodes(parentNode, GetAllTreeNodesDir(newDirNodes), newFileNodes);
            }
            else if (parentNode is VM.TreeViewModelDir)
            {
                AddNodes(parentNode, GetAllTreeNodesDir(newDirNodes), newFileNodes);
            }

            void AddNodes(object parentNode, List<VM.TreeViewModelDir> newDirNodes, List<IOInfoShadow> newFileNodes)
            {
                VM.TreeViewModelPhysical parentNodeDir = (VM.TreeViewModelPhysical)parentNode;
                List<VM.TreeViewModelDir> curDirList = GetAllTreeNodesDir(parentNodeDir.Children);
                VM.TreeViewModelDir foundNodeDir;
                IOInfoShadow foundIO;
                foreach (VM.TreeViewModelDir newDir in newDirNodes)
                {
                    foundNodeDir = curDirList.Find(d => d.dirInfo.name.ToLower() == newDir.dirInfo.name.ToLower());
                    if (foundNodeDir == null)
                    {
                        parentNodeDir.Children.Add(newDir);
                        newDir.ActionExpandedChanged = TreeNodeExpendChangedHandler;
                    }
                    else
                    {
                        foundNodeDir.UpdateFrom(newDir);
                    }
                    foundIO = parentNodeDir.fullContent.Find(di => di.name.ToLower() == newDir.dirInfo.name.ToLower());
                    if (foundIO == null)
                    {
                        parentNodeDir.fullContent.Add(newDir.dirInfo);
                    }
                    else
                    {
                        foundIO.UpdateFrom(newDir.dirInfo);
                    }
                }
                if (newFileNodes != null)
                {
                    foreach (IOInfoShadow newFi in newFileNodes)
                    {
                        foundIO = parentNodeDir.fullContent.Find(fi => fi.name.ToLower() == newFi.name.ToLower());
                        if (foundIO == null)
                        {
                            parentNodeDir.fullContent.Add(newFi);
                        }
                        else
                        {
                            foundIO.UpdateFrom(newFi);
                        }
                    }
                }
            }
        }
        private void TreeNodesAdd(object parentNode, List<Core.ReloadItemInfo> dirInfoList, List<Core.ReloadItemInfo> fileInfoList)
        {
            TreeNodesAdd(parentNode, GetNodes(parentNode, dirInfoList), GetFiles(fileInfoList));
        }
        #endregion


        #region 操纵datagrid同步、增加、删除行

        private ObservableCollection<object> dataGridItemSource = new ObservableCollection<object>();
        private void DataGridItemClear(bool showLoadingLabel = false)
        {
            dataGridItemSource.Clear();
            if (showLoadingLabel)
                DataGridShowLoadingLabel();
        }

        private void DataGridShowLoadingLabel()
        {
            tb_datagrid_loading.Visibility = Visibility.Visible;
        }
        private void DataGridHideLoadingLabel()
        {
            tb_datagrid_loading.Visibility = Visibility.Collapsed;
        }

        private List<VM.DataGridRowModle_dirNFile> GetAllDataGridDirNFile()
        {
            List<VM.DataGridRowModle_dirNFile> result = new List<VM.DataGridRowModle_dirNFile>();
            if (dataGridItemSource.Count > 0 && dataGridItemSource[0] is VM.DataGridRowModle_dirNFile)
            {
                foreach (object o in dataGridItemSource)
                {
                    if (o is VM.DataGridRowModle_dirNFile)
                        result.Add((VM.DataGridRowModle_dirNFile)o);
                }
            }
            return result;
        }
        private List<VM.DataGridRowModle_disk> GetAllDataGridDisk()
        {
            List<VM.DataGridRowModle_disk> result = new List<VM.DataGridRowModle_disk>();
            if (dataGridItemSource.Count > 0 && dataGridItemSource[0] is VM.DataGridRowModle_disk)
            {
                foreach (VM.DataGridRowModle_disk disk in dataGridItemSource)
                    result.Add(disk);
            }
            return result;
        }
        private List<VM.DataGridRowModle_host> GetAllDataGridHost()
        {
            List<VM.DataGridRowModle_host> result = new List<VM.DataGridRowModle_host>();
            if (dataGridItemSource.Count > 0 && dataGridItemSource[0] is VM.DataGridRowModle_host)
            {
                foreach (VM.DataGridRowModle_host host in dataGridItemSource)
                    result.Add(host);
            }
            return result;
        }

        public async void DataGridItemSync(List<IOInfoShadow> newIOList)
        {
            List<VM.DataGridRowModle_dirNFile> curDFList = GetAllDataGridDirNFile();
            VM.DataGridRowModle_dirNFile foundDF, newDF;
            List<IOInfoShadow> curIOList = curDFList.Select(a => a.iois).ToList();
            IOInfoShadow foundIO;
            List<object> colorItemsAdded = new List<object>();
            List<object> colorItemsUpdated = new List<object>();
            // 用新清单做参考，找出原表中没有的（需增加）和 已存在的（需更新）；
            foreach (IOInfoShadow newIO in newIOList)
            {
                foundIO = curIOList.Find(io => io.fullName.ToLower() == newIO.fullName.ToLower());
                if (foundIO == null)
                {
                    newDF = new VM.DataGridRowModle_dirNFile(newIO);
                    dataGridItemSource.Add(newDF);
                    colorItemsAdded.Add(newDF);
                }
                else
                {
                    foundDF = curDFList.Find(df => df.iois == foundIO);
                    if (foundDF.UpdateFrom(newIO))
                        colorItemsUpdated.Add(foundDF);
                }
            }
            // 用原表做参考，找出新清单中没有的（需删除）；
            foreach (IOInfoShadow curIO in curIOList)
            {
                foundIO = newIOList.Find(io => io.fullName.ToLower() == curIO.fullName.ToLower());
                if (foundIO == null)
                {
                    foundDF = curDFList.Find(df => df.iois.fullName.ToLower() == curIO.fullName.ToLower());
                    dataGridItemSource.Remove(foundDF);
                    curDFList.Remove(foundDF);
                }
            }
            await dataGrid_delayRefreshAsync();

            // 新增，更新的条目，增加色块动画
            dataGrid_doAnimateAsync(
                dataGrid_doAnimate_getVisiblePosis(colorItemsAdded),
                dataGrid_doAnimate_colorAdded);
            dataGrid_doAnimateAsync(
                dataGrid_doAnimate_getVisiblePosis(colorItemsUpdated),
                dataGrid_doAnimate_colorUpdated);
        }
        public async void DataGridItemSync(List<VM.TreeViewModelDisk> newDiskList)
        {
            List<VM.DataGridRowModle_disk> curDiskList = GetAllDataGridDisk();
            VM.DataGridRowModle_disk foundDGDisk, newDGDisk;
            List<object> animaListAdd = new List<object>();
            List<object> animaListUpdate = new List<object>();
            foreach (VM.TreeViewModelDisk tnDisk in newDiskList)
            {
                foundDGDisk = curDiskList.Find(d => d.diskInfo.name[0] == tnDisk.diskInfo.name[0]);
                if (foundDGDisk == null)
                {
                    newDGDisk = new VM.DataGridRowModle_disk(tnDisk);
                    dataGridItemSource.Add(newDGDisk);
                    curDiskList.Add(newDGDisk);
                    animaListAdd.Add(newDGDisk);
                }
                else
                {
                    foundDGDisk.UpdateFrom(tnDisk);
                    animaListUpdate.Add(foundDGDisk);
                }
            }
            VM.TreeViewModelDisk foundTNDisk;
            foreach (VM.DataGridRowModle_disk dgDisk in curDiskList)
            {
                foundTNDisk = newDiskList.Find(d => d.diskInfo.name[0] == dgDisk.diskInfo.name[0]);
                if (foundTNDisk == null)
                {
                    dataGridItemSource.Remove(dgDisk);
                }
            }
            await dataGrid_delayRefreshAsync();

            // 动画
            dataGrid_doAnimateAsync(
                dataGrid_doAnimate_getVisiblePosis(animaListAdd),
                dataGrid_doAnimate_colorAdded);
            dataGrid_doAnimateAsync(
                dataGrid_doAnimate_getVisiblePosis(animaListUpdate),
                dataGrid_doAnimate_colorUpdated);
        }
        public async void DataGridItemSync(List<VM.TreeViewModelHost> newHostList)
        {
            List<VM.DataGridRowModle_host> curHostList = GetAllDataGridHost();
            VM.DataGridRowModle_host foundDGHost, newDGHost;
            List<object> animaListAdd = new List<object>();
            List<object> animaListUpdate = new List<object>();
            foreach (VM.TreeViewModelHost newHost in newHostList)
            {
                foundDGHost = curHostList.Find(h => h.name == newHost.hostName);
                if (foundDGHost == null)
                {
                    newDGHost = new VM.DataGridRowModle_host(newHost);
                    dataGridItemSource.Add(newDGHost);
                    curHostList.Add(newDGHost);
                    animaListAdd.Add(newDGHost);
                }
                else
                {
                    if (foundDGHost.UpdateFrom(newHost))
                        animaListUpdate.Add(foundDGHost);
                    dataGrid_delayRefreshAsync();
                }
            }
            VM.TreeViewModelHost foundTNHost;
            foreach (VM.DataGridRowModle_host curHost in curHostList)
            {
                foundTNHost = newHostList.Find(h => h.hostName == curHost.name);
                if (foundTNHost == null)
                    dataGridItemSource.Remove(curHost);
            }

            // 动画
            dataGrid_doAnimateAsync(
                dataGrid_doAnimate_getVisiblePosis(animaListAdd),
                dataGrid_doAnimate_colorAdded);
            dataGrid_doAnimateAsync(
                dataGrid_doAnimate_getVisiblePosis(animaListUpdate),
                dataGrid_doAnimate_colorUpdated);
        }
        public void DataGridItemSync(object parentTreeNode)
        {
            if (parentTreeNode == treeViewRoot_thisPC)
            {
                List<VM.TreeViewModelDisk> newDiskList = new List<VM.TreeViewModelDisk>();
                foreach (object o in treeViewRoot_thisPC.Children)
                {
                    if (o is VM.TreeViewModelDisk)
                        newDiskList.Add((VM.TreeViewModelDisk)o);
                }
                DataGridItemSync(newDiskList);
            }
            else if (parentTreeNode == treeViewRoot_network)
            {
                List<VM.TreeViewModelHost> newHostList = new List<VM.TreeViewModelHost>();
                foreach (object o in treeViewRoot_network.Children)
                {
                    if (o is VM.TreeViewModelHost)
                        newHostList.Add((VM.TreeViewModelHost)o);
                }
                DataGridItemSync(newHostList);
            }
            else // dir
            {
                VM.TreeViewModelPhysical ioTN = (VM.TreeViewModelPhysical)parentTreeNode;
                if (ioTN != null)
                    DataGridItemSync(ioTN.fullContent);

                // 2023 1115 sort
                dataGrid.Items.SortDescriptions.Add(new SortDescription(dataGrid_colSortMemberPath_nameForSorting, ListSortDirection.Ascending));
            }
        }
        public void DataGridItemAdd(List<IOInfoShadow> newIOList, bool doAnimate = true)
        {
            List<object> vmList = new List<object>();
            VM.DataGridRowModle_dirNFile vm;
            foreach (IOInfoShadow iois in newIOList)
            {
                vm = new VM.DataGridRowModle_dirNFile(iois);
                dataGridItemSource.Add(vm);
                if (doAnimate)
                    vmList.Add(vm);
            }

            // 增加色块动画
            if (doAnimate)
                dataGrid_doAnimateAsync(
                    dataGrid_doAnimate_getVisiblePosis(vmList),
                    dataGrid_doAnimate_colorAdded);
        }
        public void DataGridItemAdd_afterTreeViewRootSelect(IEnumerable<object> treeNodes)
        {
            object test = treeNodes.FirstOrDefault();
            if (test is VM.TreeViewModelLink)
            {
                VM.DataGridRowModle_link dgItem;
                foreach (VM.TreeViewModelLink tnLink in GetAllTreeNodesLink())
                {
                    dgItem = new VM.DataGridRowModle_link(tnLink);
                    dataGridItemSource.Add(dgItem);
                }
            }
            else if (test is VM.TreeViewModelDisk)
            {
                VM.DataGridRowModle_disk dgItem;
                foreach (VM.TreeViewModelDisk tnDisk in GetAllTreeNodesDisk(treeNodes))
                {
                    dgItem = new VM.DataGridRowModle_disk(tnDisk);
                    dataGridItemSource.Add(dgItem);
                }
            }
            else if (test is VM.TreeViewModelHost)
            {
                VM.DataGridRowModle_host dgItem;
                foreach (VM.TreeViewModelHost tnHost in GetAllTreeNodesHost(treeNodes))
                {
                    dgItem = new VM.DataGridRowModle_host(tnHost);
                    dataGridItemSource.Add(dgItem);
                }
            }
            else
            {
                //else if (treeView.SelectedItem is VM.TreeViewModelHost)
                // 当显示host根文件夹时，图标采用网络文件夹图标；
                // 直接采用treeviewnode的图标，所以无需再次判断

                VM.DataGridRowModle_dirNFile dgItem;
                foreach (VM.TreeViewModelDir tnDir in GetAllTreeNodesDir(treeNodes))
                {
                    dgItem = new VM.DataGridRowModle_dirNFile(tnDir);
                    dataGridItemSource.Add(dgItem);
                }
            }
        }

        #endregion


        #region 文件操作、变化，改变ui外观 和 播放色块动画

        private string[] justCopyCutFilesPre;
        private void Core_FilesCut(Core curCore, string[] files)
        {
            // 将上次设定了透明度的项目回复不透明；
            dataGrid_setItemsOpacity(justCopyCutFilesPre, false);
            // 设定被剪切项目的透明度
            justCopyCutFilesPre = files;
            dataGrid_setItemsOpacity(justCopyCutFilesPre, true);
            // 播放色块动画
            dataGrid_doAnimate_files(justCopyCutFilesPre, dataGrid_doAnimate_colorCut);
        }
        private void Core_FilesCopyed(Core curCore, string[] files)
        {
            // 将上次设定了透明度的项目回复不透明；
            dataGrid_setItemsOpacity(justCopyCutFilesPre, false);
            // 设定被复制项目的透明度
            justCopyCutFilesPre = files;
            dataGrid_setItemsOpacity(justCopyCutFilesPre, false);
            // 播放色块动画
            dataGrid_doAnimate_files(justCopyCutFilesPre, dataGrid_doAnimate_colorCopy);
        }
        private void dataGrid_setItemsOpacity(string[] files, bool isCut = true)
        {
            if (files == null)
                return;
            if (dataGrid.Items == null || dataGrid.Items.Count == 0)
                return;
            object testI = dataGrid.Items[0];
            if (!(testI is VM.DataGridRowModle_dirNFile))
                return;

            foreach (string f in files)
            {
                foreach (VM.DataGridRowModle_dirNFile i in dataGrid.Items)
                {
                    if (i.iois.fullName == f)
                    {
                        if (isCut)
                        {
                            i.iconOpacity = 0.3d;
                        }
                        else
                        {
                            if (i.iois.attributes.hidden)
                                i.iconOpacity = 0.5d;
                            else
                                i.iconOpacity = 1d;
                        }
                    }
                }
            }
        }

        public static Color dataGrid_doAnimate_colorCopy = Colors.LightGreen;
        public static Color dataGrid_doAnimate_colorCut = Colors.LightBlue;
        public static Color dataGrid_doAnimate_colorAdded = Colors.DodgerBlue;
        public static Color dataGrid_doAnimate_colorUpdated = Colors.Orange;
        private void dataGrid_doAnimate_files(string[] files, Color fillColor)
        {
            if (files == null || files.Length == 0)
                return;
            if (dataGrid.Items == null || dataGrid.Items.Count == 0)
                return;
            if (!(dataGrid.Items[0] is VM.DataGridRowModle_dirNFile))
                return;

            // 1 storyboard 可以包含若干 animation， 每个animation关联一个图形/UI
            // storyboard 用完后，回收，动画保留，备用；


            // 先搜索当前datagrid所显示出来的需要加动画的项目
            // x- row height, y row top
            List<object> vmList = new List<object>();
            foreach (string f in files)
            {
                foreach (VM.DataGridRowModle_dirNFile i in dataGrid.Items)
                {
                    if (i.iois.fullName == f)
                        vmList.Add(i);
                }
            }
            List<Point> posiList = dataGrid_doAnimate_getVisiblePosis(vmList);
            if (posiList.Count == 0)
                return;

            dataGrid_doAnimateAsync(posiList, fillColor);
        }
        private Task dataGrid_doAnimateAsync(List<Point> posiList, Color color)
        {
            return Task.Run(() =>
            {
                // 制作并播放动画
                // 获取动画元素
                List<ColorBlockAnimatePair> freeBlocks = dataGrid_doAnimate_getFreeBlocks(posiList.Count);
                // 改变颜色
                dataGrid_doAnimate_setBlockColor(freeBlocks, color);


                //sBrd.Completed += (s1, e1) => { sBrd.cle };
                Dispatcher.Invoke(() =>
                {
                    Storyboard sBrd = new Storyboard() { AccelerationRatio = 0.8d, };
                    ColorBlockAnimatePair cbap;
                    Point pt;
                    for (int i = 0, iv = posiList.Count; i < iv; ++i)
                    {
                        cbap = freeBlocks[i];
                        pt = posiList[i];
                        cbap.block.Height = pt.X;
                        Canvas.SetTop(cbap.block, pt.Y);
                        sBrd.Children.Add(cbap.anima);
                    }
                    sBrd.Begin();
                });
            });
        }

        private List<Point> dataGrid_doAnimate_getVisiblePosis(List<object> vmList)
        {
            List<Point> result = new List<Point>();
            Rect? dgRowBoundry;
            Rect dgRowBoundry1;
            double dgHeight = dataGrid.ActualHeight;
            DataGridRow dgRow;
            foreach (object i in vmList)
            {
                dgRow = UI.VisualHelper.DataGrid.GetItemUI(dataGrid, i);
                if (dgRow == null)
                    continue;
                dgRowBoundry = UI.VisualHelper.DataGrid.GetItemBoundry(dataGrid, dgRow);
                if (dgRowBoundry == null)
                    continue;
                dgRowBoundry1 = (Rect)dgRowBoundry;
                if (dgHeight < dgRowBoundry1.Top)
                    continue;
                if (dgRowBoundry1.Bottom < 0)
                    continue;

                result.Add(new Point(dgRowBoundry1.Height, dgRowBoundry1.Top));
            }
            return result;
        }


        private class ColorBlockAnimatePair
        {
            public ColorBlockAnimatePair(Rectangle block, DoubleAnimation anima)
            {
                this.block = block;
                this.anima = anima;
            }
            public Rectangle block;
            public DoubleAnimation anima;
            public bool isIdle
            {
                get
                {
                    return block.Opacity == 0;
                }
            }
            public void SetBlockPosition(double x, double y)
            {
                Canvas.SetLeft(block, x);
                Canvas.SetTop(block, y);
            }
        }
        List<ColorBlockAnimatePair> colorBlockAnimateList = new List<ColorBlockAnimatePair>();
        private List<ColorBlockAnimatePair> dataGrid_doAnimate_getFreeBlocks(int count)
        {
            List<ColorBlockAnimatePair> result = new List<ColorBlockAnimatePair>();
            int curC = 0;
            ColorBlockAnimatePair newCBA;
            Dispatcher.Invoke(() =>
            {
                // 复用空闲的动画
                foreach (ColorBlockAnimatePair i in colorBlockAnimateList)
                {
                    if (i.isIdle)
                    {
                        result.Add(i);
                        if (++curC >= count)
                            break;
                    }
                }
                // 新建缺少的动画
                for (; curC < count; ++curC)
                {
                    newCBA = new ColorBlockAnimatePair(
                        // 色块的宽度，默认宽度 10
                        new Rectangle() { Width = 10, Height = 10, Opacity = 0d, },
                        new DoubleAnimation()
                        {
                            From = 1d,
                            To = 0d,
                            Duration = new Duration(new TimeSpan(0, 0, 0, 0, 300))
                        });
                    canvas_colorBlocks.Children.Add(newCBA.block);
                    Storyboard.SetTargetProperty(newCBA.anima, new PropertyPath("Opacity"));
                    Storyboard.SetTarget(newCBA.anima, newCBA.block);
                    colorBlockAnimateList.Add(newCBA);
                    result.Add(newCBA);
                }
            });
            return result;
        }
        private void dataGrid_doAnimate_setBlockColor(List<ColorBlockAnimatePair> bList, Color clr)
        {
            Dispatcher.Invoke(() =>
            {
                SolidColorBrush brush = new SolidColorBrush(clr);
                foreach (ColorBlockAnimatePair i in bList)
                    i.block.Fill = brush;
            });
        }

        #endregion





        #region set datagrid columns

        private string dataGrid_columns_flag = null;

        public string dataGrid_colSortMemberPath_name = "name";
        public string dataGrid_colSortMemberPath_nameForSorting = "nameForSorting";
        public string dataGrid_colSortMemberPath_link = "link";

        public string dataGrid_colSortMemberPath_diskVolume = "diskVolume";
        public string dataGrid_colSortMemberPath_diskType = "diskType";
        public string dataGrid_colSortMemberPath_diskFormat = "diskFormat";
        public string dataGrid_colSortMemberPath_totalSize = "totalSize";
        public string dataGrid_colSortMemberPath_freeSpace = "freeSpace";
        public string dataGrid_colSortMemberPath_usedPersentage = "usedPersentage";

        public string dataGrid_colSortMemberPath_fileType = "fileType";
        public string dataGrid_colSortMemberPath_fileSize = "fileSize";
        public string dataGrid_colSortMemberPath_attributes = "attributes";
        public string dataGrid_colSortMemberPath_modifyTime = "modifyTime";

        //FrameworkElementFactory dataGrid_column_iconNnameFE;
        private Style _dataGridCellStyle_verticalCenter;
        Style dataGridCellStyle_verticalCenter
        {
            get
            {
                if (_dataGridCellStyle_verticalCenter == null)
                {
                    _dataGridCellStyle_verticalCenter = new Style();
                    dataGridCellStyle_verticalCenter.Setters.Add(
                        new Setter(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center));
                }
                return _dataGridCellStyle_verticalCenter;
            }
        }

        private FrameworkElementFactory GetNameColVT()
        {
            //if (dataGrid_column_iconNnameFE == null)
            //{
            FrameworkElementFactory dataGrid_column_iconNnameFE = new FrameworkElementFactory(typeof(Grid));
            //dataGrid_column_iconNnameFE.SetValue(StackPanel.BackgroundProperty, Brushes.Transparent);
            //dataGrid_column_iconNnameFE.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            dataGrid_column_iconNnameFE.SetValue(StackPanel.MarginProperty, new Thickness(1));
            FrameworkElementFactory icon = new FrameworkElementFactory(typeof(Image));
            icon.SetBinding(Image.SourceProperty, new Binding("icon"));
            icon.SetBinding(Image.OpacityProperty, new Binding("iconOpacity"));
            //icon.SetValue(Image.StretchProperty, Stretch.None);
            icon.SetValue(Image.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            icon.SetValue(Image.WidthProperty, 16d);
            icon.SetValue(Image.HeightProperty, 16d);
            icon.SetValue(Image.MarginProperty, new Thickness(2, 0, 0, 0));

            icon.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.NearestNeighbor);
            icon.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            dataGrid_column_iconNnameFE.AppendChild(icon);
            FrameworkElementFactory name = new FrameworkElementFactory(typeof(TextBlock));
            //name.SetValue(TextBlock.BackgroundProperty, Brushes.Transparent);
            name.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            name.SetValue(TextBlock.FontFamilyProperty, new FontFamily("consolas"));
            name.SetValue(TextBlock.FontSizeProperty, 12d);
            name.SetValue(TextBlock.MarginProperty, new Thickness(18, 0, 0, 0));
            name.SetValue(TextBlock.PaddingProperty, new Thickness(4, 0, 4, 0));
            name.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
            name.SetBinding(TextBlock.TextProperty, new Binding("name"));

            dataGrid_column_iconNnameFE.AppendChild(name);
            //}
            return dataGrid_column_iconNnameFE;
        }
        private FrameworkElementFactory GetTextBlockCol(string bindingTxName, bool? tx_nullLeft_falseMid_trueRight = null)
        {
            //FrameworkElementFactory container = new FrameworkElementFactory(typeof(Grid));

            FrameworkElementFactory txCol = new FrameworkElementFactory(typeof(TextBlock));
            //txCol.SetValue(TextBlock.BackgroundProperty, Brushes.Transparent);
            txCol.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            txCol.SetValue(Image.MarginProperty, new Thickness(2, 0, 2, 0));
            txCol.SetBinding(TextBlock.TextProperty, new Binding(bindingTxName));
            if (tx_nullLeft_falseMid_trueRight == false)
                txCol.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
            else if (tx_nullLeft_falseMid_trueRight == true)
                txCol.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
            //txCol.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Stretch);
            //container.AppendChild(txCol);
            //return container;
            return txCol;
        }
        private FrameworkElementFactory GetCombinedText(string numBindingName, string unitBindingName)
        {
            FrameworkElementFactory container = new FrameworkElementFactory(typeof(DockPanel));
            container.SetValue(DockPanel.LastChildFillProperty, true);
            FrameworkElementFactory tx2 = new FrameworkElementFactory(typeof(TextBlock));
            tx2.SetValue(TextBlock.PaddingProperty, new Thickness(6, 0, 0, 0));
            tx2.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            tx2.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
            tx2.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            tx2.SetValue(DockPanel.DockProperty, Dock.Right);
            tx2.SetBinding(TextBlock.TextProperty, new Binding(unitBindingName));
            container.AppendChild(tx2);
            //container.SetValue(DockPanel., Orientation.Horizontal);
            FrameworkElementFactory tx1 = new FrameworkElementFactory(typeof(TextBlock));
            tx1.SetValue(TextBlock.PaddingProperty, new Thickness(6, 0, 0, 0));
            tx1.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            tx1.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
            tx1.SetBinding(TextBlock.TextProperty, new Binding(numBindingName));
            container.AppendChild(tx1);
            //tx2.SetValue(TextBlock.ForegroundProperty, dataGrid_column_unitFontBrush);
            return container;
        }

        private ObservableCollection<DataGridColumn> dataGrid_columns_link = null;
        private void dataGrid_setColumns_links()
        {
            if (dataGrid_columns_flag == "link")
                return;
            dataGrid_columns_flag = "link";

            // name   link
            if (dataGrid_columns_link == null)
            {
                dataGrid_columns_link = new ObservableCollection<DataGridColumn>();

                //fef.SetValue(Image.HeightProperty, 16.0);
                dataGrid_columns_link.Add(new DataGridTemplateColumn()
                {
                    Header = core.GetLangTx("txViewCol_name"),
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetNameColVT(),
                    },
                    CellStyle = dataGridCellStyle_verticalCenter,
                    CanUserSort = true,
                    SortMemberPath = dataGrid_colSortMemberPath_name,
                });
                dataGrid_columns_link.Add(new DataGridHyperlinkColumn()
                {
                    Header = core.GetLangTx("txViewCol_linkAddress"),
                    IsReadOnly = true,
                    Binding = new Binding("link"),
                    CellStyle = dataGridCellStyle_verticalCenter,
                    SortMemberPath = dataGrid_colSortMemberPath_link,
                });
            }
            dataGrid.Columns.Clear();
            foreach (DataGridColumn dgc in dataGrid_columns_link)
                dataGrid.Columns.Add(dgc);
        }

        private Brush dataGrid_column_unitFontBrush = new SolidColorBrush(Color.FromRgb(0, 0, 128));
        private ObservableCollection<DataGridColumn> dataGrid_columns_disks = null;

        private void dataGrid_setColumns_disks()
        {
            if (dataGrid_columns_flag == "disks")
                return;
            dataGrid_columns_flag = "disks";

            // name   type   format   total-size   free-space   used-persentage
            if (dataGrid_columns_disks == null)
            {
                dataGrid_columns_disks = new ObservableCollection<DataGridColumn>();
                dataGrid_columns_disks.Add(new DataGridTemplateColumn()
                {
                    Header = core.GetLangTx("txViewCol_name"),
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetNameColVT(),
                    },
                    CellStyle = dataGridCellStyle_verticalCenter,
                    CanUserSort = true,
                    SortMemberPath = dataGrid_colSortMemberPath_diskVolume,
                });
                dataGrid_columns_disks.Add(new DataGridTemplateColumn()
                {
                    Header = core.GetLangTx("txViewCol_diskType"),
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetTextBlockCol("diskType"),
                    },
                    IsReadOnly = true,
                    SortMemberPath = dataGrid_colSortMemberPath_diskType,
                });
                dataGrid_columns_disks.Add(new DataGridTemplateColumn()
                {
                    Header = core.GetLangTx("txViewCol_diskFormat"),
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetTextBlockCol("diskFormat"),
                    },
                    IsReadOnly = true,
                    SortMemberPath = dataGrid_colSortMemberPath_diskFormat,
                });


                dataGrid_columns_disks.Add(new DataGridTemplateColumn()
                {
                    Header = core.GetLangTx("txViewCol_totalSize"),
                    IsReadOnly = true,
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetCombinedText("totalSizeTxN", "totalSizeTxU"),
                    },
                    CanUserSort = true,
                    SortMemberPath = dataGrid_colSortMemberPath_totalSize,
                });

                dataGrid_columns_disks.Add(new DataGridTemplateColumn()
                {
                    Header = core.GetLangTx("txViewCol_FreeSpace"),
                    IsReadOnly = true,
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetCombinedText("freeSpaceTxN", "freeSpaceTxU"),
                    },
                    CanUserSort = true,
                    SortMemberPath = dataGrid_colSortMemberPath_freeSpace,
                });

                FrameworkElementFactory container = new FrameworkElementFactory(typeof(Grid));
                FrameworkElementFactory progressBar = new FrameworkElementFactory(typeof(ProgressBar));
                progressBar.SetValue(ProgressBar.WidthProperty, 100d);
                progressBar.SetValue(ProgressBar.MinimumProperty, 0d);
                progressBar.SetValue(ProgressBar.MaximumProperty, 1d);
                progressBar.SetBinding(ProgressBar.ValueProperty, new Binding("usedPersentage"));
                container.AppendChild(progressBar);
                FrameworkElementFactory progressText = new FrameworkElementFactory(typeof(TextBlock));
                progressText.SetBinding(TextBlock.TextProperty, new Binding("usedPersentageTx"));
                progressText.SetValue(TextBlock.HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Center);
                progressText.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                container.AppendChild(progressText);

                dataGrid_columns_disks.Add(new DataGridTemplateColumn()
                {
                    Header = core.GetLangTx("txViewCol_usedPersent"),
                    IsReadOnly = true,
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = container,
                    },
                    CanUserSort = true,
                    SortMemberPath = dataGrid_colSortMemberPath_usedPersentage,
                });
            }
            dataGrid.Columns.Clear();
            foreach (DataGridColumn dgc in dataGrid_columns_disks)
                dataGrid.Columns.Add(dgc);
        }

        private ObservableCollection<DataGridColumn> dataGrid_columns_hosts = null;
        private void dataGrid_setColumns_hosts()
        {
            if (dataGrid_columns_flag == "hosts")
                return;
            dataGrid_columns_flag = "hosts";

            // name ip
            if (dataGrid_columns_hosts == null)
            {
                dataGrid_columns_hosts = new ObservableCollection<DataGridColumn>();
                dataGrid_columns_hosts.Add(new DataGridTemplateColumn()
                {
                    Header = core.GetLangTx("txViewCol_name"),
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetNameColVT(),
                    },
                    CanUserSort = true,
                    SortMemberPath = dataGrid_colSortMemberPath_name,
                });
                Style style = new Style(typeof(TextBlock), dataGridCellStyle_verticalCenter);
                style.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(8, 0, 8, 0)));
                dataGrid_columns_hosts.Add(new DataGridTextColumn()
                {
                    Header = core.GetLangTx("txViewCol_ipv4"),
                    Binding = new Binding("hostIPv4"),
                    ElementStyle = style,
                    //Width = 100d,
                });
                dataGrid_columns_hosts.Add(new DataGridTextColumn()
                {
                    Header = core.GetLangTx("txViewCol_ipv6"),
                    Binding = new Binding("hostIPv6"),
                    ElementStyle = style,
                    //Width = 220d,
                });
            }
            dataGrid.Columns.Clear();
            foreach (DataGridColumn dgc in dataGrid_columns_hosts)
                dataGrid.Columns.Add(dgc);
        }

        private ObservableCollection<DataGridColumn> dataGrid_columns_hostDir = null;
        private void dataGrid_setColumns_hostDirs()
        {
            if (dataGrid_columns_flag == "hostDirs")
                return;
            dataGrid_columns_flag = "hostDirs";

            // name
            //if (dataGrid_columns_hostDir == null)
            //{
            dataGrid_columns_hostDir = new ObservableCollection<DataGridColumn>();
            dataGrid_columns_hostDir.Add(new DataGridTemplateColumn()
            {
                Header = core.GetLangTx("txViewCol_name"),
                CellTemplate = new DataTemplate()
                {
                    VisualTree = GetNameColVT(),
                },
                CanUserSort = true,
                SortMemberPath = dataGrid_colSortMemberPath_name,
                SortDirection = ListSortDirection.Ascending,
            });
            //}
            dataGrid.Columns.Clear();
            foreach (DataGridColumn dgc in dataGrid_columns_hostDir)
                dataGrid.Columns.Add(dgc);
        }

        private ObservableCollection<DataGridColumn> dataGrid_columns_dirsNfiles = null;

        private void dataGrid_setColumns_dirsNfiles()
        {
            if (dataGrid_columns_flag == "dirsNfiles")
                return;
            dataGrid_columns_flag = "dirsNfiles";

            // name   modify-time   type   size
            if (dataGrid_columns_dirsNfiles == null)
            {
                dataGrid_columns_dirsNfiles = new ObservableCollection<DataGridColumn>();
                dataGrid_columns_dirsNfiles.Add(new DataGridTemplateColumn()
                {
                    Header = core.GetLangTx("txViewCol_name"),
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetNameColVT(),
                    },
                    CanUserSort = true,
                    SortMemberPath = dataGrid_colSortMemberPath_nameForSorting,
                    SortDirection = ListSortDirection.Ascending,
                    // 2023 1115
                    Width = new DataGridLength(200d, DataGridLengthUnitType.Pixel),
                });

                Style style_HeaderCenter = new Style();
                style_HeaderCenter.Setters.Add(new Setter(GridViewColumnHeader.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Center));

                //Style style_right = new Style(typeof(TextBlock), dataGridCellStyle_verticalCenter);
                //style_right.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
                dataGrid_columns_dirsNfiles.Add(new DataGridTemplateColumn()
                {
                    Header = core.GetLangTx("txViewCol_type"),
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetTextBlockCol("fileType", false),
                    },
                    HeaderStyle = style_HeaderCenter,
                    SortMemberPath = dataGrid_colSortMemberPath_fileType,
                });


                dataGrid_columns_dirsNfiles.Add(new DataGridTemplateColumn()
                {
                    Header = core.GetLangTx("txViewCol_size"),
                    IsReadOnly = true,
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetCombinedText("fileSizeTxN", "fileSizeTxU"),
                    },
                    CanUserSort = true,
                    SortMemberPath = dataGrid_colSortMemberPath_fileSize,
                });

                dataGrid_columns_dirsNfiles.Add(new DataGridTemplateColumn()
                {
                    Header = core.GetLangTx("txViewCol_attributes"),
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetTextBlockCol("attributes"),
                    },
                    SortMemberPath = dataGrid_colSortMemberPath_attributes,
                });

                dataGrid_columns_dirsNfiles.Add(new DataGridTemplateColumn()
                {
                    Header = core.GetLangTx("txViewCol_modifyTime"),
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetTextBlockCol("modifyTimeTx"),
                    },
                    IsReadOnly = true,
                    SortMemberPath = dataGrid_colSortMemberPath_modifyTime,
                });
            }
            dataGrid.Columns.Clear();
            foreach (DataGridColumn dgc in dataGrid_columns_dirsNfiles)
                dataGrid.Columns.Add(dgc);
        }



        #endregion




        #region datagrid events handling, key down, text input



        private DateTime dataGrid_PreviewTextInput_time = DateTime.MinValue;
        private StringBuilder dataGrid_PreviewTextInput_quickFocusString = new StringBuilder();
        private string dataGrid_PreviewTextInput_txPre;
        private object dataGrid_PreviewTextInput_firstTxItem = null;
        private void dataGrid_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // quick find and focus
            // 用win浏览器测试，连续按键的间隔为500ms以内，如果超出这个时间，则不算连续输入
            // 如果按下相同的键，且有多个相同的同名项目，则以此遍历选择；

            DateTime now = DateTime.Now;
            string tx = e.Text.ToLower();

            if ((now - dataGrid_PreviewTextInput_time).TotalMilliseconds > 500)
            {
                dataGrid_PreviewTextInput_quickFocusString.Clear();
                dataGrid_PreviewTextInput_firstTxItem = null;
                dataGrid_PreviewTextInput_txPre = null;
            }
            if (dataGrid_PreviewTextInput_txPre != tx)
            {
                dataGrid_PreviewTextInput_quickFocusString.Append(tx);
            }

            object foundNext = null;
            if (dataGrid_PreviewTextInput_quickFocusString.ToString() == tx)
            {
                // 检查当前选项的开头是否和tx一样，一样的话找下一个选中
                int selectionIdx = -1;
                int dgItems = dataGridItemSource.Count;
                if (dataGrid.SelectedItems.Count > 0)
                {
                    selectionIdx = dataGridItemSource.IndexOf(dataGrid.SelectedItems[0]);
                    // 从当前选中位置往后找，找到了则选中
                }
                else
                {
                    // 从一开始开始找
                }
                for (int i = selectionIdx + 1; i < dgItems; ++i)
                {
                    foundNext = TestItem(dataGridItemSource[i], ref tx);
                    if (foundNext != null) break;
                }
                if (foundNext == null)
                {
                    for (int i = 0; i < selectionIdx; ++i)
                    {
                        foundNext = TestItem(dataGridItemSource[i], ref tx);
                        if (foundNext != null) break;
                    }
                }
            }
            else
            {
                // 按字符串找前缀一样的，如果有，则聚焦，没有，什么都不做
                tx = dataGrid_PreviewTextInput_quickFocusString.ToString();
                for (int i = 0, iv = dataGridItemSource.Count; i < iv; ++i)
                {
                    foundNext = TestItem(dataGridItemSource[i], ref tx);
                    if (foundNext != null) break;
                }
            }
            if (foundNext != null)
            {
                // 聚焦找到的内容；
                dataGrid.SelectedItems.Clear();
                if (dataGrid.SelectedItem != foundNext)
                    dataGrid.SelectedItem = foundNext;
                dataGrid_SetCurrentCell(foundNext, 1d);
                //dataGrid.Focus();
            }
            object TestItem(object obj, ref string tx)
            {
                VM.DataGridRowModleBase dgItemBase = (VM.DataGridRowModleBase)obj;
                if (dgItemBase.name.ToLower().StartsWith(tx))
                {
                    return dgItemBase;
                }
                return null;
            }

            dataGrid_PreviewTextInput_txPre = tx;
            dataGrid_PreviewTextInput_time = now;
        }



        private void dataGrid_executeItems(IList dgItems)
        {
            object test;
            if (dgItems == null || dgItems.Count <= 0)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    string pathToNewExplorer = null;
                    if (treeView.SelectedItem is VM.TreeViewModelDir)
                        pathToNewExplorer = ((VM.TreeViewModelDir)treeView.SelectedItem).dirInfo.fullName;
                    else if (treeView.SelectedItem is VM.TreeViewModelDisk)
                        pathToNewExplorer = ((VM.TreeViewModelDisk)treeView.SelectedItem).diskInfo.name;

                    if (pathToNewExplorer != null)
                        Process.Start("Explorer.exe", pathToNewExplorer);
                }
                return;
            }
            else if (dgItems.Count == 1)
            {
                test = dataGrid.SelectedItems[0];
                string pathToNavigate = null;
                if (test is VM.DataGridRowModle_link)
                    pathToNavigate = ((VM.DataGridRowModle_link)test).link;
                else if (test is VM.DataGridRowModle_disk)
                    pathToNavigate = ((VM.DataGridRowModle_disk)test).diskInfo.name;
                else if (test is VM.DataGridRowModle_host)
                    pathToNavigate = "\\\\" + ((VM.DataGridRowModle_host)test).name;
                else if (test is VM.DataGridRowModle_dirNFile)
                {
                    VM.DataGridRowModle_dirNFile dgItemDF = (VM.DataGridRowModle_dirNFile)test;
                    if (dgItemDF.iois.attributes.directory)
                        pathToNavigate = dgItemDF.iois.fullName;
                    else
                        dataGrid_executeFile(dgItemDF);
                }

                if (pathToNavigate != null)
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        if (Directory.Exists(pathToNavigate))
                        {
                            Process.Start("Explorer.exe", pathToNavigate);
                        }
                    }
                    else
                    {
                        NavigateTo(pathToNavigate);
                    }
                }
            }
            else // > 1
            {
                // 多个链接、磁盘、主机 - 无动作
                test = dgItems[0];
                if (test is VM.DataGridRowModle_link
                    || test is VM.DataGridRowModle_disk
                    || test is VM.DataGridRowModle_host)
                    return;

                // 1.选中了多个文件夹 - 无动作
                // 2.选中了文件夹和文件 - 无动作
                // 3.选中了多个不同后缀的文件 - 无动作
                // 4.选中了多个相同后缀的文件 - 全部打开

                List<VM.DataGridRowModle_dirNFile> dirs = new List<VM.DataGridRowModle_dirNFile>();
                List<VM.DataGridRowModle_dirNFile> files = new List<VM.DataGridRowModle_dirNFile>();
                foreach (VM.DataGridRowModle_dirNFile df in dgItems)
                {
                    if (df.iois.wasFile)
                    {
                        if (dirs.Count > 0) // 2
                            return;
                        files.Add(df);
                    }
                    else
                    {
                        if (dirs.Count > 1) // 1
                            return;
                        dirs.Add(df);
                    }
                }
                string ext = files[0].iois.extension.ToLower();
                for (int i = 1, iv = files.Count; i < iv; i++)
                {
                    if (files[i].iois.extension.ToLower() != ext) // 3
                        return;
                }
                // 4
                foreach (VM.DataGridRowModle_dirNFile file in files)
                {
                    dataGrid_executeFile(file);
                }
            }
        }
        private void dataGrid_executeFile(VM.DataGridRowModle_dirNFile file)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = file.iois.fullName,
                WorkingDirectory = file.iois.directoryName,
                UseShellExecute = true,
            });
        }






        private int dataGrid_SelectionChanged_itemIndexPre1;
        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // show states
            SetStatesSelection();
            if (dataGrid.SelectedItem != null)
            {
                dataGrid_SelectionChanged_itemIndexPre1 = dataGrid.SelectedIndex;
            }
        }


        private void dataGrid_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            //如果正在编辑文件名，则确认    
            // 只有通过鼠标拖动滚动条时才会触发；
            if (tb_rename.Visibility == Visibility.Visible)
            {
                if (TryStartRename_isDataGrid_orTreeView)
                    DataGridRename(tb_rename.Text.Trim());
                else
                    TreeViewRename(tb_rename.Text.Trim());
                dataGrid.Focus();
            }
        }
        private void dataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            dataGrid_Scroll(null, null);
        }



        #endregion




        #region treeview, mouse operations

        private Point treeView_PreviewMouseDown_point;
        private MouseButton? treeView_PreviewMouseDown_mouseButton = null;
        private DateTime treeView_PreviewMouseDown_time = DateTime.MinValue;
        private object treeView_PreviewMouseDown_pointItem;
        private Rect treeView_PreviewMouseDown_pointItem_boundry;
        private bool treeView_PreviewMouseDown_mouseMoved = false;
        private void treeView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            treeView_PreviewMouseDown_point = Mouse.GetPosition(treeView);
            if (treeView_PreviewMouseDown_point.X >= 0 && treeView_PreviewMouseDown_point.X < treeView.ActualWidth - 17
               && treeView_PreviewMouseDown_point.Y >= 0 && treeView_PreviewMouseDown_point.Y < treeView.ActualHeight - 17)
            {
                // 点击了视图区域（非滚动条区域）
                e.Handled = true;

                treeView_PreviewMouseDown_mouseButton = e.ChangedButton;
                DateTime now = DateTime.Now;

                treeView_PreviewMouseDown_time = now;
                treeView_PreviewMouseDown_pointItem = UI.VisualHelper.TreeView.GetNodeVM(treeView, treeView_PreviewMouseDown_point);

                if (treeView_PreviewMouseDown_pointItem != null)
                {
                    treeView_PreviewMouseDown_pointItem_boundry = UI.VisualHelper.TreeView.GetNodeBoundry(treeView, UI.VisualHelper.TreeView.GetNodeUI(treeView, treeView_PreviewMouseDown_pointItem));

                    if (treeView_PreviewMouseDown_pointItem is VM.TreeViewModelContainer)
                    {
                        VM.TreeViewModelContainer tnCtn = (VM.TreeViewModelContainer)treeView_PreviewMouseDown_pointItem;
                        if ((treeView_PreviewMouseDown_pointItem_boundry.Left + 21) <= treeView_PreviewMouseDown_point.X && treeView_PreviewMouseDown_point.X <= treeView_PreviewMouseDown_pointItem_boundry.Right)
                        {
                            // clicked node
                            tnCtn.IsSelected = true;
                        }
                        else if (treeView_PreviewMouseDown_pointItem_boundry.Left <= treeView_PreviewMouseDown_point.X && treeView_PreviewMouseDown_point.X <= (treeView_PreviewMouseDown_pointItem_boundry.Left + 19))
                        {
                            // clicked expander
                            if (tnCtn.Children != null && tnCtn.Children.Count > 0)
                                tnCtn.IsExpanded = !tnCtn.IsExpanded;
                        }
                    }
                }

                treeView_PreviewMouseDown_mouseMoved = false;

                if (tb_rename.Visibility == Visibility.Visible)
                {
                    TreeViewRename(tb_rename.Text.Trim());
                }
                flag_lastFocused_TVorDG = true;
                treeView.Focus();
            }
            else
            {
                // 点击了滚动条
                flag_lastFocused_TVorDG = null;
            }
        }

        private object treeView_LMB_pointNode = null;
        private DateTime treeView_LMB_timePre = DateTime.MaxValue;
        private object treeView_RMB_pointNode = null;
        private void treeView_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (treeView_PreviewMouseDown_mouseMoved)
            {
            }
            else
            {
                Point mp = Mouse.GetPosition(treeView);
                if (0 <= mp.X && mp.X < treeView.ActualWidth - 17
                    && 0 <= mp.Y && mp.Y < treeView.ActualHeight - 17)
                {
                    object curPointNode = UI.VisualHelper.TreeView.GetNodeVM(treeView, mp);
                    if (curPointNode != null)
                    {
                        switch (e.ChangedButton)
                        {
                            case MouseButton.Right:
                                treeView_RMB_pointNode = curPointNode;
                                treeView_showContextMenu(curPointNode);
                                break;
                            case MouseButton.Left:
                                DateTime now = DateTime.Now;
                                if (curPointNode != null
                                    && curPointNode == treeView_LMB_pointNode
                                    && curPointNode == treeView.SelectedItem
                                    && (now - treeView_LMB_timePre).TotalMilliseconds > 500)
                                {
                                    // 39 = 21 + 18
                                    if ((treeView_PreviewMouseDown_pointItem_boundry.Left + 39) <= treeView_PreviewMouseDown_point.X
                                        && treeView_PreviewMouseDown_point.X <= treeView_PreviewMouseDown_pointItem_boundry.Right)
                                    {
                                        TreeViewTryStartRename();
                                    }
                                }
                                treeView_LMB_pointNode = curPointNode;
                                treeView_LMB_timePre = now;
                                break;
                            case MouseButton.XButton1:
                                btn_back_Click(null, null);
                                break;
                            case MouseButton.XButton2:
                                btn_fore_Click(null, null);
                                break;
                        }
                    }
                }
            }
            treeView_PreviewMouseDown_mouseButton = null;
        }
        private void treeView_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            if (tb_rename.Visibility == Visibility.Visible)
            {
                treeView.Focus();
            }
        }
        private void treeView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            treeView_Scroll(null, null);
        }


        private void treeView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (treeView_PreviewMouseDown_mouseButton != null)
            {
                e.Handled = true;
                Point mp = Mouse.GetPosition(dataGrid);
                double mouseMoveXAbs = Math.Abs(mp.X - treeView_PreviewMouseDown_point.X);
                double mouseMoveYAbs = Math.Abs(mp.Y - treeView_PreviewMouseDown_point.Y);
                if (mouseMoveXAbs > 2 || mouseMoveYAbs > 2)
                {
                    if (treeView_PreviewMouseDown_pointItem == null)
                    {
                        treeView_PreviewMouseDown_mouseMoved = true;
                    }
                    else
                    {
                        object testItem = treeView.SelectedItem;
                        if (testItem == null)
                            return;

                        // start draging                        
                        if (testItem is VM.TreeViewModelDir)
                        {
                            // copy, move, link files
                            DragFiles(treeView, new string[] { ((VM.TreeViewModelDir)testItem).dirInfo.fullName });
                        }
                        else
                        {
                            // link, pathes
                            string[] files = new string[1];
                            if (testItem is VM.TreeViewModelDisk)
                            {
                                files[0] = ((VM.TreeViewModelDisk)testItem).diskInfo.name;
                            }
                            else if (testItem is VM.TreeViewModelHost)
                            {
                                files[0] = "\\\\" + ((VM.TreeViewModelHost)testItem).hostName;
                            }
                            else if (testItem is VM.TreeViewModelLink)
                            {
                                files[0] = ((VM.TreeViewModelLink)testItem).link.fullName;
                            }
                            else files[0] = null;

                            if (files[0] != null)
                            {
                                DragFiles_link(treeView, files);
                            }
                        }
                        treeView_PreviewMouseDown_mouseButton = null;
                    }
                }
            }
        }


        private void treeView_PreviewDragEnter(object sender, DragEventArgs e)
        {
            treeView_generalPreviewDragEnter(ref e);
            rect_treeView_dropZone.Visibility = Visibility.Visible;
        }

        private void rect_treeView_dropZone_PreviewDragEnter(object sender, DragEventArgs e)
        {
            treeView_generalPreviewDragEnter(ref e);
        }
        private void treeView_generalPreviewDragEnter(ref DragEventArgs e)
        {
            object tnSelected = treeView.SelectedItem;
            if (tnSelected == treeViewRoot_thisPC
                || tnSelected == treeViewRoot_network
                || tnSelected == treeViewRoot_quickAccess
                || tnSelected is VM.TreeViewModelHost
                || tnSelected is VM.TreeViewModelLink
                )
            {
                // do not alow to drop
                e.Effects = DragDropEffects.None;
            }
            else
            {
                // allow to drop
                // disk, dir
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effects = DragDropEffects.Move;
                else
                    e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }


        private object treeView_PreviewDragOver_pointItem;

        private string[] rect_treeView_dropZone_DragSources;
        private string rect_treeView_dropZone_DropTarget;
        private DragDropEffects rect_treeView_dropZone_finalDragDropEffect;
        private void rect_treeView_dropZone_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.AllowedEffects == DragDropEffects.None)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }


            Point mp = e.GetPosition((Rectangle)sender);
            //mp = new Point(core.mainWindow.Left - mp.X, core.mainWindow.Top - mp.Y);
            treeView_PreviewDragOver_pointItem = UI.VisualHelper.TreeView.GetNodeVM(treeView, mp);


            // 设定投放效果
            rect_treeView_dropZone_DragSources = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (treeView_PreviewDragOver_pointItem != null)
            {
                rect_treeView_dropZone_DropTarget = GetTreeNodeURL(treeView_PreviewDragOver_pointItem);
            }
            else
            {
                // 落点在空白位置，parent-dir
                rect_treeView_dropZone_DropTarget = null;
            }
            string statesMsg1, missingStr = null;
            if (Directory.Exists(rect_treeView_dropZone_DropTarget))
            {
                if (MouseNKeyboardHelper.CheckFileDrops(
                    rect_treeView_dropZone_DragSources,
                    rect_treeView_dropZone_DropTarget,
                    out bool withinSameRoot, out bool withinSameDir, out Exception err))
                {
                    DragDropEffects defaultDDE = DragDropEffects.None;
                    if (withinSameDir)
                    {
                        // 将文件向其所在文件夹中拖放时，默认不允许
                        // 不过按住ctrl，可以复制，或alt创建连接
                        //e.Effects = DragDropEffects.None;
                    }
                    else if (withinSameRoot)
                    {
                        defaultDDE = DragDropEffects.Move;
                    }
                    else
                    {
                        defaultDDE = DragDropEffects.Copy;
                    }
                    e.Effects = MouseNKeyboardHelper.GetDemandDragDropEffect(e.KeyStates, e.AllowedEffects, defaultDDE);
                    statesMsg1 = core.GetLangTx("txStatus_releaseTo", e.Effects.ToString());
                }
                else
                {
                    if (withinSameDir)
                    {
                        if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                        {
                            e.Effects = DragDropEffects.Copy;
                            statesMsg1 = core.GetLangTx("txStatus_releaseTo", e.Effects.ToString());
                        }
                        else if (e.KeyStates.HasFlag(DragDropKeyStates.AltKey))
                        {
                            e.Effects = DragDropEffects.Link;
                            statesMsg1 = core.GetLangTx("txStatus_releaseTo", e.Effects.ToString());
                        }
                        else
                        {
                            e.Effects = DragDropEffects.None;
                            statesMsg1 = core.GetLangTx("txStatus_holdCtrlToDup");
                        }
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                        statesMsg1 = core.GetLangTx("txStatus_inSameDir");
                    }
                }
            }
            else
            {
                // 不允许将文件，拖放到文件中
                // file?
                statesMsg1 = core.GetLangTx("txStatus_notSupport");
                e.Effects = DragDropEffects.None;
            }
            rect_treeView_dropZone_finalDragDropEffect = e.Effects;
            SetStates(ref statesMsg1, ref missingStr, ref missingStr, ref missingStr, ref missingStr, ref missingStr);
            e.Handled = true;


            //显示投放的dgItem的显示外框
            if (treeView_PreviewDragOver_pointItem != null)
            {
                Rect tnMouseOverItemBoundry
                    = UI.VisualHelper.TreeView.GetNodeBoundry(
                        treeView,
                        UI.VisualHelper.TreeView.GetNodeUI(treeView,
                        treeView_PreviewDragOver_pointItem));


                rect_treeView_dropTarget.Width = tnMouseOverItemBoundry.Width - 21;
                rect_treeView_dropTarget.Height = tnMouseOverItemBoundry.Height;
                Thickness tvMargin = treeView.Margin;
                rect_treeView_dropTarget.Margin = new Thickness(
                    tvMargin.Left + tnMouseOverItemBoundry.Left + 21,
                    tvMargin.Top + tnMouseOverItemBoundry.Top,
                    0, 0);
                switch (rect_treeView_dropZone_finalDragDropEffect)
                {
                    case DragDropEffects.None:
                        DragOver_setZoneColor(rect_treeView_dropTarget, 1);
                        break;
                    case DragDropEffects.Copy:
                        DragOver_setZoneColor(rect_treeView_dropTarget, 2);
                        break;
                    case DragDropEffects.Move:
                        DragOver_setZoneColor(rect_treeView_dropTarget, 3);
                        break;
                }
                rect_treeView_dropTarget.Visibility = Visibility.Visible;
            }
            else
            {
                rect_treeView_dropTarget.Visibility = Visibility.Collapsed;
            }

        }

        private void rect_treeView_dropZone_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            rect_treeView_dropZone.Visibility = Visibility.Collapsed;
            rect_treeView_dropTarget.Visibility = Visibility.Collapsed;
        }

        private void rect_treeView_dropZone_PreviewDrop(object sender, DragEventArgs e)
        {
            rect_treeView_dropZone.Visibility = Visibility.Collapsed;
            rect_treeView_dropTarget.Visibility = Visibility.Collapsed;
            treeView_PreviewMouseDown_mouseButton = null;

            if (e.AllowedEffects == DragDropEffects.None)
                return;

            if (core.isPreventDropOnce)
            {
                core.isPreventDropOnce = false;
                return;
            }

            core.isDragingFile = false;
            core.Explorer_HandleFileDrop(
                rect_treeView_dropZone_DragSources,
                rect_treeView_dropZone_finalDragDropEffect,
                rect_treeView_dropZone_DropTarget);
        }
        private void rect_treeView_dropZone_PreviewDragLeave(object sender, DragEventArgs e)
        {
            rect_treeView_dropZone.Visibility = Visibility.Collapsed;
            rect_treeView_dropTarget.Visibility = Visibility.Collapsed;
            //treeView_PreviewMouseDown_mouseButton = null;
        }
        private void treeView_GotFocus(object sender, RoutedEventArgs e)
        {
            grid_dataGrid_dropZone.Visibility = Visibility.Collapsed;
            rect_dataGrid_dropZone.Visibility = Visibility.Collapsed;
        }


        #endregion


        #region treeview, keyboard operations

        private void treeView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F2:
                    TreeViewTryStartRename();
                    break;
                case Key.Delete:
                    if (treeView.SelectedItem is VM.TreeViewModelDir)
                        core.Explorer_Delete((VM.TreeViewModelDir)treeView.SelectedItem);
                    break;
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                object selectedTN = treeView.SelectedItem;
                if (selectedTN == null)
                    return;

                switch (e.Key)
                {
                    case Key.X:
                    case Key.C:
                        bool isCopy = e.Key == Key.C;
                        if (selectedTN is VM.TreeViewModelDir)
                        {
                            string[] files = new string[] { ((VM.TreeViewModelDir)selectedTN).dirInfo.fullName };
                            Utilities.ClipBoard.SetFileDrags(isCopy, true, files);
                            if (isCopy)
                                core.BroadcastCopy(files);
                            else
                                core.BroadcastCut(files);
                            justCopyCutFiles_forSelecting = files;
                        }
                        e.Handled = true;
                        break;
                    case Key.V:
                        core.Explorer_Paste(GetTreeNodeURL());
                        dataGrid.SelectedItems.Clear();
                        justCopyCutFiles_time_forTimeOut = DateTime.Now;
                        e.Handled = true;
                        break;
                }
            }
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                switch (e.Key)
                {
                    case Key.Delete:
                        if (treeView.SelectedItem is VM.TreeViewModelDir)
                            core.Explorer_Delete((VM.TreeViewModelDir)treeView.SelectedItem, true);
                        break;
                }
            }
        }
        private void treeView_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Apps:
                    e.Handled = true;
                    treeView_showContextMenu(treeView.SelectedItem, true);
                    break;
            }
        }

        #endregion


        #region treeview, others

        private void treeView_showContextMenu(object tvItem, bool byKey = false)
        {
            Point mPt;
            if (byKey)
                mPt = Mouse.GetPosition(treeView);

            string parentDir;
            string[] files;
            if (tvItem is VM.TreeViewModelDisk)
            {
                VM.TreeViewModelDisk disk = (VM.TreeViewModelDisk)tvItem;
                parentDir = disk.GetFullPath();
                files = null;
            }
            else if (tvItem is VM.TreeViewModelDir)
            {
                VM.TreeViewModelDir dir = (VM.TreeViewModelDir)tvItem;
                parentDir = System.IO.Path.GetDirectoryName(dir.GetFullPath());
                files = new string[] { dir.GetFullPath() };
            }
            else
            {
                treeView.ContextMenu = null;
                return;
            }
            if (byKey)
            {
                TreeViewItem ui = UI.VisualHelper.TreeView.GetNodeUI(treeView, tvItem);
                UI.VisualHelper.TreeView.BringIntoView(treeView, ui);
                if (ui != null)
                {
                    Rect nodeUIBoundry = UI.VisualHelper.TreeView.GetNodeBoundry(
                        treeView, ui);

                    core.contextMenuExplorer = this;
                    core.contextMenuCtrl = treeView;
                    core.explorerContextMenu.ShowContextMenu(
                        treeView,
                        nodeUIBoundry.X + nodeUIBoundry.Width - mPt.X,
                        nodeUIBoundry.Y + nodeUIBoundry.Height / 2 - mPt.Y,
                        parentDir, files);
                }
            }
            else
            {
                core.contextMenuExplorer = this;
                core.contextMenuCtrl = treeView;
                core.explorerContextMenu.ShowContextMenu(treeView, parentDir, files);
            }
        }

        #endregion




        #region datagrid, mouse operations



        [DllImport("user32.dll")]
        static extern uint GetDoubleClickTime();

        private Point dataGrid_PreviewMouseDown_point;
        private MouseButton? dataGrid_PreviewMouseDown_mouseButton = null;
        private DateTime dataGrid_PreviewMouseDown_time = DateTime.MinValue;
        private object dataGrid_PreviewMouseDown_pointItem;
        private bool dataGrid_PreviewMouseDown_toRename = false;
        private bool dataGrid_PreviewMouseDown_mouseMoved = false;
        private void dataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            dataGrid_PreviewMouseDown_point = Mouse.GetPosition(dataGrid);
            if (dataGrid_PreviewMouseDown_point.Y <= 23)
            {
                // header clicked
            }
            else if (dataGrid_PreviewMouseDown_point.X >= 0 && dataGrid_PreviewMouseDown_point.X < dataGrid.ActualWidth - 17
                && dataGrid_PreviewMouseDown_point.Y > 23 && dataGrid_PreviewMouseDown_point.Y < dataGrid.ActualHeight - 17)
            {
                // 点击了视图区域（非滚动条区域）
                e.Handled = true;

                //UI.OutputWindow.Output("selected item:" + debug_getDGItemInfo(dataGrid.SelectedItem));

                // 2022-10-16 超过5分钟后，鼠标按下则重新载入当前目录；
                DateTime now = DateTime.Now;
                TimeSpan timePassed = (now - dataGrid_PreviewMouseDown_time);
                if (timePassed.TotalMinutes >= 5)
                {
                    if (treeView.SelectedItem != null
                        && (treeView.SelectedItem is VM.TreeViewModelDir
                            || treeView.SelectedItem is VM.TreeViewModelDisk))
                    {
                        core.ReloadDirAsync(GetTreeNodeURL());
                    }
                }

                dataGrid_PreviewMouseDown_mouseButton = e.ChangedButton;
                bool timeOfExcute;
                // 2023-02-27 右击不允许双击执行，右击则重置时间；
                if (dataGrid_PreviewMouseDown_mouseButton == MouseButton.Left)
                {
                    timeOfExcute = timePassed.TotalMilliseconds <= GetDoubleClickTime();
                    dataGrid_PreviewMouseDown_time = now;
                }
                else
                {
                    timeOfExcute = false;
                    dataGrid_PreviewMouseDown_time = DateTime.MinValue;
                }

                dataGrid_PreviewMouseDown_pointItem = UI.VisualHelper.DataGrid.GetNodeVM(dataGrid, dataGrid_PreviewMouseDown_point, false);



                //UI.OutputWindow.Output("mouse point item:" + debug_getDGItemInfo(dataGrid_PreviewMouseDown_pointItem));

                dataGrid_PreviewMouseDown_toRename = false;
                dataGrid_PreviewMouseDown_mouseMoved = false;

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (dataGrid_PreviewMouseDown_pointItem != null)
                    {
                        if (dataGrid.SelectedItems.Contains(dataGrid_PreviewMouseDown_pointItem))
                        {
                            dataGrid.SelectedItems.Remove(dataGrid_PreviewMouseDown_pointItem);
                            dataGrid_PreviewKeyDown_isCtrlDown_dgSelection.Remove(dataGrid_PreviewMouseDown_pointItem);
                        }
                        else
                        {
                            dataGrid.SelectedItems.Add(dataGrid_PreviewMouseDown_pointItem);
                            dataGrid_PreviewKeyDown_isCtrlDown_dgSelection.Add(dataGrid_PreviewMouseDown_pointItem);
                        }
                        dataGrid_SetCurrentCell(dataGrid_PreviewMouseDown_pointItem, dataGrid_PreviewMouseDown_point.X);
                    }
                }
                else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    if (dataGrid.SelectedItems.Count == 0)
                    {
                        dataGrid.SelectedItem = dataGrid_PreviewMouseDown_pointItem;
                    }
                    else
                    {
                        // 获取当前选择的最大、最小索引（列表中的索引，不是源列表的）
                        int idxCur = dataGrid.Items.IndexOf(dataGrid_PreviewMouseDown_pointItem);
                        dataGrid_GetMinMaxSelectedIdx(out int idxMin, out int idxMax);

                        if (idxCur < idxMin)
                        {
                            // 前向选择
                            dataGrid_selectBetween(idxCur, idxMin - 1);
                        }
                        else if (idxMax < idxCur)
                        {
                            // 后向选择
                            dataGrid_selectBetween(idxMax + 1, idxCur);
                        }
                        else
                        {
                            // 点击在选取范围内，
                            // 只选择 从点击位置 到距离最远索引 之间的项目，
                            if (idxMin != idxCur && idxCur != idxMax)
                            {
                                if ((idxCur - idxMin) < (idxMax - idxCur))
                                {
                                    // 当前点偏前，前侧不选，后侧全选
                                    dataGrid_selectBetween(idxMin, idxCur - 1, false);
                                    dataGrid_selectBetween(idxCur, idxMax);
                                }
                                else
                                {
                                    // 当前点偏后，前侧全选，后侧不选
                                    dataGrid_selectBetween(idxMin, idxCur);
                                    dataGrid_selectBetween(idxCur + 1, idxMax, false);
                                }
                            }
                        }

                        dataGrid_SetCurrentCell(dataGrid_PreviewMouseDown_pointItem, dataGrid_PreviewMouseDown_point.X);
                    }
                }
                else
                {
                    if (dataGrid.SelectedItems.Count > 0)
                    {
                        if (!dataGrid.SelectedItems.Contains(dataGrid_PreviewMouseDown_pointItem))
                        {
                            dataGrid.SelectedItems.Clear();
                            dataGrid.SelectedItem = dataGrid_PreviewMouseDown_pointItem;
                            dataGrid_SetCurrentCell(dataGrid_PreviewMouseDown_pointItem, dataGrid_PreviewMouseDown_point.X);
                        }
                        else if (dataGrid.SelectedItems.Count == 1)
                        {
                            if (dataGrid.SelectedItem == dataGrid_PreviewMouseDown_pointItem)
                            {
                                if (timeOfExcute)
                                {
                                    if (dataGrid.SelectedItems.Count == 1)
                                    {
                                        dataGrid_executeItems(dataGrid.SelectedItems);
                                    }
                                    dataGrid_PreviewMouseDown_mouseButton = null;
                                }
                                else
                                {
                                    dataGrid_PreviewMouseDown_toRename = true;
                                    dataGrid_SetCurrentCell(dataGrid_PreviewMouseDown_pointItem, dataGrid_PreviewMouseDown_point.X);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (dataGrid_PreviewMouseDown_pointItem != null)
                        {
                            dataGrid.SelectedItem = dataGrid_PreviewMouseDown_pointItem;
                        }
                        dataGrid_SetCurrentCell(dataGrid_PreviewMouseDown_pointItem, dataGrid_PreviewMouseDown_point.X);
                    }
                    if (dataGrid_columns_flag == "link")
                    {
                        if (dataGrid_PreviewMouseDown_pointItem != null)
                        {
                            double col1Po = dataGrid.Padding.Left + dataGrid.Columns[0].ActualWidth;
                            double col2Po = col1Po + dataGrid.Columns[1].ActualWidth;
                            if (col1Po <= dataGrid_PreviewMouseDown_point.X && dataGrid_PreviewMouseDown_point.X <= col2Po)
                            {
                                NavigateTo(dataGrid_PreviewMouseDown_pointItem.ToString());
                            }
                        }
                    }
                }

                if (tb_rename.Visibility == Visibility.Visible)
                {
                    dataGrid_PreviewMouseDown_toRename = false;
                    DataGridRename(tb_rename.Text.Trim());
                }
            }
            else
            {
                // 点击了滚动条
            }
            dataGrid.Focus();
            flag_lastFocused_TVorDG = false;
        }
        private void dataGrid_GetMinMaxSelectedIdx(out int idxMin, out int idxMax)
        {
            idxMin = dataGridItemSource.Count; idxMax = -1;
            int idxTest;
            foreach (object i in dataGrid.SelectedItems)
            {
                idxTest = dataGrid.Items.IndexOf(i);
                if (idxTest < 0)
                    continue;
                if (idxTest < idxMin)
                    idxMin = idxTest;
                if (idxMax < idxTest)
                    idxMax = idxTest;
            }
        }
        private void dataGrid_SetCurrentCell(object item, double offsetX)
        {
            if (item == null)
                return;

            double colPo = dataGrid.Padding.Left;
            int i = 0;
            for (int iv = dataGrid.Columns.Count; i < iv; ++i)
            {
                colPo += dataGrid.Columns[i].ActualWidth;
                if (offsetX < colPo)
                {
                    break;
                }
            }
            dataGrid.CurrentCell = new DataGridCellInfo(item, dataGrid.Columns[i]);
        }

        private void dataGrid_selectBetween(int idxS, int idxE, bool toSelect = true)
        {
            if (idxS < 0 || idxE < 0)
                return;

            object curItem;
            for (int i = idxS; i <= idxE; ++i)
            {
                curItem = dataGrid.Items[i];
                if (dataGrid.SelectedItems.Contains(curItem))
                {
                    if (!toSelect)
                        dataGrid.SelectedItems.Remove(curItem);
                }
                else
                {
                    if (toSelect)
                        dataGrid.SelectedItems.Add(curItem);
                }
            }
        }

        private DateTime dataGrid_PreviewMouseUp_pre1ItemLMBTime;
        private void dataGrid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            DateTime curTime = DateTime.Now;
            if (dataGrid_PreviewMouseDown_mouseMoved)
            {
            }
            else
            {
                if (dataGrid_PreviewMouseDown_mouseButton == MouseButton.Left)
                {
                    if (dataGrid.SelectedItems.Count > 1)
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)
                            || Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                        {
                        }
                        else
                        {
                            dataGrid.SelectedItems.Clear();
                            dataGrid.SelectedItem = dataGrid_PreviewMouseDown_pointItem;
                        }
                    }
                    else if (dataGrid.SelectedItems.Count == 1)
                    {
                        object curDGItem = dataGrid.SelectedItem;
                        if (dataGrid_PreviewMouseDown_toRename
                            && (curDGItem is VM.DataGridRowModle_disk
                                || curDGItem is VM.DataGridRowModle_dirNFile))
                        {
                            DataGridRow dgRow = UI.VisualHelper.DataGrid.GetItemUI(dataGrid, curDGItem);
                            if (dgRow != null)
                            {
                                // 2024 0228 如果本次抬起时间和上次抬起时间间隔小于等于双击间隔，则执行运行文件，而不是重命名；
                                // 间隔超过双击间隔，则启用重命名功能；
                                dataGrid_PreviewMouseUp_waitRenameOrExecute(dgRow, dataGrid.SelectedItem);

                            }
                        }
                        dataGrid_PreviewMouseUp_pre1ItemLMBTime = curTime;
                    }
                    else
                    {
                        // nothing selected, do nothing
                    }
                }
                else if (dataGrid_PreviewMouseDown_mouseButton == MouseButton.Right)
                {
                    dataGrid_showContextMenu(dataGrid.SelectedItems);
                }
                else if (dataGrid_PreviewMouseDown_mouseButton == MouseButton.XButton1)
                {
                    // back
                    btn_back_Click(null, null);
                }
                else if (dataGrid_PreviewMouseDown_mouseButton == MouseButton.XButton2)
                {
                    // forward
                    btn_fore_Click(null, null);
                }
            }
            dataGrid_PreviewMouseDown_mouseButton = null;
            rect_dataGrid_selectZone.Visibility = Visibility.Collapsed;
        }
        private bool dataGrid_PreviewMouseUp_waitRenameOrExecute_inCycle = false;
        private object dataGrid_PreviewMouseUp_waitRenameOrExecute_item = null;
        private DateTime dataGrid_PreviewMouseUp_waitRenameOrExecute_activeTime = DateTime.MinValue;
        private async void dataGrid_PreviewMouseUp_waitRenameOrExecute(DataGridRow dgRow, object DGItem)
        {
            dataGrid_PreviewMouseUp_waitRenameOrExecute_activeTime = DateTime.Now;
            if (dataGrid_PreviewMouseUp_waitRenameOrExecute_inCycle)
            {
                return;
            }
            dataGrid_PreviewMouseUp_waitRenameOrExecute_inCycle = true;
            if (dataGrid_PreviewMouseUp_waitRenameOrExecute_item != null
                && dataGrid_PreviewMouseUp_waitRenameOrExecute_item != DGItem)
            {
                dataGrid_PreviewMouseUp_waitRenameOrExecute_inCycle = false;
                dataGrid_PreviewMouseUp_waitRenameOrExecute_item = DGItem;
                return;
            }
            dataGrid_PreviewMouseUp_waitRenameOrExecute_item = DGItem;

            int doubleClickTime = (int)GetDoubleClickTime();
            await Task.Delay(doubleClickTime);

            TimeSpan timePass = DateTime.Now - dataGrid_PreviewMouseUp_waitRenameOrExecute_activeTime;
            if (timePass.TotalMilliseconds <= doubleClickTime)
            {
                // active
                dataGrid_executeItems(dataGrid.SelectedItems);
            }
            else
            {
                // rename
                Rect? dgRowBoundry = UI.VisualHelper.DataGrid.GetItemBoundry(dataGrid, dgRow);
                if (dgRowBoundry != null)
                {
                    Rect dgRowBoundry1 = (Rect)dgRowBoundry;
                    double offsetX = dataGrid.Margin.Left + dgRowBoundry1.Left;
                    double col2Po = dataGrid.Margin.Left + dataGrid.Columns[0].ActualWidth;
                    if (20 + offsetX <= dataGrid_PreviewMouseDown_point.X
                        && dataGrid_PreviewMouseDown_point.X <= col2Po
                        && dgRowBoundry1.Top <= dataGrid_PreviewMouseDown_point.Y
                        && dataGrid_PreviewMouseDown_point.Y <= dgRowBoundry1.Top + dgRowBoundry1.Height)
                    {
                        DataGridTryStartRename();
                    }

                    // start rename
                    // do not edit time, do that in properties window
                }
            }
            dataGrid_PreviewMouseUp_waitRenameOrExecute_inCycle = false;
        }
        private void rect_dataGrid_selectZone_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            dataGrid_PreviewMouseDown_mouseButton = null;
            rect_dataGrid_selectZone.Visibility = Visibility.Collapsed;

            if (dataGrid_PreviewKeyDown_isCtrlDown)
            {
                dataGrid_PreviewKeyDown_isCtrlDown_dgSelection.Clear();
                foreach (object i in dataGrid.SelectedItems)
                    dataGrid_PreviewKeyDown_isCtrlDown_dgSelection.Add(i);
            }
        }

        private void dataGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (rect_dataGrid_selectZone.Visibility == Visibility.Visible)
            {
                e.Handled = true;
                // continue select items
                dataGrid_MouseMove_SelectZoneNSelect();
            }
            else if (dataGrid_PreviewMouseDown_mouseButton != null)
            {
                e.Handled = true;
                Point mp = Mouse.GetPosition(dataGrid);
                double mouseMoveXAbs = Math.Abs(mp.X - dataGrid_PreviewMouseDown_point.X);
                double mouseMoveYAbs = Math.Abs(mp.Y - dataGrid_PreviewMouseDown_point.Y);
                if (mouseMoveXAbs > 2 || mouseMoveYAbs > 2)
                {
                    if (dataGrid_PreviewMouseDown_pointItem == null)
                    {
                        // show rectangle, to select items
                        rect_dataGrid_selectZone.Width = mouseMoveXAbs;
                        rect_dataGrid_selectZone.Height = mouseMoveYAbs;
                        Thickness dgMargin = dataGrid.Margin;
                        rect_dataGrid_selectZone.Margin = new Thickness(
                            Math.Min(mp.X, dataGrid_PreviewMouseDown_point.X) + dgMargin.Left,
                            Math.Min(mp.Y, dataGrid_PreviewMouseDown_point.Y) + dgMargin.Top,
                            0, 0
                            );
                        rect_dataGrid_selectZone.Visibility = Visibility.Visible;
                        dataGrid_PreviewMouseDown_mouseMoved = true;
                    }
                    else
                    {
                        if (dataGrid.SelectedItems == null || dataGrid.SelectedItems.Count <= 0)
                            return;

                        // start draging
                        List<string> files = new List<string>();
                        object testItem = dataGrid.SelectedItems[0];
                        if (testItem is VM.DataGridRowModle_dirNFile)
                        {
                            // copy, move, link files
                            foreach (VM.DataGridRowModle_dirNFile f in dataGrid.SelectedItems)
                            {
                                files.Add(f.iois.fullName);
                            }
                            DragFiles(dataGrid, files.ToArray());
                        }
                        else
                        {
                            // link, pathes
                            if (testItem is VM.DataGridRowModle_disk)
                            {
                                foreach (VM.DataGridRowModle_disk d in dataGrid.SelectedItems)
                                {
                                    files.Add(d.diskInfo.name);
                                }
                            }
                            else if (testItem is VM.DataGridRowModle_host)
                            {
                                foreach (VM.DataGridRowModle_host h in dataGrid.SelectedItems)
                                {
                                    files.Add("\\\\" + h.name);
                                }
                            }
                            else if (testItem is VM.DataGridRowModle_link)
                            {
                                foreach (VM.DataGridRowModle_link l in dataGrid.SelectedItems)
                                {
                                    files.Add(l.link);
                                }
                            }
                            if (files.Count > 0)
                            {
                                DragFiles_link(dataGrid, files.ToArray());
                            }
                        }
                        dataGrid_PreviewMouseDown_mouseButton = null;
                    }
                }
            }
        }
        private void rect_dataGrid_selectZone_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            dataGrid_MouseMove_SelectZoneNSelect();
        }
        private void dataGrid_MouseMove_SelectZoneNSelect()
        {
            Point mp = Mouse.GetPosition(dataGrid);
            if (mp.X < 0 || mp.Y < 0
                || dataGrid.ActualWidth - 17 < mp.X || dataGrid.ActualHeight - 17 < mp.Y)
            {
                dataGrid_MouseLeave_clear();
            }

            double mouseMoveXAbs = Math.Abs(mp.X - dataGrid_PreviewMouseDown_point.X);
            double mouseMoveYAbs = Math.Abs(mp.Y - dataGrid_PreviewMouseDown_point.Y);
            double mouseMinX = Math.Min(mp.X, dataGrid_PreviewMouseDown_point.X);
            double mouseMinY = Math.Min(mp.Y, dataGrid_PreviewMouseDown_point.Y);
            rect_dataGrid_selectZone.Width = mouseMoveXAbs;
            rect_dataGrid_selectZone.Height = mouseMoveYAbs;
            Thickness dgMargin = dataGrid.Margin;
            rect_dataGrid_selectZone.Margin = new Thickness(
                mouseMinX + dgMargin.Left, mouseMinY + dgMargin.Top, 0, 0);
            double right = mouseMinX + mouseMoveXAbs;
            double bottom = mouseMinY + mouseMoveYAbs;
            double dgLeft = dataGrid.Padding.Left;
            if (right < dgLeft
                || UI.VisualHelper.DataGrid.GetTotalColsWidth(dataGrid) + dgLeft < mouseMinX)
            {
                dataGrid.SelectedItems.Clear();
            }
            else
            {
                List<object> rangeVms = UI.VisualHelper.DataGrid.GetNodesVMList(dataGrid, mouseMinY, bottom);
                if (dataGrid_PreviewKeyDown_isCtrlDown)
                {
                    // de-select from origin selection
                    object foundOI;
                    foreach (object i in rangeVms)
                    {
                        foundOI = dataGrid_PreviewKeyDown_isCtrlDown_dgSelection.Find(o => o == i);
                        if (foundOI == null)
                        {
                            if (!dataGrid.SelectedItems.Contains(i))
                                dataGrid.SelectedItems.Add(i);
                        }
                        else
                        {
                            if (dataGrid.SelectedItems.Contains(i))
                                dataGrid.SelectedItems.Remove(i);
                        }
                    }
                }
                else
                {
                    // re-select
                    dataGrid.SelectedItems.Clear();
                    foreach (object i in rangeVms)
                    {
                        dataGrid.SelectedItems.Add(i);
                    }
                }
            }
        }


        private void dataGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            //dataGrid_MouseLeave_checkClear();
        }

        private void rect_dataGrid_selectZone_MouseLeave(object sender, MouseEventArgs e)
        {
            //dataGrid_MouseLeave_checkClear();

        }
        private void dataGrid_MouseLeave_clear()
        {
            dataGrid_PreviewMouseDown_mouseButton = null;
            rect_dataGrid_selectZone.Visibility = Visibility.Collapsed;
        }
        private void dataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            rect_treeView_dropZone.Visibility = Visibility.Collapsed;
            rect_treeView_dropTarget.Visibility = Visibility.Collapsed;
        }



        private void DragFiles(Control dragFrom, params string[] files)
        {
            DragDrop.DoDragDrop(
                dragFrom,
                new DataObject(DataFormats.FileDrop, files),
                //DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
                DragDropEffects.All);
        }
        private void DragFiles_link(Control dragFrom, params string[] files)
        {
            DragDrop.DoDragDrop(
                dragFrom,
                new DataObject(DataFormats.FileDrop, files),
                DragDropEffects.Link);
        }

        private void dataGrid_PreviewDragEnter(object sender, DragEventArgs e)
        {
            //UI.OutputWindow.Output("drag enter dataGrid");
            //if (!UI.OutputWindow.HasBtn("dropText"))
            //{
            //    UI.OutputWindow.AddSetBtn("dropText", debug_dropInfo);
            //}
            dataGrid_generalPreviewDragEnter(ref e);

            grid_dataGrid_dropZone.Visibility = Visibility.Visible;
        }
        //private void debug_dropInfo()
        //{
        //    StringBuilder strBdr = new StringBuilder();
        //    strBdr.Append($"dropGrid visible:{grid_dataGrid_dropZone.Visibility}");
        //    UI.OutputWindow.Output(strBdr.ToString());
        //}
        private void grid_dataGrid_dropZone_PreviewDragEnter(object sender, DragEventArgs e)
        {
            // 设定效果后，需要e.Handled = true; 但这只是一闪而过，dragover里还会变为allowAll，需要重新设定效果；
            dataGrid_generalPreviewDragEnter(ref e);
        }
        private void dataGrid_generalPreviewDragEnter(ref DragEventArgs e)
        {
            object tnSelected = treeView.SelectedItem;
            if (tnSelected == treeViewRoot_quickAccess
                || tnSelected == treeViewRoot_network)
            {
                // do not alow to drop
                e.Effects = DragDropEffects.None;
            }
            else
            {
                // allow to drop
                // disk, host-root, dir
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effects = DragDropEffects.Move;
                else
                    e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private object dataGrid_PreviewDragOver_pointItem;

        private string[] grid_dataGrid_dropZone_DragSources;
        private string grid_dataGrid_dropZone_DropTarget;
        private DragDropEffects grid_dataGrid_dropZone_finalDragDropEffect;
        private void grid_dataGrid_dropZone_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.AllowedEffects == DragDropEffects.None)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }


            Point mp = e.GetPosition((Grid)sender);
            //mp = new Point(core.mainWindow.Left - mp.X, core.mainWindow.Top - mp.Y);
            dataGrid_PreviewDragOver_pointItem = UI.VisualHelper.DataGrid.GetNodeVM(dataGrid, mp, false);


            // 设定投放效果
            grid_dataGrid_dropZone_DragSources = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (dataGrid_PreviewDragOver_pointItem != null)
            {
                grid_dataGrid_dropZone_DropTarget = GetDataGridItemURL(dataGrid_PreviewDragOver_pointItem);
            }
            else
            {
                // 落点在空白位置，parent-dir
                grid_dataGrid_dropZone_DropTarget = GetTreeNodeURL();
            }
            string statesMsg1, missingStr = null;
            if (Directory.Exists(grid_dataGrid_dropZone_DropTarget))
            {
                if (MouseNKeyboardHelper.CheckFileDrops(
                    grid_dataGrid_dropZone_DragSources,
                    grid_dataGrid_dropZone_DropTarget,
                    out bool withinSameRoot, out bool withinSameDir, out Exception err))
                {
                    DragDropEffects defaultDDE = DragDropEffects.None;
                    if (withinSameDir)
                    {
                        // 将文件向其所在文件夹中拖放时，默认不允许
                        // 不过按住ctrl，可以复制，或alt创建连接
                        //e.Effects = DragDropEffects.None;
                    }
                    else if (withinSameRoot)
                    {
                        defaultDDE = DragDropEffects.Move;
                    }
                    else
                    {
                        defaultDDE = DragDropEffects.Copy;
                    }
                    e.Effects = MouseNKeyboardHelper.GetDemandDragDropEffect(e.KeyStates, e.AllowedEffects, defaultDDE);
                    statesMsg1 = core.GetLangTx("txStatus_releaseTo", e.Effects.ToString());
                }
                else
                {
                    if (withinSameDir)
                    {
                        if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                        {
                            e.Effects = DragDropEffects.Copy;
                            statesMsg1 = core.GetLangTx("txStatus_releaseTo", e.Effects.ToString());
                        }
                        else if (e.KeyStates.HasFlag(DragDropKeyStates.AltKey))
                        {
                            e.Effects = DragDropEffects.Link;
                            statesMsg1 = core.GetLangTx("txStatus_releaseTo", e.Effects.ToString());
                        }
                        else
                        {
                            e.Effects = DragDropEffects.None;
                            statesMsg1 = core.GetLangTx("txStatus_holdCtrlToDup");
                        }
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                        statesMsg1 = core.GetLangTx("txStatus_inSameDir");
                    }
                }
            }
            else
            {
                // 不允许将文件，拖放到文件中
                // file?
                statesMsg1 = core.GetLangTx("txStatus_cantDragToFile");
                e.Effects = DragDropEffects.None;
            }
            grid_dataGrid_dropZone_finalDragDropEffect = e.Effects;
            SetStates(ref statesMsg1, ref missingStr, ref missingStr, ref missingStr, ref missingStr, ref missingStr);
            e.Handled = true;


            //显示投放的dgItem的显示外框
            bool toCollapse = true;
            if (dataGrid_PreviewDragOver_pointItem != null)
            {
                System.Windows.Controls.DataGridRow dgRow = UI.VisualHelper.DataGrid.GetItemUI(dataGrid, dataGrid_PreviewDragOver_pointItem);
                if (dgRow != null)
                {
                    Rect? dgMouseOverItemBoundry = UI.VisualHelper.DataGrid.GetItemBoundry(dataGrid, dgRow);
                    if (dgMouseOverItemBoundry != null)
                    {
                        Rect dgMouseOverItemBoundry1 = (Rect)dgMouseOverItemBoundry;
                        rect_dataGrid_dropZone.Width = dgMouseOverItemBoundry1.Width;
                        rect_dataGrid_dropZone.Height = dgMouseOverItemBoundry1.Height;
                        Thickness dgMargin = dataGrid.Margin;
                        rect_dataGrid_dropZone.Margin = new Thickness(
                            dgMargin.Left + dgMouseOverItemBoundry1.Left,
                            dgMargin.Top + dgMouseOverItemBoundry1.Top,
                            0, 0);
                        switch (grid_dataGrid_dropZone_finalDragDropEffect)
                        {
                            case DragDropEffects.None:
                                DragOver_setZoneColor(rect_dataGrid_dropZone, 1);
                                break;
                            case DragDropEffects.Copy:
                                DragOver_setZoneColor(rect_dataGrid_dropZone, 2);
                                break;
                            case DragDropEffects.Move:
                                DragOver_setZoneColor(rect_dataGrid_dropZone, 3);
                                break;
                        }
                        rect_dataGrid_dropZone.Visibility = Visibility.Visible;
                        toCollapse = false;
                    }
                }
            }
            if (toCollapse)
            {
                rect_dataGrid_dropZone.Visibility = Visibility.Collapsed;
            }
        }

        private void grid_dataGrid_dropZone_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            grid_dataGrid_dropZone.Visibility = Visibility.Collapsed;
            rect_dataGrid_dropZone.Visibility = Visibility.Collapsed;
        }


        /// <summary>
        /// 设定矩形的填充颜色
        /// </summary>
        /// <param name="rectZone">被设定颜色的矩形</param>
        /// <param name="clrIdx">颜色索引，1-红色；2-绿色；3-蓝色；</param>
        private void DragOver_setZoneColor(Rectangle rectZone, int clrIdx)
        {
            switch (clrIdx)
            {
                case 1:
                    rectZone.Fill = DragOver_zoneColor1;
                    break;
                case 2:
                    rectZone.Fill = DragOver_zoneColor2;
                    break;
                case 3:
                    rectZone.Fill = DragOver_zoneColor3;
                    break;
            }
        }
        private SolidColorBrush DragOver_zoneColor1 = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
        private SolidColorBrush DragOver_zoneColor2 = new SolidColorBrush(Color.FromArgb(100, 0, 255, 0));
        private SolidColorBrush DragOver_zoneColor3 = new SolidColorBrush(Color.FromArgb(100, 0, 0, 255));

        private void dataGrid_PreviewDragLeave(object sender, DragEventArgs e)
        {
            // 当dropZone显示后，会触发此事件，导致enter和leave循环触发；
            // so, do nothing
            //UI.OutputWindow.Output("drag leave dataGrid");
            //grid_dataGrid_dropZone.Visibility = Visibility.Collapsed;
        }
        private void grid_dataGrid_dropZone_PreviewDragLeave(object sender, DragEventArgs e)
        {
            grid_dataGrid_dropZone.Visibility = Visibility.Collapsed;
            rect_dataGrid_dropZone.Visibility = Visibility.Collapsed;
        }

        private void dataGrid_PreviewDrop(object sender, DragEventArgs e)
        {
            // datagrid fully covered by drop-zone, will never triger
        }
        private void grid_dataGrid_dropZone_PreviewDrop(object sender, DragEventArgs e)
        {
            grid_dataGrid_dropZone.Visibility = Visibility.Collapsed;
            rect_dataGrid_dropZone.Visibility = Visibility.Collapsed;

            if (e.AllowedEffects == DragDropEffects.None)
                return;

            if (core.isPreventDropOnce)
            {
                core.isPreventDropOnce = false;
                return;
            }

            core.isDragingFile = false;
            core.Explorer_HandleFileDrop(
                grid_dataGrid_dropZone_DragSources,
                grid_dataGrid_dropZone_finalDragDropEffect,
                grid_dataGrid_dropZone_DropTarget);
        }

        #endregion


        #region datagrid, keyboard operations

        private bool dataGrid_PreviewKeyDown_isCtrlDown = false;
        private List<object> dataGrid_PreviewKeyDown_isCtrlDown_dgSelection = new List<object>();
        private async void dataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isShiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool isCtrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            switch (e.Key)
            {
                #region F2-rename, F4-navigateBar, F5-refresh, Enter-activate, delete, back, forward
                case Key.F2: // rename
                    {
                        object curDGItem = dataGrid.CurrentCell.Item;
                        string sortMemPath = dataGrid.CurrentCell.Column.SortMemberPath;
                        if (sortMemPath == dataGrid_colSortMemberPath_name)
                        {
                            if (curDGItem is VM.DataGridRowModle_dirNFile)
                            {
                                DataGridTryStartRename();
                            }
                            else if (curDGItem is VM.DataGridRowModle_disk
                                && dataGrid.SelectedItems.Count == 1)
                            {
                                DataGridTryStartRename();
                            }
                        }
                    }
                    break;
                case Key.F4: // focus address bar
                             // cancel edit, then click address drop down
                    navigateBar.FocusEdit();
                    break;
                case Key.F5:
                    core.ReloadDirAsync(GetTreeNodeURL());
                    break;
                case Key.Enter: // execute, or go into dir
                    e.Handled = true;
                    dataGrid_executeItems(dataGrid.SelectedItems);
                    break;
                case Key.Back:
                case Key.BrowserBack:
                    if (btn_up.IsEnabled)
                        btn_up_Click(null, null);
                    e.Handled = true;
                    break;
                case Key.Delete:
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                        core.Explorer_Delete(dataGrid.SelectedItems, true);
                    else
                        core.Explorer_Delete(dataGrid.SelectedItems);
                    break;
                #endregion

                #region ctrl, remember selection
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    if (!dataGrid_PreviewKeyDown_isCtrlDown)
                    {
                        dataGrid_PreviewKeyDown_isCtrlDown = true;
                        dataGrid_PreviewKeyDown_isCtrlDown_dgSelection.Clear();
                        foreach (object i in dataGrid.SelectedItems)
                            dataGrid_PreviewKeyDown_isCtrlDown_dgSelection.Add(i);
                    }
                    break;
                #endregion

                #region ctrl x, c, v
                case Key.X:
                case Key.C:
                    {
                        if (!isCtrlDown)
                            break;
                        if (dataGrid.SelectedItems != null && dataGrid.SelectedItems.Count > 0)
                        {
                            bool isCopy = e.Key == Key.C;
                            // 提取文件；
                            List<string> sourceFileList = new List<string>();
                            List<object> oriSelectedItems = new List<object>();
                            object testI = dataGrid.SelectedItems[0];
                            if (testI is VM.DataGridRowModle_dirNFile)
                            {
                                VM.DataGridRowModle_dirNFile curI;
                                foreach (object i in dataGrid.SelectedItems)
                                {
                                    oriSelectedItems.Add(i);
                                    curI = (VM.DataGridRowModle_dirNFile)i;
                                    sourceFileList.Add(curI.iois.fullName);
                                }
                            }
                            if (sourceFileList.Count > 0)
                            {
                                // 放入剪贴板；
                                string[] files = sourceFileList.ToArray();
                                Utilities.ClipBoard.SetFileDrags(isCopy, true, files);

                                dataGrid_delayRefreshAsync();
                                dataGrid.Focus();
                                // 当使用复制，或刷新后，则取消之前的透明效果；

                                dataGrid.SelectedItems.Clear();
                                foreach (object i in oriSelectedItems)
                                {
                                    dataGrid.SelectedItems.Add(i);
                                }

                                if (isCopy)
                                    core.BroadcastCopy(files);
                                else
                                    core.BroadcastCut(files);
                                justCopyCutFiles_forSelecting = files;
                            }
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.V:
                    {
                        if (!isCtrlDown)
                            break;
                        if (dataGrid.Items != null && dataGrid.Items.Count > 0
                            && dataGrid.Items[0] is VM.DataGridRowModle_dirNFile)
                        {
                            VM.DataGridRowModle_dirNFile curI;
                            foreach (object i in dataGrid.Items)
                            {
                                curI = (VM.DataGridRowModle_dirNFile)i;
                                if (curI.iois.attributes.hidden)
                                    curI.iconOpacity = 0.5d;
                                else
                                    curI.iconOpacity = 1d;
                            }
                            dataGrid_delayRefreshAsync();
                            dataGrid.Focus();
                        }
                        dataGrid.SelectedItems.Clear();
                        justCopyCutFiles_time_forTimeOut = DateTime.Now;
                        e.Handled = true;
                        core.Explorer_Paste(GetTreeNodeURL());
                    }
                    break;
                #endregion

                #region arrow, up down, +shift-multiSelect, left right currentCell left right
                case Key.Up:
                    switch (isShiftDown, isCtrlDown)
                    {
                        case (false, false): // select one up
                            dataGrid_selectRowOffset(true);
                            e.Handled = true;
                            break;
                        case (true, false): // multi select +1 up
                            dataGrid_GetMinMaxSelectedIdx(out int idxMin, out int idxMax);
                            if (idxMin > 0)
                            {
                                dataGrid_selectBetween(idxMin - 1, idxMax);
                            }
                            e.Handled = true;
                            break;
                    }
                    break;
                case Key.Down:
                    switch (isShiftDown, isCtrlDown)
                    {
                        case (false, false): // select one up
                            dataGrid_selectRowOffset(false);
                            e.Handled = true;
                            break;
                        case (true, false): // multi select +1 up
                            dataGrid_GetMinMaxSelectedIdx(out int idxMin, out int idxMax);
                            if (idxMax < dataGrid.Items.Count - 1)
                            {
                                dataGrid_selectBetween(idxMin, idxMax + 1);
                            }
                            e.Handled = true;
                            break;
                    }
                    break;
                case Key.Left:
                    dataGrid_curCellOffset(true);
                    e.Handled = true;
                    break;
                case Key.Right:
                    dataGrid_curCellOffset(false);
                    e.Handled = true;
                    break;
                    #endregion
            }
            void dataGrid_curCellOffset(bool goLeftOrRight)
            {
                if (dataGrid.CurrentCell.Column == null)
                    return;
                int curCDI = dataGrid.CurrentCell.Column.DisplayIndex;
                if (goLeftOrRight && curCDI == 0)
                    return;
                int colCount = dataGrid.Columns.Count;
                if (!goLeftOrRight && colCount - 1 <= curCDI)
                    return;
                int tarCDI;
                tarCDI = curCDI + (goLeftOrRight ? -1 : 1);
                //tarCDI = curCDI;
                foreach (DataGridColumn c in dataGrid.Columns)
                {
                    if (c.DisplayIndex == tarCDI)
                    {
                        dataGrid.CurrentCell = new DataGridCellInfo(dataGrid.CurrentCell.Item, c);
                        break;
                    }
                }
            }
            void dataGrid_selectRowOffset(bool goUpOrDown)
            {
                object curItem = dataGrid.SelectedItem;
                if (curItem == null)
                {
                    // 2024 0205 如果没有选中项目，则默认选中上次选择的位置的项目；
                    int idxToSelect;
                    if (dataGrid_SelectionChanged_itemIndexPre1 >= 0)
                    {
                        idxToSelect = dataGrid_SelectionChanged_itemIndexPre1;
                    }
                    else
                    {
                        idxToSelect = 0;
                    }
                    if (dataGrid.Items.Count > 0)
                    {
                        if (idxToSelect >= dataGrid.Items.Count)
                        {
                            idxToSelect = dataGrid.Items.Count - 1;
                        }
                        else if (goUpOrDown && idxToSelect > 0)
                        {
                            --idxToSelect;
                        }
                        dataGrid.SelectedIndex = idxToSelect;
                    }

                    return;
                }
                int curRI = dataGrid.Items.IndexOf(curItem);
                if (goUpOrDown && curRI == 0)
                    return;
                int rowCount = dataGrid.Items.Count;
                if (!goUpOrDown && rowCount - 1 <= curRI)
                    return;
                int tarRI = curRI + (goUpOrDown ? -1 : 1);
                dataGrid.SelectedItem = dataGrid.Items[tarRI];
                dataGrid.ScrollIntoView(dataGrid.SelectedItem);
            }
        }
        private void dataGrid_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;


            switch (e.Key)
            {
                case Key.Apps:
                    e.Handled = true;
                    dataGrid_showContextMenu(dataGrid.SelectedItems, true);
                    break;
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    dataGrid_PreviewKeyDown_isCtrlDown = false;
                    dataGrid_PreviewKeyDown_isCtrlDown_dgSelection.Clear();
                    break;
            }
        }

        #endregion


        #region datagrid, exec, rename, contextmenu, etc.


        private async void dataGrid_showContextMenu(IList selectedItems, bool byKey = false)
        {
            Point mPt;
            if (byKey)
                mPt = Mouse.GetPosition(dataGrid);

            string parentDir;
            List<string> files = new List<string>();

            #region get parentDir N' files, or return

            object tvItem = treeView.SelectedItem;
            if (tvItem == treeViewRoot_thisPC)
            {
                if (selectedItems != null && selectedItems.Count == 1)
                {
                    VM.DataGridRowModle_disk disk = (VM.DataGridRowModle_disk)selectedItems[0];
                    parentDir = disk.diskInfo.name;
                    files.Clear();
                }
                else
                {
                    treeView.ContextMenu = null;
                    return;
                }
            }
            else if (tvItem is VM.TreeViewModelDisk)
            {
                parentDir = ((VM.TreeViewModelDisk)tvItem).GetFullPath();
                VM.DataGridRowModle_dirNFile dgItem;
                foreach (object o in selectedItems)
                {
                    if (o is VM.DataGridRowModle_dirNFile)
                    {
                        dgItem = (VM.DataGridRowModle_dirNFile)o;
                        files.Add(dgItem.iois.fullName);
                    }
                }
            }
            else if (tvItem is VM.TreeViewModelDir)
            {
                parentDir = ((VM.TreeViewModelDir)tvItem).GetFullPath();
                VM.DataGridRowModle_dirNFile dgItem;
                foreach (object o in selectedItems)
                {
                    if (o is VM.DataGridRowModle_dirNFile)
                    {
                        dgItem = (VM.DataGridRowModle_dirNFile)o;
                        files.Add(dgItem.iois.fullName);
                    }
                }
            }
            else
            {
                treeView.ContextMenu = null;
                return;
            }

            #endregion

            if (byKey)
            {
                if (selectedItems != null && selectedItems.Count >= 1)
                {
                    // 滚动到底部，随后选中第一个选中的项目，对这个项目显示菜单；
                    UI.VisualHelper.DataGrid.ScrollIntoView(
                        dataGrid,
                        dataGridItemSource[dataGridItemSource.Count - 1],
                        dataGrid.SelectedItems[0]);

                    DataGridRow rowUI = UI.VisualHelper.DataGrid.GetItemUI(dataGrid, selectedItems[0]);
                    Rect? rowBoundry = UI.VisualHelper.DataGrid.GetItemBoundry(dataGrid, rowUI);

                    if (rowBoundry != null)
                    {
                        Rect rowBoundry1 = (Rect)rowBoundry;
                        core.contextMenuExplorer = this;
                        core.contextMenuCtrl = dataGrid;
                        core.explorerContextMenu.ShowContextMenu(
                            dataGrid,
                            rowBoundry1.X + dataGrid.Columns[0].ActualWidth - mPt.X,
                            rowBoundry1.Y + rowBoundry1.Height / 2 - mPt.Y,
                            parentDir,
                            files.ToArray());
                    }
                }
                else
                {
                    // 滚动到最底部，在空白处出现菜单；
                    if (dataGridItemSource.Count > 0)
                    {
                        UI.VisualHelper.DataGrid.ScrollIntoView(
                            dataGrid,
                            dataGridItemSource[dataGridItemSource.Count - 1],
                            null);
                    }

                    core.contextMenuExplorer = this;
                    core.contextMenuCtrl = dataGrid;
                    core.explorerContextMenu.ShowContextMenu(
                        dataGrid,
                        10 - mPt.X,
                        dataGrid.ActualHeight - dataGrid.ColumnHeaderHeight - mPt.Y,
                        parentDir,
                        null);
                }
            }
            else
            {
                core.contextMenuExplorer = this;
                core.contextMenuCtrl = dataGrid;
                core.explorerContextMenu.ShowContextMenu(dataGrid, parentDir, files.ToArray());
            }
        }

        #endregion



        #region States

        public void SetBusy(bool useStartingCursor = true, bool useGlassPanel = true)
        {
            if (useGlassPanel)
                rect_topGlass.Visibility = Visibility.Visible;
            else
                rect_topGlass.Visibility = Visibility.Collapsed;

            if (useStartingCursor)
                this.Cursor = Cursors.AppStarting;
            else
                this.Cursor = Cursors.Arrow;
        }

        public void SetStates(ref string msg1, ref string msg2, ref string msg3, ref string sub1, ref string sub2, ref string sub3)
        {
            tb_state_l1.Text = msg1;
            tb_state_l2.Text = msg2;
            tb_state_l3.Text = msg3;
            tb_state_r1.Text = sub1;
            tb_state_r2.Text = sub2;
            tb_state_r3.Text = sub3;
        }
        public void SetStatesSelection()
        {
            List<VM.DataGridRowModle_disk> discs = new List<VM.DataGridRowModle_disk>();
            List<VM.DataGridRowModle_dirNFile> dirs = new List<VM.DataGridRowModle_dirNFile>();
            List<VM.DataGridRowModle_dirNFile> files = new List<VM.DataGridRowModle_dirNFile>();
            List<VM.DataGridRowModle_host> hosts = new List<VM.DataGridRowModle_host>();
            List<VM.DataGridRowModle_link> links = new List<VM.DataGridRowModle_link>();

            #region make lists
            if (dataGrid.SelectedItems.Count == 0)
            {
                foreach (object o in dataGrid.Items)
                {
                    AppendToList(o);
                }
            }
            else
            {
                foreach (object o in dataGrid.SelectedItems)
                {
                    AppendToList(o);
                }
            }

            void AppendToList(object o)
            {
                if (o is VM.DataGridRowModle_disk)
                    discs.Add((VM.DataGridRowModle_disk)o);
                else if (o is VM.DataGridRowModle_host)
                    hosts.Add((VM.DataGridRowModle_host)o);
                else if (o is VM.DataGridRowModle_link)
                    links.Add((VM.DataGridRowModle_link)o);
                else if (o is VM.DataGridRowModle_dirNFile)
                {
                    VM.DataGridRowModle_dirNFile df = (VM.DataGridRowModle_dirNFile)o;
                    if (df.iois.wasFile)
                        files.Add(df);
                    else
                        dirs.Add(df);
                }
            }
            #endregion

            Core core = Core.GetInstance();
            string msg1 = null, msg2 = null, msg3 = null, sub1 = null, sub2 = null, sub3 = null;
            if (dirs.Count > 0 && files.Count > 0)
            {
                // X dirs, x files      Total x MB
                msg1 = core.GetLangTx("txStatus_someDirs", dirs.Count.ToString());
                msg2 = core.GetLangTx("txStatus_someFiles", files.Count.ToString());
                sub1 = GetTotalFileSize();
            }
            else if (dirs.Count > 0)
            {
                if (dirs.Count == 1)
                {
                    // [D]name            modifyTime
                    msg1 = core.GetLangTx("txStatus_dirPref", dirs[0].name);
                    sub1 = TimeToString(ref dirs[0].iois.lastWriteTime);
                }
                else
                {
                    // X dirs
                    msg1 = core.GetLangTx("txStatus_someDirs", dirs.Count.ToString());
                }
            }
            else if (files.Count > 0)
            {
                if (files.Count == 1)
                {
                    // [F]name            size   att   modifyTime
                    msg1 = core.GetLangTx("txStatus_filePref", files[0].name);
                    sub1 = SimpleStringHelper.UnitsOfMeasure.GetShortString(files[0].iois.length, "B", 1024);
                    sub2 = files[0].iois.attributes.ToShortString7();
                    sub3 = TimeToString(ref files[0].iois.lastWriteTime);
                }
                else
                {
                    // X files            Total x MB
                    msg1 = core.GetLangTx("txStatus_someFiles", files.Count.ToString());
                    sub1 = GetTotalFileSize();
                }
            }
            else if (discs.Count > 0)
            {
                if (discs.Count == 1)
                {
                    VM.DataGridRowModle_disk d = discs[0];
                    msg1 = $"{d.diskType}";
                    msg2 = $"{d.diskFormat}";
                    msg3 = $"{d.name}";
                    sub1 = core.GetLangTx("txStatus_capNUsed",
                        d.totalSizeTxN,
                        d.totalSizeTxU,
                        SimpleStringHelper.UnitsOfMeasure.GetShortString(d.totalSize - d.freeSpace, "B", 1024));
                    sub2 = core.GetLangTx("txStatus_free",
                        ((double)d.freeSpace / d.totalSize).ToString("P2"));
                    sub3 = core.GetLangTx("txStatus_available",
                        SimpleStringHelper.UnitsOfMeasure.GetShortString(d.diskInfo.availableFreeSpace, "B", 1024));

                }
                else
                {
                    msg1 = core.GetLangTx("txStatus_someDisks", discs.Count.ToString());
                }
            }
            else if (hosts.Count > 0)
            {
                if (hosts.Count == 1)
                {
                    VM.DataGridRowModle_host h = hosts[0];
                    msg1 = h.name;
                    sub1 = h.hostIPv4;
                    sub2 = h.hostIPv6;
                }
                else
                {
                    msg1 = core.GetLangTx("txStatus_someHosts", hosts.Count.ToString());
                }
            }
            else if (links.Count > 0)
            {
                if (links.Count == 1)
                {
                    VM.DataGridRowModle_link l = links[0];
                    msg1 = $"{l.name}";
                    msg2 = $"{l.link}";
                }
                else
                {
                    msg1 = core.GetLangTx("txStatus_someLinks", links.Count.ToString());
                }
            }
            SetStates(ref msg1, ref msg2, ref msg3, ref sub1, ref sub2, ref sub3);

            string GetTotalFileSize()
            {
                long ts = 0;
                foreach (VM.DataGridRowModle_dirNFile f in files)
                    ts += f.fileSize;
                return SimpleStringHelper.UnitsOfMeasure.GetShortString(ts, "B", 1024);
            }
            string TimeToString(ref DateTime time)
            {
                return time.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        #endregion


        #region after copy/cut, select

        public string[] justCopyCutFiles_forSelecting;
        public DateTime justCopyCutFiles_time_forTimeOut = DateTime.MinValue;
        public int justCopyCutFiles_timeOutSec = 5;
        private async void TrySelectCopiedCutFiles(string curFile)
        {
            if ((DateTime.Now - justCopyCutFiles_time_forTimeOut).TotalSeconds > justCopyCutFiles_timeOutSec)
            {
                justCopyCutFiles_forSelecting = null;
                return;
            }
            if (justCopyCutFiles_forSelecting == null || justCopyCutFiles_forSelecting.Length == 0)
                return;
            if (dataGrid.Items == null || dataGrid.Items.Count == 0)
                return;
            if (!(dataGrid.Items[0] is VM.DataGridRowModle_dirNFile))
                return;
            string dir = System.IO.Path.GetDirectoryName(core.justCopyCutFiles[0]);
            if (navigateBar.TextboxURL != dir)
                return;

            // 检查当前文件是否在复制、剪切清单中
            bool isFile = File.Exists(curFile);
            string fileNameExt = null, curFilePre = null;
            if (isFile)
            {
                fileNameExt = System.IO.Path.GetExtension(curFile);
                curFilePre = curFile.Substring(0, curFile.Length - fileNameExt.Length);
            }
            bool doNotSelect = true;
            foreach (string fullName in justCopyCutFiles_forSelecting)
            {
                if (isFile)
                {
                    if (curFilePre.StartsWith(fullName.Substring(0, fullName.Length - fileNameExt.Length)))
                    {
                        doNotSelect = false;
                        break;
                    }
                }
                else if (curFile.StartsWith(fullName))
                {
                    doNotSelect = false;
                    break;
                }
            }
            if (doNotSelect)
                return;


            // 找到并选中新粘贴的项目；
            string testFullName;
            VM.DataGridRowModle_dirNFile curVM = null;
            foreach (VM.DataGridRowModle_dirNFile vm in dataGrid.ItemsSource)
            {
                if (vm.iois.fullName == curFile)
                {
                    if (!dataGrid.SelectedItems.Contains(vm))
                    {
                        dataGrid.SelectedItems.Add(vm);
                        curVM = vm;
                    }
                    break;
                }
            }
            //await dataGrid_delayRefreshAsync();

            // 找到最下面一个item，滚动入视野；
            if (curVM != null)
                UI.VisualHelper.DataGrid.ScrollIntoView(dataGrid, null, curVM);
        }


        #endregion


        #region rename after new file\dir

        public string newFileToRename = null;
        private bool TryRenameNewFile()
        {
            if (newFileToRename != null)
            {
                if (dataGridItemSource != null && dataGridItemSource.Count > 0
                    && dataGridItemSource[0] is VM.DataGridRowModle_dirNFile)
                {
                    foreach (VM.DataGridRowModle_dirNFile dgItem in dataGridItemSource)
                    {
                        if (dgItem.iois.fullName == newFileToRename)
                        {
                            dataGrid.SelectedItem = dgItem;
                            dataGrid.ScrollIntoView(dgItem);
                            TryStartRename(true);
                            break;
                        }
                    }
                }
                newFileToRename = null;
                return true;
            }
            return false;
        }

        #endregion


        #region rename

        private string TryStartRename_namePre;
        internal bool DataGridTryStartRename()
        {
            return TryStartRename(true);
        }
        internal bool TreeViewTryStartRename(bool useMousePointNode = false)
        {
            return TryStartRename(false, useMousePointNode);
        }

        private bool TryStartRename_isDataGrid_orTreeView;

        internal bool TryStartRename(bool isDataGrid_orTreeView, bool useMousePointTreeNode = false)
        {
            DataGridRename_preSelectedItems.Clear();
            TreeViewRename_preSelectedNode = null;

            TryStartRename_isDataGrid_orTreeView = isDataGrid_orTreeView;
            object dgItem = null;
            object tvItem = useMousePointTreeNode ? treeView_RMB_pointNode : treeView.SelectedItem;
            if (isDataGrid_orTreeView)
            {
                // 当没有选中列表项目，或者选中的单元格不是名称列的单元格，则返回false
                if (dataGrid.SelectedItems.Count <= 0 || dataGrid.Columns[0].SortMemberPath != dataGrid_colSortMemberPath_nameForSorting)
                    return false;
                dgItem = dataGrid.SelectedItems[0];
                if (dgItem is VM.DataGridRowModle_disk
                    || dgItem is VM.DataGridRowModle_dirNFile)
                { }
                else
                { return false; }

                DataGridRename_startingRenameItem = dataGrid.CurrentCell.Item;
                foreach (object o in dataGrid.SelectedItems)
                {
                    DataGridRename_preSelectedItems.Add(o);
                }
            }
            else
            {
                // 当没有选择树节点，或者节点是不可重命名的，则返回false
                if (tvItem == null)
                {
                    return false;
                }
                else if (tvItem == treeViewRoot_quickAccess
                    || tvItem == treeViewRoot_thisPC
                    || tvItem == treeViewRoot_network)
                {
                    return false;
                }
                else if (tvItem is VM.TreeViewModelLink
                    || tvItem is VM.TreeViewModelHost)
                {
                    return false;
                }
                else if (treeView.SelectedItem is VM.TreeViewModelDir)
                {
                    VM.TreeViewModelDir testTN = (VM.TreeViewModelDir)tvItem;
                    if (testTN.dirInfo.fullName.StartsWith("\\\\"))
                    {
                        if (testTN.dirInfo.fullName == Utilities.FilePath.GetUNCRootName(testTN.dirInfo.fullName))
                            return false;
                    }
                }

                TreeViewRename_preSelectedNode = treeView.SelectedItem;
            }

            // 显示重命名 输入框，聚焦
            // 当焦点丢失，或按下回车，或视图发生滚动，确认重命名
            // 当按下esc，取消重命名

            if (isDataGrid_orTreeView)
            {
                DataGridRow dgRow = UI.VisualHelper.DataGrid.GetItemUI(dataGrid, dgItem);
                Rect? dgRowBoundry = UI.VisualHelper.DataGrid.GetItemBoundry(dataGrid, dgRow);
                if (dgRowBoundry != null)
                {
                    Rect dgRowBoundry1 = (Rect)dgRowBoundry;
                    Thickness dgMargin = dataGrid.Margin;
                    Grid.SetColumn(tb_rename, 1);
                    tb_rename.Margin = new Thickness(
                        dgMargin.Left + dgRowBoundry1.Left + 21,
                        dgMargin.Top + dgRowBoundry1.Top + 2,
                        0, 0
                        );
                    tb_rename.Width = dataGrid.Columns[0].ActualWidth - 20;
                    if (dgItem is VM.DataGridRowModle_disk)
                    {
                        VM.DataGridRowModle_disk diskItem = (VM.DataGridRowModle_disk)dgItem;
                        tb_rename.Text = diskItem.diskInfo.volumeLabel;
                    }
                    else
                    {
                        VM.DataGridRowModle_dirNFile dirItem = (VM.DataGridRowModle_dirNFile)dgItem;
                        tb_rename.Text = dirItem.iois.name;
                    }
                }
            }
            else
            {
                TreeViewItem tnUI = UI.VisualHelper.TreeView.GetNodeUI(treeView, tvItem);
                Rect tnUIBoundry = UI.VisualHelper.TreeView.GetNodeBoundry(treeView, tnUI);
                Thickness tvMargin = treeView.Margin;
                Grid.SetColumn(tb_rename, 0);
                tb_rename.Margin = new Thickness(
                    tvMargin.Left + tnUIBoundry.Left + 38,
                    tvMargin.Top + tnUIBoundry.Top,
                    0, 0
                    );
                tb_rename.Width = tnUIBoundry.Width - 28;
                if (tvItem is VM.TreeViewModelDisk)
                {
                    VM.TreeViewModelDisk tnDisk = (VM.TreeViewModelDisk)tvItem;
                    tb_rename.Text = tnDisk.diskInfo.volumeLabel;
                }
                else
                {
                    VM.TreeViewModelDir tnDir = (VM.TreeViewModelDir)tvItem;
                    tb_rename.Text = tnDir.dirInfo.name;
                }
            }

            flag_lastFocused_TVorDG = null;

            TryStartRename_namePre = tb_rename.Text;
            tb_rename.SelectionStart = 0;
            int dotIdx = TryStartRename_namePre.LastIndexOf('.');
            if (dotIdx >= 0)
                tb_rename.SelectionLength = dotIdx;
            else
                tb_rename.SelectionLength = TryStartRename_namePre.Length;
            tb_rename.Visibility = Visibility.Visible;
            tb_rename.Focus();
            return true;
        }
        private async void tb_rename_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    e.Handled = true;
                    await Task.Delay(10);
                    if (TryStartRename_isDataGrid_orTreeView)
                        DataGridRename(tb_rename.Text.Trim());
                    else
                        TreeViewRename(tb_rename.Text.Trim());
                    break;
                case Key.Escape:
                    e.Handled = true;
                    await Task.Delay(10);
                    tb_rename.Visibility = Visibility.Collapsed;
                    ppu_text.IsOpen = false;
                    dataGrid.Focus();
                    break;
            }
        }

        private void tb_rename_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TryStartRename_isDataGrid_orTreeView)
                DataGridRename(tb_rename.Text.Trim());
            else
                TreeViewRename(tb_rename.Text.Trim());
        }
        private bool tb_rename_selfSetting = false;
        private void tb_rename_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tb_rename_selfSetting)
            {
                tb_rename_selfSetting = false;
                return;
            }

            if (tb_rename.Visibility == Visibility.Visible)
            {
                string testStr = tb_rename.Text.Trim();
                if (!Utilities.Checker.CheckIOName(testStr))
                {
                    tb_rename_selfSetting = true;
                    tb_rename.Text = testStr.Replace("/", "").Replace("\\", "").Replace("\"", "")
                        .Replace("?", "").Replace("*", "").Replace(":", "").Replace("|", "").Replace("<", "").Replace(">", "");
                    string txIllegalChars = "/ \\ \" ? * : | < >";
                    tb_ppu_text.Text = core.GetLangTx("txRename_illegalChars", txIllegalChars);
                    Thickness tbRenameMargin = tb_rename.Margin;
                    ppu_text.Margin = new Thickness(
                        tbRenameMargin.Left,
                        tbRenameMargin.Top + tb_rename.ActualHeight + 3,
                        0, 0
                        );
                    ppu_text.IsOpen = true;
                }
                else
                {
                    ppu_text.IsOpen = false;
                }
            }
        }
        private object DataGridRename_startingRenameItem;
        private List<object> DataGridRename_preSelectedItems = new List<object>();
        private void DataGridRename(string newName)
        {
            ppu_text.IsOpen = false;
            if (tb_rename.Visibility == Visibility.Visible)
            {
                tb_rename.Visibility = Visibility.Collapsed;

                if (TryStartRename_namePre == newName
                    && DataGridRename_preSelectedItems.Count == 1)
                    return;

                List<string> sourceList = new List<string>();
                bool added;
                VM.DataGridRowModle_dirNFile vmFile;
                VM.DataGridRowModle_disk vmDisk;
                foreach (object o in DataGridRename_preSelectedItems)
                {
                    added = false;
                    if (o is VM.DataGridRowModle_dirNFile)
                    {
                        vmFile = (VM.DataGridRowModle_dirNFile)o;
                        sourceList.Add(vmFile.iois.fullName);
                        added = true;
                    }
                    else if (o is VM.DataGridRowModle_disk)
                    {
                        vmDisk = (VM.DataGridRowModle_disk)o;
                        sourceList.Add(vmDisk.diskInfo.name);
                        added = true;
                    }
                    if (added && o == DataGridRename_startingRenameItem)
                    {
                        int tmpIdx = sourceList.Count - 1;
                        string tmpStr = sourceList[tmpIdx];
                        sourceList.RemoveAt(tmpIdx);
                        sourceList.Insert(0, tmpStr);
                    }
                }
                core.Rename(sourceList.ToArray(), newName);
                dataGrid.Focus();
            }
        }
        private object TreeViewRename_preSelectedNode;
        private void TreeViewRename(string newName)
        {
            ppu_text.IsOpen = false;
            if (tb_rename.Visibility == Visibility.Visible)
            {
                tb_rename.Visibility = Visibility.Collapsed;

                if (TryStartRename_namePre == newName)
                    return;

                core.Rename(new string[] { GetTreeNodeURL(TreeViewRename_preSelectedNode) }, newName);
                treeView.Focus();
            }
        }

        #endregion


        #region quality of life

        private bool? flag_lastFocused_TVorDG = null;

        private void SetFocuseByLastFocused()
        {
            if (this != core.currentExplorer)
                return;

            Dispatcher.BeginInvoke(DispatcherPriority.Background, async () =>
            {
                // while? treeView.IsFocused == false even after treeView.Focus() and it IS focused by appearance;
                if (flag_lastFocused_TVorDG == true && !treeView.IsFocused)
                {
                    treeView.Focus();
                    //await Task.Delay(100);
                }
                if (flag_lastFocused_TVorDG == false && !dataGrid.IsFocused)
                {
                    dataGrid.Focus();
                    //await Task.Delay(100);
                }
            });
        }

        private List<object> qol_DGSelection = new List<object>();
        private string qol_DGCurrentCellColHeader = null;
        private void qol_Load_DGSelection()
        {
            return;

            qol_DGSelection.Clear();
            foreach (object i in dataGrid.SelectedItems)
                qol_DGSelection.Add(i);

            if (dataGrid.CurrentItem != null)
            {
                object ci = qol_DGSelection.Find(i => i == dataGrid.CurrentItem);
                if (ci != null && qol_DGSelection.IndexOf(ci) != qol_DGSelection.Count - 1)
                {
                    qol_DGSelection.Remove(ci);
                    qol_DGSelection.Add(ci);
                }
                qol_DGCurrentCellColHeader = dataGrid.CurrentCell.Column.Header.ToString();
            }
            else
            {
                qol_DGCurrentCellColHeader = null;
            }
        }
        private void qol_Set_DGSelection()
        {
            return;

            if (dataGrid.SelectedItems.Count <= 0 && qol_DGSelection.Count > 0)
            {
                object foundI;
                foreach (object i in qol_DGSelection)
                {
                    foundI = dataGridItemSource.Where(oi => oi == i).FirstOrDefault();
                    if (foundI != null)
                    {
                        dataGrid.SelectedItems.Add(foundI);
                    }
                }

                if (qol_DGCurrentCellColHeader != null)
                {
                    foundI = dataGridItemSource.Where(oi => oi == qol_DGSelection[qol_DGSelection.Count - 1]).FirstOrDefault();
                    if (foundI != null)
                    {
                        dataGrid.CurrentItem = foundI;
                        DataGridColumn foundCol = dataGrid.Columns.Where(c => c.Header.ToString() == qol_DGCurrentCellColHeader).FirstOrDefault();
                        if (foundCol != null)
                        {
                            dataGrid.CurrentCell = new DataGridCellInfo(foundI, foundCol);
                        }
                    }
                }
            }
        }

        #endregion


        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            string msg1 = core.GetLangTx("txStatus_doubleClickToCloseSubWindow");
            if (string.IsNullOrWhiteSpace(msg1))
            {
                msg1 = "Double click to close sub-window.";
            }
            string strMissing = null;
            SetStates(ref msg1, ref strMissing, ref strMissing, ref strMissing, ref strMissing, ref strMissing);
        }
        private void btn_close_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            core.LayoutRemoveExplorer(this);
        }

    }
}
