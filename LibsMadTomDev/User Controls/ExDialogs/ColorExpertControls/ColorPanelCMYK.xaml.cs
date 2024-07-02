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

namespace MadTomDev.UI.ColorExpertControls
{
    /// <summary>
    /// Interaction logic for ColorPanelCMYK.xaml
    /// </summary>
    public partial class ColorPanelCMYK : UserControl
    {
        Class.ColorExpertCore core;
        Class.ColorExpertCore.AdjusterCMYK clrAdjuster;
        public ColorPanelCMYK()
        {
            InitializeComponent();
        }
        public void InitFromStaticCore()
        {
            Init(Class.ColorExpertCore.GetInstance());
        }
        public void Init(Class.ColorExpertCore core)
        {
            this.core = core;
            clrAdjuster = core.adjusterCMYK;

            core.WorkingColorIndexChanged += ClrMgr_WorkingColorIndexChanged;
            core.WorkingColorChanged += ClrMgr_WorkingColorChanged;

            sld_c.ValueChanged += Sld_ValueChanged;
            sld_m.ValueChanged += Sld_ValueChanged;
            sld_y.ValueChanged += Sld_ValueChanged;
            sld_k.ValueChanged += Sld_ValueChanged;

            clrAdjuster.SetColor();
            if (clrAdjuster.M == 0)
                _M = 1;
            RefreshPanel();

            IsEnabledChanged += (s1, e1) =>
            {
                if (this.IsEnabled)
                {
                    _C = -1;
                    RefreshPanel();
                }
            };
        }

        private bool isOuterSetting = false;
        double _C, _M, _Y, _K;
        public void RefreshPanel()
        {
            if (_C != clrAdjuster.C || _M != clrAdjuster.M
                || _Y != clrAdjuster.Y || _K != clrAdjuster.K)
            {
                tb_c.Text = clrAdjuster.C.ToString("P0");
                tb_m.Text = clrAdjuster.M.ToString("P0");
                tb_y.Text = clrAdjuster.Y.ToString("P0");
                tb_k.Text = clrAdjuster.K.ToString("P0");

                isOuterSetting = true;
                sld_c.Value = clrAdjuster.C;
                sld_m.Value = clrAdjuster.M;
                sld_y.Value = clrAdjuster.Y;
                sld_k.Value = clrAdjuster.K;
                isOuterSetting = false;

                StringBuilder clrTxBdr = new StringBuilder();
                clrTxBdr.Append(clrAdjuster.C.ToString("0.00"));
                clrTxBdr.Append(", ");
                clrTxBdr.Append(clrAdjuster.M.ToString("0.00"));
                clrTxBdr.Append(", ");
                clrTxBdr.Append(clrAdjuster.Y.ToString("0.00"));
                clrTxBdr.Append(", ");
                clrTxBdr.Append(clrAdjuster.K.ToString("0.00"));
                clrTxBdr.AppendLine();


                Color curColor = core.WorkingColor;
                clrTxBdr.Append(curColor.ToString());
                clrTxBdr.AppendLine();
                clrTxBdr.Append(curColor.A.ToString());
                clrTxBdr.Append(", ");
                clrTxBdr.Append(curColor.R.ToString());
                clrTxBdr.Append(", ");
                clrTxBdr.Append(curColor.G.ToString());
                clrTxBdr.Append(", ");
                clrTxBdr.Append(curColor.B.ToString());
                tb_color.Text = clrTxBdr.ToString();
                tb_color.Background = new SolidColorBrush(curColor);
                tb_color.Foreground = new SolidColorBrush(core.GetBetterForeColor(curColor));
            }

            _C = clrAdjuster.C;
            _M = clrAdjuster.M;
            _Y = clrAdjuster.Y;
            _K = clrAdjuster.K;
        }
        private void ClrMgr_WorkingColorChanged(object sender, int workingColorIndex, object changer)
        {
            if (!IsEnabled)
                return;
            if (sender != this)
                clrAdjuster.SetColor();

            _C = -1;
            RefreshPanel();
        }
        private void ClrMgr_WorkingColorIndexChanged(object sender, int workingColorIndex)
        {
            if (!IsEnabled)
                return;

            clrAdjuster.SetColor();
            _C = -1;
            RefreshPanel();
        }

        private void Sld_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isOuterSetting)
                return;

            if (sender == sld_c)
                clrAdjuster.C = sld_c.Value;
            else if (sender == sld_m)
                clrAdjuster.M = sld_m.Value;
            else if (sender == sld_y)
                clrAdjuster.Y = sld_y.Value;
            else if (sender == sld_k)
                clrAdjuster.K = sld_k.Value;

            clrAdjuster.SetBackWorkingColor();
        }
    }
}
