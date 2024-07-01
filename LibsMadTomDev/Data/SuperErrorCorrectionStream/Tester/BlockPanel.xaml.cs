using MadTomDev.Data;
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
    /// Interaction logic for BlockPanel.xaml
    /// </summary>
    public partial class BlockPanel : UserControl
    {
        public BlockPanel()
        {
            InitializeComponent();
            Data.Block tmpBlock = new Data.Block(3);
            tmpBlock.data = new byte[][]
            {
                new byte[]{222,1,2 },
                new byte[]{1,1,2 },
                new byte[]{0,0,0 },
            };
            tmpBlock.GenerateCorrectingCode();
            SetOriBlock(tmpBlock);
        }

        public bool CanBtnResponse { set; get; } = true;
        public Action blockDataChangedFromBtn;

        public int btnWidth = 32;
        public FontFamily btnFontFamily = new FontFamily("consolas");
        public double btnFontSize = 13;
        public FontWeight btnFontWeight = FontWeights.Bold;

        public Data.Block oriBlock;
        public Data.Block curBlock;
        public void SetOriBlock(Data.Block block)
        {
            oriBlock = block;
            this.curBlock = block.Clone();

            // remove olds
            RemoveBtnListeners();
            allButtons.Clear();
            sPanel_rows.Children.Clear();
            //GC.Collect();

            // add new
            if (block != null)
            {
                StackPanel rowPanel;
                Button btn = null;
                int i, r, c, rv = block.data.Length, cv = block.data[0].Length;
                rowPanel = new StackPanel()
                { Orientation = Orientation.Horizontal, };
                for (c = 0; c < cv; c++)
                {
                    i = cv - c - 1;
                    btn = new Button()
                    {
                        FontFamily = btnFontFamily,
                        FontSize = btnFontSize,
                        FontWeight = btnFontWeight,
                        Width = btnWidth,
                        Height = btnWidth,
                        Content = block.ccA[i],
                        Tag = new Point(i, -3),
                    };
                    btn.Click += Btn_Click;
                    rowPanel.Children.Add(btn);
                    allButtons.Add(btn);
                }
                sPanel_rows.Children.Add(rowPanel);

                for (r = 0; r < rv; r++)
                {
                    rowPanel = new StackPanel()
                    { Orientation = Orientation.Horizontal, };
                    for (c = 0; c < cv; c++)
                    {
                        btn = new Button()
                        {
                            FontFamily = btnFontFamily,
                            FontSize = btnFontSize,
                            FontWeight = btnFontWeight,
                            Width = btnWidth,
                            Height = btnWidth,
                            Content = block.data[r][c],
                            Tag = new Point(c, r),
                        };
                        btn.Click += Btn_Click;
                        rowPanel.Children.Add(btn);
                        allButtons.Add(btn);
                    }
                    btn = new Button()
                    {
                        FontFamily = btnFontFamily,
                        FontSize = btnFontSize,
                        FontWeight = btnFontWeight,
                        Width = btnWidth,
                        Height = btnWidth,
                        Content = block.ccR[r],
                        Tag = new Point(r, -1),
                    };
                    btn.Click += Btn_Click;
                    rowPanel.Children.Add(btn);
                    allButtons.Add(btn);

                    sPanel_rows.Children.Add(rowPanel);
                }

                rowPanel = new StackPanel()
                { Orientation = Orientation.Horizontal, };
                for (c = 0; c < cv; c++)
                {
                    btn = new Button()
                    {
                        FontFamily = btnFontFamily,
                        FontSize = btnFontSize,
                        FontWeight = btnFontWeight,
                        Width = btnWidth,
                        Height = btnWidth,
                        Content = block.ccC[c],
                        Tag = new Point(c, -2),
                    };
                    btn.Click += Btn_Click;
                    rowPanel.Children.Add(btn);
                    allButtons.Add(btn);
                }
                sPanel_rows.Children.Add(rowPanel);

                if (btnOriForeBrush == null)
                {
                    btnOriForeBrush = btn.Foreground;
                    btnOriBGBrush = btn.Background;
                }
            }
        }
        public void SetErrBlock(Data.Block errBlock)
        {
            curBlock = errBlock;
        }

        private Brush btnOriForeBrush = null;
        private Brush btnErrForeBrush = new SolidColorBrush(Colors.DarkRed);
        private Brush btnOriBGBrush = null;
        private List<Button> allButtons = new List<Button>();
        private void RemoveBtnListeners()
        {
            foreach (Button btn in allButtons)
            {
                btn.Click -= Btn_Click;
            }
        }

        private Random rand = new Random((int)DateTime.Now.Ticks);
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            if (!CanBtnResponse)
                return;

            Button btn = (Button)sender;
            Point pt = (Point)btn.Tag;

            int x = (int)pt.X;
            int y = (int)pt.Y;
            if (y == -3)
            {
                ValueSwitch(ref curBlock.ccA[x], ref oriBlock.ccA[x], rand);
            }
            else if (y == -2)
            {
                ValueSwitch(ref curBlock.ccC[x], ref oriBlock.ccC[x], rand);
            }
            else if (y == -1)
            {
                ValueSwitch(ref curBlock.ccR[x], ref oriBlock.ccR[x], rand);
            }
            else
            {
                ValueSwitch(ref curBlock.data[y][x], ref oriBlock.data[y][x], rand);
            }

            void ValueSwitch(ref byte vC, ref byte vO, Random rand)
            {
                if (vC == vO)
                {
                    // set to error value
                    do
                    {
                        vC = (byte)rand.Next(byte.MaxValue + 1);
                    }
                    while (vC == vO);
                }
                else
                {
                    // restore
                    vC = vO;
                }
            }

            RefreshBlockDifferences();
            List<int> eRows = new List<int>();
            List<int> eCols = new List<int>();
            List<int> eAlts = new List<int>();
            Data.Block.CheckBlock(curBlock, ref eRows, ref eCols, ref eAlts);
            SetError(eRows, eCols, eAlts);

            blockDataChangedFromBtn?.Invoke();
        }
        public void RefreshBlockDifferences()
        {
            Point pt;
            int x, y;
            bool isErr;
            foreach (Button btn in allButtons)
            {
                pt = (Point)btn.Tag;
                x = (int)pt.X;
                y = (int)pt.Y;
                if (y == -3)
                {
                    isErr = curBlock.ccA[x] != oriBlock.ccA[x];
                    btn.Content = curBlock.ccA[x];
                }
                else if (y == -2)
                {
                    isErr = curBlock.ccC[x] != oriBlock.ccC[x];
                    btn.Content = curBlock.ccC[x];
                }
                else if (y == -1)
                {
                    isErr = curBlock.ccR[x] != oriBlock.ccR[x];
                    btn.Content = curBlock.ccR[x];
                }
                else
                {
                    isErr = curBlock.data[y][x] != oriBlock.data[y][x];
                    btn.Content = curBlock.data[y][x];
                }
                if (isErr)
                    btn.Foreground = btnErrForeBrush;
                else
                    btn.Foreground = btnOriForeBrush;

            }
        }

        private List<Border> errRowBdrList = new List<Border>();
        private List<Border> errColBdrList = new List<Border>();
        private Brush btnAltErrBrush = new SolidColorBrush(Color.FromArgb(64, 0, 255, 0));
        public void SetError(List<int> eRows, List<int> eCols, List<int> eAlts)
        {
            Point pt;
            int x, y, a;
            bool inR, inC, inA;
            if (eRows != null && eCols != null)
            {
                foreach (Button btn in allButtons)
                {
                    pt = (Point)btn.Tag;
                    x = (int)pt.X;
                    y = (int)pt.Y;

                    inR = eRows.Contains(y);
                    inC = eCols.Contains(x);
                    if (y == -3) // cc a
                    {
                        if (eAlts.Contains(x))
                            //btn.Background = (Brush)FindResource("btnErrBGBrushA");
                            btn.Background = btnAltErrBrush;
                        else
                            btn.Background = btnOriBGBrush;
                    }
                    else if (y == -2) // cc c
                    {
                        if (eCols.Contains(x))
                            btn.Background = (Brush)FindResource("btnErrBGBrushC");
                        else
                            btn.Background = btnOriBGBrush;
                    }
                    else if (y == -1)  // cc r
                    {
                        if (eRows.Contains(x))
                            btn.Background = (Brush)FindResource("btnErrBGBrushR");
                        else
                            btn.Background = btnOriBGBrush;
                    }
                    else
                    {
                        a = IndexerSet.GetInstance().GetDataIndexer(curBlock.dataHeight).GetAltStep(y, x) / curBlock.dataHeight;
                        inA = eAlts.Contains(a);
                        if (inR && inC && inA)
                            btn.Background = (Brush)FindResource("btnErrBGBrushRCA");
                        else if (inR && inC)
                            btn.Background = (Brush)FindResource("btnErrBGBrushRC");
                        else if (inR && inA)
                            btn.Background = (Brush)FindResource("btnErrBGBrushRA");
                        else if (inC && inA)
                            btn.Background = (Brush)FindResource("btnErrBGBrushCA");
                        else if (inR)
                            btn.Background = (Brush)FindResource("btnErrBGBrushR");
                        else if (inC)
                            btn.Background = (Brush)FindResource("btnErrBGBrushC");
                        else if (inA)
                            btn.Background = (Brush)FindResource("btnErrBGBrushA");
                        else
                            btn.Background = btnOriBGBrush;
                    }
                }
            }
            else
            {
                foreach (Button btn in allButtons)
                {
                    btn.Background = btnOriBGBrush;
                }
            }
        }
    }
}
