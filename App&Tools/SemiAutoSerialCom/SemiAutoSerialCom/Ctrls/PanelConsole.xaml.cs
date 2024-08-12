using MadTomDev.App.Classes;
using MadTomDev.App.Translators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Console = MadTomDev.App.Classes.Console;

namespace MadTomDev.App.Ctrls
{
    /// <summary>
    /// Interaction logic for PanelConsole.xaml
    /// </summary>
    public partial class PanelConsole : UserControl
    {
        public PanelConsole()
        {
            InitializeComponent();
        }

        private Core core = Core.Instance;
        private Settings setting = Core.Instance.setting;
        private QuickInputProfiles qiProfiles = Core.Instance.quickInputProfiles;


        #region receive messages from console, init console - load translators, quick inputs

        private Console _Console;
        public Console Console
        {
            set
            {
                if (_Console != null)
                {
                    _Console.RaiseTraffic -= _Console_RaiseTraffic;
                    _Console.RaiseSerialError -= _Console_RaiseSerialError;
                    _Console = null;
                }
                _Console = value;
                _Console.RaiseEvent += _Console_RaiseEvent;
                _Console.RaiseTraffic += _Console_RaiseTraffic;
                _Console.RaiseSerialError += _Console_RaiseSerialError;

                // try load translators
                if (value != null && value.cfg.translatorList.Count > 0)
                {
                    expTranslators.IsExpanded = true;
                    // first expand, load after load list;
                }

                // try load quick-inputs
                ReLoadQuickInputList(out string matchProfileName);
                if (matchProfileName != null)
                {
                    cbQuickInputProfiles.SelectedItem = matchProfileName;
                    cbQuickInputProfiles_SelectionChanged(null, null);
                }
            }
        }

        private void _Console_RaiseEvent(Console sender, string msg)
        {
            TextOutAppendLine("Serial Event: " + msg, true);
        }
        private void _Console_RaiseTraffic(Console sender, bool inOrOut, string msg)
        {
            // translate machineMsg to userMsg
            bool tSeccess;
            string[] newMsgs = TryTranslate(msg, true, out tSeccess);
            if (newMsgs.Length > 0)
            {
                string lastLine = newMsgs[newMsgs.Length - 1];
                if (!lastLine.EndsWith('\r') && !lastLine.EndsWith('\n'))
                {
                    newMsgs[newMsgs.Length - 1] += "\r";
                }
            }

            if (tSeccess)
            {
                foreach (string newMsg in newMsgs)
                {
                    TextOutAppendLine(newMsg, inOrOut);
                }
            }
            else
            {
                TextOutAppendLine(msg, inOrOut);
                TextOutAppendLine_translateFailed_noSuitableTranslator();
            }


        }
        private void _Console_RaiseSerialError(Console sender, System.IO.Ports.SerialError err)
        {
            TextOutAppendLine("Serial Error: " + err.ToString(), false);
        }


        #endregion


        #region set textboxes
        public void TextOutAppend(string tx, bool withoutTime = false)
        {
            Dispatcher.Invoke(() =>
            {
                if (withoutTime)
                {
                    tbOut.AppendText(tx);
                }
                else
                {
                    if (setting.isOutputTime)
                        tbOut.AppendText($"{DateTime.Now.ToString(setting.outputTimeFormat)} {tx}");
                    else
                        tbOut.AppendText(tx);
                }
            });
        }
        /// <summary>
        /// Set text to main textBox
        /// </summary>
        /// <param name="tx">test to set</param>
        /// <param name="inOrOut">in-from machine; out-from console;</param>
        /// <param name="withoutTime">false to add a time at the beginning of line</param>
        public void TextOutAppendLine(string tx, bool inOrOut, bool withoutTime = false)
        {
            Dispatcher.Invoke(() =>
            {
                string outMarkStr = (inOrOut ? " >> " : "<<  ");
                string fullMsg;
                if (withoutTime)
                {
                    fullMsg = outMarkStr + tx;
                }
                else
                {
                    if (setting.isOutputTime)
                        fullMsg = $"{DateTime.Now.ToString(setting.outputTimeFormat)} {outMarkStr}{tx}";
                    else
                        fullMsg = outMarkStr + tx;
                }

                core.TryLog("O:" + fullMsg);
                tbOut.AppendText(Environment.NewLine + fullMsg);
                tbOut.SelectionLength = 0;
                tbOut.SelectionStart = tbOut.Text.Length;
                tbOut.ScrollToEnd();
            });
        }
        public void TextInSet(string tx)
        {
            Dispatcher.Invoke(() =>
            {
                core.TryLog("I:" + tx);
                tbIn.Text = tx;
                tbIn.SelectionLength = 0;
                tbIn.SelectionStart = tx.Length;
            });
        }
        public void TextOutClear()
        {
            Dispatcher.BeginInvoke(() =>
            {
                core.TryLog("Clear output text.");
                tbOut.Clear();
                core.ShowStatues($"Clear screen.", Core.StatuesLevels.InfoGreen);
            });
        }
        public void TextInClear()
        {
            Dispatcher.BeginInvoke(() =>
            {
                core.TryLog("Clear input text.");
                tbIn.Clear();
            });
        }

        #endregion


        #region send cmd btns, remember cmd input history

        private List<string> cmdHistory = new List<string>();
        private int cmdHistoryPtr;
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string cmd = tbIn.Text;
            if (cmd == "")
            {
                tbOut.AppendText(Environment.NewLine);
                tbOut.SelectionLength = 0;
                tbOut.SelectionStart = tbOut.Text.Length;
                tbOut.ScrollToEnd();
                return;
            }



            if (cmdHistory.Count > 100)
            {
                for (int c = 50; c > 0; --c)
                {
                    cmdHistory.RemoveAt(0);
                }
            }

            TextInClear();

            if (cmd.Contains('\r') || cmd.Contains('\n'))
                cmd = cmd.Split('\r', '\n')[0];
            // 2024-08-09
            // no duplication in last history
            if (cmdHistory.Count == 0
                || cmdHistory[cmdHistory.Count - 1] != cmd)
            {
                cmdHistory.Add(cmd);
            }
            cmdHistoryPtr = cmdHistory.Count - 1;

            // translate userCmd to machineCmd
            string[] newCmds = TryTranslate(cmd, false, out bool tSeccess);
            if (!tSeccess)
            {
                TextOutAppendLine_translateFailed_noSuitableTranslator();
            }

            bool sendSeccess = true;
            foreach (string newCmd in newCmds)
            {
                sendSeccess = _Console.Send(newCmd, false);
                if (sendSeccess == false)
                {
                    break;
                }
            }

            if (sendSeccess)
            {
                TextOutAppendLine(cmd, false);
                core.ShowStatues($"Cmd sent.", Core.StatuesLevels.InfoGreen);
            }
            else
            {
                this.IsEnabled = false;
                core.ShowStatues($"Send cmd failed, Com may has closed!", Core.StatuesLevels.ErrorRed);
            }

            tbIn_upkeyPressed = false;
        }

        private void btnCls_Click(object sender, RoutedEventArgs e)
        {
            TextOutClear();
        }

        private bool tbIn_upkeyPressed = false;
        private void tbIn_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (cmdHistory.Count == 0)
                    {
                        break;
                    }
                    if (tbIn_upkeyPressed)
                    {
                        if (--cmdHistoryPtr < 0)
                        {
                            cmdHistoryPtr = 0;
                        }
                    }
                    TextInSet(cmdHistory[cmdHistoryPtr]);
                    tbIn_upkeyPressed = true;
                    break;
                case Key.Down:
                    if (cmdHistory.Count <= ++cmdHistoryPtr)
                        cmdHistoryPtr = cmdHistory.Count - 1;
                    TextInSet(cmdHistory[cmdHistoryPtr]);
                    break;
                case Key.Enter:
                    btnSend_Click(null, null);
                    break;
            }
        }

        #endregion


        #region translators

        int translatorsCountAll, translatorsCountSelected;
        List<ITranslator> activeTranslators = new List<ITranslator>();
        bool settingTranslatorsLoaded = false;
        private void expTranslators_Expanded(object sender, RoutedEventArgs e)
        {
            foreach (UIElement ui in spTranslators.Children)
            {
                if (ui is CheckBoxTranslator)
                {
                    ((CheckBoxTranslator)ui).CheckChanged -= NewCBT_CheckChanged;
                }
            }
            spTranslators.Children.Clear();

            // load from setting - 1 vars
            List<Core.TranslatorInfo> settingSelectedTInfoList = new List<Core.TranslatorInfo>();
            List<string> settingTNameList = _Console.cfg.translatorList;

            core.TryLog("Load translator metaData list.");
            CheckBoxTranslator newCBT;
            string fileNamePrefix, title;
            translatorsCountAll = 0;
            translatorsCountSelected = 0;
            foreach (Core.TranslatorInfo info in core.ReloadTranslatorInfoList())
            {
                if (!info.isDuplicated)
                {
                    ++translatorsCountAll;
                }

                // 2023 1219 如果文件名和翻译器名不匹配，则额外显示文件名；
                fileNamePrefix = GetFileNamePrefix(info.file.Name);
                if (fileNamePrefix != info.metaData?.Name)
                {
                    title = info.metaData?.Name + Environment.NewLine + $"({info.file.Name})";
                }
                else
                {
                    title = fileNamePrefix;
                }

                newCBT = new CheckBoxTranslator()
                {
                    TranslatorInfo = info,
                    TextTitle = title,
                    TextContent = info.GetMetaDataInfoString(),
                    IsChecked = !info.isDuplicated && (activeTranslators.Find(t => t.GetType().Name == info.metaData?.Name) != null),
                    IsEnabled = !info.isDuplicated,
                };

                // load from setting - 2 load info-list
                if (settingTranslatorsLoaded == false)
                {
                    if (info.metaData != null && info.metaData.Name != null
                        && settingTNameList.Contains(info.metaData.Name))
                    {
                        settingSelectedTInfoList.Add(info);
                        newCBT.IsChecked = true;
                    }
                }

                newCBT.CheckChanged += NewCBT_CheckChanged;
                spTranslators.Children.Add(newCBT);
            }

            // load from setting - 3 load translators
            if (settingTranslatorsLoaded == false)
            {
                activeTranslators = core.ReloadTranslators(settingSelectedTInfoList);
                core.TryLog($"Load translators [{GetTranslatorNamesString(activeTranslators)}] from setting.");
                translatorsCountSelected = activeTranslators.Count;
                tbSelectedTranslatorCount_setCount();
                settingTranslatorsLoaded = true;
            }

            btnTranslatorsApply.IsEnabled = false;

            string GetFileNamePrefix(string fileName)
            {
                return fileName.Substring(0, fileName.Length - 4);
            }
        }
        private void tbSelectedTranslatorCount_setCount()
        {
            tbSelectedTranslatorCount.Text = $"({translatorsCountSelected} / {translatorsCountAll})";
        }
        private string GetTranslatorNamesString(List<Core.TranslatorInfo> tInfoList)
        {
            if (tInfoList == null || tInfoList.Count == 0)
                return null;

            StringBuilder strBdr = new StringBuilder();
            foreach (Core.TranslatorInfo t in tInfoList)
            {
                strBdr.Append(t.metaData.Name);
                strBdr.Append(", ");
            }
            strBdr.Remove(strBdr.Length - 2, 2);

            return strBdr.ToString();
        }
        private string GetTranslatorNamesString(List<ITranslator> tList)
        {
            if (tList == null || tList.Count == 0)
                return null;

            StringBuilder strBdr = new StringBuilder();
            foreach (ITranslator t in tList)
            {
                strBdr.Append(t.GetType().Name);
                strBdr.Append(", ");
            }
            strBdr.Remove(strBdr.Length - 2, 2);

            return strBdr.ToString();
        }

        private void NewCBT_CheckChanged(CheckBoxTranslator obj)
        {
            btnTranslatorsApply.IsEnabled = true;
        }
        private void btnTranslatorsApply_Click(object sender, RoutedEventArgs e)
        {
            List<Core.TranslatorInfo> selectedTransLaterInfoList = new List<Core.TranslatorInfo>();
            foreach (CheckBoxTranslator cbt in spTranslators.Children)
            {
                if (cbt.IsChecked)
                {
                    selectedTransLaterInfoList.Add(cbt.TranslatorInfo);
                }
            }
            if (selectedTransLaterInfoList.Count > 0)
            {
                activeTranslators = core.ReloadTranslators(selectedTransLaterInfoList);
                core.TryLog($"Load translators [{GetTranslatorNamesString(activeTranslators)}] by user selection.");
            }
            else
            {
                activeTranslators.Clear();
                core.TryLog($"Un-Load all translators.");
            }
            translatorsCountSelected = activeTranslators.Count;
            tbSelectedTranslatorCount_setCount();

            // save to setting
            _Console.cfg.translatorList.Clear();
            foreach (Core.TranslatorInfo tInfo in selectedTransLaterInfoList)
            {
                if (tInfo.metaData != null && tInfo.metaData.Name != null)
                {
                    _Console.cfg.translatorList.Add(tInfo.metaData.Name);
                }
            }
            setting.Save();

            btnTranslatorsApply.IsEnabled = false;
            tbIn.Focus();
        }

        private string[] TryTranslate(string oriMsg, bool toUser_orMachine, out bool seccess)
        {
            if (activeTranslators == null || activeTranslators.Count == 0)
            {
                seccess = true;
                return new string[] { oriMsg };
            }

            seccess = false;
            List<string> result = new List<string>();
            string newMsg;
            bool tSeccess;
            foreach (ITranslator t in activeTranslators)
            {
                if (toUser_orMachine)
                {
                    newMsg = t.ToUser(oriMsg, out tSeccess);
                }
                else
                {
                    newMsg = t.ToMachine(oriMsg, out tSeccess);
                }
                if (tSeccess)
                {
                    seccess = true;
                    result.Add(newMsg);
                }
            }
            if (!seccess)
            {
                return new string[] { oriMsg };
            }
            else
            {
                return result.ToArray();
            }
        }

        #endregion

        #region 

        private void TextOutAppendLine_translateFailed_noSuitableTranslator()
        {
            TextOutAppendLine("Translate failed, no suitable translator.", true);
        }







        #endregion


        #region quick input

        private void ReLoadQuickInputList(out string matchProfileName)
        {
            matchProfileName = null;
            cbQuickInputProfiles.Items.Clear();
            if (_Console != null)
            {
                string cfgName = _Console.cfg.Name;
                foreach (string pName in qiProfiles.ProfileNameList)
                {
                    cbQuickInputProfiles.Items.Add(pName);
                    if (pName == cfgName)
                    {
                        matchProfileName = pName;
                    }
                }
            }
            QuickInputPanelClear();
            btnAddQuickInputItem.IsEnabled = false;
        }

        private string cbQuickInputProfiles_SelectionPre;
        private string cbQuickInputProfiles_curProfileName = null;
        private List<string> cbQuickInputProfiles_curProfile;
        private void cbQuickInputProfiles_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TrySaveCurProfileFromUI();
        }
        public void TrySaveCurProfileFromUI()
        {
            if (QuickInputs_curProfileFromUI())
            {
                core.quickInputProfiles.Save();
            }
        }
        public void TryReloadCurProfile()
        {
            if (cbQuickInputProfiles_curProfile != null)
            {
                cbQuickInputProfiles_curProfile = core.quickInputProfiles.GetProfile(cbQuickInputProfiles_curProfileName);
                QuickInputPanelFill(cbQuickInputProfiles_curProfile);
            }
        }
        private bool QuickInputs_curProfileFromUI()
        {
            if (cbQuickInputProfiles_curProfile != null)
            {
                cbQuickInputProfiles_curProfile = core.quickInputProfiles.GetProfile(cbQuickInputProfiles_curProfileName);
                cbQuickInputProfiles_curProfile.Clear();
                object testUI;
                for (int i = 0, iv = spQuickInputs.Children.Count; i < iv; ++i)
                {
                    testUI = spQuickInputs.Children[i];
                    if (testUI is PanelQuickInput)
                    {
                        cbQuickInputProfiles_curProfile.Add(((PanelQuickInput)testUI).QuickText);
                    }
                }
                return true;
            }
            return false;
        }
        private void cbQuickInputProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string pName = (string)cbQuickInputProfiles.SelectedItem;
            if (string.IsNullOrEmpty(pName))
            {
                cbQuickInputProfiles_curProfileName = null;
                cbQuickInputProfiles_curProfile = null;
                return;
            }
            if (cbQuickInputProfiles_SelectionPre == pName)
                return;

            TrySaveCurProfileFromUI();

            cbQuickInputProfiles_curProfileName = pName;
            cbQuickInputProfiles_curProfile = qiProfiles.GetProfile(pName);
            if (cbQuickInputProfiles_curProfile != null)
            {
                cbQuickInputProfiles_SelectionPre = pName;
                QuickInputPanelFill(cbQuickInputProfiles_curProfile);
                btnAddQuickInputItem.IsEnabled = true;
            }
            //else
            //{
            //   it never fired
            //    cbQuickInputProfiles_SelectionPre = null;
            //    QuickInputPanelClear();
            //    btnAddQuickInputItem.IsEnabled = false;
            //}
        }
        private async void cbQuickInputProfiles_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            await Task.Delay(3);
            string pName = cbQuickInputProfiles.Text;
            cbQuickInputProfiles_curProfile = qiProfiles.GetProfile(pName);
            if (cbQuickInputProfiles_curProfile == null)
            {
                cbQuickInputProfiles_SelectionPre = null;
                QuickInputPanelClear();
                btnAddQuickInputItem.IsEnabled = false;
            }
        }
        private void QuickInputPanelClear()
        {
            UIElement ui;
            for (int i = spQuickInputs.Children.Count - 1; i >= 0; --i)
            {
                ui = spQuickInputs.Children[i];
                if (ui is PanelQuickInput)
                {
                    spQuickInputs.Children.RemoveAt(i);
                }
            }
        }
        /// <summary>
        /// 填充快速输入列表；
        /// *无需清空，少项自动增加，多项自动缩减；
        /// </summary>
        /// <param name="profile"></param>
        private void QuickInputPanelFill(List<string> profile)
        {
            int oriQIICount = spQuickInputs.Children.Count - 2;
            if (oriQIICount < profile.Count)
            {
                // increase items
                for (int i = profile.Count - oriQIICount; i > 0; --i)
                {
                    spQuickInputs_increaseItem();
                }
            }
            else if (oriQIICount > profile.Count)
            {
                // reduce items
                for (int i = oriQIICount - profile.Count; i > 0; --i)
                {
                    spQuickInputs_decreaseItem();
                }
            }
            for (int i = 1, j = 0, jv = profile.Count; j < jv; ++i, ++j)
            {
                ((PanelQuickInput)spQuickInputs.Children[i]).QuickText = profile[j];
            }
        }
        private void spQuickInputs_increaseItem()
        {
            int insertIdx = spQuickInputs.Children.Count - 1;
            PanelQuickInput newItem = new PanelQuickInput()
            { QuickText = $"[{insertIdx}]", };
            newItem.btnRemoveClicked += QIItem_btnRemoveClicked;
            newItem.btnSendClicked += QIItem_btnSendClicked;
            newItem.MouseDown += NewItem_MouseDown;
            spQuickInputs.Children.Insert(insertIdx, newItem);
        }
        private void spQuickInputs_decreaseItem()
        {
            PanelQuickInput oldItem = (PanelQuickInput)spQuickInputs.Children[1];
            oldItem.btnRemoveClicked -= QIItem_btnRemoveClicked;
            oldItem.btnSendClicked -= QIItem_btnSendClicked;
            oldItem.MouseDown -= NewItem_MouseDown;
            spQuickInputs.Children.Remove(oldItem);
        }


        private void btnAddProfile_Click(object sender, RoutedEventArgs e)
        {
            TrySaveCurProfileFromUI();

            string newProfileName = cbQuickInputProfiles.Text;
            if (string.IsNullOrEmpty(newProfileName))
            {
                if (_Console != null)
                {
                    newProfileName = _Console.cfg.Name;
                }
            }
            if (qiProfiles.HasProfile(newProfileName))
            {
                // exists, show warnning
                core.ShowStatues($"Profile [{newProfileName}] already exists!", Core.StatuesLevels.WarningOrange);
            }
            else
            {
                // add profile
                qiProfiles.AddProfile(newProfileName);
                cbQuickInputProfiles_curProfileName = newProfileName;
                cbQuickInputProfiles_curProfile = qiProfiles.GetProfile(newProfileName);
                ReLoadQuickInputList(out string missing);
                cbQuickInputProfiles.Text = newProfileName;
                spQuickInputs_increaseItem();
                btnAddQuickInputItem.IsEnabled = true;
                core.ShowStatues($"New profile [{newProfileName}] created.", Core.StatuesLevels.InfoGreen);
            }
        }


        private void QIItem_btnSendClicked(PanelQuickInput self)
        {
            tbIn.Focus();
            if (tbIn.Text == self.QuickText)
            {
                btnSend_Click(null, null);
            }
            else
            {
                tbIn.Text = self.QuickText;
                core.ShowStatues($"Cmd set, click again to send.", Core.StatuesLevels.InfoGreen);
            }
        }

        private void QIItem_btnRemoveClicked(PanelQuickInput self)
        {
            spQuickInputs.Children.Remove(self);
            core.ShowStatues($"Entry removed.", Core.StatuesLevels.WarningOrange);
        }

        private void btnAddQuickInputItem_Click(object sender, RoutedEventArgs e)
        {
            spQuickInputs_increaseItem();
            core.ShowStatues($"Entry added.", Core.StatuesLevels.InfoGreen);
        }

        #region quick input ui, drag to move


        private PanelQuickInput QuickInputItem_mouseDown = null;
        private DependencyObject QuickInputItem_mouseOverPre = null;
        private Point QuickInputItem_mouseDownPoint;
        private void NewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            QuickInputItem_mouseDownPoint = Mouse.GetPosition(spQuickInputs);
            QuickInputItem_mouseDown = (PanelQuickInput)sender;
        }


        private bool QuickInputItem_doMove = false;
        private void spQuickInputs_MouseMove(object sender, MouseEventArgs e)
        {
            if (QuickInputItem_mouseDown != null)
            {
                Point curMP = Mouse.GetPosition(spQuickInputs);
                if (QuickInputItem_doMove)
                {
                    PanelQuickInput curQIItem = null;
                    HitTestResult hitTestResults = VisualTreeHelper.HitTest(spQuickInputs, curMP);
                    if (hitTestResults != null
                        && hitTestResults.VisualHit is Visual)
                    {
                        if (QuickInputItem_mouseOverPre != hitTestResults.VisualHit)
                        {
                            QuickInputItem_mouseOverPre = hitTestResults.VisualHit;

                            if (hitTestResults.VisualHit is Rectangle)
                            {
                                Rectangle testRect = (Rectangle)hitTestResults.VisualHit;
                                if (testRect.Parent is Grid)
                                {
                                    Grid testGrid = (Grid)testRect.Parent;
                                    if (testGrid.Parent is PanelQuickInput)
                                    {
                                        curQIItem = (PanelQuickInput)testGrid.Parent;
                                    }
                                }
                            }
                        }
                    }
                    if (curQIItem != null)
                    {
                        int idxCur = spQuickInputs.Children.IndexOf(curQIItem);
                        int idxSur = spQuickInputs.Children.IndexOf(QuickInputItem_mouseDown);
                        int idxIdc = spQuickInputs.Children.IndexOf(bdrMovePosiIndicator);
                        if (idxCur == idxSur)
                        {
                            QuickInputItem_IndicatorReset();
                        }
                        else if (idxCur < idxSur)
                        {
                            if (idxIdc > idxCur)
                                IndicatorShow(idxCur);
                            else
                                IndicatorShow(idxCur - 1);
                        }
                        else
                        {
                            if (idxIdc > idxCur)
                                IndicatorShow(idxCur + 1);
                            else
                                IndicatorShow(idxCur);
                        }
                    }
                }
                else
                {
                    if (Math.Abs(QuickInputItem_mouseDownPoint.X - curMP.X) >= 3
                        || Math.Abs(QuickInputItem_mouseDownPoint.Y - curMP.Y) >= 3)
                    {
                        QuickInputItem_setAllTopCovers(true);
                        spQuickInputs.Cursor = Cursors.SizeNS;
                        QuickInputItem_doMove = true;
                    }
                }
                void IndicatorShow(int posi)
                {
                    bdrMovePosiIndicator.Visibility = Visibility.Visible;
                    spQuickInputs.Children.Remove(bdrMovePosiIndicator);
                    spQuickInputs.Children.Insert(posi, bdrMovePosiIndicator);
                }
            }
        }
        private void spQuickInputs_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (QuickInputItem_mouseDown != null
                && bdrMovePosiIndicator.Visibility == Visibility.Visible)
            {
                int idxIdc = spQuickInputs.Children.IndexOf(bdrMovePosiIndicator);
                spQuickInputs.Children.Remove(QuickInputItem_mouseDown);
                spQuickInputs.Children.Insert(idxIdc, QuickInputItem_mouseDown);
            }

            QuickInputItem_dragReset();
        }
        private void spQuickInputs_MouseLeave(object sender, MouseEventArgs e)
        {
            QuickInputItem_dragReset();
        }
        private void QuickInputItem_setAllTopCovers(bool isTopCoverOn)
        {
            foreach (UIElement ui in spQuickInputs.Children)
            {
                if (ui is PanelQuickInput)
                {
                    ((PanelQuickInput)ui).IsTopCoverOn = isTopCoverOn;
                }
            }
        }
        private void QuickInputItem_IndicatorReset()
        {
            bdrMovePosiIndicator.Visibility = Visibility.Hidden;
            spQuickInputs.Children.Remove(bdrMovePosiIndicator);
            spQuickInputs.Children.Insert(0, bdrMovePosiIndicator);
        }

        

        

        private void QuickInputItem_dragReset()
        {
            QuickInputItem_IndicatorReset();
            spQuickInputs.Cursor = Cursors.Arrow;
            QuickInputItem_doMove = false;
            QuickInputItem_mouseDown = null;
            QuickInputItem_mouseOverPre = null;
            QuickInputItem_setAllTopCovers(false);
        }


        #endregion

        #endregion

    }
}
