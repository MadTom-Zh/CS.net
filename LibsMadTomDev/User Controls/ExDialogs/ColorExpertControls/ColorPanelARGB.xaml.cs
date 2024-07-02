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
    /// Interaction logic for ColorPanelARGB.xaml
    /// </summary>
    public partial class ColorPanelARGB : UserControl
    {
        Class.ColorExpertCore core;
        BitmapSource chessboardImg;
        public ColorPanelARGB()
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
            core.WorkingColorIndexChanged += ClrMgr_WorkingColorIndexChanged;
            core.WorkingColorChanged += ClrMgr_WorkingColorChanged;
            core.WorkingColorWithAlphaChanged += ClrMgr_WorkingColorWithAlphaChanged;
            core.AlphaChanged += ClrMgr_AlphaChanged;

            sld_a.ValueChanged += Sld_ValueChanged;
            sld_r.ValueChanged += Sld_ValueChanged;
            sld_g.ValueChanged += Sld_ValueChanged;
            sld_b.ValueChanged += Sld_ValueChanged;

            chessboardImg = QuickGraphics.Image.GetChessboardImage(40, 40);
            alphaSlider.Init(chessboardImg, false, 1);
            ImageBrush bgBrush = new ImageBrush(chessboardImg)
            {
                TileMode = TileMode.Tile,
                ViewportUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(0, 0, 40, 40),
            };
            grid_bg.Background = bgBrush;
            alphaSlider.SetValueFromSlider = new Action<double>(SetAlphaFromSlider);

            if (core.WorkingColor.R == 0)
                _R = 1;
            RefreshPanel();

            IsEnabledChanged += (s1, e1) =>
            {
                if (this.IsEnabled)
                {
                    if (core.WorkingColor.R == 0)
                        _R = 1;
                    RefreshPanel();
                }
            };
        }



        private bool isOuterSetting = false;
        byte _A, _R, _G, _B;
        public void RefreshPanel()
        {
            Color curColor = core.WorkingColor;
            tb_a.Text = curColor.A.ToString();
            tb_r.Text = curColor.R.ToString();
            tb_g.Text = curColor.G.ToString();
            tb_b.Text = curColor.B.ToString();

            isOuterSetting = true;
            sld_a.Value = curColor.A;
            sld_r.Value = curColor.R;
            sld_g.Value = curColor.G;
            sld_b.Value = curColor.B;
            isOuterSetting = false;

            bool isRGBDif = false;
            if (_R != curColor.R || _G != curColor.G || _B != curColor.B)
            {
                isRGBDif = true;
                alphaSlider.StriptImage = QuickGraphics.Image.GetSlideImage(
                    new float[] { 0, 1 },
                    new Color[]
                    {
                    Color.FromArgb(0, curColor.R, curColor.G, curColor.B),
                    Color.FromArgb(255, curColor.R, curColor.G, curColor.B)
                    }, 64, 256, false);
            }
            if (isRGBDif || _A != curColor.A)
            {
                alphaSlider.SetHandlePosi(sld_a.Value / 255);

                tb_color.Background = new SolidColorBrush(curColor);
                StringBuilder clrTxBdr = new StringBuilder();
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
                tb_color.Foreground = new SolidColorBrush(core.GetBetterForeColor(curColor));
            }

            _A = curColor.A;
            _R = curColor.R;
            _G = curColor.G;
            _B = curColor.B;
        }
        private void ClrMgr_AlphaChanged(object sender, byte alpla)
        {
            if (!IsEnabled)
                return;
            RefreshPanel();
        }
        private void ClrMgr_WorkingColorWithAlphaChanged(object sender, Color argb)
        {
            if (!IsEnabled)
                return;
            RefreshPanel();
        }
        private void ClrMgr_WorkingColorChanged(object sender, int workingColorIndex, object changer)
        {
            if (!IsEnabled)
                return;
            if (sender == this)
                return;

            RefreshPanel();
        }
        private void ClrMgr_WorkingColorIndexChanged(object sender, int workingColorIndex)
        {
            if (!IsEnabled)
                return;
            RefreshPanel();
        }

        private void Sld_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isOuterSetting)
                return;

            core.SetWorkingColor(Color.FromArgb(
                (byte)sld_a.Value,
                (byte)sld_r.Value,
                (byte)sld_g.Value,
                (byte)sld_b.Value), this);

            if (sender == sld_a)
            {
                alphaSlider.SetHandlePosi(sld_a.Value / 255);
            }
        }
        private void SetAlphaFromSlider(double newAlpha)
        {
            core.SetWorkingColor(Color.FromArgb(
                (byte)(255 * newAlpha),
                (byte)sld_r.Value,
                (byte)sld_g.Value,
                (byte)sld_b.Value), this);
        }
    }
}
