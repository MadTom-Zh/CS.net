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

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for WindowSelectThing.xaml
    /// </summary>
    public partial class WindowSelectThings : Window
    {
        public WindowSelectThings()
        {
            InitializeComponent();

            tbv_thingsUnderScene.Text = $"Things from [{Core.Instance.SceneName}]";

            searcherThingName = new WindowMaintain.SearchHelper<DataGridItemModelThing>(dg_things, tbv_nameCount);
            searcherThingName.CheckItemFunc = CheckThingName;
            searcherThingType = new WindowMaintain.SearchHelper<DataGridItemModelThing>(dg_things, tbv_typeCount);
            searcherThingType.CheckItemFunc = CheckThingType;
        }
        private WindowMaintain.SearchHelper<DataGridItemModelThing> searcherThingName;
        private WindowMaintain.SearchHelper<DataGridItemModelThing> searcherThingType;


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
                    dg_things.SelectionMode = DataGridSelectionMode.Extended;
                }
                else
                {
                    dg_things.SelectionMode = DataGridSelectionMode.Single;
                }
            }
        }


        #region dataGrid, source, select changed
        private ObservableCollection<DataGridItemModelThing>? _DataGridSource;
        public ObservableCollection<DataGridItemModelThing>? DataGridSource
        {
            get => _DataGridSource;
            set
            {
                _DataGridSource = value;
                dg_things.ItemsSource = value;
            }
        }

        private void dg_things_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            searcherThingName.SkipOrReset();
            searcherThingType.SkipOrReset();

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
                tb_type.Text = lastItem.Type;


                tbv_description.Text = lastItem.Description;
            }
            else
            {
                ClearUI();
            }
        }
        private void ClearUI()
        {
            tb_name.Clear();
            tb_type.Clear();
            tbv_nameCount.Text = "";
            tbv_typeCount.Text = "";
            img.Source = ImageIO.Image_Unknow;
            tbv_description.Clear();
        }


        #endregion


        #region quick search by name

        private Common.SimpleStringHelper.Checker_starNQues checkerThingName = new Common.SimpleStringHelper.Checker_starNQues();
        List<DataGridItemModelThing> quickSearchResultName = new List<DataGridItemModelThing>();
        int quickSearchResultName_curIndex = -1;
        int quickSearchResultName_curState = 0;
        private void tb_name_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btn_searchName_Click(sender, e);
            }
        }
        private void tb_name_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            searcherThingName.Reset();
        }

        private void btn_searchName_Click(object sender, RoutedEventArgs e)
        {
            searcherThingName.SearchOrNext(tb_name.Text);
        }
        private bool CheckThingName(object helper, DataGridItemModelThing item)
        {
            SearchHelper<DataGridItemModelThing> h = (SearchHelper<DataGridItemModelThing>)helper;
            if (item.Name != null && h.Check(item.Name))
            {
                return true;
            }
            if (item.Description != null && h.Check(item.Description))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region quick search by type
        private void tb_type_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btn_searchType_Click(sender, e);
            }
        }
        private void tb_type_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            searcherThingType.Reset();
        }

        private void btn_searchType_Click(object sender, RoutedEventArgs e)
        {
            searcherThingType.SearchOrNext(tb_type.Text);
        }
        private bool CheckThingType(object helper, DataGridItemModelThing item)
        {
            SearchHelper<DataGridItemModelThing> h = (SearchHelper<DataGridItemModelThing>)helper;
            if (item.Type != null && h.Check(item.Type))
            {
                return true;
            }
            return false;
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

    }
}
