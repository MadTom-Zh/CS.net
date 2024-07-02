using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

using System.Windows;
using System.Threading;
using System.ComponentModel;
using System.Windows.Threading;

namespace MadTomDev.UI.VMBase
{

    public class TreeViewNodeModelBase : INotifyPropertyChanged
    {
        public TreeViewNodeModelBase(object parent)
        {
            this.parent = parent;
        }

        public object parent;
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


        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePCEvent(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static TreeViewNodeModelBase preExpandedVM = null;
        private bool _IsExpanded = false;
        public bool IsExpanded
        {
            set
            {
                _IsExpanded = value;
                RaisePCEvent("IsExpanded");

                //尝试使用延迟执行，第一次启动执行了4次；
                //Dispatcher.CurrentDispatcher.BeginInvoke(ActionExpandedChanged, DispatcherPriority.Background, this);

                // 尝试使用记录，不重复，执行，第一次执行了4次；
                // 程序正常后，已经展开过的节点，再次收起和展开，此方法不被触发；
                //if (preExpandedVM != this)
                //{
                //    preExpandedVM = this;
                //    ActionExpandedChanged?.Invoke(this);
                //}

                ActionExpandedChanged?.Invoke(this);
            }
            get => _IsExpanded;
        }

        /// <summary>
        /// 因为会触发两次，所以，请在此方法中使用低优先级beginInvoke执行代码；
        /// </summary>
        public Action<object> ActionExpandedChanged;

        private bool _IsSelected = false;
        public bool IsSelected
        {
            get => _IsSelected;
            set
            {
                _IsSelected = value;
                RaisePCEvent("IsSelected");
            }
        }
    }
}
