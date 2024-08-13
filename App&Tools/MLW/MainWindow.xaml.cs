using MLW_Succubus_Storys.Classes;
using System;
using System.Collections.Generic;
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

namespace MLW_Succubus_Storys
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal bool isSetting = true;

        public MainWindow()
        {
            InitializeComponent();
            if (Core.localizeDict.ContainsKey("mainTitle"))            
                this.Title = Core.localizeDict["mainTitle"] + " - by MadTom 2023 0414";            
            if (Core.localizeDict.ContainsKey("house"))
                tabItemHouse.Header = Core.localizeDict["house"];
            Core.BGMChangingStart += () => { Dispatcher.BeginInvoke(() => { spBGM.IsEnabled = false; }); };
            Core.BGMChangingEnd += () => { Dispatcher.BeginInvoke(() => { spBGM.IsEnabled = true; }); };
        }

        private bool isHouseLoaded = false;
        private void Window_Activated(object sender, EventArgs e)
        {
            if (isHouseLoaded)
                return;

            SuccuNode sNode;
            Ctrls.BtnSuccuEntry btn;
            for (int i = 0, iv = Core.allSuccubus.Count; i < iv; ++i)
            {
                sNode = Core.allSuccubus[i];
                btn = new Ctrls.BtnSuccuEntry()
                {
                    IsEnabled = sNode.storyLv1.Count > 0,
                    Tag = sNode,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    IconSuccu = sNode.succuIcon,
                    NameSuccu = sNode.succuNameLocalized,
                    IconMtl1 = sNode.mtl1Icon,
                    IconMtl2 = sNode.mtl2Icon,
                    IconMtl3 = sNode.mtl3Icon,
                    actionEnter = BtnSuccuEntryClicked,
                };
                Grid.SetColumn(btn, i / 7);
                Grid.SetRow(btn, i % 7);
                gridSuccuList.Children.Add(btn);
            }

            string exWifeImgKey = "IconExWife";
            string mtlUnknowImgKey = "MtUnknow";
            if (Core.imageDict.ContainsKey(exWifeImgKey)
                && Core.imageDict.ContainsKey(mtlUnknowImgKey))
            {
                btn = new Ctrls.BtnSuccuEntry()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    IconSuccu = Core.imageDict[exWifeImgKey],
                    NameSuccu = "[Unknow]",
                    IconMtl1 = Core.imageDict[mtlUnknowImgKey],
                    IconMtl2 = Core.imageDict[mtlUnknowImgKey],
                    IconMtl3 = Core.imageDict[mtlUnknowImgKey],
                };
                Grid.SetColumn(btn, 2);
                Grid.SetRow(btn, 6);
                gridSuccuList.Children.Add(btn);
            }


            tbtnBGM_Click(tbtnBGM1, null);
            isHouseLoaded = true;
        }
        private void BtnSuccuEntryClicked(Ctrls.BtnSuccuEntry sender)
        {
            if (sender.Tag is SuccuNode)
            {
                SuccuNode sNode = (SuccuNode)sender.Tag;
                // 如果已经打开，则跳转
                for (int i = 1, iv = tcMain.Items.Count; i < iv; ++i)
                {
                    if (((Ctrls.TabHeader)((TabItem)tcMain.Items[i]).Header).Text == sNode.succuNameLocalized)
                    {
                        tcMain.SelectedIndex = i;
                        return;
                    }
                }

                // 如果未打开，则打开
                Ctrls.SuccuStoryPanel ssPanel = new Ctrls.SuccuStoryPanel();
                TabItem newTab = new TabItem()
                {
                    Header = new Ctrls.TabHeader()
                    {
                        Icon = sNode.succuIcon,
                        Text = sNode.succuNameLocalized,
                        actionClose = BtnCloseTabClicked,
                    },
                    Content = ssPanel,
                };
                tcMain.Items.Add(newTab);
                ssPanel.Init(sNode);
                tcMain.SelectedItem = newTab;
            }
        }
        private void BtnCloseTabClicked(Ctrls.TabHeader sender)
        {
            for (int i = 1, iv = tcMain.Items.Count; i < iv; ++i)
            {
                if (((TabItem)tcMain.Items[i]).Header == sender)
                {
                    tcMain.Items.RemoveAt(i);
                    break;
                }
            }
        }

        #region BGM

        private void tbtnBGM_Click(object sender, RoutedEventArgs e)
        {
            if (sender is null)
                return;

            int bgmIdx = -1;
            if (sender == tbtnBGM1)
                bgmIdx = 0;
            else if (sender == tbtnBGM2)
                bgmIdx = 1;
            else if (sender == tbtnBGMEA)
                bgmIdx = 2;
            else if (sender == tbtnBGMEB)
                bgmIdx = 3;

            tbtnBGM1.IsChecked = bgmIdx == 0;
            tbtnBGM2.IsChecked = bgmIdx == 1;
            tbtnBGMEA.IsChecked = bgmIdx == 2;
            tbtnBGMEB.IsChecked = bgmIdx == 3;
            if (bgmIdx >= 0)
                Core.BGMPlay(bgmIdx);
            else
                Core.BGMPause();
        }

        private DateTime sldBGMVolume_ValueChangedTime = DateTime.MinValue;
        private void sldBGMVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isSetting) return;
            Core.BGMVolume = (int)sldBGMVolume.Value;
            sldBGMVolume_ValueChangedTime = DateTime.Now;
            sldBGMVolume_ValueChangedDelaySave();
        }
        private void sldBGMVolume_ValueChangedDelaySave()
        {
            Task.Run(() =>
            {
                if (sldBGMVolume_ValueChangedTime == DateTime.MinValue)
                    return;
                do
                {
                    Task.Delay(800);
                }
                while ((DateTime.Now - sldBGMVolume_ValueChangedTime).TotalMilliseconds > 800);

                sldBGMVolume_ValueChangedTime = DateTime.MinValue;
                Core.SettingSave();
            });
        }

        #endregion



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Core.Exit();
        }

    }
}
