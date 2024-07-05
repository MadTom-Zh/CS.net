using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MadTomDev.App.VM
{
    internal class DataGridRowModleBase
    {
        public BitmapSource _icon;
        public BitmapSource icon
        {
            set
            {                
                _icon = value;
                RaisePropertyChanged("icon");
            }
            get => _icon;
        }
        public string _name;
        public string name
        {
            set
            {
                _name = value;
                RaisePropertyChanged("name");
            }
            get => _name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
