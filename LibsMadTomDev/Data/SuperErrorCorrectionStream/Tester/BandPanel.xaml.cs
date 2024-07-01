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

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for BandPanel.xaml
    /// </summary>
    public partial class BandPanel : UserControl
    {
        public BandPanel()
        {
            InitializeComponent();
        }

        private bool _CanBtnResponse = true;
        public bool CanBtnResponse
        {
            set
            {
                if (_CanBtnResponse == value)
                    return;
                _CanBtnResponse = value;
                foreach (BlockPanel bp in subBlockList)
                    bp.CanBtnResponse = value;
            }
            get => _CanBtnResponse;
        }

        public Data.Band oriBand, curBand;
        private Random rand = new Random((int)DateTime.Now.Ticks);
        public void SetOriBand(Data.Band oriBand)
        {
            this.oriBand = oriBand;
            ReSetPanels();
        }
        public void SetCurBand(Data.Band curBand, bool showBlockErrDims)
        {
            this.curBand = curBand;
            RefreshDifference(showBlockErrDims);
        }



        private List<BlockPanel> subBlockList = new List<BlockPanel>();
        private void ReSetPanels()
        {
            if (subBlockList.Count > 0)
            {
                if (subBlockList[0].curBlock.dataHeight != oriBand.blocks[0].dataHeight)
                {
                    foreach (BlockPanel bp in subBlockList)
                    {
                        bp.blockDataChangedFromBtn = null;
                    }
                    vsPanel.Children.Clear();
                    subBlockList.Clear();
                }
                else if (subBlockList.Count > oriBand.blocks.Length)
                {
                    int deCount = subBlockList.Count - oriBand.blocks.Length;
                    vsPanel.Children.RemoveRange(0, deCount);
                    subBlockList.RemoveRange(0, deCount);
                }
            }

            BlockPanel newBP;
            int i, iv;
            if (subBlockList.Count > 0)
            {
                for (i = 0, iv = subBlockList.Count; i < iv; i++)
                {
                    subBlockList[i].SetOriBlock(oriBand.blocks[i]);
                }
            }
            for (i = subBlockList.Count, iv = oriBand.BandLength; i < iv; i++)
            {
                newBP = new BlockPanel()
                {
                    CanBtnResponse = CanBtnResponse,
                    blockDataChangedFromBtn = BlockBtnClicked,
                };
                newBP.SetOriBlock(oriBand.blocks[i]);
                vsPanel.Children.Add(newBP);
                subBlockList.Add(newBP);
            }
        }

        private void BlockBtnClicked()
        {
            if (_CanBtnResponse)
            {
                DataChangedFromBtn?.Invoke();
            }
        }
        public Action DataChangedFromBtn;
        private void RefreshDifference(bool showBlockErrDims)
        {
            List<int> rErrs, cErrs, aErrs;
            for (int i = 0, iv = Math.Min(oriBand.BandLength, curBand.BandLength); i < iv; i++)
            {
                subBlockList[i].SetErrBlock(curBand.blocks[i]);
                subBlockList[i].RefreshBlockDifferences();

                if (showBlockErrDims)
                {
                    rErrs = new List<int>();
                    cErrs = new List<int>();
                    aErrs = new List<int>();
                    Data.Block.CheckBlock(subBlockList[i].curBlock, ref rErrs, ref cErrs, ref aErrs);
                    subBlockList[i].SetError(rErrs, cErrs, aErrs);
                }
                else
                {
                    subBlockList[i].SetError(null, null, null);
                }
            }
        }
    }
}
