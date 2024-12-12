using MadTomDev.App.Classes;
using MadTomDev.App.Ctrls;
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
using System.Windows.Shapes;

namespace MadTomDev.App.SubWindows
{
    /// <summary>
    /// Interaction logic for WindowLinkSpeed.xaml
    /// </summary>
    public partial class WindowLinkSpeed : Window
    {
        public WindowLinkSpeed()
        {
            InitializeComponent();
        }
        public WindowLinkSpeed(
            FlowGraphAlpha_Manu graph,
            FlowGraphAlpha_Manu.LinkLineV2 linkLine,
            bool applyByOkBtn = true)
        {
            this.graph = graph;
            this.linkLine = linkLine;
            this.applyByOkBtn = applyByOkBtn;
            InitializeComponent();
        }
        private Core core = Core.Instance;
        FlowGraphAlpha_Manu graph;
        public FlowGraphAlpha_Manu.LinkLineV2 linkLine;
        private bool isIniting = false;

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await core.SetCursorWait(this);
            isIniting = true;

            // load info
            ReLoadMainPanel();

            // load inputs
            ReLoadLinkPanels(true);

            // load outputs
            ReLoadLinkPanels(false);


            isIniting = false;
            core.SetCursorArrow(this);
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    btn_ok_Click(sender, e);
                }
            }
        }




        #region center panel

        private void ReLoadMainPanel()
        {
            switch (core.DefaultSpeedUnit)
            {
                case Core.DefaultSpeedUnits.UnitsPerSec:
                    rb_UPSec.IsChecked = true;
                    break;
                case Core.DefaultSpeedUnits.UnitsPerMin:
                    rb_UPMin.IsChecked = true;
                    break;
                case Core.DefaultSpeedUnits.UnitsPerHour:
                    rb_UPHour.IsChecked = true;
                    break;
            }

            if (linkLine is null)
            {
                ClearSidePanels(true);
                ClearSidePanels(false);
                twl_channel.ThingBase = null;
                return;
            }

            twl_channel.ThingBase = linkLine.link.thing;

            tbv_oriQuantity.Text = linkLine.link.baseQuantity.ToString("0.00");
            nud_newQuantity.Value = linkLine.link.calQuantity;
        }

        #region change default speed units
        private void rb_UPSec_Checked(object sender, RoutedEventArgs e)
        {
            if (isIniting) return;
            core.DefaultSpeedUnit = Core.DefaultSpeedUnits.UnitsPerSec;
            SpeedChanger = nud_newQuantity;
            UpdateAllSpeeds();
            SpeedChanger = null;
        }
        private void rb_UPMin_Checked(object sender, RoutedEventArgs e)
        {
            if (isIniting) return;
            core.DefaultSpeedUnit = Core.DefaultSpeedUnits.UnitsPerMin;
            SpeedChanger = nud_newQuantity;
            UpdateAllSpeeds();
            SpeedChanger = null;
        }
        private void rb_UPHour_Checked(object sender, RoutedEventArgs e)
        {
            if (isIniting) return;
            core.DefaultSpeedUnit = Core.DefaultSpeedUnits.UnitsPerHour;
            SpeedChanger = nud_newQuantity;
            UpdateAllSpeeds();
            SpeedChanger = null;
        }
        private void UpdateAllSpeeds()
        {
            if (isIniting) return;

            UpdateSPList(sp_input);
            UpdateSPList(sp_output);

            void UpdateSPList(StackPanel sp)
            {
                PortSpeed_linkSpeed psls;
                LinkSpeed ls;
                foreach (UIElement u in sp.Children)
                {
                    if (u is PortSpeed_linkSpeed)
                    {
                        psls = (PortSpeed_linkSpeed)u;
                        psls.UpdateUI();
                    }
                    else if (u is LinkSpeed)
                    {
                        ls = (LinkSpeed)u;
                        ls.UpdateSpeed();
                    }
                }
            }
        }

        private UIElement? _SpeedChanger = null;
        private bool? SpeedChanger_inInputOrOutput = null;
        private UIElement? SpeedChanger
        {
            set => _SpeedChanger = value;
            get
            {
                // main panel
                if (nud_newQuantity.IsFocused)
                {
                    SpeedChanger_inInputOrOutput = null;
                    return nud_newQuantity;
                }
                if (_SpeedChanger is not null)
                {
                    return _SpeedChanger;
                }
                return null;
            }
        }


        private void nud_newQuantity_ValueChanged(UI.NumericUpDown sender)
        {
            if (isIniting)
            {
                return;
            }
            // change all other(inputs n' outputs) new speeds
            if (linkLine.link.channel is null
                || linkLine.link.channel.speed is null)
            {
                return;
            }
            linkLine.link.calQuantity = nud_newQuantity.Value;
            decimal newSpeed = linkLine.link.GetCalSpeed();
            UIElement? changer = SpeedChanger;
            isIniting = true;
            SetNewSPSpeeds(sp_input);
            SetNewSPSpeeds(sp_output);
            isIniting = false;
            SpeedChanger = null;

            void SetNewSPSpeeds(StackPanel sp)
            {
                UIElement u;
                PortSpeed_linkSpeed psls;
                LinkSpeed ls;
                for (int i = 1, iv = sp.Children.Count; i < iv; ++i)
                {
                    u = sp.Children[i];
                    if (u is PortSpeed_linkSpeed)
                    {
                        psls = (PortSpeed_linkSpeed)u;
                        psls.UpdateUI();
                    }
                    else if (u is LinkSpeed)
                    {
                        ls = (LinkSpeed)u;
                        ls.SetNewSpeed(WindowNodeSpeed.GetQuantityValue_inCurUnit(newSpeed));
                        ls.UpdateBackground();
                    }
                }
            }
        }

        #endregion


        #region bottom buttons
        private void btn_matchSource_Click(object sender, RoutedEventArgs e)
        {
            if (lsPrev.optNodeInfo.linkCount != 1)
            {
                return;
            }
            SpeedChanger = nud_newQuantity;
            decimal nodeSpeed = (lsPrev.optNodeInfo.headSpeed_perSec < 0)
                ? lsPrev.optNodeInfo.portSpeed_perSec
                : lsPrev.optNodeInfo.headSpeed_perSec;
            nud_newQuantity.Value = nodeSpeed / linkLine.link.GetChannelSpeed();
            SpeedChanger = null;
        }

        private void btn_matchTarget_Click(object sender, RoutedEventArgs e)
        {
            if (lsNext.optNodeInfo.linkCount != 1)
            {
                return;
            }
            SpeedChanger = nud_newQuantity;
            decimal nodeSpeed = (lsNext.optNodeInfo.headSpeed_perSec < 0)
                ? lsNext.optNodeInfo.portSpeed_perSec
                : lsNext.optNodeInfo.headSpeed_perSec;
            nud_newQuantity.Value = nodeSpeed / linkLine.link.GetChannelSpeed();
            SpeedChanger = null;
        }

        private bool applyByOkBtn = false;
        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            isApplyToAll = false;
            if (applyByOkBtn)
            {
                Apply(isApplyToAll);
            }
            this.DialogResult = true;
            Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            linkLine.link.calQuantity = linkLine.link.baseQuantity;
            this.DialogResult = false;
            Close();
        }

        public bool isApplyToAll = false;
        private void btn_applyAll_Click(object sender, RoutedEventArgs e)
        {
            isApplyToAll = true;
            if (applyByOkBtn)
            {
                Apply(isApplyToAll);
            }
            this.DialogResult = true;
            Close();
        }
        private void Apply(bool isApplyToAll)
        {
            if (isApplyToAll)
            {
                graph.SetGraphBaseSpeed(linkLine, linkLine.link.calQuantity,
                    out List<FlowGraphAlpha.ProcessPanel> unsetPPanels,
                    out List<FlowGraphAlpha_Manu.LinkLineV2> unsetLinkLines);
                int unsetPanelCount = 0;
                int unsetLineCount = 0;
                if (unsetPPanels is not null && unsetPPanels.Count > 0)
                {
                    unsetPanelCount = unsetPPanels.Count;
                }
                if (unsetLinkLines is not null && unsetLinkLines.Count > 0)
                {
                    unsetLineCount += unsetLinkLines.Count;
                }
                if (unsetPanelCount > 0 || unsetLineCount > 0)
                {
                    MessageBox.Show(this, $"Some({unsetLineCount}) Lines and ({unsetPanelCount}) Nodes are not set.", "Warning",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                graph.SetLinkBaseQuantity(linkLine, linkLine.link.calQuantity);
            }
        }
        #endregion

        #endregion



        #region left panel, input to link

        private LinkSpeed lsPrev, lsNext;
        private void ReLoadLinkPanels(bool onInputOrOutput)
        {
            ClearSidePanels(onInputOrOutput);
            if (linkLine is null) return;

            StackPanel sp;
            ProcessingChains.ProcessingNodeBase optNB;
            ProcessingChains.ProcessingNode? optPNode = null;
            ProcessingChains.ProcessingHead? optHead = null;
            List<ProcessingChains.ProcessingLink> optNodeLinks;
            if (onInputOrOutput)
            {
                sp = sp_input;
                optNB = linkLine.link.nodePrev;
                optNodeLinks = optNB.FindNextLinks(linkLine.link.thing.id);
                btn_matchSource.IsEnabled = optNodeLinks.Count == 1;
            }
            else
            {
                sp = sp_output;
                optNB = linkLine.link.nodeNext;
                optNodeLinks = optNB.FindPrevLinks(linkLine.link.thing.id);
                btn_matchTarget.IsEnabled = optNodeLinks.Count == 1;
            }
            ThingWithLabel topItemLabel = (ThingWithLabel)sp.Children[0];
            topItemLabel.ThingBase = linkLine.link.thing;
            if (optNB is ProcessingChains.ProcessingNode)
            {
                optPNode = (ProcessingChains.ProcessingNode)optNB;
                int portIndex = onInputOrOutput
                    ? optPNode.IndexOfOutput(linkLine.link.thing)
                    : optPNode.IndexOfInput(linkLine.link.thing);
                topItemLabel.SetSpeed_perSec(optPNode.GetPortSpeed_perSec(onInputOrOutput == false, portIndex));

            }
            else if (optNB is ProcessingChains.ProcessingHead)
            {
                optHead = (ProcessingChains.ProcessingHead)optNB;
                topItemLabel.SetSpeed_perSec(optHead.baseQuantity);
            }


            ProcessingChains.ProcessingLink curLink;
            PortSpeed_linkSpeed psls;
            LinkSpeed ls;
            for (int i = 0, iv = optNodeLinks.Count; i < iv; ++i)
            {
                curLink = optNodeLinks[i];
                if (curLink == linkLine.link)
                {
                    ls = new LinkSpeed();
                    ls.SetLink(curLink, onInputOrOutput);
                    ls.UpdateBackground();
                    ls.ValueChanged = LinkSpeed_ValueChanged;
                    if (onInputOrOutput)
                    {
                        lsPrev = ls;
                    }
                    else
                    {
                        lsNext = ls;
                    }
                    sp.Children.Add(ls);
                }
                else
                {
                    psls = new PortSpeed_linkSpeed();
                    psls.SetData(onInputOrOutput, curLink);
                    psls.UpdateUI();
                    sp.Children.Add(psls);
                }

            }
        }
        private void LinkSpeed_ValueChanged(LinkSpeed sender, decimal newValue)
        {
            if (SpeedChanger == nud_newQuantity)
            {
                return;
            }
            SpeedChanger = sender;

            // change main quantity
            nud_newQuantity.Value = newValue / WindowNodeSpeed.GetQuantityValue_inCurUnit(linkLine.link.GetChannelSpeed());
        }

        private void ClearSidePanels(bool onInputOrOutput)
        {
            StackPanel sp;
            if (onInputOrOutput)
            {
                sp = sp_input;
            }
            else
            {
                sp = sp_output;
            }
            for (int i = sp.Children.Count - 1; 1 <= i; --i)
            {
                sp.Children.RemoveAt(i);
            }
        }




        #endregion

        #region right panel

        #endregion

    }
}
