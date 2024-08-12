using MadTomDev.App.Classes;
using MadTomDev.Common;
using MadTomDev.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace MadTomDev.App.VMs
{
    public class DataGridItemModelThing : ViewModelBase
    {
        public Things.Thing? dataParent;
        public Things.Thing? data;
        public DataGridItemModelThing(Things.Thing? dataParent, Things.Thing? data)
        {
            if (dataParent == null && data == null)
            {
                throw new ArgumentException($"Things are all null.");
            }
            if (dataParent != null && data != null && dataParent.id != data.parentID)
            {
                throw new ArgumentException($"Thing[{dataParent.id}] is not the parent of thing[{data.id}].");
            }
            this.dataParent = dataParent;
            this.data = data;
            Update();
        }
        internal void Update()
        {
            if (dataParent != null)
            {
                Id = dataParent.id;
                IsExcluded = dataParent.isExcluded == true;
                Name = dataParent.name;
                Description = dataParent.description;
            }
            if (data != null)
            {
                Id = data.id;
                if (data.isExcluded != null)
                {
                    IsExcluded = (bool)data.isExcluded;
                }
                if (data.name != null)
                {
                    Name = data.name;
                }
                if (data.description != null)
                {
                    Description = data.description;
                }
            }
            // Image;
            if (data != null)
            {
                BitmapImage? curDataImg = ImageIO.GetOut(data);
                if (curDataImg == null)
                {
                    if (dataParent != null)
                    {
                        BitmapImage? parDataImg = ImageIO.GetOut(dataParent);
                        Image = parDataImg == null ? ImageIO.Image_Unknow : parDataImg;
                    }
                    else
                    {
                        Image = ImageIO.Image_Unknow;
                    }
                }
                else
                {
                    Image = curDataImg;
                }
            }
            else
            {
                if (dataParent != null)
                {
                    BitmapImage? parDataImg = ImageIO.GetOut(dataParent);
                    Image = parDataImg == null ? ImageIO.Image_Unknow : parDataImg;
                }
                else
                {
                    Image = ImageIO.Image_Unknow;
                }
            }
            // Relevance;
            if (dataParent != null && data != null)
            {
                Relevance = SceneMgr.Relevances.Inherited;
            }
            else if (dataParent != null)
            {
                Relevance = SceneMgr.Relevances.Parent;
            }
            else //if (data != null)
            {
                Relevance = SceneMgr.Relevances.Origin;
            }
            // both parent n' data are null is not possible
        }


        private Guid _Id;
        public Guid Id
        {
            get => _Id;
            set
            {
                _Id = value;
                RaisePropertyChanged(nameof(Id));
            }
        }
        private BitmapImage? _Image;
        public BitmapImage? Image
        {
            get => _Image;
            set
            {
                _Image = value;
                RaisePropertyChanged(nameof(Image));
            }
        }
        private bool _IsExcluded;
        public bool IsExcluded
        {
            get => _IsExcluded;
            set
            {
                _IsExcluded = value;
                RaisePropertyChanged(nameof(IsExcluded));
            }
        }

        private SceneMgr.Relevances _Relevance;
        public SceneMgr.Relevances Relevance
        {
            get => _Relevance;
            set
            {
                _Relevance = value;
                switch (value)
                {
                    case SceneMgr.Relevances.Origin:
                        RelevanceTx = "O"; break;
                    case SceneMgr.Relevances.Parent:
                        RelevanceTx = "P"; break;
                    case SceneMgr.Relevances.Inherited:
                        RelevanceTx = "I"; break;
                }
                RaisePropertyChanged(nameof(Relevance));
            }
        }
        private string? _RelevanceTx;
        public string? RelevanceTx
        {
            get => _RelevanceTx;
            private set
            {
                _RelevanceTx = value;
                RaisePropertyChanged(nameof(RelevanceTx));
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
        private string? _Type;
        public string? Type
        {
            get => _Type;
            set
            {
                _Type = value;
                RaisePropertyChanged(nameof(Type));
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
    }

}
