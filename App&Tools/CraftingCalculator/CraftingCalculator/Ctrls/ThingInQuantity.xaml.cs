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

namespace MadTomDev.App.Ctrls
{
    /// <summary>
    /// Interaction logic for ThingInQuantity.xaml
    /// </summary>
    public partial class ThingInQuantity : UserControl
    {
        public ThingInQuantity()
        {
            InitializeComponent();
        }

        public bool IsInputEnabled
        {
            get
            {
                return gridInput.IsEnabled;
            }
            set
            {
                if (value)
                {
                    gridInput.IsEnabled = true;
                    gridInput.Visibility = Visibility.Visible;
                }
                else
                {
                    gridInput.Visibility = Visibility.Collapsed;
                    gridInput.IsEnabled = false;
                }
            }
        }
        public bool IsInputVolatileEnabled
        {
            get
            {
                return tb_numMinus.IsEnabled;
            }
            set
            {
                if (value)
                {
                    tb_numMinus.IsEnabled = true;
                    tb_numPlus.IsEnabled = true;
                    tb_numMinus.Visibility = Visibility.Visible;
                    tb_numPlus.Visibility = Visibility.Visible;
                    tbv_minus.Visibility = Visibility.Visible;
                    tbv_plus.Visibility = Visibility.Visible;
                }
                else
                {
                    tb_numMinus.IsEnabled = false;
                    tb_numPlus.IsEnabled = false;
                    tb_numMinus.Visibility = Visibility.Collapsed;
                    tb_numPlus.Visibility = Visibility.Collapsed;
                    tbv_minus.Visibility = Visibility.Collapsed;
                    tbv_plus.Visibility = Visibility.Collapsed;
                }
            }
        }
        public bool IsCloseEnabled
        {
            get
            {
                return btn_close.IsEnabled;
            }
            set
            {
                if (value)
                {
                    btn_close.IsEnabled = true;
                    btn_close.Visibility = Visibility.Visible;
                }
                else
                {
                    btn_close.Visibility = Visibility.Collapsed;
                    btn_close.IsEnabled = false;
                }
            }
        }



        public event EventHandler? IconDoubleClicked = null;
        private DateTime rect_image_PreviewMouseDownTime = DateTime.MinValue;
        private void rect_image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IconDoubleClicked == null)
            {
                return;
            }
            DateTime now = DateTime.Now;
            if ((now - rect_image_PreviewMouseDownTime).TotalMilliseconds <= Core.Instance.mouseDoubleClickInterval)
            {
                IconDoubleClicked.Invoke(this, e);
            }
            else
            {
                rect_image_PreviewMouseDownTime = now;
            }
        }

        private Recipes.Recipe.PIOItem _PIOItem = new Recipes.Recipe.PIOItem(null, null);
        public Recipes.Recipe.PIOItem PIOItem
        {
            get
            {
                if (_PIOItem.quantity == null)
                {
                    _PIOItem.quantity = new Recipes.Recipe.Quantity();
                }
                _PIOItem.quantity.StringValueFix = tb_numFix.Text;
                _PIOItem.quantity.StringValueMinus = tb_numMinus.Text;
                _PIOItem.quantity.StringValuePlus = tb_numPlus.Text;

                return _PIOItem;
            }
            set
            {
                _PIOItem = value;
                Recipes.Recipe.Quantity? q = null;
                if (value != null)
                {
                    Item = _PIOItem.item;
                    q = _PIOItem.quantity;
                }
                else
                {
                    Item = null;
                }

                if (q != null)
                {
                    tb_numFix.Text = q.StringValueFix;
                    tb_numMinus.Text = q.StringValueMinus;
                    tb_numPlus.Text = q.StringValuePlus;
                }
                else
                {
                    tb_numFix.Clear();
                    tb_numMinus.Clear();
                    tb_numPlus.Clear();
                }
            }
        }
        public Things.Thing? Item
        {
            get
            {
                if (_PIOItem == null || _PIOItem.item == null)
                {
                    return null;
                }
                return _PIOItem.item;
            }
            set
            {
                if (_PIOItem == null)
                {
                    _PIOItem = new Recipes.Recipe.PIOItem(value, null);
                }
                else
                {
                    _PIOItem.item = value;
                }
                SetThingToUI();
            }
        }
        private void SetThingToUI()
        {
            if (_PIOItem == null || _PIOItem.item == null)
            {
                img.Source = ImageIO.Image_Unknow;
                tbvName.Text = "[Name]";
                tbv_description.Text = "";
                tbv_description.ToolTip = null;
                tbv_unit.Text = "";
            }
            else
            {
                img.Source = ImageIO.GetOut(_PIOItem.item);
                Things.Thing t = _PIOItem.item;
                tbvName.Text = t.name;
                tbv_description.Text = t.description;
                if (t.description != null
                    && (t.description.Contains('\r') || t.description.Contains('\n')))
                {
                    tbv_description.ToolTip = t.description;
                }
                else
                {
                    tbv_description.ToolTip = null;
                }
                tbv_unit.Text = t.unit;
            }
        }

        public char BtnCloseTx
        {
            get
            {
                string? tx = " ";
                if (btn_close.Content != null)
                {
                    tx = btn_close.Content.ToString();
                }
                if (tx != null && tx.Length > 0)
                {
                    return tx[0];
                }
                else
                {
                    return ' ';
                }
            }
            set
            {
                btn_close.Content = value.ToString();
            }
        }

        public event EventHandler? CloseBtnClicked = null;
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            CloseBtnClicked?.Invoke(this, e);
        }

        internal void Update()
        {
            throw new NotImplementedException();
        }
    }
}
