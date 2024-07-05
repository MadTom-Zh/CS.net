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
using System.Windows.Shapes;

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for WindowSetting.xaml
    /// </summary>
    public partial class WindowSetting : Window
    {
        public WindowSetting()
        {
            InitializeComponent();
        }

        private Setting setting;
        private void Window_Initialized(object sender, EventArgs e)
        {
            Grid container = (Grid)pGB_isLogEnabled.Content;
            cb_log_general = (CheckBox)container.Children[0];
            cb_log_error = (CheckBox)container.Children[1];
            cb_log_storageAccess = (CheckBox)container.Children[2];
            cb_log_storagePlug = (CheckBox)container.Children[3];
            cb_log_trans = (CheckBox)container.Children[4];
            cb_log_transDetails = (CheckBox)container.Children[5];
            cb_log_transError = (CheckBox)container.Children[6];
            cb_log_FSWatcher = (CheckBox)container.Children[7];

            setting = Setting.Instance;

            pGB_isEnableFileSystemWatcher.IsChecked = setting.isFileSystemWatcherEnabled;

            switch (setting.sameNameDirHandleType)
            {
                case Data.TransferManager.SameNameDirHandleTypes.Rename:
                    rb_sd_new.IsChecked = true;
                    break;
                case Data.TransferManager.SameNameDirHandleTypes.Combine:
                    rb_sd_combin.IsChecked = true;
                    break;
                case Data.TransferManager.SameNameDirHandleTypes.Ask:
                    rb_sd_ask.IsChecked = true;
                    break;
                case Data.TransferManager.SameNameDirHandleTypes.Skip:
                    rb_sd_skip.IsChecked = true;
                    break;
            }
            switch (setting.sameNameFileHandleType)
            {
                case Data.TransferManager.SameNameFileHandleTypes.Rename:
                    rb_sf_new.IsChecked = true;
                    break;
                case Data.TransferManager.SameNameFileHandleTypes.Overwrite:
                    rb_sf_overwrite.IsChecked = true;
                    break;
                case Data.TransferManager.SameNameFileHandleTypes.Ask:
                    rb_sf_ask.IsChecked = true;
                    break;
                case Data.TransferManager.SameNameFileHandleTypes.Skip:
                    rb_sf_skip.IsChecked = true;
                    break;
            }

            pGB_isLogEnabled.IsChecked = setting.isLogEnabled;

            cb_log_general.IsChecked = setting.logFlagGeneral;
            cb_log_error.IsChecked = setting.logFlagError;
            cb_log_storageAccess.IsChecked = setting.logFlagStorageAccess;
            cb_log_storagePlug.IsChecked = setting.logFlagStoragePlugInOut;
            cb_log_trans.IsChecked = setting.logFlagTransTask;
            cb_log_transDetails.IsChecked = setting.logFlagTransDetails;
            cb_log_transError.IsChecked = setting.logFlagTransError;
            cb_log_FSWatcher.IsChecked = setting.logFlagFSWatcher;

            cb_languages_DropDownOpened(null, null);
        }

        private CheckBox cb_log_general;
        private CheckBox cb_log_error;
        private CheckBox cb_log_storageAccess;
        private CheckBox cb_log_storagePlug;
        private CheckBox cb_log_trans;
        private CheckBox cb_log_transDetails;
        private CheckBox cb_log_transError;
        private CheckBox cb_log_FSWatcher;

        private bool isApply = false;
        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            isApply = true;
            this.DialogResult = true;
            Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Core core = Core.GetInstance();
            if (isApply)
                core.LogGeneral(core.GetLangTx("txLog_settingApply"));
            else
                core.LogGeneral(core.GetLangTx("txLog_settingCancel"));
            if (isApply)
            {
                setting.isFileSystemWatcherEnabled = pGB_isEnableFileSystemWatcher.IsChecked;

                if (rb_sd_new.IsChecked == true)
                    setting.sameNameDirHandleType = Data.TransferManager.SameNameDirHandleTypes.Rename;
                else if (rb_sd_combin.IsChecked == true)
                    setting.sameNameDirHandleType = Data.TransferManager.SameNameDirHandleTypes.Combine;
                else if (rb_sd_ask.IsChecked == true)
                    setting.sameNameDirHandleType = Data.TransferManager.SameNameDirHandleTypes.Ask;
                else
                    setting.sameNameDirHandleType = Data.TransferManager.SameNameDirHandleTypes.Skip;

                if (rb_sf_new.IsChecked == true)
                    setting.sameNameFileHandleType = Data.TransferManager.SameNameFileHandleTypes.Rename;
                else if (rb_sf_overwrite.IsChecked == true)
                    setting.sameNameFileHandleType = Data.TransferManager.SameNameFileHandleTypes.Overwrite;
                else if (rb_sf_ask.IsChecked == true)
                    setting.sameNameFileHandleType = Data.TransferManager.SameNameFileHandleTypes.Ask;
                else
                    setting.sameNameFileHandleType = Data.TransferManager.SameNameFileHandleTypes.Skip;

                setting.isLogEnabled = pGB_isLogEnabled.IsChecked;
                setting.logFlagGeneral = cb_log_general.IsChecked == true;
                setting.logFlagError = cb_log_error.IsChecked == true;
                setting.logFlagStorageAccess = cb_log_storageAccess.IsChecked == true;
                setting.logFlagStoragePlugInOut = cb_log_storagePlug.IsChecked == true;
                setting.logFlagTransTask = cb_log_trans.IsChecked == true;
                setting.logFlagTransDetails = cb_log_transDetails.IsChecked == true;
                setting.logFlagTransError = cb_log_transError.IsChecked == true;
                setting.logFlagFSWatcher = cb_log_FSWatcher.IsChecked == true;
            }
        }



        private void cb_languages_DropDownOpened(object sender, EventArgs e)
        {
            cb_languages_initing = true;
            cb_languages.Items.Clear();
            string langName;
            System.Globalization.CultureInfo ci;
            cb_languages.Items.Add($"Default");
            foreach (FileInfo lanFi in new DirectoryInfo("Languages").GetFiles("*.txt"))
            {
                langName = lanFi.Name.Substring(0, lanFi.Name.Length - 4);
                try
                {
                    ci = new System.Globalization.CultureInfo(langName);
                    cb_languages.Items.Add($"{langName} ({ci.DisplayName})");
                    if (setting.language == langName)
                    {
                        cb_languages.SelectedItem = cb_languages.Items[cb_languages.Items.Count - 1];
                    }
                }
                catch (Exception)
                {
                    cb_languages.Items.Add($"{langName} (unknow)");
                }
            }
            
            cb_languages_initing = false;
        }
        private bool cb_languages_initing = false;
        private void cb_languages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_languages_initing)
                return;

            if (cb_languages.SelectedItem == null)
            {
                setting.language = null;
                return;
            }
            string langName = cb_languages.SelectedItem.ToString();
            int testIdx = langName.IndexOf(" (");
            if (testIdx < 0)
            {
                setting.language = null;
                return;
            }
            langName = langName.Substring(0, testIdx);
            Core core = Core.GetInstance();
            core.TrySetLang(langName);
            setting.language = langName;
        }

    }
}
