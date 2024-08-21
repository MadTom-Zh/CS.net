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

namespace DroplitzHelper.UCs
{
    /// <summary>
    /// UC_MainPanel.xaml 的交互逻辑
    /// </summary>
    public partial class UC_MainPanel : UserControl
    {
        public UC_MainPanel()
        {
            InitializeComponent();
            SPMgr = new Classes.SwitchPanelMgr();
            SPMgr.SizeChanged += spMgr_SizeChanged;
            SPMgr.MatrixExpand(1, 0);
        }
        public Classes.SwitchPanelMgr SPMgr;
        public event EventHandler SPMgrComputingStart;
        public event EventHandler SPMgrComputingEnd;
        public void SPMgrReCalculateWays(bool maxLength)
        {
            if (SPMgrComputingStart != null) SPMgrComputingStart(this,new EventArgs());
            SPMgr.ReCalculateWays(maxLength);
            if (SPMgrComputingEnd != null) SPMgrComputingEnd(this, new EventArgs());
        }

        public int TapCount
        {
            get
            {
                return SPMgr.TapCount;
            }
        }
        public int RowCount
        {
            get
            {
                return SPMgr.RowCount;
            }
        }
        public void IncreaseRow(int increment)
        {
            SPMgr.MatrixExpand(0, increment);
        }
        public void IncreaseTap(int increment)
        {
            SPMgr.MatrixExpand(increment, 0);
        }

        void spMgr_SizeChanged(object sender, EventArgs e)
        {
            ReFreshUI();
        }
        double hWidthMuti = UC_Switch.StaticHeight * Math.Sin(Math.PI / 3);
        private void ReFreshUI()
        {
            this.gridContainer.Children.Clear();

            double posiTapVHeightBase = UC_Switch.StaticHeight / 2;
            double vHeightBase;

            UC_MainPanel mainPanel = (UC_MainPanel)(gridContainer.Parent);
            mainPanel.Width = hWidthMuti * (SPMgr.TapCount * 2) + UC_Switch.StaticWidth;
            mainPanel.Height = UC_Switch.StaticHeight * (SPMgr.RowCount + 2.5);

            Classes.Switch6Ways dataItem;
            UC_Switch switchUI;
            for (int colIdx = SPMgr.TapCount * 2; colIdx >= 0; colIdx--)
            {
                for (int rowIdx = SPMgr.RowCount + 1; rowIdx >= 0; rowIdx--)
                {
                    dataItem = SPMgr.GetSwitchItem(colIdx, rowIdx);
                    if (dataItem != null)
                    {
                        switchUI = dataItem.UI;
                        switchUI.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        switchUI.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        if (colIdx % 2 == 0)
                        {
                            vHeightBase = 0;
                        }
                        else
                        {
                            vHeightBase = posiTapVHeightBase;
                        }
                        switchUI.Margin
                            = new Thickness(
                                hWidthMuti * colIdx,
                                (mainPanel.Height - UC_Switch.StaticHeight * (rowIdx + 1)) - vHeightBase,
                                0,
                                0
                                );
                        this.gridContainer.Children.Add(switchUI);
                    }
                }
            }
        }

        public void TrySendKey(Key key, Point gridContainerMicePosi)
        {
            if (gridContainerMicePosi.X < 0
                || gridContainerMicePosi.X > this.gridContainer.Width)
                return;
            else if (gridContainerMicePosi.Y < 0
                || gridContainerMicePosi.Y > this.gridContainer.Height)
                return;
            else
            {
                Classes.SwitchPanelMgr.Point gmPosi
                    = GetMiceSwitchPoint(gridContainerMicePosi);

                SetSwitchUI(gmPosi.X, gmPosi.Y, key);
            }
        }
        public void TrySendMiceDown(bool isLeftBtnOrRight, Point gridContainerMicePosi)
        {
            if (gridContainerMicePosi.X < 0
                || gridContainerMicePosi.X > this.gridContainer.Width)
                return;
            else if (gridContainerMicePosi.Y < 0
                || gridContainerMicePosi.Y > this.gridContainer.Height)
                return;
            else
            {
                Classes.SwitchPanelMgr.Point gmPosi
                    = GetMiceSwitchPoint(gridContainerMicePosi);
                Classes.Switch6Ways item = SPMgr.GetSwitchItem(gmPosi.X, gmPosi.Y);
                if (item != null)
                {
                    if (isLeftBtnOrRight == true)
                    {
                        item.TurnWaysLeft(1);
                    }
                    else
                    {
                        item.TurnWaysRight(1);
                    }
                }
            }
        }
        private Classes.SwitchPanelMgr.Point GetMiceSwitchPoint(Point gridContainerMicePoint)
        {
            Classes.SwitchPanelMgr.Point result = new Classes.SwitchPanelMgr.Point();
            result.X = (int)(((gridContainerMicePoint.X - (UC_Switch.StaticWidth - hWidthMuti) / 2)) / hWidthMuti);

            if (result.X % 2 == 0)
            {
                result.Y = (int)((gridContainer.ActualHeight - gridContainerMicePoint.Y) / UC_Switch.StaticHeight);
            }
            else
            {
                result.Y = (int)((gridContainer.ActualHeight - gridContainerMicePoint.Y - (UC_Switch.StaticHeight / 2)) / UC_Switch.StaticHeight);
            }
            return result;
        }
        private void SetSwitchUI(int colIdx, int rowIdx, Key key)
        {
            Classes.Switch6Ways s6wData = SPMgr.GetSwitchItem(colIdx, rowIdx);
            if (s6wData == null) return;
            if (s6wData.MyType == Classes.Switch6Ways.Type.START
                || s6wData.MyType == Classes.Switch6Ways.Type.END)
            {
                return;
            }
            switch (key)
            {
                case Key.D1:
                    s6wData.MyType = Classes.Switch6Ways.Type.U;
                    break;
                case Key.D2:
                    s6wData.MyType = Classes.Switch6Ways.Type.I;
                    break;
                case Key.W:
                    s6wData.MyType = Classes.Switch6Ways.Type.L;
                    break;
                case Key.D3:
                    s6wData.MyType = Classes.Switch6Ways.Type.Y;
                    break;
                case Key.D4:
                    s6wData.MyType = Classes.Switch6Ways.Type.X;
                    break;
                case Key.Back:
                    SPMgr.KillAtPoint(colIdx, rowIdx);
                    break;
            }
        }
    }
}
