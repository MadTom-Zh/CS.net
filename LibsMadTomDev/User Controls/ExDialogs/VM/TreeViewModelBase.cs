using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MadTomDev.UI.VM
{
    public class TreeViewNodeModelBase : INotifyPropertyChanged //TreeViewItem,
    {
        public TreeViewNodeModelBase( object parent)
        {
            this.parent = parent;
        }

        public object parent;
        private ObservableCollection<object> _Children;
        public ObservableCollection<object> Children
        {
            set
            {
                _Children = value;
                RaisePCEvent("Children");
            }
            get => _Children;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePCEvent(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private string _Text;
        public string Text
        {
            set
            {
                _Text = value;
                RaisePCEvent("Text");
            }
            get => _Text;
        }
        private bool _IsExpanded = false;
        public bool IsExpanded
        {
            set
            {
                _IsExpanded = value;
                RaisePCEvent("IsExpanded");
                ExpandedChanged?.Invoke();
            }
            get => _IsExpanded;
        }
        public delegate void ExpandedChangedDelegate();
        public ExpandedChangedDelegate ExpandedChanged;
        private bool _IsSelected = false;
        public bool IsSelected
        {
            set
            {
                _IsSelected = value;
                RaisePCEvent("IsSelected");
            }
            get => _IsSelected;
        }

        private int _Level = -1;
        public int Level
        {
            get
            {
                if (_Level < 0)
                {
                    ReCalLevel();
                }
                return _Level;
            }
        }
        public void ReCalLevel(bool fromParent = true)
        {
            _Level = 0;
            if (parent != null)
            {
                if (fromParent)
                {
                    TreeViewNodeModelBase iParent = (TreeViewNodeModelBase)parent;
                    do
                    {
                        _Level++;
                        iParent = (TreeViewNodeModelBase)iParent.parent;
                    }
                    while (iParent != null);
                }
                else
                {
                    _Level = ((TreeViewNodeModelBase)parent).Level + 1;
                }
            }

            if (Children != null)
            {
                foreach (TreeViewNodeModelBase sub in Children)
                {
                    sub.ReCalLevel(false);
                }
            }
        }
    }
}
