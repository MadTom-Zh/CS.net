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

namespace MadTomDev.UI
{
    /// <summary>
    /// Interaction logic for ColorDialog.xaml
    /// </summary>
    public partial class ColorExpertDialog : Window
    {
        public ColorExpertDialog()
        {
            InitializeComponent();

            panelAllInOne.InitFromStaticCore();

            // for testing
            //colorExpertCore.colorMgr.SetWorkingColor(Colors.Pink, null);
        }

        #region panel pinched switches
        public bool IsPanelCommonChecked
        {
            get => panelAllInOne.IsPanelCommonChecked;
            set => panelAllInOne.IsPanelCommonChecked = value;
        }
        public bool IsPanelSLHChecked
        {
            get => panelAllInOne.IsPanelSLHChecked;
            set => panelAllInOne.IsPanelSLHChecked = value;
        }
        public bool IsPanelSVHChecked
        {
            get => panelAllInOne.IsPanelSVHChecked;
            set => panelAllInOne.IsPanelSVHChecked = value;
        }
        public bool IsPanelCMYKChecked
        {
            get => panelAllInOne.IsPanelCMYKChecked;
            set => panelAllInOne.IsPanelCMYKChecked = value;
        }
        public bool IsPanelARGBChecked
        {
            get => panelAllInOne.IsPanelARGBChecked;
            set => panelAllInOne.IsPanelARGBChecked = value;
        }
        public bool IsPanelScreenPPChecked
        {
            get => panelAllInOne.IsPanelScreenPPChecked;
            set => panelAllInOne.IsPanelScreenPPChecked = value;
        }
        public bool IsPanelColorCacheChecked
        {
            get => panelAllInOne.IsPanelColorCacheChecked;
            set => panelAllInOne.IsPanelColorCacheChecked = value;
        }

        #endregion

        public Color WorkingColor
        {
            get => panelAllInOne.WorkingColor;
            set => panelAllInOne.WorkingColor = value;
        }
        public List<Color> WorkingColorList
        {
            get => panelAllInOne.WorkingColorList;
        }
        public int WorkingColorIndex
        {
            get => panelAllInOne.WorkingColorIndex;
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btn_cancle_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
