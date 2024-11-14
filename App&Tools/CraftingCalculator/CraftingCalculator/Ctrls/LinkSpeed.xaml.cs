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
    /// Interaction logic for LinkSpeed.xaml
    /// </summary>
    public partial class LinkSpeed : UserControl
    {
        public LinkSpeed()
        {
            InitializeComponent();
        }

        public Action<LinkSpeed, decimal> ValueChanged;
        private ProcessingChains.ProcessingLink link;
        private bool isIniting = false;
        public void SetLink(
            ProcessingChains.ProcessingLink link,
            bool onInputOrOutput)
        {
            isIniting = true;
            this.link = link;
            OnInputOrOutput = onInputOrOutput;

            UpdateSpeed();
            isIniting = false;
        }
        internal void UpdateSpeed()
        {
            tbv_speedOri.Text = WindowNodeSpeed.GetQuantityText_inCurUnit(link.GetBaseSpeed());
            nud_speedNew.Value = WindowNodeSpeed.GetQuantityValue_inCurUnit(link.GetCalSpeed());

            // if all speed match, set color
            UpdateBackground();
        }
        internal void SetNewSpeed(decimal newSpeed)
        {
            nud_speedNew.Value = newSpeed;

            UpdateBackground();
        }
        public class OptNodeInfo
        {
            public bool onInputOrOutput;
            public ProcessingChains.ProcessingNode? pNode = null;
            public Recipes.Recipe.PIOItem? portItem = null;
            public decimal portSpeed_perSec = -1;
            public ProcessingChains.ProcessingHead? head = null;
            public decimal headSpeed_perSec = -1;
            public int linkCount = 0;
            public decimal linkSpeedTotal = -1;
            public OptNodeInfo(ProcessingChains.ProcessingLink link, bool onInputOrOutput)
            {
                this.onInputOrOutput = onInputOrOutput;
                ProcessingChains.ProcessingNodeBase opeNode;
                if (onInputOrOutput)
                {
                    opeNode = link.nodePrev;
                }
                else
                {
                    opeNode = link.nodeNext;
                }
                if (opeNode is ProcessingChains.ProcessingNode)
                {
                    pNode = (ProcessingChains.ProcessingNode)opeNode;
                    linkSpeedTotal = 0;
                    if (onInputOrOutput)
                    {
                        portItem = pNode.recipe.outputs.Find(
                            a => a.thing is not null && a.thing.id == link.thing.id);
                        foreach (ProcessingChains.ProcessingLink l in pNode.FindNextLinks(link.thing.id))
                        {
                            linkCount++;
                            linkSpeedTotal += l.GetCalSpeed();
                        }
                    }
                    else
                    {
                        portItem = pNode.recipe.inputs.Find(
                            a => a.thing is not null && a.thing.id == link.thing.id);
                        foreach (ProcessingChains.ProcessingLink l in pNode.FindPrevLinks(link.thing.id))
                        {
                            linkCount++;
                            linkSpeedTotal += l.GetCalSpeed();
                        }
                    }
                    if (portItem is not null
                        && portItem.quantity is not null
                        && pNode.recipe.period is not null)
                    {
                        portSpeed_perSec
                            = portItem.quantity.ValueCurrentInGeneral
                                * pNode.baseQuantity
                                / pNode.recipe.period.Value;
                    }
                }
                else if (opeNode is ProcessingChains.ProcessingHead)
                {
                    head = (ProcessingChains.ProcessingHead)opeNode;
                    headSpeed_perSec = head.baseQuantity;
                    linkSpeedTotal = 0;
                    if (onInputOrOutput)
                    {
                        foreach (ProcessingChains.ProcessingLink l in head.linksNext)
                        {
                            linkCount++;
                            linkSpeedTotal += l.GetCalSpeed();
                        }
                    }
                    else
                    {
                        foreach (ProcessingChains.ProcessingLink l in head.linksPrev)
                        {
                            linkCount++;
                            linkSpeedTotal += l.GetCalSpeed();
                        }
                    }
                }
            }
        }
        public OptNodeInfo optNodeInfo;
        public void UpdateBackground()
        {
            optNodeInfo = new OptNodeInfo(link, OnInputOrOutput);
            decimal nodeSpeed = 0;
            if (optNodeInfo.pNode is not null) nodeSpeed = optNodeInfo.portSpeed_perSec;
            else if(optNodeInfo.head is not null) nodeSpeed = optNodeInfo.headSpeed_perSec;
            if (Math.Abs(optNodeInfo.linkSpeedTotal - nodeSpeed) < ProcessingChains.aboutZero)
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
                UpdateSpeed();
            }
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
