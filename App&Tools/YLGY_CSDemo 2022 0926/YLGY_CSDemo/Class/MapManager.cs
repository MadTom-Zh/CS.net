using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Windows;
using MadTomDev.Demo.Ctrls;

namespace MadTomDev.Demo.Class
{
    internal class MapManager
    {
        private static string mapDir = "maps";
        public MapManager()
        {
            LoadMapList();
        }
        // 读取地图数据文件夹，列出可用地图；
        public void LoadMapList()
        {
            allMaps.Clear();
            if (Directory.Exists(mapDir))
            {
                foreach (FileInfo mapFI in new DirectoryInfo(mapDir).GetFiles("*.map"))
                {
                    allMaps.Add(new MapData(mapFI));
                }
                allMaps.Sort((a, b) => a.name.CompareTo(b.name));
            }
        }
        public List<MapData> allMaps = new List<MapData>();

    }

    public class MapData
    {
        public string name;
        public int width, height;
        public string[] tileNames;
        public int playerTrayLength;

        public int countLayers = 0;
        public int countSeats = 0;

        public MapData(FileInfo mapFileInfo)
        {
            // 按顺序，在地图上生成空位，自动检查位置关系，并在对应层加入位置；
            int fileContentStep = -1;
            string fileLine;
            foreach (string l in File.ReadAllLines(mapFileInfo.FullName))
            {
                fileLine = l.Trim();
                if (string.IsNullOrWhiteSpace(fileLine))
                    continue;
                if (fileLine.StartsWith("//"))
                    continue;

                if (fileLine == flag_baseInfo)
                {
                    fileContentStep = 0;
                    continue;
                }
                else if (fileLine == flag_seats)
                {
                    fileContentStep = 1;
                    continue;
                }

                switch (fileContentStep)
                {
                    case 0:
                        if (fileLine.StartsWith(flag_baseInfo_name))
                        {
                            // 地图名称
                            name = fileLine.Substring(flag_baseInfo_name.Length);
                        }
                        else if (fileLine.StartsWith(flag_baseInfo_size))
                        {
                            // 地图尺寸
                            string sizeStr = fileLine.Substring(flag_baseInfo_size.Length);
                            int cmIdx = sizeStr.IndexOf(',');
                            width = int.Parse(sizeStr.Substring(0, cmIdx));
                            height = int.Parse(sizeStr.Substring(cmIdx + 1));
                        }
                        else if (fileLine.StartsWith(flag_baseInfo_tileNames))
                        {
                            // 选用的牌的图案
                            tileNames = fileLine.Substring(flag_baseInfo_tileNames.Length).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        }
                        else if (fileLine.StartsWith(flag_baseInfo_playerTrayLength))
                        {
                            // 玩家说面长度（可放牌的个数）
                            playerTrayLength = int.Parse(fileLine.Substring(flag_baseInfo_playerTrayLength.Length));
                        }
                        break;
                    case 1:
                        // 载入地图
                        AddSeat(fileLine);
                        break;
                }
            }
        }




        #region file flags

        private static string flag_baseInfo = "[baseInfo]";
        private static string flag_baseInfo_name = "name:";
        private static string flag_baseInfo_size = "size:";
        private static string flag_baseInfo_tileNames = "tileNames:";
        private static string flag_baseInfo_playerTrayLength = "trayLength:";

        private static string flag_seats = "[seats]";

        #endregion


        public void AddSeat(string seatStr)
        {
            string[] ints = seatStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            AddSeat(int.Parse(ints[0]), int.Parse(ints[1]), int.Parse(ints[2]), int.Parse(ints[3]));
        }
        public void AddSeat(int x, int y, int width, int height)
        {
            // 找最上面的可用位置，安放位置；
            Seat newSeat = new Seat()
            {
                x = x,
                y = y,
                width = width,
                height = height,
            };
            Layer availableLayer = GetAddAvailableLayer(newSeat);
            newSeat.parent = availableLayer;
            availableLayer.allSeats.Add(newSeat);
            ++countSeats;

            // 寻找当前位置下覆盖的位置，记录位置关系；
            Layer lowerLayer;
            Extends.RectangleCollection layTest = new Extends.RectangleCollection();
            layTest.AddRect(newSeat.GetRect());
            for (int li = availableLayer.Index - 1; li >= 0; --li)
            {
                lowerLayer = allLayers[li];
                foreach (Seat s in lowerLayer.allSeats)
                {
                    if (layTest.CutFrom(s.GetRect()))
                    {
                        newSeat.lowerSeats.Add(s);
                        s.upperSeats.Add(newSeat);

                        if (!layTest.HaveRect)
                            break;
                    }
                }
            }
        }
        public Layer GetAddAvailableLayer(Seat seatToPlace)
        {
            Layer result;
            if (allLayers.Count <= 0)
            {
                result = new Layer() { parent = allLayers, };
                allLayers.Add(result);
                ++countLayers;
            }
            else
            {
                Layer preLayer = null, curLayer;
                for (int i = allLayers.Count - 1; i >= 0; --i)
                {
                    curLayer = allLayers[i];
                    if (curLayer.CheckCanPlace(seatToPlace))
                    {
                        preLayer = curLayer;
                    }
                    else
                    {
                        break;
                    }
                }
                if (preLayer == null)
                {
                    result = new Layer() { parent = allLayers, };
                    allLayers.Add(result);
                    ++countLayers;
                }
                else
                {
                    result = preLayer;
                }
            }
            return result;
        }


        private List<Seat> curTopNullTileSeats;
        public void AddTile(UIElement tile)
        {
            if (curTopNullTileSeats != null && curTopNullTileSeats.Count > 0)
            {

            }
        }

        public List<Seat> GetSeats_topTiles()
        {
            List<Seat> result = new List<Seat>();
            foreach (Layer l in allLayers)
            {
                result.AddRange(l.allSeats.FindAll(s => s.IsTileOnTop == true));
            }
            return result;
        }
        public List<Seat> GetSeats_nullTilesOnTopTiles()
        {
            List<Seat> topTiles = GetSeats_topTiles();
            List<Seat> result = new List<Seat>();
            if (topTiles.Count == 0)
            {
                if (allLayers.Count > 0)
                    result.AddRange(allLayers[0].allSeats);
            }
            else
            {
                foreach (Seat ts in topTiles)
                    result.AddRange(ts.upperSeats);

                result.Sort((a, b) => a.parent.Index.CompareTo(b.parent.Index));
                // 消除重复
                Seat sTop, sBtm;
                for (int bi, ti = result.Count - 1; ti > 0; --ti)
                {
                    sTop = result[ti];
                    for (bi = ti - 1; bi >= 0; --bi)
                    {
                        sBtm = result[bi];
                        if (sTop.SeatIndexOfLayer == sBtm.SeatIndexOfLayer
                            && sTop.SeatIndexOfMap == sBtm.SeatIndexOfMap)
                        {
                            result.RemoveAt(bi);
                            --ti;
                        }
                    }
                }
                // 从当前集合中，过滤掉更高覆盖的块，如a上面还盖着b，则去掉b
                for (int bi, ti = result.Count - 1; ti > 0; --ti)
                {
                    sTop = result[ti];
                    for (bi = ti - 1; bi >= 0; --bi)
                    {
                        sBtm = result[bi];
                        if (sBtm.upperSeats.Contains(sTop))
                        {
                            result.Remove(sTop);
                            break;
                        }
                    }
                }
            }
            return result;
        }

        internal List<Tile> GetTiles_visible()
        {
            List<Tile> result = new List<Tile>();
            foreach (Layer l in allLayers)
            {
                foreach (Seat s in l.allSeats)
                {
                    if (s.tile != null && s.tile.Visibility == Visibility.Visible)
                    {
                        result.Add(s.tile);
                    }
                }
            }
            return result;
        }
        internal void ClearTiles()
        {
            foreach (Layer l in allLayers)
            {
                foreach (Seat s in l.allSeats)
                {
                    s.tile = null;
                }
            }
        }


        public List<Layer> allLayers = new List<Layer>();
        public class Layer
        {
            public List<Layer> parent;
            public List<Seat> allSeats = new List<Seat>();

            public int Index
            {
                get => parent.IndexOf(this);
            }
            public int CountTiles
            {
                get
                {
                    int result = 0;
                    foreach (Seat s in allSeats)
                    {
                        if (s.tile != null)
                            ++result;
                    }
                    return result;
                }
            }
            public int CountTilesVisible
            {
                get
                {
                    int result = 0;
                    foreach (Seat s in allSeats)
                    {
                        if (s.tile != null && s.tile.Visibility == Visibility.Visible)
                            ++result;
                    }
                    return result;
                }
            }

            public bool CheckCanPlace(Seat testSeat)
            {
                foreach (Seat s in allSeats)
                {
                    if (Extends.CheckOverlay(s, testSeat))
                        return false;
                }
                return true;
            }
        }
        public class Seat
        {
            public Layer parent;
            public int x, y, width, height;
            public Tile tile = null;

            public bool IsNullOrEmpty
            {
                get
                {
                    if (tile == null)
                        return true;
                    else if (tile.Visibility != Visibility.Visible)
                        return true;
                    else
                        return false;
                }
            }
            public bool? IsTileOnTop
            {
                get
                {
                    if (IsNullOrEmpty)
                        return null;

                    foreach (Seat us in upperSeats)
                    {
                        if (!us.IsNullOrEmpty)
                            return false;
                    }
                    return true;
                }
            }

            public List<Seat> upperSeats = new List<Seat>();
            public List<Seat> lowerSeats = new List<Seat>();

            /// <summary>
            /// 当前空位在当前层的索引
            /// </summary>
            public int SeatIndexOfLayer
            {
                get => parent.allSeats.IndexOf(this);
            }

            /// <summary>
            /// 当前牌在当前层的索引
            /// </summary>
            public int TileIndexOfLayer
            {
                get
                {
                    if (tile == null)
                        return -1;

                    int result = -1;
                    Seat curSeat;
                    for (int i = 0, iv = parent.allSeats.Count; i < iv; ++i)
                    {
                        curSeat = parent.allSeats[i];
                        if (curSeat.tile != null)
                        {
                            ++result;
                            if (curSeat == this)
                                break;
                        }
                    }
                    return result;
                }
            }

            /// <summary>
            /// 当前空位在整个地图中的索引
            /// </summary>
            public int SeatIndexOfMap
            {
                get
                {
                    int result = 0;
                    Layer curLayer;
                    for (int li = 0, liv = parent.parent.Count; li < liv; ++li)
                    {
                        curLayer = parent.parent[li];
                        if (curLayer == parent)
                            break;
                        result += curLayer.allSeats.Count;
                    }
                    result += SeatIndexOfLayer;
                    return result;
                }
            }

            /// <summary>
            /// 当前牌在整个地图中的索引
            /// </summary>
            public int TileIndexOfMap
            {
                get
                {
                    if (tile == null)
                        return -1;

                    int result = 0;
                    Layer curLayer;
                    for (int li = 0, liv = parent.parent.Count; li < liv; ++li)
                    {
                        curLayer = parent.parent[li];
                        if (curLayer == parent)
                            break;
                        result += curLayer.CountTiles;
                    }
                    result += TileIndexOfLayer;
                    return result;
                }
            }

            public bool HaveVisibleUpperTiles
            {
                get
                {
                    foreach (Seat s in upperSeats)
                    {
                        if (s.tile != null && s.tile.Visibility == Visibility.Visible)
                            return true;
                    }
                    return false;
                }
            }

            internal Extends.Rect GetRect()
            {
                return Extends.Rect.NewXYWH(x, y, width, height);
            }
        }

        public static class Extends
        {
            public static bool CheckOverlay(Seat seat1, Seat seat2)
            {
                return CheckOverlay(ref seat1.x, ref seat1.y, ref seat1.width, ref seat1.height,
                    ref seat2.x, ref seat2.y, ref seat2.width, ref seat2.height);
            }
            public static bool CheckOverlay(Rect rect1, Rect rect2)
            {
                return CheckOverlay(ref rect1.X, ref rect1.Y, ref rect1.Width, ref rect1.Height,
                    ref rect2.X, ref rect2.Y, ref rect2.Width, ref rect2.Height);
            }
            public static bool CheckOverlay(ref int x1, ref int y1, ref int width1, ref int height1,
                ref int x2, ref int y2, ref int width2, ref int height2)
            {
                return _CheckInRange(ref x1, ref width1, ref x2, ref width2)
                    && _CheckInRange(ref y1, ref height1, ref y2, ref height2);
            }
            private static bool _CheckInRange(ref int a, ref int aLength, ref int b, ref int bLength)
            {
                return (a <= b && b < a + aLength) || (b <= a && a < b + bLength);
            }
            public static bool CheckInside(Rect area, ref Point point)
            {
                return area.X < point.X && point.X < area.Right
                    && area.Y < point.Y && point.Y < area.Bottom;
            }
            public static bool CheckInside(Rect area, ref int x, ref int y)
            {
                return area.X < x && x < area.Right
                    && area.Y < y && y < area.Bottom;
            }
            public static bool CutRect(Rect source, Rect cutter, out List<Rect> remains)
            {
                bool result = false;
                // -1 negative, 0 in range, 1 positive
                int xPo, yPo, rPo, bPo, tmp1, tmp2;
                tmp1 = source.Right;
                tmp2 = cutter.Right;
                GetPo(ref cutter.X, ref source.X, ref tmp1, out xPo);
                GetPo(ref tmp2, ref source.X, ref tmp1, out rPo);
                tmp1 = source.Bottom;
                tmp2 = cutter.Bottom;
                GetPo(ref cutter.Y, ref source.Y, ref tmp1, out yPo);
                GetPo(ref tmp2, ref source.Y, ref tmp1, out bPo);
                void GetPo(ref int test, ref int left, ref int right, out int result)
                {
                    if (test <= left)
                        result = -1;
                    else if (right <= test)
                        result = 1;
                    else
                        result = 0;
                }

                remains = new List<Rect>();
                switch (xPo, rPo, yPo, bPo)
                {
                    #region 左侧被切除
                    case (-1, 0, -1, 0): // 上部分被切除
                        remains.Add(Rect.NewLTRB(cutter.Right, source.Y, source.Right, cutter.Bottom));
                        remains.Add(Rect.NewLTRB(source.X, cutter.Bottom, source.Right, source.Bottom));
                        result = true;
                        break;
                    case (-1, 0, -1, 1): // 整列被切除
                        remains.Add(Rect.NewLTRB(cutter.Right, source.Y, source.Right, source.Bottom));
                        result = true;
                        break;
                    case (-1, 0, 0, 0): // 竖直方向中间被切除
                        remains.Add(Rect.NewLTRB(source.X, source.Y, source.Right, cutter.Y));
                        remains.Add(Rect.NewLTRB(cutter.Right, cutter.Y, source.Right, cutter.Bottom));
                        remains.Add(Rect.NewLTRB(source.X, cutter.Bottom, source.Right, source.Bottom));
                        result = true;
                        break;
                    case (-1, 0, 0, 1): // 下部分被切除
                        remains.Add(Rect.NewLTRB(source.X, source.Y, source.Right, cutter.Y));
                        remains.Add(Rect.NewLTRB(cutter.Right, cutter.Y, source.Right, source.Bottom));
                        result = true;
                        break;
                    #endregion

                    #region 整行被切除
                    case (-1, 1, -1, 0):
                    case (-2, 2, -2, 0):
                        remains.Add(Rect.NewLTRB(source.X, cutter.Bottom, source.Right, source.Bottom));
                        result = true;
                        break;
                    //case (-1, 1, -1, 1): // 完全切除，什么都不剩
                    //    break;
                    case (-1, 1, 0, 0):
                        remains.Add(Rect.NewLTRB(source.X, source.Y, source.Right, cutter.Y));
                        remains.Add(Rect.NewLTRB(source.X, cutter.Bottom, source.Right, source.Bottom));
                        result = true;
                        break;
                    case (-1, 1, 0, 1):
                        remains.Add(Rect.NewLTRB(source.X, source.Y, source.Right, cutter.Y));
                        result = true;
                        break;
                    #endregion

                    #region 横向中间部分被切除
                    case (0, 0, -1, 0):
                        remains.Add(Rect.NewLTRB(source.X, source.Y, cutter.X, cutter.Bottom));
                        remains.Add(Rect.NewLTRB(cutter.Right, source.Y, source.Right, cutter.Bottom));
                        remains.Add(Rect.NewLTRB(source.X, cutter.Bottom, source.Right, source.Bottom));
                        result = true;
                        break;
                    case (0, 0, -1, 1):
                        remains.Add(Rect.NewLTRB(source.X, source.Y, cutter.X, source.Bottom));
                        remains.Add(Rect.NewLTRB(cutter.Right, source.Y, source.Right, source.Bottom));
                        result = true;
                        break;
                    case (0, 0, 0, 0):
                        remains.Add(Rect.NewLTRB(source.X, source.Y, source.Right, cutter.Y));
                        remains.Add(Rect.NewLTRB(source.X, cutter.Y, cutter.X, cutter.Bottom));
                        remains.Add(Rect.NewLTRB(cutter.Right, cutter.Y, source.Right, cutter.Bottom));
                        remains.Add(Rect.NewLTRB(source.X, cutter.Bottom, source.Right, source.Bottom));
                        result = true;
                        break;
                    case (0, 0, 0, 1):
                        remains.Add(Rect.NewLTRB(source.X, source.Y, source.Right, cutter.Y));
                        remains.Add(Rect.NewLTRB(source.X, cutter.Y, cutter.X, source.Bottom));
                        remains.Add(Rect.NewLTRB(cutter.Right, cutter.Y, source.Right, source.Bottom));
                        result = true;
                        break;
                    #endregion

                    #region 右侧切除
                    case (0, 1, -1, 0):
                        remains.Add(Rect.NewLTRB(source.X, source.Y, cutter.X, cutter.Bottom));
                        remains.Add(Rect.NewLTRB(source.X, cutter.Bottom, source.Right, source.Bottom));
                        result = true;
                        break;
                    case (0, 1, -1, 1):
                        remains.Add(Rect.NewLTRB(source.X, source.Y, cutter.X, source.Bottom));
                        result = true;
                        break;
                    case (0, 1, 0, 0):
                        remains.Add(Rect.NewLTRB(source.X, source.Y, source.Right, cutter.Y));
                        remains.Add(Rect.NewLTRB(source.X, cutter.Y, cutter.X, cutter.Bottom));
                        remains.Add(Rect.NewLTRB(source.X, cutter.Bottom, source.Right, source.Bottom));
                        result = true;
                        break;
                    case (0, 1, 0, 1):
                        remains.Add(Rect.NewLTRB(source.X, source.Y, source.Right, cutter.Y));
                        remains.Add(Rect.NewLTRB(source.X, cutter.Y, cutter.X, source.Bottom));
                        result = true;
                        break;
                    #endregion

                    #region 整块切除
                    case (-1, 1, -1, 1):
                        result = true;
                        break;
                        #endregion
                }
                return result;
            }


            public struct Point
            {
                public int X, Y;
            }

            public class Rect
            {
                public int X, Y, Width, Height;
                public int Right { get => X + Width; }
                public int Bottom { get => Y + Height; }

                public Rect() { }
                public Rect(int x, int y, int width, int height)
                {
                    if (width <= 0)
                        throw new ArgumentOutOfRangeException("width", "Width can not be less than 0.");
                    if (height <= 0)
                        throw new ArgumentOutOfRangeException("height", "Height can not be less than 0.");

                    this.X = x; this.Y = y;
                    this.Width = width; this.Height = height;
                }
                public static Rect NewXYWH(int x, int y, int width, int height)
                {
                    return new Rect(x, y, width, height);
                }
                public static Rect NewLTRB(int left, int top, int right, int bottom)
                {
                    return new Rect(left, top, right - left, bottom - top);
                }

                public bool CheckInside(ref int x, ref int y)
                {
                    return Extends.CheckInside(this, ref x, ref y);
                }
                public bool CheckInside(ref Point point)
                {
                    return Extends.CheckInside(this, ref point);
                }
            }

            public class RectangleCollection
            {
                private List<Rect> rectList = new List<Rect>();
                public List<Rect> GetRectList
                {
                    get => rectList;
                }
                public bool HaveRect { get => rectList.Count > 0; }

                public void AddRect(Rect rect)
                {
                    _ResetMinOrMax(true);
                    rectList.Add(rect);
                }
                public Rect AddRect(int x, int y, int width, int height)
                {
                    _ResetMinOrMax(true);
                    Rect newRect = new Rect(x, y, width, height);
                    rectList.Add(newRect);
                    return newRect;
                }
                public void RemoveRect(Rect rect)
                {
                    _ResetMinOrMax(true);
                    rectList.Remove(rect);
                }
                public void RemoveRect(int x, int y, int width, int height)
                {
                    List<Rect> foundRects = rectList.FindAll(r =>
                        r.X == x && r.Y == y
                            && r.Width == width && r.Height == height);
                    if (foundRects != null)
                    {
                        foreach (Rect r in foundRects)
                            rectList.Remove(r);
                    }
                }

                #region min x, min y, max right, max bottom

                private bool _MinX_needRefresh = true;
                private int _MinX;
                public int MinX
                {
                    get
                    {
                        if (_MinX_needRefresh)
                            _GetMinNMax();

                        return _MinX;
                    }
                }
                private bool _MinY_needRefresh = true;
                private int _MinY;
                public int MinY
                {
                    get
                    {
                        if (_MinY_needRefresh)
                            _GetMinNMax();

                        return _MinY;
                    }
                }

                private bool _MaxRight_needRefresh = true;
                private int _MaxRight;
                public int MaxRight
                {
                    get
                    {
                        if (_MaxRight_needRefresh)
                            _GetMinNMax();

                        return _MaxRight;
                    }
                }
                private bool _MaxBottom_needRefresh = true;
                private int _MaxBottom;
                public int MaxBottom
                {
                    get
                    {
                        if (_MaxBottom_needRefresh)
                            _GetMinNMax();

                        return _MaxBottom;
                    }
                }
                private void _GetMinNMax()
                {
                    _MinX = int.MaxValue;
                    _MinY = int.MaxValue;
                    _MaxRight = int.MinValue;
                    _MaxBottom = int.MinValue;
                    int tmp1;
                    foreach (Rect r in rectList)
                    {
                        if (r.X < _MinX)
                            _MinX = r.X;
                        tmp1 = r.X + r.Width;
                        if (_MaxRight < tmp1)
                            _MaxRight = tmp1;

                        if (r.Y < _MinY)
                            _MinY = r.Y;
                        tmp1 = r.Y + r.Height;
                        if (_MaxBottom < tmp1)
                            _MaxBottom = tmp1;
                    }
                    _ResetMinOrMax(false);
                }
                private void _ResetMinOrMax(bool isNeedRefresh)
                {
                    _MinX_needRefresh
                        = _MinY_needRefresh
                        = _MaxRight_needRefresh
                        = _MaxBottom_needRefresh
                        = isNeedRefresh;
                }

                #endregion

                public bool CutFrom(Rect rect)
                {
                    if (rectList.Count <= 0)
                        return false;

                    int testX = MinX, testY = MinY,
                        testWidth = MaxRight - MinX,
                        testHeight = MaxBottom - MinY;
                    if (CheckOverlay(ref rect.X, ref rect.Y, ref rect.Width, ref rect.Height, ref testX, ref testY, ref testWidth, ref testHeight))
                    {
                        bool result = false;
                        Rect curRect;
                        List<Rect> remains;
                        for (int i = rectList.Count - 1; i >= 0; --i)
                        {
                            curRect = rectList[i];
                            if (Extends.CutRect(curRect, rect, out remains))
                            {
                                rectList.RemoveAt(i);
                                rectList.AddRange(remains);
                                _GetMinNMax();
                                result = true;
                            }
                        }
                        return result;
                    }
                    return false;
                }

                public bool CutFrom(List<Rect> rectList)
                {
                    bool result = false;
                    foreach (Rect r in rectList)
                    {
                        if (CutFrom(r))
                            result = true;
                    }
                    return result;
                }
            }

        }
    }
}
