using MadTomDev.Demo.Ctrls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.Demo
{
    internal class Core
    {
        public MainWindow mainWindow;
        public Class.ImageManager imgMgr = new Class.ImageManager();
        public Class.MapManager mapMgr = new Class.MapManager();
        private static Core instance = null;
        public static Core Instance
        {
            get
            {
                if (instance == null)
                    instance = new Core();
                return instance;
            }
        }
        private Core() { }

        public int currentMapIndex = -1;
        public Class.MapData CurrentMap
        {
            get
            {
                if (currentMapIndex < mapMgr.allMaps.Count)
                    return mapMgr.allMaps[currentMapIndex];
                else
                    return null;
            }
        }

        public enum TilePatternListSortMethods
        { Random, Group, Single, }

        /// <summary>
        /// 获取地图中（剩余）牌，按照给定方法重新排序
        /// </summary>
        /// <param name="map">null-当前地图，或给定地图</param>
        /// <param name="sortMethod">排序方法，Random-默认，Group-活局，Single-死局？</param>
        /// <returns></returns>
        internal List<string> GetTilePatternList(Class.MapData map = null, TilePatternListSortMethods sortMethod = TilePatternListSortMethods.Random)
        {
            // 先将牌的种类和数量做一个统计
            if (map == null)
                map = CurrentMap;
            Dictionary<string, int> tilesCount = new Dictionary<string, int>();
            List<Ctrls.Tile> restTiles = map.GetTiles_visible();
            int tileNamesLength = map.tileNames.Length;
            int countAll = 0;
            if (restTiles.Count <= 0)
            {
                // all
                int onePCount = map.countSeats / tileNamesLength;
                onePCount -= onePCount % 3;
                foreach (string p in map.tileNames)
                {
                    tilesCount.Add(p, onePCount);
                    countAll += onePCount;
                }
                int restCount = map.countSeats - countAll;
                for (int i = 0, iv = restCount / 3; i < iv; ++i)
                {
                    tilesCount[map.tileNames[i]] += 3;
                    countAll += 3;
                }
                restCount %= 3;
                if (restCount > 0)
                {
                    tilesCount[map.tileNames[tileNamesLength - 1]] += restCount;
                    countAll += restCount;
                }
            }
            else
            {
                foreach (Ctrls.Tile t in restTiles)
                {
                    if (tilesCount.ContainsKey(t.PatternName))
                        tilesCount[t.PatternName] += 1;
                    else
                        tilesCount.Add(t.PatternName, 1);
                }
                countAll = restTiles.Count;
            }

            // 按照统计好的种类和数量，进性排序
            Random rand = new Random((int)DateTime.Now.Ticks);
            int GetNextRand_preIdx = -1;
            List<string> result = new List<string>();
            string pName;
            switch (sortMethod)
            {
                case TilePatternListSortMethods.Random:
                    for (int i = 0; i < countAll; ++i)
                    {
                        pName = map.tileNames[GetNextRand()];
                        if (tilesCount.ContainsKey(pName))
                        {
                            result.Add(pName);
                            if (tilesCount[pName] <= 1)
                            {
                                tilesCount.Remove(pName);
                            }
                            else
                            {
                                tilesCount[pName] -= 1;
                            }
                        }
                        else
                        {
                            --i;
                        }
                    }
                    break;
                case TilePatternListSortMethods.Group:
                    for (int i = 0; i < countAll;)
                    {
                        pName = map.tileNames[GetNextRand()];
                        if (tilesCount.ContainsKey(pName))
                        {
                            if (tilesCount[pName] >= 3)
                            {
                                result.Add(pName);
                                result.Add(pName);
                                result.Add(pName);
                                tilesCount[pName] -= 3;
                                i += 3;
                            }
                            else if (tilesCount[pName] > 0)
                            {
                                for (int i1 = tilesCount[pName]; i1 > 0; --i1)
                                {
                                    result.Add(pName);
                                    ++i;
                                }
                            }

                            if (tilesCount[pName] <= 0)
                                tilesCount.Remove(pName);
                        }
                        else
                        {
                        }
                    }
                    break;
                case TilePatternListSortMethods.Single:
                    for (int i = 0; i < countAll;)
                    {
                        foreach (string p in map.tileNames)
                        {
                            if (tilesCountsDown(p))
                            {
                                result.Add(p);
                                ++i;
                            }
                        }
                    }
                    break;
            }

            bool tilesCountsDown(string key)
            {
                if (tilesCount.ContainsKey(key)
                    && tilesCount[key] > 0)
                {
                    tilesCount[key] -= 1;
                    return true;
                }
                return false;
            }
            int GetNextRand(bool doNotRepeat = false)
            {
                if (tileNamesLength == 1)
                    return 0;

                int result = rand.Next(tileNamesLength);
                if (doNotRepeat)
                {
                    while (result == GetNextRand_preIdx)
                    {
                        result = rand.Next(tileNamesLength);
                    }
                }
                GetNextRand_preIdx = result;
                return result;
            }

            return result;
        }

        internal void TrySetLowerTilesEnable(Tile clickedTile)
        {
            List<Class.MapData.Seat> testingSeats = new List<Class.MapData.Seat>();
            testingSeats.AddRange(clickedTile.Seat.lowerSeats);
            Class.MapData.Seat test;
            for (int i = testingSeats.Count - 1; i >= 0; --i)
            {
                test = testingSeats[i];

                // 如果不可见，则不在测试范围内
                if (test.tile.Visibility != System.Windows.Visibility.Visible)
                    testingSeats.RemoveAt(i);
                // 如果之上有可见的牌，则不在考虑范围内
                else if(test.HaveVisibleUpperTiles)
                    testingSeats.RemoveAt(i);
            }
            //for (int i = testingSeats.Count - 1; i >= 0; --i)
            //{
            //    // 如果当前作为是区域内另一个作为的下层，则去除
            //    test = testingSeats.Find(s => s.lowerSeats.Contains(testingSeats[i]));
            //    if (test != null)
            //        testingSeats.RemoveAt(i);
            //}

            foreach (Class.MapData.Seat s in testingSeats)
            {
                    s.tile.IsEnabled = true;                
            }
        }
    }
}
