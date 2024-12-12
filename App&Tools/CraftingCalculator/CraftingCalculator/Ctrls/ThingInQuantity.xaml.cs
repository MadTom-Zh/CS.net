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
using static MadTomDev.App.Classes.Recipes.Recipe;

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


        #region double click
        public event EventHandler? IconDoubleClicked = null;
        private DateTime rect_image_PreviewMouseDownTime = DateTime.MinValue;
        private void rect_image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            rect_image_MouseDownPoint = Mouse.GetPosition(null);
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

        #endregion

        #region drag
        public event EventHandler? IconDragStart = null;

        private Point? rect_image_MouseDownPoint = null;
        private void rect_image_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (IconDragStart == null)
            {
                return;
            }
            if (rect_image_MouseDownPoint == null)
            {
                return;
            }
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                Point curMP = Mouse.GetPosition(null);
                if (Math.Abs(curMP.X - rect_image_MouseDownPoint.Value.X) > 3
                    || Math.Abs(curMP.Y - rect_image_MouseDownPoint.Value.Y) > 3)
                {
                    IconDragStart.Invoke(this, e);
                }
            }
            else
            {
                rect_image_MouseDownPoint = null;
            }
        }
        #endregion

        private Guid _ThingId = Guid.Empty;
        public Guid ThingId
        {
            get
            {
                return _ThingId;
            }
            set
            {
                if (_ThingId == value)
                {
                    return;
                }
                _ThingId = value;
                Things.Thing? t = Core.Instance.FindThing(_ThingId);
                if (t == null)
                {
                    _ThingId = Guid.Empty;
                    img.Source = ImageIO.Image_Unknow;
                    tbvName.Text = "[Name]";
                    tbv_description.Text = "";
                    tbv_description.ToolTip = null;
                    //tbv_unit.Text = "";
                    return;
                }

                img.Source = ImageIO.GetOut(t);
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
                //tbv_unit.Text = t.unit;
            }
        }

        public void SetQuantity(Recipes.Recipe.Quantity? q)
        {
            if (q == null)
            {
                tb_numFix.Clear();
                tb_numMinus.Clear();
                tb_numPlus.Clear();
            }
            else
            {
                tb_numFix.Text = q.StringValueFix;
                tb_numMinus.Text = q.StringValueMinus;
                tb_numPlus.Text = q.StringValuePlus;
            }
        }
        public Recipes.Recipe.Quantity GetQuantityCopy()
        {
            Recipes.Recipe.Quantity result = new Recipes.Recipe.Quantity()
            {
                StringValueFix = tb_numFix.Text,
                StringValueMinus = tb_numMinus.Text,
                StringValuePlus = tb_numPlus.Text,
            };
            return result;
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
