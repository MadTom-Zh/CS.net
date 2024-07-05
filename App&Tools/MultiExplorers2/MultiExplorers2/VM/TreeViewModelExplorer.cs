using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

using MadTomDev.Data;
using System.Windows;
using System.Threading;

namespace MadTomDev.App.VM
{
    public class TreeViewModelContainer : UI.VMBase.TreeViewNodeModelBase
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
        private ObservableCollection<object> _Children = null;// = new ObservableCollection<object>();
        public ObservableCollection<object> Children
        {
            set
            {
                _Children = value;
                RaisePCEvent("Children");
            }
            get => _Children;
        }

        public static string TxLoading = "loading...";
        public void AddLoadingLabelNode()
        {
            if (_Children == null)
                _Children = new ObservableCollection<object>();
            Children.Add(new UI.VMBase.TreeViewNodeModelBase(this)
            {
                Text = TxLoading,
            });
        }
        public bool HaveLoadingLabelNode()
        {
            if (Children == null)
                return false;
            object child;
            for (int i = 0, iv = Children.Count; i < iv; i++)
            {
                child = Children[i];
                if (child is UI.VMBase.TreeViewNodeModelBase)
                {
                    if (((UI.VMBase.TreeViewNodeModelBase)child).Text == TxLoading)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public void RemoveLoadingLabelNode()
        {
            if (Children == null)
                return;
            object child;
            for (int i = 0, iv = Children.Count; i < iv; i++)
            {
                child = Children[i];
                if (child is UI.VMBase.TreeViewNodeModelBase)
                {
                    if (((UI.VMBase.TreeViewNodeModelBase)child).Text == TxLoading)
                    {
                        Children.Remove(child);
                        i--; iv--;
                    }
                }
            }
        }

        public TreeViewModelContainer(object parent) : base(parent)
        {

        }
    }
    public sealed class TreeViewModelLink : TreeViewModelContainer
    {
        public TreeViewModelLink(object parent) : base(parent)
        {

        }
        public IOInfoShadow link;
    }

    public abstract class TreeViewModelPhysical : TreeViewModelContainer
    {
        public TreeViewModelPhysical(object parent) : base(parent)
        {
        }
        public List<IOInfoShadow> fullContent = new List<IOInfoShadow>();
        public abstract string GetFullPath();
        public Exception error;
    }
    public sealed class TreeViewModelDisk : TreeViewModelPhysical
    {
        public TreeViewModelDisk(object parent) : base(parent)
        {

        }

        public DriveInfoShadow diskInfo;
        public override string GetFullPath()
        {
            return diskInfo.name;
        }

        public string TextWithLabel
        {
            get
            {
                if (string.IsNullOrWhiteSpace(diskInfo.volumeLabel))
                    return diskInfo.name;
                else
                    return $"{diskInfo.volumeLabel} ({diskInfo.name})";
            }
            set
            {
            }
        }

        internal void UpdateFrom(TreeViewModelDisk newDisk)
        {
            this.diskInfo = newDisk.diskInfo;
            this.Text = newDisk.Text;
            this.Icon = newDisk.Icon;
            this.error = newDisk.error;
        }
    }
    public sealed class TreeViewModelDir : TreeViewModelPhysical
    {
        public TreeViewModelDir(object parent) : base(parent)
        {

        }
        public TreeViewModelDir(object parent, Core.ReloadItemInfo reloadedInfo) : base(parent)
        {
            this.dirInfo = reloadedInfo.ioInfo;
            this.Text = reloadedInfo.Name;

            bool isNetDirRoot = Utilities.FilePath.CheckIsUngRoot(reloadedInfo.ioInfo.fullName);
            if (reloadedInfo.Err != null)
            {
                if (isNetDirRoot)
                    this.Icon = Common.IconHelper.Shell32Icons.Instance.GetIcon(274, false);
                else
                    this.Icon = Common.IconHelper.Shell32Icons.Instance.GetIcon(234, false);
                this.error = reloadedInfo.Err;
            }
            else
            {
                if (isNetDirRoot)
                    this.Icon = Common.IconHelper.Shell32Icons.Instance.GetIcon(275, false);
                else
                    this.Icon = Common.IconHelper.FileSystem.Instance.GetDirIcon(true);
            }
            if (reloadedInfo.hasSubDir)
            {
                this.AddLoadingLabelNode();
            }
        }
        public TreeViewModelDir(object parent, IOInfoShadow ioInfo) : base(parent)
        {
            this.dirInfo = ioInfo;
            this.Text = ioInfo.name;

            bool haveSubs = false;
            Exception haveSubsErr = null;
            try
            {
                haveSubs = new DirectoryInfo(ioInfo.fullName).GetDirectories().Length > 0;
            }
            catch (Exception err)
            {
                haveSubsErr = err;
            }
            if (haveSubsErr != null)
            {
                this.Icon = Common.IconHelper.Shell32Icons.Instance.GetIcon(234, false);
                this.error = haveSubsErr;
            }
            else
            {
                this.Icon = Common.IconHelper.FileSystem.Instance.GetDirIcon(true);
            }
            if (haveSubs)
            {
                this.AddLoadingLabelNode();
            }
        }

        public IOInfoShadow dirInfo;
        public override string GetFullPath()
        {
            return dirInfo.fullName;
        }

        internal void SetLockDown()
        {
            Children = null;
            Icon = Core.GetInstance().iconS32.GetIcon(234, false);
            //Text += " (" + err.Message + ")";
        }

        internal void UpdateFrom(TreeViewModelDir newDir)
        {
            this.dirInfo = newDir.dirInfo;
            this.Text = newDir.Text;
            this.Icon = newDir.Icon;
            this.error = newDir.error;
        }
        internal void UpdateFrom(IOInfoShadow dirIO)
        {
            this.dirInfo = dirIO;
            this.Text = dirIO.name;
        }
    }


    public sealed class TreeViewModelHost : TreeViewModelPhysical
    {
        public TreeViewModelHost(object parent)
            : base(parent)
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

        internal void UpdateFrom(TreeViewModelHost newHost)
        {
            this.Text = newHost.Text;
            this.Icon = newHost.Icon;
            this.error = newHost.error;
        }
    }
}
