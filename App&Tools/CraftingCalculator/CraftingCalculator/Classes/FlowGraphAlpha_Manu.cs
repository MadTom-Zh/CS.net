using MadTomDev.App.Ctrls;
using MadTomDev.Common;
using MadTomDev.Data;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using static MadTomDev.App.Classes.FlowGraphAlpha;
using static MadTomDev.App.Classes.Recipes;
using static MadTomDev.Data.SettingXML;
using static System.Net.WebRequestMethods;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Control = System.Windows.Controls.Control;
using File = System.IO.File;
using FontFamily = System.Windows.Media.FontFamily;
using FontStyle = System.Windows.FontStyle;
using Image = System.Windows.Controls.Image;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using TextBox = System.Windows.Controls.TextBox;


namespace MadTomDev.App.Classes
{
    public class FlowGraphAlpha_Manu : FlowGraphAlpha
    {
        //ProcessingChains.SearchResult data ;
        public List<NotePanel> notePanelList = new List<NotePanel>();
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

        public new async void SetAutoMargin(bool waitRender = true)
        {
            SetAutoMargin_busy = true;
            if (waitRender)
            {
                do
                {
                    await Task.Delay(20);
                }
                while (IsRendering);
            }

            double minLeft = double.MaxValue, minTop = double.MaxValue,
                maxRight = double.MinValue, maxBottom = double.MinValue;
            double testD;
            SetValueFrom(inputPanel);
            SetValueFrom(outputPanel);
            foreach (IPositionNSize i in processPanelList)
            {
                SetValueFrom(i);
            }
            foreach (IPositionNSize i in notePanelList)
            {
                SetValueFrom(i);
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
            UpdateAllNotePanels();
            SetAutoMargin_busy = false;

            void SetValueFrom(IPositionNSize ui)
            {
                if (minLeft > ui.X) minLeft = ui.X;
                if (minTop > ui.Y) minTop = ui.Y;
                testD = ui.X + ui.Width;
                if (maxRight < testD) maxRight = testD;
                testD = ui.Y + ui.Height;
                if (maxBottom < testD) maxBottom = testD;
            }
        }
        public new void MoveAllPanels(double? offsetX, double? offsetY)
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
            foreach (NotePanel nPanel in notePanelList)
            {
                SetPanelPosition(nPanel, nPanel.X + offsetX, nPanel.Y + offsetY);
            }
        }
        public new bool IsRendering
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
                foreach (NotePanel nPanel in notePanelList)
                {
                    if (nPanel.IsRendering)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public async void UpdateAllNotePanels(bool waitRender = true)
        {
            if (waitRender)
            {
                await Task.Delay(40);
                while (IsRendering)
                {
                    await Task.Delay(10);
                }
            }
            foreach (NotePanel np in notePanelList)
            {
                np.Update();
            }
        }


        #region add/remove node, link, note
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


        internal async void ManuAddNote(double posiX, double posiY)
        {
            NotePanel nPanel = new NotePanel(this);
            notePanelList.Add(nPanel);
            SetPanelPosition(nPanel, posiX, posiY);
            nPanel.SetLineEnd(null, null);

            do
            {
                await Task.Delay(20);
            }
            while (nPanel.IsRendering);
            SetAutoMargin();

            nPanel.TitleMouseDown += Panel_TitleMouseDown;
            nPanel.ResizeHandleMouseDown += Panel_ResizeHandleMouseDown;
            nPanel.LineHandleLeftMouseDown = NoteLineHandleLeftMouseDowned;
        }
        public NotePanel? notePanel_withLMD = null;
        private void NoteLineHandleLeftMouseDowned(NotePanel sender)
        {
            notePanel_withLMD = sender;
        }
        internal void ManuRemoveNote(NotePanel nPanel)
        {
            nPanel.RemoveLine();
            mainPanel.Children.Remove(nPanel);
            notePanelList.Remove(nPanel);
        }
        internal void ManuRemoveAllNotes()
        {
            for (int i = notePanelList.Count - 1; i >= 0; --i)
            {
                ManuRemoveNote(notePanelList[i]);
            }
        }


        #endregion

        #region for selecting node/subPanel

        public class SelectedUIInfo
        {
            public Point mousePoint;
            public FlowGraphAlpha.IOPanel? ioPanel = null;
            public FlowGraphAlpha.ProcessPanel? processPanel = null;
            public ThingWithLabel? subPanel = null;
            public bool subPanel_isInOrOut = true;

            public FlowGraphAlpha_Manu.LinkLineV2? linkLine = null;
            public bool linkLine_OnPanel = false;
            public FlowGraphAlpha_Manu.NotePanel? notePanel = null;
            public bool notePanel_onHandle = false;

            public bool HasControls
            {
                get
                {
                    return ioPanel is not null
                        || processPanel is not null
                        || linkLine is not null
                        || notePanel is not null;
                }
            }
            public void Clear()
            {
                mousePoint = new Point();
                ioPanel = null;
                processPanel = null;
                linkLine = null;
                subPanel = null;
                notePanel = null;
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
                linkLine_OnPanel = source.linkLine_OnPanel;
                subPanel = source.subPanel;
                subPanel_isInOrOut = source.subPanel_isInOrOut;
                notePanel = source.notePanel;
            }
        }
        public void GetUIInfo(ref SelectedUIInfo infoContainer, Canvas canvas, Point cursorPoint)
        {
            infoContainer.Clear();
            infoContainer.mousePoint = cursorPoint;
            FrameworkElement fe = UI.VisualHelper.GetSubVisual(canvas, cursorPoint);
            UIElement? ui = TryGetUIRootItem(fe, out Type? type);

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
                infoContainer.linkLine_OnPanel = false;
            }
            else if (type == typeof(ThingWithLabel))
            {
                infoContainer.subPanel = (ThingWithLabel)ui;
            }
            else if (type == typeof(FlowGraphAlpha_Manu.NotePanel))
            {
                infoContainer.notePanel = (FlowGraphAlpha_Manu.NotePanel)ui;
                infoContainer.notePanel_onHandle = false;
            }
            else if (type == typeof(Ellipse))
            {
                infoContainer.notePanel = TryGetNotePanel((Ellipse)ui);
                infoContainer.notePanel_onHandle = true;
            }
            else if (type == typeof(TextBlock)
                || type == typeof(TextBox)
                || type == typeof(Image)
                || type == typeof(Border))
            {
                infoContainer.subPanel = TryGetSubPanel(fe);
                if (infoContainer.subPanel is null)
                {
                    infoContainer.linkLine = TryGetLinkLine(fe);
                    if (infoContainer.linkLine is null)
                    {
                        infoContainer.ioPanel = TryGetIOPanel(fe);
                        if (infoContainer.ioPanel is null)
                        {
                            infoContainer.processPanel = TryGetProcessPanel(fe);
                            if (infoContainer.processPanel is null)
                            {
                                infoContainer.subPanel = TryGetSubPanel(fe);
                                if (infoContainer.subPanel is null)
                                {
                                    infoContainer.notePanel = TryGetNotePanel(fe);
                                    infoContainer.notePanel_onHandle = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        infoContainer.linkLine_OnPanel = true;
                    }
                }
            }


            if (infoContainer.subPanel is not null)
            {
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
                if (test is Ellipse)
                {
                    uiType = typeof(Ellipse);
                    return (UIElement)test;
                }
                if (test is Border)
                {
                    uiType = typeof(Border);
                    return (UIElement)test;
                }
                if (test is TextBlock)
                {
                    uiType = typeof(TextBlock);
                    return (UIElement)test;
                }
                if (test is TextBox)
                {
                    uiType = typeof(TextBox);
                    return (UIElement)test;
                }
                if (test is Image)
                {
                    uiType = typeof(Image);
                    return (UIElement)test;
                }
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
                if (test is FlowGraphAlpha_Manu.NotePanel)
                {
                    uiType = typeof(FlowGraphAlpha_Manu.NotePanel);
                    return (UIElement)test;
                }

                if (test.Parent == null)
                {
                    return null;
                }
                test = (FrameworkElement)test.Parent;
            }
        }
        public ProcessPanel? TryFindParent_fromProcessPanels(ThingWithLabel ioPanel, out bool onInputOrOutput)
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

        public IOPanel? TryFindParent_fromIOPanels(ThingWithLabel ioPanel, out bool onInputOrOutput)
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


        public IOPanel? TryGetIOPanel(FrameworkElement testUi)
        {
            do
            {
                if (testUi.TemplatedParent is IOPanel)
                {
                    return (IOPanel)testUi.TemplatedParent;
                }
                if (testUi.Parent is IOPanel)
                {
                    return (IOPanel)testUi.Parent;
                }
                testUi = (FrameworkElement)testUi.Parent;
            }
            while (testUi is not null);
            return null;
        }
        public ProcessPanel? TryGetProcessPanel(FrameworkElement testUi)
        {
            do
            {
                if (testUi.TemplatedParent is ProcessPanel)
                {
                    return (ProcessPanel)testUi.TemplatedParent;
                }
                if (testUi.Parent is ProcessPanel)
                {
                    return (ProcessPanel)testUi.Parent;
                }
                testUi = (FrameworkElement)testUi.Parent;
            }
            while (testUi is not null);
            return null;
        }
        public NotePanel? TryGetNotePanel(FrameworkElement testUi)
        {
            do
            {
                if (testUi.TemplatedParent is NotePanel)
                {
                    return (NotePanel)testUi.TemplatedParent;
                }
                if (testUi.Parent is NotePanel)
                {
                    return (NotePanel)testUi.Parent;
                }
                testUi = (FrameworkElement)testUi.Parent;
            }
            while (testUi is not null);
            return null;
        }
        public NotePanel? TryGetNotePanel(Ellipse dot)
        {
            Ellipse? testDot;
            foreach (FlowGraphAlpha_Manu.NotePanel nPanel in notePanelList)
            {
                testDot = nPanel.lineHandle;
                if (testDot is null
                    || testDot.Visibility != Visibility.Visible
                    || testDot != dot)
                {
                    continue;
                }
                return nPanel;
            }
            return null;
        }
        public LinkLineV2? TryGetLinkLine(FrameworkElement testUi)
        {
            do
            {
                if (testUi.TemplatedParent is LinkLineV2)
                {
                    return (LinkLineV2)testUi.TemplatedParent;
                }
                if (testUi.Parent is LinkLineV2)
                {
                    return (LinkLineV2)testUi.Parent;
                }
                testUi = (FrameworkElement)testUi.Parent;
            }
            while (testUi is not null);
            return null;
        }
        public ThingWithLabel? TryGetSubPanel(FrameworkElement testUi)
        {
            do
            {
                if (testUi.TemplatedParent is ThingWithLabel)
                {
                    return (ThingWithLabel)testUi.TemplatedParent;
                }
                if (testUi.Parent is ThingWithLabel)
                {
                    return (ThingWithLabel)testUi.Parent;
                }
                testUi = (FrameworkElement)testUi.Parent;
            }
            while (testUi is not null);
            return null;
        }

        #endregion

        #region save/load graph

        private static string XMLFLAG_DESCRIPTION = "Description";

        private static string XMLFLAG_PANEL_INPUT = "InputPanel";
        private static string XMLFLAG_PANEL_OUTPUT = "OutputPanel";
        private static string XMLFLAG_POSITION = "Position";
        private static string XMLFLAG_SIZE = "Size";

        private static string XMLFLAG_PANEL_SUB = "SubPanel";
        private static string XMLFLAG_THING_ID = "ThingID";
        private static string XMLFLAG_SPEED_BASE = "SpeedBase";

        private static string XMLFLAG_PANEL_PROCESS = "ProcessPanel";
        private static string XMLFLAG_RECIPE_ID = "RecipeID";

        private static string XMLFLAG_LINKLINE = "LinkLine";
        private static string XMLFLAG_LINKLINE_CHANNEL = "Channel";

        private static string XMLFLAG_LINKTO = "LinkTo";
        private static string XMLFLAG_LINKTO_NODETYPE = "NodeType";
        private static string XMLFLAG_LINKTO_NODEINDEX = "NodeIndex";

        private static string XMLFLAG_PANEL_NOTE = "NotePanel";
        private static string XMLFLAG_PANEL_NOTE_COLOR = "Color";
        private static string XMLFLAG_PANEL_NOTE_COLOR_BG = "BG";
        private static string XMLFLAG_PANEL_NOTE_COLOR_FG = "FG";
        private static string XMLFLAG_PANEL_NOTE_TITLE = "Title";
        private static string XMLFLAG_PANEL_NOTE_CONTENT = "Content";
        private static string XMLFLAG_PANEL_NOTE_FONT = "Font";
        private static string XMLFLAG_PANEL_NOTE_FONT_FAMILY = "Family";
        private static string XMLFLAG_PANEL_NOTE_FONT_SIZE = "Size";
        private static string XMLFLAG_PANEL_NOTE_FONT_WEIGHT = "Weight";
        private static string XMLFLAG_PANEL_NOTE_FONT_STYLE = "Style";
        private static string XMLFLAG_PANEL_NOTE_FONT_DECORATION = "Decoration";

        private static string XMLFLAG_LINKTO_SUBNODE_INOROUT = "OnInOrOut";
        private static string XMLFLAG_LINKTO_SUBNODE_INDEX = "SubNodeIndex";






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

            // description
            SettingXML.Node xmlDescription = new SettingXML.Node(XMLFLAG_DESCRIPTION);
            xmlDescription.Text = description;
            root.Add(xmlDescription);



            // input panel, sub panels, speed, position, size
            SetIOPanelXML(inputPanel, true);

            // output panel
            SetIOPanelXML(outputPanel, false);

            // all process nodes, base speed, //cal speed
            ProcessPanel pPanel;
            SettingXML.Node xmlProcessPanel;
            for (int i = 0, iv = processPanelList.Count; i < iv; ++i)
            {
                pPanel = processPanelList[i];
                xmlProcessPanel = new SettingXML.Node(XMLFLAG_PANEL_PROCESS);
                xmlProcessPanel.attributes.AddUpdate(XMLFLAG_RECIPE_ID, pPanel.processNode.recipe.id.ToString());
                xmlProcessPanel.attributes.AddUpdate(XMLFLAG_SPEED_BASE, pPanel.processNode.baseQuantity.ToString());
                xmlProcessPanel.attributes.AddUpdate(XMLFLAG_POSITION, pPanel.X + "," + pPanel.Y);
                xmlProcessPanel.attributes.AddUpdate(XMLFLAG_SIZE, pPanel.ActualWidth + "," + pPanel.ActualHeight);
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
                xmlLinkLine.attributes.AddUpdate(XMLFLAG_THING_ID, link.thing.id.ToString());
                xmlLinkLine.attributes.AddUpdate(XMLFLAG_SPEED_BASE, link.GetBaseSpeed().ToString());

                xmlLinkLine.Add(MakeLinkToXMLNode_prevNode(link.nodePrev));
                xmlLinkLine.Add(MakeLinkToXMLNode_nextNode(link.nodeNext));

                root.Add(xmlLinkLine);
            }

            // all note nodes
            NotePanel nPanel;
            SettingXML.Node xmlNotePanel, xmlNPSub;
            for (int i = 0, iv = notePanelList.Count; i < iv; ++i)
            {
                nPanel = notePanelList[i];
                xmlNotePanel = new SettingXML.Node(XMLFLAG_PANEL_NOTE);
                xmlNotePanel.attributes.AddUpdate(XMLFLAG_POSITION, nPanel.X + "," + nPanel.Y);
                xmlNotePanel.attributes.AddUpdate(XMLFLAG_SIZE, nPanel.ActualWidth + "," + nPanel.ActualHeight);

                xmlNPSub = new SettingXML.Node(XMLFLAG_PANEL_NOTE_COLOR);
                xmlNPSub.attributes.AddUpdate(XMLFLAG_PANEL_NOTE_COLOR_BG, nPanel.BackgroundColor.ToString());
                xmlNPSub.attributes.AddUpdate(XMLFLAG_PANEL_NOTE_COLOR_FG, nPanel.ForegroundColor.ToString());
                xmlNotePanel.Add(xmlNPSub);

                xmlNPSub = new SettingXML.Node(XMLFLAG_PANEL_NOTE_TITLE)
                { Text = nPanel.Title, };
                xmlNotePanel.Add(xmlNPSub);
                xmlNPSub = new SettingXML.Node(XMLFLAG_PANEL_NOTE_CONTENT)
                { Text = nPanel.ContentText, };
                xmlNotePanel.Add(xmlNPSub);

                xmlNPSub = new SettingXML.Node(XMLFLAG_PANEL_NOTE_FONT);
                xmlNPSub.attributes.AddUpdate(XMLFLAG_PANEL_NOTE_FONT_FAMILY, nPanel.FontFamily.FamilyNames.Values.First());
                xmlNPSub.attributes.AddUpdate(XMLFLAG_PANEL_NOTE_FONT_SIZE, nPanel.FontSize.ToString());
                xmlNPSub.attributes.AddUpdate(XMLFLAG_PANEL_NOTE_FONT_WEIGHT, nPanel.FontWeight.ToString());
                xmlNPSub.attributes.AddUpdate(XMLFLAG_PANEL_NOTE_FONT_STYLE, nPanel.FontStyle.ToString());
                xmlNPSub.attributes.AddUpdate(
                    XMLFLAG_PANEL_NOTE_FONT_DECORATION,
                    UI.FontDialog.TextDecorationToString(nPanel.TextDecoration));
                xmlNotePanel.Add(xmlNPSub);

                xmlNotePanel.Add(MakeLinkToXMLNode_notePanel(nPanel));

                root.Add(xmlNotePanel);
            }



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

            SettingXML.Node MakeLinkToXMLNode_prevNode(ProcessingChains.ProcessingNodeBase linkedNode)
            {
                if (linkedNode is ProcessingChains.ProcessingNode)
                {
                    return MakeLinkToXMLNode((ProcessingChains.ProcessingNode)linkedNode);
                }
                else // is ProcessingChains.ProcessingHead
                {
                    SettingXML.Node node = new SettingXML.Node(XMLFLAG_LINKTO);
                    node.attributes.AddUpdate(XMLFLAG_LINKTO_NODETYPE, nameof(inputPanel));
                    node.attributes.AddUpdate(
                        XMLFLAG_LINKTO_NODEINDEX,
                        inputPanel.IndexOfHead((ProcessingChains.ProcessingHead)linkedNode).ToString());
                    return node;
                }
            }
            SettingXML.Node MakeLinkToXMLNode_nextNode(ProcessingChains.ProcessingNodeBase linkedNode)
            {
                if (linkedNode is ProcessingChains.ProcessingNode)
                {
                    return MakeLinkToXMLNode((ProcessingChains.ProcessingNode)linkedNode);
                }
                else // is ProcessingChains.ProcessingHead
                {
                    SettingXML.Node node = new SettingXML.Node(XMLFLAG_LINKTO);
                    node.attributes.AddUpdate(XMLFLAG_LINKTO_NODETYPE, nameof(outputPanel));
                    node.attributes.AddUpdate(
                        XMLFLAG_LINKTO_NODEINDEX,
                        outputPanel.IndexOfHead((ProcessingChains.ProcessingHead)linkedNode).ToString());
                    return node;
                }
            }
            SettingXML.Node MakeLinkToXMLNode(ProcessingChains.ProcessingNode pNode)
            {
                SettingXML.Node node = new SettingXML.Node(XMLFLAG_LINKTO);
                node.attributes.AddUpdate(XMLFLAG_LINKTO_NODETYPE, nameof(ProcessingChains.ProcessingNode));
                testIdx = pNodeList.IndexOf(pNode);
                node.attributes.AddUpdate(XMLFLAG_LINKTO_NODEINDEX, testIdx.ToString());
                return node;
            }
            SettingXML.Node MakeLinkToXMLNode_notePanel(FlowGraphAlpha_Manu.NotePanel nPanel)
            {
                SettingXML.Node node = new SettingXML.Node(XMLFLAG_LINKTO);
                //NodeType  NodeIndex  OnInOrOut  SubNodeIndex position
                string nodeTypeName = "";
                int nodeIndex = -1, subNodeIndex = -1;
                bool? inOrOut = null;
                if (nPanel.optUi == inputPanel) nodeTypeName = nameof(inputPanel);
                else if (nPanel.optUi == outputPanel) nodeTypeName = nameof(outputPanel);
                else if (nPanel.optUi is ProcessPanel)
                {
                    nodeTypeName = nameof(ProcessPanel);
                    nodeIndex = processPanelList.IndexOf((ProcessPanel)nPanel.optUi);
                }
                else if (nPanel.optUi is ThingWithLabel)
                {
                    ThingWithLabel subPanel = (ThingWithLabel)nPanel.optUi;
                    IOPanel? ioPanel = TryFindParent_fromIOPanels(subPanel, out bool vb);
                    if (ioPanel == inputPanel)
                    {
                        nodeTypeName = nameof(inputPanel);
                        nodeIndex = inputPanel.IndexOfHead((ProcessingChains.ProcessingHead)subPanel.Tag);
                    }
                    else if (ioPanel == outputPanel)
                    {
                        nodeTypeName = nameof(outputPanel);
                        nodeIndex = outputPanel.IndexOfHead((ProcessingChains.ProcessingHead)subPanel.Tag);
                    }
                    else
                    {
                        ProcessPanel? pPanel = TryFindParent_fromProcessPanels(subPanel, out vb);
                        if (pPanel is not null)
                        {
                            nodeTypeName = nameof(ProcessPanel);
                            nodeIndex = processPanelList.IndexOf(pPanel);
                            Recipes.Recipe.PIOItem ioItem = (Recipes.Recipe.PIOItem)subPanel.Tag;
                            if (ioItem.thing is not null)
                            {
                                if (vb)
                                {
                                    subNodeIndex = pPanel.processNode.IndexOfInput(ioItem.thing);
                                }
                                else
                                {
                                    subNodeIndex = pPanel.processNode.IndexOfOutput(ioItem.thing);
                                }
                            }
                            inOrOut = vb;
                        }
                    }
                }
                else if (nPanel.optUi is LinkLine)
                {
                    nodeTypeName = nameof(LinkLine);
                    nodeIndex = linkLineList.IndexOf((LinkLine)nPanel.optUi);
                }
                else if (nPanel.optUi is NotePanel)
                {
                    nodeTypeName = nameof(NotePanel);
                    nodeIndex = notePanelList.IndexOf((NotePanel)nPanel.optUi);
                }

                node.attributes.AddUpdate(XMLFLAG_LINKTO_NODETYPE, nodeTypeName);
                node.attributes.AddUpdate(XMLFLAG_LINKTO_NODEINDEX, nodeIndex.ToString());
                node.attributes.AddUpdate(XMLFLAG_LINKTO_SUBNODE_INOROUT,
                    inOrOut is null ? "" : inOrOut.Value.ToString());
                node.attributes.AddUpdate(XMLFLAG_LINKTO_SUBNODE_INDEX, subNodeIndex < 0 ? "" : subNodeIndex.ToString());
                node.attributes.AddUpdate(XMLFLAG_POSITION, nPanel.optRefPt is null ? "" : nPanel.optRefPt.Value.ToString());


                return node;
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
            ManuRemoveAllNotes();
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

            Core core = Core.Instance;
            string? testStr, testStr2;
            Guid testID;


            // description
            nodes = root[XMLFLAG_DESCRIPTION];
            if (nodes.Count > 0)
            {
                ReLoadGraph_description = nodes[0].Text;
            }

            // input panel
            LoadIOPanel(ref inputPanel, xmlInputPanel);

            // output panel
            LoadIOPanel(ref outputPanel, xmlOutputPanel);

            #region all process nodes
            SettingXML.Node xmlPPanel;
            Recipes.Recipe? foundRecipe;
            decimal? testDec;
            Point? testPoint, testPoint2;
            object? testObj;
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

                testDec = xmlPPanel.attributes[XMLFLAG_SPEED_BASE].TryGetDecimal();
                if (testDec is null)
                {
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
            #endregion



            #region all links
            SettingXML.Node xmlLinkLine;
            Channels.Channel? foundChannel;
            Things.Thing? foundThing;
            List<SettingXML.Node> xmlLinkLine_linksTo;
            SettingXML.Node xmlLinkTo;
            for (int i = 0, iv = xmlLinkLines.Count; i < iv; ++i)
            {
                xmlLinkLine = xmlLinkLines[i];
                xmlLinkLine_linksTo = xmlLinkLine[XMLFLAG_LINKTO];
                if (xmlLinkLine_linksTo.Count != 2)
                {
                    continue;
                }
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

                // node prev
                // node next
                xmlLinkTo = xmlLinkLine_linksTo[0];
                testStr = xmlLinkTo.attributes[XMLFLAG_LINKTO_NODETYPE];
                testStr2 = xmlLinkTo.attributes[XMLFLAG_LINKTO_NODEINDEX];
                GetNode(out ProcessingChains.ProcessingNodeBase? nodeLeft, testStr, testStr2);
                xmlLinkTo = xmlLinkLine_linksTo[1];
                testStr = xmlLinkTo.attributes[XMLFLAG_LINKTO_NODETYPE];
                testStr2 = xmlLinkTo.attributes[XMLFLAG_LINKTO_NODEINDEX];
                GetNode(out ProcessingChains.ProcessingNodeBase? nodeRight, testStr, testStr2);

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

                void GetNode(out ProcessingChains.ProcessingNodeBase? node, string? nodeTypeStr, string? nodeIndexStr)
                {
                    node = null;
                    if (nodeTypeStr is null || nodeIndexStr is null)
                    {
                        return;
                    }
                    int nodeIndex;
                    if (int.TryParse(nodeIndexStr, out nodeIndex) == false)
                    {
                        return;
                    }
                    if (nodeTypeStr == nameof(inputPanel))
                    {
                        node = tmpSHeadlList[nodeIndex];
                    }
                    else if (nodeTypeStr == nameof(outputPanel))
                    {
                        node = tmpPHeadlList[nodeIndex];
                    }
                    else //if (nodeTypeStr == nameof(ProcessPanel))
                    {
                        node = tmpPNodelList[nodeIndex];
                    }
                    //else if (nodeTypeStr == nameof(NotePanel))
                    //{
                    //}
                }
            }
            foreach (LinkLineV2 l in linkLineList)
            {
                l.Update();
            }
            #endregion


            // all note nodes

            Dictionary<int, (int, Point?)> noteToNoteIdxList = new Dictionary<int, (int, Point?)>();

            List<SettingXML.Node> xmlNotes = root[XMLFLAG_PANEL_NOTE];
            List<SettingXML.Node> xmlNSubs;
            SettingXML.Node xmlNote, xmlNSub;
            Color? clr1, clr2;
            FontWeightConverter fontWeightCvtr = new FontWeightConverter();
            FontStyleConverter fontStyleCvtr = new FontStyleConverter();
            bool? testBool;
            NotePanel nPanel;
            List<NotePanel?> tmpNotelList = new List<NotePanel?>();
            for (int i = 0, iv = xmlNotes.Count; i < iv; ++i)
            {
                tmpNotelList.Add(null);
                xmlNote = xmlNotes[i];

                #region position, size
                testPoint = xmlNote.attributes[XMLFLAG_POSITION].TryGetWPoint();
                if (testPoint is null)
                {
                    continue;
                }
                testPoint2 = xmlNote.attributes[XMLFLAG_SIZE].TryGetWPoint();
                if (testPoint2 is null)
                {
                    continue;
                }
                #endregion

                #region color
                xmlNSubs = xmlNote[XMLFLAG_PANEL_NOTE_COLOR];
                if (xmlNSubs.Count != 1)
                {
                    continue;
                }
                xmlNSub = xmlNSubs[0];
                testObj = ColorConverter.ConvertFromString(xmlNSub.attributes[XMLFLAG_PANEL_NOTE_COLOR_BG]);
                if (testObj is Color)
                {
                    clr1 = (Color)testObj;
                }
                else
                {
                    clr1 = null;
                }
                testObj = ColorConverter.ConvertFromString(xmlNSub.attributes[XMLFLAG_PANEL_NOTE_COLOR_FG]);
                if (testObj is Color)
                {
                    clr2 = (Color)testObj;
                }
                else
                {
                    clr2 = null;
                }
                #endregion

                #region title, content
                xmlNSubs = xmlNote[XMLFLAG_PANEL_NOTE_TITLE];
                if (xmlNSubs.Count < 1)
                {
                    continue;
                }
                testStr = xmlNSubs[0].Text;
                xmlNSubs = xmlNote[XMLFLAG_PANEL_NOTE_CONTENT];
                if (xmlNSubs.Count < 1)
                {
                    continue;
                }
                testStr2 = xmlNSubs[0].Text;

                #endregion

                ManuAddNote(testPoint.Value.X, testPoint.Value.Y);
                nPanel = notePanelList.Last();
                nPanel.Width = testPoint2.Value.X;
                nPanel.Height = testPoint2.Value.Y;
                nPanel.Title = testStr;
                nPanel.ContentText = testStr2;
                tmpNotelList[i] = nPanel;

                if (clr1 is not null) nPanel.BackgroundColor = clr1.Value;
                if (clr2 is not null) nPanel.ForegroundColor = clr2.Value;


                #region font
                xmlNSubs = xmlNote[XMLFLAG_PANEL_NOTE_FONT];
                if (xmlNSubs.Count > 0)
                {
                    xmlNSub = xmlNSubs[0];
                    testStr = xmlNSub.attributes[XMLFLAG_PANEL_NOTE_FONT_FAMILY];
                    if (testStr is not null
                        && string.IsNullOrWhiteSpace(testStr) == false)
                    {
                        IEnumerable<FontFamily> foundFontFamilies = Fonts.SystemFontFamilies.Where(f => f.FamilyNames.Values.Contains(testStr));
                        if (foundFontFamilies.Count() > 0)
                        {
                            nPanel.FontFamily = foundFontFamilies.First();
                        }
                    }
                    testStr = xmlNSub.attributes[XMLFLAG_PANEL_NOTE_FONT_SIZE];
                    if (double.TryParse(testStr, out double vd))
                    {
                        nPanel.FontSize = vd;
                    }
                    testStr = xmlNSub.attributes[XMLFLAG_PANEL_NOTE_FONT_WEIGHT];
                    if (testStr is not null)
                    {
                        testObj = fontWeightCvtr.ConvertFromString(testStr);
                        if (testObj is FontWeight)
                        {
                            nPanel.FontWeight = (FontWeight)testObj;
                        }
                    }
                    testStr = xmlNSub.attributes[XMLFLAG_PANEL_NOTE_FONT_STYLE];
                    if (testStr is not null)
                    {
                        testObj = fontStyleCvtr.ConvertFromString(testStr);
                        if (testObj is FontStyle)
                        {
                            nPanel.FontStyle = (FontStyle)testObj;
                        }
                    }
                    testStr = xmlNSub.attributes[XMLFLAG_PANEL_NOTE_FONT_DECORATION];
                    if (string.IsNullOrWhiteSpace(testStr) == false)
                    {
                        nPanel.TextDecoration = UI.FontDialog.TextDecorationFromString(testStr);
                    }
                }



                #endregion

                #region link to
                xmlNSubs = xmlNote[XMLFLAG_LINKTO];
                if (xmlNSubs.Count > 0)
                {
                    xmlNSub = xmlNSubs[0];
                    testStr = xmlNSub.attributes[XMLFLAG_POSITION];
                    testPoint = testStr.TryGetWPoint();
                    testStr = xmlNSub.attributes[XMLFLAG_LINKTO_NODETYPE];
                    if (bool.TryParse(xmlNSub.attributes[XMLFLAG_LINKTO_SUBNODE_INOROUT], out bool vb))
                    {
                        testBool = vb;
                    }
                    else
                    {
                        testBool = null;
                    }
                    if (testStr == nameof(inputPanel)
                        || testStr == nameof(outputPanel))
                    {
                        IOPanel tarIOPanel = testStr == nameof(inputPanel) ? inputPanel : outputPanel;
                        if (int.TryParse(xmlNSub.attributes[XMLFLAG_LINKTO_NODEINDEX], out int vi)
                            && vi >= 0)
                        {
                            nPanel.SetLineEnd(tarIOPanel.GetHeadPanel(vi), testPoint);
                        }
                        else
                        {
                            nPanel.SetLineEnd(tarIOPanel, testPoint);
                        }
                    }
                    else if (testStr == nameof(ProcessPanel))
                    {
                        if (int.TryParse(xmlNSub.attributes[XMLFLAG_LINKTO_NODEINDEX], out int ppi))
                        {
                            pPanel = processPanelList[ppi];
                            if (testBool is null)
                            {
                                nPanel.SetLineEnd(pPanel, testPoint);
                            }
                            else
                            {
                                if (int.TryParse(xmlNSub.attributes[XMLFLAG_LINKTO_SUBNODE_INDEX], out int vi)
                                    && vi >= 0)
                                {
                                    if (testBool == true)
                                    {
                                        nPanel.SetLineEnd(pPanel.inputPanelList[vi], testPoint);
                                    }
                                    else if (testBool == false)
                                    {
                                        nPanel.SetLineEnd(pPanel.outputPanelList[vi], testPoint);
                                    }
                                    else
                                    {
                                        nPanel.SetLineEnd(pPanel, testPoint);
                                    }
                                }
                                else
                                {
                                    nPanel.SetLineEnd(pPanel, testPoint);
                                }
                            }
                        }
                    }
                    else if (testStr == nameof(LinkLine))
                    {
                        if (int.TryParse(xmlNSub.attributes[XMLFLAG_LINKTO_NODEINDEX], out int li)
                            && li >= 0)
                        {
                            nPanel.SetLineEnd(linkLineList[li], testPoint);
                        }
                    }
                    else if (testStr == nameof(NotePanel))
                    {
                        if (int.TryParse(xmlNSub.attributes[XMLFLAG_LINKTO_NODEINDEX], out int ni)
                            && ni >= 0)
                        {
                            // link note to note, after load all notes
                            noteToNoteIdxList.Add(i, (ni, testPoint));
                        }
                    }
                    else // null
                    {
                        nPanel.SetLineEnd(null, testPoint);
                    }
                }
                #endregion
                //nPanel.Update();

            }

            (int, Point?) kv;
            foreach (int k in noteToNoteIdxList.Keys)
            {
                kv = noteToNoteIdxList[k];
                notePanelList[k].SetLineEnd(notePanelList[kv.Item1], kv.Item2);
            }
            foreach (NotePanel np in notePanelList)
            {
                np.Update();
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

        private void SetNodeBaseQuantity(ProcessPanel pPanel, decimal baseQuantity)
        {
            pPanel.processNode.baseQuantity
                = pPanel.processNode.calQuantity
                = baseQuantity;
            pPanel.UpdateQuantities();
        }
        public void SetNodeBaseQuantity(ProcessPanel pPanel, decimal baseQuantity,
            bool setLinks, out List<LinkLineV2>? unsetPrevLines, out List<LinkLineV2>? unsetNextLines)
        {
            unsetPrevLines = null; unsetNextLines = null;
            SetNodeBaseQuantity(pPanel, baseQuantity);
            if (setLinks && pPanel.processNode.recipe.period is not null)
            {
                decimal period = pPanel.processNode.recipe.period.Value;
                unsetPrevLines = new List<LinkLineV2>();
                unsetNextLines = new List<LinkLineV2>();

                Recipes.Recipe recipe = pPanel.processNode.recipe;
                Recipes.Recipe.Quantity? q;

                List<LinkLineV2> curPortLines;
                LinkLineV2 curLine;
                for (int i = 0, iv = recipe.inputs.Count; i < iv; ++i)
                {
                    q = recipe.inputs[i].quantity;
                    curPortLines = GetLinkLines(pPanel, true, i);
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
                        curLine = curPortLines[0];
                        SetLinkBaseSpeed(
                            curLine,
                            baseQuantity * q.ValueCurrentInGeneral / period);

                        // 更新对方节点相关联的连线；
                        if (curLine.link.nodePrev is ProcessingChains.ProcessingNode)
                        {
                            ProcessingChains.ProcessingNode optPNode
                                = (ProcessingChains.ProcessingNode)curLine.link.nodePrev;
                            if (optPNode.ui is ProcessPanel)
                            {
                                foreach (LinkLineV2 l in GetLinkLines((ProcessPanel)optPNode.ui, false, curLine.link.thing))
                                {
                                    if (curLine != l)
                                    {
                                        l.Update();
                                    }
                                }
                            }
                        }
                    }
                }
                for (int i = 0, iv = recipe.outputs.Count; i < iv; ++i)
                {
                    q = recipe.outputs[i].quantity;
                    curPortLines = GetLinkLines(pPanel, false, i);
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
                        curLine = curPortLines[0];
                        SetLinkBaseSpeed(
                            curLine,
                            baseQuantity * q.ValueCurrentInGeneral / period);

                        // 更新对方节点相关联的连线；
                        if (curLine.link.nodeNext is ProcessingChains.ProcessingNode)
                        {
                            ProcessingChains.ProcessingNode optPNode
                                = (ProcessingChains.ProcessingNode)curLine.link.nodeNext;
                            if (optPNode.ui is ProcessPanel)
                            {
                                foreach (LinkLineV2 l in GetLinkLines((ProcessPanel)optPNode.ui, true, curLine.link.thing))
                                {
                                    if (curLine != l)
                                    {
                                        l.Update();
                                    }
                                }
                            }
                        }
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
        public List<LinkLineV2> GetLinkLines(ProcessPanel pPanel, bool onInputOrOutput, Things.ThingBase onThing)
        {
            List<LinkLineV2> result = new List<LinkLineV2>();
            List<ProcessingChains.ProcessingLink> linkList;
            if (onInputOrOutput)
            {
                linkList = pPanel.processNode.FindPrevLinks(onThing.id);
            }
            else
            {
                linkList = pPanel.processNode.FindNextLinks(onThing.id);
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
        public List<LinkLineV2> GetLinkLines(ProcessPanel pPanel, bool onInputOrOutput)
        {
            List<LinkLineV2> result = new List<LinkLineV2>();
            List<ProcessingChains.ProcessingLink> linkList;
            if (onInputOrOutput)
            {
                linkList = pPanel.processNode.linksPrev;
            }
            else
            {
                linkList = pPanel.processNode.linksNext;
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

        public void SetLinkBaseQuantity(LinkLineV2 linkLine, decimal baseQuantity, bool update = true)
        {
            linkLine.link.baseQuantity
                = linkLine.link.calQuantity
                = baseQuantity;
            if (update)
            {
                linkLine.Update();

                // update related lines
                ProcessingChains.ProcessingHead pHead;
                if (linkLine.link.nodePrev is ProcessingChains.ProcessingHead)
                {
                    pHead = (ProcessingChains.ProcessingHead)linkLine.link.nodePrev;
                    UpdateOtherLinkLines(pHead.linksNext, linkLine.link);
                }
                else if (linkLine.link.nodeNext is ProcessingChains.ProcessingHead)
                {
                    pHead = (ProcessingChains.ProcessingHead)linkLine.link.nodeNext;
                    UpdateOtherLinkLines(pHead.linksPrev, linkLine.link);
                }
                ProcessingChains.ProcessingNode pNode;
                Guid thingId = linkLine.link.thing.id;
                if (linkLine.link.nodePrev is ProcessingChains.ProcessingNode)
                {
                    pNode = (ProcessingChains.ProcessingNode)linkLine.link.nodePrev;
                    UpdateOtherLinkLines(pNode.FindNextLinks(thingId), linkLine.link);
                }
                else if (linkLine.link.nodeNext is ProcessingChains.ProcessingNode)
                {
                    pNode = (ProcessingChains.ProcessingNode)linkLine.link.nodeNext;
                    UpdateOtherLinkLines(pNode.FindPrevLinks(thingId), linkLine.link);
                }
            }


            void UpdateOtherLinkLines(
                List<ProcessingChains.ProcessingLink> linkList,
                ProcessingChains.ProcessingLink selfLink)
            {
                foreach (ProcessingChains.ProcessingLink pl in linkList)
                {
                    if (selfLink == pl)
                    {
                        continue;
                    }
                    if (pl.ui is LinkLineV2)
                    {
                        ((LinkLineV2)pl.ui).Update();
                    }
                }
            }
        }
        public void SetLinkBaseSpeed(LinkLineV2 linkLine, decimal baseSpeed, bool update = true)
        {
            SetLinkBaseQuantity(linkLine, baseSpeed / linkLine.link.GetChannelSpeed(), update);
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


        public class NotePanel : MovablePanel, IPositionNSize
        {
            FlowGraphAlpha_Manu parent;

            Grid gridContent = new Grid();
            public NotePanel(FlowGraphAlpha_Manu parent)
            {
                //Background = parent.basicConfig.PanelBrushProcess;
                this.parent = parent;
                IsSelectable = true;
                Background = Brushes.PaleGoldenrod;

                Width = 150;
                Title = "Note";

                // text block
                tbvContent = new TextBlock()
                {
                    Text = "Content...",
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                    VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
                };
                gridContent.Children.Add(tbvContent);
                sp_main.Children.Add(gridContent);
                parent.mainPanel.Children.Add(this);

                // line
                //SetLineEnd(this, null);
                //Update();

                // events
                SelectChanged += NotePanel_SelectChanged;
                TitleMouseDown += NotePanel_TitleMouseDown;
                tbvContent.MouseLeftButtonDown += TbvContent_MouseLeftButtonDown;
            }




            #region edit title
            private DateTime preTitleMouseDownTime = DateTime.MinValue;
            private void NotePanel_TitleMouseDown(MovablePanel obj)
            {
                DateTime now = DateTime.Now;
                if ((now - preTitleMouseDownTime).TotalMilliseconds <= MouseNKeyboardHelper.MouseDoubleClickTime)
                {
                    StartEditTitle();
                }
                preTitleMouseDownTime = now;
            }
            private TextBox? titleTextEditor = null;
            private string? preTitleText;
            public async void StartEditTitle()
            {
                preTitleText = Title;
                if (titleTextEditor is null)
                {
                    titleTextEditor = new TextBox()
                    {
                        AcceptsReturn = false,
                    };
                    SyncTextStyles(ref tb_title, ref titleTextEditor, true);
                    Thickness margin = tb_title.Margin;
                    margin.Top -= 1;
                    margin.Bottom -= 1;
                    titleTextEditor.Margin = margin;
                    titleTextEditor.PreviewKeyDown += TbTitle_PreviewKeyDown;
                    titleTextEditor.LostFocus += TbTitle_LostFocus;
                }
                await Task.Delay(20);
                titleTextEditor.Text = Title;
                grid_title.Children.Add(titleTextEditor);
                titleTextEditor.Focus();
                TextBoxSelectionToEnd(ref titleTextEditor);
                isEndEditTitle = false;
            }

            private void TbTitle_LostFocus(object sender, RoutedEventArgs e)
            {
                EndEditTitle();
            }
            private void TbTitle_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
            {
                switch (e.Key)
                {
                    case System.Windows.Input.Key.Enter:
                        EndEditTitle();
                        break;
                    case System.Windows.Input.Key.Escape:
                        EndEditTitle(false);
                        break;
                }
            }

            private bool isEndEditTitle = true;
            public void EndEditTitle(bool confirm = true)
            {
                if (isEndEditTitle)
                {
                    return;
                }
                isEndEditTitle = true;
                grid_title.Children.Remove(titleTextEditor);
                if (confirm)
                {
                    Title = titleTextEditor.Text;
                }
                else
                {
                    Title = preTitleText;
                }
            }
            private void SyncTextStyles(ref TextBlock tbv, ref TextBox tb, bool tbvToTb)
            {
                if (tbvToTb)
                {
                    tb.TextAlignment = tbv.TextAlignment;
                    tb.FontFamily = tbv.FontFamily;
                    tb.FontSize = tbv.FontSize;
                    tb.FontStyle = tbv.FontStyle;
                    tb.FontWeight = tbv.FontWeight;
                    tb.FontStretch = tbv.FontStretch;
                }
                else
                {
                    tbv.TextAlignment = tb.TextAlignment;
                    tbv.FontFamily = tb.FontFamily;
                    tbv.FontSize = tb.FontSize;
                    tbv.FontStyle = tb.FontStyle;
                    tbv.FontWeight = tb.FontWeight;
                    tbv.FontStretch = tb.FontStretch;
                }
            }
            #endregion



            #region edit content

            private TextBlock tbvContent;
            public string? ContentText
            {
                get => tbvContent.Text;
                set => tbvContent.Text = value;
            }
            private DateTime preContentMouseDownTime = DateTime.MinValue;
            private void TbvContent_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
            {
                DateTime now = DateTime.Now;
                if ((now - preContentMouseDownTime).TotalMilliseconds <= MouseNKeyboardHelper.MouseDoubleClickTime)
                {
                    StartEditContent();
                }
                preContentMouseDownTime = now;
            }

            private TextBox? contentTextEditor = null;
            private string? preContentText;
            public async void StartEditContent()
            {
                preContentText = ContentText;
                if (contentTextEditor is null)
                {
                    contentTextEditor = new TextBox()
                    {
                        AcceptsReturn = true,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                    };
                    SyncTextStyles(ref tbvContent, ref contentTextEditor, true);
                    Thickness margin = tbvContent.Margin;
                    margin.Top -= 1; margin.Bottom -= 1;
                    margin.Left -= 1; margin.Right -= 1;
                    contentTextEditor.Margin = margin;
                    contentTextEditor.PreviewKeyDown += TbContent_PreviewKeyDown;
                    contentTextEditor.LostFocus += TbContent_LostFocus;
                }
                await Task.Delay(20);
                contentTextEditor.Text = ContentText;
                gridContent.Children.Add(contentTextEditor);
                contentTextEditor.Focus();
                TextBoxSelectionToEnd(ref contentTextEditor);
                isEndEditContent = false;
            }
            private void TextBoxSelectionToEnd(ref TextBox tb)
            {
                tb.ScrollToEnd();
                tb.SelectionLength = 0;
                tb.SelectionStart = tb.Text is null ? 0 : tb.Text.Length;
            }
            private void TbContent_LostFocus(object sender, RoutedEventArgs e)
            {
                EndEditContent();
            }
            private void TbContent_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
            {
                switch (e.Key)
                {
                    case System.Windows.Input.Key.Escape:
                        EndEditContent(false);
                        break;
                }
            }
            private bool isEndEditContent = true;
            public void EndEditContent(bool confirm = true)
            {
                if (isEndEditContent)
                {
                    return;
                }
                isEndEditContent = true;
                gridContent.Children.Remove(contentTextEditor);
                if (confirm)
                {
                    ContentText = contentTextEditor.Text;
                }
                else
                {
                    ContentText = preContentText;
                }
            }
            #endregion


            #region line display
            private SolidColorBrush _LineColorBrush = Brushes.DarkBlue;
            public SolidColorBrush LineColorBrush
            {
                get => _LineColorBrush;
                set
                {
                    _LineColorBrush = value;
                    Update();
                }
            }
            private SolidColorBrush _HandleColorBrush = Brushes.GreenYellow;
            public SolidColorBrush HandleColorBrush
            {
                get => _HandleColorBrush;
                set
                {
                    _HandleColorBrush = value;
                    Update();
                }
            }
            private double _LineThickness = 2d;
            public double LineThickness
            {
                get => _LineThickness;
                set
                {
                    _LineThickness = value;
                    Update();
                }
            }
            private double _DotDiameter = 5d;
            public double DotDiameter
            {
                get => _DotDiameter;
                set
                {
                    _DotDiameter = value;
                    Update();
                }
            }


            public UIElement? optUi = null;
            public Point? optRefPt = null;

            public void SetLineEnd(UIElement? optUi, Point? optRefPt)
            {
                this.optUi = optUi;
                this.optRefPt = optRefPt;
                Update();
            }

            private Line? uiLine = null;
            private Ellipse? uiDot = null;

            public async void Update()
            {
                // get end points
                Point ptEnd = new Point();
                if (optUi is not null)
                {
                    if (optUi is ThingWithLabel)
                    {
                        ThingWithLabel ui = (ThingWithLabel)optUi;
                        if (parent.inputPanel.ContainsHeadPanel(optUi))
                        {
                            SetCenterPt_Bounds(parent.inputPanel.GetSubPanelBoundsRect(ui));
                            AddRefPt(ui.ActualWidth, ui.ActualHeight);
                        }
                        else if (parent.outputPanel.ContainsHeadPanel(optUi))
                        {
                            SetCenterPt_Bounds(parent.outputPanel.GetSubPanelBoundsRect(ui));
                            AddRefPt(ui.ActualWidth, ui.ActualHeight);
                        }
                        else if (parent.mainPanel.Children.Contains(optUi))
                        {
                            SetCenterPt(ui);
                            AddRefPt(ui.ActualWidth, ui.ActualHeight);
                        }
                        else
                        {
                            optUi = this;
                        }
                    }
                    else if (optUi is LinkLine)
                    {
                        LinkLine ui = (LinkLine)optUi;
                        if (parent.linkLineList.Contains(ui))
                        {
                            ptEnd.X = ui.CenterX;
                            ptEnd.Y = ui.CenterY;
                            AddRefPt(ui.ActualWidth, ui.ActualHeight);
                        }
                        else
                        {
                            optUi = this;
                        }
                    }
                    else if (optUi is ProcessPanel)
                    {
                        ProcessPanel ui = (ProcessPanel)optUi;
                        if (parent.processPanelList.Contains(ui) == false)
                        {
                            optUi = this;
                        }
                    }
                    else if (optUi is NotePanel)
                    {
                        NotePanel ui = (NotePanel)optUi;
                        if (parent.notePanelList.Contains(ui) == false)
                        {
                            optUi = this;
                        }
                    }

                    if (optUi is MovablePanel)
                    {
                        MovablePanel ui = (MovablePanel)optUi;
                        if (ui == this)
                        {
                            ptEnd.X = Canvas.GetLeft(ui) + ui.ActualWidth + 12;
                            ptEnd.Y = Canvas.GetTop(ui) + ui.ActualHeight / 2;
                        }
                        else
                        {
                            SetCenterPt(ui);
                            AddRefPt(ui.ActualWidth, ui.ActualHeight);
                        }
                    }
                }
                else
                {
                    if (optRefPt is null)
                    {
                        ptEnd.X = Canvas.GetLeft(this) + this.ActualWidth + 12;
                        ptEnd.Y = Canvas.GetTop(this) + this.ActualHeight / 2;
                    }
                    else
                    {
                        SetCenterPt(this);
                        AddRefPt(0, 0);
                    }
                }
                void SetCenterPt(Control ui)
                {
                    ptEnd.X = Canvas.GetLeft(ui) + ui.ActualWidth / 2;
                    ptEnd.Y = Canvas.GetTop(ui) + ui.ActualHeight / 2;
                }
                void SetCenterPt_Bounds(RectangleF boundry)
                {
                    ptEnd.X = boundry.X + boundry.Width / 2;
                    ptEnd.Y = boundry.Y + boundry.Height / 2;
                }
                void AddRefPt(double parentWidth, double parentHeight)
                {
                    if (optRefPt is not null)
                    {
                        if (parentWidth > 0)
                        {
                            double halfW = parentWidth / 2;
                            if (optRefPt.Value.X < -halfW)
                            {
                                ptEnd.X -= halfW;
                            }
                            else if (halfW < optRefPt.Value.X)
                            {
                                ptEnd.X += halfW;
                            }
                            else
                            {
                                ptEnd.X += optRefPt.Value.X;
                            }
                        }
                        else
                        {
                            ptEnd.X += optRefPt.Value.X;
                        }
                        if (parentHeight > 0)
                        {
                            double halfH = parentHeight / 2;
                            if (optRefPt.Value.Y < -halfH)
                            {
                                ptEnd.Y -= halfH;
                            }
                            else if (halfH < optRefPt.Value.Y)
                            {
                                ptEnd.Y += halfH;
                            }
                            else
                            {
                                ptEnd.Y += optRefPt.Value.Y;
                            }
                        }
                        else
                        {
                            ptEnd.Y += optRefPt.Value.Y;
                        }
                    }
                }

                // draw line
                if (uiLine is null)
                {
                    uiLine = new Line();
                    parent.mainPanel.Children.Insert(0, uiLine);
                }
                uiLine.Stroke = LineColorBrush;
                uiLine.StrokeThickness = LineThickness;
                while (IsRendering)
                {
                    await Task.Delay(20);
                }
                uiLine.X1 = CenterX;
                uiLine.Y1 = CenterY;
                uiLine.X2 = ptEnd.X;
                uiLine.Y2 = ptEnd.Y;

                // draw end dot
                if (uiDot is null)
                {
                    uiDot = new Ellipse();
                    parent.mainPanel.Children.Insert(1, uiDot);
                }
                uiDot.Fill = LineColorBrush;
                uiDot.Width = DotDiameter;
                uiDot.Height = DotDiameter;
                double dotDiameter_div2 = DotDiameter / 2;
                Canvas.SetLeft(uiDot, ptEnd.X - dotDiameter_div2);
                Canvas.SetTop(uiDot, ptEnd.Y - dotDiameter_div2);

                if (lineHandle is not null
                    && lineHandle.Visibility == Visibility.Visible)
                {
                    double halfW = lineHandle.ActualWidth / 2;
                    Canvas.SetLeft(lineHandle, ptEnd.X - halfW);
                    Canvas.SetTop(lineHandle, ptEnd.Y - halfW);
                }
            }
            internal void RemoveLine()
            {
                parent.mainPanel.Children.Remove(lineHandle);
                parent.mainPanel.Children.Remove(uiLine);
                parent.mainPanel.Children.Remove(uiDot);
            }
            #endregion


            #region select, move line

            public bool LineHandleIsMouseOver
            {
                get
                {
                    if (lineHandle is not null
                        && lineHandle.IsMouseOver)
                    {
                        return true;
                    }
                    return false;
                }
            }
            public Action<NotePanel> LineHandleLeftMouseOver;
            public Action<NotePanel> LineHandleLeftMouseDown;
            public Ellipse? lineHandle;
            private void NotePanel_SelectChanged(MovablePanel mp)
            {
                if (mp != this)
                {
                    return;
                }
                if (lineHandle is null)
                {
                    lineHandle = new Ellipse();
                    lineHandle.Cursor = System.Windows.Input.Cursors.ScrollAll;
                    lineHandle.MouseLeftButtonDown += LineHandle_MouseLeftButtonDown;
                }
                lineHandle.Fill = HandleColorBrush;
                double handleWidth = DotDiameter + 8;
                lineHandle.Width = handleWidth;
                lineHandle.Height = handleWidth;

                if (mp.IsSelected)
                {
                    lineHandle.Visibility = Visibility.Visible;
                    double offset = (DotDiameter - handleWidth) / 2;
                    Canvas.SetLeft(lineHandle, Canvas.GetLeft(uiDot) + offset);
                    Canvas.SetTop(lineHandle, Canvas.GetTop(uiDot) + offset);
                    parent.mainPanel.Children.Add(lineHandle);
                }
                else
                {
                    parent.mainPanel.Children.Remove(lineHandle);
                    lineHandle.Visibility = Visibility.Collapsed;
                }


            }


            private void LineHandle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
            {
                LineHandleLeftMouseDown?.Invoke(this);
            }
            #endregion


            #region X, Y, centerX, centerY, width, height, boundsRect
            public double CenterX
            {
                get => Canvas.GetLeft(this) + ActualWidth / 2;
                set => Canvas.SetLeft(this, value - ActualWidth / 2);
            }
            public double CenterY
            {
                get => Canvas.GetTop(this) + ActualHeight / 2;
                set => Canvas.SetTop(this, value - ActualHeight / 2);
            }
            double IPositionNSize.X
            {
                get => Canvas.GetLeft(this);
                set => Canvas.SetLeft(this, value);
            }
            double IPositionNSize.Y
            {
                get => Canvas.GetTop(this);
                set => Canvas.SetTop(this, value);
            }

            public new double Width
            {
                set => base.Width = value;
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
                set => base.Height = value;
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
            public System.Drawing.RectangleF GetBoundsRect()
            {
                return new System.Drawing.RectangleF(
                    (float)X, (float)Y,
                    (float)Width, (float)Height
                    );
            }

            #endregion


            public bool IsRendering
            {
                get
                {
                    if (this.ActualWidth <= 0)
                    {
                        return true;
                    }
                    return false;







                }
            }

            public Color BackgroundColor
            {
                get => ((SolidColorBrush)Background).Color;
                set => Background = new SolidColorBrush(value);
            }
            public Color ForegroundColor
            {
                get => ((SolidColorBrush)tb_title.Foreground).Color;
                set
                {
                    SolidColorBrush scb = new SolidColorBrush(value);
                    tb_title.Foreground = scb;
                    tbvContent.Foreground = scb;
                }
            }

            public new FontFamily FontFamily
            {
                get => tbvContent.FontFamily;
                set
                {
                    tb_title.FontFamily = value;
                    tbvContent.FontFamily = value;
                }
            }
            public new double FontSize
            {
                get => tbvContent.FontSize;
                set
                {
                    tb_title.FontSize = value * 1.2;
                    tbvContent.FontSize = value;
                }
            }
            public new FontWeight FontWeight
            {
                get => tbvContent.FontWeight;
                set
                {

                    tb_title.FontWeight = FontWeights.Bold;
                    tbvContent.FontWeight = value;
                }
            }
            public new FontStretch FontStretch
            {
                get => tbvContent.FontStretch;
                set
                {

                    tb_title.FontStretch = value;
                    tbvContent.FontStretch = value;
                }
            }
            public new FontStyle FontStyle
            {
                get => tbvContent.FontStyle;
                set
                {
                    tb_title.FontStyle = value;
                    tbvContent.FontStyle = value;
                }
            }
            public TextDecorationCollection TextDecoration
            {
                get => tbvContent.TextDecorations;
                set
                {
                    tb_title.TextDecorations = value;
                    tbvContent.TextDecorations = value;
                }
            }
        }
    }

}
