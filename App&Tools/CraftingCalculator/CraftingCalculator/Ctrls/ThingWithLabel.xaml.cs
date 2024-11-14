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
using static MadTomDev.App.Ctrls.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MadTomDev.App.Ctrls
{
    /// <summary>
    /// Interaction logic for ThingWithLabel.xaml
    /// </summary>
    public partial class ThingWithLabel : UserControl, SelectableUIItem
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
        public string? TxName
        {
            get => tbv_name.Text;
            set => tbv_name.Text = value;
        }
        public string? TxDescription
        {
            get => tbv_description.Text;
            set
            {
                tbv_description.Text = value;
                tbv_description.ToolTip = value;
            }
        }

        public static string GetCommonSpeed(decimal? speed)
        {
            if (speed == null)
            {
                return "";
            }
            decimal v = (decimal)speed;
            StringBuilder strBdr = new StringBuilder();
            strBdr.AppendLine(Common.SimpleStringHelper.UnitsOfMeasure.GetShortString(v, ""));
            strBdr.AppendLine(Common.SimpleStringHelper.UnitsOfMeasure.GetShortString(v * 60, ""));
            strBdr.AppendLine(Common.SimpleStringHelper.UnitsOfMeasure.GetShortString(v * 3600, ""));
            return strBdr.ToString();
        }
        public static string TX_COMMON_SPEED_UNITS = "/s" + Environment.NewLine + "/m" + Environment.NewLine + "/h";

        public string TxLabel1
        {
            get => tbv_label1.Text;
            set
            {
                tbv_label1.Text = value;
                if (string.IsNullOrEmpty(value))
                {
                    tbv_label1.Visibility = Visibility.Collapsed;
                }
                else
                {
                    tbv_label1.Visibility = Visibility.Visible;
                }
            }
        }
        public string TxLabel2
        {
            get => tbv_label2.Text;
            set
            {
                tbv_label2.Text = value;
                if (string.IsNullOrEmpty(value))
                {
                    tbv_label2.Visibility = Visibility.Collapsed;
                }
                else
                {
                    tbv_label2.Visibility = Visibility.Visible;
                }
            }
        }
        public string TxLabel3
        {
            get => tbv_label3.Text;
            set
            {
                tbv_label3.Text = value;
                if (string.IsNullOrEmpty(value))
                {
                    tbv_label3.Visibility = Visibility.Collapsed;
                }
                else
                {
                    tbv_label3.Visibility = Visibility.Visible;
                }
            }
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

        public decimal speedPerSecSet;
        internal void SetSpeed_perSec(decimal speedPerSec)
        {
            TxLabel1 = "";
            speedPerSecSet = speedPerSec;
            TxLabel2 = GetCommonSpeed(speedPerSecSet);
            TxLabel3 = ThingWithLabel.TX_COMMON_SPEED_UNITS;
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


        #region colors

        public Color? BackgroundColor
        {
            get
            {
                if (grid.Background is SolidColorBrush)
                {
                    return ((SolidColorBrush)grid.Background).Color;
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    grid.Background = null;
                    //grid.Background = new SolidColorBrush(Colors.Transparent);
                }
                else
                {
                    grid.Background = new SolidColorBrush((Color)value);
                }
            }
        }
        public Color? BorderColor
        {
            get
            {
                if (bdr_cover.BorderBrush is SolidColorBrush)
                {
                    return ((SolidColorBrush)bdr_cover.BorderBrush).Color;
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    bdr_cover.BorderBrush = null;
                    //grid.Background = new SolidColorBrush(Colors.Transparent);
                }
                else
                {
                    bdr_cover.BorderBrush = new SolidColorBrush((Color)value);
                }
            }
        }

        #endregion

        #region selectable

        private Thickness selectedBdrThickness = new Thickness(4);
        private Thickness unselectedBdrThickness = new Thickness(1);

        private bool _IsSelectable = false;
        public bool IsSelectable
        { 
            get => _IsSelectable;
            set
            {
                if (!value && _IsSelectable)
                {
                    IsSelected = false;
                }
                _IsSelectable = value;
            }
        }

        public Color selectedBdrColor = Colors.Orange;
        private bool _IsSelected = false;
        public bool IsSelected
        { 
            get => _IsSelected;
            set
            {
                if (!_IsSelectable)
                {
                    return;
                    // throw new InvalidOperationException("This UI was set to not selectable.");
                }

                _IsSelected = value;
                if (value)
                {
                    BorderColor = selectedBdrColor;
                    bdr_cover.BorderThickness = selectedBdrThickness;
                }
                else
                {
                    BorderColor = Colors.Gray;
                    bdr_cover.BorderThickness = unselectedBdrThickness;
                }
            }
        }

        #endregion
    }
}
