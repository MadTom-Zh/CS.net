using MadTomDev.App.Classes;
using MadTomDev.App.Ctrls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
using Console = MadTomDev.App.Classes.Console;

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
            setting = core.setting;
            core.mainWindow = this;

            combo_stopBits.Items.Clear();
            Type t = typeof(StopBits);
            foreach (string en in t.GetEnumNames())
            {
                combo_stopBits.Items.Add(en);
            }
            combo_stopBits.Text = StopBits.One.ToString();

            combo_cfgParity.Items.Clear();
            t = typeof(Parity);
            foreach (string en in t.GetEnumNames())
            {
                combo_cfgParity.Items.Add(en);
            }
            combo_cfgParity.Text = Parity.None.ToString();

            combo_cfgFlowControl.Items.Clear();
            t = typeof(FlowControl);
            foreach (string en in t.GetEnumNames())
            {
                combo_cfgFlowControl.Items.Add(en);
            }
            combo_cfgFlowControl.Text = FlowControl.Next.ToString();

            dataGrid_cfgList.ItemsSource = datagrid_cfgList_source;

            bdr_oBG.Background = setting.clrOutputBG;
            bdr_oFG.Background = setting.clrOutputFG;
            bdr_iBG.Background = setting.clrInputBG;
            bdr_iFG.Background = setting.clrInputFG;

            cb_isOutputTimePrefix.IsChecked = setting.isOutputTime;
            cb_isLog.IsChecked = setting.isLog;
            tb_outputTimePrefixFormat.Text = setting.outputTimeFormat;

            isIniting = false;
        }
        private Core core = Core.Instance;
        private bool isIniting = true;
        private Settings setting;

        private void Window_Initialized(object sender, EventArgs e)
        {
            btn_refresh_Click(null, null);
            btn_cfgListRefresh_Click(null, null);

            core.ShowStatues("Ready", Core.StatuesLevels.InfoGreen);
        }

        #region 顶端 串口 按钮列表

        private List<ButtonCom> comBtnList = new List<ButtonCom>();
        private bool btn_refresh_working = false;
        private void btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            if (btn_refresh_working)
                return;
            btn_refresh_working = true;
            btn_refresh.IsEnabled = false;

            Dispatcher.BeginInvoke(() =>
            {
                string[] serialNames = SerialPort.GetPortNames();
                ButtonCom foundBtn, newBtn;
                int idx = 0;
                foreach (string sn in serialNames)
                {
                    foundBtn = comBtnList.Find(a => a.Text == sn);
                    if (foundBtn == null)
                    {
                        // 原表中没有，创建按钮
                        newBtn = new ButtonCom() { Text = sn, };
                        newBtn.btn.Click += ComBtn_Click;
                        comBtnList.Insert(idx, newBtn);
                        sPanel_comList.Children.Insert(idx + 1, newBtn);
                    }
                    else if (comBtnList.IndexOf(foundBtn) != idx)
                    {
                        // 原表中存在，但位置不对，重排顺序
                        comBtnList.Remove(foundBtn);
                        comBtnList.Insert(idx, foundBtn);
                        sPanel_comList.Children.Remove(foundBtn);
                        sPanel_comList.Children.Insert(idx + 1, foundBtn);
                    }
                    ++idx;
                }
                // 删除当前不存在的多余的按钮
                for (int i = idx, iv = comBtnList.Count; i < iv; ++i)
                {
                    foundBtn = comBtnList[idx];
                    foundBtn.btn.Click -= ComBtn_Click;
                    comBtnList.RemoveAt(idx);
                    sPanel_comList.Children.RemoveAt(idx + 1);
                }

                // 检查连接状态，标记按钮颜色；
                List<string> comListInUse = new List<string>();
                comListInUse.AddRange(core.ComNameListInUse);
                foreach (ButtonCom btn in comBtnList)
                {
                    if (core.CheckComInUse(btn.Text))
                        btn.IsEnabled = false;
                }
                btn_refresh_working = false;
                btn_refresh.IsEnabled = true;
            });
        }

        ContextMenu comBtnContextMenu = new ContextMenu();
        MenuItem comBtnContextMenu_disConnect = null;
        string comBtnContextMenu_curComName;
        private async void ComBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null
                || !(sender is Button))
                return;
            Button btn = (Button)sender;
            if (btn.IsEnabled != true)
                return;
            btn.Cursor = Cursors.Wait;
            btn.IsEnabled = false;
            await Task.Delay(1);
            if (btn.Parent is ButtonCom)
                comBtnContextMenu_curComName = ((ButtonCom)btn.Parent).Text;

            // 显示右键菜单
            foreach (object i in comBtnContextMenu.Items)
            {
                if (i is MenuItem)
                {
                    ((MenuItem)i).Click -= CfgItem_Click;
                }
            }
            comBtnContextMenu.Items.Clear();
            bool cmHasItems = false;
            Console c = core.GetConsole_Com(comBtnContextMenu_curComName);
            if (c == null)
            {
                // config list
                MenuItem cfgItem;
                foreach (DGI_ComCfg cfg in setting.cfgList)
                {
                    cfgItem = new MenuItem()
                    { Header = cfg.Name, Tag = cfg, };
                    cfgItem.Click += CfgItem_Click;
                    comBtnContextMenu.Items.Add(cfgItem);
                    cmHasItems = true;
                }
            }
            else
            {
                // in use
                if (comBtnContextMenu_disConnect == null)
                {
                    comBtnContextMenu_disConnect = new MenuItem()
                    { Header = "In use!", IsEnabled = false, };
                }
                comBtnContextMenu.Items.Add(comBtnContextMenu_disConnect);
            }
            comBtnContextMenu.PlacementTarget = btn;
            comBtnContextMenu.IsOpen = cmHasItems;
            btn.Cursor = Cursors.Arrow;
            btn.IsEnabled = true;
            if (!cmHasItems)
            {
                MessageBox.Show("No com config exists, create one first.");
            }
        }

        private void CfgItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null
                || !(sender is MenuItem))
                return;
            MenuItem menuItem = (MenuItem)sender;
            if (menuItem.Tag == null
                || !(menuItem.Tag is DGI_ComCfg))
                return;
            DGI_ComCfg cfg = (DGI_ComCfg)menuItem.Tag;

            Dispatcher.BeginInvoke(async () =>
            {
                // 检查是否已经打开了同名配置的页
                Console console = core.GetConsole(comBtnContextMenu_curComName, cfg.Name);
                if (console != null)
                {
                    // 已经打开了-转到
                    tc.SelectedIndex = GetConsoleTabIndex(cfg.Name);
                }
                else
                {
                    // 新建页；
                    console = core.NewConsole(comBtnContextMenu_curComName, cfg, out TabItemHeader tabHeader, out PanelConsole panelConsole);
                    TabItem newTab = new TabItem()
                    {
                        Header = tabHeader,
                        Content = panelConsole,
                    };
                    tc.Items.Add(newTab);
                    tc.SelectedItem = newTab;
                }
                // 尝试连接；
                bool result = await console.TryOpenComAsync(comBtnContextMenu_curComName);
                TabItemHeader header = GetConsoleTabHeader(tc.SelectedIndex);
                PanelConsole panel = GetConsoleTabPanel(tc.SelectedIndex);
                panel.IsEnabled = result;
                if (result)
                {
                    core.ShowStatues($"Com [{console.comName}] connected.", Core.StatuesLevels.InfoGreen);
                }
                else
                {
                    core.ShowStatues($"Failed to connect to Com [{console.comName}].", Core.StatuesLevels.ErrorRed);
                }
            });
        }

        #endregion

        #region 设置，连接配置


        private ObservableCollection<DGI_ComCfg> datagrid_cfgList_source = new ObservableCollection<DGI_ComCfg>();
        private bool btn_cfgListRefresh_working = false;
        private void btn_cfgListRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (btn_cfgListRefresh_working)
                return;
            btn_cfgListRefresh_working = true;
            Dispatcher.BeginInvoke(() =>
            {
                datagrid_cfgList_source.Clear();
                foreach (DGI_ComCfg c in setting.cfgList)
                {
                    datagrid_cfgList_source.Add(c);
                }
                btn_cfgListRefresh_working = false;
            });
        }
        private void dataGrid_cfgList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    if (dataGrid_cfgList.SelectedItem != null)
                    {
                        DGI_ComCfg i = (DGI_ComCfg)dataGrid_cfgList.SelectedItem;
                        datagrid_cfgList_source.Remove(i);
                        setting.cfgList.Remove(i);
                        setting.Save();
                    }
                    break;
                case Key.Enter:
                    dataGrid_cfgList_PreviewMouseDoubleClick(null, null);
                    break;
            }
        }

        private DGI_ComCfg? dataGrid_cfgList_SelectionPre = null;
        private void dataGrid_cfgList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid_cfgList_SelectionPre == dataGrid_cfgList.SelectedItem)
                return;
            dataGrid_cfgList_SelectionPre = (DGI_ComCfg)dataGrid_cfgList.SelectedItem;
            if (dataGrid_cfgList_SelectionPre == null)
                return;

            Cfg_toUI(dataGrid_cfgList_SelectionPre);

        }

        private void btn_cfgCreateUpdate_Click(object sender, RoutedEventArgs e)
        {
            string cfgName = tb_cfgName.Text.Trim();
            DGI_ComCfg? foundCfg = setting.cfgList.Find(a => a.Name == cfgName);

            if (foundCfg == null)
            {
                // create
                Cfg_create();
            }
            else
            {
                // update
                Cfg_fromUI(foundCfg);
                setting.Save();
            }
        }
        private void Cfg_create()
        {
            DGI_ComCfg newCfg = new DGI_ComCfg();
            Cfg_fromUI(newCfg);
            setting.cfgList.Add(newCfg);
            setting.Save();
            datagrid_cfgList_source.Add(newCfg);
        }
        private void Cfg_toUI(DGI_ComCfg cfg)
        {
            tb_cfgName.Text = cfg.Name;
            tb_cfgSpeed.Text = cfg.SpeedTx;
            tb_dataBits.Text = cfg.DataBitsTx;
            combo_stopBits.Text = cfg.StopBitsTx;
            combo_cfgParity.Text = cfg.ParityTx;
            combo_cfgFlowControl.Text = cfg.FlowControlTx;
        }
        private void Cfg_fromUI(DGI_ComCfg cfg)
        {
            cfg.Name = tb_cfgName.Text;
            if (int.TryParse(tb_cfgSpeed.Text, out int vInt))
                cfg.Speed = vInt;
            else
                MessageBox.Show(core.mainWindow, "Wrong parameter [Speed].", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            if (int.TryParse(tb_dataBits.Text, out vInt))
                cfg.DataBits = vInt;
            else
                MessageBox.Show(core.mainWindow, "Wrong parameter [DataBits].", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            if (Settings.TryParseStopBits(combo_stopBits.Text, out StopBits vS))
                cfg.StopBits = vS;
            else
                MessageBox.Show(core.mainWindow, "Wrong parameter [StopBits].", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            if (Settings.TryParseParity(combo_cfgParity.Text, out Parity vP))
                cfg.Parity = vP;
            else
                MessageBox.Show(core.mainWindow, "Wrong parameter [Parity].", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            if (Settings.TryParseFlowControl(combo_cfgFlowControl.Text, out FlowControl vFC))
                cfg.FlowControl = vFC;
            else
                MessageBox.Show(core.mainWindow, "Wrong parameter [FlowControl].", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

        }
        #endregion


        #region 设置，外观，背景和前景 颜色， 字体尺寸
        private void btn_oBG_Click(object sender, RoutedEventArgs e)
        {
            if (NewColorDialog(bdr_oBG.Background, out Color newColor))
            {
                setting.clrOutputBG = new SolidColorBrush(newColor);
                setting.Save();
                bdr_oBG.Background = setting.clrOutputBG;
            }
        }
        private void btn_oFG_Click(object sender, RoutedEventArgs e)
        {
            if (NewColorDialog(bdr_oFG.Background, out Color newColor))
            {
                setting.clrOutputFG = new SolidColorBrush(newColor);
                setting.Save();
                bdr_oFG.Background = setting.clrOutputFG;
            }
        }
        private void btn_iBG_Click(object sender, RoutedEventArgs e)
        {
            if (NewColorDialog(bdr_iBG.Background, out Color newColor))
            {
                setting.clrInputBG = new SolidColorBrush(newColor);
                setting.Save();
                bdr_iBG.Background = setting.clrInputBG;
            }
        }
        private void btn_iFG_Click(object sender, RoutedEventArgs e)
        {
            if (NewColorDialog(bdr_iFG.Background, out Color newColor))
            {
                setting.clrInputFG = new SolidColorBrush(newColor);
                setting.Save();
                bdr_iFG.Background = setting.clrInputFG;
            }
        }
        private bool NewColorDialog(Brush clrBrush, out Color newColor)
        {
            UI.ColorExpertDialog clrWin = new UI.ColorExpertDialog()
            {
                Owner = this,
                WorkingColor = ((SolidColorBrush)clrBrush).Color,
            };
            //clrWin.btn_ok.Content = "Ok";
            //clrWin.btn_cancle.Content = "Cancel";
            if (clrWin.ShowDialog() == true)
            {
                newColor = clrWin.WorkingColor;
                return true;
            }
            return false;
        }



        private void btn_font_Click(object sender, RoutedEventArgs e)
        {
            UI.FontDialog fontWin = new UI.FontDialog()
            {
                Owner = core.mainWindow,
                SettingFontFamily = setting.fontFamily,
                SettingFontSize = setting.fontSize,
                SettingFontStyle = setting.fontStyle,
                SettingFontWeight = setting.fontWeight,
            };

            if (fontWin.ShowDialog() == true)
            {
                setting.fontFamily = fontWin.SettingFontFamily;
                setting.fontSize = fontWin.SettingFontSize;
                setting.fontStyle = fontWin.SettingFontStyle;
                setting.fontWeight = fontWin.SettingFontWeight;
                setting.Save();
            }
        }

        #endregion


        #region 设置，输出 时间前缀， 记录日志
        private void cb_isOutputTimePrefix_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (isIniting) return;
            setting.isOutputTime = cb_isOutputTimePrefix.IsChecked == true;
            setting.Save();
        }
        private void cb_isLog_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (isIniting) return;
            setting.isLog = cb_isLog.IsChecked == true;
            setting.Save();
        }

        private void tb_outputTimePrefixFormat_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isIniting) return;

            tb_timeExample.Text = DateTime.Now.ToString(tb_outputTimePrefixFormat.Text);
        }
        private void tb_outputTimePrefixFormat_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (isIniting) return;
            switch (e.Key)
            {
                case Key.Enter:
                    tb_outputTimePrefixFormat_LostFocus(null, null);
                    break;
                case Key.Escape:
                    tb_outputTimePrefixFormat.Text = setting.outputTimeFormat;
                    break;
            }
        }

        private void tb_outputTimePrefixFormat_LostFocus(object sender, RoutedEventArgs e)
        {
            if (isIniting) return;
            setting.outputTimeFormat = tb_outputTimePrefixFormat.Text;
            setting.Save();
        }



        #endregion


        #region 工作窗口
        private async void dataGrid_cfgList_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {


        }
        private int GetConsoleTabIndex(string cfgName)
        {
            TabItem ti;
            TabItemHeader header;
            for (int i = 1, iv = tc.Items.Count; i < iv; ++i)
            {
                ti = (TabItem)tc.Items[i];
                if (ti.Header is TabItemHeader)
                {
                    header = (TabItemHeader)ti.Header;
                    if (cfgName == header.tb2.Text)
                        return i;
                }
            }
            return -1;
        }
        private TabItemHeader GetConsoleTabHeader(int idx)
        {
            return (TabItemHeader)((TabItem)tc.Items[idx]).Header;
        }
        private PanelConsole GetConsoleTabPanel(int idx)
        {
            return (PanelConsole)((TabItem)tc.Items[idx]).Content;
        }
        private PanelConsole tc_SelectedPanelPre;
        private void tc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tc.SelectedItem != null && setting.isLog)
            {
                object tabHeader = ((TabItem)tc.SelectedItem).Header;
                if (tabHeader is TabItemHeader)
                {
                    core.Logger.Log($"Change Main-Tab to [{((TabItemHeader)tabHeader).GetHeaderText()}]");
                }
                else if (tabHeader is string)
                {
                    core.Logger.Log($"Change Main-Tab to [{tabHeader}]");
                }
                else
                {
                    core.Logger.Log($"Change Main-Tab to index [{tc.SelectedIndex}]");
                }
            }

            if (tc_SelectedPanelPre != null)
            {
                tc_SelectedPanelPre.TrySaveCurProfileFromUI();
            }

            if (!(tc.SelectedContent is PanelConsole))
            {
                tc_SelectedPanelPre = null;
                return;
            }

            // 检查字号和颜色，更新；
            PanelConsole panel = (PanelConsole)tc.SelectedContent;
            panel.tbOut.Background = setting.clrOutputBG;
            panel.tbOut.Foreground = setting.clrOutputFG;
            panel.tbIn.Background = setting.clrInputBG;
            panel.tbIn.Foreground = setting.clrInputFG;

            panel.TryReloadCurProfile();
            tc_SelectedPanelPre = panel;

            //
            //
            //
            //
        }

        internal void CloseConsoleTab(string cfgName)
        {
            int tabIdx = GetConsoleTabIndex(cfgName);
            if (tabIdx > 0)
                tc.Items.RemoveAt(tabIdx);
        }


        #endregion

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F4:
                    if (tc.SelectedIndex > 0
                        && tc.SelectedContent is PanelConsole)
                    {
                        PanelConsole panelConsole = (PanelConsole)tc.SelectedContent;
                        panelConsole.tbIn.Focus();
                    }
                    break;
            }
        }

    }
}
