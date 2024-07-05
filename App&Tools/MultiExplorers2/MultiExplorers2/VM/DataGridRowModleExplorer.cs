using MadTomDev.Common;
using MadTomDev.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MadTomDev.App.VM
{
    internal class DataGridRowModle_link : DataGridRowModleBase
    {
        public DataGridRowModle_link(TreeViewModelLink rawItem)
        {
            icon = rawItem.Icon;
            name = rawItem.Text;
            link = rawItem.link.fullName;
        }

        public string link { set; get; }
    }
    internal class DataGridRowModle_disk : DataGridRowModleBase, INotifyPropertyChanged
    {
        private TreeViewModelDisk treeNodeItem;
        public DataGridRowModle_disk(TreeViewModelDisk rawItem)
        {
            UpdateFrom(rawItem);
        }
        internal void UpdateFrom(TreeViewModelDisk treeNodeDisk)
        {
            treeNodeItem = treeNodeDisk;
            //icon = (BitmapSource)rawItem.Icon.Clone();
            diskVolume = treeNodeDisk.diskInfo.name[0];
            diskInfo = treeNodeDisk.diskInfo;
            icon = treeNodeDisk.Icon;
            name = treeNodeDisk.TextWithLabel;
            //_EditingTx = treeNodeDisk.diskInfo.volumeLabel;
            diskType = treeNodeDisk.diskInfo.driveType.ToString();
            diskFormat = treeNodeDisk.diskInfo.driveFormat;
            totalSize = treeNodeDisk.diskInfo.totalSize;
            string tmp = SimpleStringHelper.UnitsOfMeasure.GetShortString(totalSize, "B", 1024);
            int tmpI = tmp.IndexOf(' ');
            totalSizeTxN = tmp.Substring(0, tmpI);
            totalSizeTxU = tmp.Substring(tmpI + 1);
            freeSpace = treeNodeDisk.diskInfo.availableFreeSpace;
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
            if (diskInfo.volumeLabel.Length > 0)
            {
                name = $"{diskInfo.volumeLabel} ({diskInfo.name})";
            }
            else
            {
                name = $"({diskInfo.name})";
            }
            RaisePropertyChanged("name");
        }
        public DriveInfoShadow diskInfo;
        public char diskVolume { set; get; }
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
    internal class DataGridRowModle_host : DataGridRowModleBase
    {
        public DataGridRowModle_host(TreeViewModelHost rawItem)
        {
            UpdateFrom(rawItem);
        }
        internal bool UpdateFrom(TreeViewModelHost newHost)
        {
            bool needUpdate = false;
            needUpdate = newHost.Text != name;

            //icon = (BitmapSource)rawItem.Icon.Clone();
            icon = newHost.Icon;
            name = newHost.Text;

            try
            {
                IPAddress[] ips = Dns.GetHostAddresses(name);
                bool gotIPv4 = false, gotIPv6 = false;
                string tmp;
                foreach (IPAddress ip in ips)
                {
                    if (!gotIPv4 && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        tmp = ip.ToString();
                        needUpdate = needUpdate || tmp != hostIPv4;
                        hostIPv4 = tmp;
                        gotIPv4 = true;
                    }
                    if (!gotIPv6 && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        tmp = ip.ToString();
                        needUpdate = needUpdate || tmp != hostIPv6;
                        hostIPv6 = tmp;
                        gotIPv6 = true;
                    }
                }
            }
            catch (Exception err)
            {
            }
            return needUpdate;
        }

        public string hostIPv4 { set; get; }
        public string hostIPv6 { set; get; }

    }
    internal class DataGridRowModle_dirNFile : DataGridRowModleBase, INotifyPropertyChanged
    {
        public IOInfoShadow iois;
        public TaskScheduler uiContext = TaskScheduler.FromCurrentSynchronizationContext();
        public DataGridRowModle_dirNFile(object treeViewModleDir_fileIoInfoShadow)
        {
            if (treeViewModleDir_fileIoInfoShadow is TreeViewModelDir)
            {
                TreeViewModelDir item = (TreeViewModelDir)treeViewModleDir_fileIoInfoShadow;
                iois = item.dirInfo;
                icon = item.Icon;
                isIconReady = true;
                name = item.Text;
            }
            else
            {
                iois = (IOInfoShadow)treeViewModleDir_fileIoInfoShadow;
                SetData1(iois);
            }

            SetData2(iois);

        }
        private void SetData1(IOInfoShadow iois)
        {
            Task.Factory.StartNew(() =>
            {
                if (iois.attributes.hidden)
                {
                    if (!iois.fullName.StartsWith("\\\\")
                        || !Utilities.Checker.CheckIsHostRootPath(iois.fullName))
                    {
                        iconOpacity = 0.5d;
                    }
                }

                IconHelper.FileSystem fsicon = IconHelper.FileSystem.Instance;
                BitmapSource bitmap;
                if (fsicon.HaveIcon(iois.fullName, true, iois.attributes.directory))
                {
                    if (iois.wasFile)
                    {
                        icon = fsicon.GetIcon(iois.fullName, true, iois.attributes.directory);
                    }
                    else
                    {
                        if (iois.dirError != null)
                            icon = IconHelper.Shell32Icons.Instance.GetIcon(234, false);
                        else if (Utilities.FilePath.CheckIsUngRoot(iois.fullName))
                            icon = IconHelper.Shell32Icons.Instance.GetIcon(275, false);
                        else
                            icon = fsicon.GetIcon(iois.fullName, true, iois.attributes.directory);
                    }

                    isIconReady = true;
                }
                else
                {
                    if (iois.extension.ToLower() == ".exe")
                        icon = IconHelper.Shell32Icons.Instance.GetIcon(2, false);
                    else
                        icon = IconHelper.Shell32Icons.Instance.GetIcon(20, false);
                    fsicon.GetIconAsync(iois.fullName, true, iois.attributes.directory);
                }
            });//, CancellationToken.None, TaskCreationOptions.None, uiContext);
            name = iois.name;
            string testName = iois.name;
            // 如果是数字开头，则进一步增加前缀，目前给定为 0 000，000，000，000，000， 15位
            StringBuilder strBdr = new StringBuilder();
            if (Utilities.Checker.CheckIsNumberStart(ref testName, out string numStr, out int intLength))
            {
                strBdr.Append("0");
                for (int i = intLength; i < 15; ++i)
                {
                    strBdr.Append("0");
                }
                strBdr.Append(testName);
                testName = strBdr.ToString();
            }
            // 如果是数字开头
            nameForSorting = (iois.wasFile ? "" : "..") + testName;
        }
        private void SetData2(IOInfoShadow iois)
        {
            //_EditingTx = iois.name;
            modifyTime = iois.lastWriteTime;
            modifyTimeTx = iois.lastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
            if (iois.attributes.directory)
                fileType = "";
            else
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
        internal bool UpdateFrom(IOInfoShadow newIO)
        {
            bool needUpdate
                = iois.length != newIO.length
                || iois.lastAccessTime != newIO.lastAccessTime
                || iois.lastWriteTime != newIO.lastWriteTime
                || iois.attributes != newIO.attributes
                || iois.wasExists != newIO.wasExists
                || iois.wasFile != newIO.wasFile
                || iois.creationTime != newIO.creationTime
                || iois.fullName != newIO.fullName;


            this.iois = newIO;
            if (needUpdate)
            {
                SetData1(iois);
                SetData2(iois);
                return true;
            }
            return false;
        }
        public bool isIconReady = false;
        private BitmapSource _icon;

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //private string _EditingTx;
        //public string EditingTx
        //{
        //    set
        //    {
        //        if (value != name)
        //        {
        //            try
        //            {
        //                string targetFullName
        //                    = Path.Combine(
        //                        Path.GetDirectoryName(iois.fullName),
        //                        value);
        //                if (iois.attributes.directory)
        //                {
        //                    Directory.Move(iois.fullName, targetFullName);
        //                }
        //                else
        //                {
        //                    File.Move(iois.fullName, targetFullName);
        //                }
        //                iois.fullName = targetFullName;
        //                iois.name = value;
        //                name = value;
        //                _EditingTx = value;
        //                RaisePropertyChanged("name");
        //            }
        //            catch (Exception err)
        //            {
        //                _EditingTx = iois.name;
        //                MessageBox.Show(err.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }

        //    }
        //    get => _EditingTx;
        //}
        //public string modifyTimeTx { set; get; }

        public string nameForSorting { set; get; }

        public DateTime modifyTime { set; get; }
        public string modifyTimeTx { set; get; }
        public string fileType { set; get; }
        public long fileSize { set; get; }
        public string fileSizeTxN { set; get; }
        public string fileSizeTxU { set; get; }
        public string attributes { set; get; }

        private double _iconOpacity = 1d;
        public double iconOpacity
        {
            set
            {
                _iconOpacity = value;
                RaisePropertyChanged("iconOpacity");
            }
            get => _iconOpacity;
        }

    }
}
