using MadTomDev.App.Classes;
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
    public partial class WindowSelectThings : Window
    {
        public WindowSelectThings()
        {
            InitializeComponent();

        }
        private WindowMaintain.SearchHelper<DataGridItemModelThing> searcherThingName;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //tbv_Scene.Text = string.Format(
            //    (string)Application.Current.TryFindResource("lb_winSelectThings_thingsFrom"),
            //    Core.Instance.SceneName);
            tbv_Scene.Text = $"[{Core.Instance.SceneName}]";

            searcherThingName = new WindowMaintain.SearchHelper<DataGridItemModelThing>(dg_things, tbv_nameCount);
            searcherThingName.CheckItemFunc = CheckThingName;

            tb_name.Focus();
        }


        public bool IsMultiSelect
        {
            get
            {
                return dg_things.SelectionMode == DataGridSelectionMode.Extended;
            }
            set
            {
                if (value)
                {
                    Title = (string)Application.Current.Resources["lb_winSelectThings_titleSelectMultiple"];
                    dg_things.SelectionMode = DataGridSelectionMode.Extended;
                }
                else
                {
                    Title = (string)Application.Current.Resources["lb_winSelectThings_titleSelectSingle"];
                    dg_things.SelectionMode = DataGridSelectionMode.Single;
                }
            }
        }


        #region dataGrid, source, select changed
        private ObservableCollection<DataGridItemModelThing> _DataGridSource = new ObservableCollection<DataGridItemModelThing>();
        public ObservableCollection<DataGridItemModelThing>? DataGridSource
        {
            get => _DataGridSource;
            set
            {
                _DataGridSource.Clear();
                if (value != null)
                {
                    for (int i = value.Count - 1; i >= 0; --i)
                    {
                        _DataGridSource.Add(value[i]);
                    }
                }
                dg_things.ItemsSource = _DataGridSource;
            }
        }

        private void dg_things_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            searcherThingName.SkipOrReset();

            _SelectedThingIdList?.Clear();
            DataGridItemModelThing curItem;
            DataGridItemModelThing? lastItem = null;
            foreach (object raw in dg_things.SelectedItems)
            {
                if (raw is not DataGridItemModelThing)
                {
                    continue;
                }

                curItem = (DataGridItemModelThing)raw;
                _SelectedThingIdList?.Add(curItem.Id);
                lastItem = curItem;
            }
            if (lastItem != null)
            {
                img.Source = lastItem.Image;
                tb_name.Text = lastItem.Name;
                tbv_description.Text = lastItem.Description;
            }
            else
            {
                ClearUI();
            }

            //if (dg_things.SelectedItem != null)
            //{
            //    dg_things.ScrollIntoView(dg_things.SelectedItem);
            //}
        }
        private void ClearUI()
        {
            tb_name.Clear();
            tbv_nameCount.Text = "";
            img.Source = ImageIO.Image_Unknow;
            tbv_description.Clear();
        }


        #endregion


        #region quick search by name

        private Common.SimpleStringHelper.Checker_starNQues checkerThingName = new Common.SimpleStringHelper.Checker_starNQues();
        List<DataGridItemModelThing> quickSearchResultName = new List<DataGridItemModelThing>();
        int quickSearchResultName_curIndex = -1;
        int quickSearchResultName_curState = 0;
        private bool tb_name_ignoreKeyUpOnce = false;
        private void tb_name_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl)
                    || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    return;
                }
                tb_name_ignoreKeyUpOnce = true;
                btn_searchName_Click(sender, e);
            }
        }
        private void tb_name_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (tb_name_ignoreKeyUpOnce)
            {
                tb_name_ignoreKeyUpOnce = false;
                return;
            }
            searcherThingName.Reset();
        }

        private async void btn_searchName_Click(object sender, RoutedEventArgs e)
        {
            await Core.Instance.SetCursorWait(this);
            searcherThingName.SearchOrNext(tb_name.Text);
            await Core.Instance.SetCursorArrow(this);
        }
        private int CheckThingName(object helper, DataGridItemModelThing item, object? missing)
        {
            SearchHelper<DataGridItemModelThing> h = (SearchHelper<DataGridItemModelThing>)helper;
            if (item.Name != null && h.Check(item.Name))
            {
                return 1;
            }
            if (item.Description != null && h.Check(item.Description))
            {
                return 2;
            }
            return 0;
        }
        #endregion

        #region btn ok, cancel
        private List<Guid>? _SelectedThingIdList = new List<Guid>();
        public List<Guid>? SelectedThingIdList
        {
            get => _SelectedThingIdList;
            private set
            {
                _SelectedThingIdList = value;
                btn_ok.IsEnabled = value != null;
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
            _SelectedThingIdList = null;
            this.Close();
        }
        private DateTime dg_things_PreviewMouseDownTime = DateTime.MinValue;
        private void dg_things_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DateTime now = DateTime.Now;
            if ((now - dg_things_PreviewMouseDownTime).TotalMilliseconds <= Core.Instance.mouseDoubleClickInterval)
            {
                btn_ok_Click(sender, e);
            }
            else
            {
                dg_things_PreviewMouseDownTime = now;
            }
        }
        private void dg_things_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btn_ok_Click(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                btn_cancel_Click(sender, e);
            }
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
