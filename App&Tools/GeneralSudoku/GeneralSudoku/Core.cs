using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MadTomDev.App
{
    internal class Core
    {
        private static Core? _Instance = null;
        private Core()
        {
            ReInitBoards();
        }
        public static Core Instance
        {
            get
            {
                if (_Instance is null)
                {
                    _Instance = new Core();
                }
                return _Instance;
            }
        }



        #region common

        public Action<Core> NumberCharListChanged;
        private string _NumberCharList = "0123456789";
        //public static string DefaultCharList // 10 + 26 + 26 + 10 + 10 = 82 enough ??
        //    = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ一二三四五六七八九十壹贰叁肆伍陆柒捌玖拾";
        public string NumberCharList
        {
            get => _NumberCharList;
            set
            {
                // remove blank spaces
                string tx = value.Replace(" ", "").Replace("　", "");

                // remove duplicats
                Char c;
                for (int i = 0, iv = tx.Length - 1; i < iv; ++i)
                {
                    c = tx[i];
                    for (int j = iv; i < j; --j)
                    {
                        if (c == tx[j])
                        {
                            tx.Remove(j);
                            iv--;
                        }
                    }
                }

                if (_NumberCharList == tx)
                {
                    return;
                }
                _NumberCharList = tx;
                NumberCharListChanged?.Invoke(this);

                ClearInitBoards();
            }
        }

        public static int[][] NewBoard(int width, int defaultValue = -1)
        {
            int[][] result = new int[width][];
            int[] row;
            int i, c;
            for (i = 0; i < width; ++i)
            {
                row = new int[width];
                for (c = 0; c < width; ++c)
                {
                    row[c] = defaultValue;
                }
                result[i] = row;
            }
            return result;
        }
        private static void ReSizeBoard(ref int[][] board, int width)
        {
            if (board.Length != width)
            {
                int[][] nb = NewBoard(width);
                CopyBoard(board, ref nb);
                board = nb;
            }
        }
        public static void CopyBoard(int[][]? sour, ref int[][] dest)
        {
            if (sour is null || sour.Length <= 0)
            {
                dest = new int[0][];
                return;
            }
            int i, c, iv = Math.Min(sour.Length, dest.Length);
            for (i = 0; i < iv; ++i)
            {
                for (c = 0; c < iv; ++c)
                {
                    dest[i][c] = sour[i][c];
                }
            }
        }



        public static bool CheckRow(ref int[][] board, int row, out Position[] samePosis)
        {
            bool result = true;
            List<Position> samePosiList = new List<Position>();
            int[] testingRow = board[row].ToArray();
            int w = board.Length;
            int curValue, curValue1;
            int c, cv = w - 1, c1;
            bool firstPosiNotAdded;
            for (c = 0; c < cv; ++c)
            {
                firstPosiNotAdded = true;
                curValue = testingRow[c];
                if (curValue < 0)
                {
                    continue;
                }

                for (c1 = c + 1; c1 < w; ++c1)
                {
                    curValue1 = testingRow[c1];
                    if (curValue1 < 0)
                    {
                        continue;
                    }
                    if (curValue == curValue1)
                    {
                        result = false;
                        if (firstPosiNotAdded)
                        {
                            samePosiList.Add(new Position()
                            { r = row, c = c, });
                            firstPosiNotAdded = false;
                        }
                        samePosiList.Add(new Position()
                        { r = row, c = c1, });
                        testingRow[c1] = -1;
                    }
                }
            }
            samePosis = samePosiList.ToArray();
            return result;
        }
        public static bool CheckCol(ref int[][] board, int col, out Position[] samePosis)
        {
            bool result = true;
            List<Position> samePosiList = new List<Position>();
            int w = board.Length;
            int[] testingCol = new int[w];
            int r, rv = w - 1, r1;
            for (r = 0; r < w; ++r)
            {
                testingCol[r] = board[r][col];
            }
            int curValue, curValue1;
            bool firstPosiNotAdded;
            for (r = 0; r < rv; ++r)
            {
                firstPosiNotAdded = true;
                curValue = testingCol[r];
                if (curValue < 0)
                {
                    continue;
                }

                for (r1 = r + 1; r1 < w; ++r1)
                {
                    curValue1 = testingCol[r1];
                    if (curValue1 < 0)
                    {
                        continue;
                    }
                    if (curValue == curValue1)
                    {
                        result = false;
                        if (firstPosiNotAdded)
                        {
                            samePosiList.Add(new Position()
                            { r = r, c = col, });
                            firstPosiNotAdded = false;
                        }
                        samePosiList.Add(new Position()
                        { r = r1, c = col, });
                        testingCol[r1] = -1;
                    }
                }
            }
            samePosis = samePosiList.ToArray();
            return result;
        }

        public static bool TestCell(ref int[][] board, int row, int col, int v)
        {
            int r, c, w = board.Length;
            int[] testRow = board[row];
            for (c = 0; c < w; ++c)
            {
                if (c == col)
                {
                    continue;
                }
                if (testRow[c] == v)
                {
                    return false;
                }
            }
            for (r = 0; r < w; ++r)
            {
                if (r == row)
                {
                    continue;
                }
                if (board[r][col] == v)
                {
                    return false;
                }
            }
            return true;
        }


        #region generate random sudoku

        private Random rand = new Random((int)DateTime.Now.Ticks);
        public static Task<int[][]> GenerateNormalizedSudoku(Random rand, int width)
        {
            return Task.Run(() =>
            {
                int[][] result = NewBoard(width);
                int r, c;
                for (r = 0; r < width; ++r)
                {
                    result[r][0] = r;
                    result[0][r] = r;
                }
                _genertatedNormalizedSudoku = null;
                Dictionary<Position, int> testTable = new Dictionary<Position, int>();
                _GenerateNormalizedSudoku_Loop(rand, 0, testTable, width, 1, 1);
                if (_genertatedNormalizedSudoku is null)
                {
                    throw new Exception("Node chain not generated.");
                }
                Position posi;
                foreach (KeyValuePair<Position, int> kv in _genertatedNormalizedSudoku)
                {
                    posi = kv.Key;
                    r = posi.r;
                    c = posi.c;
                    result[r][c] = kv.Value;
                    if (posi.r != posi.c)
                    {
                        result[c][r] = kv.Value;
                    }
                }
                return result;
            });
        }

        private static int?[] _GenerateNormalizedIntArray_cache = null;
        private static int _GenerateNormalizedIntArray_width = 0;
        private static int?[] _GenerateNormalizedIntArray(int width)
        {
            if (_GenerateNormalizedIntArray_cache is null
                || width != _GenerateNormalizedIntArray_width)
            {
                _GenerateNormalizedIntArray_width = width;
                _GenerateNormalizedIntArray_cache = new int?[width];
                for (int i = 0; i < width; ++i)
                {
                    _GenerateNormalizedIntArray_cache[i] = i;
                }
            }
            return _GenerateNormalizedIntArray_cache.ToArray();
        }
        private static List<int> _GenerateNormalizedSudoku_getAValuesList(int w, int r, int c, ref Dictionary<Position, int> boardTable)
        {
            int?[] testValues = _GenerateNormalizedIntArray(w);
            testValues[c] = null;
            testValues[r] = null;

            Position testPosi;
            int curR, curC;
            testPosi.c = c;
            int existValue;
            // current col
            for (curR = 0; curR < w; ++curR)
            {
                if (curR == r)
                {
                    continue;
                }
                testPosi.r = curR;
                if (boardTable.ContainsKey(testPosi))
                {
                    existValue = boardTable[testPosi];
                    testValues[existValue] = null;
                }
            }
            // current row
            testPosi.r = r;
            for (curC = 0; curC < w; ++curC)
            {
                if (curC == c)
                {
                    continue;
                }
                testPosi.c = curC;
                if (boardTable.ContainsKey(testPosi))
                {
                    existValue = boardTable[testPosi];
                    testValues[existValue] = null;
                }
            }
            // row from col-mirrlr
            if (r != c)
            {
                testPosi.c = r;
                for (curR = 1; curR < r; ++curR)
                {
                    testPosi.r = curR;
                    if (boardTable.ContainsKey(testPosi))
                    {
                        existValue = boardTable[testPosi];
                        testValues[existValue] = null;
                    }
                }
            }

            List<int> resultList = new List<int>();
            foreach (int? v in testValues)
            {
                if (v is null)
                {
                    continue;
                }
                resultList.Add(v.Value);
            }
            return resultList;
        }
        private static int _PopRandom(Random rand, ref List<int> intList)
        {
            if (intList.Count <= 0)
            {
                throw new Exception("No element to Pop.");
            }
            int idx = rand.Next(intList.Count);
            int result = intList[idx];
            intList.RemoveAt(idx);
            return result;
        }
        private static bool _GenerateNormalizedSudoku_GetNextPosi(int w, int r, int c, out Position nextPosi)
        {
            nextPosi = new Position()
            {
                r = r,
                c = c + 1,
            };
            if (w <= nextPosi.c)
            {
                nextPosi.r += 1;
                if (w <= nextPosi.r)
                {
                    return false;
                }
                nextPosi.c = nextPosi.r;
            }
            return true;
        }

        private static Dictionary<Position, int>? _genertatedNormalizedSudoku;
        private static void _GenerateNormalizedSudoku_Loop(Random rand, int level, Dictionary<Position, int> initTable, int w, int r, int c)
        {
            if (_genertatedNormalizedSudoku is not null)
            {
                return;
            }

            int nextLevel = level + 1;
            // get valiable values

            List<int> aValues = _GenerateNormalizedSudoku_getAValuesList(w, r, c, ref initTable);
            if (aValues is null || aValues.Count <= 0)
            {
                // no aValue
                return;
            }
            Position curPosi = new Position() { r = r, c = c, };

            if (r + 1 == w)
            {
                // end of sdu, 
                int a = _PopRandom(rand, ref aValues);
                initTable.Add(curPosi, a);
                _genertatedNormalizedSudoku = initTable;
                return;
            }


            // get next posi
            if (_GenerateNormalizedSudoku_GetNextPosi(w, r, c, out Position nextPosi) == false)
            {
                // end
                return;
            }

            // loop aValues
            while (0 < aValues.Count)
            {
                int a = _PopRandom(rand, ref aValues);
                Dictionary<Position, int> copiedTable = initTable.ToDictionary<Position, int>();
                copiedTable.Add(curPosi, a);
                _GenerateNormalizedSudoku_Loop(rand, nextLevel, copiedTable, w, nextPosi.r, nextPosi.c);
            }
        }

        public static int[][] RandomlizeSudoku(Random rand, int[][] sourceSudoku)
        {
            int w = sourceSudoku.Length;
            int[][] randSdu = NewBoard(w);

            // randomize cols
            int r, c;
            int[] randPopIdxList = _GetRandomPopIndexList(rand, w);
            for (r = 0; r < w; ++r)
            {
                List<int> tmpRow = new List<int>();
                tmpRow.AddRange(sourceSudoku[r]);
                for (c = 0; c < w; ++c)
                {
                    randSdu[r][c] = tmpRow[randPopIdxList[c]];
                    tmpRow.RemoveAt(randPopIdxList[c]);
                }
            }

            // rand rows
            randPopIdxList = _GetRandomPopIndexList(rand, w);
            List<int[]> tmpRows = new List<int[]>();
            tmpRows.AddRange(randSdu);
            for (r = 0; r < w; ++r)
            {
                randSdu[r] = tmpRows[randPopIdxList[r]];
                tmpRows.RemoveAt(randPopIdxList[r]);
            }
            return randSdu;
        }
        public static Task<int[][]> GenerateRandomSudoku(Random rand, int width)
        {
            return Task.Run(async () =>
            {
                int[][] normalizedSdu = await GenerateNormalizedSudoku(rand, width);
                return RandomlizeSudoku(rand, normalizedSdu);
            });
        }
        private static int[] _GetRandomPopIndexList(Random rand, int width)
        {
            int[] result = new int[width];
            for (int i = 0; i < width; ++i)
            {
                result[i] = rand.Next(width - i);
            }
            return result;
        }

        #endregion


        public struct Position
        {
            public int c;
            public int r;
        }

        public static List<Position> GetEmptyPositions(int[][]? boardData)
        {
            List<Position> result = new List<Position>();
            if (boardData is null || boardData.Length <= 0)
            {
                return result;
            }
            int c, r, w = boardData.Length;
            for (r = 0; r < w; ++r)
            {
                for (c = 0; c < w; ++c)
                {
                    if (boardData[r][c] < 0)
                    {
                        result.Add(new Position() { c = c, r = r, });
                    }
                }
            }

            return result;
        }
        public static List<Position> GetPresentPositions(int[][]? boardData)
        {
            List<Position> result = new List<Position>();
            if (boardData is null || boardData.Length <= 0)
            {
                return result;
            }
            int c, r, w = boardData.Length;
            for (r = 0; r < w; ++r)
            {
                for (c = 0; c < w; ++c)
                {
                    if (boardData[r][c] >= 0)
                    {
                        result.Add(new Position() { c = c, r = r, });
                    }
                }
            }

            return result;
        }


        private bool CheckBoardData(int[][] board, out int[][]? errFlags)
        {
            // check data
            bool result = true;
            int r, c, w = board.Length;
            errFlags = NewBoard(w);
            Position[] sameValuePosis;
            for (r = 0; r < w; ++r)
            {
                if (CheckRow(ref board, r, out sameValuePosis) == false)
                {
                    result = false;
                    SetErrFlags(ref errFlags, sameValuePosis);
                }
            }
            for (c = 0; c < w; ++c)
            {
                if (CheckCol(ref board, c, out sameValuePosis) == false)
                {
                    result = false;
                    SetErrFlags(ref errFlags, sameValuePosis);
                }
            }
            if (result)
            {
                errFlags = null;
            }
            return result;

            void SetErrFlags(ref int[][] errFlags, Position[] posis)
            {
                foreach (Position p in posis)
                {
                    errFlags[p.r][p.c] = 1;
                }
            }
        }


        #endregion




        #region init

        public int[][]? initBoardDataNormal = null;
        public int[][]? initBoardDataRand = null;
        public int[][]? initBoardTemplate = null;

        public Action<Core> InitBoardDataChanged;
        public Action<Core> InitBoardTemplateChanged;

        public void ClearInitBoards()
        {
            initBoardDataNormal = null;
            initBoardDataRand = null;
            InitBoardDataChanged?.Invoke(this);
            initBoardTemplate = null;
            InitBoardTemplateChanged?.Invoke(this);
        }
        public void ClearInitBoardTemplate()
        {
            if (initBoardTemplate is null)
            {
                return;
            }
            int r, c, w = initBoardTemplate.Length;
            bool changed = false;
            for (r = 0; r < w; ++r)
            {
                for (c = 0; c < w; ++c)
                {
                    if (0 <= initBoardTemplate[r][c])
                    {
                        initBoardTemplate[r][c] = -1;
                        changed = true;
                    }
                }
            }
            if (changed)
            {
                InitBoardTemplateChanged?.Invoke(this);
            }
        }
        public void ReInitBoards()
        {
            int w = _NumberCharList.Length;
            Task<int[][]> gerner = GenerateNormalizedSudoku(rand, w);
            while (gerner.Status != TaskStatus.RanToCompletion)
            {
                Task.Delay(1).Wait();
            }
            initBoardDataNormal = gerner.Result;
            initBoardDataRand = RandomlizeSudoku(rand, initBoardDataNormal);
            InitBoardDataChanged?.Invoke(this);
            initBoardTemplate = NewBoard(w);
            InitBoardTemplateChanged?.Invoke(this);
        }



        internal int AddRandomWithinInitBoardTemplate(int count = 1)
        {
            if (initBoardDataRand is null)
            {
                ReInitBoards();
            }
            if (initBoardDataRand is null)
            {
                throw new Exception("Init board data not generated.");
            }

            int addCount = 0;
            List<Position> blanks = GetEmptyPositions(initBoardTemplate);
            count = Math.Min(count, blanks.Count);
            if (count <= 0)
            {
                return 0;
            }
            int w = initBoardTemplate.Length;
            int blankIdx;
            Position blankPosi;
            int charListLength = _NumberCharList.Length;
            for (int i = count; 0 < i; --i)
            {
                blankIdx = rand.Next(blanks.Count);
                blankPosi = blanks[blankIdx];
                blanks.RemoveAt(blankIdx);
                initBoardTemplate[blankPosi.r][blankPosi.c]
                     = initBoardDataRand[blankPosi.r][blankPosi.c];
                addCount++;
            }
            if (0 < addCount)
            {
                InitBoardTemplateChanged?.Invoke(this);
            }
            return addCount;
        }

        internal int RemoveRandomWithinInitBoardTemplate(int count = 1)
        {
            int rmCount = 0;
            List<Position> presents = GetPresentPositions(initBoardTemplate);
            count = Math.Min(count, presents.Count);
            if (count <= 0)
            {
                return 0;
            }

            int presentIdx;
            Position presentPosi;
            for (int i = count; 0 < i; --i)
            {
                presentIdx = rand.Next(presents.Count);
                presentPosi = presents[presentIdx];
                presents.RemoveAt(presentIdx);
                initBoardTemplate[presentPosi.r][presentPosi.c] = -1;
                rmCount++;
            }
            if (0 < rmCount)
            {
                InitBoardTemplateChanged?.Invoke(this);
            }
            return rmCount;
        }


        #endregion



        #region play

        public int[][] playBoard;
        public int[][]? playBoard_errorFlags;
        public Action<Core> PlayBoardErrorFound;
        public Action<Core> PlayBoardNoErrorFound;
        internal void PlayInit()
        {
            playBoard = Core.NewBoard(initBoardTemplate.Length);
            Core.CopyBoard(initBoardTemplate, ref playBoard);
        }
        public void PlaySetCell(int row, int col, string? chr)
        {
            #region check n throw exceptions
            bool isChrErr = false;
            bool isChrEmpty = string.IsNullOrEmpty(chr);
            if (isChrEmpty == false)
            {
                if (chr.Length != 1
                    || NumberCharList.Contains(chr) == false)
                {
                    isChrErr = true;
                }
            }
            if (isChrErr)
            {
                throw new ArgumentOutOfRangeException("Cell char is out of range.");
            }
            if (initBoardTemplate is null)
            {
                throw new ArgumentNullException(nameof(initBoardTemplate));
            }
            if (playBoard is null)
            {
                throw new ArgumentNullException(nameof(playBoard));
            }
            int w = playBoard.Length;
            if (w != initBoardTemplate.Length)
            {
                throw new ArgumentException("Size between template and play board is different.");
            }
            if (row < 0 || col < 0
                || w <= row || w <= col)
            {
                throw new ArgumentOutOfRangeException("Position out of range.");
            }
            if (0 <= initBoardTemplate[row][col])
            {
                throw new ArgumentException("That position is not editable.");
            }
            #endregion
            if (isChrEmpty)
            {
                playBoard[row][col] = -1;
            }
            else
            {
                playBoard[row][col] = NumberCharList.IndexOf(chr);
            }
            if (CheckBoardData(playBoard, out playBoard_errorFlags) == false)
            {
                PlayBoardErrorFound?.Invoke(this);
            }
            else
            {
                PlayBoardNoErrorFound?.Invoke(this);
            }
        }

        #endregion



        #region solve

        public int[][] solvBoard_template;
        private int _solvBoard_width = 0;

        private object _solvCountSteps_locker = new object();
        public long solvCountSteps = 0;
        private object _solvCountSolutions_locker = new object();
        public long solvCountSolutions = 0;

        public Action<Core, int[][]> SolvBoardErrorFound;
        public Action<Core> SolvingComplete;
        public Action<Core> SolvingCanceled;
        public Action<Core, int[][]> SolvingSolutionFound;

        internal void SolvInit(int[][] templateBoard)
        {
            if (templateBoard is null || templateBoard.Length <= 0)
            {
                _solvBoard_width = 0;
                solvBoard_template = NewBoard(0);
                return;
            }
            _solvBoard_width = templateBoard.Length;
            solvBoard_template = NewBoard(_solvBoard_width);
            CopyBoard(templateBoard, ref solvBoard_template);

        }
        internal void SolvStart(bool parallel = true)
        {
            if (_solvBoard_width <= 0)
            {
                SolvingCanceled?.Invoke(this);
                return;
            }
            List<Position> emptys = GetEmptyPositions(solvBoard_template);
            if (emptys.Count <= 0)
            {
                SolvingCanceled?.Invoke(this);
                return;
            }
            if (CheckBoardData(solvBoard_template, out int[][]? errors) == false
                && errors is not null)
            {
                SolvingCanceled?.Invoke(this);
                SolvBoardErrorFound?.Invoke(this, errors);
                return;
            }

            // start loop
            _solvCancelFlag = false;
            solvCountSteps = 0;
            solvCountSolutions = 0;
            _SolvStart_loop(solvBoard_template, 0, parallel);
        }

        private Task _SolvStart_loop(int[][] testingBoard, int level, bool parallel)
        {
            return Task.Run(() =>
            {
                if (_solvCancelFlag)
                {
                    return;
                }
                int w = testingBoard.Length;
                List<Position> blanks = GetEmptyPositions(testingBoard);
                if (blanks.Count <= 0)
                {
                    SolvingSolutionFound?.Invoke(this, testingBoard);
                    _SolvStart_add_SolutionsFound();
                    return;
                }
                int nextLevel = level + 1;
                Position firstBlankPosi = blanks[0];
                List<Task> subTasks = new List<Task>();
                Task subTask;
                for (int v = 0; v < w; ++v)
                {
                    _SolvStart_add_Steps();
                    if (TestCell(ref testingBoard, firstBlankPosi.r, firstBlankPosi.c, v))
                    {
                        int[][] newBoard = NewBoard(w);
                        CopyBoard(testingBoard, ref newBoard);
                        newBoard[firstBlankPosi.r][firstBlankPosi.c] = v;
                        subTask = _SolvStart_loop(newBoard, nextLevel, parallel);
                        subTasks.Add(subTask);
                        if (parallel == false)
                        {
                            Task.WaitAll(subTask);
                        }
                    }
                }
                Task.WaitAll(subTasks.ToArray());
                if (level == 0)
                {
                    if (_solvCancelFlag)
                    {
                        SolvingCanceled?.Invoke(this);
                    }
                    else
                    {
                        SolvingComplete?.Invoke(this);
                    }
                }
            });
        }
        private void _SolvStart_add_Steps()
        {
            lock (_solvCountSteps_locker)
            {
                solvCountSteps++;
            }
        }
        private void _SolvStart_add_SolutionsFound()
        {
            lock (_solvCountSolutions_locker)
            {
                solvCountSolutions++;
            }
        }



        private bool _solvCancelFlag = false;
        internal void SolvCancel()
        {
            _solvCancelFlag = true;
        }
        #endregion
    }
}
