using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace MadTomDev.UI
{
    public class WindowInScreen
    {
        public WindowInScreen()
        {
            ReloadScreenInfo();
        }
        public class ScreenInfo
        {
            public ScreenInfo(string deviceName, bool isPrimary, Rectangle bounds)
            {
                this.deviceName = deviceName;
                this.isPrimary = isPrimary;
                this.bounds = bounds;
            }
            public string deviceName;
            public bool isPrimary;
            public Rectangle bounds;
            public int X { get => bounds.X; }
            public int Y { get => bounds.Y; }
            public int Width { get => bounds.Width; }
            public int Height { get => bounds.Height; }

            public int Left { get => bounds.X; }
            public int Top { get => bounds.Y; }
            public int Right { get => bounds.X + bounds.Width; }
            public int Bottom { get => bounds.Y + bounds.Height; }


        }
        private List<ScreenInfo> allScreens = new List<ScreenInfo>();
        public void ReloadScreenInfo()
        {
            allScreens.Clear();
            foreach (Screen s in Screen.AllScreens)
            {
                allScreens.Add(new ScreenInfo(
                    s.DeviceName,
                    s.Primary,
                    s.Bounds
                ));
            }
        }

        public enum PointPositionRelations
        {
            OutScreen,
            OnBoundary,
            InScreen,
        }
        public enum WindowPositionRelations
        {
            OutScreen,
            InScreen, InScreenMultiScreen,
            Partial, PartialMultiScreen,
        }

        public PointPositionRelations CheckPoint(System.Windows.Point point, out ScreenInfo? inScreen)
        {
            inScreen = null;
            foreach (ScreenInfo s in allScreens)
            {
                if (point.X < s.Left || point.X > s.Right
                    || point.Y < s.Top || point.Y > s.Bottom)
                {
                    return PointPositionRelations.OutScreen;
                }
                else if (point.X == s.Left || point.X == s.Right
                    || point.Y == s.Top || point.Y == s.Bottom)
                {
                    inScreen = s;
                    return PointPositionRelations.OnBoundary;
                }
                else
                {
                    inScreen = s;
                    return PointPositionRelations.InScreen;
                }
            }
            return PointPositionRelations.OutScreen;
        }

        public WindowPositionRelations CheckWindow(Window window, out ScreenInfo? nearestcreen)
        {
            nearestcreen = null;
            WindowPositionRelations result = WindowPositionRelations.OutScreen;

            double wndL = window.Left;
            double wndT = window.Top;
            double wndR = window.Left + window.Width;
            double wndB = window.Top + window.Height;

            ScreenInfo? wndTL, wndTR, wndBL, wndBR;
            PointPositionRelations prTL = CheckPoint(new System.Windows.Point(wndL, wndT), out wndTL);
            PointPositionRelations prTR = CheckPoint(new System.Windows.Point(wndR, wndT), out wndTR);
            PointPositionRelations prBL = CheckPoint(new System.Windows.Point(wndL, wndB), out wndBL);
            PointPositionRelations prBR = CheckPoint(new System.Windows.Point(wndR, wndB), out wndBR);

            //检查窗口跨越了多少个屏幕
            int countDifWindows = 0;
            List<ScreenInfo> tmpSILis = new List<ScreenInfo>();
            CheckWndNCount(prTL, wndTL);
            CheckWndNCount(prTR, wndTR);
            CheckWndNCount(prBL, wndBL);
            CheckWndNCount(prBR, wndBR);

            if (tmpSILis.Count == 0)
            {
                // 屏幕之外，或在边界
                result = WindowPositionRelations.OutScreen;
                // 检查和哪个屏幕最近
                double minGap = double.MaxValue, test = 0;
                foreach (ScreenInfo si in allScreens)
                {
                    test = GetMaxGap(si);
                    if (minGap > test)
                    {
                        minGap = test;
                        nearestcreen = si;
                    }
                }
            }
            else if (tmpSILis.Count == 1)
            {
                // 和一个屏幕有交集
                if (prTL == PointPositionRelations.OutScreen
                    || prTR == PointPositionRelations.OutScreen
                    || prBL == PointPositionRelations.OutScreen
                    || prBR == PointPositionRelations.OutScreen)
                {
                    result = WindowPositionRelations.Partial;
                }
                else
                {
                    result = WindowPositionRelations.InScreen;
                }

                nearestcreen = tmpSILis[0];
            }
            else
            {
                // 跨越了多个屏幕
                if (prTL == PointPositionRelations.OutScreen
                    || prTR == PointPositionRelations.OutScreen
                    || prBL == PointPositionRelations.OutScreen
                    || prBR == PointPositionRelations.OutScreen)
                {
                    return WindowPositionRelations.PartialMultiScreen;
                }
                else
                {
                    return WindowPositionRelations.InScreenMultiScreen;
                }
                // 检查在哪个屏幕中占比最多
                double maxArea = 0, testArea = 0;
                if (wndTL != null)
                {
                    testArea = GetArea(wndTL);
                    if (maxArea < testArea)
                    {
                        maxArea = testArea;
                        nearestcreen = wndTL;
                    }
                }
                if (wndTR != null)
                {
                    testArea = GetArea(wndTR);
                    if (maxArea < testArea)
                    {
                        maxArea = testArea;
                        nearestcreen = wndTR;
                    }
                }
                if (wndBL != null)
                {
                    testArea = GetArea(wndBL);
                    if (maxArea < testArea)
                    {
                        maxArea = testArea;
                        nearestcreen = wndBL;
                    }
                }
                if (wndBR != null)
                {
                    testArea = GetArea(wndBR);
                    if (maxArea < testArea)
                    {
                        maxArea = testArea;
                        nearestcreen = wndBR;
                    }
                }
            }


            return WindowPositionRelations.OutScreen;

            void CheckWndNCount(PointPositionRelations ppr, ScreenInfo? si)
            {
                if (ppr == PointPositionRelations.InScreen)
                {
                    ++countDifWindows;
                    if (!tmpSILis.Contains(si))
                    {
                        tmpSILis.Add(si);
                    }
                }
            }
            double GetMaxGap(ScreenInfo si)
            {
                double max = 0, test = 0;
                if (si.Left > wndR)
                {
                    test = si.Left - wndR;
                    if (max < test) max = test;
                }
                else if (wndL > si.Right)
                {
                    test = wndL - si.Right;
                    if (max < test) max = test;
                }

                if (si.Top > wndB)
                {
                    test = si.Top - wndB;
                    if (max < test) max = test;
                }
                else if (wndT > si.Bottom)
                {
                    test = wndT - si.Bottom;
                    if (max < test) max = test;
                }

                return max;
            }
            double GetArea(ScreenInfo si)
            {
                double result = 0, areaWidth, areaHeight;

                if (si.Left < wndL && wndL < si.Right)
                {
                    if (si.Right < wndR)
                    {
                        areaWidth = si.Right - wndL;
                    }
                    else
                    {
                        areaWidth = wndR - wndL;
                    }
                }
                else if (si.Left < wndR && wndR < si.Right)
                {
                    if (wndL < si.Left)
                    {
                        areaWidth = wndR - si.Left;
                    }
                    else
                    {
                        areaWidth = wndR - wndL;
                    }
                }
                else
                {
                    areaWidth = 0;
                }

                if (si.Top < wndT && wndT < si.Bottom)
                {
                    if (si.Bottom < wndB)
                    {
                        areaHeight = si.Bottom - wndT;
                    }
                    else
                    {
                        areaHeight = wndB - wndT;
                    }
                }
                else if (si.Top < wndB && wndB < si.Bottom)
                {
                    if (wndT < si.Top)
                    {
                        areaHeight = wndB - si.Top;
                    }
                    else
                    {
                        areaHeight = wndB - wndT;
                    }
                }
                else
                {
                    areaHeight = 0;
                }

                return areaWidth * areaHeight;
            }
        }

        public bool TryMoveWindowInScreen(Window window, Rectangle? guaranteeWindowBounds = null)
        {
            ScreenInfo? nearestScreen;
            WindowPositionRelations wpr = CheckWindow(window, out nearestScreen);
            if (nearestScreen != null
                && (wpr != WindowPositionRelations.InScreen
                    || wpr != WindowPositionRelations.InScreenMultiScreen))
            {
                Rectangle guaranteeRect;
                if (guaranteeWindowBounds == null)
                {
                    guaranteeRect = new Rectangle(0, 0, (int)window.Width, (int)window.Height);
                }
                else
                {
                    guaranteeRect = (Rectangle)guaranteeWindowBounds;
                }
                // 将窗口移动到最近的屏幕，保证区域必须全部在窗口中
                Rectangle curGRect = new Rectangle((int)window.Left + guaranteeRect.Left, (int)window.Top + guaranteeRect.Top, guaranteeRect.Width, guaranteeRect.Height);
                Rectangle screenRect = nearestScreen.bounds;
                QuickCheckNCalculate.GraphRelations gr = QuickCheckNCalculate.CheckRectangleRelation(curGRect, screenRect);
                if (gr == QuickCheckNCalculate.GraphRelations.Containing)
                {
                    // 保证区域已经在屏幕中
                    return false;
                }
                else
                {
                    // 将保证区域，完全移动到屏幕中；
                    int offX = 0, offY = 0;
                    if (curGRect.Left < screenRect.Left)
                    {
                        offX = screenRect.Left - curGRect.Left;
                    }
                    else if (screenRect.Right < curGRect.Right)
                    {
                        offX = screenRect.Right - curGRect.Right;
                    }
                    if (curGRect.Top < screenRect.Top)
                    {
                        offY = screenRect.Top - curGRect.Top;
                    }
                    else if (screenRect.Bottom < curGRect.Bottom)
                    {
                        offY = screenRect.Bottom - curGRect.Bottom;
                    }
                    window.Left += offX;
                    window.Top += offY;
                    return true;
                }
            }


            return false;
        }
    }
}
