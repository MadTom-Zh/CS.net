using MadTomDev.App.Classes;
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
using static MadTomDev.App.Classes.Things;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MadTomDev.App.Ctrls
{
    /// <summary>
    /// Interaction logic for ThingWithLabel.xaml
    /// </summary>
    public partial class ThingWithLabel : UserControl
    {
        public ThingWithLabel()
        {
            InitializeComponent();
        }


        public Things.ThingBase? _ThingBase = null;
        /// <summary>
        /// 按照输入物品，设置图标，名称，和描述
        /// </summary>
        public Things.ThingBase? ThingBase
        {
            get => _ThingBase;
            set
            {
                _ThingBase = value;
                if (value == null)
                {
                    img.Source = ImageIO.Image_Unknow;
                    TxName = "[Name]";
                    TxDescription = "";
                }
                else
                {
                    BitmapImage? testImg = ImageIO.GetOut(value);
                    if (testImg == null)
                    {
                        img.Source = ImageIO.Image_Unknow;
                    }
                    else
                    {
                        img.Source = testImg;
                    }
                    TxName = value.name;
                    TxDescription = value.description;
                }
            }
        }


        #region set image, labels

        public ImageSource Icon
        {
            get => img.Source;
            set => img.Source = value;
        }
        public string TxName
        {
            get => tbv_name.Text;
            set => tbv_name.Text = value;
        }
        public string TxDescription
        {
            get => tbv_description.Text;
            set
            {
                tbv_description.Text = value;
                tbv_description.ToolTip = value;
            }
        }
        public static string GetCommonSpeed(double? speed)
        {
            if (speed == null)
            {
                return "";
            }
            decimal v = (decimal)speed;
            StringBuilder strBdr = new StringBuilder();
            strBdr.AppendLine(Common.SimpleStringHelper.UnitsOfMeasure.GetShortString(v, ""));
            strBdr.AppendLine(Common.SimpleStringHelper.UnitsOfMeasure.GetShortString(v*60, ""));
            strBdr.AppendLine(Common.SimpleStringHelper.UnitsOfMeasure.GetShortString(v*3600, ""));
            return strBdr.ToString();
        }
        public static string TX_COMMON_SPEED_UNITS = "/s"+Environment.NewLine+"/m" + Environment.NewLine+"/h";

        public string TxLabel1
        {
            get => tbv_label1.Text;
            set => tbv_label1.Text = value;
        }
        public string TxLabel2
        {
            get => tbv_label2.Text;
            set => tbv_label2.Text = value;
        }
        public string TxLabel3
        {
            get => tbv_label3.Text;
            set => tbv_label3.Text = value;
        }

        public new object ToolTip
        {
            get => rect_cover.ToolTip;
            set
            {
                rect_cover.ToolTip = value;
            }
        }

        #endregion


        #region IsCheckable, IsChecked, CheckChanged
        public bool IsCheckable
        {
            get => rect_cover.Visibility == Visibility.Visible;
            set
            {
                if (value)
                {
                    rect_cover.Visibility = Visibility.Visible;
                }
                else
                {
                    rect_cover.Visibility = Visibility.Collapsed;
                }
            }
        }

        private SolidColorBrush Brush_TransGray = new SolidColorBrush(new Color() { R = 0, G = 0, B = 0, A = 127 });
        private bool _IsChecked = false;
        public bool IsChecked
        {
            get => _IsChecked;
            set
            {
                if (_IsChecked == value)
                {
                    return;
                }
                _IsChecked = value;
                if (value)
                {
                    rect_cover.Fill = Brush_TransGray;
                }
                else
                {
                    rect_cover.Fill = Brushes.Transparent;
                }
                CheckChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler? CheckChanged;
        private void rect_cover_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
        }

        #endregion


        #region label visibilities
        public bool IsNameLabelsVisible
        {
            get => grid_nameNDescription.Visibility == Visibility.Visible;
            set
            {
                if (value)
                {
                    DockPanel.SetDock(sp_numbers, Dock.Right);
                    grid_nameNDescription.Visibility = Visibility.Visible;
                }
                else
                {
                    grid_nameNDescription.Visibility = Visibility.Collapsed;
                    DockPanel.SetDock(sp_numbers, Dock.Left);
                }
            }
        }

        public bool IsNumberLabelsVisible
        {
            get => sp_numbers.Visibility == Visibility.Visible;
            set
            {
                if (value)
                {
                    sp_numbers.Visibility = Visibility.Visible;
                }
                else
                {
                    sp_numbers.Visibility = Visibility.Collapsed;
                }
            }
        }


        #endregion

    }
}
