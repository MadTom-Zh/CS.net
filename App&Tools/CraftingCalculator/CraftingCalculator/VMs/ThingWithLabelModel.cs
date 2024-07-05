using MadTomDev.App.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MadTomDev.App.VMs
{
    public class ThingWithLabelModel : ViewModelBase
    {

        private BitmapImage _Icon;
        public BitmapImage Icon
        {
            get => _Icon;
            set
            {
                _Icon = value;
                RaisePropertyChanged(nameof(Icon));
            }
        }

        private string? _Name;
        public string? Name
        {
            get => _Name;
            set
            {
                _Name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }
        private string? _Description;
        public string? Description
        {
            get => _Description;
            set
            {
                _Description = value;
                RaisePropertyChanged(nameof(Description));
            }
        }
        private string? _IconToolTip;
        public string? IconToolTip
        {
            get => _IconToolTip;
            set
            {
                _IconToolTip = value;
                RaisePropertyChanged(nameof(IconToolTip));
            }
        }


        private string? _Label1;
        public string? Label1
        {
            get => _Label1;
            set
            {
                _Label1 = value;
                RaisePropertyChanged(nameof(Label1));
            }
        }
        private string? _Label2;
        public string? Label2
        {
            get => _Label2;
            set
            {
                _Label2 = value;
                RaisePropertyChanged(nameof(Label2));
            }
        }
        private string? _Label3;
        public string? Label3
        {
            get => _Label3;
            set
            {
                _Label3 = value;
                RaisePropertyChanged(nameof(Label3));
            }
        }
    }
}
