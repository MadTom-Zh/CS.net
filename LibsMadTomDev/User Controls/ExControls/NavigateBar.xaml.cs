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

namespace MadTomDev.UI
{
    /// <summary>
    /// 结构为 当前图标，根下拉按钮，导航按钮序列，地址下拉按钮，刷新按钮；
    /// 点击当前图标 或 序列空白区域，变更模式为地址输入模式，此时保留当前图标，之后内容换为输入框；
    /// 点击按钮主体，跳转（让tv选取此节点）到对应路径；
    /// 点击下拉按钮，弹出该层下所有目录的列表，如果不是路径尾节点，路径下一节点对应名称显示为黑体；
    /// 点击地址下拉按钮，改为地址输入模式，并弹出之前访问过的历史路径列表；
    /// </summary>
    public partial class NavigateBar : UserControl
    {
        public NavigateBar()
        {
            InitializeComponent();
            cMenu_btn.Closed += CMenu_btn_Closed;
        }

        private async void CMenu_btn_Closed(object sender, RoutedEventArgs e)
        {
            await Task.Delay(50);
            isCMenuBtnOpenedPre = false;
        }

        private string _PathSeparator = "\\";
        public string PathSeparator
        {
            get => _PathSeparator;
            set
            {
                _PathSeparator = value;
            }
        }

        public class NodeData
        {
            public BitmapSource icon;
            public string fullPath;
            /// <summary>
            /// 物理路径的名称，如C:\
            /// </summary>
            public string text;
            /// <summary>
            /// 节点显示的文本
            /// </summary>
            public string nodeText;
            public object tag;
        }

        /// <summary>
        /// 逐个节点，推送路径，如推送C: 和 tmp，得到C:\tmp；
        /// </summary>
        /// <param name="node"></param>
        public void PushNode(NodeData node, bool setIconN_URLText = false)
        {
            BtnDropDown newBtn = new BtnDropDown()
            {
                Tag = node,
                Text = node.text,

                ActionClick = ActionBtnClick,
                ActionDropDownClick = ActionBtnDropDownClick,
            };

            sPanel_btns.Children.Add(newBtn);
            newBtn.GetBtnWidth();
            if (setIconN_URLText)
            {
                Icon = node.icon;
                TextboxURL = GetBtnChainURL(newBtn);
            }
            Grid_SizeChanged(null, null);
        }

        public void AddCurURLToHistory()
        {
            if (!string.IsNullOrWhiteSpace(TextboxURL))
                AddHistory(Icon, TextboxURL);
        }


        /// <summary>
        /// 在指定位置切掉节点，及删除后续节点；
        /// 如当前路径C:tmp，删除位置0，则清空，从C:和以后全部删除；
        /// </summary>
        /// <param name="startIdx"></param>
        public void CutNodeAt(int startIdx, bool setIconN_URLText = false)
        {
            if (startIdx < 0)
                startIdx = 0;
            for (int i = sPanel_btns.Children.Count - 1; i >= startIdx; i--)
            {
                sPanel_btns.Children.RemoveAt(i);
            }
            if (startIdx > 0 && setIconN_URLText)
            {
                BtnDropDown lastBtn = (BtnDropDown)sPanel_btns.Children[startIdx - 1];
                Icon = ((NodeData)lastBtn.Tag).icon;
                TextboxURL = GetBtnChainURL(lastBtn);
            }
            Grid_SizeChanged(null, null);
        }
        public void SetFileRename(string oldFullName, string newName)
        {
            List<BtnDropDown> btnChain = GetBtnChain();
            BtnDropDown curBtn;
            NodeData curBtnData;
            string oldFullNameWithSpt = oldFullName + System.IO.Path.DirectorySeparatorChar;
            string newFullName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(oldFullName), newName);
            string newFullNameWithSpt = newFullName + System.IO.Path.DirectorySeparatorChar;
            for (int i = 0, iv = btnChain.Count; i < iv; ++i)
            {
                curBtn = btnChain[i];
                if (curBtn.Tag is NodeData)
                {
                    curBtnData = (NodeData)curBtn.Tag;
                    if (curBtnData.fullPath != null)
                    {
                        if (curBtnData.fullPath.StartsWith(oldFullName))
                        {
                            curBtnData.fullPath = curBtnData.fullPath.Replace(oldFullNameWithSpt, newFullNameWithSpt);
                        }
                        else if (curBtnData.fullPath == oldFullName)
                        {
                            curBtnData.fullPath = newFullName;
                            curBtnData.text = newName;
                            curBtnData.nodeText = newName;
                            curBtn.Text = newName;
                        }
                    }
                }
            }
        }


        public List<BtnDropDown> GetBtnChain()
        {
            List<BtnDropDown> chain = new List<BtnDropDown>();
            for (int i = 0, iv = sPanel_btns.Children.Count; i < iv; ++i)
            {
                chain.Add((BtnDropDown)sPanel_btns.Children[i]);
            }
            return chain;
        }
        private string GetBtnChainURL(BtnDropDown btn)
        {
            StringBuilder strBdr = new StringBuilder();
            BtnDropDown curBtn;
            NodeData curBtnData;
            for (int i = 0, iv = sPanel_btns.Children.Count; i < iv; i++)
            {
                curBtn = (BtnDropDown)sPanel_btns.Children[i];
                curBtnData = (NodeData)curBtn.Tag;
                strBdr.Append(curBtnData.nodeText);
                strBdr.Append(_PathSeparator);
                if (curBtn == btn)
                    break;
            }
            if (strBdr.Length > 0)
            {
                int psLength = _PathSeparator.Length;
                strBdr.Remove(strBdr.Length - psLength, psLength);
            }

            return strBdr.ToString();
        }

        public BitmapSource Icon
        {
            get
            {
                object testIcon = img_rootIcon.Source;
                if (testIcon is BitmapSource)
                {
                    return (BitmapSource)testIcon;
                }
                return null;
            }
            set => img_rootIcon.Source = value;
        }
        public BitmapSource GetLastBtnIcon()
        {
            if (sPanel_btns.Children.Count > 0)
            {
                BtnDropDown btn = (BtnDropDown)sPanel_btns.Children[sPanel_btns.Children.Count - 1];
                return ((NodeData)btn.Tag).icon;
            }
            return null;
        }
        private BtnDropDown GetNextBtn(BtnDropDown curBtn)
        {
            int btnIdx = sPanel_btns.Children.IndexOf(curBtn);
            if (btnIdx >= 0 && btnIdx < sPanel_btns.Children.Count - 1)
            {
                return (BtnDropDown)sPanel_btns.Children[btnIdx + 1];
            }
            return null;
        }

        public string TextboxURL
        {
            get => tb_url.Text;
            set => tb_url.Text = value;
        }

        public Action<string> ActionGotoURL;
        public Func<string, List<NodeData>> FuncGetSubList = null;

        public void ActionBtnClick(BtnDropDown btn)
        {
            ActionGotoURL?.Invoke(GetBtnChainURL(btn));
        }

        public void ActionBtnDropDownClick(BtnDropDown btn)
        {
            if (FuncGetSubList == null)
                return;
            string btnURL = GetBtnChainURL(btn);
            BtnDropDown nextBtn = GetNextBtn(btn);

            OpenSubDirMenu(FuncGetSubList.Invoke(btnURL), btn, btnURL, nextBtn == null ? null : nextBtn.Text);
        }
        private void OpenSubDirMenu(List<NodeData> subNodes, Control parent, string parentPath, string nextNodeText)
        {
            cMenu_btn.Items.Clear();
            parent.ContextMenu = cMenu_btn;
            NodeData node;
            MenuItem mi;
            for (int i = 0, iv = subNodes.Count; i < iv; i++)
            {
                node = subNodes[i];
                mi = new MenuItem()
                {
                    Tag = node,
                    Header = node.text,
                    Icon = new Image() { Source = node.icon },
                };
                if (node.text == nextNodeText)
                {
                    mi.FontWeight = FontWeights.Bold;
                }
                else
                {
                    mi.Click += (s1, e1) =>
                    {
                        MenuItem miClicked = (MenuItem)s1;
                        NodeData miNode = (NodeData)miClicked.Tag;
                        string newPath;
                        if (string.IsNullOrWhiteSpace(parentPath))
                        {
                            newPath = miNode.nodeText;
                        }
                        else
                        {
                            newPath = parentPath + _PathSeparator + miNode.nodeText;
                        }
                        ActionGotoURL?.Invoke(newPath);
                    };
                }
                cMenu_btn.Items.Add(mi);
            }
            parent.ContextMenu.IsOpen = true;
            isCMenuBtnOpenedPre = true;
        }
        private bool isCMenuBtnOpenedPre = false;
        private void OpenChainDirMenu(List<NodeData> nodeChain, Control parent)
        {
            cMenu_btn.Items.Clear();
            parent.ContextMenu = cMenu_btn;

            NodeData node;
            MenuItem mi;
            for (int i = 0, iv = nodeChain.Count; i < iv; i++)
            {
                node = nodeChain[i];
                mi = new MenuItem()
                {
                    Header = node.text,
                    Icon = new Image() { Source = node.icon },
                    Tag = node,
                };
                mi.Click += (s1, e1) =>
                {
                    MenuItem miClicked = (MenuItem)s1;
                    NodeData miNode = (NodeData)miClicked.Tag;
                    List<BtnDropDown> btnChain = GetBtnChain();
                    StringBuilder strBdr = new StringBuilder();
                    BtnDropDown curBtn;
                    for (int i = 0, iv = btnChain.Count; i < iv; i++)
                    {
                        curBtn = btnChain[i];
                        strBdr.Append(((NodeData)curBtn.Tag).nodeText);
                        strBdr.Append(PathSeparator);

                        if (curBtn.Tag == miNode)
                            break;
                    }
                    if (strBdr.Length > 0)
                    {
                        int psLen = PathSeparator.Length;
                        strBdr.Remove(strBdr.Length - psLen, psLen);
                    }
                    ActionGotoURL?.Invoke(strBdr.ToString());
                };
                cMenu_btn.Items.Add(mi);
            }

            parent.ContextMenu.IsOpen = true;
            isCMenuBtnOpenedPre = true;
        }

        private void btn_rootDown_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (sPanel_btns_allVisible)
            {
                if (FuncGetSubList == null)
                    return;
                OpenSubDirMenu(FuncGetSubList.Invoke(null), btn_rootDown, null, null);
            }
            else
            {
                List<NodeData> nodeChain = new List<NodeData>();
                UIElement ui;
                for (int i = 0, iv = sPanel_btns.Children.Count; i < iv; i++)
                {
                    ui = sPanel_btns.Children[i];
                    if (ui.Visibility == Visibility.Visible)
                        break;
                    nodeChain.Add((NodeData)((BtnDropDown)ui).Tag);
                }
                OpenChainDirMenu(nodeChain, btn_rootDown);
            }
        }
        private bool sPanel_btns_allVisible = true;

        private DateTime Grid_SizeChanged_actTime = DateTime.MinValue;
        private async void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sPanel_btns.Children.Count > 0)
            {
                DateTime now = DateTime.Now;
                if ((now - Grid_SizeChanged_actTime).TotalMilliseconds < 20)
                {
                    Grid_SizeChanged_actTime = now;
                    return;
                }
                Grid_SizeChanged_actTime = now;
                while ((DateTime.Now - Grid_SizeChanged_actTime).TotalMilliseconds < 30)
                {
                    await Task.Delay(30);
                }

                double btnPanelWidth = grid1.ActualWidth - btn_urlDown.ActualWidth - sPanel_btns.Margin.Left;
                if (btnPanelWidth <= 0)
                    return;
                int idx = sPanel_btns.Children.Count - 1;
                if (idx < 0)
                    return;
                double widthCount = ((BtnDropDown)sPanel_btns.Children[idx]).BtnWidth;
                int waitCount = 0;
                while (widthCount <= 0)
                {
                    await Task.Delay(5);
                    idx = sPanel_btns.Children.Count - 1;
                    if (idx < 0)
                        return;
                    widthCount = ((BtnDropDown)sPanel_btns.Children[idx]).BtnWidth;
                    if (++waitCount > 20)
                        return;
                }
                BtnDropDown child;
                sPanel_btns_allVisible = true;
                for (idx--; idx >= 0; idx--)
                {
                    child = (BtnDropDown)sPanel_btns.Children[idx];
                    widthCount += child.BtnWidth;
                    if (widthCount > btnPanelWidth)
                    {
                        child.Visibility = Visibility.Collapsed;
                        sPanel_btns_allVisible = false;
                    }
                    else
                    {
                        child.Visibility = Visibility.Visible;
                    }
                }

            }
            if (sPanel_btns_allVisible)
            {
                btn_rootDown.Content = GraphResource.PathArrowSmallRight;
            }
            else
            {
                btn_rootDown.Content = GraphResource.PathArrowSmallDoubleRight;
            }
        }



        private string URLPre;
        private void btn_root_Click(object sender, RoutedEventArgs e)
        {
            if (e != null)
                e.Handled = true;
            if (tb_url.Visibility != Visibility.Visible)
            {
                URLPre = TextboxURL;
                tb_url.Visibility = Visibility.Visible;
                tb_url.SelectAll();
                tb_url.Focus();
                tb_url_userInput = true;
                tb_url_TextPre = tb_url.Text;
                sPanel_btns.Visibility = Visibility.Collapsed;
            }
        }
        public void FocusEdit()
        {
            btn_root_Click(null, null);
        }

        private void sPanel_btns_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isCMenuBtnOpenedPre)
            {
                isCMenuBtnOpenedPre = false;
                e.Handled = true;
                return;
            }

            if (sPanel_btns.Children.Count > 0)
            {
                Point mp = e.GetPosition(sPanel_btns);
                BtnDropDown lastBtn = (BtnDropDown)sPanel_btns.Children[sPanel_btns.Children.Count - 1];
                Point lastBtnPosi = lastBtn.TranslatePoint(new Point(), sPanel_btns);
                if (mp.X >= 0 && mp.X <= lastBtnPosi.X + lastBtn.ActualWidth
                    && mp.Y >= lastBtnPosi.Y && mp.Y <= lastBtnPosi.Y + lastBtn.ActualHeight)
                {
                    // click some button
                }
                else
                {
                    // click blank area
                    btn_root_Click(null, null);
                }
            }
        }

        private bool tb_url_userInput = false;
        private string tb_url_TextPre;
        private void tb_url_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tb_url_userInput)
            {
                if (tb_url.Text == tb_url_TextPre)
                {
                    img_btn_refresh.Source = GraphResource.UIIconRefresh16;
                }
                else
                {
                    img_btn_refresh.Source = GraphResource.UIIconGoto16;
                }
            }
        }
        private void tb_url_LostFocus(object sender, RoutedEventArgs e)
        {
            tb_url_userInput = false;
            img_btn_refresh.Source = GraphResource.UIIconRefresh16;
            sPanel_btns.Visibility = Visibility.Visible;
            tb_url.Visibility = Visibility.Collapsed;
        }


        private void tb_url_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    ActionGotoURL(tb_url.Text);
                    tb_url_LostFocus(null, null);
                    break;
                case Key.Escape:
                    tb_url.Text = URLPre;
                    tb_url_LostFocus(null, null);
                    break;
            }
        }

        private struct HistoryData
        {
            public BitmapSource icon;
            public string fullPath;
        }
        List<HistoryData> history = new List<HistoryData>();
        public int HistoryMaxCount = 20;
        private void AddHistory(BitmapSource icon, string fullPath)
        {
            if (history.Count > 0 && history[0].fullPath == fullPath)
                return;

            HistoryData foundHistory = history.Find(h => h.fullPath == fullPath);
            HistoryData newHistory;
            if (foundHistory.fullPath != null)
            {
                newHistory = foundHistory;
                history.Remove(foundHistory);
            }
            else
            {
                newHistory = new HistoryData()
                { icon = icon, fullPath = fullPath };
                while (history.Count > HistoryMaxCount)
                    history.RemoveAt(history.Count - 1);
            }
            history.Insert(0, newHistory);
        }
        private void btn_urlDown_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (cMenu_url.IsOpen)
            {
                cMenu_url.IsOpen = false;
            }
            else
            {
                cMenu_url.Items.Clear();
                MenuItem mi;
                HistoryData hd;
                for (int i = 0, iv = history.Count; i < iv; i++)
                {
                    hd = history[i];
                    mi = new MenuItem()
                    { Icon = hd.icon, Header = hd.fullPath };
                    cMenu_url.Items.Add(mi);
                }

                cMenu_url.IsOpen = true;
            }
        }

        private void btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            ActionGotoURL(tb_url.Text);
        }

    }
}
