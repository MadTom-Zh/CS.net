using MadTomDev.App.VMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.App.Classes
{
    public class Calculator
    {
        private Things.Thing _Product;
        public Things.Thing Product
        {
            get => _Product;
        }

        public ProcessingChains? calculatedProcessingChains;
        public ObservableCollection<DataGridItemModelProcessingCombing> calculatedProcessingCombings = new ObservableCollection<DataGridItemModelProcessingCombing>();

        public Calculator(Things.Thing product)
        {
            _Product = product;

            calculatedProcessingChains = new ProcessingChains(product);

            calculatedProcessingCombings.Clear();
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

    }
}
