using CraftingCalculator;
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
    /// Interaction logic for WindowNodeSpeed.xaml
    /// </summary>
    public partial class WindowNodeSpeed : Window
    {
        public WindowNodeSpeed()
        {
            InitializeComponent();
        }
        public WindowNodeSpeed(FlowGraphAlpha_Manu graph, FlowGraphAlpha.ProcessPanel pPanel, bool applyByOkBtn = true)
        {
            this.graph = graph;
            this.pPanel = pPanel;
            this.applyByOkBtn = applyByOkBtn;
            InitializeComponent();
        }

        private Core core = Core.Instance;
        FlowGraphAlpha_Manu graph;
        public FlowGraphAlpha.ProcessPanel pPanel;
        private bool isIniting = false;
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await core.SetCursorWait(this);
            isIniting = true;

            // load info
            ReLoadMainPanel();

            // load inputs
            ReLoadPortPanels(true);

            // load outputs
            ReLoadPortPanels(false);


            isIniting = false;
            await core.SetCursorArrow(this);
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

            if (pPanel is null)
            {
                twl_processor.ThingBase = null;
                return;
            }

            tbv_recipe.Text = pPanel.processNode.recipe.name;
            if (pPanel.processNode.recipe.processor is not null)
            {
                twl_processor.ThingBase = core.FindThing(pPanel.processNode.recipe.processor.Value);
            }
            tbv_oriQuantity.Text = pPanel.processNode.baseQuantity.ToString("0.00");
            nud_newQuantity.Value = pPanel.processNode.baseQuantity;

            sp_accessories.Children.Clear();
            if (pPanel.processNode.recipe.accessories.Count > 0)
            {
                tbv_titleAccessories.Visibility = Visibility.Visible;
                ThingWithLabel aPanel;
                foreach (Recipes.Recipe.PIOItem a in pPanel.processNode.recipe.accessories)
                {
                    aPanel = new ThingWithLabel()
                    {
                        IsCheckable = false,
                        IsNumberLabelsVisible = false,
                        ThingBase = a.thing,
                    };
                    sp_accessories.Children.Add(aPanel);
                }
            }
            else
            {
                tbv_titleAccessories.Visibility = Visibility.Collapsed;
            }

        }
        public static decimal GetQuantityValue_inCurUnit(decimal quantity)
        {
            switch (Core.Instance.DefaultSpeedUnit)
            {
                case Core.DefaultSpeedUnits.UnitsPerMin:
                    return quantity * 60;
                case Core.DefaultSpeedUnits.UnitsPerHour:
                    return quantity * 3600;
                default:
                case Core.DefaultSpeedUnits.UnitsPerSec:
                    return quantity;
            }
        }
        public static decimal GetQuantityValue_perSec(decimal quantityInCurUnit)
        {
            switch (Core.Instance.DefaultSpeedUnit)
            {
                case Core.DefaultSpeedUnits.UnitsPerMin:
                    return quantityInCurUnit / 60;
                case Core.DefaultSpeedUnits.UnitsPerHour:
                    return quantityInCurUnit / 3600;
                default:
                case Core.DefaultSpeedUnits.UnitsPerSec:
                    return quantityInCurUnit;
            }
        }
        public static string GetQuantityText_inCurUnit(decimal quantityPerSec)
        {
            return GetQuantityValue_inCurUnit(quantityPerSec).ToString("0.00");
        }
        public static string GetSpeedUnit(Core.DefaultSpeedUnits speedUnit)
        {
            switch (speedUnit)
            {
                default:
                case Core.DefaultSpeedUnits.UnitsPerSec:
                    return "/s";
                case Core.DefaultSpeedUnits.UnitsPerMin:
                    return "/m";
                case Core.DefaultSpeedUnits.UnitsPerHour:
                    return "/h";
            }
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

            //tbv_oriQuantity.Text = pPanel.processNode.baseQuantity.ToString("0.00");
            //if (nud_newQuantity.Value != 0 && unitChanged)
            //{
            //    decimal mul = core.GetChangedUnitMultiple();
            //    if (mul != 1)
            //    {
            //        nud_newQuantity.Value *= mul;
            //    }
            //}
            UpdateSPList(sp_inputs);
            UpdateSPList(sp_outputs);

            void UpdateSPList(StackPanel sp)
            {
                PortSpeed ps;
                foreach (UIElement u in sp.Children)
                {
                    if (u is not PortSpeed)
                    {
                        continue;
                    }
                    ps = (PortSpeed)u;
                    //ps.UpdateSpeeds(unitChanged);
                    ps.UpdateSpeeds();
                }
            }
        }
        #endregion

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
            decimal newQuantity = nud_newQuantity.Value;
            pPanel.processNode.calQuantity = newQuantity;
            UIElement? changer = SpeedChanger;
            isIniting = true;
            SetNewSPSpeeds(sp_inputs);
            SetNewSPSpeeds(sp_outputs);
            isIniting = false;
            SpeedChanger = null;

            void SetNewSPSpeeds(StackPanel sp)
            {
                PortSpeed ps;
                foreach (UIElement u in sp.Children)
                {
                    if (u is not PortSpeed)
                    {
                        continue;
                    }
                    ps = (PortSpeed)u;
                    if (changer is not null && ps == changer)
                    {
                        ps.UpdateBackground();
                    }
                    else
                    {
                        ps.SetNewSpeeds_byQuantity(newQuantity);
                    }
                }
            }
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
        private void Apply(bool isApplyToAll)
        {
            // change speed of pNode
            pPanel.processNode.baseQuantity = pPanel.processNode.calQuantity;

            // and its links (an outer nodes)
            if (isApplyToAll)
            {
                graph.SetGraphBaseSpeed(pPanel, pPanel.processNode.calQuantity,
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
                    MessageBox.Show(this, $"Some({unsetPanelCount}) Nodes and ({unsetLineCount}) Lines are not set.", "Warning",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                graph.SetNodeBaseQuantity(pPanel, pPanel.processNode.calQuantity, true,
                    out List<FlowGraphAlpha_Manu.LinkLineV2>? unsetPrevLines,
                    out List<FlowGraphAlpha_Manu.LinkLineV2>? unsetNextLines);
                int unsetLineCount = 0;
                if (unsetPrevLines is not null && unsetPrevLines.Count > 0)
                {
                    unsetLineCount += unsetPrevLines.Count;
                }
                if (unsetNextLines is not null && unsetNextLines.Count > 0)
                {
                    unsetLineCount += unsetNextLines.Count;
                }
                if (unsetLineCount > 0)
                {
                    MessageBox.Show(this, $"Some({unsetLineCount}) link-lines are not set.", "Warning",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            pPanel.processNode.calQuantity = pPanel.processNode.baseQuantity;
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

        #endregion

        #region input stack panel
        private void ReLoadPortPanels(bool onInputOrOutput)
        {
            ClearIOPanels(onInputOrOutput);
            if (pPanel is null) return;
            List<Recipes.Recipe.PIOItem> ports;
            StackPanel sp;
            if (onInputOrOutput)
            {
                ports = pPanel.processNode.recipe.inputs;
                sp = sp_inputs;
            }
            else
            {
                ports = pPanel.processNode.recipe.outputs;
                sp = sp_outputs;
            }
            Recipes.Recipe.PIOItem curInput;
            PortSpeed ps;
            for (int i = 0, iv = ports.Count; i < iv; ++i)
            {
                curInput = ports[i];
                ps = new PortSpeed()
                {
                    Tag = curInput,
                };
                ps.SetPort(pPanel.processNode, onInputOrOutput, i, core.DefaultSpeedUnit);
                ps.ValueChanged = PortSpeed_ValueChanged;
                sp.Children.Add(ps);
            }
        }
        private void PortSpeed_ValueChanged(PortSpeed sender, decimal newValue)
        {
            if (SpeedChanger == nud_newQuantity)
            {
                return;
            }
            SpeedChanger = sender;

            // change main quantity
            if (sender.portItem.quantity is null)
            {
                return;
            }

            nud_newQuantity.Value = newValue / GetQuantityValue_inCurUnit(sender.GetBaseSpeed_perSec());
        }
        private void ClearIOPanels(bool onInputOrOutput)
        {
            StackPanel sp;
            if (onInputOrOutput)
            {
                sp = sp_inputs;
            }
            else
            {
                sp = sp_outputs;
            }
            //foreach (UIElement u in sp.Children)
            //{
            //    if (u is not PortSpeed)
            //    {
            //        continue;
            //    }
            //    ((PortSpeed)u).MatchBtnClicked = null;
            //}

            sp.Children.Clear();
        }
        #endregion

        #region output stack panel

        #endregion

    }
}
