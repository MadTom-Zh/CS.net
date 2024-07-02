using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MadTomDev.UI.VM
{
    public class _DataGridColumnSetter
    {
        private DataGrid dg;
        public _DataGridColumnSetter(DataGrid dg, bool autoGenCols = false)
        {
            this.dg = dg;
            dg.AutoGenerateColumns = autoGenCols;
        }
        public void ReSetColumns(List<ColumnInfo> colInfoList)
        {
            dg.Columns.Clear();
            ColumnInfo curColInfo;
            for (int i = 0, iv = colInfoList.Count; i < iv; i++)
            {
                curColInfo = colInfoList[i];
                switch (curColInfo.columnType)
                {
                    case ColumnTypes.Text:
                        dg.Columns.Add(new DataGridTextColumn
                        {
                            HeaderTemplate = curColInfo.headerTemplate,
                            Header = curColInfo.header,
                            Width = curColInfo.width,
                            Binding = curColInfo.bingding,
                            IsReadOnly = curColInfo.isReadOnly,
                        });
                        break;
                    case ColumnTypes.CheckBox:
                        dg.Columns.Add(new DataGridCheckBoxColumn
                        {
                            HeaderTemplate = curColInfo.headerTemplate,
                            Header = curColInfo.header,
                            Width = curColInfo.width,
                            Binding = curColInfo.bingding,
                            IsReadOnly = curColInfo.isReadOnly,
                        });
                        break;
                    //case ColumnTypes.ComboBox:
                    //    dg.Columns.Add(new DataGridComboBoxColumn
                    //    {
                    //        HeaderTemplate = curColInfo.headerTemplate,
                    //        Header = curColInfo.header,
                    //        Width = curColInfo.width,
                    //        SelectedItemBinding = curColInfo.comboboxSelectedItemBinding,
                    //        ItemsSource = curColInfo.comboboxItemsSource,
                    //        IsReadOnly = curColInfo.isReadOnly,
                    //    });
                    //    break;
                    case ColumnTypes.Hyperlink:
                        dg.Columns.Add(new DataGridHyperlinkColumn
                        {
                            HeaderTemplate = curColInfo.headerTemplate,
                            Header = curColInfo.header,
                            Width = curColInfo.width,
                            Binding = curColInfo.bingding,
                            IsReadOnly = curColInfo.isReadOnly,
                        });
                        break;
                    case ColumnTypes.Template_custom:
                        dg.Columns.Add(new DataGridTemplateColumn
                        {
                            HeaderTemplate = curColInfo.headerTemplate,
                            Header = curColInfo.header,
                            Width = curColInfo.width,
                            CellTemplate = curColInfo.customCellTemplate,
                            CellEditingTemplate = curColInfo.customCellEditingTemplate,
                            IsReadOnly = curColInfo.isReadOnly,
                        });
                        break;
                }

            }


        }
        public enum ColumnTypes
        {
            Text, CheckBox,  Hyperlink, Template_custom,
            //ComboBox,
        }
        public struct ColumnInfo
        {
            public ColumnTypes columnType;

            public DataTemplate headerTemplate;
            public object header;
            public DataGridLength width;
            public Binding bingding;

            //public Binding comboboxSelectedItemBinding;
            //public IEnumerable<T> comboboxItemsSource;

            public DataTemplate customCellTemplate;
            public DataTemplate customCellEditingTemplate;

            public bool isReadOnly;
        }

    }
}
