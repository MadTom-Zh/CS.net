using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MadTomDev.App
{
    class VMs
    {
        public class SolutionItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            private void RaisePropertyChangedEvent(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public int[][] solution;

            private string _Text = "";
            public string Text
            {
                get => _Text;
                set
                {
                    _Text = value;
                    RaisePropertyChangedEvent(nameof(Text));
                }
            }

            private bool _isChecked = false;
            public bool IsChecked
            {
                get=> _isChecked;
                set
                {
                    _isChecked = value;
                    RaisePropertyChangedEvent(nameof(IsChecked));
                    BdrVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            private Visibility _BdrVisibility = Visibility.Collapsed;
            public Visibility BdrVisibility
            {
                get => _BdrVisibility;
                set
                {
                    _BdrVisibility = value;
                    RaisePropertyChangedEvent(nameof(BdrVisibility));
                }
            }
        }
    }
}
