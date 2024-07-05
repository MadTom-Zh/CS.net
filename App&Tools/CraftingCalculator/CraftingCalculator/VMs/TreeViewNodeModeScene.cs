using MadTomDev.App.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MadTomDev.App.VMs
{
    public class TreeViewNodeModelScene : UI.VMBase.TreeViewNodeModelBase
    {
        private BitmapSource _Icon;
        public BitmapSource Icon
        {
            set
            {
                _Icon = value;
                RaisePCEvent("Icon");
            }
            get => _Icon;
        }
        private string _Description;
        public string Description
        {
            set
            {
                _Description = value;
                RaisePCEvent("Description");
            }
            get => _Description;
        }

        public string  dirName;
        public string GetDirFullName()
        {
            if (parent != null && parent is TreeViewNodeModelScene)
            {
                TreeViewNodeModelScene p = (TreeViewNodeModelScene)parent;
                return Path.Combine(p.GetDirFullName(), dirName);
            }
            else
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SceneMgr.DIRNAME_DEPOT, dirName);
            }
        }


        private ObservableCollection<object> _Children = new ObservableCollection<object>();
        public ObservableCollection<object> Children
        {
            set
            {
                _Children = value;
                RaisePCEvent("Children");
            }
            get => _Children;
        }
        public  bool HasChild(TreeViewNodeModelScene testNode) 
        {
            if (_Children.Count > 0)
            {
                bool test;
                foreach (object c in _Children)
                {
                    if (c == testNode)
                    {
                        return true;
                    }
                    if (c is TreeViewNodeModelScene)
                    {
                        test = HasChild((TreeViewNodeModelScene)c);
                        if (test)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static string TxLoading = "loading...";
        public void AddLoadingLabelNode()
        {
            if (_Children == null)
                _Children = new ObservableCollection<object>();
            Children.Add(new UI.VMBase.TreeViewNodeModelBase(this)
            {
                Text = TxLoading,
            });
        }
        public bool HasLoadingLabelNode()
        {
            if (Children == null)
                return false;
            object child;
            for (int i = 0, iv = Children.Count; i < iv; i++)
            {
                child = Children[i];
                if (child is UI.VMBase.TreeViewNodeModelBase)
                {
                    if (((UI.VMBase.TreeViewNodeModelBase)child).Text == TxLoading)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public void RemoveLoadingLabelNodes()
        {
            if (Children == null)
                return;
            object child;
            for (int i = 0, iv = Children.Count; i < iv; i++)
            {
                child = Children[i];
                if (child is UI.VMBase.TreeViewNodeModelBase)
                {
                    if (((UI.VMBase.TreeViewNodeModelBase)child).Text == TxLoading)
                    {
                        Children.Remove(child);
                        i--; iv--;
                    }
                }
            }
        }


        public TreeViewNodeModelScene(object? parent) : base(parent)
        {

        }
    }
}
