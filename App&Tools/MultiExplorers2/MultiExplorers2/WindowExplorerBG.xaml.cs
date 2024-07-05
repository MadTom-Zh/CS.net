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

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for WindowExplorerBG.xaml
    /// </summary>
    public partial class WindowExplorerBG : Window
    {
        public WindowExplorerBG()
        {
            InitializeComponent();

            Core core = Core.GetInstance();
            core.settingsLanguage.TrySetLang(cpa_bg.Resources, false);
            core.settingsLanguage.TrySetLang(cpc_bg.Resources, false);
        }


        public Ctrls.Explorer explorer;
        private bool isIniting = true;
        public UI.Class.ColorExpertCore clrCore_bg;
        private Brush preBrushBG;
        public void Init(Ctrls.Explorer explorer)
        {
            this.explorer = explorer;

            clrCore_bg = new UI.Class.ColorExpertCore();
            cpc_bg.Init(clrCore_bg);
            cpa_bg.Init(clrCore_bg);

            preBrushBG = explorer.Background;
            if (preBrushBG is SolidColorBrush)
                clrCore_bg.SetWorkingColor(((SolidColorBrush)preBrushBG).Color, null);

            clrCore_bg.WorkingColorChanged += (s1, w1, c1) =>
            {
                if (isIniting) return;
                explorer.Background = new SolidColorBrush(clrCore_bg.WorkingColor);
            };

            clrCore_bg.WorkingColorWithAlphaChanged += (s4, c4) =>
            {
                if (isIniting) return;
                explorer.Background = new SolidColorBrush(clrCore_bg.WorkingColor);
            };
            isIniting = false;
        }

        private bool restoreSetting = true;
        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            restoreSetting = false;
            Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (restoreSetting)
            {
                explorer.Background = preBrushBG;
            }
        }
    }
}
