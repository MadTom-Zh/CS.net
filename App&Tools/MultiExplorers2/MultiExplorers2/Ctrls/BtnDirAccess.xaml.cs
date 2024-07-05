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
    /// Interaction logic for BtnDirAccess.xaml
    /// </summary>
    public partial class BtnDirAccess : UserControl
    {
        public BtnDirAccess()
        {
            InitializeComponent();
        }

        private string _LoadingPath;
        public string LoadingPath
        {
            get => _LoadingPath;
            set
            {
                _LoadingPath = value;
                tb_fullPath.Text = value;

                string tmp;
                if (value == "PC")
                {
                    tmp = value;
                }

                if (value.StartsWith("\\\\"))
                    tb.Text = value.Substring(2);
                else
                    tb.Text = value;
            }
        }
        public enum IconTypes
        { None, PC, Custom, Dir, Network, Host, HostRoot, }
        private IconTypes _IconType = IconTypes.None;
        public IconTypes IconType
        {
            get => _IconType;
            set
            {
                if (_IconType == value) return;

                _IconType = value;
                switch (value)
                {
                    default:
                    case IconTypes.None:
                    case IconTypes.Custom:
                        img_leftIcon.Source = null;
                        break;
                    case IconTypes.PC:
                        img_leftIcon.Source = StaticResource.UIIconExplorer16;
                        break;
                    case IconTypes.Dir:
                        img_leftIcon.Source = Common.IconHelper.FileSystem.Instance.GetDirIcon(true);
                        break;
                    case IconTypes.Network:
                        img_leftIcon.Source = StaticResource.UIIconNetwork16;
                        break;
                    case IconTypes.Host:
                        img_leftIcon.Source = StaticResource.UIIconHost16;
                        break;
                    case IconTypes.HostRoot:
                        img_leftIcon.Source = StaticResource.UIIconNetDir16;
                        break;
                }
            }
        }

        public BitmapSource Icon
        {
            set => img_leftIcon.Source = value;
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            btn_toolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
            btn_toolTip.IsOpen = true;
        }
        private void btn_MouseLeave(object sender, MouseEventArgs e)
        {
            btn_toolTip.IsOpen = false;
        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            btn_toolTip.IsOpen = !btn_toolTip.IsOpen;
        }
    }
}
