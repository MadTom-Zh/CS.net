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
    public class DataGridItemModelChannel : ViewModelBase
    {
        public Channels.Channel? dataParent;
        public Channels.Channel? data;
        public DataGridItemModelChannel(Channels.Channel? dataParent, Channels.Channel? data)
        {
            if (dataParent == null && data == null)
            {
                throw new ArgumentException($"Channels are all null.");
            }
            if (dataParent != null && data != null && dataParent.id != data.parentID)
            {
                throw new ArgumentException($"Channel[{dataParent.id}] is not the parent of channel[{data.id}].");
            }
            this.dataParent = dataParent;
            this.data = data;
            Update();
        }

        public List<Channels.Channel.ContentItem> contentList = new List<Channels.Channel.ContentItem>();
        internal void Update()
        {
            if (dataParent != null)
            {
                Id = dataParent.id;
                IsExcluded = dataParent.isExcluded == true;
                Speed = dataParent.speed == null? 0 : (double)dataParent.speed;
                Name = dataParent.name;
                Description = dataParent.description;
            }
            if (data != null)
            {
                Id = data.id;
                if (data.name != null)
                {
                    Name = data.name;
                }
                if (data.isExcluded != null)
                {
                    IsExcluded = data.isExcluded == true;
                }
                if (data.speed != null)
                {
                    Speed = (double)data.speed;
                }
                if (data.description != null)
                {
                    Description = data.description;
                }
            }

            contentList = Core.Instance.GetNewChannelContentList(this.Id);
            CCount = contentList.Count.ToString();

            StringBuilder strBdr = new StringBuilder();
            Channels.Channel.ContentItem curItem;
            Things.Thing? foundThing;
            for (int i=0,iv = contentList.Count;i<iv;++i)
            {
                curItem = contentList[i];
                if (curItem.addOrRemove == false)
                {
                    continue;
                }
                foundThing = Core.Instance.FindThing(contentList[i].contentId);
                if (foundThing != null)
                {
                    strBdr.Append(foundThing.name);
                    strBdr.Append(", ");
                }
            }
            if (strBdr.Length > 1)
            {
                strBdr.Remove(strBdr.Length -2,2);
            }
            ContentListTx = strBdr.ToString();

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
                Relevance = Relevances.Inherited;
            }
            else if (dataParent != null)
            {
                Relevance = Relevances.Parent;
            }
            else //if (data != null)
            {
                Relevance = Relevances.Origin;
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
        private bool _IsExcluded = false;
        public bool IsExcluded
        {
            get => _IsExcluded;
            set
            {
                _IsExcluded = value;
                RaisePropertyChanged(nameof(IsExcluded));
            }
        }
        private double _Speed = 0;
        public double Speed
        {
            get => _Speed;
            set
            {
                _Speed = value;
                RaisePropertyChanged(nameof(Speed));
            }
        }
        public enum Relevances
        {
            Origin, Parent, Inherited,
        }
        private Relevances _Relevance;
        public Relevances Relevance
        {
            get => _Relevance;
            set
            {
                _Relevance = value;
                switch (value)
                {
                    case Relevances.Origin:
                        RelevanceTx = "O"; break;
                    case Relevances.Parent:
                        RelevanceTx = "P"; break;
                    case Relevances.Inherited:
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

        private string? _CCount;
        public string? CCount
        {
            get => _CCount;
            set
            {
                _CCount = value;
                RaisePropertyChanged(nameof(CCount));
            }
        }


        private string? _ContentListTx;
        internal string? ContentListTx
        {
            get => _ContentListTx;
            set
            {
                _ContentListTx = value;
                RaisePropertyChanged(nameof(ContentListTx));
            }
        }
    }
}
