using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.Data
{
    public class IndexerSet
    {
        private static IndexerSet _instance = null;
        public static IndexerSet GetInstance()
        {
            if (_instance == null)
            {
                _instance = new IndexerSet();
            }
            return _instance;
        }
        private ConcurrentDictionary<int, BlockIndexer> blockIndexerDict = new ConcurrentDictionary<int, BlockIndexer>();
        
        public BlockIndexer GetBlockIndexer(int dataHeight)
        {
            if (blockIndexerDict.ContainsKey(dataHeight))
                return blockIndexerDict[dataHeight];

            BlockIndexer bi = new BlockIndexer(dataHeight);
            blockIndexerDict.TryAdd(dataHeight, bi);
            return bi;
        }
        private ConcurrentDictionary<int, DataIndexer> dataIndexerDict = new ConcurrentDictionary<int, DataIndexer>();
        public DataIndexer GetDataIndexer(int dataHeight)
        {
            //Dictionary<int, DataIndexer> result = null
            if (dataIndexerDict.ContainsKey(dataHeight))
                return dataIndexerDict[dataHeight];

            DataIndexer di = new DataIndexer(dataHeight);
            dataIndexerDict.TryAdd(dataHeight, di);
            return di;
        }
    }
}
