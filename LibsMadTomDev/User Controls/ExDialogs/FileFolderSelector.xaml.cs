using MadTomDev.Common;
using MadTomDev.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MadTomDev.UI
{
    /// <summary>
    /// Interaction logic for FileFolderSelector.xaml
    /// </summary>
    public partial class FileFolderSelector : Window
    {
        /// <summary>
        ///    canSelectDirs = false,
        ///    canSelectFiles = true,
        ///    isSingleSelectFile = false,
        ///    isSingleSelectDir = false,
        ///    showFiles = true,
        ///    isSaveFile = false,
        ///    fileFilter = new MadTomDev.Data.FileFilterHelper("all files|*.*|text file|*.txt"),
        ///    btn_ok_finalText = "get some",
        ///    InitedDir = "C:\\Temp",
        /// </summary>
        public FileFolderSelector()
        {
            InitializeComponent();
        }

        public bool canSelectDirs;
        public bool canSelectFiles;
        public bool isSingleSelectFile;
        public bool isSingleSelectDir;
        public bool showFiles;
        public bool isSaveFile;
        public FileFilterHelper fileFilter;
        public string btn_ok_finalText;

        public List<string> selected_dirs = new List<string>();
        public List<string> selected_files = new List<string>();
        public string saveFileName;
        public string saveFileFullName;

        public string InitedDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


        private bool isRootQuickAccessInited = false;
        private int currentFileFilterIndex { set; get; } = -1;
        private void Window_Activated(object sender, EventArgs e)
        {
            if (!isRootQuickAccessInited)
            {
                this.DialogResult = null;

                if (isSaveFile)
                    tb_selectedLabel.Text = "保存到文件";
                else if (canSelectDirs && canSelectFiles)
                    tb_selectedLabel.Text = "选择的文件夹和文件";
                else if (canSelectDirs)
                    tb_selectedLabel.Text = "选择的文件夹";
                else //if (canSelectFiles)
                    tb_selectedLabel.Text = "选择的文件";


                dataGridCellStyle_verticalCenter = new Style();
                dataGridCellStyle_verticalCenter.Setters.Add(new Setter(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center));

                icons_fs.IconGot += Icons_fs_IconGot;

                // pre load
                icons_shell32.GetIcon(20, false);

                btn_ok.Content = btn_ok_finalText;

                treeViewRoot_quickAccess.IsExpanded = true;
                isRootQuickAccessInited = true;

                cb_filter.Items.Clear();
                if (fileFilter != null && fileFilter.filters.Count > 0)
                {
                    for (int i = 0, iv = fileFilter.filters.Count; i < iv; i++)
                        cb_filter.Items.Add(fileFilter.filters[i].FullDescription);
                    cb_filter.SelectedIndex = 0;
                    currentFileFilterIndex = 0;

                }
                //else
                //{
                //    currentFileFilterIndex = -1;
                //    currentFileFilter = "all files(*.*)";
                //}

                cb_selected.Items.Clear();
                for (int i = 0, iv = resentFilesCache.cache.Count; i < iv; i++)
                    cb_selected.Items.Add(resentFilesCache.cache[i]);


                if (!string.IsNullOrWhiteSpace(InitedDir))
                    GotoUri(InitedDir);
            }
        }



        #region treeview init, make treeview node

        private IconHelper.FileSystem icons_fs = IconHelper.FileSystem.Instance;
        private IconHelper.Shell32Icons icons_shell32 = IconHelper.Shell32Icons.Instance;
        private void Window_Initialized(object sender, EventArgs e)
        {
            // 树视图里只留 3 个节点，一个常用地址，如桌面、文档、下载等等；（默认展开）
            // 第二个“我的电脑”，列出所有驱动器（默认收起）
            // 第三个“网络”，列出所有网络主机（默认收起）

            // 展开路径时，读取其中的内容，并填充树图；
            // 收起时，增加“加载中...”节点，回收其他节点资源；

            // 操作： 进入路径
            // 当双击列表中的某个文件夹时，获取完整路径，执行“进入路径”
            // 当点击树表中某个文件夹时，同上；
            // 当地址栏中输入路径并安下回车，同上；

            // 地址栏中的文本变化后，刷新按钮变为 前往按钮；

            #region 初始化 树表

            treeView.ItemsSource = treeViewNodes;


            IconHelper.Shell32Icons shell32Icons = IconHelper.Shell32Icons.Instance;
            treeViewRoot_quickAccess = new VM.TreeViewModelContainer(null)
            {
                Icon = shell32Icons.GetIcon(320, false),
                Text = "快速访问",
                Children = new ObservableCollection<object>(),
            };


            #region init quick access links (one time)

            string qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = icons_shell32.GetIcon(34, false),
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
                    Icon = icons_shell32.GetIcon(122, false),
                    link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
                });
            }
            qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = icons_shell32.GetIcon(1, false),
                link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
            });
            qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = icons_shell32.GetIcon(324, false),
                link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
            });
            qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = icons_shell32.GetIcon(116, false),
                link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
            });
            qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = icons_shell32.GetIcon(115, false),
                link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
            });
            qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.Favorites);
            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = icons_shell32.GetIcon(86, false),
                link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
            });
            qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.Templates);
            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = icons_shell32.GetIcon(114, false),
                link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
            });
            qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = icons_shell32.GetIcon(89, false),
                link = new IOInfoShadow(new DirectoryInfo(qaDirPath)),
            });
            qaDirPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
            treeViewRoot_quickAccess.Children.Add(new VM.TreeViewModelLink(treeViewRoot_quickAccess)
            {
                Text = System.IO.Path.GetFileName(qaDirPath),
                Icon = icons_shell32.GetIcon(90, false),
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

            treeViewRoot_thisPC = new VM.TreeViewModelPC(null, CheckTreeNodeCollapseRecycle)
            {
                Icon = shell32Icons.GetIcon(15, false),
                Text = "我的电脑",
                Children = new ObservableCollection<object>(),
            };
            treeViewRoot_thisPC.AddLoadingLabelNode();

            treeViewRoot_network = new VM.TreeViewModelNetWork(null, CheckTreeNodeCollapseRecycle)
            {
                Icon = shell32Icons.GetIcon(13, false),
                Text = "网络",
                Children = new ObservableCollection<object>(),
            };
            treeViewRoot_network.AddLoadingLabelNode();

            treeViewNodes.Add(treeViewRoot_quickAccess);
            treeViewNodes.Add(treeViewRoot_thisPC);
            treeViewNodes.Add(treeViewRoot_network);


            #endregion

        }

        private bool CheckTreeNodeCollapseRecycle(VM.TreeViewModelIORefreashable collapsingNode)
        {
            if (curSelectedFullPath == null)
                return true;

            string fullPath = collapsingNode.GetFullPath();
            bool result = !curSelectedFullPath.Contains(fullPath);
            return result;
        }

        private ObservableCollection<object> treeViewNodes = new ObservableCollection<object>();
        private VM.TreeViewModelContainer treeViewRoot_quickAccess;
        private VM.TreeViewModelPC treeViewRoot_thisPC;
        private VM.TreeViewModelNetWork treeViewRoot_network;


        #endregion


        #region tree node selected, file filter, load subs to dataGrid, goto url(find tree node)


        private string curSelectedFullPath;
        private Guid treeView_SelectedItemChanged_preTaskID;
        private object treeView_SelectedItem_pre;
        private bool dataGridItemSource_loading = false;
        private ObservableCollection<object> dataGridItemSource = new ObservableCollection<object>();
        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //e.Handled = true;

            // https://www.codeproject.com/Tips/208896/WPF-TreeView-SelectedItemChanged-called-twice
            // Very often, we need to execute some code in SelectedItemChanged depending on the selected TreeViewItem. But SelectedItemChanged is called twice. This is due to stealing focus from the main window, which is screwing something up.
            // What we have to do to avoid this is simply delay the call to our code.
            Dispatcher.BeginInvoke(async () =>
            {
                dataGrid.ItemsSource = null;
                if (treeView.SelectedItem == null)
                {
                    return;
                }
                //if (((VM.TreeViewNodeModelBase)treeView.SelectedItem).IsSelected == false)
                //    return;
                tb_datagrid_loading.Visibility = Visibility.Visible;

                Guid curTaskId = Guid.NewGuid();
                treeView_SelectedItemChanged_preTaskID = curTaskId;

                if (treeView.SelectedItem is VM.TreeViewModelContainer)
                {
                    VM.TreeViewModelContainer container = (VM.TreeViewModelContainer)treeView.SelectedItem;
                    if (container.Children.Count > 0 && container.Children[0] is VM.TreeViewModelLink)
                    {
                        dataGrid_setColumns_links();
                        dataGridItemSource_loading = true;
                        dataGridItemSource.Clear();
                        foreach (object o in container.Children)
                        {
                            if (o is VM.TreeViewModelLink)
                                dataGridItemSource.Add(new DataGridRowModle_link((VM.TreeViewModelLink)o));
                        }
                        dataGrid.ItemsSource = dataGridItemSource;
                        dataGridItemSource_loading = false;
                        LoadDataGridItemIcons();
                    }
                    tb_datagrid_loading.Visibility = Visibility.Hidden;
                    tb_uri.Text = container.Text;
                }
                else if (treeView.SelectedItem is VM.TreeViewModelPC)
                {
                    dataGrid_setColumns_disks();
                    VM.TreeViewModelPC pcNode = (VM.TreeViewModelPC)treeView.SelectedItem;
                    if (!pcNode.isLoaded)
                    {
                        await pcNode.LoadChildrenAsync();
                    }
                    if (treeView_SelectedItemChanged_preTaskID != curTaskId)
                        return;

                    dataGridItemSource_loading = true;
                    dataGridItemSource.Clear();
                    foreach (object o in pcNode.Children)
                    {
                        if (o is VM.TreeViewModelDisk)
                            dataGridItemSource.Add(new DataGridRowModle_disk((VM.TreeViewModelDisk)o));
                    }
                    tb_datagrid_loading.Visibility = Visibility.Hidden;
                    dataGrid.ItemsSource = dataGridItemSource;
                    dataGridItemSource_loading = false;
                    LoadDataGridItemIcons();
                    tb_uri.Text = pcNode.Text;
                }
                else if (treeView.SelectedItem is VM.TreeViewModelDisk)
                {
                    dataGrid_setColumns_dirsNfiles();
                    VM.TreeViewModelDisk pcNode = (VM.TreeViewModelDisk)treeView.SelectedItem;
                    if (!pcNode.isLoaded)
                    {
                        await pcNode.LoadChildrenAsync();
                    }
                    if (treeView_SelectedItemChanged_preTaskID != curTaskId)
                        return;

                    dataGridItemSource_loading = true;
                    ReLoadDGWithFilter(pcNode.fullContent);
                    tb_datagrid_loading.Visibility = Visibility.Hidden;
                    dataGrid.ItemsSource = dataGridItemSource;
                    dataGridItemSource_loading = false;
                    LoadDataGridItemIcons();
                    tb_uri.Text = pcNode.diskInfo.Name;
                    historyMgr.Write_History(tb_uri.Text, pcNode.Icon, null, tb_uri.Text);
                }
                else if (treeView.SelectedItem is VM.TreeViewModelDir)
                {
                    dataGrid_setColumns_dirsNfiles();
                    VM.TreeViewModelDir dirNode = (VM.TreeViewModelDir)treeView.SelectedItem;
                    if (!dirNode.isLoaded)
                    {
                        await dirNode.LoadChildrenAsync();
                    }
                    if (treeView_SelectedItemChanged_preTaskID != curTaskId)
                        return;

                    dataGridItemSource_loading = true;
                    ReLoadDGWithFilter(dirNode.fullContent);
                    tb_datagrid_loading.Visibility = Visibility.Hidden;
                    dataGrid.ItemsSource = dataGridItemSource;
                    dataGridItemSource_loading = false;
                    LoadDataGridItemIcons();
                    tb_uri.Text = dirNode.dirInfo.fullName;
                    historyMgr.Write_History(tb_uri.Text, dirNode.Icon, null, dirNode.dirInfo.name);
                }
                else if (treeView.SelectedItem is VM.TreeViewModelNetWork)
                {
                    dataGrid_setColumns_hosts();
                    VM.TreeViewModelNetWork hostNode = (VM.TreeViewModelNetWork)treeView.SelectedItem;
                    if (!hostNode.isLoaded)
                    {
                        await hostNode.LoadChildrenAsync();
                    }
                    if (treeView_SelectedItemChanged_preTaskID != curTaskId)
                        return;

                    dataGridItemSource_loading = true;
                    dataGridItemSource.Clear();
                    foreach (object o in hostNode.Children)
                    {
                        if (o is VM.TreeViewModelHost)
                            dataGridItemSource.Add(new DataGridRowModle_host((VM.TreeViewModelHost)o));
                    }
                    tb_datagrid_loading.Visibility = Visibility.Hidden;
                    dataGrid.ItemsSource = dataGridItemSource;
                    dataGridItemSource_loading = false;
                    LoadDataGridItemIcons();
                    tb_uri.Text = hostNode.Text;
                }
                else if (treeView.SelectedItem is VM.TreeViewModelHost)
                {
                    dataGrid_setColumns_hostDirs();
                    VM.TreeViewModelHost hostNode = (VM.TreeViewModelHost)treeView.SelectedItem;
                    if (!hostNode.isLoaded)
                    {
                        await hostNode.LoadChildrenAsync();
                    }
                    if (treeView_SelectedItemChanged_preTaskID != curTaskId)
                        return;

                    dataGridItemSource_loading = true;
                    dataGridItemSource.Clear();
                    foreach (object o in hostNode.Children)
                    {
                        if (o is VM.TreeViewModelDir)
                            dataGridItemSource.Add(new DataGridRowModle_dirNFile((VM.TreeViewModelDir)o));
                    }
                    tb_datagrid_loading.Visibility = Visibility.Hidden;
                    dataGrid.ItemsSource = dataGridItemSource;
                    dataGridItemSource_loading = false;
                    LoadDataGridItemIcons();
                    tb_uri.Text = "\\\\" + hostNode.hostName;
                    historyMgr.Write_History(tb_uri.Text, hostNode.Icon, null, tb_uri.Text);
                }
                else if (treeView.SelectedItem is VM.TreeViewModelLink)
                {
                    if (treeView_SelectedItemChanged_preTaskID != curTaskId)
                        return;
                    //tb_uri.Text = "";
                    VM.TreeViewModelLink lnkTN = (VM.TreeViewModelLink)treeView.SelectedItem;
                    //lnkTN.IsSelected = false;
                    //if (tb_uri.Text != lnkTN.link.fullName)
                    GotoUri(lnkTN.link.fullName);
                }
                else
                {
                    tb_uri.Text = "";
                    dataGrid.Columns.Clear();
                    tb_datagrid_loading.Visibility = Visibility.Hidden;
                }

                curSelectedFullPath = tb_uri.Text;
                treeView_SelectedItem_pre = treeView.SelectedItem;

                TreeViewItem tvi = FindTreeNodeContainer((VM.TreeViewNodeModelBase)treeView.SelectedItem);
                tvi.BringIntoView();

                TreeViewItem FindTreeNodeContainer(VM.TreeViewNodeModelBase item)
                {
                    List<VM.TreeViewNodeModelBase> nodeChain = new List<VM.TreeViewNodeModelBase>();
                    nodeChain.Add(item);
                    while (item.parent != null)
                    {
                        item = (VM.TreeViewNodeModelBase)item.parent;
                        nodeChain.Insert(0,item);
                    }
                    TreeViewItem result = (TreeViewItem)(treeView.ItemContainerGenerator.ContainerFromItem(nodeChain[0]));
                    for (int i = 1, iv = nodeChain.Count; i < iv; i++)
                    {
                        result = (TreeViewItem)(result.ItemContainerGenerator.ContainerFromItem(nodeChain[i]));
                    }
                    return result;                    
                }

            }, DispatcherPriority.Background);
        }
        private void LoadDataGridItemIcons()
        {
            if (dataGridItemSource.Count == 0 || !(dataGridItemSource[0] is DataGridRowModle_dirNFile))
                return;

            Dispatcher.BeginInvoke(() =>
            {
                DataGridRowModle_dirNFile item;
                foreach (object o in dataGridItemSource)
                {
                    item = (DataGridRowModle_dirNFile)o;
                    if (!item.isIconReady)
                    {
                        if (icons_fs.HaveIcon(item.iois.fullName, true, item.iois.attributes.directory))
                        {
                            item.icon = icons_fs.GetIcon(item.iois.fullName, true, item.iois.attributes.directory);
                            item.isIconReady = true;
                        }
                    }
                }
            }, DispatcherPriority.Background);
        }
        private void Icons_fs_IconGot(IconHelper.FileSystem sender, string path, string ext, bool smallIcon, bool isDirectory, BitmapSource icon)
        {
            if (dataGridItemSource_loading)
                return;
            if (dataGridItemSource.Count > 0 && dataGridItemSource[0] is DataGridRowModle_dirNFile)
            {
                DataGridRowModle_dirNFile item;
                foreach (object o in dataGridItemSource)
                {
                    item = (DataGridRowModle_dirNFile)o;
                    if (!item.isIconReady && IconHelper.FileSystem.GetExt(item.iois.fullName, item.iois.attributes.directory) == ext)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            item.icon = icon;
                            item.isIconReady = true;
                        });
                    }
                }
            }
        }

        private void cb_filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentFileFilterIndex != cb_filter.SelectedIndex)
            {
                cb_selected.Text = "";
                currentFileFilterIndex = cb_filter.SelectedIndex;
                if (treeView.SelectedItem is VM.TreeViewModelDisk)
                {
                    VM.TreeViewModelDisk pcNode = (VM.TreeViewModelDisk)treeView.SelectedItem;
                    ReLoadDGWithFilter(pcNode.fullContent);
                }
                else if (treeView.SelectedItem is VM.TreeViewModelDir)
                {
                    VM.TreeViewModelDir dirNode = (VM.TreeViewModelDir)treeView.SelectedItem;
                    ReLoadDGWithFilter(dirNode.fullContent);
                }
            }
        }
        private void ReLoadDGWithFilter(List<IOInfoShadow> allIois)
        {
            dataGridItemSource.Clear();
            if (currentFileFilterIndex < 0)
            {
                // all
                foreach (IOInfoShadow o in allIois)
                {
                    if (!showFiles && o.attributes.archive)
                        continue;
                    dataGridItemSource.Add(new DataGridRowModle_dirNFile(o));
                }
            }
            else
            {
                // with filter
                List<IOInfoShadow> allFiles = new List<IOInfoShadow>();
                foreach (IOInfoShadow o in allIois)
                {
                    if (o.attributes.archive)
                    {
                        if (!showFiles)
                            continue;
                        allFiles.Add(o);
                    }
                    else
                    {
                        dataGridItemSource.Add(new DataGridRowModle_dirNFile(o));
                    }
                }
                foreach (IOInfoShadow o in fileFilter.Filter(currentFileFilterIndex, allFiles))
                    dataGridItemSource.Add(new DataGridRowModle_dirNFile(o));
            }
        }

        private void GotoUri(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return;
            }
            fullPath = fullPath.Trim();
            if (fullPath.StartsWith("\\\\"))
            {
                GotoNode((VM.TreeViewModelIORefreashable)treeView.Items[2], fullPath);
            }
            else if (fullPath.Length > 1 && fullPath[1] == ':')
            {
                GotoNode((VM.TreeViewModelIORefreashable)treeView.Items[1], fullPath);
            }
            else if (fullPath == ((VM.TreeViewNodeModelBase)treeView.Items[0]).Text)
                ((VM.TreeViewNodeModelBase)treeView.Items[0]).IsSelected = true;
            else if (fullPath == ((VM.TreeViewNodeModelBase)treeView.Items[1]).Text)
                ((VM.TreeViewNodeModelBase)treeView.Items[1]).IsSelected = true;
            else if (fullPath == ((VM.TreeViewNodeModelBase)treeView.Items[2]).Text)
                ((VM.TreeViewNodeModelBase)treeView.Items[2]).IsSelected = true;


            async void GotoNode(VM.TreeViewModelIORefreashable parentNode, string fullPath)
            {
                string[] parts = fullPath.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0 && parts[0].Length == 2 && parts[0][1] == ':')
                {
                    parts[0] = parts[0] + "\\";
                }
                string part = null;

                VM.TreeViewModelDisk subNodeDisk;
                VM.TreeViewModelDir subNodeDir;
                VM.TreeViewModelHost subNodeHost;
                VM.TreeViewModelIORefreashable foundSub = null;

                if (parts.Length > 1)
                {
                    for (int i = 0, iv = parts.Length; i < iv; i++)
                    {
                        part = parts[i].Trim().ToLower();
                        if (!parentNode.isLoaded)
                        {
                            await parentNode.LoadChildrenAsync();
                        }
                        if (!parentNode.IsExpanded)
                            parentNode.IsExpanded = true;
                        foundSub = FindSub(parentNode, part);

                        if (foundSub == null)
                        {
                            break;
                        }
                        parentNode = foundSub;
                    }
                }
                else
                {
                    if (!parentNode.isLoaded)
                    {
                        await parentNode.LoadChildrenAsync();
                    }
                    foundSub = FindSub(parentNode, parts[parts.Length - 1]);
                }

                if (foundSub == null)
                {
                    parentNode.IsSelected = true;
                    System.Media.SystemSounds.Beep.Play();
                }
                else
                {
                    foundSub.IsSelected = true;
                }

                VM.TreeViewModelIORefreashable FindSub(VM.TreeViewModelIORefreashable parentNode, string name)
                {
                    name = name.ToLower();
                    foreach (object o in parentNode.Children)
                    {
                        if (o is VM.TreeViewModelDisk)
                        {
                            subNodeDisk = (VM.TreeViewModelDisk)o;
                            if (subNodeDisk.diskInfo.Name.ToLower() == name)
                            {
                                return subNodeDisk;
                            }
                        }
                        else if (o is VM.TreeViewModelDir)
                        {
                            subNodeDir = (VM.TreeViewModelDir)o;
                            if (subNodeDir.dirInfo.name.ToLower() == name)
                            {
                                return subNodeDir;
                            }
                        }
                        else if (o is VM.TreeViewModelHost)
                        {
                            subNodeHost = (VM.TreeViewModelHost)o;
                            if (subNodeHost.hostName.ToLower() == name)
                            {
                                return subNodeHost;
                            }
                        }
                    }
                    return null;
                }
            }
        }
        private void btn_goUri_Click(object sender, RoutedEventArgs e)
        {
            GotoUri(tb_uri.Text);
        }

        private void tb_uri_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                btn_goUri_Click(null, null);
        }


        #endregion

        #region set dataGrid columns , viewModle 


        #region datagrid columns
        private string dataGrid_columns_flag = null;

        //FrameworkElementFactory dataGrid_column_iconNnameFE;
        Style dataGridCellStyle_verticalCenter;

        private FrameworkElementFactory GetNameColVT()
        {
            //if (dataGrid_column_iconNnameFE == null)
            //{
            FrameworkElementFactory dataGrid_column_iconNnameFE = new FrameworkElementFactory(typeof(StackPanel));
            dataGrid_column_iconNnameFE.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            dataGrid_column_iconNnameFE.SetValue(StackPanel.MarginProperty, new Thickness(1));
            FrameworkElementFactory icon = new FrameworkElementFactory(typeof(Image));
            icon.SetBinding(Image.SourceProperty, new Binding("icon"));
            icon.SetValue(Image.StretchProperty, Stretch.None);
            icon.SetValue(Image.MarginProperty, new Thickness(2, 0, 0, 0));

            icon.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.NearestNeighbor);
            icon.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            dataGrid_column_iconNnameFE.AppendChild(icon);
            FrameworkElementFactory name = new FrameworkElementFactory(typeof(TextBlock));
            name.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            name.SetValue(TextBlock.PaddingProperty, new Thickness(4, 0, 4, 0));
            name.SetBinding(TextBlock.TextProperty, new Binding("name"));
            dataGrid_column_iconNnameFE.AppendChild(name);
            //}
            return dataGrid_column_iconNnameFE;
        }

        private DataTemplate GetNameColEditDT()
        {
            FrameworkElementFactory container = new FrameworkElementFactory(typeof(StackPanel));
            container.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            container.SetValue(StackPanel.MarginProperty, new Thickness(1));
            FrameworkElementFactory icon = new FrameworkElementFactory(typeof(Image));
            icon.SetBinding(Image.SourceProperty, new Binding("icon"));
            icon.SetValue(Image.StretchProperty, Stretch.None);
            icon.SetValue(Image.MarginProperty, new Thickness(2, 0, 0, 0));

            icon.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.NearestNeighbor);
            icon.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            container.AppendChild(icon);

            FrameworkElementFactory tb = new FrameworkElementFactory(typeof(TextBox));
            tb.SetValue(TextBox.VerticalAlignmentProperty, VerticalAlignment.Center);
            tb.SetValue(TextBox.PaddingProperty, new Thickness(1, 0, 0, 0));
            tb.SetBinding(TextBox.TextProperty, new Binding("EditingTx"));
            container.AppendChild(tb);
            DataTemplate dataGrid_column_iconNnameEditDT = new DataTemplate()
            {
                VisualTree = container,
            };

            return dataGrid_column_iconNnameEditDT;
        }

        private FrameworkElementFactory GetCombinedText(string numBindingName, string unitBindingName)
        {
            FrameworkElementFactory container = new FrameworkElementFactory(typeof(DockPanel));
            container.SetValue(DockPanel.LastChildFillProperty, true);
            FrameworkElementFactory tx2 = new FrameworkElementFactory(typeof(TextBlock));
            tx2.SetValue(TextBlock.PaddingProperty, new Thickness(6, 0, 0, 0));
            tx2.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
            tx2.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            tx2.SetValue(DockPanel.DockProperty, Dock.Right);
            tx2.SetBinding(TextBlock.TextProperty, new Binding(unitBindingName));
            container.AppendChild(tx2);
            //container.SetValue(DockPanel., Orientation.Horizontal);
            FrameworkElementFactory tx1 = new FrameworkElementFactory(typeof(TextBlock));
            tx1.SetValue(TextBlock.PaddingProperty, new Thickness(6, 0, 0, 0));
            tx1.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
            tx1.SetBinding(TextBlock.TextProperty, new Binding(numBindingName));
            container.AppendChild(tx1);
            //tx2.SetValue(TextBlock.ForegroundProperty, dataGrid_column_unitFontBrush);
            return container;
        }

        private ObservableCollection<DataGridColumn> dataGrid_columns_link = null;
        public string dataGrid_dataTable_col_iconName = "名称";
        public string dataGrid_dataTable_col_linkAddr = "链接地址";
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
                    Header = dataGrid_dataTable_col_iconName,
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetNameColVT(),
                    },
                    CellStyle = dataGridCellStyle_verticalCenter,
                    CanUserSort = true,
                    SortMemberPath = "name",
                });
                dataGrid_columns_link.Add(new DataGridHyperlinkColumn()
                {
                    Header = dataGrid_dataTable_col_linkAddr,
                    IsReadOnly = true,
                    Binding = new Binding("link"),
                    CellStyle = dataGridCellStyle_verticalCenter,
                });
            }
            dataGrid.Columns.Clear();
            foreach (DataGridColumn dgc in dataGrid_columns_link)
                dataGrid.Columns.Add(dgc);
        }

        private Brush dataGrid_column_unitFontBrush = new SolidColorBrush(Color.FromRgb(0, 0, 128));
        private ObservableCollection<DataGridColumn> dataGrid_columns_disks = null;
        public string dataGrid_dataTable_col_diskType = "存储类型";
        public string dataGrid_dataTable_col_diskFormat = "存储格式";
        public string dataGrid_dataTable_col_totalSize = "总容量";
        public string dataGrid_dataTable_col_freeSpace = "空闲空间";
        public string dataGrid_dataTable_col_usedPersent = "使用率";
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
                    Header = dataGrid_dataTable_col_iconName,
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetNameColVT(),
                    },
                    CellStyle = dataGridCellStyle_verticalCenter,
                    CanUserSort = true,
                    SortMemberPath = "name",
                    CellEditingTemplate = GetNameColEditDT(),
                });
                dataGrid_columns_disks.Add(new DataGridTextColumn()
                {
                    Header = dataGrid_dataTable_col_diskType,
                    IsReadOnly = true,
                    Binding = new Binding("diskType"),
                    CellStyle = dataGridCellStyle_verticalCenter,
                });
                dataGrid_columns_disks.Add(new DataGridTextColumn()
                {
                    Header = dataGrid_dataTable_col_diskFormat,
                    IsReadOnly = true,
                    Binding = new Binding("diskFormat"),
                    CellStyle = dataGridCellStyle_verticalCenter,
                });


                dataGrid_columns_disks.Add(new DataGridTemplateColumn()
                {
                    Header = dataGrid_dataTable_col_totalSize,
                    IsReadOnly = true,
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetCombinedText("totalSizeTxN", "totalSizeTxU"),
                    },
                    CellStyle = dataGridCellStyle_verticalCenter,
                    CanUserSort = true,
                    SortMemberPath = "totalSize",
                });

                dataGrid_columns_disks.Add(new DataGridTemplateColumn()
                {
                    Header = dataGrid_dataTable_col_freeSpace,
                    IsReadOnly = true,
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetCombinedText("freeSpaceTxN", "freeSpaceTxU"),
                    },
                    CellStyle = dataGridCellStyle_verticalCenter,
                    CanUserSort = true,
                    SortMemberPath = "freeSpace",
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
                progressText.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                progressText.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                container.AppendChild(progressText);

                dataGrid_columns_disks.Add(new DataGridTemplateColumn()
                {
                    Header = dataGrid_dataTable_col_usedPersent,
                    IsReadOnly = true,
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = container,
                    },
                    CanUserSort = true,
                    SortMemberPath = "usedPersentage",
                });
            }
            dataGrid.Columns.Clear();
            foreach (DataGridColumn dgc in dataGrid_columns_disks)
                dataGrid.Columns.Add(dgc);
        }

        private ObservableCollection<DataGridColumn> dataGrid_columns_hosts = null;
        public string dataGrid_dataTable_col_hostIPv4 = "主机IPv4";
        public string dataGrid_dataTable_col_hostIPv6 = "主机IPv6";
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
                    Header = dataGrid_dataTable_col_iconName,
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetNameColVT(),
                    },
                    CanUserSort = true,
                    SortMemberPath = "name",
                });
                Style style = new Style(typeof(TextBlock), dataGridCellStyle_verticalCenter);
                style.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(8, 0, 8, 0)));
                dataGrid_columns_hosts.Add(new DataGridTextColumn()
                {
                    Header = dataGrid_dataTable_col_hostIPv4,
                    Binding = new Binding("hostIPv4"),
                    ElementStyle = style,
                    //Width = 100d,
                });
                dataGrid_columns_hosts.Add(new DataGridTextColumn()
                {
                    Header = dataGrid_dataTable_col_hostIPv6,
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
                Header = dataGrid_dataTable_col_iconName,
                CellTemplate = new DataTemplate()
                {
                    VisualTree = GetNameColVT(),
                },
                CanUserSort = true,
                SortMemberPath = "name",
            });
            //}
            dataGrid.Columns.Clear();
            foreach (DataGridColumn dgc in dataGrid_columns_hostDir)
                dataGrid.Columns.Add(dgc);
        }

        private ObservableCollection<DataGridColumn> dataGrid_columns_dirsNfiles = null;
        public string dataGrid_dataTable_col_ModifyTime = "修改时间";
        public string dataGrid_dataTable_col_fileType = "文件类型";
        public string dataGrid_dataTable_col_fileSize = "文件尺寸";
        public string dataGrid_dataTable_col_fileAttributes = "文件属性";
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
                    Header = dataGrid_dataTable_col_iconName,
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetNameColVT(),
                    },
                    CanUserSort = true,
                    SortMemberPath = "name",
                    CellEditingTemplate = GetNameColEditDT(),
                });
                dataGrid_columns_dirsNfiles.Add(new DataGridTextColumn()
                {
                    Header = dataGrid_dataTable_col_ModifyTime,
                    Binding = new Binding("modifyTimeTx"),
                    CellStyle = dataGridCellStyle_verticalCenter,
                });
                Style style_center = new Style();
                style_center.BasedOn = dataGridCellStyle_verticalCenter;
                style_center.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));

                Style style_HeaderCenter = new Style();
                style_HeaderCenter.Setters.Add(new Setter(GridViewColumnHeader.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));

                //Style style_right = new Style(typeof(TextBlock), dataGridCellStyle_verticalCenter);
                //style_right.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
                dataGrid_columns_dirsNfiles.Add(new DataGridTextColumn()
                {
                    Header = dataGrid_dataTable_col_fileType,
                    Binding = new Binding("fileType"),
                    CellStyle = style_center,
                    HeaderStyle = style_HeaderCenter,
                });

                dataGrid_columns_dirsNfiles.Add(new DataGridTemplateColumn()
                {
                    Header = dataGrid_dataTable_col_fileSize,
                    IsReadOnly = true,
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetCombinedText("fileSizeTxN", "fileSizeTxU"),
                    },
                    CellStyle = dataGridCellStyle_verticalCenter,
                    CanUserSort = true,
                    SortMemberPath = "fileSize",
                });



                dataGrid_columns_dirsNfiles.Add(new DataGridTextColumn()
                {
                    Header = dataGrid_dataTable_col_fileAttributes,
                    Binding = new Binding("attributes"),
                    CellStyle = dataGridCellStyle_verticalCenter,
                });
            }
            dataGrid.Columns.Clear();
            foreach (DataGridColumn dgc in dataGrid_columns_dirsNfiles)
                dataGrid.Columns.Add(dgc);
        }

        private ObservableCollection<DataGridColumn> dataGrid_columns_searchResult = null;
        public string dataGrid_dataTable_col_parentPath = "所在路径";
        private void dataGrid_setColumns_searchResult()
        {
            if (dataGrid_columns_flag == "searchResult")
                return;
            dataGrid_columns_flag = "searchResult";

            // name   modify-time   size   parentPath
            if (dataGrid_columns_searchResult == null)
            {
                dataGrid_columns_searchResult = new ObservableCollection<DataGridColumn>();
                dataGrid_columns_searchResult.Add(new DataGridTemplateColumn()
                {
                    Header = dataGrid_dataTable_col_iconName,
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetNameColVT(),
                    },
                    CanUserSort = true,
                    SortMemberPath = "name",
                    CellEditingTemplate = GetNameColEditDT(),
                });
                dataGrid_columns_searchResult.Add(new DataGridTextColumn()
                {
                    Header = dataGrid_dataTable_col_ModifyTime,
                    Binding = new Binding("modifyTimeTx"),
                    CellStyle = dataGridCellStyle_verticalCenter,
                });
                Style style_center = new Style();
                style_center.BasedOn = dataGridCellStyle_verticalCenter;
                style_center.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));

                Style style_HeaderCenter = new Style();
                style_HeaderCenter.Setters.Add(new Setter(GridViewColumnHeader.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));

                dataGrid_columns_searchResult.Add(new DataGridTemplateColumn()
                {
                    Header = dataGrid_dataTable_col_fileSize,
                    IsReadOnly = true,
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree = GetCombinedText("fileSizeTxN", "fileSizeTxU"),
                    },
                    CellStyle = dataGridCellStyle_verticalCenter,
                    CanUserSort = true,
                    SortMemberPath = "fileSize",
                });



                dataGrid_columns_searchResult.Add(new DataGridTextColumn()
                {
                    Header = dataGrid_dataTable_col_fileAttributes,
                    Binding = new Binding("attributes"),
                    CellStyle = dataGridCellStyle_verticalCenter,
                });

                dataGrid_columns_searchResult.Add(new DataGridTextColumn()
                {
                    Header = dataGrid_dataTable_col_parentPath,
                    Binding = new Binding("parentPath"),
                    CellStyle = dataGridCellStyle_verticalCenter,
                });
            }
            dataGrid.Columns.Clear();
            foreach (DataGridColumn dgc in dataGrid_columns_searchResult)
                dataGrid.Columns.Add(dgc);
        }

        #endregion


        #region datagrid item viewModle

        private class DataGridRowModleBase
        {
            public BitmapSource icon { set; get; }
            public string name { set; get; }
        }
        private class DataGridRowModle_link : DataGridRowModleBase
        {
            public DataGridRowModle_link(VM.TreeViewModelLink rawItem)
            {
                icon = rawItem.Icon;
                name = rawItem.Text;
                link = rawItem.link.fullName;
            }

            public string link { set; get; }
        }
        private class DataGridRowModle_disk : DataGridRowModleBase, INotifyPropertyChanged
        {
            private VM.TreeViewModelDisk treeNodeItem;
            public DataGridRowModle_disk(VM.TreeViewModelDisk rawItem)
            {
                treeNodeItem = rawItem;
                //icon = (BitmapSource)rawItem.Icon.Clone();
                diskInfo = rawItem.diskInfo;
                icon = rawItem.Icon;
                name = rawItem.Text;
                _EditingTx = rawItem.diskInfo.VolumeLabel;
                diskType = rawItem.diskInfo.DriveType.ToString();
                diskFormat = rawItem.diskInfo.DriveFormat;
                totalSize = rawItem.diskInfo.TotalSize;
                string tmp = SimpleStringHelper.UnitsOfMeasure.GetShortString(totalSize, "B", 1024);
                int tmpI = tmp.IndexOf(' ');
                totalSizeTxN = tmp.Substring(0, tmpI);
                totalSizeTxU = tmp.Substring(tmpI + 1);
                freeSpace = rawItem.diskInfo.AvailableFreeSpace;
                if (freeSpace == 0)
                {
                    freeSpaceTxN = "0";
                    freeSpaceTxU = " B";
                }
                else
                {
                    tmp = SimpleStringHelper.UnitsOfMeasure.GetShortString(freeSpace, "B", 1024);
                    tmpI = tmp.IndexOf(' ');
                    freeSpaceTxN = tmp.Substring(0, tmpI);
                    freeSpaceTxU = tmp.Substring(tmpI + 1);
                }
                usedPersentage = (double)(totalSize - freeSpace) / totalSize;
                usedPersentageTx = usedPersentage.ToString("P2");
            }

            private void ReGenerateName()
            {
                if (diskInfo.VolumeLabel.Length > 0)
                {
                    name = $"{diskInfo.VolumeLabel } ({diskInfo.Name})";
                }
                else
                {
                    name = $"({diskInfo.Name})";
                }
                RaisePropertyChanged("name");
            }
            public DriveInfo diskInfo;
            public event PropertyChangedEventHandler PropertyChanged;
            private void RaisePropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private string _EditingTx;
            public string EditingTx
            {
                set
                {
                    if (value != diskInfo.VolumeLabel)
                    {
                        try
                        {
                            diskInfo.VolumeLabel = value;
                            _EditingTx = value;
                            ReGenerateName();
                            treeNodeItem.Text = name;
                        }
                        catch (Exception err)
                        {
                            _EditingTx = diskInfo.VolumeLabel;
                            MessageBox.Show(err.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                }
                get => _EditingTx;
            }
            public string diskType { set; get; }
            public string diskFormat { set; get; }
            public long totalSize { set; get; }
            public string totalSizeTxN { set; get; }
            public string totalSizeTxU { set; get; }
            public long freeSpace { set; get; }
            public string freeSpaceTxN { set; get; }
            public string freeSpaceTxU { set; get; }
            public double usedPersentage { set; get; }
            public string usedPersentageTx { set; get; }
        }
        private class DataGridRowModle_host : DataGridRowModleBase
        {
            public DataGridRowModle_host(VM.TreeViewModelHost rawItem)
            {
                //icon = (BitmapSource)rawItem.Icon.Clone();
                icon = rawItem.Icon;
                name = rawItem.Text;

                IPAddress[] ips = Dns.GetHostAddresses(name);

                bool gotIPv4 = false, gotIPv6 = false;
                foreach (IPAddress ip in ips)
                {
                    if (!gotIPv4 && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        hostIPv4 = ip.ToString();
                        gotIPv4 = true;
                    }
                    if (!gotIPv6 && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        hostIPv6 = ip.ToString();
                        gotIPv6 = true;
                    }
                }
            }

            public string hostIPv4 { set; get; }
            public string hostIPv6 { set; get; }
        }
        private class DataGridRowModle_dirNFile : DataGridRowModleBase, INotifyPropertyChanged
        {
            public IOInfoShadow iois;
            public TaskScheduler uiContext = TaskScheduler.FromCurrentSynchronizationContext();
            public DataGridRowModle_dirNFile(object treeViewModleDir_fileIoInfoShadow)
            {
                if (treeViewModleDir_fileIoInfoShadow is VM.TreeViewModelDir)
                {
                    VM.TreeViewModelDir item = (VM.TreeViewModelDir)treeViewModleDir_fileIoInfoShadow;
                    iois = item.dirInfo;
                    icon = item.Icon;
                    isIconReady = true;
                    name = item.Text;
                }
                else
                {
                    iois = (IOInfoShadow)treeViewModleDir_fileIoInfoShadow;
                    Task.Factory.StartNew(() =>
                    {
                        IconHelper.FileSystem fsicon = IconHelper.FileSystem.Instance;
                        BitmapSource bitmap;
                        if (fsicon.HaveIcon(iois.fullName, true, iois.attributes.directory))
                        {
                            //bitmap = fsicon.GetIcon(iois.fullName, true, iois.attributes.directory);
                            //bitmap.Freeze();
                            //icon = bitmap;
                            icon = fsicon.GetIcon(iois.fullName, true, iois.attributes.directory);
                            isIconReady = true;
                        }
                        else
                        {
                            //bitmap = IconHelper.Shell32Icons.GetInstance().GetIcon(20, false);
                            //bitmap?.Freeze();
                            //icon = bitmap;
                            icon = IconHelper.Shell32Icons.Instance.GetIcon(20, false);
                            fsicon.GetIconAsync(iois.fullName, true, iois.attributes.directory);
                        }
                    });//, CancellationToken.None, TaskCreationOptions.None, uiContext);
                    name = iois.name;
                }
                _EditingTx = iois.name;

                modifyTimeTx = iois.lastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                fileType = iois.extension;
                fileSize = iois.length;
                if (iois.attributes.directory)
                {
                    fileSizeTxN = "--";
                }
                else
                {
                    if (fileSize == 0)
                    {
                        fileSizeTxN = "0";
                        fileSizeTxU = " B";
                    }
                    else
                    {
                        string tmp = SimpleStringHelper.UnitsOfMeasure.GetShortString(fileSize, "B", 1024);
                        int tmpI = tmp.IndexOf(' ');
                        fileSizeTxN = tmp.Substring(0, tmpI);
                        fileSizeTxU = tmp.Substring(tmpI + 1);
                    }
                }
                attributes = iois.attributes.ToShortString7();
            }
            public bool isIconReady = false;
            private BitmapSource _icon;

            public event PropertyChangedEventHandler PropertyChanged;
            public void RaisePropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            private string _EditingTx;
            public string EditingTx
            {
                set
                {
                    if (value != name)
                    {
                        try
                        {
                            string targetFullName
                                = System.IO.Path.Combine(
                                    System.IO.Path.GetDirectoryName(iois.fullName),
                                    value);
                            if (iois.attributes.directory)
                            {
                                Directory.Move(iois.fullName, targetFullName);
                            }
                            else
                            {
                                File.Move(iois.fullName, targetFullName);
                            }
                            iois.fullName = targetFullName;
                            iois.name = value;
                            name = value;
                            _EditingTx = value;
                            RaisePropertyChanged("name");
                        }
                        catch (Exception err)
                        {
                            _EditingTx = iois.name;
                            MessageBox.Show(err.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                }
                get => _EditingTx;
            }
            public string modifyTimeTx { set; get; }
            public string fileType { set; get; }
            public long fileSize { set; get; }
            public string fileSizeTxN { set; get; }
            public string fileSizeTxU { set; get; }
            public string attributes { set; get; }
        }
        private class DataGridRowModle_searchResult : DataGridRowModleBase, INotifyPropertyChanged
        {
            public IOInfoShadow iois;
            public TaskScheduler uiContext = TaskScheduler.FromCurrentSynchronizationContext();
            public DataGridRowModle_searchResult(IOInfoShadow iois)
            {
                Task.Factory.StartNew(() =>
                {
                    IconHelper.FileSystem fsicon = IconHelper.FileSystem.Instance;
                    BitmapSource bitmap;
                    if (fsicon.HaveIcon(iois.fullName, true, iois.attributes.directory))
                    {
                        //bitmap = fsicon.GetIcon(iois.fullName, true, iois.attributes.directory);
                        //bitmap.Freeze();
                        //icon = bitmap;
                        icon = fsicon.GetIcon(iois.fullName, true, iois.attributes.directory);
                        isIconReady = true;
                    }
                    else
                    {
                        //bitmap = IconHelper.Shell32Icons.GetInstance().GetIcon(20, false);
                        //bitmap?.Freeze();
                        //icon = bitmap;
                        icon = IconHelper.Shell32Icons.Instance.GetIcon(20, false);
                        fsicon.GetIconAsync(iois.fullName, true, iois.attributes.directory);
                    }
                });//, CancellationToken.None, TaskCreationOptions.None, uiContext);
                name = iois.name;

                modifyTimeTx = iois.lastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                fileSize = iois.length;
                if (iois.attributes.directory)
                {
                    fileSizeTxN = "--";
                }
                else
                {
                    if (fileSize == 0)
                    {
                        fileSizeTxN = "0";
                        fileSizeTxU = " B";
                    }
                    else
                    {
                        string tmp = SimpleStringHelper.UnitsOfMeasure.GetShortString(fileSize, "B", 1024);
                        int tmpI = tmp.IndexOf(' ');
                        fileSizeTxN = tmp.Substring(0, tmpI);
                        fileSizeTxU = tmp.Substring(tmpI + 1);
                    }
                }
                attributes = iois.attributes.ToShortString7();
                parentPath = iois.directoryName;
            }
            public bool isIconReady = false;
            private BitmapSource _icon;

            public event PropertyChangedEventHandler PropertyChanged;
            public void RaisePropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public string modifyTimeTx { set; get; }
            public long fileSize { set; get; }
            public string fileSizeTxN { set; get; }
            public string fileSizeTxU { set; get; }
            public string attributes { set; get; }
            public string parentPath { set; get; }
        }

        #endregion


        #endregion


        #region goto uri, focus to tree node




        #endregion


        #region user sort data grid
        private myDataGridSorter dataGrid_sorter;

        private void dataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (dataGridItemSource_loading || dataGridItemSource.Count <= 0)
                return;

            if (dataGrid_sorter == null)
                dataGrid_sorter = new myDataGridSorter(this);

            if (dataGrid_sorter.CheckNeedMySorter(dataGridItemSource[0], e.Column))
            {
                e.Handled = true;
                dataGrid_sorter.SetSortColumnType(e.Column);

                //use a ListCollectionView to do the sort.
                ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(dataGridItemSource);

                lcv.CustomSort = dataGrid_sorter;
            }
        }
        public class myDataGridSorter : IComparer
        {
            public myDataGridSorter(FileFolderSelector parentWindow)
            {
                this.parentWindow = parentWindow;
                //comparer = Comparer.Default;
                SortColumnType = SortColumnTypes.Normal;
            }

            private DataGridColumn column;
            private FileFolderSelector parentWindow;
            internal void SetSortColumnType(DataGridColumn column)
            {
                string headerName = column.Header.ToString();
                if (this.column == column)
                {
                    if (column.SortDirection == ListSortDirection.Ascending)
                        column.SortDirection = ListSortDirection.Descending;
                    else
                        column.SortDirection = ListSortDirection.Ascending;
                }
                else
                {
                    column.SortDirection = ListSortDirection.Ascending;
                }
                this.column = column;
            }
            public bool CheckNeedMySorter(object testRowItem, DataGridColumn column)
            {
                string colHeader = column.Header.ToString();
                if (testRowItem is DataGridRowModle_link)
                {
                    if (colHeader == parentWindow.dataGrid_dataTable_col_iconName)
                        return true;
                }
                else if (testRowItem is DataGridRowModle_disk)
                {
                    if (colHeader == parentWindow.dataGrid_dataTable_col_iconName)
                        return true;
                }
                else if (testRowItem is DataGridRowModle_dirNFile)
                {
                    if (colHeader == parentWindow.dataGrid_dataTable_col_iconName)
                        return true;
                    if (colHeader == parentWindow.dataGrid_dataTable_col_fileType)
                        return true;
                    if (colHeader == parentWindow.dataGrid_dataTable_col_fileSize)
                        return true;
                }
                else if (testRowItem is DataGridRowModle_host)
                {
                    if (colHeader == parentWindow.dataGrid_dataTable_col_iconName)
                        return true;
                }

                return false;
            }
            public int Compare(object x, object y)
            {
                long vlx = -10, vly = -10;
                string vsx = null, vsy = null;
                double vdx = -10, vdy = -10;
                string colHeader = column.Header.ToString();

                #region check col, get value
                if (x is DataGridRowModle_link)
                {
                    if (colHeader == parentWindow.dataGrid_dataTable_col_iconName)
                    {
                        vsx = ((DataGridRowModle_link)x).name;
                        vsy = ((DataGridRowModle_link)y).name;
                        SortColumnType = SortColumnTypes.Name;
                    }
                }
                else if (x is DataGridRowModle_disk)
                {
                    if (colHeader == parentWindow.dataGrid_dataTable_col_iconName)
                    {
                        vsx = ((DataGridRowModle_disk)x).name;
                        vsy = ((DataGridRowModle_disk)y).name;
                        SortColumnType = SortColumnTypes.Name;
                    }
                }
                else if (x is DataGridRowModle_dirNFile)
                {
                    DataGridRowModle_dirNFile itemX, itemY;
                    if (colHeader == parentWindow.dataGrid_dataTable_col_iconName)
                    {
                        itemX = (DataGridRowModle_dirNFile)x;
                        itemY = (DataGridRowModle_dirNFile)y;
                        if (itemX.iois.attributes.directory == itemY.iois.attributes.directory)
                        {
                            vsx = itemX.name;
                            vsy = itemY.name;
                        }
                        else
                        {
                            if (itemX.iois.attributes.directory)
                            {
                                vsx = "a";
                                vsy = "b";
                            }
                            else
                            {
                                vsx = "b";
                                vsy = "a";
                            }
                        }
                        SortColumnType = SortColumnTypes.Name;
                    }
                    else if (colHeader == parentWindow.dataGrid_dataTable_col_fileType)
                    {
                        itemX = (DataGridRowModle_dirNFile)x;
                        itemY = (DataGridRowModle_dirNFile)y;
                        if (itemX.iois.attributes.directory == itemY.iois.attributes.directory)
                        {
                            vsx = itemX.fileType;
                            vsy = itemY.fileType;
                        }
                        else
                        {
                            if (itemX.iois.attributes.directory)
                            {
                                vsx = "a";
                                vsy = "b";
                            }
                            else
                            {
                                vsx = "b";
                                vsy = "a";
                            }
                        }
                        SortColumnType = SortColumnTypes.Normal;
                    }
                    else if (colHeader == parentWindow.dataGrid_dataTable_col_fileSize)
                    {
                        itemX = (DataGridRowModle_dirNFile)x;
                        if (itemX.iois.attributes.directory) vlx = -1;
                        else vlx = itemX.fileSize;
                        itemX = (DataGridRowModle_dirNFile)y;
                        if (itemX.iois.attributes.directory) vly = -1;
                        else vly = itemX.fileSize;
                        SortColumnType = SortColumnTypes.Size;
                    }
                }
                else if (x is DataGridRowModle_host)
                {
                    if (colHeader == parentWindow.dataGrid_dataTable_col_iconName)
                    {
                        vsx = ((DataGridRowModle_host)x).name;
                        vsy = ((DataGridRowModle_host)y).name;
                        SortColumnType = SortColumnTypes.Name;
                    }
                }
                else
                {
                    return 0;
                }
                #endregion

                int compareResult;
                switch (SortColumnType)
                {
                    default:
                    case SortColumnTypes.Normal:
                        if (vsx != null)
                            compareResult = vsx.CompareTo(vsy);
                        else if (vlx > -10)
                            compareResult = vlx.CompareTo(vly);
                        else if (vdx > -10)
                            compareResult = vdx.CompareTo(vdy);
                        else
                            compareResult = 0;
                        break;
                    case SortColumnTypes.Name:
                        double? nx = GetPrefixNum(vsx);
                        if (nx == null)
                        {
                            compareResult = vsx.CompareTo(vsy);
                        }
                        else
                        {
                            double? ny = GetPrefixNum(vsy);
                            if (ny == null)
                            {
                                compareResult = vsx.CompareTo(vsy);
                            }
                            else
                            {
                                compareResult = ((double)nx).CompareTo((double)ny);
                            }
                        }
                        break;
                    case SortColumnTypes.Size:
                        compareResult = vlx.CompareTo(vly);
                        break;
                    case SortColumnTypes.Ratio:
                        compareResult = vdx.CompareTo(vdy);
                        break;
                }

                if (column.SortDirection == ListSortDirection.Descending)
                    compareResult = -compareResult;
                return compareResult;

                double? GetPrefixNum(string name)
                {
                    if (string.IsNullOrWhiteSpace(name))
                        return null;

                    name = name.Trim();
                    StringBuilder numBdr = new StringBuilder();
                    int i = 0, iv = name.Length;
                    char c;
                    while (i < iv)
                    {
                        c = name[i++];
                        if (c == '.' || (c >= '0' && c <= '9'))
                            numBdr.Append(c);
                        else
                        {
                            if (c >= 'A')
                                return null;
                            break;
                        }
                    }
                    if (numBdr.Length == 0)
                        return null;
                    double.TryParse(numBdr.ToString(), out double result);
                    return result;
                }
            }


            //private Comparer comparer;

            public enum SortColumnTypes
            { Normal, Name, Size, Ratio, }
            private SortColumnTypes sortColumnType;
            public SortColumnTypes SortColumnType
            {
                get { return sortColumnType; }
                set { sortColumnType = value; }
            }
        }

        #endregion


        public class ResentFilesCache
        {
            private string cacheFile;
            public ResentFilesCache()
            {
                string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                dir = System.IO.Path.Combine(dir, "MadTomDev");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                cacheFile = System.IO.Path.Combine(dir, "BrowserDialogWPF.recents");
                Reload();
            }
            /// <summary>
            /// 存有历届写入的地址；第一个是最新加入的条目
            /// </summary>
            public List<string> cache = new List<string>();
            public void Reload()
            {
                cache.Clear();
                if (File.Exists(cacheFile))
                {
                    foreach (string p in File.ReadAllLines(cacheFile))
                    {
                        AddSet(p);
                    }
                }
            }
            public void Save()
            {
                using (Stream fs = File.OpenWrite(cacheFile))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    for (int i = 0, iv = cache.Count; i < iv; i++)
                    {
                        sw.WriteLine(cache[i]);
                    }
                    fs.Flush();
                }
            }
            public int cacheMaxCount = 10;
            public void AddSet(string path)
            {
                if (cache.Contains(path))
                    cache.Remove(path);
                cache.Insert(0, path);
                while (cache.Count > cacheMaxCount)
                    cache.RemoveAt(cache.Count - 1);
            }
        }
        private ResentFilesCache resentFilesCache = new ResentFilesCache();

        #region data grid selection changed
        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItems.Count == 0)
            {
                selected_dirs.Clear();
                selected_files.Clear();
                if (isSaveFile)
                {
                    //cb_selected.Text = "";
                    btn_ok.IsEnabled = true;
                }
                else if (canSelectDirs && isSingleSelectDir)
                {
                    btn_ok.IsEnabled = true;
                }
                else
                {
                    btn_ok.IsEnabled = false;
                }
            }
            else
            {
                if (isSaveFile)
                {
                    cb_selected.Text = "";
                    List<string> curSelectedFiles = new List<string>();
                    List<string> curSelectedDirs = new List<string>();
                    LoadCurSelectedDataGridItems(ref curSelectedFiles, ref curSelectedDirs);
                    if (curSelectedDirs.Count > 0)
                    {
                        btn_ok.IsEnabled = false;
                    }
                    else
                    {
                        if (curSelectedFiles.Count > 1)
                        {
                            btn_ok.IsEnabled = false;
                        }
                        else if (curSelectedFiles.Count == 1)
                        {
                            cb_selected_addName(selected_files[0]);
                            btn_ok.IsEnabled = true;
                        }
                        else
                        {
                            btn_ok.IsEnabled = true;
                        }
                    }
                }
                else // selected mode
                {
                    object selectedTVNodeItem = treeView.SelectedItem;
                    bool isOddSelection = false;
                    if (selectedTVNodeItem == null)
                    {
                    }
                    else if (canSelectDirs
                        && (selectedTVNodeItem is VM.TreeViewModelPC
                            || selectedTVNodeItem is VM.TreeViewModelHost))
                    {
                        // only folders(disks)
                        List<string> curSelectedFiles = new List<string>();
                        List<string> curSelectedDirs = new List<string>();
                        LoadCurSelectedDataGridItems(ref curSelectedFiles, ref curSelectedDirs);
                        //LoadSetSelectFiles(ref curSelectedFiles);
                        selected_files.Clear();
                        selected_dirs.Clear();
                        if (isSingleSelectDir)
                        {
                            if (curSelectedDirs.Count == 1)
                                selected_dirs.Add(curSelectedDirs[0]);
                        }
                        else
                        {
                            selected_dirs.AddRange(curSelectedDirs);
                        }
                    }
                    else if ((canSelectDirs || canSelectFiles)
                        && (selectedTVNodeItem is VM.TreeViewModelDisk
                            || selectedTVNodeItem is VM.TreeViewModelDir)
                        )
                    {
                        List<string> curSelectedFiles = new List<string>();
                        List<string> curSelectedDirs = new List<string>();
                        LoadCurSelectedDataGridItems(ref curSelectedFiles, ref curSelectedDirs);
                        selected_files.Clear();
                        selected_dirs.Clear();
                        if (canSelectDirs)
                        {
                            if (isSingleSelectDir)
                            {
                                if (curSelectedDirs.Count == 1)
                                    selected_dirs.Add(curSelectedDirs[0]);
                                else if (curSelectedDirs.Count > 1)
                                    isOddSelection = true;
                            }
                            else
                            {
                                selected_dirs.AddRange(curSelectedDirs);
                            }
                        }
                        if (canSelectFiles)
                        {
                            if (isSingleSelectFile)
                            {
                                if (curSelectedFiles.Count == 1)
                                    selected_files.Add(curSelectedFiles[0]);
                                else if (curSelectedFiles.Count > 1)
                                    isOddSelection = true;
                            }
                            else
                            {
                                selected_files.AddRange(curSelectedFiles);
                            }
                        }
                    }

                    cb_selected.Text = "";
                    if (!isOddSelection)
                    {
                        foreach (string d in selected_dirs)
                            cb_selected_addName(System.IO.Path.GetFileName(d));
                        foreach (string f in selected_files)
                            cb_selected_addName(System.IO.Path.GetFileName(f));
                    }

                    btn_ok.IsEnabled = !isOddSelection && (selected_dirs.Count > 0 || selected_files.Count > 0);
                }


                void LoadCurSelectedDataGridItems(ref List<string> files, ref List<string> dirs)
                {
                    List<string> result = new List<string>();
                    DataGridRowModle_dirNFile dirOrFile;
                    DataGridRowModle_searchResult searchItem;
                    foreach (object o in dataGrid.SelectedItems)
                    {
                        if (o is DataGridRowModle_disk)
                        {
                            dirs.Add(((DataGridRowModle_disk)o).diskInfo.Name);
                        }
                        else if (o is DataGridRowModle_dirNFile)
                        {
                            dirOrFile = (DataGridRowModle_dirNFile)o;
                            if (dirOrFile.iois.attributes.archive)
                                files.Add(dirOrFile.iois.fullName);
                            else if (dirOrFile.iois.attributes.directory)
                                dirs.Add(dirOrFile.iois.fullName);
                        }
                        else if (o is DataGridRowModle_searchResult)
                        {
                            searchItem = (DataGridRowModle_searchResult)o;
                            if (searchItem.iois.attributes.archive)
                                files.Add(searchItem.iois.fullName);
                            else if (searchItem.iois.attributes.directory)
                                dirs.Add(searchItem.iois.fullName);
                        }
                    }
                }

            }
        }
        private void cb_selected_addName(string name)
        {
            string tx = cb_selected.Text;
            if (tx.Length == 0)
                tx = name;
            else if (tx.StartsWith("\""))
                tx = tx + ", \"" + name + "\"";
            else
                tx = "\"" + tx + "\", \"" + name + "\"";
            cb_selected.Text = tx;
        }

        #endregion

        private void dataGrid_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGrid_isEditing)
                return;
            dataGrid_activateSelection();
        }
        private void dataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (dataGrid_isEditing)
                return;
            switch (e.Key)
            {
                case Key.Enter:
                    e.Handled = true;
                    dataGrid_activateSelection();
                    break;
                case Key.Delete:
                    if ((treeView.SelectedItem is VM.TreeViewModelDisk
                        || treeView.SelectedItem is VM.TreeViewModelDir)
                        && dataGrid.SelectedItems.Count > 0)
                    {
                        List<string> i2dList = new List<string>();
                        List<object> i2dItems = new List<object>();
                        foreach (object o in dataGrid.SelectedItems)
                        {
                            i2dList.Add(((DataGridRowModle_dirNFile)o).iois.fullName);
                            i2dItems.Add(o);
                        }
                        Utilities.MSVBFileOperation.Delete(i2dList.ToArray(), out Exception err);
                        if (err == null)
                        {
                            foreach (object o in i2dItems)
                                dataGridItemSource.Remove(o);
                        }
                    }
                    break;
            }
        }
        private void dataGrid_activateSelection()
        {
            if (dataGrid_isEditing)
                return;
            if (dataGrid.SelectedItems.Count == 0)
                return;
            object o;
            if (dataGrid.SelectedItems.Count == 1)
            {
                o = dataGrid.SelectedItems[0];
                if (o is DataGridRowModle_link)
                {
                    GotoUri(((DataGridRowModle_link)o).link);
                }
                else if (o is DataGridRowModle_host)
                {
                    DataGridRowModle_host host = (DataGridRowModle_host)o;
                    GotoUri("\\\\" + host.name);
                }
                else if (o is DataGridRowModle_disk)
                {
                    DataGridRowModle_disk disk = (DataGridRowModle_disk)o;
                    GotoUri(disk.diskInfo.Name);
                }
                else if (o is DataGridRowModle_dirNFile)
                {
                    DataGridRowModle_dirNFile dirOrFile = (DataGridRowModle_dirNFile)o;
                    if (dirOrFile.iois.attributes.directory)
                    {
                        GotoUri(dirOrFile.iois.fullName);
                    }
                    else if (btn_ok.IsEnabled == true)
                    {
                        btn_ok_Click(null, null);
                    }
                }
                else if (o is DataGridRowModle_searchResult)
                {
                    DataGridRowModle_searchResult sr = (DataGridRowModle_searchResult)o;
                    if (sr.iois.attributes.directory)
                    {
                        GotoUri(sr.iois.fullName);
                    }
                    else if (btn_ok.IsEnabled == true)
                    {
                        btn_ok_Click(null, null);
                    }
                }
            }
            else // dataGrid.SelectedItems.Count > 1
            {
                if (btn_ok.IsEnabled == true)
                {
                    btn_ok_Click(null, null);
                }
            }
        }




        private bool dataGrid_isEditing = false;


        private void dataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Header.ToString() == dataGrid_dataTable_col_iconName)
            {
                object o = e.Row.Item;
                if (o is DataGridRowModle_disk
                    || o is DataGridRowModle_dirNFile)
                {
                    dataGrid_isEditing = true;
                }
                else
                {
                    e.Cancel = true;
                    dataGrid_isEditing = false;
                }
            }
            else
            {
                e.Cancel = true;
                dataGrid_isEditing = false;
            }
        }
        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            dataGrid_isEditing = false;
            //tb_datagrid_textCellEditing.Visibility = Visibility.Hidden;
        }


        private void btn_createDir_Click(object sender, RoutedEventArgs e)
        {
            if (treeView.SelectedItem is VM.TreeViewModelDisk)
            {
                CreateDirNEditLabel(((VM.TreeViewModelDisk)treeView.SelectedItem).diskInfo.Name);
            }
            else if (treeView.SelectedItem is VM.TreeViewModelDir)
            {
                CreateDirNEditLabel(((VM.TreeViewModelDir)treeView.SelectedItem).dirInfo.fullName);
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
            }

            void CreateDirNEditLabel(string parentDir)
            {
                #region 寻找空白文件夹名称，并创建
                DirectoryInfo pDi = new DirectoryInfo(parentDir);
                DirectoryInfo[] subDi;
                string dirNameBase = "新建文件夹";
                string dirName = dirNameBase;
                int tmp1 = 1;
                bool dnFound;
                do
                {
                    dnFound = false;
                    subDi = pDi.GetDirectories(dirName);
                    foreach (DirectoryInfo sd in subDi)
                    {
                        if (sd.Name == dirName)
                        {
                            dnFound = true;
                            break;
                        }
                    }
                    if (dnFound)
                    {
                        dirName = $"{dirNameBase} {tmp1++}";
                    }
                }
                while (dnFound);

                string newDir = System.IO.Path.Combine(parentDir, dirName);
                DirectoryInfo nDi = null;
                try
                {
                    nDi = Directory.CreateDirectory(newDir);
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.ToString(), "创建文件夹错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                #endregion

                Dispatcher.BeginInvoke(() =>
                {
                    DataGridRowModle_dirNFile newDirM = new DataGridRowModle_dirNFile(new IOInfoShadow(nDi));
                    dataGridItemSource.Add(newDirM);
                    dataGrid.Focus();
                    dataGrid.SelectedItem = newDirM;
                    DataGridCellInfo cellInfo = new DataGridCellInfo(newDirM, dataGrid.Columns[0]);
                    dataGrid.CurrentCell = cellInfo;
                    dataGrid.BeginEdit();
                }, DispatcherPriority.Background);
            }
        }




        #region search
        private void tb_search_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                btn_search_Click(null, null);
        }
        private string searchText;
        private bool search_running = false;
        private bool search_cancelFlag = false;
        private async void btn_search_Click(object sender, RoutedEventArgs e)
        {
            if (search_running)
            {
                search_cancelFlag = true;
                btn_search.IsEnabled = false;
                await Task.Run(() =>
                {
                    while (search_running)
                    {
                        Task.Delay(10).Wait();
                    }
                });
                btn_search.IsEnabled = true;
            }
            searchText = tb_search.Text.Trim();
            if (string.IsNullOrWhiteSpace(searchText))
                return;

            if (treeView.SelectedItem is VM.TreeViewModelContainer)
                return;

            dataGridItemSource.Clear();
            dataGrid_setColumns_searchResult();
            searchChecker.ClearPatterns();
            if (searchText.Contains("*"))
                searchChecker.AddPattern(searchText);
            else
                searchChecker.AddPattern($"*{searchText}*");

            btn_closeSearch.Visibility = Visibility.Visible;
            dataGrid.Cursor = Cursors.Wait;
            search_running = true;
            object treeNode = treeView.SelectedItem;
            Task.Run(() =>
            {
                SearchLoop(treeNode);
                search_cancelFlag = false;
                search_running = false;
                Dispatcher.Invoke(() =>
                {
                    btn_closeSearch.Visibility = Visibility.Collapsed;
                    dataGrid.Cursor = Cursors.Arrow;
                });
            });
        }
        private void btn_closeSearch_Click(object sender, RoutedEventArgs e)
        {
            search_cancelFlag = true;
            btn_closeSearch.Visibility = Visibility.Collapsed;
        }
        private async void SearchLoop(object treeNode)
        {
            if (search_cancelFlag)
                return;

            if (treeNode is VM.TreeViewModelPC)
            {
                VM.TreeViewModelPC pcTN = (VM.TreeViewModelPC)treeNode;
                if (!pcTN.isLoaded)
                {
                    await pcTN.LoadChildrenAsync();
                }
                foreach (object diskTN in pcTN.Children)
                {
                    if (search_cancelFlag)
                        return;
                    SearchLoop(diskTN);
                }
            }
            else if (treeNode is VM.TreeViewModelNetWork)
            {
                VM.TreeViewModelNetWork netTN = (VM.TreeViewModelNetWork)treeNode;
                if (!netTN.isLoaded)
                {
                    await netTN.LoadChildrenAsync();
                }
                foreach (object hostTN in netTN.Children)
                {
                    if (search_cancelFlag)
                        return;
                    SearchLoop(hostTN);
                }
            }
            else if (treeNode is VM.TreeViewModelHost)
            {
                VM.TreeViewModelHost hostTN = (VM.TreeViewModelHost)treeNode;
                if (!hostTN.isLoaded)
                {
                    await hostTN.LoadChildrenAsync();
                }
                foreach (object shareDir in hostTN.Children)
                {
                    if (search_cancelFlag)
                        return;
                    SearchLoop(shareDir);
                }
            }
            else if (treeNode is VM.TreeViewModelDisk)
            {
                SearchLoop(((VM.TreeViewModelDisk)treeNode).diskInfo.RootDirectory);
            }
            else if (treeNode is VM.TreeViewModelDir)
            {
                SearchLoop(new DirectoryInfo(((VM.TreeViewModelDir)treeNode).dirInfo.fullName));
            }
        }
        Common.SimpleStringHelper.Checker_starNQues searchChecker = new Common.SimpleStringHelper.Checker_starNQues();
        private async void SearchLoop(DirectoryInfo dirInfo)
        {
            if (search_cancelFlag)
                return;

            try
            {
                foreach (FileInfo subFi in dirInfo.GetFiles())
                {
                    if (search_cancelFlag)
                        return;

                    if (searchChecker.Check(subFi.Name))
                        SearchLoop_addResult(subFi);
                }
            }
            catch (Exception) { }

            try
            {
                foreach (DirectoryInfo subDi in dirInfo.GetDirectories())
                {
                    if (search_cancelFlag)
                        return;

                    if (searchChecker.Check(subDi.Name))
                        SearchLoop_addResult(subDi);

                    SearchLoop(subDi);
                }
            }
            catch (Exception) { }

        }
        private void SearchLoop_addResult(DirectoryInfo dir)
        {
            Dispatcher.Invoke(() =>
            {
                dataGridItemSource.Add(new DataGridRowModle_searchResult(new IOInfoShadow(dir)));
            });
        }
        private void SearchLoop_addResult(FileInfo file)
        {
            Dispatcher.Invoke(() =>
            {
                dataGridItemSource.Add(new DataGridRowModle_searchResult(new IOInfoShadow(file)));
            });
        }

        #endregion


        #region view history

        ViewHistory historyMgr = new ViewHistory(10, "\\");


        private void btn_arrowLeft_Click(object sender, RoutedEventArgs e)
        {
            if (historyMgr.CanGetPrev)
            {
                ViewHistory.Item his = historyMgr.Get_Entry(-1);
                GotoUri(his.fullName);
            }
        }

        private void btn_arrowRight_Click(object sender, RoutedEventArgs e)
        {
            if (historyMgr.CanGetNext)
            {
                ViewHistory.Item ftr = historyMgr.Get_Entry(1);
                GotoUri(ftr.fullName);
            }
        }
        ContextMenu historyContextMenu = null;
        private void btn_historyArrowDown_Click(object sender, RoutedEventArgs e)
        {
            if (historyMgr.Count <= 1)
                return;

            if (historyContextMenu == null)
            {
                historyContextMenu = new ContextMenu();
                historyContextMenu.PlacementTarget = btn_arrowRight;
                historyContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            }

            foreach (MenuItem i in historyContextMenu.Items)
            {
                i.Click -= historyItem_Click;
            }
            historyContextMenu.Items.Clear();

            List<ViewHistory.Item> futureItems = historyMgr.Read_Future();
            for (int i = futureItems.Count - 1, im = 0; i >= im; i--)
            {
                history_AddNewHItem(futureItems[i], true);
            }
            history_AddNewHItem(historyMgr.Read(), false);
            List<ViewHistory.Item> historyItems = historyMgr.Read_History();
            for (int i = 0, iv = historyItems.Count; i < iv; i++)
            {
                history_AddNewHItem(historyItems[i], true);
            }
            historyContextMenu.IsOpen = true;
        }
        private void history_AddNewHItem(ViewHistory.Item hItem, bool isEnabled)
        {
            MenuItem newHItem = new MenuItem()
            {
                Header = hItem.label,
                Icon = new Image() { Source = hItem.image },
                Tag = hItem,
                IsEnabled = isEnabled,
            };
            newHItem.Click += historyItem_Click;
            historyContextMenu.Items.Add(newHItem);
        }

        private void historyItem_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            GotoUri(((ViewHistory.Item)((MenuItem)sender).Tag).fullName);
        }

        private void btn_arrowUp_Click(object sender, RoutedEventArgs e)
        {
            VM.TreeViewNodeModelBase tn = (VM.TreeViewNodeModelBase)treeView.SelectedItem;
            if (tn.Level > 0)
            {
                //Dispatcher.BeginInvoke(() =>
                //{
                ((VM.TreeViewNodeModelBase)tn.parent).IsSelected = true;
                //}, DispatcherPriority.Background);
            }
        }
        #endregion


        private void btn_test_Click(object sender, RoutedEventArgs e)
        {
            //dataGrid.SelectedItems.RemoveAt(1);




        }


        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            // 检查选择结果，(不)给出消息框
            if (string.IsNullOrWhiteSpace(cb_selected.Text))
                return;
            if (treeView.SelectedItem is VM.TreeViewModelContainer
                || treeView.SelectedItem is VM.TreeViewModelNetWork)
            {
                return;
            }

            // 设置返回结果数据
            HashSet<char> missing;
            if (isSaveFile)
            {
                saveFileName = Utilities.FilePath.CorrectorName(cb_selected.Text, out missing);
                if (missing.Count > 0)
                {
                    if (MessageBox.Show("保存文件名中出现非法字符，" + Environment.NewLine
                            + "是否自动更正？", "发现无效字符", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                        == MessageBoxResult.No)
                        return;
                }
                string baseDir = null;
                if (treeView.SelectedItem is VM.TreeViewModelDisk)
                    baseDir = ((VM.TreeViewModelDisk)treeView.SelectedItem).diskInfo.Name;
                else
                    baseDir = ((VM.TreeViewModelDir)treeView.SelectedItem).dirInfo.fullName;
                saveFileFullName = System.IO.Path.Combine(baseDir, saveFileName);
            }

            // 将当前选择结果的第一条，写入最近文件缓存中；
            if (isSaveFile)
            {
                resentFilesCache.AddSet(saveFileFullName);
            }
            else
            {
                if (selected_dirs.Count > 0)
                    resentFilesCache.AddSet(selected_dirs[0]);
                else if (selected_files.Count > 0)
                    resentFilesCache.AddSet(selected_files[0]);
            }
            resentFilesCache.Save();

            // 关闭窗口
            this.DialogResult = true;
            this.Close();
        }
        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
