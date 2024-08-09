using MadTomDev.App.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace MadTomDev.App.VMs
{
    public class DataGridItemModelRecipe : ViewModelBase
    {
        public Recipes.Recipe? dataParent;
        public Recipes.Recipe? data;
        public DataGridItemModelRecipe(Recipes.Recipe? dataParent, Recipes.Recipe? data)
        {
            if (dataParent == null && data == null)
            {
                throw new ArgumentException($"Recipes are all null.");
            }
            if (dataParent != null && data != null && dataParent.id != data.parentID)
            {
                throw new ArgumentException($"Recipe[{dataParent.id}] is not the parent of recipe[{data.id}].");
            }
            this.dataParent = dataParent;
            this.data = data;
            Update();
        }

        public List<Recipes.Recipe.PIOItem> aList;
        public List<Recipes.Recipe.PIOItem> iList;
        public List<Recipes.Recipe.PIOItem> oList;
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
                    IsExcluded = data.isExcluded == true;
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

            aList = Core.Instance.GetNewRecipePIOList(this.Id, 0);
            iList = Core.Instance.GetNewRecipePIOList(this.Id, 1);
            oList = Core.Instance.GetNewRecipePIOList(this.Id, 2);
            CountATx = $"A {aList.Count}";
            CountITx = $"I {iList.Count}";
            CountOTx = $"O {oList.Count}";
            InputsTx = $"I {GetNameListTx(iList)}";
            OutputsTx = $"O {GetNameListTx(oList)}";

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

            string GetNameListTx(List<Recipes.Recipe.PIOItem> list)
            {
                StringBuilder strBdr = new StringBuilder();
                Things.Thing? t;
                for (int i = 0, iv = list.Count; i < iv; ++i)
                {
                    t = list[i].thing;
                    if (t != null)
                    {
                        strBdr.Append(t.name);
                        strBdr.Append(", ");
                    }
                }
                if (strBdr.Length > 1)
                {
                    strBdr.Remove(strBdr.Length - 2, 2);
                }
                return strBdr.ToString();
            }
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

        private string? _CountATx;
        public string? CountATx
        {
            get => _CountATx;
            set
            {
                _CountATx = value;
                RaisePropertyChanged(nameof(CountATx));
            }
        }
        private string? _CountITx;
        public string? CountITx
        {
            get => _CountITx;
            set
            {
                _CountITx = value;
                RaisePropertyChanged(nameof(CountITx));
            }
        }
        private string? _CountOTx;
        public string? CountOTx
        {
            get => _CountOTx;
            set
            {
                _CountOTx = value;
                RaisePropertyChanged(nameof(CountOTx));
            }
        }
        private string? _InputsTx;
        public string? InputsTx
        {
            get => _InputsTx;
            set
            {
                _InputsTx = value;
                RaisePropertyChanged(nameof(InputsTx));
            }
        }
        private string? _OutputsTx;
        public string? OutputsTx
        {
            get => _OutputsTx;
            set
            {
                _OutputsTx = value;
                RaisePropertyChanged(nameof(OutputsTx));
            }
        }
    }
}
