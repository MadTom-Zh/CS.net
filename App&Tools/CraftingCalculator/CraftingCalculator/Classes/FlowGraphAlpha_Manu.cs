using MadTomDev.App.Ctrls;
using MadTomDev.Common;
using MadTomDev.Data;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using static MadTomDev.App.Classes.FlowGraphAlpha;
using static MadTomDev.App.Classes.Recipes;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace MadTomDev.App.Classes
{
    public class FlowGraphAlpha_Manu : FlowGraphAlpha
    {
        //ProcessingChains.SearchResult data ;
        public FlowGraphAlpha_Manu(ref Canvas canvas)
            : base(ref canvas, new ProcessingChains.ResultHelper(), null, false)
        {
            //data = base.searchResult; 
            InitIOPanels();
        }
        private void InitIOPanels()
        {
            SetPanelPosition(outputPanel, 1200, 800);
            //await Task.Delay(20);
            SetAutoMargin();
        }


        #region add/remove node, link
        public async void ManuAddPPanel(Recipes.Recipe recipe, double posiX, double posiY)
        {
            ProcessingChains.ProcessingNode pNode = new ProcessingChains.ProcessingNode(recipe)
            { baseQuantity = 1, calQuantity = 1, };
            ProcessPanel pPanel = new ProcessPanel(this, pNode)
            { IsSelectable = true };
            pNode.ui = pPanel;
            foreach (ThingWithLabel subPanel in pPanel.inputPanelList)
            {
                subPanel.IsSelectable = true;
            }
            foreach (ThingWithLabel subPanel in pPanel.outputPanelList)
            {
                subPanel.IsSelectable = true;
            }
            processPanelList.Add(pPanel);
            SetPanelPosition(pPanel, posiX, posiY);
            do
            {
                await Task.Delay(20);
            }
            while (pPanel.IsRendering);
            pPanel.ReSetIOLayout();
            SetAutoMargin();

            pPanel.TitleMouseDown += Panel_TitleMouseDown;
            pPanel.ResizeHandleMouseDown += Panel_ResizeHandleMouseDown;
        }
        internal void ManuRemoveProcessPanel(ProcessPanel pPanel)
        {
            LinkLineV2 line;
            for (int i = linkLineList.Count - 1; 0 <= i; --i)
            {
                line = (LinkLineV2)linkLineList[i];
                if (line.link.nodePrev == pPanel.processNode)
                {
                    ManuRemoveLinkLine(line);
                }
                else if (line.link.nodeNext == pPanel.processNode)
                {
                    ManuRemoveLinkLine(line);
                }
            }

            mainPanel.Children.Remove(pPanel);
            foreach (ThingWithLabel p in pPanel.inputPanelList)
            {
                mainPanel.Children.Remove(p);
            }
            foreach (ThingWithLabel p in pPanel.outputPanelList)
            {
                mainPanel.Children.Remove(p);
            }
            processPanelList.Remove(pPanel);
        }

        internal void ManuAddLinkLine(
            ProcessingChains.ProcessingNodeBase pNode1,
            ProcessingChains.ProcessingNodeBase pNode2,
        Channels.Channel linkChannel,
        Things.Thing thing, decimal speed, bool update = true)
        {
            ProcessingChains.ProcessingLink newLink
                = new ProcessingChains.ProcessingLink(
                    pNode1, pNode2, thing, linkChannel, speed);
            pNode1.linksNext.Add(newLink);
            pNode2.linksPrev.Add(newLink);

            LinkLineV2 newLine = new LinkLineV2(this, newLink, false);
            newLink.ui = newLine;
            linkLineList.Add(newLine);
            mainPanel.Children.Insert(linkLineList.Count - 1, newLine);

            if (update)
            {
                newLine.Update();
            }
        }
        internal void ManuRemoveLinkLine(LinkLineV2 line)
        {
            mainPanel.Children.Remove(line);
            linkLineList.Remove(line);
            ((ProcessingChains.ProcessingNodeBase)line.link.nodePrev).linksNext.Remove(line.link);
            ((ProcessingChains.ProcessingNodeBase)line.link.nodeNext).linksPrev.Remove(line.link);
            //line.link.nodePrev = null;
            //line.link.nodeNext = null;
        }
        internal void ManuRemoveLinkLines(ProcessPanel pPanel)
        {
            List<ProcessingChains.ProcessingLink> links = new List<ProcessingChains.ProcessingLink>();
            links.AddRange(pPanel.processNode.linksPrev);
            links.AddRange(pPanel.processNode.linksNext);
            foreach (ProcessingChains.ProcessingLink lk in links)
            {
                if (lk.ui is LinkLineV2)
                {
                    ManuRemoveLinkLine((LinkLineV2)lk.ui);
                }
            }
        }
        internal void ManuRemoveLinkLines(IOPanel ioPanel)
        {
            List<ProcessingChains.ProcessingLink> links = new List<ProcessingChains.ProcessingLink>();
            foreach (ProcessingChains.ProcessingHead h in ioPanel.HeadList)
            {
                links.AddRange(h.linksPrev);
                links.AddRange(h.linksNext);
            }
            foreach (ProcessingChains.ProcessingLink lk in links)
            {
                if (lk.ui is LinkLineV2)
                {
                    ManuRemoveLinkLine((LinkLineV2)lk.ui);
                }
            }
        }
        internal void ManuRemoveAllLinkLines()
        {
            for (int i = linkLineList.Count - 1; i >= 0; --i)
            {
                ManuRemoveLinkLine((LinkLineV2)linkLineList[i]);
            }
        }

        #endregion

        #region for selecting node/subPanel

        public class SelectedUIInfo
        {
            public FlowGraphAlpha.IOPanel? ioPanel = null;
            public FlowGraphAlpha.ProcessPanel? processPanel = null;
            public FlowGraphAlpha_Manu.LinkLineV2? linkLine = null;
            public ThingWithLabel? subPanel = null;
            public bool subPanel_isInOrOut = true;

            public bool HasControls
            {
                get
                {
                    return ioPanel is not null
                        || processPanel is not null
                        || linkLine is not null;
                }
            }
            public void Clear()
            {
                ioPanel = null;
                processPanel = null;
                linkLine = null;
                subPanel = null;
            }

            public SelectedUIInfo()
            {
            }
            public SelectedUIInfo(SelectedUIInfo copyFrom)
            {
                SetInfoFrom(copyFrom);
            }
            internal void SetInfoFrom(SelectedUIInfo source)
            {
                ioPanel = source.ioPanel;
                processPanel = source.processPanel;
                linkLine = source.linkLine;
                subPanel = source.subPanel;
                subPanel_isInOrOut = source.subPanel_isInOrOut;
            }
        }
        public void GetUIInfo(ref SelectedUIInfo infoContainer, Canvas canvas, Point cursorPoint)
        {
            FrameworkElement fe = UI.VisualHelper.GetSubVisual(canvas, cursorPoint);
            UIElement? ui = TryGetUIRootItem(fe, out Type? type);

            infoContainer.linkLine = null;
            infoContainer.subPanel = null;
            infoContainer.ioPanel = null;
            infoContainer.processPanel = null;
            if (type == null || ui == null)
            {
                return;
            }
            if (type == typeof(FlowGraphAlpha.IOPanel))
            {
                infoContainer.ioPanel = (FlowGraphAlpha.IOPanel)ui;
            }
            else if (type == typeof(FlowGraphAlpha.ProcessPanel))
            {
                infoContainer.processPanel = (FlowGraphAlpha.ProcessPanel)ui;
            }
            else if (type == typeof(FlowGraphAlpha_Manu.LinkLineV2))
            {
                infoContainer.linkLine = (FlowGraphAlpha_Manu.LinkLineV2)ui;
            }
            else if (type == typeof(ThingWithLabel))
            {
                infoContainer.subPanel = (ThingWithLabel)ui;

                infoContainer.processPanel = TryFindParent_fromProcessPanels(
                    infoContainer.subPanel, out infoContainer.subPanel_isInOrOut);
                if (infoContainer.processPanel is null)
                {
                    infoContainer.ioPanel = TryFindParent_fromIOPanels(
                        infoContainer.subPanel, out infoContainer.subPanel_isInOrOut);
                }
            }
        }

        public UIElement? TryGetUIRootItem(FrameworkElement fe, out Type? uiType)
        {
            uiType = null;
            if (fe is null)
            {
                return null;
            }
            FrameworkElement test = fe;
            while (true)
            {
                if (test is ThingWithLabel)
                {
                    uiType = typeof(ThingWithLabel);
                    return (UIElement)test;
                }
                if (test is FlowGraphAlpha.IOPanel)
                {
                    uiType = typeof(FlowGraphAlpha.IOPanel);
                    return (UIElement)test;
                }
                if (test is FlowGraphAlpha.ProcessPanel)
                {
                    uiType = typeof(FlowGraphAlpha.ProcessPanel);
                    return (UIElement)test;
                }
                if (test is FlowGraphAlpha_Manu.LinkLineV2)
                {
                    uiType = typeof(FlowGraphAlpha_Manu.LinkLineV2);
                    return (UIElement)test;
                }

                if (test.Parent == null)
                {
                    return null;
                }
                test = (FrameworkElement)test.Parent;
            }
        }
        public FlowGraphAlpha.ProcessPanel? TryFindParent_fromProcessPanels(ThingWithLabel ioPanel, out bool onInputOrOutput)
        {
            foreach (FlowGraphAlpha.ProcessPanel pPanel in processPanelList)
            {
                if (pPanel.inputPanelList.Contains(ioPanel))
                {
                    onInputOrOutput = true;
                    return pPanel;
                }
                if (pPanel.outputPanelList.Contains(ioPanel))
                {
                    onInputOrOutput = false;
                    return pPanel;
                }
            }
            onInputOrOutput = true;
            return null;
        }
        public FlowGraphAlpha.IOPanel? TryFindParent_fromIOPanels(ThingWithLabel ioPanel, out bool onInputOrOutput)
        {
            if (inputPanel.sp_main.Children.Contains(ioPanel))
            {
                onInputOrOutput = true;
                return inputPanel;
            }
            if (outputPanel.sp_main.Children.Contains(ioPanel))
            {
                onInputOrOutput = false;
                return outputPanel;
            }

            onInputOrOutput = true;
            return null;
        }

        #endregion

        #region save/load graph

        private static string XMLFLAG_DESCRIPTION = "Description";
        private static string XMLFLAG_POSITION = "Position";
        private static string XMLFLAG_SIZE = "Size";
        private static string XMLFLAG_SPEED_BASE = "SpeedBase";
        // private static string XMLFLAG_SPEED_CAL = "SpeedCal";
        private static string XMLFLAG_THING_ID = "ThingID";
        private static string XMLFLAG_RECIPE_ID = "RecipeID";

        private static string XMLFLAG_PANEL_INPUT = "InputPanel";
        private static string XMLFLAG_PANEL_OUTPUT = "OutputPanel";
        private static string XMLFLAG_PANEL_SUB = "SubPanel";
        private static string XMLFLAG_PANEL_PROCESS = "ProcessPanel";

        private static string XMLFLAG_LINKLINE = "LinkLine";
        private static string XMLFLAG_LINKLINE_CHANNEL = "Channel";
        private static string XMLFLAG_LINKLINE_NODEINDEX_PREV = "NodeIndexPrev";
        private static string XMLFLAG_LINKLINE_NODEINDEX_NEXT = "NodeIndexNext";
        private static string XMLFLAG_LINKLINE_NODE_IN = "in";
        private static string XMLFLAG_LINKLINE_NODE_OUT = "out";

        internal void SaveGraph(string name, string description, bool overwrite = false)
        {
            string fileName = name + ".xml";
            string fileFullName = SceneMgr.Instance.GetSelectedSceneDirFullPath(SceneMgr.DIRNAME_FGAM);
            if (Directory.Exists(fileFullName) == false)
            {
                Directory.CreateDirectory(fileFullName);
            }
            fileFullName = System.IO.Path.Combine(fileFullName, fileName);
            if (overwrite == false && File.Exists(fileFullName))
            {
                throw new IOException($"File [{fileName}] already exists.");
            }
            // test writing file
            File.WriteAllText(fileFullName, "");

            SettingXML xml = new SettingXML();
            SettingXML.Node root = xml.rootNode;

            // input panel, sub panels, speed, position, size
            SetIOPanelXML(inputPanel, true);

            // output panel
            SetIOPanelXML(outputPanel, false);

            // all nodes, base speed, //cal speed
            ProcessPanel pPanel;
            SettingXML.Node xmlProcessPanel;
            for (int i = 0, iv = processPanelList.Count; i < iv; ++i)
            {
                pPanel = processPanelList[i];
                xmlProcessPanel = new SettingXML.Node(XMLFLAG_PANEL_PROCESS);
                xmlProcessPanel.attributes.AddUpdate(XMLFLAG_RECIPE_ID, pPanel.processNode.recipe.id.ToString());
                xmlProcessPanel.attributes.AddUpdate(XMLFLAG_POSITION, pPanel.X + "," + pPanel.Y);
                xmlProcessPanel.attributes.AddUpdate(XMLFLAG_SIZE, pPanel.ActualWidth + "," + pPanel.ActualHeight);
                xmlProcessPanel.attributes.AddUpdate(XMLFLAG_SPEED_BASE, pPanel.processNode.baseQuantity.ToString());
                root.Add(xmlProcessPanel);
            }

            // all links, speed, thing
            // sepec node index, inputPanel -1, outputPanel -2
            LinkLineV2 line;
            ProcessingChains.ProcessingLink link;
            SettingXML.Node xmlLinkLine;
            int testIdx;
            List<ProcessingChains.ProcessingNode> pNodeList = processPanelList.Select(a => a.processNode).ToList();
            for (int i = 0, iv = linkLineList.Count; i < iv; ++i)
            {
                line = (LinkLineV2)linkLineList[i];
                xmlLinkLine = new SettingXML.Node(XMLFLAG_LINKLINE);
                link = line.link;
                if (link.channel is null)
                {
                    continue;
                }
                xmlLinkLine.attributes.AddUpdate(XMLFLAG_LINKLINE_CHANNEL, link.channel.id.ToString());

                if (link.nodePrev is ProcessingChains.ProcessingNode)
                {
                    testIdx = pNodeList.IndexOf((ProcessingChains.ProcessingNode)link.nodePrev);
                    xmlLinkLine.attributes.AddUpdate(XMLFLAG_LINKLINE_NODEINDEX_PREV, testIdx.ToString());
                }
                else // is ProcessingChains.ProcessingHead
                {
                    xmlLinkLine.attributes.AddUpdate(XMLFLAG_LINKLINE_NODEINDEX_PREV,
                        XMLFLAG_LINKLINE_NODE_IN + inputPanel.IndexOfHead((ProcessingChains.ProcessingHead)link.nodePrev));
                }
                if (link.nodeNext is ProcessingChains.ProcessingNode)
                {
                    testIdx = pNodeList.IndexOf((ProcessingChains.ProcessingNode)link.nodeNext);
                    xmlLinkLine.attributes.AddUpdate(XMLFLAG_LINKLINE_NODEINDEX_NEXT, testIdx.ToString());
                }
                else // is ProcessingChains.ProcessingHead
                {
                    xmlLinkLine.attributes.AddUpdate(XMLFLAG_LINKLINE_NODEINDEX_NEXT,
                        XMLFLAG_LINKLINE_NODE_OUT + outputPanel.IndexOfHead((ProcessingChains.ProcessingHead)link.nodeNext));
                }
                xmlLinkLine.attributes.AddUpdate(XMLFLAG_THING_ID, link.thing.id.ToString());
                xmlLinkLine.attributes.AddUpdate(XMLFLAG_SPEED_BASE, link.GetBaseSpeed().ToString());

                root.Add(xmlLinkLine);
            }
            SettingXML.Node xmlDescription = new SettingXML.Node(XMLFLAG_DESCRIPTION);
            xmlDescription.Text = description;
            root.Add(xmlDescription);

            xml.Save(fileFullName);

            void SetIOPanelXML(IOPanel ioPanel, bool isInOrOut)
            {
                SettingXML.Node xmlIOPanel = new SettingXML.Node(isInOrOut ? XMLFLAG_PANEL_INPUT : XMLFLAG_PANEL_OUTPUT);
                xmlIOPanel.attributes.AddUpdate(XMLFLAG_POSITION, ioPanel.X + "," + ioPanel.Y);
                xmlIOPanel.attributes.AddUpdate(XMLFLAG_SIZE, ioPanel.ActualWidth + "," + ioPanel.ActualHeight);
                SettingXML.Node xmlSubPanel;
                ProcessingChains.ProcessingHead subItem;
                for (int i = 0, iv = ioPanel.HeadList.Count; i < iv; ++i)
                {
                    subItem = ioPanel.HeadList[i];
                    xmlSubPanel = new SettingXML.Node(XMLFLAG_PANEL_SUB);
                    xmlSubPanel.attributes.AddUpdate(XMLFLAG_THING_ID, subItem.thing.id.ToString());
                    xmlSubPanel.attributes.AddUpdate(XMLFLAG_SPEED_BASE, subItem.baseQuantity.ToString());
                    xmlIOPanel.Add(xmlSubPanel);
                }
                root.Add(xmlIOPanel);
            }
        }
        internal void DeleteGraph(string name)
        {
            string fileName = name + ".xml";
            string dir = SceneMgr.Instance.GetSelectedSceneDirFullPath(SceneMgr.DIRNAME_FGAM);
            if (Directory.Exists(dir))
            {
                File.Delete(System.IO.Path.Combine(dir, fileName));
            }
        }

        public string ReLoadGraph_description;
        internal async void ReLoadGraph(string name)
        {
            ReLoadGraph_description = "";
            string fileName = name + ".xml";
            string fileFullName = SceneMgr.Instance.GetSelectedSceneDirFullPath(SceneMgr.DIRNAME_FGAM);
            if (Directory.Exists(fileFullName) == false)
            {
                throw new IOException($"Directory [{SceneMgr.DIRNAME_FGAM}] does NOT exist.");
            }
            fileFullName = System.IO.Path.Combine(fileFullName, fileName);
            if (File.Exists(fileFullName) == false)
            {
                throw new IOException($"File [{fileName}] does NOT exist.");
            }
            SettingXML xml = SettingXML.FromFile(fileFullName);
            SettingXML.Node root = xml.rootNode;

            // load check data
            List<SettingXML.Node> nodes = root[XMLFLAG_PANEL_INPUT];
            if (nodes.Count != 1)
            {
                throw new InvalidDataException("Input-Panel data count is not 1.");
            }
            SettingXML.Node xmlInputPanel = nodes[0];
            nodes = root[XMLFLAG_PANEL_OUTPUT];
            if (nodes.Count != 1)
            {
                throw new InvalidDataException("Output-Panel data count is not 1.");
            }
            SettingXML.Node xmlOutputPanel = nodes[0];
            List<SettingXML.Node> xmlProcessPanels = root[XMLFLAG_PANEL_PROCESS];
            List<SettingXML.Node> xmlLinkLines = root[XMLFLAG_LINKLINE];


            // clear origin data
            for (int i = linkLineList.Count - 1; i >= 0; --i)
            {
                ManuRemoveLinkLine((LinkLineV2)linkLineList[i]);
            }
            for (int i = processPanelList.Count - 1; i >= 0; --i)
            {
                ManuRemoveProcessPanel(processPanelList[i]);
            }
            inputPanel.ClearHeadList();
            outputPanel.ClearHeadList();

            List<ProcessingChains.ProcessingNodeBase?> tmpSHeadlList = new List<ProcessingChains.ProcessingNodeBase?>();
            List<ProcessingChains.ProcessingNodeBase?> tmpPHeadlList = new List<ProcessingChains.ProcessingNodeBase?>();
            List<ProcessingChains.ProcessingNodeBase?> tmpPNodelList = new List<ProcessingChains.ProcessingNodeBase?>();
            List<string> tmpLogs = new List<string>();

            // input panel
            Core core = Core.Instance;
            string? testStr, testStr2;
            Guid testID;
            LoadIOPanel(ref inputPanel, xmlInputPanel);

            // output panel
            LoadIOPanel(ref outputPanel, xmlOutputPanel);

            // all nodes
            SettingXML.Node xmlPPanel;
            Recipes.Recipe? foundRecipe;
            decimal? testDec;
            Point? testPoint;
            ProcessPanel pPanel;
            for (int i = 0, iv = xmlProcessPanels.Count; i < iv; ++i)
            {
                tmpPNodelList.Add(null);
                xmlPPanel = xmlProcessPanels[i];
                testStr = xmlPPanel.attributes[XMLFLAG_RECIPE_ID];
                if (testStr is null)
                {
                    continue;
                }
                if (Guid.TryParse(testStr, out testID) == false)
                {
                    tmpLogs.Add($"Can't parse string[{testStr}] to Guid.");
                    continue;
                }
                foundRecipe = core.FindRecipe(testID);
                if (foundRecipe is null)
                {
                    tmpLogs.Add($"Can't find recipe[{testID}] in scene[{core.SceneName}].");
                    continue;
                }
                testPoint = xmlPPanel.attributes[XMLFLAG_POSITION].TryGetWPoint();
                if (testPoint is null)
                {
                    continue;
                }
                ManuAddPPanel(foundRecipe, testPoint.Value.X, testPoint.Value.Y);

                testPoint = xmlPPanel.attributes[XMLFLAG_SIZE].TryGetWPoint();
                if (testPoint is null)
                {
                    continue;
                }
                pPanel = processPanelList[processPanelList.Count - 1];
                tmpPNodelList[i] = pPanel.processNode;
                pPanel.Width = testPoint.Value.X;
                pPanel.Height = testPoint.Value.Y;

                testDec = xmlPPanel.attributes[XMLFLAG_SPEED_BASE].TryGetDecimal();
                if (testDec is null)
                {
                    continue;
                }
                pPanel.processNode.baseQuantity = testDec.Value;
                pPanel.processNode.calQuantity = testDec.Value;
                pPanel.UpdateQuantities();
            }

            // wait for rendering ??
            int counter = 30;
            foreach (ProcessPanel pp in processPanelList)
            {
                counter = 30;
                do
                {
                    await Task.Delay(10);
                }
                while (pp.IsRendering && counter-- > 0);
            }



            // all links
            SettingXML.Node xmlLinkLine;
            Channels.Channel? foundChannel;
            Things.Thing? foundThing;
            for (int i = 0, iv = xmlLinkLines.Count; i < iv; ++i)
            {
                xmlLinkLine = xmlLinkLines[i];
                // channel
                testStr = xmlLinkLine.attributes[XMLFLAG_LINKLINE_CHANNEL];
                if (testStr is null)
                {
                    continue;
                }
                foundChannel = core.FindChannel(Guid.Parse(testStr));
                if (foundChannel is null)
                {
                    continue;
                }
                // speed
                testStr = xmlLinkLine.attributes[XMLFLAG_SPEED_BASE];
                if (testStr is null)
                {
                    continue;
                }
                testDec = testStr.TryGetDecimal();
                if (testDec is null)
                {
                    continue;
                }
                // thing
                testStr = xmlLinkLine.attributes[XMLFLAG_THING_ID];
                if (testStr is null)
                {
                    continue;
                }
                foundThing = core.FindThing(Guid.Parse(testStr));
                if (foundThing is null)
                {
                    continue;
                }

                // node prev
                // node next
                testStr = xmlLinkLine.attributes[XMLFLAG_LINKLINE_NODEINDEX_PREV];
                testStr2 = xmlLinkLine.attributes[XMLFLAG_LINKLINE_NODEINDEX_NEXT];
                if (testStr is null || testStr2 is null)
                {
                    continue;
                }
                GetNode(out ProcessingChains.ProcessingNodeBase? nodeLeft, testStr);
                GetNode(out ProcessingChains.ProcessingNodeBase? nodeRight, testStr2);
                void GetNode(out ProcessingChains.ProcessingNodeBase? node, string flag)
                {
                    if (flag.StartsWith(XMLFLAG_LINKLINE_NODE_IN))
                    {
                        node = tmpSHeadlList[int.Parse(flag.Substring(XMLFLAG_LINKLINE_NODE_IN.Length))];
                    }
                    else if (flag.StartsWith(XMLFLAG_LINKLINE_NODE_OUT))
                    {
                        node = tmpPHeadlList[int.Parse(flag.Substring(XMLFLAG_LINKLINE_NODE_OUT.Length))];
                    }
                    else
                    {
                        node = tmpPNodelList[int.Parse(flag)];
                    }
                }
                if (nodeLeft is null)
                {
                    tmpLogs.Add($"Can't find prevNode on link[{i}].");
                }
                else if (nodeRight is null)
                {
                    tmpLogs.Add($"Can't find nextNode on link[{i}].");
                }
                else
                {
                    ManuAddLinkLine(
                        nodeLeft,
                        nodeRight,
                        foundChannel, foundThing, testDec.Value,
                        false
                        );
                }
            }
            foreach (LinkLineV2 l in linkLineList)
            {
                l.Update();
            }

            // description
            nodes = root[XMLFLAG_DESCRIPTION];
            if (nodes.Count > 0)
            {
                ReLoadGraph_description = nodes[0].Text;
            }

            if (tmpLogs.Count > 0)
            {
                StringBuilder strBdr = new StringBuilder();
                foreach (string l in tmpLogs)
                {
                    strBdr.AppendLine(l);
                }
                MessageBox.Show(strBdr.ToString(), "Missing elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            void LoadIOPanel(ref IOPanel ioPanel, SettingXML.Node xmlData)
            {
                List<ProcessingChains.ProcessingNodeBase?> tmpList;
                if (ioPanel == inputPanel)
                {
                    tmpList = tmpSHeadlList;
                }
                else
                {
                    tmpList = tmpPHeadlList;
                }
                testStr = xmlData.attributes[XMLFLAG_POSITION];
                if (testStr is not null)
                {
                    Point? tp = testStr.TryGetWPoint();
                    if (tp is not null)
                    {
                        ioPanel.X = tp.Value.X;
                        ioPanel.Y = tp.Value.Y;
                    }
                }
                testStr = xmlData.attributes[XMLFLAG_SIZE];
                if (testStr is not null)
                {
                    Size? sz = testStr.TryGetWSize();
                    if (sz is not null)
                    {
                        ioPanel.Width = sz.Value.Width;
                        ioPanel.Height = sz.Value.Height;
                    }
                }

                nodes = xmlData[XMLFLAG_PANEL_SUB];
                List<ProcessingChains.ProcessingHead> subHeads = new List<ProcessingChains.ProcessingHead>();
                SettingXML.Node xmlSubNode;
                Things.Thing? foundThing;
                ProcessingChains.ProcessingHead newHead;
                for (int i = 0, iv = nodes.Count; i < iv; ++i)
                {
                    tmpList.Add(null);
                    xmlSubNode = nodes[i];
                    testStr = xmlSubNode.attributes[XMLFLAG_THING_ID];
                    if (testStr is null)
                    {
                        continue;
                    }
                    if (Guid.TryParse(testStr, out testID) == false)
                    {
                        tmpLogs.Add($"Can't parse string[{testStr}] to Guid.");
                        continue;
                    }
                    foundThing = core.FindThing(testID);
                    testStr = xmlSubNode.attributes[XMLFLAG_SPEED_BASE];
                    if (foundThing is null || testStr is null)
                    {
                        tmpLogs.Add($"Can't find thing[{testID}] in scene[{core.SceneName}].");
                        continue;
                    }
                    newHead = new ProcessingChains.ProcessingHead(ioPanel == inputPanel, foundThing, decimal.Parse(testStr));
                    subHeads.Add(newHead);
                    tmpList[i] = newHead;
                }
                ioPanel.HeadList = subHeads;
            }
        }

        internal List<FileInfo> GetGraphFiles()
        {
            List<FileInfo> result = new List<FileInfo>();
            string dir = SceneMgr.Instance.GetSelectedSceneDirFullPath(SceneMgr.DIRNAME_FGAM);
            if (Directory.Exists(dir))
            {
                result.AddRange(new DirectoryInfo(dir).GetFiles("*.xml"));
            }
            return result;
        }


        #endregion

        #region set speeds

        private void SetNodeBaseQuantity(ProcessPanel startNodePanel, decimal baseQuantity)
        {
            startNodePanel.processNode.baseQuantity
                = startNodePanel.processNode.calQuantity
                = baseQuantity;
            startNodePanel.UpdateQuantities();
        }
        public void SetNodeBaseQuantity(ProcessPanel startNodePanel, decimal baseQuantity,
            bool setLinks, out List<LinkLineV2>? unsetPrevLines, out List<LinkLineV2>? unsetNextLines)
        {
            unsetPrevLines = null; unsetNextLines = null;
            SetNodeBaseQuantity(startNodePanel, baseQuantity);
            if (setLinks && startNodePanel.processNode.recipe.period is not null)
            {
                decimal period = startNodePanel.processNode.recipe.period.Value;
                unsetPrevLines = new List<LinkLineV2>();
                unsetNextLines = new List<LinkLineV2>();

                Recipes.Recipe recipe = startNodePanel.processNode.recipe;
                Recipes.Recipe.Quantity? q;

                List<LinkLineV2> curPortLines;
                for (int i = 0, iv = recipe.inputs.Count; i < iv; ++i)
                {
                    q = recipe.inputs[i].quantity;
                    curPortLines = GetLinkLines(startNodePanel, true, i);
                    if (q is null)
                    {
                        unsetPrevLines.AddRange(curPortLines);
                        continue;
                    }
                    if (curPortLines.Count != 1)
                    {
                        unsetPrevLines.AddRange(curPortLines);
                        continue;
                    }
                    else
                    {
                        SetLinkBaseSpeed(
                            curPortLines[0],
                            baseQuantity * q.ValueCurrentInGeneral / period);
                    }
                }
                for (int i = 0, iv = recipe.outputs.Count; i < iv; ++i)
                {
                    q = recipe.outputs[i].quantity;
                    curPortLines = GetLinkLines(startNodePanel, false, i);
                    if (q is null)
                    {
                        unsetNextLines.AddRange(curPortLines);
                        continue;
                    }
                    if (curPortLines.Count != 1)
                    {
                        unsetNextLines.AddRange(curPortLines);
                        continue;
                    }
                    else
                    {
                        SetLinkBaseSpeed(
                            curPortLines[0],
                            baseQuantity * q.ValueCurrentInGeneral / period);
                    }
                }
            }
            UpdateLines(unsetPrevLines);
            UpdateLines(unsetNextLines);
            void UpdateLines(List<LinkLineV2>? lines)
            {
                if (lines is not null)
                {
                    foreach (LinkLineV2 l in lines)
                    {
                        l.Update();
                    }
                }
            }
        }
        public List<LinkLineV2> GetLinkLines(ProcessPanel pPanel, bool onInputOrOutput, int portIndex)
        {
            List<LinkLineV2> result = new List<LinkLineV2>();
            List<ProcessingChains.ProcessingLink> linkList;
            if (onInputOrOutput)
            {
                linkList = pPanel.processNode.FindPrevLinks(portIndex);
            }
            else
            {
                linkList = pPanel.processNode.FindNextLinks(portIndex);
            }
            foreach (ProcessingChains.ProcessingLink link in linkList)
            {
                if (link.ui is LinkLineV2)
                {
                    result.Add((LinkLineV2)link.ui);
                }
            }
            return result;
        }
        public List<LinkLineV2> GetLinkLines(IOPanel ioPanel, bool onInputOrOutput, int headIndex)
        {
            ProcessingChains.ProcessingHead head = ioPanel.HeadList[headIndex];
            List<ProcessingChains.ProcessingLink> linkList;
            if (onInputOrOutput)
            {
                linkList = head.linksPrev;
            }
            else
            {
                linkList = head.linksNext;
            }
            List<LinkLineV2> result = new List<LinkLineV2>();
            foreach (ProcessingChains.ProcessingLink link in linkList)
            {
                if (link.ui is LinkLineV2)
                {
                    result.Add((LinkLineV2)link.ui);
                }
            }
            return result;
        }
        public List<LinkLineV2> GetLinkLines(IOPanel ioPanel, bool onInputOrOutput, Things.Thing onThing)
        {
            int headIndex = ioPanel.IndexOfHead(onThing);
            return GetLinkLines(ioPanel, onInputOrOutput, headIndex);
        }
        public List<LinkLineV2> GetLinkLines(ProcessingChains.ProcessingNodeBase nodeBase, bool onInputOrOutput, Things.Thing onThing)
        {
            if (nodeBase is ProcessingChains.ProcessingNode)
            {
                ProcessingChains.ProcessingNode pNode = (ProcessingChains.ProcessingNode)nodeBase;
                int portIndex;
                if (onInputOrOutput)
                {
                    portIndex = pNode.IndexOfInput(onThing);
                }
                else
                {
                    portIndex = pNode.IndexOfOutput(onThing);
                }
                if (pNode.ui is ProcessPanel)
                {
                    return GetLinkLines((ProcessPanel)pNode.ui, onInputOrOutput, portIndex);
                }
                else
                {
                    return new List<LinkLineV2>();
                }
            }
            else if (nodeBase is ProcessingChains.ProcessingHead)
            {
                ProcessingChains.ProcessingHead head = (ProcessingChains.ProcessingHead)nodeBase;
                if (head.ui is ThingWithLabel)
                {
                    List<ProcessingChains.ProcessingLink> linkList;
                    if (onInputOrOutput)
                    {
                        linkList = head.linksPrev;
                    }
                    else
                    {
                        linkList = head.linksNext;
                    }
                    List<LinkLineV2> result = new List<LinkLineV2>();
                    foreach (ProcessingChains.ProcessingLink link in linkList)
                    {
                        if (link.ui is LinkLineV2)
                        {
                            result.Add((LinkLineV2)link.ui);
                        }
                    }
                    return result;
                }
                else
                {
                    return new List<LinkLineV2>();
                }
            }
            else
            {
                return new List<LinkLineV2>();
            }
        }

        public void SetLinkBaseSpeed(LinkLineV2 linkLine, decimal baseSpeed, bool update = true)
        {
            linkLine.link.baseQuantity
                = linkLine.link.calQuantity
                = baseSpeed / linkLine.link.GetChannelSpeed();
            if (update)
            {
                linkLine.Update();
            }
        }
        public void SetGraphBaseSpeed(ProcessPanel startNodePanel, decimal? baseQuantity,
            out List<ProcessPanel> unsetPPanels, out List<LinkLineV2> unsetLinkLines)
        {
            if (baseQuantity is null)
            {
                baseQuantity = startNodePanel.processNode.baseQuantity;
            }

            unsetPPanels = processPanelList.ToList();
            unsetLinkLines = linkLineList.Select(a => (LinkLineV2)a).ToList();
            List<LinkLineV2> allLinesList = unsetLinkLines.ToList();

            SetGraphBaseSpeed_loop(
                ref unsetPPanels, ref unsetLinkLines,
                startNodePanel, baseQuantity.Value);

            while (SetGraphBaseSpeed_fromSetLinks(ref unsetPPanels, ref unsetLinkLines))
            {
            }

            foreach (LinkLineV2 l in allLinesList)
            {
                l.Update();
            }
        }
        private void SetGraphBaseSpeed_loop(
            ref List<ProcessPanel> unsetPPanels, ref List<LinkLineV2> unsetLinkLines,
            ProcessPanel curPPanel, decimal baseQuantity)
        {
            if (unsetPPanels.Contains(curPPanel) == false)
            {
                return;
            }
            SetNodeBaseQuantity(curPPanel, baseQuantity);
            unsetPPanels.Remove(curPPanel);

            // set inputs, remove link, if multiple links, skip

            SetGraphBaseSpeed_loop_SetLinkLines(
                ref unsetPPanels, ref unsetLinkLines, baseQuantity,
                curPPanel, true);

            // set outputs
            SetGraphBaseSpeed_loop_SetLinkLines(
                ref unsetPPanels, ref unsetLinkLines, baseQuantity,
                curPPanel, false);


        }
        private void SetGraphBaseSpeed_loop_SetLinkLines(
            ref List<ProcessPanel> unsetPPanels,
            ref List<LinkLineV2> unsetLinkLines,
            decimal baseQuantity,
            ProcessPanel pPanel,
            bool onInputOrOutput)
        {
            Recipes.Recipe recipe = pPanel.processNode.recipe;

            List<Recipes.Recipe.PIOItem> ports;
            if (onInputOrOutput)
            {
                ports = recipe.inputs;
            }
            else
            {
                ports = recipe.outputs;
            }

            if (recipe.period is null || recipe.period <= 0)
            {
                throw ProcessingChains.ResultHelper.Error_Recipe_Peroid_isNullOrZero(recipe.name);
            }
            decimal period1 = recipe.period.Value, period2;
            List<LinkLineV2> portLines, optNodeLines;
            LinkLineV2 line;
            Recipes.Recipe.Quantity? q1, q2;
            decimal q1v;
            for (int i = 0, iv = ports.Count; i < iv; ++i)
            {
                q1 = ports[i].quantity;
                if (q1 is null)
                {
                    continue;
                }
                portLines = GetLinkLines(pPanel, onInputOrOutput, i);
                if (portLines.Count != 1)
                {
                    continue;
                }
                line = portLines[0];
                if (unsetLinkLines.Contains(line) == false)
                {
                    continue;
                }

                q1v = q1.ValueCurrentInGeneral;
                SetLinkBaseSpeed(line, baseQuantity * q1v / period1, false);
                unsetLinkLines.Remove(line);

                if (onInputOrOutput)
                {
                    optNodeLines = GetLinkLines(line.link.nodePrev, false, line.link.thing);
                }
                else
                {
                    optNodeLines = GetLinkLines(line.link.nodeNext, true, line.link.thing);
                }
                if (optNodeLines.Count != 1)
                {
                    continue;
                }

                ProcessingChains.ProcessingNodeBase optNodeBase;
                if (onInputOrOutput)
                {
                    optNodeBase = line.link.nodePrev;
                }
                else
                {
                    optNodeBase = line.link.nodeNext;
                }


                ProcessingChains.ProcessingNode optNode;
                ProcessingChains.ProcessingHead optHead;

                if (optNodeBase is ProcessingChains.ProcessingNode)
                {
                    optNode = (ProcessingChains.ProcessingNode)optNodeBase;
                    if (onInputOrOutput)
                    {
                        q2 = optNode.recipe.outputs[optNode.IndexOfOutput(line.link.thing)].quantity;
                    }
                    else
                    {
                        q2 = optNode.recipe.inputs[optNode.IndexOfInput(line.link.thing)].quantity;
                    }
                    if (optNode.ui is ProcessPanel
                        && q2 is not null
                        && optNode.recipe.period is not null)
                    {
                        ProcessPanel prePPanel = (ProcessPanel)optNode.ui;
                        period2 = optNode.recipe.period.Value;
                        decimal baseQuantity2 = baseQuantity * (q1v / q2.ValueCurrentInGeneral) * (period2 / period1);
                        SetGraphBaseSpeed_loop(ref unsetPPanels, ref unsetLinkLines,
                            prePPanel, baseQuantity2);
                    }
                }
                else if (optNodeBase is ProcessingChains.ProcessingHead)
                {
                    optHead = (ProcessingChains.ProcessingHead)optNodeBase;
                    optHead.calQuantity = optHead.baseQuantity = q1v;
                }
            }
        }

        /// <summary>
        /// 从节点的输入或输入侧寻找已经设置链接，如果存在多条，则根据链接速度设定其余节点；
        /// </summary>
        /// <param name="unsetPPanels"></param>
        /// <param name="unsetLinkLines"></param>
        /// <param name="curPPanel"></param>
        private bool SetGraphBaseSpeed_fromSetLinks(
            ref List<ProcessPanel> unsetPPanels, ref List<LinkLineV2> unsetLinkLines)
        {
            ProcessingChains.ProcessingNode pNode;
            decimal baseMultiple;
            for (int i = 0, iv = unsetPPanels.Count; i < iv; ++i)
            {
                pNode = unsetPPanels[i].processNode;
                if (pNode.linksPrev.Count > 0)
                {
                    if (CheckLinksHadSet(pNode.linksPrev, unsetLinkLines)
                        && CheckPortsSpeeds(pNode, true, out baseMultiple))
                    {
                        SetGraphBaseSpeed_loop(ref unsetPPanels, ref unsetLinkLines,
                            unsetPPanels[i], baseMultiple);

                        return true;
                    }
                }
                if (pNode.linksNext.Count > 0)
                {
                    if (CheckLinksHadSet(pNode.linksNext, unsetLinkLines)
                        && CheckPortsSpeeds(pNode, false, out baseMultiple))
                    {
                        SetGraphBaseSpeed_loop(ref unsetPPanels, ref unsetLinkLines,
                            unsetPPanels[i], baseMultiple);

                        return true;
                    }
                }
            }
            bool CheckPortsSpeeds(ProcessingChains.ProcessingNode pNode, bool inOrOut, out decimal baseMultiple)
            {
                baseMultiple = -1;
                List<Recipe.PIOItem> portItems;
                if (inOrOut)
                {
                    portItems = pNode.recipe.inputs;
                }
                else
                {
                    portItems = pNode.recipe.outputs;
                }
                decimal multiple = -1, multiple_pre = -2,
                    portSpeed, totalLinkSpeed;
                List<ProcessingChains.ProcessingLink> portLinks;
                for (int i = 0, iv = portItems.Count; i < iv; ++i)
                {
                    if (inOrOut)
                    {
                        portLinks = pNode.FindPrevLinks(i);
                    }
                    else
                    {
                        portLinks = pNode.FindNextLinks(i);
                    }
                    if (portLinks.Count == 0)
                    {
                        continue;
                    }

                    ProcessingChains.ResultHelper.GetFlowSpeeds(
                        pNode, inOrOut, i, out portSpeed, out totalLinkSpeed);
                    if (totalLinkSpeed < ProcessingChains.aboutZero)
                    {
                        return false;
                    }
                    multiple = totalLinkSpeed / pNode.GetPortSpeed_perSec(inOrOut, i, 1);

                    if (multiple_pre > 0
                        && Math.Abs(multiple - multiple_pre) > ProcessingChains.aboutZero)
                    {
                        return false;
                    }

                    multiple_pre = multiple;
                }
                if (multiple < 0)
                {
                    return false;
                }
                baseMultiple = multiple;
                return true;
            }
            bool CheckLinksHadSet(List<ProcessingChains.ProcessingLink> linkList, List<LinkLineV2> unsetLinkLines)
            {
                foreach (ProcessingChains.ProcessingLink l in linkList)
                {
                    if (unsetLinkLines.Find(a => a.link == l) is not null)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public void SetGraphBaseSpeed(LinkLineV2 startLinkLine, decimal? baseQuantity,
            out List<ProcessPanel> unsetPPanels, out List<LinkLineV2> unsetLinkLines)
        {
            if (baseQuantity is null)
            {
                baseQuantity = startLinkLine.link.baseQuantity;
            }

            // conditions:
            // no node, one node, or two nodes
            // node is p-node
            //      it has only one(current) link
            //      it has multiple links
            // node is p-head
            //      it has only one(current) link
            //      it has multiple links


            ProcessingChains.ProcessingNodeBase optBaseNode = startLinkLine.link.nodePrev;
            ProcessingChains.ProcessingNode optPNode;

            // check left p-node
            if (optBaseNode is ProcessingChains.ProcessingNode)
            {
                optPNode = (ProcessingChains.ProcessingNode)optBaseNode;
                if (optPNode.ui is null)
                {
                    throw new Exception("No UI!");
                }
                List<ProcessingChains.ProcessingLink> outputLinks
                    = optPNode.FindNextLinks(startLinkLine.link.thing.id);
                if (outputLinks.Count == 1)
                {
                    decimal portSpeed_baseOne = optPNode.GetPortSpeed_perSec(
                        false,
                        optPNode.IndexOfOutput(startLinkLine.link.thing),
                        1
                        );
                    SetGraphBaseSpeed(
                        (ProcessPanel)optPNode.ui,
                        baseQuantity * startLinkLine.link.GetChannelSpeed() / portSpeed_baseOne,
                        out unsetPPanels, out unsetLinkLines
                        );
                    return;
                }
            }
            // check right p-node
            optBaseNode = startLinkLine.link.nodeNext;
            if (optBaseNode is ProcessingChains.ProcessingNode)
            {
                optPNode = (ProcessingChains.ProcessingNode)optBaseNode;
                if (optPNode.ui is null)
                {
                    throw new Exception("No UI!");
                }
                List<ProcessingChains.ProcessingLink> inputLinks
                    = optPNode.FindPrevLinks(startLinkLine.link.thing.id);
                if (inputLinks.Count == 1)
                {
                    decimal portSpeed_baseOne = optPNode.GetPortSpeed_perSec(
                        true,
                        optPNode.IndexOfInput(startLinkLine.link.thing),
                        1
                        );
                    SetGraphBaseSpeed(
                        (ProcessPanel)optPNode.ui,
                        baseQuantity * startLinkLine.link.GetChannelSpeed() / portSpeed_baseOne,
                        out unsetPPanels, out unsetLinkLines
                        );
                    return;
                }
            }


            SetLinkBaseSpeed(startLinkLine, baseQuantity.Value, false);

            // check left head
            optBaseNode = startLinkLine.link.nodePrev;
            ProcessingChains.ProcessingHead optHead;
            ProcessingChains.ProcessingLink testLink;
            ProcessingChains.ProcessingNode testPNode;
            if (optBaseNode is ProcessingChains.ProcessingHead)
            {
                optHead = (ProcessingChains.ProcessingHead)optBaseNode;
                if (optHead.linksNext.Count == 1)
                {
                    optHead.baseQuantity
                        = optHead.calQuantity
                        = baseQuantity.Value * startLinkLine.link.GetChannelSpeed();
                    if (optHead.linksPrev.Count == 1)
                    {
                        testLink = optHead.linksPrev[0];
                        testLink.baseQuantity
                            = testLink.calQuantity
                            = optHead.baseQuantity / testLink.GetChannelSpeed();
                        if (testLink.nodePrev is ProcessingChains.ProcessingNode)
                        {
                            testPNode = (ProcessingChains.ProcessingNode)testLink.nodePrev;
                            if (testPNode.ui is null)
                            {
                                throw new Exception("No UI!");
                            }
                            int outputPortIndex = testPNode.IndexOfOutput(testLink.thing);
                            if (testPNode.FindNextLinks(outputPortIndex).Count == 1)
                            {
                                decimal portSpeed_baseOne = testPNode.GetPortSpeed_perSec(
                                    false,
                                    outputPortIndex,
                                    1
                                    );
                                SetGraphBaseSpeed(
                                    (ProcessPanel)testPNode.ui,
                                    optHead.baseQuantity / portSpeed_baseOne,
                                    out unsetPPanels, out unsetLinkLines
                                    );
                                return;
                            }
                        }
                    }
                    UpdateLinkLines(optHead.linksNext.Select(a => (LinkLineV2?)a.ui));
                    UpdateLinkLines(optHead.linksPrev.Select(a => (LinkLineV2?)a.ui));
                }
            }
            // check right head
            optBaseNode = startLinkLine.link.nodeNext;
            if (optBaseNode is ProcessingChains.ProcessingHead)
            {
                optHead = (ProcessingChains.ProcessingHead)optBaseNode;
                if (optHead.linksPrev.Count == 1)
                {
                    optHead.baseQuantity
                        = optHead.calQuantity
                        = baseQuantity.Value * startLinkLine.link.GetChannelSpeed();
                    if (optHead.linksNext.Count == 1)
                    {
                        testLink = optHead.linksNext[0];
                        testLink.baseQuantity
                            = testLink.calQuantity
                            = optHead.baseQuantity / testLink.GetChannelSpeed();
                        if (testLink.nodeNext is ProcessingChains.ProcessingNode)
                        {
                            testPNode = (ProcessingChains.ProcessingNode)testLink.nodeNext;
                            if (testPNode.ui is null)
                            {
                                throw new Exception("No UI!");
                            }
                            int inputPortIndex = testPNode.IndexOfInput(testLink.thing);
                            if (testPNode.FindPrevLinks(inputPortIndex).Count == 1)
                            {
                                decimal portSpeed_baseOne = testPNode.GetPortSpeed_perSec(
                                    true,
                                    inputPortIndex,
                                    1
                                    );
                                SetGraphBaseSpeed(
                                    (ProcessPanel)testPNode.ui,
                                    optHead.baseQuantity / portSpeed_baseOne,
                                    out unsetPPanels, out unsetLinkLines
                                    );
                                return;
                            }
                        }
                    }
                    UpdateLinkLines(optHead.linksNext.Select(a => (LinkLineV2?)a.ui));
                    UpdateLinkLines(optHead.linksPrev.Select(a => (LinkLineV2?)a.ui));
                }
            }


            unsetPPanels = processPanelList.ToList();
            unsetLinkLines = linkLineList.Select(a => (LinkLineV2)a).ToList();
            unsetLinkLines.Remove(startLinkLine);

            void UpdateLinkLines(IEnumerable<LinkLineV2?> lines)
            {
                foreach (LinkLineV2 l in lines)
                {
                    l?.Update();
                }
            }
        }

        #endregion

        #region checks

        internal bool CheckHasRecipe(Recipes.Recipe recipe)
        {
            return processPanelList.Find(a => a.processNode.recipe == recipe) is not null;
        }
        internal bool CheckHasRecipe_withSameOutputs(Recipes.Recipe recipe, out Recipes.Recipe? existRecipe)
        {
            existRecipe = null;
            foreach (ProcessPanel pp in processPanelList)
            {
                if (pp.processNode.recipe.outputs.Count == recipe.outputs.Count)
                {
                    foreach (Recipes.Recipe.PIOItem item in recipe.outputs)
                    {
                        if (pp.processNode.recipe.outputs.Find(a => a.thing != null && item.thing != null
                            && a.thing.id == item.thing.id) == null)
                        {
                            return false;
                        }
                    }
                    existRecipe = pp.processNode.recipe;
                    return true;
                }
            }
            return false;
        }

        internal bool CheckHasLink(ProcessingChains.ProcessingNode pNode, Guid itemId, bool? inOrOut = null)
        {
            if (inOrOut is null || inOrOut == true)
            {
                foreach (LinkLineV2 l in linkLineList)
                {
                    if (l.link.nodeNext == pNode && l.link.thing.id == itemId)
                    {
                        return true;
                    }
                }
            }
            if (inOrOut is null || inOrOut == false)
            {
                foreach (LinkLineV2 l in linkLineList)
                {
                    if (l.link.nodePrev == pNode && l.link.thing.id == itemId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool CheckHasFullLink(ProcessingChains.ProcessingNode pNode, bool? inOrOut)
        {
            bool isInFullLinked = false;
            bool isOutFullLinked = false;

            if (inOrOut is null || inOrOut == true)
            {
                foreach (Recipes.Recipe.PIOItem pio in pNode.recipe.inputs)
                {
                    if (pio.thing == null)
                    {
                        continue;
                    }
                    if (linkLineList.Find(l => l.link.nodeNext == pNode && l.link.thing.id == pio.thing.id) is null)
                    {
                        break;
                    }
                }
                isInFullLinked = true;
            }
            if (inOrOut is null || inOrOut == false)
            {
                foreach (Recipes.Recipe.PIOItem pio in pNode.recipe.outputs)
                {
                    if (pio.thing == null)
                    {
                        continue;
                    }
                    if (linkLineList.Find(l => l.link.nodePrev == pNode && l.link.thing.id == pio.thing.id) is null)
                    {
                        break;
                    }
                }
                isOutFullLinked = true;
            }
            if (inOrOut == true && isInFullLinked)
            {
                return true;
            }
            else if (inOrOut == false && isOutFullLinked)
            {
                return true;
            }
            else if (inOrOut == null && isInFullLinked && isOutFullLinked)
            {
                return true;
            }
            return false;
        }


        internal bool CheckCanLink(SelectedUIInfo first, SelectedUIInfo second,
            out Things.Thing? linkOnThing, out decimal linkSpeed)
        {
            linkOnThing = null;
            linkSpeed = 0;
            if (first.processPanel is null || second.processPanel is null)
            {
                return false;
            }
            if (second.subPanel is not null && second.subPanel.ThingBase is not null)
            {
                Guid secSubThingId = second.subPanel.ThingBase.id;
                if (CheckHasLink(
                    first.processPanel.processNode,
                    second.processPanel.processNode,
                    secSubThingId))
                {
                    return false;
                }
                if (first.subPanel != null)
                {
                    if (first.subPanel_isInOrOut)
                    {
                        return false;
                    }
                    if (first.subPanel.ThingBase is not null
                        && first.subPanel.ThingBase.id == secSubThingId)
                    {
                        // check if link already exists

                        linkOnThing = (Things.Thing)first.subPanel.ThingBase;

                        if (first.processPanel != null)
                        {
                            Recipes.Recipe.PIOItem? outPIO = first.processPanel.processNode.recipe.outputs
                                .Find(a => a.thing != null && a.thing.id == secSubThingId);

                            linkSpeed = first.processPanel.processNode.GetPortSpeed_perSec(false, outPIO);
                            return linkSpeed > 0;
                        }
                    }
                    return false;
                }
                else if (first.processPanel != null)
                {
                    if (first.processPanel.outputPanelList.
                        Find(a => a.ThingBase is not null && a.ThingBase.id == secSubThingId)
                        is not null)
                    {
                        linkOnThing = (Things.Thing)second.subPanel.ThingBase;

                        Recipes.Recipe.PIOItem? outPIO = first.processPanel.processNode.recipe.outputs
                            .Find(a => a.thing != null && a.thing.id == secSubThingId);
                        linkSpeed = first.processPanel.processNode.GetPortSpeed_perSec(false, outPIO);
                        return linkSpeed > 0;
                    }
                }
                return false;
            }
            else if (second.processPanel is not null)
            {
                if (first.subPanel is not null && first.subPanel.ThingBase is not null
                    && first.processPanel is not null)
                {
                    if (first.subPanel_isInOrOut)
                    {
                        return false;
                    }
                    Guid firstSubThingId = first.subPanel.ThingBase.id;
                    if (CheckHasLink(
                        first.processPanel.processNode,
                        second.processPanel.processNode,
                        firstSubThingId))
                    {
                        return false;
                    }
                    ThingWithLabel? foundSubPanel = second.processPanel.inputPanelList
                        .Find(a => a.ThingBase is not null && a.ThingBase.id == firstSubThingId);
                    if (foundSubPanel is not null && foundSubPanel.ThingBase is not null)
                    {
                        linkOnThing = (Things.Thing)foundSubPanel.ThingBase;

                        Recipes.Recipe.PIOItem? outPIO = first.processPanel.processNode.recipe.outputs
                            .Find(a => a.thing != null && a.thing.id == firstSubThingId);
                        linkSpeed = first.processPanel.processNode.GetPortSpeed_perSec(false, outPIO);
                        return linkSpeed > 0;
                    }
                }
                else if (first.processPanel != null)
                {
                    // find matched item id-list, from first pPanel outputs N' second pPanel inputs
                    List<Guid> matchedItemIds = new List<Guid>();
                    Guid?[] firstOutIds = first.processPanel.outputPanelList.Select(a => a.ThingBase?.id).ToArray();
                    Guid?[] secondInIds = second.processPanel.inputPanelList.Select(b => b.ThingBase?.id).ToArray();
                    foreach (Guid? id in firstOutIds)
                    {
                        if (id is null)
                        {
                            continue;
                        }
                        foreach (Guid? id2 in secondInIds)
                        {
                            if (id2 is null)
                            {
                                continue;
                            }
                            if (id == id2)
                            {
                                matchedItemIds.Add((Guid)id);
                            }
                        }
                    }

                    if (matchedItemIds.Count != 1)
                    {
                        return false;
                    }

                    // check if already had link
                    if (CheckHasLink(
                        first.processPanel.processNode,
                        second.processPanel.processNode,
                        matchedItemIds[0]))
                    {
                        return false;
                    }

                    linkOnThing = Core.Instance.FindThing(matchedItemIds[0]);
                    Recipes.Recipe.PIOItem? outPIO = first.processPanel.processNode.recipe.outputs
                        .Find(a => a.thing != null && a.thing.id == matchedItemIds[0]);
                    linkSpeed = first.processPanel.processNode.GetPortSpeed_perSec(false, outPIO);
                    return linkSpeed > 0;
                }
                return false;
            }

            return false;
        }
        internal bool CheckHasLink(ProcessingChains.ProcessingNode pNodePrev, ProcessingChains.ProcessingNode pNodeNext, Guid onThingId)
        {
            LinkLineV2? foundLinkLine = (LinkLineV2?)linkLineList.Find(l =>
                l.link.nodePrev == pNodePrev
                && l.link.nodeNext == pNodeNext);
            if (foundLinkLine is not null && foundLinkLine.link.thing.id == onThingId)
            {
                return true;
            }
            return false;
        }

        internal bool CheckLinkSpeed(LinkLineV2 line,
            out List<ProcessingChains.ProcessingLink> nodePrevMisLinks,
            out List<ProcessingChains.ProcessingLink> nodeNextMisLinks)
        {
            nodePrevMisLinks = new List<ProcessingChains.ProcessingLink>();
            nodeNextMisLinks = new List<ProcessingChains.ProcessingLink>();

            Check(line.link.nodePrev, true, ref nodePrevMisLinks);
            Check(line.link.nodeNext, false, ref nodeNextMisLinks);

            return nodePrevMisLinks.Count == 0 && nodeNextMisLinks.Count == 0;

            void Check(object node, bool checkNextLinks_orPrevLinks, ref List<ProcessingChains.ProcessingLink> misLinks)
            {
                decimal linkSpeedSum = 0;
                if (node is ProcessingChains.ProcessingNode)
                {
                    ProcessingChains.ProcessingNode pn = (ProcessingChains.ProcessingNode)node;
                    foreach (ProcessingChains.ProcessingLink l in (checkNextLinks_orPrevLinks ? pn.FindNextLinks(line.link.thing.id) : pn.FindPrevLinks(line.link.thing.id)))
                    {
                        misLinks.Add(l);
                        linkSpeedSum += l.GetBaseSpeed();
                    }
                    decimal portSpeedPS = pn.GetPortSpeed_perSec(!checkNextLinks_orPrevLinks, line.link);
                    if (Math.Abs(linkSpeedSum - portSpeedPS) <= ProcessingChains.aboutZero)
                    {
                        misLinks.Clear();
                    }
                }
                else if (node is ProcessingChains.ProcessingHead)
                {
                    ProcessingChains.ProcessingHead ph = (ProcessingChains.ProcessingHead)node;
                    foreach (ProcessingChains.ProcessingLink l in (checkNextLinks_orPrevLinks ? ph.linksNext : ph.linksPrev))
                    {
                        misLinks.Add(l);
                        linkSpeedSum += l.GetBaseSpeed();
                    }
                    decimal portSpeedPS = ph.baseQuantity;
                    if (Math.Abs(linkSpeedSum - portSpeedPS) <= ProcessingChains.aboutZero)
                    {
                        misLinks.Clear();
                    }
                }
            }
        }

        internal void ResetCalSpeeds()
        {
            SetNewCallSpeeds(1);
        }
        internal void SetBaseSpeeds(decimal multiple)
        {
            SetNewCallSpeeds(multiple, true);
        }

        internal void SetNewCallSpeeds(decimal multiple, bool setToBase = false)
        {
            foreach (ProcessingChains.ProcessingHead h in inputPanel.HeadList)
            {
                h.calQuantity = h.baseQuantity * multiple;
                if (setToBase)
                {
                    h.baseQuantity = h.calQuantity;
                }
            }
            inputPanel.UpdateQuantities();
            foreach (ProcessingChains.ProcessingHead h in outputPanel.HeadList)
            {
                h.calQuantity = h.baseQuantity * multiple;
                if (setToBase)
                {
                    h.baseQuantity = h.calQuantity;
                }
            }
            outputPanel.UpdateQuantities();
            ProcessingChains.ProcessingNode pNode;
            foreach (ProcessPanel pPanel in processPanelList)
            {
                pNode = pPanel.processNode;
                pNode.calQuantity = pNode.baseQuantity * multiple;
                if (setToBase)
                {
                    pNode.baseQuantity = pNode.calQuantity;
                }
                pPanel.UpdateQuantities();
            }
            ProcessingChains.ProcessingLink link;
            foreach (LinkLineV2 l in linkLineList)
            {
                link = l.link;
                link.calQuantity = link.baseQuantity * multiple;
                if (setToBase)
                {
                    link.baseQuantity = link.calQuantity;
                }
                l.UpdateQuantities();
            }
        }

        internal void SyncIOPanels()
        {
            // remove missing heads n' links
            inputPanel.ClearUI();
            outputPanel.ClearUI();

            for (int i = mainPanel.Children.Count - 1; i >= 0; --i)
            {
                UIElement u = mainPanel.Children[i];
                if (u is LinkLineV2)
                {
                    mainPanel.Children.Remove(u);
                }
            }
            linkLineList.Clear();

            inputPanel.HeadList = searchResult.allSources;
            outputPanel.HeadList = searchResult.allFinalProducts;

            LinkLineV2 newLine;
            foreach (ProcessingChains.ProcessingLink link in searchResult.allLinks)
            {
                newLine = new LinkLineV2(this, link, false);
                link.ui = newLine;
                linkLineList.Add(newLine);
                mainPanel.Children.Insert(linkLineList.Count - 1, newLine);

                if (link.nodePrev is ProcessingChains.ProcessingHead)
                {
                    newLine.LineColorBrush = LinkLineV2.LineColorBrush_IO;
                }
                newLine.Update();
            }

            inputPanel.AutoSetHeight();
            outputPanel.AutoSetHeight();
            SetAutoMargin();
        }





        #endregion

        public class LinkLineV2 : LinkLine
        {
            public FlowGraphAlpha_Manu parent;
            public LinkLineV2(FlowGraphAlpha_Manu parent, ProcessingChains.ProcessingLink link, bool useMouseOverEffect = true)
                : base(parent, link, useMouseOverEffect)
            {
                this.parent = parent;
            }

            public static SolidColorBrush LineColorBrush_ERR = Brushes.DarkRed;
            public new void Update()
            {
                List<ProcessingChains.ProcessingLink> nodePrevMisLinks;
                List<ProcessingChains.ProcessingLink> nodeNextMisLinks;
                if (parent.CheckLinkSpeed(this, out nodePrevMisLinks, out nodeNextMisLinks) == false)
                {
                    LineColorBrush = LineColorBrush_ERR;
                }
                else if (link.nodePrev is ProcessingChains.ProcessingHead
                    || link.nodeNext is ProcessingChains.ProcessingHead)
                {
                    LineColorBrush = LineColorBrush_IO;
                }
                else
                {
                    LineColorBrush = LineColorBrush_Normal;
                }
                base.Update();
            }
        }
    }

}
