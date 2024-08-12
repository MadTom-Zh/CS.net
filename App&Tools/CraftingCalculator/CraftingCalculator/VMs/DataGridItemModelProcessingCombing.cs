using MadTomDev.App.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MadTomDev.App.VMs
{
    public class DataGridItemModelProcessingCombing : ViewModelBase ,IDisposable
    {
        public ProcessingChains.SearchResult searchResult;
        public DataGridItemModelProcessingCombing(ProcessingChains.SearchResult searchResult)
        {
            this.searchResult = searchResult;
            IsVisible = true;
            CountTxA = $"A {searchResult.AllAccessories_noDuplicates.Count}";
            CountTxI = $"I {searchResult.AllInputs.Count}";
            CountTxO = $"O {searchResult.AllFinalOutputs.Count}";
            CountTxP = $"P {searchResult.allProcesses.Count}";

            // thing list, AIOCP
            FillVMList(_Accessories, searchResult.AllAccessories_noDuplicates);
            FillVMList(_Inputs, searchResult.AllInputs);
            FillVMList(_Outputs, searchResult.AllFinalOutputs);

            ChannelsUpdate();

            FillVMList(_Processors, searchResult.AllRecipes);

        }
        private void FillVMList(ObservableCollection<ThingWithLabelModel> vmList, IEnumerable<Things.ThingBase> dataList)
        {
            BitmapImage? testImg;
            foreach (Things.ThingBase t in dataList)
            {
                testImg = ImageIO.GetOut(t);
                vmList.Add(
                    new ThingWithLabelModel()
                    {
                        Icon = testImg == null ? ImageIO.Image_Unknow : testImg,
                        IconToolTip = t.name + Environment.NewLine + t.description,
                    });
            }
        }
        internal void ChannelsUpdate()
        {
            CountTxC = $"C {searchResult.optimizedChannels.Count}";
            _Channels.Clear();
            FillVMList(_Channels, searchResult.optimizedChannels);
        }

        public void Dispose()
        {
            _Processors.Clear();
            _Accessories.Clear();
            _Inputs.Clear();
            _Outputs.Clear();
            _Channels.Clear();
        }

        private bool _IsVisible = false;
        public bool IsVisible
        {
            get => _IsVisible;
            set
            {
                _IsVisible = value;
                RaisePropertyChanged(nameof(IsVisible));
            }
        }

        #region count tx
        private string _CountTxA = "";
        public string CountTxA
        {
            get => _CountTxA;
            set
            {
                _CountTxA = value;
                RaisePropertyChanged(nameof(CountTxA));
            }
        }
        private string _CountTxI = "";
        public string CountTxI
        {
            get => _CountTxI;
            set
            {
                _CountTxI = value;
                RaisePropertyChanged(nameof(CountTxI));
            }
        }
        private string _CountTxO = "";
        public string CountTxO
        {
            get => _CountTxO;
            set
            {
                _CountTxO = value;
                RaisePropertyChanged(nameof(CountTxO));
            }
        }
        private string _CountTxC = "";
        public string CountTxC
        {
            get => _CountTxC;
            set
            {
                _CountTxC = value;
                RaisePropertyChanged(nameof(CountTxC));
            }
        }
        private string _CountTxP = "";
        public string CountTxP
        {
            get => _CountTxP;
            set
            {
                _CountTxP = value;
                RaisePropertyChanged(nameof(CountTxP));
            }
        }
        #endregion

        #region sub lists
        private ObservableCollection<ThingWithLabelModel> _Processors = new ObservableCollection<ThingWithLabelModel>();
        public ObservableCollection<ThingWithLabelModel> Processors
        {
            get => _Processors;
            set
            {
                _Processors = value;
                RaisePropertyChanged(nameof(Processors));
            }
        }
        private ObservableCollection<ThingWithLabelModel> _Accessories = new ObservableCollection<ThingWithLabelModel>();
        public ObservableCollection<ThingWithLabelModel> Accessories
        {
            get => _Accessories;
            set
            {
                _Accessories = value;
                RaisePropertyChanged(nameof(Accessories));
            }
        }
        private ObservableCollection<ThingWithLabelModel> _Inputs = new ObservableCollection<ThingWithLabelModel>();
        public ObservableCollection<ThingWithLabelModel> Inputs
        {
            get => _Inputs;
            set
            {
                _Inputs = value;
                RaisePropertyChanged(nameof(Inputs));
            }
        }
        private ObservableCollection<ThingWithLabelModel> _Outputs = new ObservableCollection<ThingWithLabelModel>();
        public ObservableCollection<ThingWithLabelModel> Outputs
        {
            get => _Outputs;
            set
            {
                _Outputs = value;
                RaisePropertyChanged(nameof(Outputs));
            }
        }
        private ObservableCollection<ThingWithLabelModel> _Channels = new ObservableCollection<ThingWithLabelModel>();
        public ObservableCollection<ThingWithLabelModel> Channels
        {
            get => _Channels;
            set
            {
                _Channels = value;
                RaisePropertyChanged(nameof(Channels));
            }
        }

        #endregion
    }
}
