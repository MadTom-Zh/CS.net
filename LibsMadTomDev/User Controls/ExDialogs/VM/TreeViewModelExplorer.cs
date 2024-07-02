using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

using MadTomDev.Data;
using FSIcon = MadTomDev.Common.IconHelper.FileSystem;
using S32Icon = MadTomDev.Common.IconHelper.Shell32Icons;
using System.Windows;
using System.Threading;

namespace MadTomDev.UI.VM
{
    public class TreeViewModelExplorerBase : TreeViewNodeModelBase
    {
        private BitmapSource _Icon;
        public BitmapSource Icon
        {
            set
            {
                _Icon = value;
                RaisePCEvent("Icon");
            }
            get => _Icon;
        }

        public TreeViewModelExplorerBase(object parent) : base(parent)
        {
        }
    }
    public sealed class TreeViewModelContainer : TreeViewModelExplorerBase
    {
        public TreeViewModelContainer(object parent) : base(parent)
        {

        }
    }
    public sealed class TreeViewModelLink : TreeViewModelExplorerBase
    {
        public TreeViewModelLink(object parent) : base(parent)
        {

        }
        public IOInfoShadow link;
    }

    public abstract class TreeViewModelIORefreashable : TreeViewModelExplorerBase
    {
        public TreeViewModelIORefreashable(object parent, DoCollapseRecycleDelegate DoCollapseRecycle) : base(parent)
        {
            Children = new ObservableCollection<object>();
            this.DoCollapseRecycle = DoCollapseRecycle;

            ExpandedChanged = HandleExpandedChanged;
        }
        private void HandleExpandedChanged()
        {
            if (IsExpanded)
            {
                if (!isLoaded)
                    LoadChildrenAsync();
            }
            else
            {
                if (!LoadingChildren)
                {
                    if (DoCollapseRecycle == null || DoCollapseRecycle.Invoke(this))
                    {
                        isLoaded = false;
                        Children.Clear();
                        AddLoadingLabelNode();
                    }
                }
            }
        }
        public void AddLoadingLabelNode()
        {
            Children.Add(new TreeViewNodeModelBase(this)
            {
                Text = "loading...",
                Children = null,
            });
        }

        public abstract string GetFullPath();
        public delegate bool DoCollapseRecycleDelegate(TreeViewModelIORefreashable collapsingNode);
        public DoCollapseRecycleDelegate DoCollapseRecycle;
        public bool isLoaded = false;
        public delegate void LoadCompleteDelegate(object sender);
        public abstract event LoadCompleteDelegate LoadComplete;
        public TaskScheduler uiContext = TaskScheduler.FromCurrentSynchronizationContext();
        public bool LoadingChildren = false;
        public abstract Task LoadChildrenAsync();
    }
    public sealed class TreeViewModelPC : TreeViewModelIORefreashable
    {
        public TreeViewModelPC(object parent, DoCollapseRecycleDelegate DoCollapseRecycle) : base(parent, DoCollapseRecycle)
        {
        }

        public override string GetFullPath()
        {
            return null;
        }


        public override event LoadCompleteDelegate LoadComplete;

        public override Task LoadChildrenAsync()
        {
            if (LoadingChildren)
            {
                return Task.Factory.StartNew(() =>
                {
                    while (LoadingChildren)
                    {
                        Task.Delay(10).Wait();
                    }
                });
            }
            LoadingChildren = true;

            return Task.Factory.StartNew(async () =>
            {
                List<TreeViewModelDisk> newContents = new List<TreeViewModelDisk>();
                TreeViewModelDisk newDisk;
                DriveInfo[] allDrive = DriveInfo.GetDrives();
                await Task.Factory.StartNew(() =>
                {
                    foreach (DriveInfo disk in allDrive)
                    {
                        if (disk.IsReady)
                        {
                            newDisk = new TreeViewModelDisk(this, this.DoCollapseRecycle)
                            {
                                diskInfo = disk,
                                Icon = FSIcon.Instance.GetIcon(disk.Name, true, true),
                                Text = disk.VolumeLabel + " (" + disk.Name + ")",
                            };
                            if (disk.RootDirectory.EnumerateDirectories().Any())
                            {
                                newDisk.AddLoadingLabelNode();
                            }
                            newContents.Add(newDisk);
                        }
                    }
                    //}, CancellationToken.None, TaskCreationOptions.None, uiContext);
                    //await Task.Factory.StartNew(() =>
                    //{
                    Children.Clear();
                    foreach (object o in newContents)
                        Children.Add(o);
                }, CancellationToken.None, TaskCreationOptions.None, uiContext);
                LoadingChildren = false;
                isLoaded = true;
                LoadComplete?.Invoke(this);
            });
        }
    }
    public sealed class TreeViewModelDisk : TreeViewModelIORefreashable
    {
        public TreeViewModelDisk(object parent, DoCollapseRecycleDelegate DoCollapseRecycle) : base(parent, DoCollapseRecycle)
        {

        }

        public DriveInfo diskInfo;
        public override string GetFullPath()
        {
            return diskInfo.Name;
        }


        public override event LoadCompleteDelegate LoadComplete;
        public List<IOInfoShadow> fullContent = new List<IOInfoShadow>();

        public override Task LoadChildrenAsync()
        {
            if (LoadingChildren)
            {
                return Task.Factory.StartNew(() =>
                {
                    while (LoadingChildren)
                    {
                        Task.Delay(10).Wait();
                    }
                });
            }
            LoadingChildren = true;

            return Task.Factory.StartNew(async () =>
            {
                List<TreeViewModelDir> newContent = new List<TreeViewModelDir>();
                DirectoryInfo di = new DirectoryInfo(diskInfo.Name);
                DirectoryInfo[] subDirs = null;
                FileInfo[] subFiles = null;
                try
                {
                    subDirs = di.GetDirectories();
                    subFiles = di.GetFiles();
                }
                catch (Exception )//err)
                {
                    ;
                }
                IOInfoShadow io;
                fullContent.Clear();
                TreeViewModelDir newDir;
                Task.Delay(10).Wait(); // to show "loading..."
                await Task.Factory.StartNew(() =>
                {
                    if (subDirs != null)
                    {
                        foreach (DirectoryInfo subDI in subDirs)
                        {
                            io = new IOInfoShadow(subDI);
                            fullContent.Add(io);
                            newDir = new TreeViewModelDir(this, this.DoCollapseRecycle)
                            {
                                dirInfo = io,
                                Icon = FSIcon.Instance.GetIcon(subDI.FullName, true, true),
                                Text = subDI.Name,
                            };
                            try
                            {
                                if (subDI.EnumerateDirectories().Any())
                                {
                                    newDir.AddLoadingLabelNode();
                                }
                            }
                            catch (Exception err2)
                            {
                                newDir.SetLockDown(err2);
                            }
                            newContent.Add(newDir);
                        }
                    }
                    if (subFiles != null)
                    {
                        foreach (FileInfo subFI in subFiles)
                        {
                            io = new IOInfoShadow(subFI);
                            fullContent.Add(io);
                        }
                    }

                    #region old load files
                    //if (withFiles)
                    //{
                    //    try
                    //    {
                    //        string[] filters = fileFilter.Split("|", StringSplitOptions.RemoveEmptyEntries);
                    //        SimpleStringHelper.Checker_starNQues checker = new SimpleStringHelper.Checker_starNQues();
                    //        foreach (string f in filters)
                    //        {
                    //            if (f == "*.*")
                    //            {
                    //                checker.ClearPatterns();
                    //                checker.AddPattern("*");
                    //                break;
                    //            }
                    //            if (f.Contains('*') || f.Contains('?'))
                    //                checker.AddPattern(f);
                    //        }
                    //        foreach (FileInfo subFI in di.GetFiles())
                    //        {
                    //            if (checker.Check(subFI.Name))
                    //                result.Add(new Data.IOInfoShadow(subFI));
                    //        }
                    //    }
                    //    catch (Exception)
                    //    {
                    //    }
                    //}
                    #endregion

                    Children.Clear();
                    foreach (object o in newContent)
                        Children.Add(o);
                }, CancellationToken.None, TaskCreationOptions.None, uiContext);
                LoadingChildren = false;
                isLoaded = true;
                LoadComplete?.Invoke(this);
            });
        }
    }
    public sealed class TreeViewModelDir : TreeViewModelIORefreashable
    {
        public TreeViewModelDir(object parent, DoCollapseRecycleDelegate DoCollapseRecycle) : base(parent, DoCollapseRecycle)
        {

        }

        public IOInfoShadow dirInfo;
        public override string GetFullPath()
        {
            return dirInfo.fullName;
        }


        public override event LoadCompleteDelegate LoadComplete;
        public List<IOInfoShadow> fullContent = new List<IOInfoShadow>();

        public override Task LoadChildrenAsync()
        {
            if (LoadingChildren)
            {
                return Task.Factory.StartNew(() =>
                {
                    while (LoadingChildren)
                    {
                        Task.Delay(10).Wait();
                    }
                });
            }
            LoadingChildren = true;

            return Task.Factory.StartNew(async () =>
            {
                fullContent.Clear();
                List<TreeViewModelDir> newContent = new List<TreeViewModelDir>();
                DirectoryInfo di = new DirectoryInfo(dirInfo.fullName);
                DirectoryInfo[] subDirs = null;
                FileInfo[] subFiles = null;
                try
                {
                    subDirs = di.GetDirectories();
                    subFiles = di.GetFiles();
                }
                catch (Exception err)
                {
                    ;
                }
                Task.Delay(10).Wait(); // to show "loading..."
                await Task.Factory.StartNew(() =>
                {
                    TreeViewModelDir newDir;
                    IOInfoShadow io;
                    if (subDirs != null)
                    {
                        FSIcon fsicon = FSIcon.Instance;
                        foreach (DirectoryInfo subDI in subDirs)
                        {
                            io = new IOInfoShadow(subDI);
                            fullContent.Add(io);

                            newDir = new TreeViewModelDir(this, this.DoCollapseRecycle)
                            {
                                dirInfo = io,
                                Icon = fsicon.GetDirIcon(true),
                                Text = subDI.Name,
                            };

                            try
                            {
                                if (subDI.EnumerateDirectories().Any())
                                {
                                    newDir.AddLoadingLabelNode();
                                }
                            }
                            catch (Exception err2)
                            {
                                newDir.SetLockDown(err2);
                            }
                            newContent.Add(newDir);
                        }
                    }
                    if (subFiles != null)
                    {
                        foreach (FileInfo subFI in subFiles)
                        {
                            io = new IOInfoShadow(subFI);
                            fullContent.Add(io);
                        }
                    }
                    Children.Clear();
                    foreach (object o in newContent)
                        Children.Add(o);
                }, CancellationToken.None, TaskCreationOptions.None, uiContext);
                LoadingChildren = false;
                isLoaded = true;
                LoadComplete?.Invoke(this);
            });
        }

        internal void SetLockDown(Exception err)
        {
            isLoaded = true;
            Children = null;
            Icon = S32Icon.Instance.GetIcon(234, false);
            //Text += " (" + err.Message + ")";
        }
    }


    public sealed class TreeViewModelNetWork : TreeViewModelIORefreashable
    {
        public TreeViewModelNetWork(object parent, DoCollapseRecycleDelegate DoCollapseRecycle)
            : base(parent, DoCollapseRecycle)
        {

        }

        public override string GetFullPath()
        {
            return null;
        }


        public override event LoadCompleteDelegate LoadComplete;

        public override Task LoadChildrenAsync()
        {
            if (LoadingChildren)
            {
                return Task.Factory.StartNew(() =>
                {
                    while (LoadingChildren)
                    {
                        Task.Delay(10).Wait();
                    }
                });
            }
            LoadingChildren = true;

            return Task.Factory.StartNew(async () =>
            {
                IEnumerable<string> hosts = Network.Hosts.GetVisibleComputers();
                List<TreeViewModelHost> newContent = new List<TreeViewModelHost>();
                TreeViewModelHost newHost;
                await Task.Factory.StartNew(() =>
                {
                    BitmapSource hostIcon = S32Icon.Instance.GetIcon(17, false);
                    foreach (string hostName in hosts)
                    {
                        newHost = new TreeViewModelHost(this, this.DoCollapseRecycle)
                        {
                            hostName = hostName,
                            Icon = hostIcon,
                            Text = hostName
                        };
                        newHost.AddLoadingLabelNode();
                        newContent.Add(newHost);
                    }
                    Children.Clear();
                    foreach (object o in newContent)
                        Children.Add(o);
                }, CancellationToken.None, TaskCreationOptions.None, uiContext);
                LoadingChildren = false;
                isLoaded = true;
                LoadComplete?.Invoke(this);
            });
        }
    }
    public sealed class TreeViewModelHost : TreeViewModelIORefreashable
    {
        public TreeViewModelHost(object parent, DoCollapseRecycleDelegate DoCollapseRecycle)
            : base(parent, DoCollapseRecycle)
        {

        }

        public string hostName;
        private string fullPath = null;
        public override string GetFullPath()
        {
            if (fullPath == null)
                fullPath = "\\\\" + hostName;
            return fullPath;
        }


        public override event LoadCompleteDelegate LoadComplete;

        public override Task LoadChildrenAsync()
        {
            if (LoadingChildren)
            {
                return Task.Factory.StartNew(() =>
                {
                    while (LoadingChildren)
                    {
                        Task.Delay(10).Wait();
                    }
                });
            }
            LoadingChildren = true;

            return Task.Factory.StartNew(async () =>
            {
                List<TreeViewModelDir> newContents = new List<TreeViewModelDir>();
                TreeViewModelDir newDir;
                string subDirFullName;
                DirectoryInfo subDirInfo;
                Network.Hosts.SHARE_INFO_1[] shares = Network.Hosts.EnumNetShares(hostName);
                Task.Delay(10).Wait(); // to show "loading..."
                await Task.Factory.StartNew(() =>
                {
                    foreach (Network.Hosts.SHARE_INFO_1 si in shares)
                    {
                        if (!si.shi1_netname.StartsWith("ERROR=")
                            && !si.shi1_netname.EndsWith('$'))
                        {
                            subDirFullName = Path.Combine(GetFullPath(), si.shi1_netname);
                            subDirInfo = new DirectoryInfo(subDirFullName);
                            newDir = new TreeViewModelDir(this, this.DoCollapseRecycle)
                            {
                                dirInfo = new IOInfoShadow(subDirInfo),
                                Icon = S32Icon.Instance.GetIcon(275, false), // FSIcon.GetInstance().GetIcon(subDirFullName, true, true),
                                Text = si.shi1_netname,
                            };
                            if (subDirInfo.EnumerateDirectories().Any())
                            {
                                newDir.AddLoadingLabelNode();
                            }
                            newContents.Add(newDir);
                        }
                    }
                    Children.Clear();
                    foreach (object o in newContents)
                        Children.Add(o);
                }, CancellationToken.None, TaskCreationOptions.None, uiContext);
                LoadingChildren = false;
                isLoaded = true;
                LoadComplete?.Invoke(this);
            });
        }
    }
}
