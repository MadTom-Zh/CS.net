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
    /// Interaction logic for PortSpeed_linkSpeed.xaml
    /// </summary>
    public partial class PortSpeed_linkSpeed : UserControl
    {
        public PortSpeed_linkSpeed()
        {
            InitializeComponent();
        }

        private bool onInputOrOutput;
        public ProcessingChains.ProcessingLink link;
        public void SetData(
            bool onInputOrOutput,
            ProcessingChains.ProcessingLink link)
        {
            this.onInputOrOutput = onInputOrOutput;
            this.link = link;
        }
        public void UpdateUI()
        {
            if (link is null )
            {
                tbv_value.Text = "";
                return;
            }

            ProcessingChains.ProcessingNodeBase optNode;
            List<ProcessingChains.ProcessingLink> optNodeLinks;
            if (onInputOrOutput)
            {
                optNode = link.nodePrev;
                optNodeLinks = optNode.linksNext.FindAll(a=> a.thing.id == link.thing.id).ToList();
            }
            else
            {
                optNode=link.nodeNext;
                optNodeLinks = optNode.linksPrev.FindAll(a => a.thing.id == link.thing.id).ToList();
            }
            if (optNodeLinks.Count > 1)
            {
                this.Background = Brushes.Yellow;
                tbv_value.Text = WindowNodeSpeed.GetQuantityText_inCurUnit(link.GetBaseSpeed()) + " (~)";
            }
            else
            {
                this.Background = Brushes.Transparent;
                tbv_value.Text = WindowNodeSpeed.GetQuantityText_inCurUnit(link.GetBaseSpeed());
            }
            tbv_unit.Text = WindowNodeSpeed.GetSpeedUnit(Core.Instance.DefaultSpeedUnit);
        }
    }
}
