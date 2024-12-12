using MadTomDev.App.Classes;
using MadTomDev.App.SubWindows;
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

namespace MadTomDev.App.Ctrls
{
    /// <summary>
    /// Interaction logic for PortSpeed.xaml
    /// </summary>
    public partial class PortSpeed : UserControl
    {
        public PortSpeed()
        {
            InitializeComponent();
        }

        public Action<PortSpeed, decimal> ValueChanged;
        private ProcessingChains.ProcessingNode pNode;
        public Recipes.Recipe.PIOItem portItem;
        private bool isIniting = false;
        public void SetPort(
            ProcessingChains.ProcessingNode pNode,
            bool onInputOrOutput,
            int portIndex,
            Core.DefaultSpeedUnits speedUnit)
        {
            isIniting = true;
            this.pNode = pNode;
            OnInputOrOutput = onInputOrOutput;
            List<ProcessingChains.ProcessingLink> pNodeLinkList;
            if (onInputOrOutput)
            {
                pNodeLinkList = pNode.linksPrev;
                portItem = pNode.recipe.inputs[portIndex];
            }
            else
            {
                pNodeLinkList = pNode.linksNext;
                portItem = pNode.recipe.outputs[portIndex];
            }
            SpeedUnit = speedUnit;
            ProcessingChains.ProcessingLink curLink;
            sp_linkSpeeds.Children.Clear();
            PortSpeed_linkSpeed outerSpeedUi;
            if (portItem.thing is not null)
            {
                for (int i = 0, iv = pNodeLinkList.Count; i < iv; ++i)
                {
                    curLink = pNodeLinkList[i];
                    if (curLink.thing.id == portItem.thing.id)
                    {
                        outerSpeedUi = new PortSpeed_linkSpeed();
                        outerSpeedUi.SetData(onInputOrOutput, curLink);
                        sp_linkSpeeds.Children.Add(outerSpeedUi);
                    }
                }
            }
            nud_speedNew.Value = WindowNodeSpeed.GetQuantityValue_inCurUnit(
                pNode.GetPortSpeed_perSec(OnInputOrOutput, portItem));
            UpdateSpeeds();
            isIniting = false;
        }
        internal void UpdateSpeeds()
        {
            PortSpeed_linkSpeed ls;
            foreach (UIElement u in sp_linkSpeeds.Children)
            {
                if (u is not PortSpeed_linkSpeed)
                {
                    continue;
                }
                ls = (PortSpeed_linkSpeed)u;
                ls.UpdateUI();
            }
            if (portItem.quantity is null)
            {
                throw new NullReferenceException("Port item quantity is NULL.");
            }

            tbv_speedOri.Text = WindowNodeSpeed.GetQuantityText_inCurUnit(
                pNode.GetPortSpeed_perSec(OnInputOrOutput, portItem, pNode.baseQuantity));
            //if (nud_speedNew.Value != 0 && unitChanged)
            //{
            //    Core core = Core.Instance;
            //    decimal changedMul = core.GetChangedUnitMultiple();
            //    if (changedMul != 1)
            //    {
            //        nud_speedNew.Value *= changedMul;
            //    }
            //}
            nud_speedNew.Value = WindowNodeSpeed.GetQuantityValue_inCurUnit(GetBaseSpeed_perSec()) * pNode.calQuantity;

            // if all speed match, set color
            UpdateBackground();
        }
        internal void SetNewSpeeds_byQuantity(decimal newQuantity)
        {
            if (portItem.quantity is null
                || pNode.recipe is null)
            {
                return;
            }
            if (pNode.recipe.period is null)
            {
                return;
            }
            nud_speedNew.Value
                = WindowNodeSpeed.GetQuantityValue_inCurUnit(
                    portItem.quantity.ValueCurrentInGeneral * newQuantity / pNode.recipe.period.Value);

            UpdateBackground();
        }
        public void UpdateBackground()
        {
            decimal speedNeeded = WindowNodeSpeed.GetQuantityValue_inCurUnit(GetTotalLinkSpeed_perSec());
            decimal speedOffered = nud_speedNew.Value;
            if (speedNeeded > 0
                && Math.Abs(speedNeeded - speedOffered) < ProcessingChains.aboutZero)
            {
                bdr_main.Background = Core.ColorBrushChannel;
            }
            else if (_OnInputOrOutput)
            {
                bdr_main.Background = Core.ColorBrushInput;
            }
            else // _OnInputOrOutput == false
            {
                bdr_main.Background = Core.ColorBrushOutput;
            }
        }
        internal decimal GetBaseSpeed_perSec()
        {
            if (portItem.quantity is null
                || pNode.recipe.period is null)
            {
                return 0;
            }
            return portItem.quantity.ValueCurrentInGeneral / pNode.recipe.period.Value;
        }

        internal decimal GetTotalLinkSpeed_perSec()
        {
            // sum of all links of this port, in units/sec
            decimal total = 0;
            PortSpeed_linkSpeed ls;
            foreach (UIElement u in sp_linkSpeeds.Children)
            {
                if (u is not PortSpeed_linkSpeed)
                {
                    continue;
                }
                ls = (PortSpeed_linkSpeed)u;
                if (ls.link is null)
                {
                    continue;
                }
                total += ls.link.GetBaseSpeed();
            }
            return total;
        }


        private static GridLength gridColumnWidth_port = new GridLength(140);
        //private static GridLength gridColumnWidth_btn = new GridLength(60);
        private static GridLength gridColumnWidth_links = new GridLength(100);
        public bool _OnInputOrOutput = false;
        public bool OnInputOrOutput
        {
            get => _OnInputOrOutput;
            set
            {
                if (_OnInputOrOutput == value)
                {
                    return;
                }
                _OnInputOrOutput = value;
                if (value)
                {
                    grid_main.ColumnDefinitions[0].Width = gridColumnWidth_links;
                    //grid_main.ColumnDefinitions[1].Width = gridColumnWidth_btn;
                    grid_main.ColumnDefinitions[2].Width = gridColumnWidth_port;
                    Grid.SetColumn(grid_port, 2);
                    Grid.SetColumn(sp_linkSpeeds, 0);
                }
                else
                {
                    grid_main.ColumnDefinitions[0].Width = gridColumnWidth_port;
                    //grid_main.ColumnDefinitions[1].Width = gridColumnWidth_btn;
                    grid_main.ColumnDefinitions[2].Width = gridColumnWidth_links;
                    Grid.SetColumn(grid_port, 0);
                    Grid.SetColumn(sp_linkSpeeds, 2);
                }
                UpdateBackground();
            }
        }

        private Core.DefaultSpeedUnits _SpeedUnit = Core.DefaultSpeedUnits.UnitsPerSec;
        public Core.DefaultSpeedUnits SpeedUnit
        {
            get => _SpeedUnit;
            set
            {
                if (_SpeedUnit == value)
                {
                    return;
                }
                _SpeedUnit = value;
                UpdateSpeeds();
            }
        }

        private void btn_match_Click(object sender, RoutedEventArgs e)
        {
            nud_speedNew.Value = WindowNodeSpeed.GetQuantityValue_inCurUnit(GetTotalLinkSpeed_perSec());
        }

        private void nud_speedNew_ValueChanged(UI.NumericUpDown sender)
        {
            if (isIniting)
            {
                return;
            }
            ValueChanged?.Invoke(this, nud_speedNew.Value);
        }

    }
}
