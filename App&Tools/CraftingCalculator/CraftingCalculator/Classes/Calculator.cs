using MadTomDev.App.VMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MadTomDev.App.Classes
{
    public class Calculator : IDisposable
    {
        private Things.Thing _Product;
        public Things.Thing Product
        {
            get => _Product;
        }

        public ProcessingChains? calculatedProcessingChains;
        public ObservableCollection<DataGridItemModelProcessingCombing> calculatedProcessingCombings = new ObservableCollection<DataGridItemModelProcessingCombing>();

        public Calculator(Guid productId, List<Guid> filterRecipeIDList,
            List<Guid> filterAccessoryIDList, List<Guid> filterChannelIDList,
            List<Guid> filterInputIDList, List<Guid> filterOutputIDList )
        {
            Things.Thing? t = Core.Instance.FindThing(productId);
            if (t == null)
            {
                throw new Exception($"No thing found with id[{productId}].");
            }
            _Product = t;

            calculatedProcessingChains = new ProcessingChains(_Product, filterRecipeIDList,
                filterAccessoryIDList, filterChannelIDList,
                filterInputIDList, filterOutputIDList);

            // calculatedProcessingChains.SearchResults.Count 数量大于 11k   ？？？？？？
            if (calculatedProcessingChains.SearchResults.Count > 100)
            {
                if (MessageBox.Show("Result count greater than 100." + Environment.NewLine + Environment.NewLine + "Continue?",
                    "Warning",
                    MessageBoxButton.YesNo,MessageBoxImage.Warning,MessageBoxResult.No) != MessageBoxResult.Yes)
                {
                    return;
                }
            }





            int i, iv;
            DataGridItemModelProcessingCombing vmPreItem;
            for (i = 0, iv = calculatedProcessingChains.SearchResults.Count; i < iv; ++i)
            {
                vmPreItem = new DataGridItemModelProcessingCombing(calculatedProcessingChains.SearchResults[i]);
                calculatedProcessingCombings.Add(vmPreItem);
            }
        }

        public enum IngrediTypes
        { Accessory, Input, Output, Channel, Procedure, }
        internal void AddFilter(Things.ThingBase? ingredi, IngrediTypes fType)
        {
            if (ingredi == null)
            {
                return;
            }
            foreach (DataGridItemModelProcessingCombing preCombinVM in calculatedProcessingCombings)
            {
                switch (fType)
                {
                    case IngrediTypes.Accessory:
                        preCombinVM.searchResult.AddFilterOutAccessory(ingredi);
                        break;
                    case IngrediTypes.Input:
                        preCombinVM.searchResult.AddFilterOutInput(ingredi);
                        break;
                    case IngrediTypes.Output:
                        preCombinVM.searchResult.AddFilterOutOutput(ingredi);
                        break;
                    case IngrediTypes.Channel:
                        preCombinVM.searchResult.AddFilterOutChannel(ingredi);
                        break;
                    case IngrediTypes.Procedure:
                        preCombinVM.searchResult.AddFilterOutProcedure(ingredi);
                        break;
                }
                if (fType == IngrediTypes.Channel)
                {
                    preCombinVM.searchResult.CheckChannels();
                    preCombinVM.ChannelsUpdate();
                }
                else
                {
                    preCombinVM.searchResult.CheckIngredients();
                }
                preCombinVM.IsVisible = preCombinVM.searchResult.IsLinksIntact && preCombinVM.searchResult.IsIngredientsIntact;
            }
        }
        internal void RemoveFilter(Things.ThingBase? ingredi, IngrediTypes fType)
        {
            if (ingredi == null)
            {
                return;
            }
            foreach (DataGridItemModelProcessingCombing preCombinVM in calculatedProcessingCombings)
            {
                switch (fType)
                {
                    case IngrediTypes.Accessory:
                        preCombinVM.searchResult.RemoveFilterOutAccessory(ingredi);
                        break;
                    case IngrediTypes.Input:
                        preCombinVM.searchResult.RemoveFilterOutInput(ingredi);
                        break;
                    case IngrediTypes.Output:
                        preCombinVM.searchResult.RemoveFilterOutOutput(ingredi);
                        break;
                    case IngrediTypes.Channel:
                        preCombinVM.searchResult.RemoveFilterOutChannel(ingredi);
                        break;
                    case IngrediTypes.Procedure:
                        preCombinVM.searchResult.RemoveFilterOutProcedure(ingredi);
                        break;
                }
                if (fType == IngrediTypes.Channel)
                {
                    preCombinVM.searchResult.CheckChannels();
                    preCombinVM.ChannelsUpdate();
                }
                else
                {
                    preCombinVM.searchResult.CheckIngredients();
                }
                preCombinVM.IsVisible = preCombinVM.searchResult.IsLinksIntact && preCombinVM.searchResult.IsIngredientsIntact;
            }
        }

        public void Dispose()
        {
            for (int i = calculatedProcessingCombings.Count - 1; i >= 0; --i)
            {
                calculatedProcessingCombings[i].Dispose();
            }
            calculatedProcessingCombings.Clear();
        }
    }
}
