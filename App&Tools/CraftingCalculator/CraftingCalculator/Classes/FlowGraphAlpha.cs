using MadTomDev.App.Ctrls;
using MadTomDev.Common;
using MadTomDev.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;


using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;



namespace MadTomDev.App.Classes
{
    /// <summary>
    /// 2024 0614 by MadTom
    /// 专为合成计算器设计的可视化类，配合SearchResult使用;
    /// 图形容器为canvas，根据内容，会自动改变canvas的尺寸；
    /// </summary>
    public class FlowGraphAlpha
    {
        public class BasicConfig
        {
            public double marginLeft = 30;
            public double marginTop = 30;
            public double marginRight = 30;
            public double marginBottom = 30;

            public double IOPanelMinWidth = 200;
            public double IOItemWidth = 200;
            public double NodeSpacingHor = 700;
            public double NodeSpacingVer = 400;

            public SolidColorBrush PanelBrushAccessory = Brushes.Magenta;
            public SolidColorBrush PanelBrushInput = Brushes.Orange;
            public SolidColorBrush PanelBrushOutput = Brushes.Aqua;
            public SolidColorBrush PanelBrushProcess = Brushes.Gold;

            public double lineDotDiameter = 16;
            public double lineMinThickness = 2;
            public double lineMaxThickness = 12;
            public double lineMinThicknessScaleValue = 0.1;
            public double lineMaxThicknessScaleValue = 10;
        }
        public Canvas mainPanel;
        ProcessingChains.SearchResult searchResult;
        private BasicConfig basicConfig;
        private IOPanel inputPanel, outputPanel;
        private List<ProcessPanel> processPanelList = new List<ProcessPanel>();
        private List<LinkLine> linkLineList = new List<LinkLine>();

        private int processMaxLevel;
        public FlowGraphAlpha(ref Canvas panel, ProcessingChains.SearchResult searchResult, BasicConfig? basicConfig = null)
        {
            mainPanel = panel;
            this.searchResult = searchResult;
            if (basicConfig == null)
            {
                this.basicConfig = new BasicConfig();
            }
            else
            {
                this.basicConfig = basicConfig;
            }

            // 绘制 原料面板
            inputPanel = new IOPanel(this)
            {
                Background = this.basicConfig.PanelBrushInput,
                Title = "Inputs",
                HeadList = searchResult.allSources,
            };
            SetPanelPosition(inputPanel, 0, 0);

            // 在右侧1000位置绘制 产品面板
            outputPanel = new IOPanel(this)
            {
                Background = this.basicConfig.PanelBrushOutput,
                Title = "Products",
                HeadList = searchResult.allFinalProducts,
            };
            SetPanelPosition(outputPanel, 1000, 0);


            // 绘制所有 处理节点
            ProcessPanel newPPanel;
            foreach (ProcessingChains.ProcessingNode pNode in searchResult.allProcesses)
            {
                newPPanel = new ProcessPanel(this, pNode);
                processPanelList.Add(newPPanel);
                SetPanelPosition(newPPanel, 500, 0);
                //mainPanel.Children.Add(newPPanel);
                newPPanel.ReSetIOLayout();
            }




            // 输入、输出面板，放在所有其他节点图形的前面
            mainPanel.Children.Add(inputPanel);
            mainPanel.Children.Add(outputPanel);


            #region 按节点层级，排列 处理面板

            // 层级计数器，用于确定每个面板纵向位置
            Dictionary<int, int> lvCounter = new Dictionary<int, int>();
            processMaxLevel = int.MinValue;
            foreach (ProcessingChains.ProcessingNode pNode in searchResult.allProcesses)
            {
                if (processMaxLevel < pNode.ProcessLevel)
                {
                    processMaxLevel = pNode.ProcessLevel;
                }
            }
            int i;
            for (i = 0; i <= processMaxLevel; i++)
            {
                lvCounter.Add(i, 0);
            }

            // 将输出面板放在应有的最右边
            SetPanelPosition(outputPanel, this.basicConfig.NodeSpacingHor * (processMaxLevel + 2), 0);


            // 排列处理面板
            int lv;
            foreach (ProcessPanel pp in processPanelList)
            {
                lv = pp.processNode.ProcessLevel;
                SetPanelPosition(
                    pp,
                    this.basicConfig.NodeSpacingHor * (processMaxLevel - lv + 1),
                    this.basicConfig.NodeSpacingVer * lvCounter[lv]++);

            }

            #endregion

            // 连线   -排版之后划线；

            // 手动调整位置、大小
            inputPanel.TitleMouseDown += Panel_TitleMouseDown;
            outputPanel.TitleMouseDown += Panel_TitleMouseDown;
            inputPanel.ResizeHandleMouseDown += Panel_ResizeHandleMouseDown;
            outputPanel.ResizeHandleMouseDown += Panel_ResizeHandleMouseDown;
            foreach (ProcessPanel pPanel in processPanelList)
            {
                pPanel.TitleMouseDown += Panel_TitleMouseDown;
                pPanel.ResizeHandleMouseDown += Panel_ResizeHandleMouseDown;
            }



            // 连线
            LinkLine newLine;
            foreach (ProcessingChains.ProcessingLink link in searchResult.allLinks)
            {
                newLine = new LinkLine(this, link);
                linkLineList.Add(newLine);
                mainPanel.Children.Add(newLine);
            }

            // 自动排版一次，设置连线
            SetAutoMargin();

            // 使用碰撞检测，将每列的面板向下移动，避开前工序和后工序之间的连线；
            // 例如电路使用 源-原木 和 太阳能-发电 来运行，那么太阳能应该避开 源-电炉 之间的连线；


            AutoLayout();
        }
        internal async void AutoLayout(bool loopToNoCollision = true)
        {
            await Task.Delay(50);
            bool layoutChanged = false;
            int lv, i, iv = processPanelList.Count,
                j, jv = linkLineList.Count,
                k;
            ProcessPanel pPanel;
            LinkLine linkLine;
            System.Drawing.RectangleF bondPPanel;
            for (lv = 0; lv <= processMaxLevel; ++lv)
            {
                // 从当前层级的面板，按顺序遍历，看是否出现了被线压的情况
                for (i = 0; i < iv; ++i)
                {
                    pPanel = processPanelList[i];
                    if (pPanel.processNode.ProcessLevel != lv)
                    {
                        continue;
                    }

                    bondPPanel = pPanel.GetBoundsRect();

                    // 如果有，则将当前及下面的面板，全部向下移动；
                    // 其中，线必须是 上级 发出，到往下一级（不是本级）
                    for (j = 0; j < jv; ++j)
                    {
                        linkLine = linkLineList[j];
                        if (linkLine.link.nodePrev is ProcessingChains.ProcessingNode
                            && ((ProcessingChains.ProcessingNode)linkLine.link.nodePrev).ProcessLevel >= lv)
                        {
                            continue;
                        }

                        if (QuickCheckNCalculate.CheckRectangleIntersection(bondPPanel, linkLine.GetBoundsRect()))
                        {
                            // 发生重叠，向下移动
                            for (k = i; k < iv; ++k)
                            {
                                pPanel = processPanelList[k];
                                if (pPanel.processNode.ProcessLevel != lv)
                                {
                                    break;
                                }

                                MovePanel(pPanel, null, basicConfig.NodeSpacingVer);
                                layoutChanged = true;
                            }
                        }
                    }
                }
            }
            if (layoutChanged)
            {
                this.SetAutoMargin();
                if (loopToNoCollision)
                {
                    AutoLayout(loopToNoCollision);
                }
            }
        }



        public event Action<FlowGraphAlpha, MovablePanel> PanelTitleMouseDown;
        public event Action<FlowGraphAlpha, MovablePanel> PanelResizeHandleMouseDown;
        private void Panel_TitleMouseDown(MovablePanel sender)
        {
            PanelTitleMouseDown?.Invoke(this, sender);
        }
        private void Panel_ResizeHandleMouseDown(MovablePanel sender)
        {
            PanelResizeHandleMouseDown?.Invoke(this, sender);
        }






        public static void SetPanelPosition(MovablePanel panel, double? x, double? y, bool byCenter = false)
        {
            if (x != null)
            {
                Canvas.SetLeft(panel, (double)x - (byCenter ? panel.Width / 2 : 0));
            }
            if (y != null)
            {
                Canvas.SetTop(panel, (double)y - (byCenter ? panel.Height / 2 : 0));
            }
        }
        public static void SetPanelPosition(ThingWithLabel panel, double? x, double? y, bool byCenter = false)
        {
            if (x != null && x is not double.NaN)
            {
                Canvas.SetLeft(panel, (double)x - (byCenter ? panel.Width / 2 : 0));
            }
            if (y != null && y is not double.NaN)
            {
                Canvas.SetTop(panel, (double)y - (byCenter ? panel.Height / 2 : 0));
            }
        }
        public static Point GetPanelPosition(FlowGraphAlpha fga, ThingWithLabel panel)
        {
            return new Point(Canvas.GetLeft(panel), Canvas.GetTop(panel));
        }

        public bool IsRendering
        {
            get
            {
                if (inputPanel.IsRendering
                    || outputPanel.IsRendering)
                {
                    return true;
                }

                foreach (ProcessPanel pPanel in processPanelList)
                {
                    if (pPanel.IsRendering)
                    {
                        return true;
                    }
                }
                foreach (LinkLine line in linkLineList)
                {
                    if (line.IsRendering)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public async void SetAutoMargin(bool waitRender = true)
        {
            if (waitRender)
            {
                await Task.Delay(20);
                while (IsRendering)
                {
                    await Task.Delay(20);
                }
            }

            double minLeft = double.MaxValue, minTop = double.MaxValue,
                maxRight = double.MinValue, maxBottom = double.MinValue;
            double testD;
            SetValue(inputPanel);
            SetValue(outputPanel);
            foreach (IPositionNSize i in processPanelList)
            {
                SetValue(i);
            }
            testD = basicConfig.marginLeft - minLeft;
            if (testD != 0)
            {
                MoveAllPanels(testD, null);
            }
            mainPanel.Width = maxRight - minLeft + basicConfig.marginLeft + basicConfig.marginRight;
            testD = basicConfig.marginTop - minTop;
            if (testD != 0)
            {
                MoveAllPanels(null, testD);
            }
            mainPanel.Height = maxBottom - minTop + basicConfig.marginTop + basicConfig.marginBottom;
            foreach (ProcessPanel pPanel in processPanelList)
            {
                pPanel.ReSetIOLayout();
            }
            UpdateAllLinkLines();

            void SetValue(IPositionNSize ui)
            {
                if (minLeft > ui.X) minLeft = ui.X;
                if (minTop > ui.Y) minTop = ui.Y;
                testD = ui.X + ui.Width;
                if (maxRight < testD) maxRight = testD;
                testD = ui.Y + ui.Height;
                if (maxBottom < testD) maxBottom = testD;
            }
        }
        public async void UpdateAllLinkLines(bool waitRender = true)
        {
            if (waitRender)
            {
                await Task.Delay(20);
                while (IsRendering)
                {
                    await Task.Delay(10);
                }
            }
            foreach (LinkLine line in linkLineList)
            {
                line.Update();
            }
        }

        public static void MovePanel(MovablePanel panel, double? offsetX, double? offsetY)
        {
            SetPanelPosition(panel, panel.X + offsetX, panel.Y + offsetY);
        }
        private void MoveAllPanels(double? offsetX, double? offsetY)
        {
            SetPanelPosition(inputPanel, inputPanel.X + offsetX, inputPanel.Y + offsetY);
            SetPanelPosition(outputPanel, outputPanel.X + offsetX, outputPanel.Y + offsetY);
            foreach (ProcessPanel pPanel in processPanelList)
            {
                SetPanelPosition(pPanel, pPanel.X + offsetX, pPanel.Y + offsetY);
            }
            foreach (LinkLine line in linkLineList)
            {
                if (offsetX != null)
                {
                    line.X += (double)offsetX;
                }
                if (offsetY != null)
                {
                    line.Y += (double)offsetY;
                }
            }
        }

        public static void ResizePanel(MovablePanel targetPanel, double offsetX, double offsetY)
        {
            targetPanel.Width = targetPanel.ActualWidth + offsetX;
            targetPanel.Height = targetPanel.ActualHeight + offsetY;
        }

        internal void UpdateQuantities()
        {
            inputPanel.UpdateQuantities();
            outputPanel.UpdateQuantities();
            foreach (ProcessPanel pPanel in processPanelList)
            {
                pPanel.UpdateQuantities();
            }
            foreach (LinkLine line in linkLineList)
            {
                line.UpdateQuantities();
            }
        }

        internal interface IPositionNSize
        {
            double X { set; get; }
            double Y { set; get; }
            double CenterX { set; get; }
            double CenterY { set; get; }
            double Width { set; get; }
            double Height { set; get; }

            System.Drawing.RectangleF GetBoundsRect();
        }
        public class IOPanel : MovablePanel, IPositionNSize
        {
            FlowGraphAlpha parent;
            public IOPanel(FlowGraphAlpha parent)
            {
                this.parent = parent;
                MinWidth = parent.basicConfig.IOPanelMinWidth;
                MinHeight = 30;
            }
            private List<ProcessingChains.ProcessingHead> _HeadList;
            public List<ProcessingChains.ProcessingHead> HeadList
            {
                get => _HeadList;
                set
                {
                    _HeadList = value;
                    ProcessingChains.ProcessingHead head;
                    ThingWithLabel headUI;
                    for (int i = 0, iv = value.Count; i < iv; ++i)
                    {
                        head = value[i];
                        headUI = new ThingWithLabel()
                        {
                            Margin = new Thickness(-1, -1, -1, 0),
                            IsCheckable = false,
                            ThingBase = head.thing,
                            ToolTip = head.thing.name + Environment.NewLine + head.thing.description,
                            TxLabel1 = "",
                            TxLabel2 = GetSpeedTxLabel(head),
                            TxLabel3 = "/s" + Environment.NewLine + "/m" + Environment.NewLine + "/h",
                        };
                        sp_main.Children.Add(headUI);
                    }
                }
            }
            public void UpdateQuantities()
            {
                if (_HeadList == null || _HeadList.Count == 0)
                {
                    return;
                }
                ProcessingChains.ProcessingHead head;
                ThingWithLabel ui;
                for (int i = 0, iv = _HeadList.Count; i < iv; ++i)
                {
                    head = _HeadList[i];
                    ui = (ThingWithLabel)sp_main.Children[i];
                    ui.TxLabel2 = GetSpeedTxLabel(head);
                }
            }
            public int IndexOfHead(ProcessingChains.ProcessingHead head)
            {
                return _HeadList.IndexOf(head);
            }

            #region position x, y, centerX, centerY, size, width, height
            public new double X
            {
                get => Canvas.GetLeft(this);
                set => Canvas.SetLeft(this, value);
            }
            public new double Y
            {
                get => Canvas.GetTop(this);
                set => Canvas.SetTop(this, value);
            }
            public double CenterX
            {
                get => Canvas.GetLeft(this) - ActualWidth / 2;
                set => Canvas.SetLeft(this, value - ActualWidth / 2);
            }
            public double CenterY
            {
                get => Canvas.GetTop(this) - ActualHeight / 2;
                set => Canvas.SetTop(this, value - ActualHeight / 2);
            }

            public new double Width
            {
                set => this.Width = value;
                get
                {
                    double baseWidth = ((FrameworkElement)this).Width;
                    if (double.IsNaN(baseWidth))
                    {
                        if (this.ActualWidth <= 0)
                        {
                            return this.MinWidth;
                        }
                        return this.ActualWidth;
                    }
                    return baseWidth;
                }
            }
            public new double Height
            {
                set => this.Height = value;
                get
                {
                    double baseHeight = ((FrameworkElement)this).Height;
                    if (double.IsNaN(baseHeight))
                    {
                        if (this.ActualHeight <= 0)
                        {
                            return this.MinHeight;
                        }
                        return this.ActualHeight;
                    }
                    return baseHeight;
                }
            }
            #endregion

            private string GetSpeedTxLabel(ProcessingChains.ProcessingHead pHead)
            {
                decimal v = (decimal)pHead.calSpeed;
                StringBuilder strBdr = new StringBuilder();
                strBdr.AppendLine(SimpleStringHelper.UnitsOfMeasure.GetShortString(v, ""));
                strBdr.AppendLine(SimpleStringHelper.UnitsOfMeasure.GetShortString(v * 60, ""));
                strBdr.Append(SimpleStringHelper.UnitsOfMeasure.GetShortString(v * 3600, ""));
                return strBdr.ToString();
            }

            public Point GetSubPanelPoint(int index, bool leftOrRight)
            {
                ThingWithLabel ui = (ThingWithLabel)sp_main.Children[index];
                Point pt = ui.TranslatePoint(new Point(), this);
                if (!leftOrRight)
                {
                    pt.X += ui.ActualWidth;
                }
                pt.Y += ui.ActualHeight / 2;
                pt.X += this.X;
                pt.Y += this.Y;
                return pt;
            }
            public bool IsRendering
            {
                get
                {
                    if (this.ActualWidth == 0)
                    {
                        return true;
                    }
                    if (sp_main.Children.Count > 0)
                    {
                        foreach (ThingWithLabel ui in sp_main.Children)
                        {
                            if (ui.ActualWidth == 0)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
            }

            public System.Drawing.RectangleF GetBoundsRect()
            {
                return new System.Drawing.RectangleF(
                    (float)X, (float)Y,
                    (float)Width, (float)Height
                    );
            }

        }
        public class ProcessPanel : MovablePanel, IPositionNSize
        {
            FlowGraphAlpha parent;
            public ProcessingChains.ProcessingNode processNode;
            private List<ThingWithLabel> inputPanelList = new List<ThingWithLabel> { };
            private List<ThingWithLabel> outputPanelList = new List<ThingWithLabel> { };

            private TextBlock tbvMachineCount;

            public ProcessPanel(FlowGraphAlpha parent, ProcessingChains.ProcessingNode pNode)
            {
                if (pNode.recipe == null)
                {
                    throw new ArgumentNullException($"Recipe of Progress is null");
                }
                Background = parent.basicConfig.PanelBrushProcess;
                this.parent = parent;
                this.processNode = pNode;

                #region 面板主体
                Title = pNode.recipe?.name;

                // add icon
                Border imgBdr = new Border()
                { Width = 64, Height = 64, };
                Image img = new Image()
                {
                    Stretch = Stretch.Uniform,
                    Source = ImageIO.GetOut(pNode.recipe),
                };
                RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
                imgBdr.Child = img;
                sp_main.Children.Add(imgBdr);
                // add machine count info
                tbvMachineCount = new TextBlock()
                {
                    Margin = new Thickness(6),
                    FontWeight = FontWeights.Bold,
                    FontSize = 24,
                    TextAlignment = TextAlignment.Center,
                };
                sp_main.Children.Add(tbvMachineCount);
                // add description
                if (pNode.recipe != null && !string.IsNullOrWhiteSpace(pNode.recipe.description))
                {
                    TextBlock tbvDescription = new TextBlock()
                    {
                        Text = pNode.recipe?.description,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(6),
                    };
                    sp_main.Children.Add(tbvDescription);
                }
                // add accessories
                if (pNode.recipe != null && pNode.recipe.accessories.Count > 0)
                {
                    StackPanel aPanel = new StackPanel()
                    {
                        Background = parent.basicConfig.PanelBrushAccessory,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                    };
                    TextBlock tbvATitle = new TextBlock()
                    {
                        FontWeight = FontWeights.Bold,
                        Text = "Accessories",
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 6, 0, 4),
                    };
                    aPanel.Children.Add(tbvATitle);


                    WrapPanel wp = new WrapPanel()
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    };
                    foreach (Recipes.Recipe.PIOItem a in pNode.recipe.accessories)
                    {
                        imgBdr = new Border()
                        {
                            Margin = new Thickness(2, 4, 2, 4),
                            Width = 42,
                            Height = 42,
                        };
                        img = new Image()
                        {
                            Stretch = Stretch.Uniform,
                            Source = ImageIO.GetOut(a.item),
                            ToolTip = a.item?.name + (a.item?.description == null ? "" : (Environment.NewLine + a.item?.description)),
                        };
                        RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
                        imgBdr.Child = img;
                        wp.Children.Add(imgBdr);
                    }
                    aPanel.Children.Add(wp);
                    sp_main.Children.Add(aPanel);
                }
                parent.mainPanel.Children.Add(this);
                #endregion


                #region 输入、输出面板

                ThingWithLabel newPanel;
                Recipes.Recipe.PIOItem ioItem;
                int i, iv;
                for (i = 0, iv = processNode.recipe.inputs.Count; i < iv; ++i)
                {
                    ioItem = processNode.recipe.inputs[i];
                    //if (ioItem == null)
                    //{
                    //    throw ProcessingChains.SearchHelper.Error_Recipe_Input_Item_isNull(i, processNode.recipe.name);
                    //}
                    newPanel = new ThingWithLabel()
                    {
                        IsCheckable = false,
                        Background = parent.basicConfig.PanelBrushInput,
                        Width = parent.basicConfig.IOItemWidth,
                        ThingBase = ioItem.item,
                        ToolTip = ioItem.item?.name + (ioItem.item?.description == null ? "" : (Environment.NewLine + ioItem.item?.description)),
                        TxLabel1 = "",
                        TxLabel3 = ThingWithLabel.TX_COMMON_SPEED_UNITS,
                    };
                    //FlowGraphAlpha.SetPanelPosition(newPanel, this.X, this.Y);
                    inputPanelList.Add(newPanel);
                    parent.mainPanel.Children.Add(newPanel);
                }
                for (i = 0, iv = processNode.recipe.outputs.Count; i < iv; ++i)
                {
                    ioItem = processNode.recipe.outputs[i];
                    newPanel = new ThingWithLabel()
                    {
                        IsCheckable = false,
                        Background = parent.basicConfig.PanelBrushOutput,
                        Width = parent.basicConfig.IOItemWidth,
                        ThingBase = ioItem.item,
                        ToolTip = ioItem.item?.name + (ioItem.item?.description == null ? "" : (Environment.NewLine + ioItem.item?.description)),
                        TxLabel1 = "",
                        TxLabel3 = ThingWithLabel.TX_COMMON_SPEED_UNITS,
                    };
                    //FlowGraphAlpha.SetPanelPosition(newPanel, this.X, this.Y);
                    outputPanelList.Add(newPanel);
                    parent.mainPanel.Children.Add(newPanel);
                }
                #endregion

                UpdateQuantities();
            }
            internal void UpdateQuantities()
            {
                if (processNode == null)
                {
                    return;
                }
                tbvMachineCount.Text = processNode.calMultiple.ToString("0.##") + Environment.NewLine
                    + "(" + Math.Ceiling(processNode.calMultiple) + ")";

                int i, iv;
                Recipes.Recipe.PIOItem ioItem;
                ThingWithLabel tPanel;
                if (processNode.recipe.period == null)
                {
                    throw new ArgumentNullException($"Period in recipe of Progress is null");
                }
                double period = (double)processNode.recipe.period;
                for (i = 0, iv = processNode.recipe.inputs.Count; i < iv; ++i)
                {
                    ioItem = processNode.recipe.inputs[i];
                    tPanel = inputPanelList[i];
                    if (ioItem.quantity == null)
                    {
                        throw ProcessingChains.SearchHelper.Error_Recipe_Input_Quantity_isNull(i, processNode.recipe.name);
                    }
                    tPanel.TxLabel2 = ThingWithLabel.GetCommonSpeed(ioItem.quantity.ValueCurrentInGeneral * processNode.calMultiple / period);

                }
                for (i = 0, iv = processNode.recipe.outputs.Count; i < iv; ++i)
                {
                    ioItem = processNode.recipe.outputs[i];
                    tPanel = outputPanelList[i];
                    if (ioItem.quantity == null)
                    {
                        throw ProcessingChains.SearchHelper.Error_Recipe_Output_Quantity_isNull(i, processNode.recipe.name);
                    }
                    tPanel.TxLabel2 = ThingWithLabel.GetCommonSpeed(ioItem.quantity.ValueCurrentInGeneral * processNode.calMultiple / period);
                }
            }


            #region position, x, y, centerX, centerY, size, width, height
            public new double X
            {
                get => Canvas.GetLeft(this);
                set => Canvas.SetLeft(this, value);
            }
            public new double Y
            {
                get => Canvas.GetTop(this);
                set => Canvas.SetTop(this, value);
            }
            public double CenterX
            {
                get => Canvas.GetLeft(this) - ActualWidth / 2;
                set => Canvas.SetLeft(this, value - ActualWidth / 2);
            }
            public double CenterY
            {
                get => Canvas.GetTop(this) - ActualHeight / 2;
                set => Canvas.SetTop(this, value - ActualHeight / 2);
            }

            public new double Width
            {
                set => this.Width = value;
                get
                {
                    double baseWidth = ((FrameworkElement)this).Width;
                    if (double.IsNaN(baseWidth))
                    {
                        if (this.ActualWidth <= 0)
                        {
                            return this.MinWidth;
                        }
                        return this.ActualWidth;
                    }
                    return baseWidth;
                }
            }
            public new double Height
            {
                set => this.Height = value;
                get
                {
                    double baseHeight = ((FrameworkElement)this).Height;
                    if (double.IsNaN(baseHeight))
                    {
                        if (this.ActualHeight <= 0)
                        {
                            return this.MinHeight;
                        }
                        return this.ActualHeight;
                    }
                    return baseHeight;
                }
            }
            #endregion


            public async void ReSetIOLayout(bool waitRender = true)
            {
                if (waitRender)
                {
                    while (IsRendering)
                    {
                        await Task.Delay(20);
                    }
                }

                // 主体面板必须高于最低侧面板的总高
                double totalIHeight = 0, totalOHeight = 0;
                foreach (ThingWithLabel t in inputPanelList)
                {
                    totalIHeight += t.ActualHeight;
                }
                foreach (ThingWithLabel t in outputPanelList)
                {
                    totalOHeight += t.ActualHeight;
                }
                double totalMaxHeigh = (totalIHeight > totalOHeight) ? totalIHeight : totalOHeight;
                this.MinHeight = totalMaxHeigh;
                double selfHeight;
                //if (!double.IsNaN(this.Width))
                //{
                //    selfHeight = this.Width;
                //}
                //else
                //{
                //    selfHeight = this.ActualHeight;
                //}
                selfHeight = this.Height;

                // 按两侧面板数量，布局
                double spacing = selfHeight / inputPanelList.Count;
                int i, iv;
                ThingWithLabel curPanel;
                double posiLeft = this.X - parent.basicConfig.IOItemWidth + 5;
                for (i = 0, iv = inputPanelList.Count; i < iv; ++i)
                {
                    curPanel = inputPanelList[i];
                    FlowGraphAlpha.SetPanelPosition(
                        curPanel,
                        posiLeft,
                        this.Y + spacing * i + spacing / 2 - curPanel.ActualHeight / 2);
                }
                spacing = selfHeight / outputPanelList.Count;
                posiLeft = this.X + this.ActualWidth - 5;
                for (i = 0, iv = outputPanelList.Count; i < iv; ++i)
                {
                    curPanel = outputPanelList[i];
                    FlowGraphAlpha.SetPanelPosition(
                        curPanel,
                        posiLeft,
                        this.Y + spacing * i + spacing / 2 - curPanel.ActualHeight / 2);
                }
            }

            public Point GetInputPanelPoint(int index)
            {
                ThingWithLabel ui = inputPanelList[index];
                Point pt = FlowGraphAlpha.GetPanelPosition(parent, ui);
                pt.Y += ui.ActualHeight / 2;
                return pt;
            }
            public Point GetOutputPanelPoint(int index)
            {
                ThingWithLabel ui = outputPanelList[index];
                Point pt = FlowGraphAlpha.GetPanelPosition(parent, ui);
                pt.X += ui.ActualWidth;
                pt.Y += ui.ActualHeight / 2;
                return pt;
            }

            public bool IsRendering
            {
                get
                {
                    if (this.ActualWidth <= 0)
                    {
                        return true;
                    }
                    foreach (ThingWithLabel t in inputPanelList)
                    {
                        if (t.ActualWidth <= 0)
                        {
                            return true;
                        }
                    }
                    foreach (ThingWithLabel t in outputPanelList)
                    {
                        if (t.ActualWidth <= 0)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }


            public System.Drawing.RectangleF GetBoundsRect()
            {
                float x, w;
                if (inputPanelList.Count > 0)
                {
                    x = (float)(FlowGraphAlpha.GetPanelPosition(parent, inputPanelList[0]).X);
                }
                else
                {
                    x = (float)X;
                }
                if (outputPanelList.Count > 0)
                {
                    w = (float)(FlowGraphAlpha.GetPanelPosition(parent, outputPanelList[0]).X + outputPanelList[0].Width) - x;
                }
                else
                {
                    w = (float)Width;
                }
                return new System.Drawing.RectangleF(
                    x, (float)Y,
                    w,
                    (float)Height
                    );
            }

        }

        public class LinkLine : Canvas, IPositionNSize
        {
            /// <summary>
            /// 画框的X值，左上点必定是内部坐标 (0,0)，可以是起点或终点
            /// </summary>
            public double X
            {
                get => Canvas.GetLeft(this);
                set => Canvas.SetLeft(this, value);
            }
            /// <summary>
            /// 画框的Y值，左上点必定是内部坐标 (0,0)，可以是起点或终点
            /// </summary>
            public double Y
            {
                get => Canvas.GetTop(this);
                set => Canvas.SetTop(this, value);
            }
            public double CenterX
            {
                get => X + (pointStart.X < pointEnd.X ? pointEnd.X / 2 : pointStart.X / 2);
                set => X += value - CenterX;
            }
            public double CenterY
            {
                get => Y + (pointStart.Y < pointEnd.Y ? pointEnd.Y / 2 : pointStart.Y / 2);
                set => Y += value - CenterY;

            }

            private FlowGraphAlpha parent;
            public ProcessingChains.ProcessingLink link;
            private IOPanel? startPanel = null;
            private ProcessPanel? startPPanel = null;
            private int outputIndex;
            private IOPanel? endPanel = null;
            private ProcessPanel? endPPanel = null;
            private int inputIndex;

            private SolidColorBrush bgLinkLabel = new SolidColorBrush(new Color() { A = 127, R = 0, G = 0, B = 0 });

            public LinkLine(FlowGraphAlpha parent, ProcessingChains.ProcessingLink link)
            {
                this.parent = parent;
                this.link = link;
                ProcessingChains.ProcessingHead pHead;
                ProcessingChains.ProcessingNode pNode;
                ProcessPanel? pPanel;
                if (link.nodePrev is ProcessingChains.ProcessingHead)
                {
                    pHead = (ProcessingChains.ProcessingHead)link.nodePrev;
                    this.startPanel = parent.inputPanel;
                    this.outputIndex = parent.inputPanel.IndexOfHead(pHead);
                }
                else
                {
                    pNode = (ProcessingChains.ProcessingNode)link.nodePrev;
                    pPanel = parent.processPanelList.Find(a => a.processNode == pNode);
                    if (pPanel == null)
                    {
                        throw new Exception($"Processing panel (with node[{pNode.recipe.name}]) not found.");
                    }
                    this.startPPanel = pPanel;
                    this.outputIndex = pNode.IndexOfOutput(link.thing);
                }

                if (link.nodeNext is ProcessingChains.ProcessingHead)
                {
                    pHead = (ProcessingChains.ProcessingHead)link.nodeNext;
                    this.endPanel = parent.outputPanel;
                    this.inputIndex = parent.outputPanel.IndexOfHead(pHead);
                }
                else
                {
                    pNode = (ProcessingChains.ProcessingNode)link.nodeNext;
                    pPanel = parent.processPanelList.Find(a => a.processNode == pNode);
                    if (pPanel == null)
                    {
                        throw new Exception($"Processing panel (with node[{pNode.recipe.name}]) not found.");
                    }
                    this.endPPanel = pPanel;
                    this.inputIndex = pNode.IndexOfInput(link.thing);
                }

                //Update();

                // shadow effect
                dropShadowEffect = new DropShadowEffect()
                {
                    ShadowDepth = 0,
                    Color = Colors.Transparent,
                    BlurRadius = 5,
                };
                this.Effect = dropShadowEffect;
                MouseLeave += (s1, e1) =>
                {
                    dropShadowEffect.Color = Colors.Transparent;
                };

                MouseEnter += (s2, e2) =>
                {
                    dropShadowEffect.Color = Colors.Lime;
                };
            }

            DropShadowEffect dropShadowEffect;


            private Point pointStart;
            private Point pointEnd;
            private TextBlock tbvChannelsNeeded;

            public async void Update()
            {
                this.Children.Clear();

                #region get start-point, end-point
                if (startPanel != null)
                {
                    while (startPanel.IsRendering)
                    {
                        await Task.Delay(20);
                    }
                    pointStart = startPanel.GetSubPanelPoint(outputIndex, false);
                    //pointStart = new Point(
                    //    startPanel.X + startPanel.Width,
                    //    startPanel.Y + startPanel.Height / 2);
                }
                else if (startPPanel != null)
                {
                    while (startPPanel.IsRendering)
                    {
                        await Task.Delay(20);
                    }
                    pointStart = startPPanel.GetOutputPanelPoint(outputIndex);
                }

                if (endPanel != null)
                {
                    while (endPanel.IsRendering)
                    {
                        await Task.Delay(20);
                    }
                    pointEnd = endPanel.GetSubPanelPoint(inputIndex, true);
                    //pointEnd = new Point(
                    //    endPanel.X,
                    //    endPanel.Y + endPanel.Height / 2);
                }
                else if (endPPanel != null)
                {
                    while (endPPanel.IsRendering)
                    {
                        await Task.Delay(20);
                    }
                    pointEnd = endPPanel.GetInputPanelPoint(inputIndex);
                }

                double minX = pointStart.X < pointEnd.X ? pointStart.X : pointEnd.X;
                double minY = pointStart.Y < pointEnd.Y ? pointStart.Y : pointEnd.Y;
                if (minX != 0)
                {
                    pointStart.X -= minX;
                    pointEnd.X -= minX;
                    this.X = minX;
                }
                if (minY != 0)
                {
                    pointStart.Y -= minY;
                    pointEnd.Y -= minY;
                    this.Y = minY;
                }
                #endregion

                #region re-draw graphs

                // cal thickness
                double lineThickness;
                if (link.calSpeed <= parent.basicConfig.lineMinThicknessScaleValue)
                {
                    lineThickness = parent.basicConfig.lineMinThickness;
                }
                else if (parent.basicConfig.lineMaxThicknessScaleValue <= link.calSpeed)
                {
                    lineThickness = parent.basicConfig.lineMaxThickness;
                }
                else
                {
                    lineThickness = parent.basicConfig.lineMinThickness
                        + (parent.basicConfig.lineMaxThickness - parent.basicConfig.lineMinThickness)
                            * ((link.calSpeed - parent.basicConfig.lineMinThicknessScaleValue)
                                / (parent.basicConfig.lineMaxThicknessScaleValue - parent.basicConfig.lineMinThicknessScaleValue));
                }



                // draw line
                double pointEndArrowHeight = parent.basicConfig.lineDotDiameter * 2;
                double pointEndArrowWidth = parent.basicConfig.lineDotDiameter * 3;
                Path path = new Path()
                {
                    StrokeThickness = lineThickness,
                    Stroke = Brushes.Black,
                };
                PathGeometry path_geometry = new PathGeometry();
                path.Data = path_geometry;
                PathFigure path_figure = new PathFigure();
                path_geometry.Figures.Add(path_figure);

                // Start at the first point.
                path_figure.StartPoint = pointStart;

                // Create a PathSegmentCollection.
                PathSegmentCollection path_segment_collection =
                    new PathSegmentCollection();
                path_figure.Segments = path_segment_collection;

                // Add the rest of the points to a PointCollection.
                PointCollection point_collection = new PointCollection(3);
                point_collection.Add(new Point(pointStart.X + 100, pointStart.Y));
                point_collection.Add(new Point(pointEnd.X - 100 - pointEndArrowWidth, pointEnd.Y));
                //point_collection.Add(new Point(pointEnd.X - pointEndArrowHeight, pointEnd.Y));
                point_collection.Add(new Point(pointEnd.X - pointEndArrowWidth, pointEnd.Y));

                // Make a PolyBezierSegment from the points.
                PolyBezierSegment bezier_segment = new PolyBezierSegment();
                bezier_segment.Points = point_collection;

                // Add the PolyBezierSegment to othe segment collection.
                path_segment_collection.Add(bezier_segment);

                this.Children.Add(path);


                // draw start dot
                double lineDotDiameter = parent.basicConfig.lineDotDiameter;
                Ellipse startDot = new Ellipse()
                {
                    Fill = Brushes.Black,
                    Width = lineDotDiameter,
                    Height = lineDotDiameter,
                };
                double lineDotDiameter_div2 = lineDotDiameter / 2;
                Canvas.SetLeft(startDot, pointStart.X - lineDotDiameter_div2);
                Canvas.SetTop(startDot, pointStart.Y - lineDotDiameter_div2);
                this.Children.Add(startDot);

                // draw end arraw
                Polygon endArrow = new Polygon()
                {
                    Fill = Brushes.Black,
                    Width = pointEndArrowWidth,
                    Height = pointEndArrowHeight,
                };
                endArrow.Points.Add(new Point(0, 0));
                endArrow.Points.Add(new Point(0, pointEndArrowHeight));
                endArrow.Points.Add(new Point(pointEndArrowWidth, pointEndArrowHeight / 2));
                Canvas.SetLeft(endArrow, pointEnd.X - pointEndArrowWidth);
                Canvas.SetTop(endArrow, pointEnd.Y - pointEndArrowHeight / 2);
                this.Children.Add(endArrow);

                // draw pic N labels
                StackPanel stackPanel = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                };
                Border iconBdr = new Border()
                {
                    Width = 32,
                    Height = 32,
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Black,
                };
                Image icon = new Image()
                {
                    Stretch = Stretch.Uniform,
                    Source = ImageIO.GetOut(link.channel),
                };
                iconBdr.Child = icon;
                stackPanel.Children.Add(iconBdr);

                tbvChannelsNeeded = new TextBlock()
                {
                    Background = bgLinkLabel,
                    TextAlignment = TextAlignment.Center,
                };
                stackPanel.Children.Add(tbvChannelsNeeded);


                Canvas.SetLeft(stackPanel, (pointStart.X + pointEnd.X - pointEndArrowWidth) / 2 - 16);
                Canvas.SetTop(stackPanel, (pointStart.Y + pointEnd.Y) / 2 - 16);
                this.Children.Add(stackPanel);

                #endregion

                // 最后一个显示正常，其他的都是闪烁一下就消失； ？？
                //StringBuilder strBdr = new StringBuilder();
                //strBdr.AppendLine(link.thing.name);
                //strBdr.AppendLine(SimpleStringHelper.UnitsOfMeasure.GetShortString((decimal)link.calSpeed, "") + " /s");
                //strBdr.AppendLine(SimpleStringHelper.UnitsOfMeasure.GetShortString((decimal)link.calSpeed * 60, "") + " /m");
                //strBdr.Append(SimpleStringHelper.UnitsOfMeasure.GetShortString((decimal)link.calSpeed * 3600, "") + " /h");
                //this.ToolTip = strBdr.ToString();

                UpdateQuantities();
            }
            internal void UpdateQuantities()
            {
                if (tbvChannelsNeeded == null)
                {
                    return;
                }
                if (link.channel == null)
                {
                    throw ProcessingChains.SearchHelper.Error_Channel_isNull();
                }
                if (link.channel.speed == null)
                {
                    throw ProcessingChains.SearchHelper.Error_Channel_Speed_isNull(link.channel);
                }
                decimal channelsNeeded = (decimal)(link.calSpeed / (double)link.channel.speed);
                tbvChannelsNeeded.Text = SimpleStringHelper.UnitsOfMeasure.GetShortString(channelsNeeded) + Environment.NewLine
                    + "(" + SimpleStringHelper.UnitsOfMeasure.GetShortString(Math.Ceiling(channelsNeeded), null, 1000, 0) + ")";
            }

            public System.Drawing.RectangleF GetBoundsRect()
            {
                return new System.Drawing.RectangleF(
                    (float)X, (float)Y,
                    (float)(pointStart.X < pointEnd.X ? pointEnd.X : pointStart.X),
                    (float)(pointStart.Y < pointEnd.Y ? pointEnd.Y : pointStart.Y)
                    );
            }


            public bool IsRendering
            {
                get
                {
                    if (this.Children.Count == 4)
                    {
                        // 0-path 1-startDot 2-endArrow
                        if (this.Children[1] is Ellipse)
                        {
                            if (((Ellipse)this.Children[1]).ActualWidth == 0)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return true;
                        }
                        if (this.Children[2] is Polygon)
                        {
                            if (((Polygon)this.Children[2]).ActualWidth == 0)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else if (this.Children.Count > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
        }
    }
}
