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
        public enum IngrediTypes
        { Accessory, Input, Output, Channel, Procedure, }

        public ProcessingChains? calculatedProcessingChains;
        public ObservableCollection<DataGridItemModelProcessingCombing> calculatedProcessingCombings = new ObservableCollection<DataGridItemModelProcessingCombing>();

        /// <summary>
        /// Init a new calculator
        /// </summary>
        /// <param name="productId">id of the thing to produce</param>
        /// <param name="filterRecipeIDList">id-list of recipes, not allowed</param>
        /// <param name="filterAccessoryIDList"></param>
        /// <param name="filterChannelIDList"></param>
        /// <param name="filterInputIDList"></param>
        /// <param name="filterOutputIDList"></param>
        /// <exception cref="Exception"></exception>
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
