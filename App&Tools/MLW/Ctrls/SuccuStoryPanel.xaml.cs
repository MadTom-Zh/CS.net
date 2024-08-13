using MLW_Succubus_Storys.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;
using static MLW_Succubus_Storys.Classes.SuccuNode;

namespace MLW_Succubus_Storys.Ctrls
{
    /// <summary>
    /// Interaction logic for SuccuStoryPanel.xaml
    /// </summary>
    public partial class SuccuStoryPanel : UserControl
    {
        public SuccuStoryPanel()
        {
            InitializeComponent();
            Core.TrySetLocalTx(tbAPath, "availablePathes_");
        }

        private SuccuNode curSuccuNode;
        public async void Init(SuccuNode succuNode)
        {
            curSuccuNode = succuNode;
            // 顶端标题
            imgSuccu.Source = succuNode.succuIcon;
            tbSuccu.Text = succuNode.succuNameLocalized;
            imgMtl1.Source = succuNode.mtl1Icon;
            imgMtl2.Source = succuNode.mtl2Icon;
            imgMtl3.Source = succuNode.mtl3Icon;

            // 生成面板和节点按钮
            spStoryLevels.Children.Clear();
            spStoryLevels.Children.Add(GenStoryLevel(succuNode.storyLv1));
            spStoryLevels.Children.Add(GenStoryLevel(succuNode.storyLv2));
            spStoryLevels.Children.Add(GenStoryLevel(succuNode.storyLv3));

            await Task.Delay(200);

            // 生成箭头，连接关系
            List<BtnNode> leafNodes;
            GenArrows_inLevel((StackPanel)spStoryLevels.Children[0], out leafNodes);
            GenArrows_leafNodes(leafNodes);
            GenArrows_inLevel((StackPanel)spStoryLevels.Children[1], out leafNodes);
            GenArrows_leafNodes(leafNodes);
            GenArrows_inLevel((StackPanel)spStoryLevels.Children[2], out leafNodes);
            GenArrows_leafNodes(leafNodes);

        }

        #region gen node buttons

        private StackPanel GenStoryLevel(List<SuccuNode.StoryNode> storyList)
        {
            StackPanel levelPanel = new StackPanel()
            {
                Margin = new Thickness(20),
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            for (int i = 0, iv = storyList.Count; i < iv; ++i)
            {
                levelPanel.Children.Add(GenStoryBlock(storyList[i], i));
            }
            return levelPanel;
        }
        private Grid GenStoryBlock(StoryNode storyNode, int storyIndex)
        {
            Grid blockGrid = new Grid()
            {
                Margin = new Thickness(30),
                //VerticalAlignment = VerticalAlignment.Stretch,
            };

            // 添加连线画板，顶端节点；
            blockGrid.Children.Add(new Canvas());
            BtnNode topBtn = new BtnNode()
            {
                Tag = storyNode,
                ActionClicked = BtnNodeClicked,
            };
            BtnNodeIsBorderedChangedHandle(topBtn);
            topBtn.SetIconType(storyNode);
            topBtn.SetText(storyIndex + 1);
            blockGrid.Children.Add(topBtn);

            // 按照层级关系，将选择节点和结局节点码放到对应层级
            GenStoryBlock_AddNodes(blockGrid, 0, storyNode.choices);
            if (blockGrid.RowDefinitions.Count > 1)
            {
                blockGrid.RowDefinitions[0].Height = new GridLength(gridBtnHeight);
                blockGrid.RowDefinitions[blockGrid.RowDefinitions.Count - 1].Height = new GridLength(gridBtnHeight);
            }

            // 将连线画板充满整个grid
            Grid.SetRowSpan(blockGrid.Children[0], blockGrid.RowDefinitions.Count);
            Grid.SetColumnSpan(blockGrid.Children[0], blockGrid.ColumnDefinitions.Count);

            // 从底层向上，将父节点剧中；
            // 生成各节点位置信息，并绘制连线
            GenStoryBlock_AlighNodes(blockGrid);



            return blockGrid;
        }
        private void GenStoryBlock_AddNodes(Grid grid, int parentLevel, List<SuccuNode.ChoiceNode> nodes)
        {
            List<KeyValuePair<int, object>> dataList = new List<KeyValuePair<int, object>>();
            for (int i = 0, iv = nodes.Count; i < iv; ++i)
            {
                dataList.Add(new KeyValuePair<int, object>(i, nodes[i]));
            }
            GenStoryBlock_AddNodes(grid, parentLevel, dataList);
        }
        private static double gridBtnHeight = 50;
        private void GenStoryBlock_AddNodes(Grid grid, int parentLevel, List<KeyValuePair<int, object>> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                return;

            // 扩展grid网格高度
            int newRowCount = (parentLevel + 1) * 2 + 1;
            for (int i = grid.RowDefinitions.Count;
                i < newRowCount; ++i)
            {
                grid.RowDefinitions.Add(new RowDefinition()
                {
                    MinHeight = gridBtnHeight,
                    Height = new GridLength(gridBtnHeight, GridUnitType.Star),
                });
            }

            // 扩展grid网格宽度
            for (int i = grid.ColumnDefinitions.Count,
                    iv = nodes.Count * 2 - 1;
                i < iv; ++i)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    MinWidth = gridBtnHeight,
                });
            }


            // 添加选择节点，记录并准备下一循环
            BtnNode cBtn;
            int nodeIndex;
            object nodeValue;
            List<KeyValuePair<int, object>> subNodes = new List<KeyValuePair<int, object>>();
            SuccuNode.ChoiceNode curChoice;
            for (int i = 0, iv = nodes.Count; i < iv; ++i)
            {
                nodeIndex = nodes[i].Key;
                nodeValue = nodes[i].Value;
                cBtn = new BtnNode()
                {
                    Tag = nodeValue,
                    ActionClicked = BtnNodeClicked,
                };
                BtnNodeIsBorderedChangedHandle(cBtn);
                cBtn.SetIconType(nodeValue);
                if (nodeValue is EndingNode)
                {
                    cBtn.SetText(((EndingNode)nodeValue).isGoodOrBad);
                }
                else if (nodeValue is StoryNode)
                {
                    cBtn.isStoryLink = true;
                    cBtn.SetText((StoryNode)nodeValue);
                }
                else // choice
                {
                    cBtn.SetText(nodeIndex + 1);
                    curChoice = (ChoiceNode)nodeValue;
                    if (curChoice.nextStory != null)
                        subNodes.Add(new KeyValuePair<int, object>(0, curChoice.nextStory));
                    else if (curChoice.nextEnding != null)
                        subNodes.Add(new KeyValuePair<int, object>(0, curChoice.nextEnding));
                    else
                    {
                        for (int j = 0, jv = curChoice.nextChoices.Count; j < jv; ++j)
                            subNodes.Add(new KeyValuePair<int, object>(j, curChoice.nextChoices[j]));
                    }
                }
                Grid.SetRow(cBtn, newRowCount - 1);
                Grid.SetColumn(cBtn, i * 2);
                grid.Children.Add(cBtn);
            }
            // 逐层添加下一层节点
            GenStoryBlock_AddNodes(grid, parentLevel + 1, subNodes);
        }

        private void GenStoryBlock_AlighNodes(Grid grid)
        {
            BtnNode? curBtn;
            BtnNode[]? subBtns;
            for (int curRowIndex = Grid.GetRow(grid.Children[grid.Children.Count - 1]) - 2; curRowIndex >= 0; curRowIndex -= 2)
            {
                curBtn = FindLastBtn(curRowIndex);
                do
                {
                    subBtns = FindSubBtns(grid, curBtn);
                    if (subBtns != null)
                    {
                        if (subBtns.Length == 1)
                        {
                            // one sub node, end
                            Grid.SetColumn(curBtn, Grid.GetColumn(subBtns[0]));
                            Grid.SetRow(subBtns[0], grid.RowDefinitions.Count - 1);
                        }
                        else
                        {
                            // tow sub nodes
                            Grid.SetColumn(curBtn, (Grid.GetColumn(subBtns[0]) + Grid.GetColumn(subBtns[1])) / 2);
                        }
                    }
                    curBtn = FindPrevBtn(curBtn, curRowIndex);
                }
                while (curBtn != null);
            }

            BtnNode? FindLastBtn(int rowIndex)
            {
                int maxIdx;
                int testIdx, testRowIdx;
                UIElement testUI;
                testIdx = grid.Children.Count / 2;
                do
                {
                    maxIdx = grid.Children.Count - 1;
                    testUI = grid.Children[testIdx];
                    testRowIdx = Grid.GetRow(testUI);
                    if (testRowIdx < rowIndex)
                    {
                        testIdx += (maxIdx + testIdx) / 2;
                    }
                    else if (testRowIdx > rowIndex)
                    {
                        testIdx = testIdx / 2;
                    }
                    else // in row  or  testIdx == maxIdx
                    {
                        return FindLastBtn_inRow(testIdx, rowIndex);
                    }
                }
                while (0 <= testIdx && testIdx <= maxIdx);
                return null;
            }
            BtnNode? FindLastBtn_inRow(int startUIIdx, int rowIndex)
            {
                UIElement uiPrev = null;
                for (int i = startUIIdx, iv = grid.Children.Count - 1; i < iv; ++i)
                {
                    if (Grid.GetRow(grid.Children[i]) > rowIndex)
                        break;
                    uiPrev = grid.Children[i];
                }
                if (uiPrev == null) return null;
                else return (BtnNode)uiPrev;
            }
            BtnNode? FindPrevBtn(BtnNode curBtn, int rowIndex)
            {
                int testBtnIdx = grid.Children.IndexOf(curBtn) - 1;
                if (testBtnIdx < 1)
                    return null;
                UIElement testBtn = grid.Children[testBtnIdx];
                if (Grid.GetRow(testBtn) < rowIndex)
                    return null;
                else
                    return (BtnNode)testBtn;
            }
        }
        private BtnNode[]? FindSubBtns(Grid grid, BtnNode? curBtn)
        {
            if (curBtn == null)
                return null;

            List<BtnNode> result = new List<BtnNode>();
            object[] subData = new object[2];
            if (curBtn.Tag is StoryNode)
            {
                StoryNode sNode = (StoryNode)curBtn.Tag;
                if (sNode.choices.Count == 2)
                {
                    subData[0] = sNode.choices[0];
                    subData[1] = sNode.choices[1];
                }
            }
            else if (curBtn.Tag is ChoiceNode)
            {
                ChoiceNode cNode = (ChoiceNode)curBtn.Tag;
                if (cNode.nextChoices.Count == 2)
                {
                    subData[0] = cNode.nextChoices[0];
                    subData[1] = cNode.nextChoices[1];
                }
                else if (cNode.nextStory != null)
                    subData[0] = cNode.nextStory;
                else if (cNode.nextEnding != null)
                    subData[0] = cNode.nextEnding;
            }
            else return null;

            BtnNode testBtn;
            int subIdx = 0;
            for (int i = grid.Children.IndexOf(curBtn) + 1, iv = grid.Children.Count; i < iv; ++i)
            {
                testBtn = (BtnNode)grid.Children[i];
                if (subIdx == 0 && testBtn.Tag == subData[0])
                {
                    result.Add(testBtn);
                    if (subData[1] == null)
                        break;
                    ++subIdx;
                }
                else if (subIdx == 1 && testBtn.Tag == subData[1])
                {
                    result.Add(testBtn);
                    break;
                }
            }
            return result.ToArray();
        }

        private void BtnNodeIsBorderedChangedHandle(BtnNode btn)
        {
            btn.IsBorderedChanged += (iBtn) =>
            {
                ReFreshPathBtns();
                PathBtn_Click();
            };
        }
        #endregion


        #region gen arrows

        private static double arrowLineWidth = 5, arrowLineStartSpace = 30, arrowLineEndSpace = 30, arrowLineEndArrowWidth = 3.5;

        private void GenArrows_inLevel(StackPanel spLevelPanel, out List<BtnNode> leafNodes)
        {
            leafNodes = new List<BtnNode>();
            List<BtnNode> slNodes;
            for (int i = 0, iv = spLevelPanel.Children.Count; i < iv; ++i)
            {
                GenArrows_inBlock((Grid)spLevelPanel.Children[i], out slNodes);
                leafNodes.AddRange(slNodes);
            }
        }
        private void GenArrows_inBlock(Grid grid, out List<BtnNode> leafNodes)
        {
            Canvas canvasBlock = (Canvas)grid.Children[0];
            List<BtnNode> pendinglBtns = new List<BtnNode>();
            leafNodes = new List<BtnNode>();

            int i, iv, i2;
            for (i = 1, iv = grid.Children.Count; i < iv; ++i)
                pendinglBtns.Add((BtnNode)grid.Children[i]);

            BtnNode curBtn, subBtn;
            BtnNode[]? subBtns;
            Point posiCurBtn, posiSubBtn;
            Point curBtnCenter, oPoint = new Point(0, 0);
            ArrowLine arrowLine;
            while (pendinglBtns.Count > 0)
            {
                // 逐个按钮添加箭头，
                curBtn = pendinglBtns[0];
                subBtns = FindSubBtns(grid, curBtn);
                if (subBtns == null || subBtns.Length == 0)
                {
                    leafNodes.Add(curBtn);
                    pendinglBtns.RemoveAt(0);
                    continue;
                }

                posiCurBtn = curBtn.TranslatePoint(oPoint, grid);
                curBtnCenter = new Point(posiCurBtn.X + curBtn.ActualWidth / 2, posiCurBtn.Y + curBtn.ActualHeight / 2);
                i2 = 0;
                for (i = 0, iv = subBtns.Length; i < iv; ++i)
                {
                    subBtn = subBtns[i];
                    if (subBtn == null)
                        continue;

                    posiSubBtn = subBtn.TranslatePoint(oPoint, grid);

                    arrowLine = new ArrowLine()
                    { btnStart = curBtn, btnEnd = subBtn, };
                    arrowLine.SetBasicProperties(arrowLineWidth, arrowLineStartSpace, arrowLineEndSpace, false, true, 0, arrowLineEndArrowWidth);
                    arrowLine.SetBrush(ArrowLine.arrowLineBrushInactive);
                    Canvas.SetLeft(arrowLine, curBtnCenter.X);
                    Canvas.SetTop(arrowLine, curBtnCenter.Y);

                    canvasBlock.Children.Add(arrowLine);
                    curBtn.arrowLinesTo.Add(arrowLine);
                    subBtn.arrowLinesFrom.Add(arrowLine);

                    arrowLine.ReDraw(new Point(posiSubBtn.X - posiCurBtn.X, posiSubBtn.Y - posiCurBtn.Y));
                }


                // 处理好的按钮，排出待处理清单
                pendinglBtns.RemoveAt(0);
            }
        }
        private void GenArrows_leafNodes(List<BtnNode> leafNodes)
        {
            canvasBtm.Width = spStoryLevels.ActualWidth;
            BtnNode curBtn, nextBtn;
            Grid curBtnParent, nextBtnParent;
            ArrowLine arrowLine;
            Point oPoint = new Point(0, 0), curBtnCenter, curBtnPosi, nextBtnPosi;
            for (int i = 0, iv = leafNodes.Count; i < iv; ++i)
            {
                curBtn = leafNodes[i];
                nextBtn = FindNextBtnNode(curBtn, out nextBtnParent);
                if (nextBtn != null)
                {
                    curBtnParent = (Grid)curBtn.Parent;
                    curBtnPosi = curBtn.TranslatePoint(oPoint, (StackPanel)((StackPanel)curBtnParent.Parent).Parent);
                    curBtnCenter = new Point(curBtnPosi.X + curBtn.ActualWidth / 2, curBtnPosi.Y + curBtn.ActualHeight / 2);
                    nextBtnPosi = nextBtn.TranslatePoint(oPoint, (StackPanel)((StackPanel)nextBtnParent.Parent).Parent);

                    arrowLine = new ArrowLine()
                    { btnStart = curBtn, btnEnd = nextBtn, };
                    arrowLine.SetBasicProperties(arrowLineWidth, arrowLineStartSpace, arrowLineEndSpace, false, true, 0, arrowLineEndArrowWidth);
                    arrowLine.SetBrush(ArrowLine.arrowLineBrushInactive);
                    Canvas.SetLeft(arrowLine, curBtnCenter.X);
                    Canvas.SetTop(arrowLine, curBtnCenter.Y);

                    canvasBtm.Children.Add(arrowLine);
                    curBtn.arrowLinesTo.Add(arrowLine);
                    nextBtn.arrowLinesFrom.Add(arrowLine);

                    arrowLine.ReDraw(new Point(nextBtnPosi.X - curBtnPosi.X, nextBtnPosi.Y - curBtnPosi.Y));
                }
            }
        }
        private BtnNode FindNextBtnNode(BtnNode curBtn, out Grid nextNodeParent)
        {
            nextNodeParent = null;
            if (curBtn.Tag is StoryNode)
            {
                Grid grid = (Grid)curBtn.Parent;
                StackPanel levelPanel = (StackPanel)grid.Parent;
                StackPanel mainPanel = (StackPanel)levelPanel.Parent;
                BtnNode testBtn;

                StoryNode storyLink = (StoryNode)curBtn.Tag;
                for (int lvIndex = mainPanel.Children.IndexOf(levelPanel), lvIndexMax = mainPanel.Children.Count;
                    lvIndex < lvIndexMax; ++lvIndex)
                {
                    levelPanel = (StackPanel)mainPanel.Children[lvIndex];
                    foreach (Grid iGrid in levelPanel.Children)
                    {
                        testBtn = (BtnNode)iGrid.Children[1];
                        if (((StoryNode)testBtn.Tag).name == storyLink.name)
                        {
                            nextNodeParent = iGrid;
                            return testBtn;
                        }
                    }
                }
            }
            return null;

        }
        #endregion


        #region user interact

        private BtnNode BtnNodeLeftClicked_btn, BtnNodeRightClicked_btn, BtnNodeClicked_pre;
        private DateTime BtnNodeLeftClicked_lastTime = DateTime.MinValue;
        private void BtnNodeClicked(BtnNode btn, bool isLeftOrRight)
        {
            if (isLeftOrRight)
            {
                DateTime now = DateTime.Now;
                if (BtnNodeClicked_pre == btn)
                {
                    if ((now - BtnNodeLeftClicked_lastTime).TotalMilliseconds <= DoubleClickInterval.ValueMs)
                    {
                        // double clicked
                        btn.IsBordered = !btn.IsBordered;
                        BtnNodeLeftClicked_lastTime = DateTime.MinValue;
                    }
                    else
                    {
                        BtnNodeLeftClicked_lastTime = DateTime.Now;
                    }
                }
                else // if(BtnNodeLeftClicked_btn != btn)
                {
                    BtnNodeLeftClicked_btn = btn;
                    // single clicked
                    // 显示故事
                    ShowChatHistory();
                    // 生成所有可用路径， 默认选中第一个
                    ReFindPathsNSelectOne();
                    BtnNodeLeftClicked_lastTime = DateTime.Now;
                }
            }
            else
            {
                // right mouse btn clicked
                BtnNodeRightClicked_btn = btn;
                BtnNodeLeftClicked_lastTime = DateTime.MinValue;

                // do nothing here

            }
            BtnNodeClicked_pre = btn;

            svChat.Focus();
        }


        private object ShowChatHistory_lastSelectedNodeData = null;
        private void ShowChatHistory()
        {
            //Dispatcher.Invoke(() =>
            //{
            if (BtnNodeLeftClicked_btn == null)
            {
                tbChatTitle.Visibility = Visibility.Collapsed;
                btnViewEnding.Visibility = Visibility.Collapsed;
                spChat.Children.Clear();
                return;
            }

            ChatHistory ch = null;
            if (BtnNodeLeftClicked_btn.Tag is StoryNode)
            {
                StoryNode curStoryNode = (StoryNode)BtnNodeLeftClicked_btn.Tag;
                if (curStoryNode == ShowChatHistory_lastSelectedNodeData)
                    return;
                if (Core.localizeDict.ContainsKey("story"))
                    tbChatTitle.Text = $"{Core.localizeDict["story"]}: " + curStoryNode.name;
                else
                    tbChatTitle.Text = $"Story: " + curStoryNode.name;
                tbChatTitle.Visibility = Visibility.Visible;
                btnViewEnding.Visibility = Visibility.Collapsed;
                spChat.Children.Clear();
                ch = curStoryNode.chatHistory;
                ShowChatHistory_lastSelectedNodeData = curStoryNode;
            }
            else if (BtnNodeLeftClicked_btn.Tag is ChoiceNode)
            {
                ChoiceNode curChoiceNode = (ChoiceNode)BtnNodeLeftClicked_btn.Tag;
                if (curChoiceNode == ShowChatHistory_lastSelectedNodeData)
                    return;
                if (Core.localizeDict.ContainsKey("choice"))
                    tbChatTitle.Text = $"{Core.localizeDict["choice"]} {curChoiceNode.name}:{Environment.NewLine}{curChoiceNode.text}";
                else
                    tbChatTitle.Text = $"Choice {curChoiceNode.name}:{Environment.NewLine}{curChoiceNode.text}";
                tbChatTitle.Visibility = Visibility.Visible;
                btnViewEnding.Visibility = Visibility.Collapsed;
                spChat.Children.Clear();
                ch = curChoiceNode.chatHistory;
                ShowChatHistory_lastSelectedNodeData = curChoiceNode;
            }
            else if (BtnNodeLeftClicked_btn.Tag is EndingNode)
            {
                tbChatTitle.Visibility = Visibility.Collapsed;
                btnViewEnding.Visibility = Visibility.Visible;
                spChat.Children.Clear();
                ShowChatHistory_lastSelectedNodeData = (EndingNode)BtnNodeLeftClicked_btn.Tag;
            }

            if (ch != null)
            {
                ChatMsgItem cmi;
                ChatHistory.ChatMsg cm;
                for (int i = 0, iv = ch.chatMsgs.Count; i < iv; ++i)
                {
                    cm = ch.chatMsgs[i];
                    cmi = new ChatMsgItem();
                    if (cm.fromSucOrPlayer)
                    {
                        cmi.SetLeft(curSuccuNode.succuIcon, curSuccuNode.succuNameLocalized, cm.msg);
                    }
                    else
                    {
                        if (Core.localizeDict.ContainsKey("Jake"))
                            cmi.SetRight(Core.imageDict["IconJake"], Core.localizeDict["Jake"], cm.msg);
                        else
                            cmi.SetRight(Core.imageDict["IconJake"], "Jake", cm.msg);
                    }
                    if (!string.IsNullOrEmpty(cm.args))
                    {
                        string[] argParts = cm.args.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string a in argParts)
                        {
                            if (a == "fontSize=Small1")
                            {
                                cmi.tbMsg.FontSize *= 0.6;
                            }
                            else if (a == "fontSize=Big1")
                            {
                                cmi.tbMsg.FontSize *= 1.8;
                            }
                            else if (a == "noIcon")
                            {
                                cmi.imgLeft.Source
                                    = cmi.imgRight.Source
                                    = null;
                            }
                            else if (a == "noCharName")
                            {
                                cmi.tbChar.Text = "";
                            }
                        }
                    }
                    spChat.Children.Add(cmi);
                }
            }
            //});
        }

        private void btnViewEnding_Click(object sender, RoutedEventArgs e)
        {
            if (ShowChatHistory_lastSelectedNodeData is EndingNode)
            {
                WindowEndingPlayer win = new WindowEndingPlayer()
                {
                    Owner = Core.mainWindow,
                    endingData = (EndingNode)ShowChatHistory_lastSelectedNodeData,
                };
                win.Show();
            }
        }

        private Thickness pathBtnPadding = new Thickness(3);
        public static SolidColorBrush pathBtn_backgroundActive = new SolidColorBrush(Colors.Lime);
        private void ReFindPathsNSelectOne()
        {
            Dispatcher.Invoke(() =>
            {
                foreach (Button btn in spPathList.Children)
                {
                    btn.Click -= PathBtn_Click;
                }
                spPathList.Children.Clear();
                SetAllArrowLinesDark();
                if (BtnNodeLeftClicked_btn == null)
                    return;

                List<StoryPath> allPath = new List<StoryPath>();
                allPath.Add(new StoryPath(BtnNodeLeftClicked_btn));
                ReFindPathsNSelectOneAsync_loop(ref allPath);

                allPath.Sort((a, b) => (a.ActiveNodeCount * 1000 + a.InactiveNodeCount) - (b.ActiveNodeCount * 1000 + b.InactiveNodeCount));

                StoryPath sp;
                Button newBtn;
                string newBtnTx;
                for (int i = 0, iv = allPath.Count; i < iv; ++i)
                {
                    // A-#-# (green)
                    // lv1 story no, locked nodes, normal nodes
                    // A-# (normal)
                    //             , all locked
                    //spPathList.Children.Add();
                    sp = allPath[i];
                    newBtn = new Button()
                    {
                        Tag = sp,
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Padding = pathBtnPadding,
                        Margin = pathBtnPadding,
                    };
                    SetPathBtn(newBtn, sp);
                    newBtn.Click += PathBtn_Click;
                    spPathList.Children.Add(newBtn);
                }

                if (spPathList.Children.Count > 0)
                    PathBtn_Click(spPathList.Children[0], null);
            });
        }
        private void ReFreshPathBtns()
        {
            StoryPath sp;
            foreach (Button btn in spPathList.Children)
            {
                if (btn.Tag is StoryPath)
                {
                    sp = (StoryPath)btn.Tag;
                    sp.ReCount();
                    SetPathBtn(btn, sp);
                }
            }
        }
        private void PathBtn_Click()
        {
            if (PathBtn_preClicked != null)
                PathBtn_Click(PathBtn_preClicked, null);
        }
        private Button PathBtn_preClicked = null;
        private void PathBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                Button btn = (Button)sender;
                PathBtn_preClicked = btn;
                if (btn.Tag is StoryPath)
                {
                    StoryPath sp = (StoryPath)btn.Tag;
                    SetAllArrowLinesDark();
                    BtnNode btnCur = null, btnPre = sp.btnList[0];
                    for (int i = 1, iv = sp.btnList.Count; i < iv; ++i)
                    {
                        btnCur = sp.btnList[i];
                        foreach (ArrowLine al in btnCur.arrowLinesFrom)
                        {
                            if (al.btnStart == btnPre)
                            {
                                if (btnCur.IsBordered || btnPre.IsBordered)
                                    al.ArrowLineStyle = ArrowLine.ArrowLineStyles.Inactive;
                                else
                                    al.ArrowLineStyle = ArrowLine.ArrowLineStyles.Active;
                                break;
                            }
                        }

                        btnPre = btnCur;
                    }
                    if (btnCur != null)
                    {
                        if (btnCur.isStoryLink)
                        {
                            ArrowLine al = btnCur.arrowLinesTo[0];
                            al.ArrowLineStyle = ArrowLine.ArrowLineStyles.Next;
                            btnCur = al.btnEnd;
                        }
                        foreach (ArrowLine al in btnCur.arrowLinesTo)
                        {
                            al.ArrowLineStyle = ArrowLine.ArrowLineStyles.Next;
                        }
                    }
                    else if (btnPre != null)
                    {
                        foreach (ArrowLine al in btnPre.arrowLinesTo)
                        {
                            al.ArrowLineStyle = ArrowLine.ArrowLineStyles.Next;
                        }
                    }
                }
            }
        }

        private static void SetPathBtn(Button btn, StoryPath sp)
        {
            StringBuilder strBdr = new StringBuilder();
            string tmp = ((StoryNode)sp.btnList[0].Tag).name;
            strBdr.Append(BtnNode.GetSymNo(tmp.Substring(tmp.IndexOf('-'))));
            strBdr.Append("-");

            if (sp.ActiveNodeCount == 0)
            {
                strBdr.Append(sp.InactiveNodeCount);
            }
            else
            {
                strBdr.Append(sp.ActiveNodeCount);
                strBdr.Append("-");
                strBdr.Append(sp.InactiveNodeCount);
            }
            btn.Content = strBdr.ToString();

            if (sp.InactiveNodeCount == 0)
                btn.Background = pathBtn_backgroundActive;
            else
                btn.Background = SystemColors.ControlBrush;
        }
        private void ReFindPathsNSelectOneAsync_loop(ref List<StoryPath> fullList)
        {
            StoryPath sp, newSp;
            BtnNode curBtn, testBtn;
            StoryNode testStoryNode;
            bool notFound = true;
            for (int i = fullList.Count - 1; i >= 0; --i)
            {
                sp = fullList[i];
                if (sp.isLocked)
                    continue;
                curBtn = sp.btnList[0];
                if (curBtn.arrowLinesFrom.Count == 0)
                    continue;

                for (int j = 1, jv = curBtn.arrowLinesFrom.Count; j < jv; ++j)
                {
                    newSp = new StoryPath(sp);
                    newSp.InsertToStart(curBtn.arrowLinesFrom[j].btnStart);
                    notFound = false;
                    fullList.Add(newSp);
                }
                testBtn = curBtn.arrowLinesFrom[0].btnStart;
                sp.InsertToStart(testBtn);
                notFound = false;
                if (curBtn.arrowLinesFrom.Count == 1
                    && testBtn.arrowLinesFrom.Count > 0
                    && !testBtn.isStoryLink
                    && testBtn.Tag is StoryNode)
                {
                    testStoryNode = (StoryNode)curBtn.arrowLinesFrom[0].btnStart.Tag;
                    if (testStoryNode.name.StartsWith("I-"))
                    {
                        sp.isLocked = true;
                        newSp = new StoryPath(sp);
                        fullList.Add(newSp);
                    }
                }
            }
            if (notFound)
                return;

            ReFindPathsNSelectOneAsync_loop(ref fullList);
        }
        private void SetAllArrowLinesDark()
        {
            if (spStoryLevels.Children.Count == 0)
                return;

            StackPanel spLevel = (StackPanel)spStoryLevels.Children[0];
            foreach (Grid grid in spLevel.Children)
            {
                SetAllArrowLinesGray_loop((BtnNode)grid.Children[1]);
            }
        }
        private void SetAllArrowLinesGray_loop(BtnNode curBtn)
        {
            foreach (ArrowLine a in curBtn.arrowLinesTo)
            {
                a.ArrowLineStyle = ArrowLine.ArrowLineStyles.Dark;
                SetAllArrowLinesGray_loop(a.btnEnd);
            }
        }
        private class StoryPath
        {
            public int ActiveNodeCount { private set; get; } = 0;
            public int InactiveNodeCount { private set; get; } = 0;

            public List<BtnNode> btnList = new List<BtnNode>();

            public StoryPath(BtnNode endBtn)
            {
                btnList.Add(endBtn);
            }
            public StoryPath(StoryPath copyFrom)
            {
                for (int i = copyFrom.btnList.Count - 1; i >= 0; --i)
                {
                    InsertToStart(copyFrom.btnList[i]);
                }
            }

            internal bool isLocked = false;
            public void InsertToStart(BtnNode btn)
            {
                btnList.Insert(0, btn);
                if (btn.isStoryLink)
                    return;

                if (btn.IsBordered)
                    ++InactiveNodeCount;
                else
                    ++ActiveNodeCount;
            }
            public bool ReCount()
            {
                BtnNode bn;
                int a = 0, ia = 0;
                for (int i = 0, iv = btnList.Count - 1; i < iv; ++i)
                {
                    bn = btnList[i];
                    if (bn.isStoryLink)
                        continue;
                    if (bn.IsBordered)
                        ++ia;
                    else
                        ++a;
                }
                if (ia != InactiveNodeCount || a != ActiveNodeCount)
                {
                    ActiveNodeCount = a;
                    InactiveNodeCount = ia;
                    return true;
                }
                return false;
            }
        }

        #endregion


    }
}
