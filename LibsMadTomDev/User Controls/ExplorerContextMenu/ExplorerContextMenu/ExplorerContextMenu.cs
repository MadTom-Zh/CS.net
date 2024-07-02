using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using System.IO;
using System.Threading;

namespace MadTomDev.UI
{
    public class ExplorerContextMenu : ContextMenu
    {
        public ExplorerContextMenu()
        {
            ReInitItems();
        }

        public void ReInitItems(Setting setting = null)
        {
            itemList.Clear();


            // open +
            // -----------
            // manage open with...
            // -----------
            // cut
            // copy
            // past
            // ------------
            // create shotcut
            // delete
            // rename
            // ------------
            // properties...

            #region init default items

            Common.IconHelper.Shell32Icons s32Icons = Common.IconHelper.Shell32Icons.Instance;

            itemOpen = new EMItemModel(false)
            {
                name = "open",
                Text = "Open",
                CommandText = "[open]",
                actionClick = ActionOpen,
                SelectionCountType = SelectionCountTypes.Files,
                SelectionFileType = SelectionFileTypes.SameExt,
            };
            itemList.Add(itemOpen);
            itemList.Add(EMItemModelExtensions.NewSeparator(1));
            itemManageOpenWith = new EMItemModel(false)
            {
                name = "manageOpenWith",
                Text = "Manage Open With...",
                Icon = s32Icons.GetIcon(316, false),
                CommandText = "[manageOpenWith]",
                actionClick = ActionManageOpenWith,
                SelectionCountType = SelectionCountTypes.Any0,
            };
            itemList.Add(itemManageOpenWith);
            itemList.Add(EMItemModelExtensions.NewSeparator(3));
            itemCut = new EMItemModel(false)
            {
                name = "cut",
                Text = "Cut",
                Icon = s32Icons.GetIcon(259, false),
                CommandText = "[cut]",
                actionClick = ActionCut,
                SelectionCountType = SelectionCountTypes.Any1,
            };
            itemList.Add(itemCut);
            itemCopy = new EMItemModel(false)
            {
                name = "copy",
                Text = "Copy",
                Icon = s32Icons.GetIcon(134, false),
                CommandText = "[copy]",
                actionClick = ActionCopy,
                SelectionCountType = SelectionCountTypes.Any1,
            };
            itemList.Add(itemCopy);
            itemPaste = new EMItemModel(false)
            {
                name = "paste",
                Text = "Paste",
                Icon = s32Icons.GetIcon(260, false),
                CommandText = "[paste]",
                actionClick = ActionPaste,
                SelectionCountType = SelectionCountTypes.None,
            };
            itemList.Add(itemPaste);
            itemList.Add(EMItemModelExtensions.NewSeparator(7));
            itemCreateShotcut = new EMItemModel(false)
            {
                name = "createShotcut",
                Text = "Create shotcut",
                Icon = s32Icons.GetIcon(29, false),
                CommandText = "[createShotcut]",
                actionClick = ActionCreateShotcut,
                SelectionCountType = SelectionCountTypes.Any1,
            };
            itemList.Add(itemCreateShotcut);
            itemDelete = new EMItemModel(false)
            {
                name = "delete",
                Text = "Delete",
                Icon = s32Icons.GetIcon(131, false),
                CommandText = "[delete]",
                actionClick = ActionDelete,
                SelectionCountType = SelectionCountTypes.Any1,
            };
            itemList.Add(itemDelete);
            itemRename = new EMItemModel(false)
            {
                name = "rename",
                Text = "Rename",
                Icon = s32Icons.GetIcon(133, false),
                CommandText = "[rename]",
                actionClick = ActionNotAssignedAlert,
                SelectionCountType = SelectionCountTypes.FilesOrDirs,
            };
            itemList.Add(itemRename);
            itemList.Add(EMItemModelExtensions.NewSeparator(11));
            itemProperties = new EMItemModel(false)
            {
                name = "properties",
                Text = "Properties...",
                Icon = s32Icons.GetIcon(221, false),
                CommandText = "[properties]",
                actionClick = ActionProperties,
                SelectionCountType = SelectionCountTypes.Any0,
            };
            itemList.Add(itemProperties);

            #endregion


            // load custom items
            if (setting == null)
                setting = new Setting();
            disabled_disabledOrHidden = setting.data.disableItemDisableOrHidden;

            System.Drawing.Point vect;
            object tmp;
            for (int i = 0, iv = setting.data.defaultItemOrderList.Count; i < iv; ++i)
            {
                vect = setting.data.defaultItemOrderList[i];
                tmp = itemList[vect.X];
                itemList.Remove(tmp);
                itemList.Insert(vect.Y, tmp);
            }

            Setting.DataSet.CustomItemData cust;
            for (int i = 0, iv = setting.data.customItemOrderList.Count; i < iv; ++i)
            {
                cust = setting.data.customItemOrderList[i];
                if (cust.vm.Text == EMItemModelExtensions.flag_separator)
                {
                    tmp = EMItemModelExtensions.NewSeparator();
                }
                else
                {
                    tmp = new EMItemModel()
                    {
                        IsEnabled = cust.vm.IsEnabled,
                        name = cust.vm.Text.ToLower(),
                        Text = cust.vm.Text,
                        CommandText = cust.vm.CommandText,
                        actionClick = ActionCustomItem,
                        SelectionCountType = cust.vm.SelectionCountType,
                        SelectionFileType = cust.vm.SelectionFileType,
                        Icon = cust.vm.Icon,
                    };
                }
                itemList.Insert(cust.InsertIndex, tmp);
            }

            // from itemSource to menu items
            foreach (object o in base.Items)
            {
                if (o is MenuItem)
                    ((MenuItem)o).Click -= MenuItem_Click;
            }
            base.Items.Clear();

            object rawMi;
            EMItemModel vmMi;
            MenuItem mi;
            for (int i = 0, iv = itemList.Count; i < iv; i++)
            {
                rawMi = itemList[i];
                if (rawMi is Separator)
                {
                    base.Items.Add(rawMi);
                }
                else if (rawMi is EMItemModel)
                {
                    vmMi = (EMItemModel)rawMi;

                    if (vmMi.IsEnabled)
                    {
                        mi = new MenuItem()
                        {
                            Tag = rawMi,
                            Icon = new Image() { Source = vmMi.Icon },
                            Header = vmMi.Text,
                            IsEnabled = true,
                        };
                        mi.Click += MenuItem_Click;
                        base.Items.Add(mi);
                    }
                    else if (disabled_disabledOrHidden)
                    {
                        // and   vmMi.IsEnabled == false
                        mi = new MenuItem()
                        {
                            Tag = rawMi,
                            Icon = new Image() { Source = vmMi.Icon },
                            Header = vmMi.Text,
                            IsEnabled = false,
                        };
                        //mi.Click += MenuItem_Click;
                        base.Items.Add(mi);
                    }
                }
            }
        }

        public bool disabled_disabledOrHidden = true;
        public List<object> itemList = new List<object>();

        public string dirFullName;
        public List<string> subDirsNFiles = new List<string>();

        public void ShowContextMenu(Control parent, string dirFullName, params string[] subDirsNFiles)
        {
            ShowContextMenu(parent, 0d, 0d, dirFullName, subDirsNFiles);
        }
        public void ShowContextMenu(Control parent, double posiOffsetX, double posiOffsetY, string dirFullName, params string[] subDirsNFiles)
        {
            this.dirFullName = dirFullName;
            this.subDirsNFiles.Clear();
            if (subDirsNFiles != null)
                this.subDirsNFiles.AddRange(subDirsNFiles);
            SetItemsStates();

            parent.ContextMenu = this;
            this.HorizontalOffset = posiOffsetX;
            this.VerticalOffset = posiOffsetY;
            parent.ContextMenu.IsOpen = true;
        }
        private void SetItemsStates()
        {
            int countFile = 0, countDir = 0;
            HashSet<string> fileExtList = new HashSet<string>();
            foreach (string f in subDirsNFiles)
            {
                if (File.Exists(f))
                {
                    ++countFile;
                    fileExtList.Add(Path.GetExtension(f));
                }
                else if (Directory.Exists(f))
                {
                    ++countDir;
                }
            }

            MenuItem mi;
            EMItemModel miVM;
            for (int i = 0, iv = Items.Count; i < iv; ++i)
            {
                if (Items[i] is MenuItem)
                {
                    mi = (MenuItem)Items[i];
                    if (mi.Tag is EMItemModel)
                    {
                        miVM = (EMItemModel)mi.Tag;
                        if (countDir == 1 && countFile <= 0
                            && miVM == itemOpen)
                        {
                            // open, for one dir
                            if (miVM.IsEnabled)
                            {
                                mi.IsEnabled = true;
                                mi.Visibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            if (miVM.IsEnabled)
                            {
                                if (CheckSelectionFileTypeMatch()
                                    && CheckSelectionCountTypeMatch())
                                {
                                    mi.IsEnabled = true;
                                    mi.Icon = new Image() { Source = miVM.Icon };
                                    mi.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    if (disabled_disabledOrHidden)
                                    {
                                        mi.IsEnabled = false;
                                        mi.Icon = null;
                                    }
                                    else
                                    {
                                        mi.Visibility = Visibility.Collapsed;
                                    }
                                }
                                if (miVM == itemPaste)
                                {
                                    if (!Clipboard.ContainsFileDropList())
                                    {
                                        if (disabled_disabledOrHidden)
                                        {
                                            mi.IsEnabled = false;
                                        }
                                        else
                                        {
                                            mi.Visibility = Visibility.Collapsed;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            bool CheckSelectionCountTypeMatch()
            {
                switch (miVM.SelectionCountType)
                {
                    default:
                    case SelectionCountTypes.None:
                        if (countFile == 0 && countDir == 0)
                            return true;
                        break;
                    case SelectionCountTypes.Any0:
                        return true;
                    case SelectionCountTypes.Any1:
                        if (countFile + countDir >= 1)
                            return true;
                        break;
                    case SelectionCountTypes.More1:
                        if (countFile + countDir > 1)
                            return true;
                        break;
                    case SelectionCountTypes.Dir:
                        if (countFile == 0 && countDir == 1)
                            return true;
                        break;
                    case SelectionCountTypes.Dirs:
                        if (countFile == 0 && countDir >= 1)
                            return true;
                        break;
                    case SelectionCountTypes.File:
                        if (countFile == 1 && countDir == 0)
                            return true;
                        break;
                    case SelectionCountTypes.Files:
                        if (countFile >= 1 && countDir == 0)
                            return true;
                        break;
                    case SelectionCountTypes.FileNDir:
                        if (countFile == 1 && countDir == 1)
                            return true;
                        break;
                    case SelectionCountTypes.FileNDirs:
                        if (countFile == 1 && countDir >= 1)
                            return true;
                        break;
                    case SelectionCountTypes.FileOrDir:
                        if ((countFile == 1 && countDir == 0)
                            || (countFile == 0 && countDir == 1))
                            return true;
                        break;
                    case SelectionCountTypes.FileOrDirs:
                        if ((countFile == 1 && countDir == 0)
                            || (countFile == 0 && countDir >= 1))
                            return true;
                        break;
                    case SelectionCountTypes.FilesNDir:
                        if (countFile >= 1 && countDir == 1)
                            return true;
                        break;
                    case SelectionCountTypes.FilesNDirs:
                        if (countFile >= 1 && countDir >= 1)
                            return true;
                        break;
                    case SelectionCountTypes.FilesOrDir:
                        if ((countFile >= 1 && countDir == 0)
                            || (countFile == 0 && countDir == 1))
                            return true;
                        break;
                    case SelectionCountTypes.FilesOrDirs:
                        if ((countFile >= 1 && countDir == 0)
                            || (countFile == 0 && countDir >= 1))
                            return true;
                        break;
                }
                return false;
            }
            bool CheckSelectionFileTypeMatch()
            {
                switch (miVM.SelectionFileType)
                {
                    case SelectionFileTypes.Any:
                        return true;
                    case SelectionFileTypes.SameExt:
                        if (countFile >= 1 && fileExtList.Count <= 1)
                            return true;
                        break;
                }
                return false;
            }
        }

        #region default items
        public EMItemModel itemOpen;
        public EMItemModel itemManageOpenWith;
        public EMItemModel itemCut;
        public EMItemModel itemCopy;
        public EMItemModel itemPaste;
        public EMItemModel itemCreateShotcut;
        public EMItemModel itemDelete;
        public EMItemModel itemRename;
        public EMItemModel itemProperties;

        #endregion


        #region default actions


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem)
            {
                MenuItem mi = (MenuItem)sender;
                if (mi.Tag is EMItemModel)
                {
                    EMItemModel vm = (EMItemModel)mi.Tag;
                    ActionBeforeItemClick?.Invoke(vm);
                    vm.actionClick?.Invoke(vm);
                }
            }
        }

        public Action<EMItemModel> ActionBeforeItemClick;
        /// <summary>
        /// 在创新文件、文件夹前，返回要创建文件、文件夹的全名；
        /// bool， true-文件；false-文件夹；
        /// string， 要创建的全面；
        /// </summary>
        public Action<bool, string> ActionNewFileDirToCreate;
        public void ActionCustomItem(EMItemModel carrier)
        {
            EMItemModelExtensions.CommandAnalysisResult cmdAnal
                = EMItemModelExtensions.AnalysisCommand(carrier.CommandText);

            switch (cmdAnal.cmdType)
            {
                case EMItemModelExtensions.CommandAnalysisResult.CmdTypes.Separator:
                    return;
                case EMItemModelExtensions.CommandAnalysisResult.CmdTypes.New:
                    string targetFullName;
                    if (File.Exists(cmdAnal.newTempletFile)
                        || Directory.Exists(cmdAnal.newTempletFile))
                    {
                        // copy to dir
                        targetFullName = Path.Combine(dirFullName, Path.GetFileName(cmdAnal.newTempletFile));
                        ActionNewFileDirToCreate?.Invoke(true, targetFullName);
                        Data.Utilities.CSharpWapper.Copy(
                            cmdAnal.newTempletFile,
                            dirFullName,
                            Data.Utilities.CSharpWapper.ExistHandleMethods.Rename,
                            Data.Utilities.CSharpWapper.ExistHandleMethods.Rename);

                    }
                    else
                    {
                        // create empty file
                        targetFullName = Data.Utilities.CSharpWapper.AutoNewFullName(
                            Path.Combine(dirFullName, cmdAnal.newFileName));
                        ActionNewFileDirToCreate?.Invoke(true, targetFullName);
                        File.WriteAllText(targetFullName, null);
                    }
                    break;
                case EMItemModelExtensions.CommandAnalysisResult.CmdTypes.NewDir:
                    string newDirFullname = Path.Combine(dirFullName, cmdAnal.newDirName);
                    ActionNewFileDirToCreate?.Invoke(false, newDirFullname);
                    newDirFullname = Data.Utilities.CSharpWapper.AutoNewFullName(newDirFullname);
                    Directory.CreateDirectory(newDirFullname);
                    break;

                default:
                case EMItemModelExtensions.CommandAnalysisResult.CmdTypes.Unknow:
                case EMItemModelExtensions.CommandAnalysisResult.CmdTypes.Exec:
                    // exec command
                    try
                    {
                        string cmd = EMItemModelExtensions.GetRealCommand(
                                cmdAnal.cmd,
                                dirFullName,
                                subDirsNFiles);
                        Process.Start(new ProcessStartInfo()
                        {
                            WorkingDirectory = dirFullName,
                            FileName = cmd,
                        });
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show(err.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
            }
        }
        private void ActionOpen(EMItemModel carrier)
        {
            if (subDirsNFiles != null && subDirsNFiles.Count == 1
                && Directory.Exists(subDirsNFiles[0]))
            {
                // call enter dir
                ActionOpenDir?.Invoke(subDirsNFiles[0]);
            }
            else
            {
                // exec files
                foreach (string f in subDirsNFiles)
                {
                    Process p = new Process();
                    p.StartInfo = new ProcessStartInfo(f)
                    { UseShellExecute = true, };
                    p.Start();
                }
            }
        }
        public Action<string> ActionOpenDir;
        public Action<WindowMenuSettings> ActionManageOpenWith_setLanguage;
        private void ActionManageOpenWith(EMItemModel carrier)
        {
            WindowMenuSettings settingWnd = new WindowMenuSettings();
            ReInitItems();
            settingWnd.Init(this);
            ActionManageOpenWith_setLanguage?.Invoke(settingWnd);
            if (settingWnd.ShowDialog() == true)
            {
                // load from this setting
                //settingWnd.setting.Save();
                ReInitItems(settingWnd.setting);
            }
        }
        public event Action<ExplorerContextMenu, string[]> ActionCutRaised;
        private void ActionCut(EMItemModel carrier)
        {
            if (subDirsNFiles.Count > 0)
            {
                string[] fileArray = subDirsNFiles.ToArray();
                Data.Utilities.ClipBoard.SetFileDrags(false, true, fileArray);
                ActionCutRaised?.Invoke(this, fileArray);
            }
        }

        public event Action<ExplorerContextMenu, string[]> ActionCopyRaised;
        private void ActionCopy(EMItemModel carrier)
        {
            if (subDirsNFiles.Count > 0)
            {
                string[] fileArray = subDirsNFiles.ToArray();
                Data.Utilities.ClipBoard.SetFileDrags(true, true, fileArray);
                ActionCopyRaised?.Invoke(this, fileArray);
            }
        }
        private void ActionPaste(EMItemModel carrier)
        {
            string[] sourceFiles = Data.Utilities.ClipBoard.GetFileDrops(out DragDropEffects copyOrMove);
            if (sourceFiles != null && sourceFiles.Length > 0)
            {
                Data.Utilities.ClipBoard.Clear();
                if (copyOrMove.HasFlag(DragDropEffects.Move))
                {
                    Data.Utilities.MSVBFileOperation.Move(sourceFiles, dirFullName, out Exception err);
                    if (err != null)
                        MessageBox.Show(err.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (copyOrMove.HasFlag(DragDropEffects.Copy))
                {
                    Data.Utilities.Shell32SHFileOperation.CopyFiles(sourceFiles, dirFullName, false, out Exception err);
                    if (err != null)
                        MessageBox.Show(err.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            //Clipboard.Clear();
        }
        private void ActionCreateShotcut(EMItemModel carrier)
        {
            if (Directory.Exists(dirFullName))
            {
                string fExt, newName;
                foreach (string f in subDirsNFiles)
                {
                    newName = Path.Combine(dirFullName, Path.GetFileName(f));
                    fExt = Path.GetExtension(Path.GetFileName(f));
                    if (fExt.Length > 0)
                        newName = Data.Utilities.CSharpWapper.AutoNewName(
                            newName.Substring(0, newName.LastIndexOf(fExt)) + " -shortcut" + fExt);
                    else
                        newName = Data.Utilities.CSharpWapper.AutoNewName(
                            newName + " -shortcut");
                    Data.Utilities.Other.GenerateShortcut(f, newName + ".lnk", dirFullName);
                }
            }
        }
        private void ActionDelete(EMItemModel carrier)
        {
            if (subDirsNFiles.Count > 0)
            {
                if (subDirsNFiles[0].StartsWith("\\\\"))
                {
                    if (MessageBox.Show("Files not on local will be delete permanently!"
                            + Environment.NewLine + Environment.NewLine + "Are you sure to continue?",
                            "Warning",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning)
                        == MessageBoxResult.No)
                    {
                        return;
                    }
                }
                Data.Utilities.Shell32SHFileOperation.DeleteFiles(subDirsNFiles.ToArray(), out Exception err);
                if (err != null)
                    MessageBox.Show(err.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ActionNotAssignedAlert(EMItemModel carrier)
        {
            MessageBox.Show("Function not assigned.");
        }
        private void ActionProperties(EMItemModel carrier)
        {
            if (subDirsNFiles.Count == 0)
            {
                if (File.Exists(dirFullName) || Directory.Exists(dirFullName))
                    Resource.CSharpUser32.StandardDialogs.ShowFileProperties(dirFullName);
            }
            if (subDirsNFiles.Count == 1)
            {
                string file = subDirsNFiles[0];
                if (File.Exists(file) || Directory.Exists(file))
                    Resource.CSharpUser32.StandardDialogs.ShowFileProperties(file);
            }
            else if (subDirsNFiles.Count > 1)
            {
                MultiFilesProperties pWnd = new MultiFilesProperties();
                pWnd.Init(subDirsNFiles.ToArray());
                pWnd.Show();
            }
        }


        #endregion

    }
}
