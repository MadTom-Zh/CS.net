using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace MadTomDev.App.Ctrls
{
    /// <summary>
    /// Interaction logic for Board.xaml
    /// </summary>
    public partial class Board : UserControl
    {
        public Board()
        {
            InitializeComponent();
        }


        public Cell? this[int r, int c]
        {
            get
            {
                int w = spRows.Children.Count;
                if (w <= 0 || r < 0 || w <= r || c < 0 || w <= c)
                {
                    return null;
                }
                return (Cell)(((StackPanel)spRows.Children[r]).Children[c]);
            }
        }


        #region set board size, data
        public void SetSize(int width)
        {
            if (width < 0)
            {
                width = 0;
            }
            int oriRowCount = spRows.Children.Count;
            if (width == oriRowCount)
            {
                return;
            }

            int i;
            if (width < oriRowCount)
            {
                // decrease rows
                bool willCheckedCellBeRemoved = false;
                int rmCellCol = -1, rmCellRow = -1;
                if (checkedCell is not null
                    && checkedCell.Parent is StackPanel)
                {
                    StackPanel rmCellSP = (StackPanel)checkedCell.Parent;
                    rmCellCol = rmCellSP.Children.IndexOf(checkedCell);
                    rmCellRow = spRows.Children.IndexOf(rmCellSP);
                    willCheckedCellBeRemoved = width <= rmCellRow;
                }
                for (i = oriRowCount - 1; width <= i; --i)
                {
                    spRows.Children.RemoveAt(i);
                }
                oriRowCount = width;
                if (willCheckedCellBeRemoved)
                {
                    _ChangeSelectedCell(null, rmCellCol, rmCellRow);
                }
            }

            // reset existing rows' width
            StackPanel row;
            for (i = 0; i < oriRowCount; ++i)
            {
                row = (StackPanel)spRows.Children[i];
                _SetRowWidth(ref row, width);
            }

            if (oriRowCount < width)
            {
                // increase rows
                for (i = oriRowCount; i < width; ++i)
                {
                    _AddRow(width);
                }
            }
        }

        private void _SetRowWidth(ref StackPanel row, int newWidth)
        {
            int cellCount = row.Children.Count;
            if (cellCount == newWidth)
            {
                return;
            }

            int i;
            if (cellCount < newWidth)
            {
                // add
                for (i = cellCount; i < newWidth; ++i)
                {
                    _AddCell(ref row);
                }
            }
            else
            {
                // remove
                bool isCheckedCellInRow = false;
                int cellCol = row.Children.IndexOf(checkedCell);
                if (checkedCell is not null
                    && 0 <= cellCol)
                {
                    isCheckedCellInRow = true;
                }
                for (i = cellCount - 1; newWidth <= i; --i)
                {
                    row.Children.RemoveAt(i);
                }
                if (isCheckedCellInRow
                    && row.Children.Contains(checkedCell) == false)
                {
                    _ChangeSelectedCell(null, cellCol, spRows.Children.IndexOf(row));
                }
            }
        }
        private void _AddCell(ref StackPanel row)
        {
            row.Children.Add(new Cell()
            {
                Text = "",
            });
        }
        private void _AddRow(int width)
        {
            StackPanel newRow = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
            };
            for (int i = 0; i < width; ++i)
            {
                _AddCell(ref newRow);
            }
            spRows.Children.Add(newRow);
        }

        public void SetData(int[][]? bData, string numList)
        {
            if (bData is null || bData.Length <= 0)
            {
                SetSize(0);
                return;
            }
            int r, c, w = bData.Length;
            SetSize(w);

            int[] vr;
            int vc;
            for (r = 0; r < w; ++r)
            {
                vr = bData[r];
                for (c = 0; c < w; ++c)
                {
                    vc = vr[c];
                    ((Cell)((StackPanel)spRows.Children[r]).Children[c]).Text
                        = vc < 0 ? "" : numList[vc].ToString();
                }
            }
        }
        public void ClearAllCellLocks(bool locked = false)
        {
            int r, c, w = spRows.Children.Count;
            StackPanel spRow;
            for (r = 0; r < w; ++r)
            {
                spRow = (StackPanel)spRows.Children[r];
                for (c = 0; c < w; ++c)
                {
                    ((Cell)spRow.Children[c]).IsLocked = locked;
                }
            }
        }
        public void SetCellLock(int r, int c, bool toLock = true)
        {
            ((Cell)((StackPanel)spRows.Children[r]).Children[c]).IsLocked = toLock;
        }

        internal void SetCellLocks(int[][] lockBoard)
        {
            int r, c, w = lockBoard.Length;
            for (r = 0; r < w; ++r)
            {
                for (c = 0; c < w; ++c)
                {
                    if (0 <= lockBoard[r][c])
                    {
                        SetCellLock(r, c);
                    }
                }
            }
        }
        public void SetErrorFlags(int[][]? errFlags)
        {
            int r, c, w;
            StackPanel spRow;
            if (errFlags is null)
            {
                w = spRows.Children.Count;
                for (r = 0; r < w; ++r)
                {
                    spRow = (StackPanel)spRows.Children[r];
                    for (c = 0; c < w; ++c)
                    {
                        ((Cell)spRow.Children[c]).IsErrorFlaged = false;
                    }
                }
            }
            else
            {
                w = Math.Min(errFlags.Length, spRows.Children.Count);
                for (r = 0; r < w; ++r)
                {
                    spRow = (StackPanel)spRows.Children[r];
                    for (c = 0; c < w; ++c)
                    {
                        ((Cell)spRow.Children[c]).IsErrorFlaged = 0 < errFlags[r][c];
                    }
                }
            }
        }

        #endregion




        #region mouse event, click to check

        private bool _CanCellChecked = true;
        public bool CanCellChecked
        {
            get => _CanCellChecked;
            set
            {
                if (value == false
                    && checkedCell is not null)
                {
                    checkedCell.IsChecked = false;
                    checkedCell = null;
                }
                _CanCellChecked = value;
            }
        }
        public Cell? checkedCell = null;
        public int checkedCellAtColPre = -1, checkedCellAtRowPre = -1;
        public int checkedCellAtCol = -1, checkedCellAtRow = -1;
        public Action CellCheckChanged;

        private void _ChangeSelectedCell(Cell? cell, int col, int row)
        {
            if (checkedCell is not null)
            {
                if (checkedCell != cell)
                {
                    checkedCell.IsChecked = false;
                }
            }
            if (cell is not null)
            {
                cell.IsChecked = true;
            }
            checkedCell = cell;
            checkedCellAtColPre = checkedCellAtCol;
            checkedCellAtRowPre = checkedCellAtRow;
            checkedCellAtCol = col;
            checkedCellAtRow = row;
            CellCheckChanged?.Invoke();
        }
        private void UserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_CanCellChecked)
            {
                if (_GetUIposiFromMousePoint(out int col, out int row) == false)
                {
                    return;
                }

                Cell mdCell = (Cell)((StackPanel)spRows.Children[row]).Children[col];
                if (checkedCell == mdCell)
                {
                    // deselecte
                    _ChangeSelectedCell(null, col, row);
                }
                else
                {
                    // select
                    _ChangeSelectedCell(mdCell, col, row);
                }
            }
        }
        private void UserControl_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            tbKeyReceiver.Focus();
        }
        private bool _GetUIposiFromMousePoint(out int col, out int row)
        {
            Point mPt = Mouse.GetPosition(this);
            return TryGetCellPosition(mPt, out col, out row);
        }
        public bool TryGetCellPosition(Point pt, out int col, out int row)
        {
            if (spRows.Children.Count <= 0)
            {
                col = -1;
                row = -1;
                return false;
            }
            if (pt.X < 0 || pt.Y < 0
                || this.ActualWidth <= pt.X || this.ActualHeight <= pt.Y)
            {
                col = -1;
                row = -1;
                return false;
            }
            Cell testCell = (Cell)((StackPanel)spRows.Children[0]).Children[0];
            col = (int)(pt.X / testCell.ActualWidth);
            row = (int)(pt.Y / testCell.ActualHeight);
            return true;
        }

        private Cell? _preHighlightedCell = null;
        private int _preHighlightedCell_preCol = -1, _preHighlightedCell_preRow = -1;


        public char PressedKeyChar = '\0';
        private void UserControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_GetUIposiFromMousePoint(out int col, out int row))
            {
                if (col == _preHighlightedCell_preCol
                    && row == _preHighlightedCell_preRow)
                {
                    return;
                }
                if (_preHighlightedCell is not null)
                {
                    _preHighlightedCell.IsHighlighted = false;
                }
                _preHighlightedCell = (Cell)((StackPanel)spRows.Children[row]).Children[col];
                _preHighlightedCell.IsHighlighted = true;

                _preHighlightedCell_preCol = col;
                _preHighlightedCell_preRow = row;
            }
            else
            {
                if (_preHighlightedCell is not null)
                {
                    _preHighlightedCell.IsHighlighted = false;
                    _preHighlightedCell = null;
                    _preHighlightedCell_preCol = -1;
                    _preHighlightedCell_preRow = -1;
                }
            }
        }

        private void tbKeyReceiver_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbKeyReceiver.Text.Length < 1)
            {
                return;
            }
            PressedKeyChar = tbKeyReceiver.Text[0];
            tbKeyReceiver.Text = "";

            if (checkedCell is not null)
            {
                KeyDownOnCheckedCell?.Invoke(PressedKey);
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_preHighlightedCell is not null)
            {
                _preHighlightedCell.IsHighlighted = false;
                _preHighlightedCell = null;
                _preHighlightedCell_preCol = -1;
                _preHighlightedCell_preRow = -1;
            }
        }


        #endregion

        #region keydown on cell

        public Action<Key> KeyDownOnCheckedCell;
        public Key PressedKey;
        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            PressedKey = e.Key;
            if (checkedCell is not null
                && (PressedKey == Key.Delete || PressedKey == Key.Back))
            {
                KeyDownOnCheckedCell?.Invoke(PressedKey);
            }
        }



        #endregion
    }
}
