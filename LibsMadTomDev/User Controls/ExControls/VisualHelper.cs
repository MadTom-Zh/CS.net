using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace MadTomDev.UI
{
    public class VisualHelper
    {
        public static DependencyObject GetSubVisual(Visual container, Point pt)
        {
            HitTestResult hitTestResults = VisualTreeHelper.HitTest(container, pt);
            if (hitTestResults != null && hitTestResults.VisualHit is Visual)
            {
                return hitTestResults.VisualHit;
            }
            return null;
        }


        /// <summary>
        /// * 请将TreeViewNodeModelBase作为节点数据的基类；
        /// </summary>
        public class TreeView
        {
            /// <summary>
            /// 从位置点获取节点数据模型
            /// </summary>
            /// <param name="treeview"></param>
            /// <param name="pt"></param>
            /// <returns></returns>
            public static object GetNodeVM(System.Windows.Controls.TreeView treeview, Point pt)//, bool ignoreLeftRightSpace = true)
            {
                HitTestResult hitTestResults = VisualTreeHelper.HitTest(treeview, pt);
                if (hitTestResults != null
                    && hitTestResults.VisualHit is FrameworkElement)
                {
                    return (hitTestResults.VisualHit as
                        FrameworkElement).DataContext;
                }
                return null;
            }

            /// <summary>
            /// 按节点TreeViewItem，获取这个ui的当前位置和尺寸
            /// </summary>
            /// <param name="treeview"></param>
            /// <param name="nodeUI"></param>
            /// <returns></returns>
            public static Rect GetNodeBoundry(System.Windows.Controls.TreeView treeview, System.Windows.Controls.TreeViewItem nodeUI)
            {
                Rect result;
                treeview.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                {
                    GeneralTransform trans = nodeUI.TransformToAncestor(treeview);
                    Point posi = trans.Transform(new Point(0, 0));
                    Size size = nodeUI.DesiredSize;

                    result = new Rect(posi.X, posi.Y, size.Width, size.Height);
                }));
                return result;
            }

            /// <summary>
            /// 获取从根节点到当前节点的节点链
            /// </summary>
            /// <param name="treeview"></param>
            /// <param name="lastNodeVM"></param>
            /// <returns></returns>
            public static List<object> GetNodeChain(System.Windows.Controls.TreeView treeview, object lastNodeVM)
            {
                List<object> nodeChain = new List<object>();
                nodeChain.Add(lastNodeVM);
                object parent = ((VMBase.TreeViewNodeModelBase)lastNodeVM).parent;
                while (parent != null)
                {
                    nodeChain.Insert(0, parent);
                    parent = ((VMBase.TreeViewNodeModelBase)parent).parent;
                }
                return nodeChain;
            }


            /// <summary>
            /// 按节点数据模型，获取节点ui；
            /// * 执行此方法后，请读取静态数据 GetNodeUI_gotten；
            /// </summary>
            /// <param name="treeview"></param>
            /// <param name="nodeVM"></param>
            public static System.Windows.Controls.TreeViewItem GetNodeUI(System.Windows.Controls.TreeView treeview, object nodeVM)
            {
                System.Windows.Controls.TreeViewItem result = null;
                List<object> nodeChain = GetNodeChain(treeview, nodeVM);

                treeview.Dispatcher.Invoke(async () =>
                {
                    System.Windows.Controls.TreeViewItem ui
                          = (System.Windows.Controls.TreeViewItem)(treeview.ItemContainerGenerator.ContainerFromItem(nodeChain[0]));

                    System.Windows.Controls.TreeViewItem test;
                    int waitCount = 0;
                    while (ui == null)
                    {
                        await Task.Delay(10);
                        if (++waitCount >= 20)
                            return;

                        ui = (System.Windows.Controls.TreeViewItem)(treeview.ItemContainerGenerator.ContainerFromItem(nodeChain[0]));
                    }
                    for (int i = 1, iv = nodeChain.Count; i < iv; i++)
                    {
                        test = (System.Windows.Controls.TreeViewItem)ui.ItemContainerGenerator.ContainerFromItem(nodeChain[i]);
                        while (test == null)
                        {
                            await Task.Delay(10);
                            if (++waitCount >= 20)
                                return;
                            test = (System.Windows.Controls.TreeViewItem)ui.ItemContainerGenerator.ContainerFromItem(nodeChain[i]);
                        }

                        waitCount = 0;
                        ui = test;
                    }
                    result = ui;
                });
                return result;
            }

            /// <summary>
            /// 按节点数据模型，转换为节点ui，进一步滚动并显示到视野内；
            /// </summary>
            /// <param name="treeview"></param>
            /// <param name="nodeVM"></param>
            public static async void BringIntoView(System.Windows.Controls.TreeView treeview, object nodeVM)
            {
                System.Windows.Controls.TreeViewItem ui = GetNodeUI(treeview, nodeVM);
                int counter = 0;
                while (ui == null)
                {
                    await Task.Delay(10);
                    ui = GetNodeUI(treeview, nodeVM);

                    if (++counter >= 20)
                        return;
                }
                BringIntoView(treeview, ui);
            }
            private static async void BringIntoView(System.Windows.Controls.TreeView treeview, System.Windows.Controls.TreeViewItem nodeUI)
            {
                if (nodeUI != null)
                {
                    nodeUI.BringIntoView();
                    double wndHeight = treeview.ActualHeight;
                    Rect itemBoundry;
                    int counter = 0;
                    do
                    {
                        await Task.Delay(10);
                        itemBoundry = GetNodeBoundry(treeview, nodeUI);
                        if (0 <= itemBoundry.Y && itemBoundry.Y < wndHeight)
                            break;
                    }
                    while (++counter <= 20);
                }
            }
        }

        public class DataGrid
        {
            public static double GetTotalColsWidth(System.Windows.Controls.DataGrid datagrid)
            {
                double result = 0;
                foreach (System.Windows.Controls.DataGridColumn col in datagrid.Columns)
                {
                    result += col.ActualWidth;
                }
                return result;
            }

            /// <summary>
            /// 从位置点获取条目数据模型；
            /// </summary>
            /// <param name="datagrid"></param>
            /// <param name="pt"></param>
            /// <param name="ignoreLeftRightSpace">true-只要竖直范围内侦测到行项目，则返回这个项目；false-点在左边界以左，或超过最右一列的右边，则返回null</param>
            /// <returns></returns>
            public static object GetNodeVM(System.Windows.Controls.DataGrid datagrid, Point pt, bool ignoreLeftRightSpace = true)
            {
                if (!ignoreLeftRightSpace)
                {
                    if (pt.X < datagrid.Padding.Left)
                        return null;

                    if (GetTotalColsWidth(datagrid) + datagrid.Padding.Left < pt.X)
                    {
                        return null;
                    }
                }

                HitTestResult hitTestResults = VisualTreeHelper.HitTest(datagrid, pt);
                if (hitTestResults != null && hitTestResults.VisualHit is FrameworkElement)
                {
                    return (hitTestResults.VisualHit as
                        FrameworkElement).DataContext;
                }
                return null;
            }


            /// <summary>
            /// 以当前视图的纵向范围，获取范围内的项目
            /// </summary>
            /// <param name="datagrid"></param>
            /// <param name="yMin"></param>
            /// <param name="yMax"></param>
            /// <param name="testHeightInterval"></param>
            /// <returns></returns>
            public static List<object> GetNodesVMList(System.Windows.Controls.DataGrid datagrid, double yMin, double yMax, double testHeightInterval = 16)
            {

                List<object> foundItems = new List<object>();
                object testItem;
                double dgLeft = datagrid.Padding.Left + 1;
                double y = yMin;
                while (true)
                {
                    testItem = UI.VisualHelper.DataGrid.GetNodeVM(datagrid, new Point(dgLeft, y));
                    if (testItem == null)
                    {
                        if (foundItems.Count > 0)
                        {
                            break;
                        }
                        else
                        {
                            if (!AddY())
                                break;
                            continue;
                        }
                    }
                    if (!foundItems.Contains(testItem))
                    {
                        foundItems.Add(testItem);
                    }
                    if (!AddY())
                        break;
                }
                bool AddY()
                {
                    if (y >= yMax)
                        return false;
                    y += testHeightInterval;
                    if (yMax < y)
                        y = yMax;
                    return true;
                }
                return foundItems;
            }


            /// <summary>
            /// 通过两步滚动(增加5ms等待)，来控制目标进入到窗口的位置；
            /// * 如果想将条目显示到顶端，则先滚动到底部，然后再滚动到目标条目；
            /// * 如果想将条目显示到底部，则先滚动到顶端，然后再滚动到目标条目；
            /// </summary>
            /// <param name="datagrid"></param>
            /// <param name="itemVM_firstScroll">先滚动到这一条，随后滚动到itemVM位置；如果为null，则直接滚动到itemVM；</param>
            /// <param name="itemVM"></param>
            public static async void ScrollIntoView(System.Windows.Controls.DataGrid datagrid, object itemVM_firstScroll, object itemVM)
            {
                object itemToView = null;
                if (itemVM_firstScroll != null)
                {
                    datagrid.ScrollIntoView(itemVM_firstScroll);
                    itemToView = itemVM_firstScroll;
                }
                if (itemVM != null)
                {
                    datagrid.ScrollIntoView(itemVM);
                    itemToView = itemVM;
                }

                if (itemToView != null)
                {
                    System.Windows.Controls.DataGridRow rowUI = GetItemUI(datagrid, itemToView);
                    double wndHeight = datagrid.ActualHeight;
                    Rect? rowBoundry;
                    Rect rowBoundry1;
                    int counter = 0;
                    do
                    {
                        await Task.Delay(5);
                        rowBoundry = GetItemBoundry(datagrid, rowUI);
                        if (rowBoundry == null)
                            break;
                        rowBoundry1 = (Rect)rowBoundry;
                        if (0 <= rowBoundry1.Y && rowBoundry1.Y < wndHeight)
                            break;
                    }
                    while (++counter <= 20);
                }
            }

            /// <summary>
            /// 按条目数据模型，获取条目ui；
            /// </summary>
            /// <param name="datagrid"></param>
            /// <param name="itemVM"></param>
            /// <returns></returns>
            public static System.Windows.Controls.DataGridRow GetItemUI(System.Windows.Controls.DataGrid datagrid, object itemVM)
            {
                return (System.Windows.Controls.DataGridRow)datagrid.ItemContainerGenerator.ContainerFromItem(itemVM);
            }

            /// <summary>
            /// 按条目ui，获取此ui当前位置和尺寸；
            /// </summary>
            /// <param name="datagrid"></param>
            /// <param name="itemUI"></param>
            /// <returns></returns>
            public static Rect? GetItemBoundry(System.Windows.Controls.DataGrid datagrid, System.Windows.Controls.DataGridRow itemUI)
            {
                try
                {
                    GeneralTransform trans = itemUI.TransformToAncestor(datagrid);
                    Point posi = trans.Transform(new Point(0, 0));
                    Size size = itemUI.DesiredSize;
                    return new Rect(posi.X, posi.Y, size.Width, size.Height);
                }
                catch (Exception)
                {
                }
                return null;
            }
        }
    }
}
