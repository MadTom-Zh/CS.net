using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace MadTomDev.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Core core = Core.Instance;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            core.mainWindow = this;
            core.currentMapIndex = 0;
            ReInitMap();
        }

        public void ClearAll()
        {
            // 清理桌面
            ClearTileParent(container_public.Children);
            container_public.Children.Clear();

            // 清理临时区域
            ClearTileParent(container_temp.Children);
            container_temp.Children.Clear();

            // 清理用户托盘
            ClearTileParent(container_player.Children);
            container_player.Children.Clear();

            void ClearTileParent(UIElementCollection children)
            {
                foreach (Ctrls.Tile t in children)
                {
                    t.Seat = null;
                    t.actionTileMouseDown = null;
                }
            }
        }
        public void ReInitMap()
        {
            // 尝试获取（下一张）地图
            Class.MapData mapData = null;
            if (core.currentMapIndex < core.mapMgr.allMaps.Count)
            {
                mapData = core.CurrentMap;
            }
            else
            {
                MessageBox.Show($"没有第 {core.currentMapIndex + 1} 关，一共只有 {core.mapMgr.allMaps.Count} 关。");
            }

            // 清理当前界面
            ClearAll();
            if (mapData == null)
                return;
            mapData.ClearTiles();
            container_public.Width = mapData.width;
            container_public.Height = mapData.height;

            // 按地图基本信息，加载界面
            // 设定玩家托盘长度
            SetPlayerTrayLength(mapData.playerTrayLength);

            // 准备牌，顺序已经排好
            List<string> tilePatternList = core.GetTilePatternList();

            // 把准备好的牌，按地图结构码放上去
            Random rand = new Random((int)DateTime.Now.Ticks);
            int idx, idx1;
            List<Class.MapData.Seat> curTopSeats;
            Class.MapData.Seat curSeat;
            Ctrls.Tile newTile;

            do
            {
                // 获取当前地图最上面一层牌的可用空位
                curTopSeats = mapData.GetSeats_nullTilesOnTopTiles();
                if (curTopSeats.Count <= 0)
                    break;

                // 码放牌，全部设定为不可点击
                do
                {
                    idx = rand.Next(curTopSeats.Count);
                    curSeat = curTopSeats[idx];
                    idx1 = tilePatternList.Count - 1;
                    newTile = new Ctrls.Tile()
                    {
                        Seat = curSeat,
                        ImgBG = core.imgMgr.img_tileBase,
                        PatternName = tilePatternList[idx1],
                        IsEnabled = false,
                        actionTileMouseDown = HandleTileClick,
                    };
                    tilePatternList.RemoveAt(idx1);
                    curTopSeats.Remove(curSeat);

                    curSeat.tile = newTile;
                    container_public.Children.Add(newTile);
                }
                while (curTopSeats.Count > 0);
            }
            while (true);

            // 将最上面一层牌，设定为可以点击
            foreach (Class.MapData.Seat s in mapData.GetSeats_topTiles())
            {
                s.tile.IsEnabled = true;
            }
        }
        internal void HandleTileClick(Ctrls.Tile clickedTile)
        {
            if (clickedTile.IsEnabled)
            {
                // 检查用户托盘是否有空位
                int insertPosi = GetPlayerTrayInsertPosition(clickedTile.PatternName);
                if (insertPosi >= 0)
                {
                    // 将地图中的这张牌隐藏（拿起）
                    clickedTile.Visibility = Visibility.Collapsed;

                    // 将这张牌下面的牌设置为可点击（如果可以）
                    core.TrySetLowerTilesEnable(clickedTile);

                    // 在玩家托盘中插入这张牌（透明，为动画预留）
                    Ctrls.Tile tileOnPlayer = new Ctrls.Tile()
                    {
                        //tagPlayer = insertPosi,
                        Width = 100,
                        Height = 120,
                        Opacity = 0.01,
                        ImgBG = core.imgMgr.img_tileBase,
                        PatternName = clickedTile.PatternName,
                        IsEnabled = true,
                        actionTileMouseDown = HandlePlayerTileClick,
                    };
                    // 将插入牌位置右边的牌，向右移动
                    Ctrls.Tile tileOnRight;
                    for (int i = insertPosi, iv = container_player.Children.Count; i < iv; ++i)
                    {
                        tileOnRight = (Ctrls.Tile)container_player.Children[i];
                        Canvas.SetLeft(tileOnRight, Canvas.GetLeft(tileOnRight) + 100);
                    }
                    Canvas.SetLeft(tileOnPlayer, insertPosi * 100);
                    container_player.Children.Insert(insertPosi, tileOnPlayer);

                    // 播放挪牌动画


                    // 动画结束后，将按钮设定为不透明
                    tileOnPlayer.Opacity = 1d;

                    // 检查是否3消
                    // 查找三个连续一样的牌，记录开始索引
                    string curTName, preTName = null;
                    int count = 0, dispelIdx = -1;
                    for (int i = 0, iv = container_player.Children.Count; i < iv; ++i)
                    {
                        curTName = ((Ctrls.Tile)container_player.Children[i]).PatternName;
                        if (curTName == preTName)
                        {
                            count++;
                            if (count >= 3)
                            {
                                dispelIdx = i - 2;
                                break;
                            }
                        }
                        else
                        {
                            count = 1;
                        }
                        preTName = curTName;
                    }
                    if (dispelIdx >= 0)
                    {
                        // 从连续索引的第一个，将连续都的3个消除；
                        container_player.Children.RemoveAt(dispelIdx);
                        container_player.Children.RemoveAt(dispelIdx);
                        container_player.Children.RemoveAt(dispelIdx);
                        // 空位右边的牌左移
                        Ctrls.Tile curTile;
                        for (int i = dispelIdx, iv = container_player.Children.Count; i < iv; ++i)
                        {
                            curTile = (Ctrls.Tile)container_player.Children[i];
                            Canvas.SetLeft(curTile, i * 100);
                        }
                    }

                    // 检查是否过关，或失败
                    Class.MapData curMap = core.CurrentMap;
                    if (curMap.allLayers[0].CountTilesVisible <= 0)
                    {
                        LevelPass();
                    }
                    else if (container_player.Children.Count >= curMap.playerTrayLength)
                    {
                        GameOver();
                    }
                }
                else
                {
                    // Game over
                    // 实际上，在插入托盘后，也检查是否已满，所以这里应该不会触发；
                    MessageBox.Show("Game over, should be happened earlier..?");
                }
            }
        }
        private int GetPlayerTrayInsertPosition(string tileName)
        {
            if (container_player.Children.Count == 0)
                return 0;
            if (container_player.Children.Count >= core.CurrentMap.playerTrayLength)
                return -1;

            int preIdx = -1;
            Ctrls.Tile curTile;
            for (int i = 0, iv = container_player.Children.Count; i < iv; ++i)
            {
                curTile = (Ctrls.Tile)container_player.Children[i];
                if (curTile.PatternName == tileName)
                    preIdx = i;
            }
            if (preIdx >= 0)
                ++preIdx;
            else
                preIdx = container_player.Children.Count;
            return preIdx;
        }

        internal void HandlePlayerTileClick(Ctrls.Tile clickedTile)
        {
            // do nothing
        }


        internal void GameOver()
        {
            MessageBox.Show("game over");
        }
        internal void LevelPass()
        {
            MessageBox.Show("level pass");
        }


        #region set player tray length
        public void SetPlayerTrayLength(int length)
        {
            int childrenCount = sPanel_fence_upper.Children.Count;
            if (length < childrenCount - 2)
            {
                // decrease
                for (int cd = childrenCount - 2 - length; cd > 0; --cd)
                {
                    sPanel_fence_upper.Children.RemoveAt(1);
                    sPanel_fence_lower.Children.RemoveAt(1);
                }
            }
            else if (childrenCount - 2 < length)
            {
                // increase
                for (int cd = length - childrenCount + 2; cd > 0; --cd)
                {
                    sPanel_fence_upper.Children.Insert(1, newImgFenceMid());
                    sPanel_fence_lower.Children.Insert(1, newImgFenceMid());
                }

                Image newImgFenceMid()
                {
                    return new Image()
                    { Source = LoadImage(System.IO.Path.Combine(Directory.GetCurrentDirectory(), @"images\fence\bottomMid.png")), };
                    //{ Source = new BitmapImage(new Uri("pack://application:,,,/images/fence/bottomMid.png")) };
                }
                BitmapImage LoadImage(string fileFullName)
                {
                    BitmapImage bmi = new BitmapImage();
                    bmi.BeginInit();
                    bmi.CacheOption = BitmapCacheOption.OnLoad;
                    bmi.UriSource = new Uri(fileFullName, UriKind.Absolute);
                    bmi.EndInit();
                    return bmi;
                }
            }
        }
        #endregion


        #region buttons

        private void btn_toolMoveOut_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_toolUndo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_toolShuffling_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_toolShuffling_live_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_toolShuffling_die_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        private void btn_test1(object sender, RoutedEventArgs e)
        {
            SetPlayerTrayLength(1);
        }
        private void btn_test2(object sender, RoutedEventArgs e)
        {
            SetPlayerTrayLength(2);

        }
        private void btn_test3(object sender, RoutedEventArgs e)
        {
            SetPlayerTrayLength(3);

        }
    }
}
