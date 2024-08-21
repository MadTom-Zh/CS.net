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
using MadTomDev.UI;

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            FileInfo? mapFi = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).GetFiles("lastMap.*").FirstOrDefault();
            if (mapFi != null)
            {
                // 如果有之前保存的地图，则加载地图
                img_mapOrigin.Source = LoadBitmapImageNoFileLock(mapFi.FullName);

                // 加载其他保存的数据
                LoadAllInfo();
            }
            isLoadingInfo = false;
        }

        private BitmapImage LoadBitmapImageNoFileLock(string imgFile)
        {
            BitmapImage result = new BitmapImage();
            result.BeginInit();
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.UriSource = new Uri(imgFile);
            result.EndInit();
            return result;
        }

        #region base points
        private Point _pt_mapTopLeft, _Pointpt_mapBottomRight;
        private Point pt_mapTopLeft
        {
            get => _pt_mapTopLeft;
            set
            {
                _pt_mapTopLeft = value;
                TrySetMapBoundryChain();
            }
        }
        private Point pt_mapBottomRight
        {
            get => _Pointpt_mapBottomRight;
            set
            {
                _Pointpt_mapBottomRight = value;
                TrySetMapBoundryChain();
            }
        }
        private Point _pt1, _pt2, _pt3, _posi1, _posi2, _posi3;
        private Point pt1
        {
            get => _pt1;
            set
            {
                _pt1 = value;
                TrySetPositionChain();
            }
        }
        private Point pt2
        {
            get => _pt2;
            set
            {
                _pt2 = value;
                TrySetPositionChain();
            }
        }
        private Point pt3
        {
            get => _pt3;
            set
            {
                _pt3 = value;
                TrySetPositionChain();
            }
        }
        private Point posi1

        {
            get => _posi1;
            set
            {
                _posi1 = value;
                TrySetPositionChain();
            }
        }
        private Point posi2
        {
            get => _posi2;
            set
            {
                _posi2 = value;
                TrySetPositionChain();
            }
        }
        private Point posi3
        {
            get => _posi3;
            set
            {
                _posi3 = value;
                TrySetPositionChain();
            }
        }
        #endregion


        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TrySaveAllInfo();
        }


        #region 读取和保存用户输入

        private string infoFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "info.txt");
        private bool needSave = false;
        public void TrySaveAllInfo()
        {
            if (needSave)
            {
                StringBuilder strBdr = new StringBuilder();
                AppendPtTx(pt_mapTopLeft); strBdr.Append(", "); AppendPtTx(pt_mapBottomRight);
                strBdr.AppendLine();

                AppendPtTx(pt1); strBdr.Append(", "); AppendPtTx(pt2); strBdr.Append(", "); AppendPtTx(pt3); strBdr.Append(", ");
                AppendPtTx(posi1); strBdr.Append(", "); AppendPtTx(posi2); strBdr.Append(", "); AppendPtTx(posi3);
                strBdr.AppendLine();

                strBdr.Append(tb_user.Text);

                File.WriteAllText(infoFilePath, strBdr.ToString());
                needSave = false;

                void AppendPtTx(Point pt)
                {
                    strBdr.Append($"{pt.X}, {pt.Y}");
                }
            }
        }
        private bool isLoadingInfo = true;
        public void LoadAllInfo()
        {
            isLoadingInfo = true;

            if (File.Exists(infoFilePath))
            {
                string[] lines = File.ReadAllLines(infoFilePath);
                // 行1， 地图边界点  1，2
                // 行2， 地图定位点 1，2，3   地图对应经纬度 1，2，3
                // 行3 及以后，用户输入的信息；
                try
                {
                    string[] parts = lines[0].Split(new string[] { "," }, StringSplitOptions.TrimEntries);
                    pt_mapTopLeft = new Point(double.Parse(parts[0]), double.Parse(parts[1]));
                    tb_topLeft.Text = pt_mapTopLeft.ToString();
                    pt_mapBottomRight = new Point(double.Parse(parts[2]), double.Parse(parts[3]));
                    tb_bottomRight.Text = pt_mapBottomRight.ToString();

                    parts = lines[1].Split(new string[] { "," }, StringSplitOptions.TrimEntries);
                    pt1 = new Point(double.Parse(parts[0]), double.Parse(parts[1]));
                    tb_pt1.Text = pt1.ToString();
                    pt2 = new Point(double.Parse(parts[2]), double.Parse(parts[3]));
                    tb_pt2.Text = pt2.ToString();
                    pt3 = new Point(double.Parse(parts[4]), double.Parse(parts[5]));
                    tb_pt3.Text = pt3.ToString();
                    posi1 = new Point(double.Parse(parts[6]), double.Parse(parts[7]));
                    tb_posi1.Text = posi1.ToString();
                    posi2 = new Point(double.Parse(parts[8]), double.Parse(parts[9]));
                    tb_posi2.Text = posi2.ToString();
                    posi3 = new Point(double.Parse(parts[10]), double.Parse(parts[11]));
                    tb_posi3.Text = posi3.ToString();

                    StringBuilder strBdr = new StringBuilder();
                    for (int i = 2, iv = lines.Length; i < iv; ++i)
                    {
                        strBdr.AppendLine(lines[i]);
                    }

                    // 将剩余文本，写入用户查询窗口中
                    tb_user.Text = strBdr.ToString();
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
            }
            isLoadingInfo = false;
        }

        #endregion


        #region 地图输入，设定地图边界，设定三角定位


        #region 地图文件输入
        private DragDropEffects dropZoneEffect = DragDropEffects.None;
        private string dropZoneFile;
        private BitmapImage testImage;
        private void tb_dropZone_PreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1)
                {
                    dropZoneFile = files[0];
                    try
                    {
                        testImage = LoadBitmapImageNoFileLock(dropZoneFile);

                        e.Effects = dropZoneEffect = DragDropEffects.Link;
                        return;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            e.Effects = dropZoneEffect = DragDropEffects.None;
        }

        private void tb_dropZone_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.Effects = dropZoneEffect;
        }

        private void tb_dropZone_PreviewDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (dropZoneEffect == DragDropEffects.Link)
            {
                FileInfo fi = new FileInfo(dropZoneFile);
                string tarFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lastMap" + fi.Extension);
                if (File.Exists(tarFile))
                {
                    img_mapOrigin.Source = null;
                    img_map.Source = null;
                    File.Delete(tarFile);
                }
                fi.CopyTo(tarFile);
                img_mapOrigin.Source = testImage;
                testImage = null;
                TrySetMapBoundryChain();
            }
        }
        #endregion


        #region 设定地图边界

        private void tb_topLeft_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isLoadingInfo)
                return;
            // 记录图片上的边界位置 - top left
            if (TryTextToPoint(tb_topLeft, out Point pt))
            {
                pt_mapTopLeft = pt;
                needSave = true;
            }
        }
        private bool TryGetPoint(string str, out Point pt)
        {
            string[] parts = str.Split(',', '，');
            if (parts.Length == 2
                && !string.IsNullOrWhiteSpace(parts[0])
                && !string.IsNullOrWhiteSpace(parts[1]))
            {
                try
                {
                    pt = new Point(double.Parse(parts[0]), double.Parse(parts[1]));
                    return true;
                }
                catch (Exception)
                {
                }
            }
            pt = new Point();
            return false;
        }
        private bool TryTextToPoint(TextBox tb, out Point pt)
        {
            if (TryGetPoint(tb.Text, out Point pt1))
            {
                pt = pt1;
                tb.Background = SystemColors.WindowBrush;
                return true;
            }
            else
            {
                pt = new Point();
                tb.Background = Brushes.Orange;
                return false;
            }
        }

        private void tb_bottomRight_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isLoadingInfo)
                return;
            // bottom right
            if (TryTextToPoint(tb_bottomRight, out Point pt))
            {
                pt_mapBottomRight = pt;
                needSave = true;
            }
        }

        #endregion


        #region 设定三角定位点

        private void tb_pt1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isLoadingInfo)
                return;
            // 记录图片上的像素点位置
            if (TryTextToPoint(tb_pt1, out Point pt))
            {
                pt1 = pt;
                needSave = true;
            }
        }
        private void tb_pt2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isLoadingInfo)
                return;
            if (TryTextToPoint(tb_pt2, out Point pt))
            {
                pt2 = pt;
                needSave = true;
            }
        }
        private void tb_pt3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isLoadingInfo)
                return;
            if (TryTextToPoint(tb_pt3, out Point pt))
            {
                pt3 = pt;
                needSave = true;
            }
        }

        private void tb_posi1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isLoadingInfo)
                return;
            // 记录经纬坐标位置
            if (TryTextToPoint(tb_posi1, out Point pt))
            {
                posi1 = pt;
                needSave = true;
            }

        }
        private void tb_posi2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isLoadingInfo)
                return;
            if (TryTextToPoint(tb_posi2, out Point pt))
            {
                posi2 = pt;
                needSave = true;
            }
        }
        private void tb_posi3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isLoadingInfo)
                return;
            if (TryTextToPoint(tb_posi3, out Point pt))
            {
                posi3 = pt;
                needSave = true;
            }
        }

        #endregion


        #endregion


        #region 按当前信息，在地图上做标记

        private void TrySetMapBoundryChain()
        {
            if (img_mapOrigin.Source == null)
                return;

            if (pt_mapTopLeft.X < pt_mapBottomRight.X
                && pt_mapTopLeft.Y < pt_mapBottomRight.Y)
            {
                // 在原地图上显示边界
                bdr_mapOrigin.Margin = new Thickness(
                    pt_mapTopLeft.X, pt_mapTopLeft.Y,
                    0, 0);
                bdr_mapOrigin.Width = pt_mapBottomRight.X - pt_mapTopLeft.X;
                bdr_mapOrigin.Height = pt_mapBottomRight.Y - pt_mapTopLeft.Y;
                bdr_mapOrigin.Visibility = Visibility.Visible;

                // 更新主地图
                BitmapSource tarMap = (BitmapSource)img_mapOrigin.Source;
                tarMap = QuickGraphics.ChopImage(
                    tarMap,
                    (int)Math.Round(pt_mapTopLeft.X),
                    (int)Math.Round(pt_mapTopLeft.Y),
                    (int)Math.Round(tarMap.Width - pt_mapBottomRight.X),
                    (int)Math.Round(tarMap.Height - pt_mapBottomRight.Y));
                img_map.Source = tarMap;

                // （因为三角定位没有变，所以其他标记不用动）
            }
            else
            {
                bdr_mapOrigin.Visibility = Visibility.Collapsed;
            }
        }


        private QuickCheckNCalculate.SpaceChange2D sc2D = null;
        private void TrySetPositionChain()
        {
            sc2D = null;
            if (img_map.Source == null)
                return;

            if (pt1 != pt2 && pt2 != pt3
                && posi1 != posi2 && posi2 != posi3)
            {
                // 重新计算转换坐标系
                sc2D = new QuickCheckNCalculate.SpaceChange2D(pt1, pt2, pt3, posi1, posi2, posi3);

                // 更新现有标记点
                ReSetPinsAsync(tb_user.Text);
            }
        }

        #endregion


        private void tb_user_TextChanged(object sender, TextChangedEventArgs e)
        {
            ReSetPinsAsync(tb_user.Text);

            if (!isLoadingInfo)
                needSave = true;
        }

        private PointPinData tb_user_SelectionChanged_prePP = null;
        private void tb_user_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (ReSetPinsAsync_task != null)
                return;

            if (sc2D != null)
            {
                // 如果当前选区是一个坐标，则让对应的标记高亮；
                string tx = null;
                int ss = -1, sl = -1;
                Dispatcher.Invoke(() =>
                {
                    tx = tb_user.Text;
                    ss = tb_user.SelectionStart;
                    sl = tb_user.SelectionLength;
                });
                PointPinData foundPP = null;

                string newLine = Environment.NewLine;
                if (ss >= 0 && !tx.Substring(ss, sl).Contains(newLine))
                {
                    string testTx = tx.Substring(0, ss);
                    int testIdx = 0, countLines = 0;
                    do
                    {
                        if (countLines <= 0)
                        {
                            testIdx = testTx.IndexOf(newLine);
                        }
                        else
                        {
                            testIdx = testTx.IndexOf(newLine, testIdx + 1);
                        }
                        ++countLines;
                    }
                    while (testIdx >= 0);

                    foundPP = allPtPins.Find(pp => pp.lineIdx == countLines - 1);
                }

                Dispatcher.Invoke(() =>
                {
                    if (tb_user_SelectionChanged_prePP != null)
                    {
                        tb_user_SelectionChanged_prePP.ui.IsHighLighted = false;
                    }
                    if (foundPP != null)
                    {
                        foundPP.ui.IsHighLighted = true;
                    }
                });
                tb_user_SelectionChanged_prePP = foundPP;
            }
        }

        private class PointPinData
        {
            public Point pt;
            public int lineIdx;
            public Ctrls.Pin ui;
        }
        private List<PointPinData> allPtPins = new List<PointPinData>();
        private void ClearPtPins()
        {
            allPtPins.Clear();
            Dispatcher.Invoke(() =>
            {
                if (grid_pins != null)
                    grid_pins.Children.Clear();
            });
        }

        private Task ReSetPinsAsync_task = null;
        private string ReSetPinsAsync_newTx, ReSetPinsAsync_handledTx;
        private async void ReSetPinsAsync(string newTx)
        {
            if (string.IsNullOrWhiteSpace(newTx))
                return;
            ReSetPinsAsync_newTx = newTx;
            if (ReSetPinsAsync_task != null)
                return;

            ReSetPinsAsync_task = Task.Run(() =>
            {
                while (ReSetPinsAsync_newTx != ReSetPinsAsync_handledTx)
                {
                    string newTx = ReSetPinsAsync_newTx;
                    if (sc2D == null)
                    {
                        ClearPtPins();
                    }
                    else
                    {

                        // 获取用户输入中的所有坐标点
                        List<PointPinData> curPPList = new List<PointPinData>();

                        string[] lines = newTx.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                        // 获取和检查文本中的坐标，如果之前存在，则预留，不存在，则清除
                        int i1;
                        string str;
                        Point outPt;
                        PointPinData foundPP;
                        for (int i = 0, iv = lines.Length; i < iv; ++i)
                        {
                            str = lines[i];
                            i1 = str.IndexOf("//");
                            if (i1 >= 0)
                                str = str.Substring(0, i1);

                            if (TryGetPoint(str, out outPt))
                            {
                                foundPP = allPtPins.Find(p => p.pt == outPt);
                                if (foundPP == null)
                                {
                                    curPPList.Add(new PointPinData()
                                    { pt = outPt, lineIdx = i });
                                }
                                else
                                {
                                    foundPP.lineIdx = i;
                                    curPPList.Add(foundPP);
                                    allPtPins.Remove(foundPP);
                                }
                            }
                        }


                        // 清除文本中没有出现的坐标标记
                        Dispatcher.Invoke(() =>
                        {
                            foreach (PointPinData pp in allPtPins)
                            {
                                grid_pins.Children.Remove(pp.ui);
                            }
                        });
                        allPtPins.Clear();

                        // 对新增加的坐标，在地图上增加标记
                        allPtPins.AddRange(curPPList);
                        Ctrls.Pin newUI;
                        Point uiPt;
                        foreach (PointPinData pp in allPtPins)
                        {
                            if (pp.ui == null)
                            {
                                uiPt = sc2D.GetOppositePoint(false, pp.pt);
                                Dispatcher.Invoke(() =>
                                {
                                    newUI = new Ctrls.Pin()
                                    { Margin = new Thickness(uiPt.X - pt_mapTopLeft.X, uiPt.Y - pt_mapTopLeft.Y, 0, 0) };
                                    pp.ui = newUI;
                                    grid_pins.Children.Add(newUI);
                                });
                            }
                        }
                    }
                    ReSetPinsAsync_handledTx = newTx;
                }
            });
            await ReSetPinsAsync_task;
            ReSetPinsAsync_task = null;

            tb_user_SelectionChanged(null, null);
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TrySaveAllInfo();
        }
    }
}
