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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MadTomDev.App.Ctrls
{
    /// <summary>
    /// Interaction logic for MovablePanel.xaml
    /// </summary>
    public partial class MovablePanel : UserControl, Interfaces.SelectableUIItem
    {
        public MovablePanel()
        {
            InitializeComponent();
        }

        public string? Title
        {
            get => tb_title.Text;
            set => tb_title.Text = value;
        }

        #region position
        public double X
        {
            get
            {
                if (Parent is not Canvas)
                {
                    throw new InvalidOperationException("Only work when parent is a canvas.");
                }
                return Canvas.GetLeft(this);
            }
        }
        public double Y
        {
            get
            {
                if (Parent is not Canvas)
                {
                    throw new InvalidOperationException("Only work when parent is a canvas.");
                }
                return Canvas.GetTop(this);
            }
        }
        #endregion

        #region mouse down, for moving, resizing

        private bool _AcceptTitleMouseDown = true;
        public bool AcceptTitleMouseDown
        {
            get => _AcceptTitleMouseDown;
            set => _AcceptTitleMouseDown = value;
        }

        private bool _AcceptResizeHandleDown = true;
        public bool AcceptResizeHandleDown
        {
            get => _AcceptResizeHandleDown;
            set
            {
                _AcceptResizeHandleDown = value;
                plg_resizeHandle.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public event Action<MovablePanel> TitleMouseDown;
        public event Action<MovablePanel> ResizeHandleMouseDown;
        private void rect_moveHandle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_AcceptTitleMouseDown
                && e.LeftButton == MouseButtonState.Pressed)
            {
                TitleMouseDown?.Invoke(this);
            }
        }

        private void plg_resizeHandle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_AcceptResizeHandleDown
                && e.LeftButton == MouseButtonState.Pressed)
            {
                ResizeHandleMouseDown.Invoke(this);
            }
        }


        public bool CheckMouseOnTitle()
        {
            Point pt = Mouse.GetPosition(rect_moveHandle);
            if (pt.X >= 0 && pt.Y >= 0
                && pt.X <= rect_moveHandle.ActualWidth
                && pt.Y <= rect_moveHandle.ActualHeight)
            {
                return true;
            }
            return false;
        }


        #endregion

        #region is select-able
        private bool _IsSelectable = false;
        public bool IsSelectable
        {
            get => _IsSelectable;
            set
            {
                _IsSelectable = value;
                if (value == false)
                {
                    _IsSelected = false;
                    if (this.Effect != null)
                    {
                        this.Effect = null;
                    }
                }
            }
        }

        public event Action<MovablePanel> SelectChanged;

        public DropShadowEffect selectedShadowEffect = FlowGraphAlpha.ShadowEffect_DarkOrange;
        private bool _IsSelected = false;
        public bool IsSelected
        {
            get => _IsSelected;
            set
            {
                if (_IsSelected == value)
                //|| _IsSelectable == false)
                {
                    return;
                }
                _IsSelected = value;
                if (value)
                {
                    this.Effect = selectedShadowEffect;
                }
                else
                {
                    this.Effect = null;
                }
                SelectChanged?.Invoke(this);
            }
        }
        #endregion
    }
}
