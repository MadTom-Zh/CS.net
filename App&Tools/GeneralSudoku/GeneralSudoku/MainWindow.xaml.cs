using MadTomDev.App.Ctrls;
using MadTomDev.UI;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Point = System.Windows.Point;

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
        Core core;
        bool isIniting = true;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            core = Core.Instance;

            // tab init
            boardInit.SetData(null, "");
            core.NumberCharListChanged += AfterCharListChanged;
            core.InitBoardDataChanged += AfterInitBoardDataChanged;
            core.InitBoardTemplateChanged += AfterInitBoardTemplateChanged;

            boardInit.CellCheckChanged = InitBoardCheckedCellChanged;

            // tab play
            core.PlayBoardErrorFound = AfterPlayBoardErrorFound;
            core.PlayBoardNoErrorFound = AfterPlayBoardNoErrorFound;
            boardPlay.CellCheckChanged = PlayCellCheckChanged;
            boardPlay.KeyDownOnCheckedCell = PlayBoardSetCellByKey;

            // tab solv
            boardSolv.SetData(null, "");
            core.SolvBoardErrorFound = AfterSolvBoardErrorFound;
            core.SolvingCanceled = AfterSolvingCanceled;
            core.SolvingComplete = AfterSolvingComplete;
            core.SolvingSolutionFound = AfterSolvingSolutionFound;
            lv_solResults.ItemsSource = solvResultItemSource;
            isIniting = false;

            tb_initNumbers_TextChanged(tb_initNumbers, null);
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (tabMain.TabIndex == 0)
            {
                if (boardInit.IsFocused)
                {
                    ;
                }
            }
        }
        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (tabMain.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    CheckSetPlayBoard();
                    break;
                case 2:
                    CheckSetSolveBoard();
                    break;
            }
        }



        #region tab init

        #region set chars, click select, keyboard set char
        private void tb_initNumbers_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isIniting) return;

            core.NumberCharList = tb_initNumbers.Text;
            int numCount = core.NumberCharList.Length;
            tbv_initNumCount.Text = $"Board size {numCount} * {numCount} .";
        }
        private void AfterCharListChanged(Core sender)
        {
        }
        private void AfterInitBoardDataChanged(Core sender)
        {
            // to do...
        }
        private void AfterInitBoardTemplateChanged(Core sender)
        {
            ReloadInitBoard();
            int numCount = core.NumberCharList.Length;
            tbv_initCellCount.Text
                = $"Blanks {Core.GetEmptyPositions(core.initBoardTemplate).Count}/{numCount * numCount} .";
        }

        private void ReloadInitBoard()
        {
            boardInit.SetData(core.initBoardTemplate, core.NumberCharList);
        }

        private bool _InitBoardCheckedCellChanged_selfChange = false;
        private void InitBoardCheckedCellChanged()
        {
            _InitBoardCheckedCellChanged_selfChange = true;

            _InitBoardCheckedCellChanged_selfChange = false;
        }
        private void tb_initCheckedCellTx_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        #endregion


        #region quick set buttons, confirm, reset
        private void btn_initClear_Click(object sender, RoutedEventArgs e)
        {
            core.ClearInitBoardTemplate();
        }
        private void btn_initReGen_Click(object sender, RoutedEventArgs e)
        {
            core.ReInitBoards();
        }

        private void btn_initAddOne_Click(object sender, RoutedEventArgs e)
        {
            core.AddRandomWithinInitBoardTemplate();
        }

        private void btn_initRemoveOne_Click(object sender, RoutedEventArgs e)
        {
            core.RemoveRandomWithinInitBoardTemplate();
        }

        private void btn_initAddFive_Click(object sender, RoutedEventArgs e)
        {
            core.AddRandomWithinInitBoardTemplate(5);
        }

        private void btn_initRemoveFive_Click(object sender, RoutedEventArgs e)
        {
            core.RemoveRandomWithinInitBoardTemplate(5);
        }


        private void btn_initViewSudokuNor_Click(object sender, RoutedEventArgs e)
        {
            Wnds.WindowSudokuViewer viewer = new Wnds.WindowSudokuViewer()
            {
                Title = "Normalized Init Sudoku",
                boardName = nameof(core.initBoardDataNormal),
            };
            viewer.Show();
        }
        private void btn_initViewSudokuRand_Click(object sender, RoutedEventArgs e)
        {
            Wnds.WindowSudokuViewer viewer = new Wnds.WindowSudokuViewer()
            {
                Title = "Randomlized Init Sudoku",
                boardName = nameof(core.initBoardDataRand),
            };
            viewer.Show();
        }

        private void btn_initConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (core.initBoardTemplate is null
                || core.initBoardTemplate.Length <= 0)
            {
                MessageBox.Show(this, "No init board template.");
                return;
            }
            int w = core.initBoardTemplate.Length;
            int presents = Core.GetPresentPositions(core.initBoardTemplate).Count;
            if (presents <= 0)
            {
                MessageBox.Show(this, "Init board template is empty.");
                return;
            }
            if (presents < (w * w / 2))
            {
                if (MessageBox.Show(this, $"Too litte blanks,{Environment.NewLine}Continue?", "Warning",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No)
                    == MessageBoxResult.No)
                {
                    return;
                }
            }

            tb_initNumbers.IsEnabled = false;

            btn_initClear.IsEnabled = false;
            btn_initReGen.IsEnabled = false;
            btn_initAddOne.IsEnabled = false;
            btn_initAddFive.IsEnabled = false;
            btn_initRemoveOne.IsEnabled = false;
            btn_initRemoveFive.IsEnabled = false;

            btn_initReset.IsEnabled = true;
            btn_initConfirm.IsEnabled = false;

            tabMain.SelectedIndex = 1;
        }

        private void btn_initReset_Click(object sender, RoutedEventArgs e)
        {
            core.ReInitBoards();

            tb_initNumbers.IsEnabled = true;

            btn_initClear.IsEnabled = true;
            btn_initReGen.IsEnabled = true;
            btn_initAddOne.IsEnabled = true;
            btn_initAddFive.IsEnabled = true;
            btn_initRemoveOne.IsEnabled = true;
            btn_initRemoveFive.IsEnabled = true;

            btn_initReset.IsEnabled = false;
            btn_initConfirm.IsEnabled = true;
        }




        #endregion

        #endregion



        #region tab play


        #region board init, set cell, key press
        private int[][]? playBoardTemplatePre = null;
        private void CheckSetPlayBoard()
        {
            if (btn_initConfirm.IsEnabled == true
                || core.initBoardTemplate is null)
            {
                // not ready
                PlayTimerStop();
                tb_playSelectedCell.IsEnabled = false;
                tb_playSelectedCell.Text = "";
                PlayClearNumberBtns();
                tbv_playStatues.Text = "Init data not ready.";
                boardPlay.IsEnabled = false;
                boardPlay.SetData(null, "");
                return;
            }
            else if (playBoardTemplatePre is null)
            {
                // restart
                PlayReStart();
                return;
            }
            else if (playBoardTemplatePre != core.initBoardTemplate)
            {
                // date changed, restart
                PlayReStart();
                return;
            }
            else
            {
                // do nothing
            }
        }
        private void PlayReStart()
        {
            if (core.initBoardTemplate is null)
            {
                throw new Exception("Play template is null.");
            }
            playBoardTemplatePre = core.initBoardTemplate;

            core.PlayInit();

            tb_playSelectedCell.IsEnabled = true;
            PlayReFillNumberBtns();
            boardPlay.IsEnabled = true;
            boardPlay.AllowDrop = true;
            boardPlay.ClearAllCellLocks();
            boardPlay.SetData(core.playBoard, core.NumberCharList);

            // set cell lock
            boardPlay.ClearAllCellLocks();
            boardPlay.SetCellLocks(core.initBoardTemplate);
            PlayTimerStart();
        }

        private void PlayCellCheckChanged()
        {
            tb_playSelectedCell_isSelfChange = true;
            if (boardPlay.checkedCell is null)
            {
                tb_playSelectedCell.Text = "";
            }
            else
            {
                tb_playSelectedCell.Text = boardPlay.checkedCell.Text;
            }
            tb_playSelectedCell_isSelfChange = false;
        }
        private bool tb_playSelectedCell_isSelfChange = false;
        private void tb_playSelectedCell_TextChanged(object sender, TextChangedEventArgs e)
        {
            string tx = tb_playSelectedCell.Text.Trim();
            if (tb_playSelectedCell_isSelfChange
                || tx.Length != 1
                || core.NumberCharList.Contains(tx) == false
                || boardPlay.checkedCell is null
                || boardPlay.checkedCell.IsLocked)
            {
                return;
            }

            tb_playSelectedCell_isSelfChange = true;
            int r = boardPlay.checkedCellAtRow, c = boardPlay.checkedCellAtCol;
            try
            {
                core.PlaySetCell(r, c, tx);
                boardPlay.checkedCell.Text = tx;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
            tb_playSelectedCell.Text = core.NumberCharList[core.playBoard[r][c]].ToString();
            tb_playSelectedCell_isSelfChange = false;
        }

        private void PlayBoardSetCellByKey(Key k)
        {
            if (boardPlay.checkedCell is null
                || boardPlay.checkedCell.IsLocked)
            {
                return;
            }
            int r = boardPlay.checkedCellAtRow, c = boardPlay.checkedCellAtCol;
            string chr = boardPlay.PressedKeyChar.ToString();
            if (k == Key.Delete || k == Key.Back)
            {
                core.PlaySetCell(r, c, null);
                boardPlay.checkedCell.Text = "";
            }
            else if (core.NumberCharList.Contains(chr))
            {
                core.PlaySetCell(r, c, chr);
                boardPlay.checkedCell.Text = chr;
            }
        }

        private void AfterPlayBoardErrorFound(Core sender)
        {
            boardPlay.SetErrorFlags(core.playBoard_errorFlags);
        }
        private void AfterPlayBoardNoErrorFound(Core sender)
        {
            boardPlay.SetErrorFlags(null);
        }

        #endregion


        #region number buttons, click, drag-drop
        private void PlayReFillNumberBtns()
        {
            wp_playCharBtns.Children.Clear();
            foreach (Char c in core.NumberCharList)
            {
                wp_playCharBtns.Children.Add(new Cell()
                {
                    Text = c.ToString(),
                });
            }
        }
        private void PlayClearNumberBtns()
        {
            wp_playCharBtns.Children.Clear();
        }

        private Cell? wp_playCharBtns_pressedBtn = null;
        private bool wp_playCharBtns_isPressing = false;
        private Point wp_playCharBtns_pressingPoint;
        private void wp_playCharBtns_MouseEnter(object sender, MouseEventArgs e)
        {
            wp_playCharBtns.Focus();
        }
        private void wp_playCharBtns_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            boardInit_DragInBtnCursorStop = null;
            wp_playCharBtns_pressingPoint = Mouse.GetPosition(wp_playCharBtns);
            wp_playCharBtns_pressedBtn = (Cell)UI.VisualHelper.WrapPanel.GetItemUI(
                wp_playCharBtns, typeof(Cell), wp_playCharBtns_pressingPoint);
            if (wp_playCharBtns_pressedBtn is not null)
            {
                wp_playCharBtns_isPressing = true;
            }
        }
        private void wp_playCharBtns_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            wp_playCharBtns_isPressing = false;

            if (wp_playCharBtns_pressedBtn is null)
            {
                return;
            }
            System.Windows.Point curMP = Mouse.GetPosition(wp_playCharBtns);
            if (Math.Abs(curMP.X - wp_playCharBtns_pressingPoint.X) <= 3
                && Math.Abs(curMP.Y - wp_playCharBtns_pressingPoint.Y) <= 3)
            {
                // as clicked
                string chr = wp_playCharBtns_pressedBtn.Text;
                if (boardPlay.checkedCell is not null
                    && boardPlay.checkedCell.IsLocked == false)
                {
                    core.PlaySetCell(boardPlay.checkedCellAtRow, boardPlay.checkedCellAtCol, chr);
                    boardPlay.checkedCell.Text = chr;
                }
            }
        }
        private void wp_playCharBtns_MouseLeave(object sender, MouseEventArgs e)
        { wp_playCharBtns_isPressing = false; }

        private void wp_playCharBtns_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (wp_playCharBtns_isPressing is false)
            {
                return;
            }
            Point curMP = Mouse.GetPosition(wp_playCharBtns_pressedBtn);
            if (3 < Math.Abs(curMP.X - wp_playCharBtns_pressingPoint.X)
                || 3 < Math.Abs(curMP.Y - wp_playCharBtns_pressingPoint.Y))
            {
                DragDrop.DoDragDrop(wp_playCharBtns_pressedBtn, wp_playCharBtns_pressedBtn.Text, DragDropEffects.Link);
                wp_playCharBtns_isPressing = false;
            }
        }
        private Cursor? boardInit_DragInBtnCursorStop = null;
        private Cursor? boardInit_DragInBtnCursorPass = null;
        private void wp_playCharBtns_PreviewGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
        }
        private void wp_playCharBtns_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (wp_playCharBtns_pressedBtn is null)
            {
                e.UseDefaultCursors = true;
                e.Handled = true;
                return;
            }
            e.UseDefaultCursors = false;
            if (boardInit_DragInBtnCursorStop is null)
            {
                BitmapImage img = wp_playCharBtns_pressedBtn.ToBitmapImage();
                BitmapImage imgTrans = QuickGraphics.Image.GetTransparentImage(img, 0.5f).ToBitmapImage();
                int crsX = (int)(imgTrans.Width / 2);
                int crsY = (int)(imgTrans.Height / 2);
                boardInit_DragInBtnCursorStop = imgTrans.CreateCursor(crsX, crsY);
                boardInit_DragInBtnCursorPass = img.CreateCursor(crsX, crsY);
            }
            if (e.Effects.HasFlag(DragDropEffects.Link))
            {
                Mouse.SetCursor(boardInit_DragInBtnCursorPass);
            }
            else
            {
                Mouse.SetCursor(boardInit_DragInBtnCursorStop);
            }
            e.Handled = true;
        }
        private void boardPlay_PreviewDrop(object sender, DragEventArgs e)
        {
            boardInit_DragInBtnCursorStop = null;
            string chr = (string)e.Data.GetData(typeof(string));
            if (string.IsNullOrEmpty(chr)
                || chr.Length != 1)
            {
                return;
            }
            Point curMP = e.GetPosition(boardPlay);
            if (boardPlay.TryGetCellPosition(curMP, out int c, out int r) == false)
            {
                return;
            }
            Cell? cellAtMP = boardPlay[r, c];
            if (cellAtMP is null
                || cellAtMP.IsLocked)
            {
                return;
            }

            core.PlaySetCell(r, c, chr);
            cellAtMP.Text = chr;
        }

        #endregion


        private DispatcherTimer? playTimer = null;
        private DateTime playTimerStartTime;
        private string playTimerStartTimeStr;
        private int playTimerSecPre;
        private void PlayTimerStart()
        {
            playTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(50),
                DispatcherPriority.Background,
                new EventHandler(PlayTimerTick),
                Dispatcher);
            playTimerStartTime = DateTime.Now;
            playTimerStartTimeStr = playTimerStartTime.ToString("yyyy-MM-dd HH:mm:ss");
            playTimer.Start();
        }
        private void PlayTimerStop()
        {
            playTimer?.Stop();
            playTimer = null;
        }
        private void PlayTimerTick(object? o, EventArgs e)
        {
            DateTime now = DateTime.Now;
            TimeSpan timePassed = now - playTimerStartTime;
            if (timePassed.Seconds == playTimerSecPre)
            {
                return;
            }
            playTimerSecPre = timePassed.Seconds;

            Dispatcher.Invoke(() =>
            {
                StringBuilder strBdr = new StringBuilder();
                strBdr.AppendLine("Err / Blk / Totl 剩余空格");
                int countErr = 0;
                if (core.playBoard_errorFlags is not null)
                {
                    countErr = Core.GetPresentPositions(core.playBoard_errorFlags).Count;
                }
                int countBlk = Core.GetEmptyPositions(core.playBoard).Count;
                int w = core.playBoard.Length;
                int countTotl = w * w;
                strBdr.AppendLine($"{countErr} / {countBlk} / {countTotl}");
                strBdr.AppendLine("Start At 开始时间");
                strBdr.AppendLine(playTimerStartTimeStr);
                strBdr.AppendLine("Time used 用时");
                strBdr.Append(timePassed.ToString(@"dd\.\ hh\:mm\:ss"));

                tbv_playStatues.Text = strBdr.ToString();

                if (countBlk == countTotl && countErr == 0)
                {
                    PlayTimerStop();
                    tb_playSelectedCell.IsEnabled = false;
                    boardPlay.IsEnabled = false;
                    boardPlay.AllowDrop = false;
                    MessageBox.Show("Congratulations!! You complete the Sudoku!!!", "Complete",
                        MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
            });
        }






        #endregion



        #region tab solve

        private void CheckSetSolveBoard()
        {
            if (btn_initConfirm.IsEnabled == true)
            {
                // not ready
                btn_solStart.IsEnabled = false;
            }
            else if (solTimer is not null)
            {
                // already solving
                btn_solStart.IsEnabled = false;
            }
            else
            {
                btn_solStart.IsEnabled = true;
            }
        }

        #region top buttons
        private void tbtn_solTemplate_Checked(object sender, RoutedEventArgs e)
        {
            tbtn_solPlayBoard.IsChecked = false;
        }
        private void tbtn_solPlayBoard_Checked(object sender, RoutedEventArgs e)
        {
            tbtn_solTemplate.IsChecked = false;
        }
        private void tbtn_solSerial_Checked(object sender, RoutedEventArgs e)
        {
            tbtn_solParallel.IsChecked = false;
        }
        private void tbtn_solParallel_Checked(object sender, RoutedEventArgs e)
        {
            tbtn_solSerial.IsChecked = false;
        }

        private void btn_solStart_Click(object sender, RoutedEventArgs e)
        {
            SolStart();
        }
        private void btn_solCancel_Click(object sender, RoutedEventArgs e)
        {
            SolCancel();
        }
        #endregion


        private DateTime _solvStartAt;
        private DateTime _solvEndAt;
        private DispatcherTimer? solTimer;
        private void SolStart()
        {
            if (solTimer is not null)
            {
                return;
            }
            tabItemInit.IsEnabled = false;
            tabItemPlay.IsEnabled = false;

            if (tbtn_solTemplate.IsChecked == true)
            {
                if (core.initBoardTemplate is null)
                {
                    throw new Exception("Template is null");
                }
                core.SolvInit(core.initBoardTemplate);
            }
            else
            {
                core.SolvInit(core.playBoard);
            }

            solvResultItemSource.Clear();
            solvResultItemSource_num = 0;
            solvResultItemSource_maxOut = false;
            boardSolv.SetData(null, "");

            _solvStartAt = DateTime.Now;
            tbv_solStartAt.Text = _solvStartAt.ToString($"yyyy-MM-dd{Environment.NewLine}HH:mm:ss");
            tbv_solEndAt.Text = "";

            solTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(30),
                DispatcherPriority.Background,
                new EventHandler(SolTimerTick),
                Dispatcher);
            solTimer.Start();

            tbv_solState.Text = "Running";
            core.SolvStart();
        }
        private void SolCancel()
        {
            if (solTimer is null)
            {
                return;
            }
            core.SolvCancel();
        }
        private void SolDone(bool isCanceled)
        {
            Dispatcher.Invoke(() =>
            {
                solTimer?.Stop();
                solTimer = null;
                tabItemInit.IsEnabled = true;
                tabItemPlay.IsEnabled = true;
                if (isCanceled)
                {
                    tbv_solState.Text = "Canceled";
                }
                else
                {
                    tbv_solState.Text = "Complete";
                }
                _solvEndAt = DateTime.Now;
                tbv_solEndAt.Text = _solvEndAt.ToString($"yyyy-MM-dd{Environment.NewLine}HH:mm:ss");
                TimeSpan timeUsed = _solvEndAt - _solvStartAt;
                tbv_solTimeUsed.Text = timeUsed.ToString(@"dd\.\ hh\:mm\:ss\.fff");

                tbv_solSteps.Text = core.solvCountSteps.ToString();
                tbv_solSolutions.Text = core.solvCountSolutions.ToString();
            });
        }

        private bool _SolTimerTick_busy = false;
        private void SolTimerTick(object? o, EventArgs e)
        {
            if (_SolTimerTick_busy)
            {
                return;
            }
            Dispatcher.Invoke(() =>
            {
                _SolTimerTick_busy = true;
                tbv_solSteps.Text = core.solvCountSteps.ToString();
                tbv_solSolutions.Text = core.solvCountSolutions.ToString();
                TimeSpan timeUsed = DateTime.Now - _solvStartAt;
                tbv_solTimeUsed.Text = timeUsed.ToString(@"dd\.\ hh\:mm\:ss\.fff");

                _SolTimerTick_busy = false;
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void AfterSolvBoardErrorFound(Core sender, int[][] errList)
        {
            MessageBox.Show("Errors in board, no way to solve.");
        }
        private void AfterSolvingCanceled(Core sender)
        {
            SolDone(true);
        }
        private void AfterSolvingComplete(Core sender)
        {
            SolDone(false);
        }

        ObservableCollection<VMs.SolutionItem> solvResultItemSource = new ObservableCollection<VMs.SolutionItem>();
        private int solvResultItemSource_num = 0;
        private bool solvResultItemSource_maxOut = false;
        private void AfterSolvingSolutionFound(Core sender, int[][] solution)
        {
            if (solvResultItemSource_maxOut)
            {
                return;
            }
            Dispatcher.Invoke(() =>
            {
                solvResultItemSource.Add(new VMs.SolutionItem()
                {
                    Text = "# " + solvResultItemSource_num++,
                    solution = solution,
                });
                solvResultItemSource_maxOut = (20 <= solvResultItemSource.Count);
            });
        }


        private VMs.SolutionItem? lv_solResults_checkedItem;
        private void lv_solResults_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void lv_solResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lv_solResults.SelectedItems.Count != 1)
            {
                return;
            }
            VMs.SolutionItem curItem = (VMs.SolutionItem)lv_solResults.SelectedValue;
            if (lv_solResults_checkedItem == curItem)
            {
                return;
            }
            if (lv_solResults_checkedItem is not null)
            {
                lv_solResults_checkedItem.IsChecked = false;
            }

            // show solution to board
            boardSolv.ClearAllCellLocks();
            boardSolv.SetData(curItem.solution, core.NumberCharList);
            boardSolv.SetCellLocks(core.solvBoard_template);

            curItem.IsChecked = true;
            lv_solResults_checkedItem = curItem;
        }


        #endregion

    }
}