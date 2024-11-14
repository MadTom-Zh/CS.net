using MadTomDev.App.Classes;
using MadTomDev.App.Ctrls;
using MadTomDev.App.VMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static MadTomDev.App.WindowMaintain;

namespace MadTomDev.App.SubWindows
{
    /// <summary>
    /// Interaction logic for WindowSelectThing.xaml
    /// </summary>
    public partial class WindowSelectChannels : Window
    {
        public WindowSelectChannels()
        {
            InitializeComponent();

            wp_channels.Children.Clear();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //tbv_Scene.Text = string.Format(
            //    (string)Application.Current.TryFindResource("lb_winSelectThings_thingsFrom"),
            //    Core.Instance.SceneName);
            tbv_Scene.Text = $"[{Core.Instance.SceneName}]";

        }


        private bool _IsMultiSelect = false;
        public bool IsMultiSelect
        {
            get => _IsMultiSelect;
            set
            {
                _IsMultiSelect = value;
                if (value)
                {
                    Title = (string)Application.Current.Resources["lb_winSelectChannels_titleSelectMultiple"];
                }
                else
                {
                    Title = (string)Application.Current.Resources["lb_winSelectChannels_titleSelectSingle"];
                    // if already select multiples, select single
                    bool foundChecked = false;
                    foreach (Ctrls.ThingWithLabel item in wp_channels.Children)
                    {
                        if (foundChecked)
                        {
                            if (item.IsChecked)
                            {
                                item.IsChecked = false;
                            }
                        }
                        else
                        {
                            if (item.IsChecked)
                            {
                                foundChecked = true;
                            }
                        }
                    }
                }
            }
        }


        #region wrapPanel, source, select changed
        private List<Channels.Channel> _Channels = new List<Channels.Channel>();
        public List<Channels.Channel>? Channels
        {
            get => _Channels;
            set
            {
                _Channels.Clear();
                if (value != null)
                {
                    for (int i = value.Count - 1; i >= 0; --i)
                    {
                        _Channels.Add(value[i]);
                    }
                }
                WrapPanelChannelsReFill();
            }
        }
        internal void SetSelection(List<Guid> checkedChannelIdList)
        {
            Ctrls.ThingWithLabel item;
            Channels.Channel channel;
            bool found;
            foreach (UIElement ui in wp_channels.Children)
            {
                if (ui is not ThingWithLabel)
                {
                    continue;
                }
                item = (ThingWithLabel)ui;
                if (item.ThingBase is not Classes.Channels.Channel)
                {
                    continue;
                }
                channel = (Channels.Channel)item.ThingBase;

                found = checkedChannelIdList.Find(a => a == channel.id) != Guid.Empty;
                item.IsChecked = found;
            }
        }


        private void WrapPanelChannelsReFill()
        {
            foreach (ThingWithLabel item in wp_channels.Children)
            {
                item.PreviewMouseLeftButtonDown -= Item_PreviewMouseLeftButtonDown;
            }
            wp_channels.Children.Clear();

            ThingWithLabel newItem;
            Channels.Channel c;
            for (int i = 0, iv = _Channels.Count; i < iv; ++i)
            {
                c = _Channels[i];
                newItem = new ThingWithLabel()
                {
                    IsSelectable = false,
                    IsCheckable = true,
                    ThingBase = c,
                    TxLabel1 = "",
                    TxLabel2 = ThingWithLabel.GetCommonSpeed(c.speed),
                    TxLabel3 = ThingWithLabel.TX_COMMON_SPEED_UNITS,
                };
                if (string.IsNullOrWhiteSpace(c.description) == false)
                {
                    newItem.ToolTip = c.name + Environment.NewLine + c.description;
                }
                newItem.PreviewMouseLeftButtonDown += Item_PreviewMouseLeftButtonDown;
                wp_channels.Children.Add(newItem);
            }
        }

        private void Item_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UIElement ue = UI.VisualHelper.WrapPanel.GetItemUI(wp_channels, typeof(ThingWithLabel), e.GetPosition(wp_channels));
            if (ue == null || ue is not ThingWithLabel)
            {
                return;
            }
            ThingWithLabel item = (ThingWithLabel)ue;
            if (item.ThingBase is null)
            {
                return;
            }
            LoadChannelInfoToWindow((Channels.Channel)item.ThingBase);
            if (_IsMultiSelect)
            {
                return;
            }
            if (!item.IsChecked)
            {
                return;
            }

            item.IsCheckable = true;
            int curItemIdx = wp_channels.Children.IndexOf(item);
            for (int i = 0, iv = wp_channels.Children.Count; i < iv; ++i)
            {
                if (i == curItemIdx
                    || wp_channels.Children[i] is not ThingWithLabel)
                {
                    continue;
                }
                item = (ThingWithLabel)wp_channels.Children[i];
                item.IsChecked = false;
            }
        }
        private void LoadChannelInfoToWindow(Channels.Channel channel)
        {
            ThingWithLabel? item = null, inItem = null;
            Channels.Channel? curChannel = null;
            foreach (UIElement ue in wp_channels.Children)
            {
                if (ue is not ThingWithLabel)
                {
                    continue;
                }
                item = (ThingWithLabel)ue;
                if (item == null
                    || item.ThingBase is not Classes.Channels.Channel)
                {
                    continue;
                }
                curChannel = (Channels.Channel)item.ThingBase;
                if (curChannel == channel)
                {
                    inItem = item;
                }
            }
            if (channel != null)
            {
                img.Source = inItem?.Icon;
                tbv_name.Text = channel.name;
                tbv_contains.Text = channel.ContentListTx;
                tbv_description.Text = channel.description;
            }
            else
            {
                ClearUI();
            }
        }
        private void ClearUI()
        {
            tbv_name.Clear();
            img.Source = ImageIO.Image_Unknow;
            tbv_description.Clear();
        }


        #endregion

        #region btn ok, cancel
        public List<Guid> CheckedChannelIdList
        {
            get
            {
                List<Guid> result = new List<Guid>();

                Ctrls.ThingWithLabel? item = null;
                Channels.Channel? curChannel = null;
                foreach (UIElement ue in wp_channels.Children)
                {
                    if (ue is not Ctrls.ThingWithLabel)
                    {
                        continue;
                    }
                    item = (Ctrls.ThingWithLabel)ue;
                    if (item == null
                        || item.ThingBase is not Classes.Channels.Channel)
                    {
                        continue;
                    }
                    curChannel = (Channels.Channel)item.ThingBase;
                    if (item.IsChecked)
                    {
                        result.Add(curChannel.id);
                    }
                }

                return result;
            }
        }
        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        #endregion

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl)
                    || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    btn_ok_Click(sender, new RoutedEventArgs());
                }
            }
        }

    }
}
