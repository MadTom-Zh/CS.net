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
using System.Windows.Shapes;

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for WindowLayoutBtnStyle.xaml
    /// </summary>
    public partial class WindowLayoutBtnStyle : Window
    {
        public UI.Class.ColorExpertCore clrCore_bg, clrCore_c, clrCore_s;


        public WindowLayoutBtnStyle()
        {
            InitializeComponent();

            Core core = Core.GetInstance();
            core.settingsLanguage.TrySetLang(cpa_bg.Resources, false);
            core.settingsLanguage.TrySetLang(cpa_c.Resources, false);
            core.settingsLanguage.TrySetLang(cpa_s.Resources, false);
            core.settingsLanguage.TrySetLang(cpc_bg.Resources, false);
            core.settingsLanguage.TrySetLang(cpc_c.Resources, false);
            core.settingsLanguage.TrySetLang(cpc_s.Resources, false);

            clrCore_bg = new UI.Class.ColorExpertCore();
            cpc_bg.Init(clrCore_bg);
            cpa_bg.Init(clrCore_bg);
            clrCore_c = new UI.Class.ColorExpertCore();
            cpc_c.Init(clrCore_c);
            cpa_c.Init(clrCore_c);
            clrCore_s = new UI.Class.ColorExpertCore();
            cpc_s.Init(clrCore_s);
            cpa_s.Init(clrCore_s);

            clrCore_bg.WorkingColorChanged += (s1, w1, c1) =>
            {
                if (isIniting) return;
                layoutBtn.layout.bgClr = clrCore_bg.WorkingColor;
                layoutBtn.SetColorBG(clrCore_bg.WorkingColor);
            };
            clrCore_c.WorkingColorChanged += (s2, w2, c2) =>
            {
                if (isIniting) return;
                layoutBtn.layout.foreClr = clrCore_c.WorkingColor;
                layoutBtn.SetColorCharTx(clrCore_c.WorkingColor);
            };
            clrCore_s.WorkingColorChanged += (s3, w3, c3) =>
            {
                if (isIniting) return;
                layoutBtn.layout.sizeClr = clrCore_s.WorkingColor;
                layoutBtn.SetColorSizeTx(clrCore_s.WorkingColor);
            };
            clrCore_bg.WorkingColorWithAlphaChanged += (s4, c4) =>
            {
                if (isIniting) return;
                layoutBtn.layout.bgClr = clrCore_bg.WorkingColor;
                layoutBtn.SetColorBG(clrCore_bg.WorkingColor);
            };
            clrCore_c.WorkingColorWithAlphaChanged += (s5, c5) =>
            {
                if (isIniting) return;
                layoutBtn.layout.foreClr = clrCore_c.WorkingColor;
                layoutBtn.SetColorCharTx(clrCore_c.WorkingColor);
            };
            clrCore_s.WorkingColorWithAlphaChanged += (s6, c6) =>
            {
                if (isIniting) return;
                layoutBtn.layout.sizeClr = clrCore_s.WorkingColor;
                layoutBtn.SetColorSizeTx(clrCore_s.WorkingColor);
            };
        }

        private bool isIniting = true;
        private Ctrls.BtnLayout layoutBtn;
        private Setting.Layout oriBtnData;
        private ToggleButton selectedCharBtn = null;
        public void Init(Ctrls.BtnLayout layoutBtn)
        {
            isIniting = true;
            this.layoutBtn = layoutBtn;
            if (layoutBtn != null)
            {
                oriBtnData = new Setting.Layout()
                {
                    c = layoutBtn.layout.c,
                    bgClr = layoutBtn.layout.bgClr,
                    foreClr = layoutBtn.layout.foreClr,
                    sizeClr = layoutBtn.layout.sizeClr,
                };
                SetChar(oriBtnData.c);
                clrCore_bg.SetWorkingColor(oriBtnData.bgClr, null);
                clrCore_c.SetWorkingColor(oriBtnData.foreClr, null);
                clrCore_s.SetWorkingColor(oriBtnData.sizeClr, null);

                tb_toolTip.Text = layoutBtn.layout.tipTx;
            }
            isIniting = false;
        }

        #region set char
        private bool isSettingChar = false;
        private void SetChar(char c)
        {
            isSettingChar = true;
            layoutBtn.layout.c = c;
            layoutBtn.SetChar(c);
            tb_char.Text = c.ToString();
            if (selectedCharBtn != null)
                selectedCharBtn.IsChecked = false;

            foreach (ToggleButton tBtn in sPanel_charBtns.Children)
            {
                if (tBtn.Content.ToString()[0] == c)
                {
                    selectedCharBtn = tBtn;
                    tBtn.IsChecked = true;
                    break;
                }
            }
            isSettingChar = false;
        }

        private void tb_char_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isSettingChar)
                return;

            if (tb_char.Text.Length > 0)
                SetChar(tb_char.Text[0]);
        }
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton tBtn = (ToggleButton)sender;
            SetChar(tBtn.Content.ToString()[0]);
        }
        #endregion



        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            windowClosingAction = 1;
            Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            windowClosingAction = 0;
            Close();
        }

        int windowClosingAction = 0;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            switch (windowClosingAction)
            {
                case 0:
                    layoutBtn.layout.c = oriBtnData.c;
                    layoutBtn.layout.foreClr = oriBtnData.foreClr;
                    layoutBtn.layout.sizeClr = oriBtnData.sizeClr;
                    layoutBtn.layout.bgClr = oriBtnData.bgClr;

                    layoutBtn.SetChar(oriBtnData.c);
                    layoutBtn.SetColorBG(oriBtnData.bgClr);
                    layoutBtn.SetColorCharTx(oriBtnData.foreClr);
                    layoutBtn.SetColorSizeTx(oriBtnData.sizeClr);
                    //Setting.Instance.Save();
                    break;
                case 1:
                    layoutBtn.layout.tipTx = tb_toolTip.Text;
                    layoutBtn.SetToolTipTx(tb_toolTip.Text);
                    Setting.Instance.Save();
                    break;
            }
        }
    }
}
