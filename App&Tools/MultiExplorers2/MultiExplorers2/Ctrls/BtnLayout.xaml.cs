using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for BtnLayout.xaml
    /// </summary>
    public partial class BtnLayout : UserControl
    {
        public BtnLayout()
        {
            InitializeComponent();
        }
        public Setting.Layout layout;
        internal void Init(Setting.Layout layout)
        {
            this.layout = layout;
            if (layout.sizeClr != Colors.Transparent)
            {
                SetColorSizeTx(layout.sizeClr);
            }
            if (layout.bgClr != Colors.Transparent)
            {
                SetColorBG(layout.bgClr);
            }
            if (layout.foreClr != Colors.Transparent)
            {
                SetColorCharTx(layout.foreClr);
            }
            SetChar(layout.c);
            SetSizeTx($"{layout.size.x}*{layout.size.y}");

            SetToolTipTx(layout.tipTx);
            SetToolTipImg(layout);
        }
        public void SetColorSizeTx(Color sizeTxClr)
        {
            tb_size.Foreground = new SolidColorBrush(sizeTxClr);
        }
        public void SetColorCharTx(Color charTxClr)
        {
            tb_char.Foreground = new SolidColorBrush(charTxClr);
        }
        public void SetColorBG(Color bgClr)
        {
            grid.Background = new SolidColorBrush(bgClr);
        }
        public void SetChar(char c)
        {
            this.tb_char.Text = c.ToString();
        }
        public void SetSizeTx(string tx)
        {
            this.tb_size.Text = tx;
        }
        public void SetToolTipTx(string tx)
        {
            this.tb_tt.Text = tx;
        }


        public void SetToolTipImg(Setting.Layout layoutData)
        {
            thumbnail.Content = null;

            if (layoutData == null)
            {
                tb_tt.Margin = new Thickness(0);
            }
            else
            {
                tb_tt.Margin = new Thickness(3);
                UniformGrid thumUG = new UniformGrid();
                thumUG.Columns = layoutData.size.x;
                thumUG.Rows = layoutData.size.y;
                Core core = Core.GetInstance();
                thumUG.Width = SystemParameters.PrimaryScreenWidth / 5;
                thumUG.Height = SystemParameters.PrimaryScreenHeight / 5;
                Border newBdr;
                Brush thumBdrBrush = new SolidColorBrush(Colors.Gray);
                Setting.Layout.Explorer foundE;
                Path blank;
                for (int c, cv = layoutData.size.x, r = 0, rv = layoutData.size.y;
                    r < rv; r++)
                {
                    for (c = 0; c < cv; c++)
                    {
                        newBdr = new Border()
                        {
                            BorderBrush = thumBdrBrush,
                            BorderThickness = new Thickness(0.5),
                        };
                        foundE = layoutData.explorerList.Find((e) => e.posi.x == c && e.posi.y == r);
                        if (foundE != null)
                        {
                            if (foundE.bgClr != Colors.Transparent)
                                newBdr.Background = new SolidColorBrush(foundE.bgClr);
                        }
                        else
                        {
                            blank = StaticResource.NewPathCross100;
                            blank.Stretch = Stretch.Fill;
                            newBdr.Child = blank;
                        }
                        thumUG.Children.Add(newBdr);
                    }
                }
                thumbnail.Content = thumUG;
            }
        }


        public Action<BtnLayout> ActionClick, ActionRightClick;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ActionClick?.Invoke(this);
        }

        private bool isRMBDown = false;
        private void Button_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            isRMBDown = true;
        }
        private void Button_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (isRMBDown)
            {
                ActionRightClick?.Invoke(this);
            }
            isRMBDown = false;
        }
    }
}
