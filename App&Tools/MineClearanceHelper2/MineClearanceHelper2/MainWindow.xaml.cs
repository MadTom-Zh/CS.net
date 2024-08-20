using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MadTomDev.App.Ctrls;
using MadTomDev.App.Classes;
using System.IO;
using System.Windows.Controls.Primitives;

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
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.EventOccurred += (s1, e1) =>
            {
                //if (e1.error != null)
                //{
                //    MessageBox.Show(this, e1.error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //}
                //else
                //{
                //    tbv_msg.Text = e1.msg;
                //}
                if (e1.EALevel == EventOccurredArgs.Level.Error) tbv_msg.Text = e1.error.Message;
                else tbv_msg.Text = e1.msg;
            };
            //return;

            while (blockMatrix == null)
            {
                await Task.Delay(5);
            }

            // for testing
            //PresetLib.Preset02(this);


            ReLoadPresets();
        }

        #region presets

        List<PresetLib.PresetData> presets;
        private void ReLoadPresets()
        {
            sp_presets.Children.Clear();
            presets = PresetLib.LoadPresets();
            foreach (PresetLib.PresetData p in presets)
            {
                LoadPreset(p);
            }
        }
        private void LoadPreset(PresetLib.PresetData data)
        {
            ToggleButton tb = new ToggleButton()
            {
                Content = data.Name,
                IsThreeState = false,
                Tag = data,
                MinWidth = 16,
                Margin = new Thickness(1,0,1,0),
                Focusable = false,
            };
            tb.Checked += Tb_Checked;
            sp_presets.Children.Add(tb);
        }

        private void Tb_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null
                || sender is not ToggleButton)
            {
                return;
            }
            ToggleButton tb = (ToggleButton)sender;
            if (tb.Tag is not PresetLib.PresetData)
            {
                return;
            }
            PresetLib.PresetData preset = (PresetLib.PresetData)tb.Tag;

            // clear board
            DisposeAllBlocks();

            // set preset
            PresetLib.SetPreset(this, preset);
        }

        private void btn_presetsRemove_Click(object sender, RoutedEventArgs e)
        {
            List<ToggleButton> tbToRemove = new List<ToggleButton>();
            foreach (UIElement ui in sp_presets.Children)
            {
                if (ui is ToggleButton)
                {
                    ToggleButton tb = (ToggleButton)ui;
                    if (tb.IsChecked != true)
                    {
                        continue;
                    }
                    PresetLib.DeletePreset(tb.Content.ToString());
                    if (tb.Tag is PresetLib.PresetData)
                    {
                        presets.Remove((PresetLib.PresetData)tb.Tag);
                    }
                    tbToRemove.Add(tb);
                }
            }
            foreach (ToggleButton tb in tbToRemove)
            {
                sp_presets.Children.Remove(tb);
            }
        }

        private WindowInputBox inputBox;
        private void btn_presetAdd_Click(object sender, RoutedEventArgs e)
        {
                inputBox = new WindowInputBox()
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    InputText = "[title]",
                };
            if (inputBox.ShowDialog() == true)
            {
                PresetLib.PresetData? newData = PresetLib.SavePreset(inputBox.InputText, GetBoardOffset(), blockMatrix);
                if (newData != null)
                {
                    presets.Add(newData);
                    LoadPreset(newData);
                }
            }
        }
        private System.Drawing.Point GetBoardOffset()
        {
            double scale = gridLines.RenderTransform.Value.M11;
            int x = (int)((gridView.Margin.Left / scale - gridView_negativeExtend.X) / OneBlock.LEN_SIDE + 0.5);
            int y = (int)((gridView.Margin.Top / scale - gridView_negativeExtend.Y) / OneBlock.LEN_SIDE + 0.5);
            return new System.Drawing.Point(x, y);
        }
        public void SetBoardOffset(System.Drawing.Point offset)
        {
            double scale = gridLines.RenderTransform.Value.M11;
            double left = (gridView_negativeExtend.X + offset.X * OneBlock.LEN_SIDE) * scale;
            double top = (gridView_negativeExtend.Y + offset.Y * OneBlock.LEN_SIDE) * scale;
            gridView.Margin = new Thickness(left, top, 0, 0);
            GridParent_SizeChanged();
        }

        #endregion



        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // not working anymore
            //uc_scrollableBG.SetKeyDown(e.Key);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            // not working anymore
            //uc_scrollableBG.SetKeyUp();
        }


        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            SetKeyDown(e.Key);
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            SetKeyUp();
        }






        #region status text, and grid lines
        public class EventOccurredArgs : EventArgs
        {
            public enum Level
            {
                Info,
                Warning,
                Error,
            }
            public Level EALevel;
            public string msg;
            public Exception error;
        }
        public event EventHandler<EventOccurredArgs> EventOccurred;

        private List<Line> gridLinesV = new List<Line>();
        private List<Line> gridLinesH = new List<Line>();
        private void gridView_Loaded(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("loaded");
            //Grid par = (Grid)Parent;
            //double windowWidth = par.ActualWidth;
            //double windowHeight = par.ActualHeight;

            //gridView.Width = 100;
            //gridView.Height = 100;
            //gridView.Margin = new Thickness(windowWidth / 2, windowHeight / 2, 0, 0);

            //gridLines.Width = windowWidth * 3;
            //gridLines.Height = windowHeight * 3;

            //GridLines_Plant();
            //GridLines_PosiTrimming();

            gridLines.HorizontalAlignment
                = gridView.HorizontalAlignment
                = System.Windows.HorizontalAlignment.Left;
            gridLines.VerticalAlignment
                = gridView.VerticalAlignment
                = System.Windows.VerticalAlignment.Top;

            blockMatrix = new BlockMatrix();
            blockMatrix.Flag_Mines += blockMatrix_Flag_Mines;
            blockMatrix.Flag_Clears += blockMatrix_Flag_Clears;
            blockMatrix.Flag_Unknows += blockMatrix_Flag_Unknows;

            blockMatrix.Call_GridViewRemoveChild += blockMatrix_Call_GridViewRemoveChild;

            gridViewAndLines_Scale(1.5, new Point(-gridView.Margin.Left, -gridView.Margin.Top));
        }

        void blockMatrix_Flag_Unknows(object sender, EventArgs e)
        {
            if (EventOccurred != null)
            {
                EventOccurredArgs args = new EventOccurredArgs();
                args.EALevel = EventOccurredArgs.Level.Info;
                args.msg = "Unknow! :?";
                EventOccurred(this, args);
            }
        }
        void blockMatrix_Flag_Clears(object sender, EventArgs e)
        {
            if (EventOccurred != null)
            {
                EventOccurredArgs args = new EventOccurredArgs();
                args.EALevel = EventOccurredArgs.Level.Info;
                args.msg = "Clear ! :]";
                EventOccurred(this, args);
            }
        }
        void blockMatrix_Flag_Mines(object sender, EventArgs e)
        {
            if (EventOccurred != null)
            {
                EventOccurredArgs args = new EventOccurredArgs();
                args.EALevel = EventOccurredArgs.Level.Warning;
                args.msg = "Mines ! :o";
                EventOccurred(this, args);
            }
        }

        private void GridParent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridParent_SizeChanged();
        }
        private void GridParent_SizeChanged()
        {
            if (gridView == null) return;
            else gridView_Extend();
            if (gridLines == null) return;
            else
            {
                GridLines_Plant();
                GridLines_PosiTrimming();
            }
        }
        private void GridLines_Plant()
        {
            double scale = gridLines.RenderTransform.Value.M11;

            double dou1 = OneBlock.LEN_SIDE * scale;
            int countVLines = (int)(gridContainer.ActualWidth / dou1) * 3;
            int countHLines = (int)(gridContainer.ActualHeight / dou1) * 3;

            // 追加线条
            Line newLint;
            double newLine_StrokeThickness = 0.2;
            Brush bs = new SolidColorBrush(Colors.Black);
            if (gridLinesV.Count < countVLines)
            {
                for (int i = gridLinesV.Count; i <= countVLines; i++)
                {
                    newLint = new Line();
                    newLint.StrokeThickness = newLine_StrokeThickness;
                    newLint.Stroke = bs;
                    newLint.X1 = (i + 1) * OneBlock.LEN_SIDE;
                    newLint.X2 = newLint.X1;
                    gridLines.Children.Add(newLint);
                    gridLinesV.Add(newLint);
                }
            }

            if (gridLinesH.Count < countHLines)
            {
                for (int i = gridLinesH.Count; i <= countHLines; i++)
                {
                    newLint = new Line();
                    newLint.StrokeThickness = newLine_StrokeThickness;
                    newLint.Stroke = bs;
                    newLint.Y1 = (i + 1) * OneBlock.LEN_SIDE;
                    newLint.Y2 = newLint.Y1;
                    gridLines.Children.Add(newLint);
                    gridLinesH.Add(newLint);
                }
            }

            // 重设线条长度
            Line curLine;
            if (gridLinesV.Count > 0 && (gridLinesV[0].Y1 * scale) > (-gridContainer.ActualHeight))
            {
                double dou2 = gridContainer.ActualHeight * 3 / scale;
                for (int i = gridLinesV.Count - 1; i >= 0; i--)
                {
                    curLine = gridLinesV[i];
                    curLine.Y1 = 0;
                    curLine.Y2 = dou2;
                }
            }
            if (gridLinesH.Count > 0 && (gridLinesH[0].X1 * scale) > (-gridContainer.ActualWidth))
            {
                double dou3 = gridContainer.ActualWidth * 3 / scale;
                for (int i = gridLinesH.Count - 1; i >= 0; i--)
                {
                    curLine = gridLinesH[i];
                    curLine.X1 = 0;
                    curLine.X2 = dou3;
                    //gridLinesH[i] = curLine;
                }
            }

        }
        private void GridLines_PosiTrimming()
        {
            double scale = gridLines.RenderTransform.Value.M11;

            gridLines.Width = gridContainer.ActualWidth * 3 / scale;
            gridLines.Height = gridContainer.ActualHeight * 3 / scale;

            double dou2 = OneBlock.LEN_SIDE * scale;
            int xMu = (int)(gridContainer.ActualWidth / dou2) + 1;
            int yMu = (int)(gridContainer.ActualHeight / dou2) + 1;
            gridLines.Margin = new Thickness(
                (gridView.Margin.Left % dou2) - (xMu * dou2),
                (gridView.Margin.Top % dou2) - (yMu * dou2),
                0,
                0);
        }

        #endregion

        #region 左键操作

        private bool startDrag = false;
        private Point window_startDragPoint;
        private Point gridView_clickPoint;
        private Thickness startDrag_margin;
        private Thickness startDrag_margin_GLines;
        private void gridView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (gridView_isMRBDown == true)
            {
                DetectNMark();
                startDrag = false;
                gridView_isMRBDown = false;
                return;
            }
            startDrag = true;
            window_startDragPoint = e.GetPosition((Grid)Parent);
            gridView_mouseMovePoint
                = gridView_clickPoint
                = e.GetPosition(gridView);
            startDrag_margin = gridView.Margin;
            startDrag_margin_GLines = gridLines.Margin;
        }
        private void gridView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (startDrag == true)
            {
                Point endPoint = e.GetPosition((Grid)Parent);
                if (Math.Abs(window_startDragPoint.X - endPoint.X) < 2
                    && Math.Abs(window_startDragPoint.Y - endPoint.Y) < 2)
                {
                    gridView_iClick(gridView_clickPoint, true);
                }
                else
                {
                    gridView_Extend();
                    GridLines_PosiTrimming();
                }
            }
            startDrag = false;
        }

        private Point gridView_negativeExtend = new Point(0, 0);
        private void gridView_Extend()
        {
            Point blockMoveVector = new Point(0, 0);

            double uiScale = gridView.RenderTransform.Value.M11;
            double dou1 = OneBlock.LEN_SIDE * uiScale;

            Thickness orgMargin = gridView.Margin;
            if (gridView.Margin.Left >= 0)
            {
                // 向左扩展
                blockMoveVector.X = (double)MathHelper.Round((gridContainer.ActualWidth + OneBlock.LEN_SIDE) / uiScale, OneBlock.LEN_SIDE);
                gridView_negativeExtend.X -= blockMoveVector.X;
                gridView.Width += blockMoveVector.X * uiScale;
                gridView.Margin = new Thickness(
                    orgMargin.Left - blockMoveVector.X * uiScale,
                    orgMargin.Top,
                    0,
                    0);
            }

            orgMargin = gridView.Margin;
            if (gridView.Margin.Top >= 0)
            {
                // 向上扩展
                blockMoveVector.Y = (double)MathHelper.Round((gridContainer.ActualHeight + OneBlock.LEN_SIDE) / uiScale, OneBlock.LEN_SIDE);
                gridView_negativeExtend.Y -= blockMoveVector.Y;
                gridView.Height += blockMoveVector.Y * uiScale;
                gridView.Margin = new Thickness(
                    orgMargin.Left,
                    orgMargin.Top - blockMoveVector.Y * uiScale,
                    0,
                    0);
            }
            if (blockMoveVector.X > 0 || blockMoveVector.Y > 0)
            {
                gridView_Extend_negaMoveBlocks(blockMoveVector);
            }

            orgMargin = gridView.Margin;
            if (((gridView.Width * uiScale) + orgMargin.Left) <= gridContainer.ActualWidth)
            {
                // 向右扩展
                gridView.Width += (double)MathHelper.Round((gridContainer.ActualWidth + OneBlock.LEN_SIDE) / uiScale, dou1);
            }

            orgMargin = gridView.Margin;
            if (((gridView.Height * uiScale) + orgMargin.Top) <= gridContainer.ActualHeight)
            {
                // 向下扩展
                gridView.Height += (double)MathHelper.Round((gridContainer.ActualHeight + OneBlock.LEN_SIDE) / uiScale, dou1);
            }
        }
        private void gridView_Extend_negaMoveBlocks(Point vactor)
        {
            OneBlock blockUI;
            for (int i = gridView.Children.Count - 1; i >= 0; i--)
            {
                blockUI = (OneBlock)gridView.Children[i];
                blockUI.Margin = new Thickness(
                    blockUI.Margin.Left + vactor.X,
                    blockUI.Margin.Top + vactor.Y,
                    0,
                    0);
            }
        }

        private Point gridView_mouseMovePoint;
        private void gridView_MouseMove(object sender, MouseEventArgs e)
        {
            int colIdx;
            int rowIdx;
            gridView_getClickedItemPosi(out rowIdx, out colIdx);
            tbv_posi.Text = $"Posi({colIdx}, {rowIdx})";

            gridView_mouseMovePoint = e.GetPosition(gridView);
            gridView_isMRBDown = false;
            if (startDrag == true)
            {
                Point newPoint = e.GetPosition((Grid)Parent);
                gridView.Margin = new Thickness(startDrag_margin.Left + (newPoint.X - window_startDragPoint.X),
                    startDrag_margin.Top + (newPoint.Y - window_startDragPoint.Y),
                    0, 0);

                gridLines.Margin = new Thickness(startDrag_margin_GLines.Left + (newPoint.X - window_startDragPoint.X),
                    startDrag_margin_GLines.Top + (newPoint.Y - window_startDragPoint.Y),
                    0, 0);
                //GridLines_PosiTrimming();
            }
            if (KeyResponse_keyPressing == true)
            {
                KeyResponse();
            }
        }
        private void gridView_MouseLeave(object sender, MouseEventArgs e)
        {
            gridView_isMRBDown = false;
            startDrag = false;
        }

        public BlockMatrix blockMatrix;
        private void gridView_iClick(Point clickPoint, bool isLMB)
        {
            int colIdx;
            int rowIdx;
            gridView_getClickedItemPosi(out rowIdx, out colIdx);

            BlockMatrix.OneBlockData item = blockMatrix.GetBlock(colIdx, rowIdx);
            if (item == null)
            {
                if (isLMB == false) return;
                // create and reg
                CreateOrChangeBlock(0);

                //ui.Call_iDetect();
                //blockMatrix.DetectOver(rowIdx, colIdx);
            }
            else
            {
                if (isLMB == true) item.uiControl.Set_SUp();
                else item.uiControl.Set_SDown();

                //item.uiControl.Call_iDetect();
                //blockMatrix.DetectOver(rowIdx, colIdx);
            }
        }
        private void gridView_getClickedItemPosi(out int rowIdx, out int colIdx)
        {
            double px = gridView_mouseMovePoint.X + gridView_negativeExtend.X;
            double py = gridView_mouseMovePoint.Y + gridView_negativeExtend.Y;
            colIdx = (int)(px / OneBlock.LEN_SIDE);
            rowIdx = (int)(py / OneBlock.LEN_SIDE);
            if (px < 0) colIdx -= 1;
            if (py < 0) rowIdx -= 1;
        }
        private void DetectNMark()
        {
            int colIdx;
            int rowIdx;
            gridView_getClickedItemPosi(out rowIdx, out colIdx);
            blockMatrix.DetectNMark(new System.Drawing.Point(colIdx, rowIdx));
        }
        private void CreateOrChangeBlock(int level)
        {
            int colIdx;
            int rowIdx;
            gridView_getClickedItemPosi(out rowIdx, out colIdx);
            CreateOrChangeBlock(colIdx, rowIdx, level);
        }
        public void CreateOrChangeBlock(int x, int y, int level)
        {
            // exist
            BlockMatrix.OneBlockData item = blockMatrix.GetBlock(x, y);
            BlockMatrix.OneBlockData.Status newStatus = (BlockMatrix.OneBlockData.Status)level;
            if (item != null && item.uiControl != null)
            {
                if (item.Statu != newStatus)
                {
                    item.SetStatus(newStatus);
                    if (EventOccurred != null)
                    {
                        EventOccurredArgs args = new EventOccurredArgs();
                        args.EALevel = EventOccurredArgs.Level.Info;
                        args.msg = "Change Signal!";
                        EventOccurred(this, args);
                    }
                }
                return;
            }

            // create and reg
            OneBlock ui = new OneBlock();
            ui.HorizontalAlignment = HorizontalAlignment.Left;
            ui.VerticalAlignment = VerticalAlignment.Top;
            ui.Margin = new Thickness(
                x * OneBlock.LEN_SIDE - gridView_negativeExtend.X,
                y * OneBlock.LEN_SIDE - gridView_negativeExtend.Y,
                0,
                0);
            ui.SetDetectStatus(newStatus);
            BlockMatrix.OneBlockData blockData = new BlockMatrix.OneBlockData(x, y, ui);
            blockData.SetStatus(newStatus, false);
            ui.blockData = blockData;
            blockMatrix.AddItem(blockData);
            gridView.Children.Add(ui);
            if (EventOccurred != null)
            {
                EventOccurredArgs args = new EventOccurredArgs();
                args.EALevel = EventOccurredArgs.Level.Info;
                args.msg = "Create Block!";
                EventOccurred(this, args);
            }
        }
        private void DisposeBlock()
        {
            int colIdx;
            int rowIdx;
            gridView_getClickedItemPosi(out rowIdx, out colIdx);
            DisposeBlock(colIdx, rowIdx);
        }
        public void DisposeBlock(int x, int y)
        {
            // exist
            BlockMatrix.OneBlockData item = blockMatrix.GetBlock(x, y);
            if (item != null)
            {
                item.uiControl.Call_iDispose();
                if (EventOccurred != null)
                {
                    EventOccurredArgs args = new EventOccurredArgs();
                    args.EALevel = EventOccurredArgs.Level.Info;
                    args.msg = "Remove Block!";
                    EventOccurred(this, args);
                }
            }
        }
        public void DisposeAllBlocks()
        {
            bool cleared = blockMatrix.allBlocksList.Count > 0;
            for (int i = blockMatrix.allBlocksList.Count - 1; i >= 0; --i)
            {
                blockMatrix.allBlocksList[i].uiControl?.Call_iDispose();
            }
            if (cleared)
            {
                if (EventOccurred != null)
                {
                    EventOccurredArgs args = new EventOccurredArgs();
                    args.EALevel = EventOccurredArgs.Level.Info;
                    args.msg = "Clear all Blocks!";
                    EventOccurred(this, args);
                }
            }
        }

        #endregion

        #region 滚轮操作
        private void gridView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            gridViewAndLines_Scale(e.Delta / 1200.0, e.GetPosition(gridView));
        }
        private void gridViewAndLines_Scale(double increment, Point gridViewMousePoint)
        {
            Matrix rt = gridLines.RenderTransform.Value;
            double newScale = increment + rt.M11;
            if (newScale <= 1) return;

            gridLines.RenderTransform
                = gridView.RenderTransform
                = new ScaleTransform(newScale, newScale);

            Thickness gvMarg = gridView.Margin;
            //Point mouseScreenPoint = e.GetPosition((Grid)Parent);
            double dVal = newScale - rt.M11;
            Point backVec = new Point(
                gridViewMousePoint.X * dVal,
                gridViewMousePoint.Y * dVal
                );

            gridView.Margin = new Thickness(
                gvMarg.Left - backVec.X,
                gvMarg.Top - backVec.Y,
                0,
                0);

            gridView_Extend();
            GridLines_Plant();
            GridLines_PosiTrimming();
        }
        #endregion

        #region 右键操作
        private bool gridView_isMRBDown = false;
        private void gridView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (startDrag == true)
            {
                DetectNMark();
                startDrag = false;
                gridView_isMRBDown = false;
                return;
            }
            gridView_isMRBDown = true;
            gridView_mouseMovePoint
                = gridView_clickPoint
                = e.GetPosition(gridView);
        }

        private void gridView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (gridView_isMRBDown == true)
            {
                gridView_iClick(gridView_clickPoint, false);
            }
            gridView_isMRBDown = false;
        }

        void blockMatrix_Call_GridViewRemoveChild(object sender, EventArgs e)
        {
            gridView.Children.Remove((OneBlock)sender);
        }

        #endregion

        #region 键盘响应
        bool KeyResponse_keyPressing = false;
        public void SetKeyDown(Key pressedKey)
        {
            this.pressedKey = pressedKey;
            KeyResponse_keyPressing = true;
        }
        public void SetKeyUp()
        {
            KeyResponse();
            KeyResponse_keyPressing = false;
        }

        Key pressedKey;
        private void KeyResponse()
        {
            switch (pressedKey)
            {
                case Key.Space:
                    DetectNMark();
                    //KeyResponse_keyPressing = false;
                    break;
                case Key.Delete:
                case Key.Back:
                    DisposeBlock();
                    //KeyResponse_keyPressing = false;
                    break;
                case Key.D0:
                case Key.NumPad0:
                    CreateOrChangeBlock(0);
                    break;
                case Key.D1:
                case Key.NumPad1:
                    CreateOrChangeBlock(1);
                    break;
                case Key.D2:
                case Key.NumPad2:
                    CreateOrChangeBlock(2);
                    break;
                case Key.D3:
                case Key.NumPad3:
                    CreateOrChangeBlock(3);
                    break;
                case Key.D4:
                case Key.NumPad4:
                    CreateOrChangeBlock(4);
                    break;
                case Key.D5:
                case Key.NumPad5:
                    CreateOrChangeBlock(5);
                    break;
                case Key.D6:
                case Key.NumPad6:
                    CreateOrChangeBlock(6);
                    break;
                case Key.D7:
                case Key.NumPad7:
                    CreateOrChangeBlock(7);
                    break;
                case Key.D8:
                case Key.NumPad8:
                    CreateOrChangeBlock(8);
                    break;
                case Key.Multiply:
                    CreateOrChangeBlock(10);
                    break;
            }
        }
        #endregion

    }
}