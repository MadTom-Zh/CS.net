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
    /// Interaction logic for ColorPanelAllInOne.xaml
    /// </summary>
    public partial class ColorPanelAllInOne : UserControl
    {
        private Class.ColorExpertCore core;
        public ColorPanelAllInOne()
        {
            InitializeComponent();
            // "txLabel_common" > Common </sys:String >
            // "txLabel_colorCode" > ColorCode </sys:String >
            // "txLabel_screenPixPicker" > ScreenPixPicker </sys:String >
            // "txLabel_colorCache" > ColorCache </sys:String >
            //ResourceDictionary rd = Application.Current.Resources;
            //if (!rd.Contains("txLabel_common"))
            //    rd.Add("txLabel_common", "Common");
            //if (!rd.Contains("txLabel_colorCode"))
            //    rd.Add("txLabel_colorCode", "ColorCode");
            //if (!rd.Contains("txLabel_screenPixPicker"))
            //    rd.Add("txLabel_screenPixPicker", "ScreenPixPicker");
            //if (!rd.Contains("txLabel_colorCache"))
            //    rd.Add("txLabel_colorCache", "ColorCache");
        }
        public void InitFromStaticCore()
        {
            Init(Class.ColorExpertCore.GetInstance());
        }
        public void Init(Class.ColorExpertCore core)
        {
            this.core = core;
            cpARGB.Init(core);
            cpCache.Init(core);
            cpCMYK.Init(core);
            cpCommon.Init(core);
            cpCC.Init(core);
            cpScreenPixPicker.Init(core);
            cpSLH.Init(core);
            cpSVH.Init(core);
        }

        #region get color panels

        private ColorPanelARGB _cpARGB;
        private ColorPanelCache _cpCache;
        private ColorPanelCMYK _cpCMYK;
        private ColorPanelCommon _cpCommon;
        private ColorCode _cpCC;
        private ColorPanelScreenPixPicker _cpScreenPixPicker;
        private ColorPanelSLH _cpSLH;
        private ColorPanelSVH _cpSVH;
        public ColorPanelARGB cpARGB
        {
            get
            {
                if (pgb_ARGB.Content is ColorPanelARGB)
                    _cpARGB = (ColorPanelARGB)pgb_ARGB.Content;
                return _cpARGB;
            }
        }
        public ColorPanelCache cpCache
        {
            get
            {
                if (pgb_colorCache.Content is ColorPanelCache)
                    _cpCache = (ColorPanelCache)pgb_colorCache.Content;
                return _cpCache;
            }
        }
        public ColorPanelCMYK cpCMYK
        {
            get
            {
                if (pgb_CMYK.Content is ColorPanelCMYK)
                    _cpCMYK = (ColorPanelCMYK)pgb_CMYK.Content;
                return _cpCMYK;
            }
        }
        public ColorPanelCommon cpCommon
        {
            get
            {
                if (pgb_common.Content is ColorPanelCommon)
                    _cpCommon = (ColorPanelCommon)pgb_common.Content;
                return _cpCommon;
            }
        }
        public ColorCode cpCC
        {
            get
            {
                if (pgb_CC.Content is ColorCode)
                    _cpCC = (ColorCode)pgb_CC.Content;
                return _cpCC;
            }
        }
        public ColorPanelScreenPixPicker cpScreenPixPicker
        {
            get
            {
                if (pgb_screenPP.Content is ColorPanelScreenPixPicker)
                    _cpScreenPixPicker = (ColorPanelScreenPixPicker)pgb_screenPP.Content;
                return _cpScreenPixPicker;
            }
        }
        public ColorPanelSLH cpSLH
        {
            get
            {
                if (pgb_SLH.Content is ColorPanelSLH)
                    _cpSLH = (ColorPanelSLH)pgb_SLH.Content;
                return _cpSLH;
            }
        }
        public ColorPanelSVH cpSVH
        {
            get
            {
                if (pgb_SVH.Content is ColorPanelSVH)
                    _cpSVH = (ColorPanelSVH)pgb_SVH.Content;
                return _cpSVH;
            }
        }

        #endregion


        public bool IsPanelCommonChecked
        {
            get => pgb_common.IsChecked;
            set => pgb_common.IsChecked = value;
        }
        public bool IsPanelCC
        {
            get => pgb_CC.IsChecked;
            set => pgb_CC.IsChecked = value;
        }
        public bool IsPanelSLHChecked
        {
            get => pgb_SLH.IsChecked;
            set => pgb_SLH.IsChecked = value;
        }
        public bool IsPanelSVHChecked
        {
            get => pgb_SVH.IsChecked;
            set => pgb_SVH.IsChecked = value;
        }
        public bool IsPanelCMYKChecked
        {
            get => pgb_CMYK.IsChecked;
            set => pgb_CMYK.IsChecked = value;
        }
        public bool IsPanelARGBChecked
        {
            get => pgb_ARGB.IsChecked;
            set => pgb_ARGB.IsChecked = value;
        }
        public bool IsPanelScreenPPChecked
        {
            get => pgb_screenPP.IsChecked;
            set => pgb_screenPP.IsChecked = value;
        }
        public bool IsPanelColorCacheChecked
        {
            get => pgb_colorCache.IsChecked;
            set => pgb_colorCache.IsChecked = value;
        }

        public Color WorkingColor
        {
            get => core.WorkingColor;
            set => core.SetWorkingColor(value, null);
        }
        public List<Color> WorkingColorList
        {
            get => core.workingColorList;
        }
        public int WorkingColorIndex
        {
            get => core.WorkingColorIndex;
        }
    }
}
