using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.Test
{
    internal class DGI_language //: INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler PropertyChanged;
        //public void RaisePropertyChanged(string propertyName)
        //{
        //    PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(propertyName));
        //}

        public string LCID { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string NativeNameName { get; set; }
        public string EnglishName { get; set; }
        public bool IsNeutral { get; set; }
        public bool IsReadOnly { get; set; }
        public string Parent { get; set; }
        public string Types { get; set; }
        public string DateTimeFormat { get; set; }
        public string NumberFormat { get; set; }
        public string KeyboardLayoutId { get; set; }
        public string leftLanguageTag { get; set; }
    }
}
