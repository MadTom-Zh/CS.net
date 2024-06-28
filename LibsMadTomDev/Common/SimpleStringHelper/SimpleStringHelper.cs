using System;

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MadTomDev.Common
{
    public partial class SimpleStringHelper
    {
        public class Checker_starNQues
        {
            private List<PatternData> pList = new List<PatternData>();

            public bool HavePattern { get => pList.Count > 0; }

            public void AddPattern(string pattern, bool matchCase = false, object tag = null)
            {
                // formate pattern string
                if (!matchCase)
                    pattern = pattern.ToLower();
                while (pattern.Contains("*?")) pattern = pattern.Replace("*?", "?*");
                while (pattern.Contains("**")) pattern = pattern.Replace("**", "*");

                // construct n' store pattern data
                PatternData newPData = new PatternData()
                { pattern = pattern, matchCase = matchCase, tag = tag };
                StringBuilder str = new StringBuilder();
                char c;
                for (int i = 0, iV = pattern.Length - 1; i <= iV; i++)
                {
                    c = pattern[i];
                    if (c == '*')
                    {
                        if (str.Length > 0)
                        {
                            newPData.pPieces.Add(new PatternData.patternPiece()
                            {
                                isStarOrQuesMark = null,
                                piece = str.ToString()
                            });
                            str.Clear();
                        }
                        newPData.pPieces.Add(new PatternData.patternPiece() { isStarOrQuesMark = true });
                    }
                    else if (c == '?')
                    {
                        if (str.Length > 0)
                        {
                            newPData.pPieces.Add(new PatternData.patternPiece()
                            {
                                isStarOrQuesMark = null,
                                piece = str.ToString()
                            });
                            str.Clear();
                        }
                        newPData.pPieces.Add(new PatternData.patternPiece() { isStarOrQuesMark = false });
                    }
                    else
                    {
                        str.Append(c);
                    }
                }
                if (str.Length > 0)
                {
                    newPData.pPieces.Add(new PatternData.patternPiece()
                    {
                        isStarOrQuesMark = null,
                        piece = str.ToString()
                    });
                }
                pList.Add(newPData);
            }
            public int RemovePattern(string pattern, bool matchCase = false)
            {
                int result = 0;
                PatternData pData;
                for (int i = pList.Count - 1; i >= 0; i--)
                {
                    pData = pList[i];
                    if (pData.pattern == pattern
                        && pData.matchCase == matchCase)
                    {
                        pList.RemoveAt(i);
                        result++;
                    }
                }
                return result;
            }
            public void ClearPatterns()
            { pList.Clear(); }

            private class PatternData
            {
                public string pattern;
                public List<patternPiece> pPieces = new List<patternPiece>();
                public bool matchCase = false;
                public struct patternPiece
                {
                    public bool? isStarOrQuesMark;
                    public string piece;
                }
                /// <summary>
                /// for additional matching, if needed
                /// </summary>
                public object tag;
            }
            public bool Check(string source, object[] inTags = null)
            {
                if (string.IsNullOrEmpty(source))
                {
                    return false;
                }

                int sourceIdx, sourceIdxTmp, sourceLength;
                string sourceTmp;
                PatternData.patternPiece curPp;
                bool? preStarOrQues = null;
                bool result = false;
                bool tagMatch = true;

                foreach (PatternData pData in pList)
                {
                    if (inTags != null)
                    {
                        tagMatch = false;
                        foreach (object o in inTags)
                        {
                            if (o.ToString() == pData.tag.ToString())
                            {
                                tagMatch = true;
                                break;
                            }
                        }
                    }
                    if (!tagMatch)
                        continue;

                    sourceIdx = 0;
                    if (pData.matchCase) sourceTmp = source;
                    else sourceTmp = source.ToLower();
                    sourceLength = sourceTmp.Length;
                    for (int i = 0, iv = pData.pPieces.Count - 1; i <= iv; i++)
                    {
                        curPp = pData.pPieces[i];
                        if (curPp.isStarOrQuesMark == true)
                        {
                            preStarOrQues = true;
                            if (i == iv)
                            {
                                result = true;
                                break;
                            }
                        }
                        else if (curPp.isStarOrQuesMark == false)
                        {
                            preStarOrQues = false;
                            sourceIdx++;
                        }
                        else
                        {
                            sourceIdxTmp = sourceTmp.IndexOf(curPp.piece, sourceIdx);
                            if (preStarOrQues != true
                                && sourceIdxTmp != sourceIdx)
                                break;
                            if (sourceIdxTmp < 0)
                                break;
                            sourceIdx = sourceIdxTmp + curPp.piece.Length;
                            preStarOrQues = null;
                        }
                        if (sourceIdx > sourceLength)
                            break;
                    }
                    if (tagMatch)
                    {
                        if (sourceIdx == sourceLength)
                            return true;
                        else if (result)
                            return true;
                    }
                }
                return false;
            }

        }
        public static bool CheckMatch_starNQues(string pattern, string source, bool matchCase = false)
        {
            Checker_starNQues checker = new Checker_starNQues();
            checker.AddPattern(pattern, matchCase);
            return checker.Check(source);
        }

        public static bool CheckLoopExist(string source, out string loopedString, int minLoopCount = 3, int minLoopStringLength = 2)
        {
            loopedString = null;
            int sourceLength = source.Length;
            int maxLoopStrLength = sourceLength / minLoopCount;
            int lastCheckStartIdx, loopCount, startIdxNext;
            string subStr1, subStr2;
            for (int loopStrLength = maxLoopStrLength; loopStrLength >= minLoopStringLength; loopStrLength--)
            {
                lastCheckStartIdx = sourceLength - loopStrLength * minLoopCount;
                for (int startIdx = 0; startIdx <= lastCheckStartIdx; startIdx++)
                {
                    startIdxNext = startIdx;
                    subStr1 = source.Substring(startIdxNext, loopStrLength);
                    loopCount = 1;
                    startIdxNext += loopStrLength;
                    while (true)
                    {
                        subStr2 = source.Substring(startIdxNext, loopStrLength);
                        if (subStr1 == subStr2)
                            loopCount++;
                        else break;
                        startIdxNext += loopStrLength;
                        if ((startIdxNext + loopStrLength) >= sourceLength)
                            break;
                    }
                    if (loopCount >= minLoopCount)
                    {
                        loopedString = subStr1;
                        return true;
                    }
                }
            }
            return false;
        }

        public class SpaceRemover
        {
            Regex charChecker = new Regex(@"^[A-Za-z0-9]+$");
            public string RemoveAdditionalSpaces(string line)
            {
                int startIdx = 0, spaceIdx = 0;
                StringBuilder sBuilder = new StringBuilder();
                line = line.Trim();
                while (line.Contains("  ")) line = line.Replace("  ", " ");

                string charBeforeSpace;
                while (true)
                {
                    spaceIdx = line.IndexOf(' ', startIdx);
                    if (spaceIdx == 0)
                    {
                        startIdx = 1;
                        continue;
                    }
                    else if (spaceIdx == -1)
                    {
                        sBuilder.Append(line.Substring(startIdx, line.Length - startIdx));
                        break;
                    }

                    charBeforeSpace = line.Substring(spaceIdx - 1, 1);
                    if (charChecker.IsMatch(charBeforeSpace))
                    {
                        sBuilder.Append(line.Substring(startIdx, spaceIdx - startIdx + 1));
                    }
                    else
                    {
                        sBuilder.Append(line.Substring(startIdx, spaceIdx - startIdx));
                    }
                    startIdx = spaceIdx + 1;
                }

                return sBuilder.ToString();
            }
        }

        public class NewLineEncoder
        {

            public static string Encode(string inText)
            {
                return inText.Replace("\\n", "\\\\nn").Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\\n");
            }
            public static string Decode(string inText)
            {
                StringBuilder strBdr = new StringBuilder();
                int i = inText.IndexOf("\\n"), iPre = 0;
                if (i < 0)
                    return inText;

                int l = inText.Length;
                while (true)
                {
                    if (i < 0)
                        break;

                    if (i > 0 && inText[i - 1] == '\\'
                        && i + 2 < l && inText[i + 2] == 'n')
                        strBdr.Append(inText.Substring(iPre, i - iPre));
                    else
                        strBdr.AppendLine(inText.Substring(iPre, i - iPre));

                    iPre = i + 2;
                    i = inText.IndexOf("\\n", i + 2);
                }
                strBdr.Append(inText.Substring(iPre));

                return strBdr.ToString();
            }
        }

        public static string RemoveAdditionalSpace(string source)
        {
            SpaceRemover remover = new SpaceRemover();
            return remover.RemoveAdditionalSpaces(source);
        }


        /// <summary>
        /// check a char is not a regular letter, such as chinese char or japanise char.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool Check_IsWideLetter(char c)
        {
            UnicodeCategory cate = char.GetUnicodeCategory(c);
            switch (cate)
            {
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.OtherPunctuation:
                    foreach (char testC in ",.<>/?;:'\"[]{}\\|`~!@#$%^&*()_+")
                    {
                        if (c == testC)
                            return false;
                    }
                    return true;
                case UnicodeCategory.InitialQuotePunctuation:
                case UnicodeCategory.FinalQuotePunctuation:
                    return true;
            }
            return false;
        }



        public class TableFormater
        {
            public TableFormater()
            { }

            public enum TableBorderStyles
            { None = 0, OuterFrame = 1, HorLines = 2, VerLines = 4 }
            public TableBorderStyles TableBorderLineStyle { set; get; }
                = TableBorderStyles.OuterFrame;
            public TableBorderStyles TableHeaderLineStyle { set; get; }
                = TableBorderStyles.OuterFrame | TableBorderStyles.VerLines;
            public TableBorderStyles TableDataRowLineStyle { set; get; }
                = TableBorderStyles.OuterFrame | TableBorderStyles.HorLines | TableBorderStyles.VerLines;

            public bool IsPrintHeader { set; get; } = true;

            private List<string> _ColumnHeaders = new List<string>();
            public List<string> ColumnHeaders
            {
                set
                {
                    _ColumnHeaders = value;
                    NeedReGenInnerRows = true;
                }
            }
            public List<string> GetColumnHeadersClone()
            {
                List<string> clone = new List<string>();
                clone.AddRange(_ColumnHeaders.ToArray());
                return clone;
            }
            public void AddColumnHeader(string header)
            {
                _ColumnHeaders.Add(header);
                NeedReGenInnerRows = true;
            }
            public void ClearColumnHeaders()
            {
                _ColumnHeaders.Clear();
                NeedReGenInnerRows = true;
            }


            private FormattedCellData ColumnHeadersInner;
            /// <summary>
            /// set all columns width to minimum, with out wrapping
            /// </summary>
            public void AutoSetColumnWidthes()
            {
                ClearColumnWidthes();
                for (int colIdx = 0, colV = _ColumnHeaders.Count; colIdx < colV; colIdx++)
                {
                    AddClumnWidth(GetMaxWidth(colIdx));
                }
                NeedReGenInnerRows = true;
            }

            private List<int> _ColumnWidthes = new List<int>();
            /// <summary>
            /// if cell width is greater than cur col-width, col-width will set to greater one,
            /// unless col-formate is set to wrapping;
            /// </summary>
            public List<int> ColumnWidthes
            {
                set
                {
                    _ColumnWidthes = value;
                    NeedReGenInnerRows = true;
                }
            }
            public List<int> GetColumnWidthesClone()
            {
                List<int> clone = new List<int>();
                clone.AddRange(_ColumnWidthes.ToArray());
                return clone;
            }
            public void AddClumnWidth(int width)
            {
                _ColumnWidthes.Add(width);
                NeedReGenInnerRows = true;
            }
            public void ClearColumnWidthes()
            {
                _ColumnWidthes.Clear();
                NeedReGenInnerRows = true;
            }
            public enum CellFormats
            { None, AlignLeft, AlignRight, AlignCenter, AlignLeftWrapping, AlignRightWrapping, AlignCenterWrapping }
            private List<CellFormats> _ColumnFormats = new List<CellFormats>();
            public List<CellFormats> ColumnFormats
            {
                set
                {
                    _ColumnFormats = value;
                    NeedReGenInnerRows = true;
                }
            }
            public List<CellFormats> GetColumnFormatsClone()
            {
                List<CellFormats> clone = new List<CellFormats>();
                clone.AddRange(_ColumnFormats.ToArray());
                return clone;
            }
            public void AddColumnFormat(CellFormats format)
            {
                _ColumnFormats.Add(format);
                NeedReGenInnerRows = true;
            }
            public void ClearColumnFormats()
            {
                _ColumnFormats.Clear();
                NeedReGenInnerRows = true;
            }

            private List<string[]> _Rows = new List<string[]>();
            public List<string[]> Rows
            {
                set
                {
                    NeedReGenInnerRows = true;
                    _Rows = value;
                }
            }
            public void ClearRows()
            {
                _Rows.Clear();
            }
            public List<string[]> GetRowsClone()
            {
                List<string[]> clone = new List<string[]>();
                clone.AddRange(_Rows.ToArray());
                return clone;
            }
            public void AddRow(params string[] row)
            {
                NeedReGenInnerRows = true;
                _Rows.Add(row);
            }
            private List<FormattedCellData> FormattedRows = new List<FormattedCellData>();
            private class FormattedCellData
            {
                public class InnerRowData
                {
                    public string rowString = "";
                    public int displayLength = 0;
                }
                public List<InnerRowData[]> innerRows = new List<InnerRowData[]>();
                public void SetCellHeight(int height)
                { CellHeight = height; }
                public int CellHeight
                {
                    private set; get;
                }
            }

            private void CheckNCurrect()
            {
                CellFormats curColFormat;
                int curColMaxWidth;
                for (int colIdx = 0, colV = _ColumnHeaders.Count; colIdx < colV; colIdx++)
                {
                    if (_ColumnFormats.Count < colIdx + 1)
                    {
                        _ColumnFormats.Add(CellFormats.None);
                    }
                    curColMaxWidth = GetMaxWidth(colIdx);
                    if (_ColumnWidthes.Count < colIdx + 1)
                    {
                        _ColumnWidthes.Add(curColMaxWidth);
                    }
                    curColFormat = _ColumnFormats[colIdx];
                    switch (curColFormat)
                    {
                        default:
                            if (_ColumnWidthes[colIdx] < curColMaxWidth)
                                _ColumnWidthes[colIdx] = curColMaxWidth;
                            break;
                        case CellFormats.AlignCenterWrapping:
                        case CellFormats.AlignLeftWrapping:
                        case CellFormats.AlignRightWrapping:
                            if (_ColumnWidthes[colIdx] < 1)
                                _ColumnWidthes[colIdx] = 1;
                            break;
                    }
                }
            }
            private int GetMaxWidth(int colIdx)
            {
                int curLength, result = 0;
                foreach (string[] row in _Rows)
                {
                    if (row.Length > colIdx)
                    {
                        curLength = row[colIdx].Length;
                        if (curLength > result)
                            result = curLength;
                    }
                }
                return Math.Max(result, _ColumnHeaders[colIdx].Length);
            }

            private bool NeedReGenInnerRows = false;
            private void TryReGenInnerRows()
            {
                if (!NeedReGenInnerRows) return;

                string tmp1;
                int tmp2;
                int colsCount = _ColumnHeaders.Count, maxInnerRowsCount;
                string[] row;
                FormattedCellData.InnerRowData[] cellBlock;
                ColumnHeadersInner = new FormattedCellData();
                maxInnerRowsCount = 1;
                for (int colIdx = 0; colIdx < colsCount; colIdx++)
                {
                    if (_ColumnFormats[colIdx].ToString().EndsWith("Wrapping"))
                    {
                        cellBlock = GetCellBlock(_ColumnHeaders[colIdx], _ColumnWidthes[colIdx]);
                        ColumnHeadersInner.innerRows.Add(cellBlock);
                        if (maxInnerRowsCount < cellBlock.Length)
                            maxInnerRowsCount = cellBlock.Length;
                    }
                    else
                    {
                        tmp1 = _ColumnHeaders[colIdx];
                        ColumnHeadersInner.innerRows.Add(
                            new FormattedCellData.InnerRowData[]
                            {
                                new FormattedCellData.InnerRowData()
                                {
                                    rowString = tmp1, displayLength=GetDisplayLength(tmp1,out tmp2)
                                }
                            });
                    }
                }
                ColumnHeadersInner.SetCellHeight(maxInnerRowsCount);

                FormattedRows.Clear();

                int oLs;
                FormattedCellData innerRow;
                for (int rowIdx = 0, rowV = _Rows.Count, colIdx, colV;
                    rowIdx < rowV; rowIdx++)
                {
                    row = _Rows[rowIdx];
                    innerRow = new FormattedCellData();
                    maxInnerRowsCount = 1;
                    for (colIdx = 0, colV = Math.Min(colsCount, row.Length);
                        colIdx < colV; colIdx++)
                    {
                        if (_ColumnFormats[colIdx].ToString().EndsWith("Wrapping"))
                        {
                            cellBlock = GetCellBlock(row[colIdx], _ColumnWidthes[colIdx]);
                            innerRow.innerRows.Add(cellBlock);
                            if (maxInnerRowsCount < cellBlock.Length)
                                maxInnerRowsCount = cellBlock.Length;
                        }
                        else
                        {
                            tmp1 = row[colIdx];
                            innerRow.innerRows.Add(
                                new FormattedCellData.InnerRowData[]
                                {
                                    new FormattedCellData.InnerRowData()
                                    {
                                        rowString= tmp1,displayLength=GetDisplayLength(tmp1,out oLs)
                                    }
                                });
                        }
                    }
                    innerRow.SetCellHeight(maxInnerRowsCount);
                    FormattedRows.Add(innerRow);
                }
            }
            private FormattedCellData.InnerRowData[] GetCellBlock(string text, int limitWidth)
            {
                int otherLetters;
                int displayLength = GetDisplayLength(text, out otherLetters);


                if (displayLength <= limitWidth)
                {
                    return new FormattedCellData.InnerRowData[]
                    {
                        new FormattedCellData.InnerRowData ()
                        { rowString = text, displayLength = displayLength }
                    };
                }

                int textLength = text.Length;
                List<FormattedCellData.InnerRowData> result
                    = new List<FormattedCellData.InnerRowData>();
                string test, tmp2;
                int startIdx = 0, idxSpaceOrSmb, limitWidth_within, tmp1;
                char[] spaceOrSmb = new char[] { ' ', ',', '，', '.', '。', '/', '\\', '|' };
                while (startIdx < textLength)
                {
                    limitWidth_within
                        = (startIdx + limitWidth > textLength)
                        ? (textLength - startIdx) : limitWidth;
                    test = text.Substring(startIdx, limitWidth_within);
                    tmp1 = GetStringLengthByDisplayLength(test, limitWidth, out otherLetters);
                    if (tmp1 < limitWidth_within)
                    {
                        limitWidth_within = tmp1;
                        test = text.Substring(startIdx, limitWidth_within);
                    }
                    idxSpaceOrSmb = test.LastIndexOfAny(spaceOrSmb);
                    if (idxSpaceOrSmb > 0)
                    {
                        tmp2 = test.Substring(0, idxSpaceOrSmb + 1);
                        result.Add(
                            new FormattedCellData.InnerRowData()
                            { rowString = tmp2, displayLength = GetDisplayLength(tmp2, out otherLetters) });
                        startIdx += idxSpaceOrSmb + 1;
                    }
                    else
                    {
                        result.Add(
                            new FormattedCellData.InnerRowData()
                            { rowString = test, displayLength = test.Length + otherLetters });
                        startIdx += limitWidth_within;
                    }
                }
                return result.ToArray();
            }
            private int GetDisplayLength(string str, out int otherLettersCount)
            {
                int lengthCount = 0;
                otherLettersCount = 0;
                foreach (char c in str)
                {

                    if (Check_IsWideLetter(c))
                    {
                        otherLettersCount++;
                        lengthCount += 2;
                    }
                    else
                    {
                        lengthCount += 1;
                    }
                }
                return lengthCount;
            }
            private int GetStringLengthByDisplayLength(string str, int displayLength, out int otherLettersCount)
            {
                otherLettersCount = 0;
                int result = 0;
                int lengthCount = 0;
                foreach (char c in str)
                {
                    result++;
                    if (Check_IsWideLetter(c))
                    {
                        otherLettersCount++;
                        lengthCount += 2;
                    }
                    else
                    {
                        lengthCount += 1;
                    }
                    if (lengthCount == displayLength)
                        return result;
                    else if (lengthCount > displayLength)
                    {
                        return result - 1;
                    }
                }
                return result;
            }

            public const string Tab_TopLeft = "┌";
            public const string Tab_TopOrButtom = "─";
            public const string Tab_TopRight = "┐";
            public const string Tab_TopToButtom = "┬";
            public const string Tab_LeftOrRight = "│";
            public const string Tab_LeftToRight = "├";
            public const string Tab_RightToLeft = "┤";
            public const string Tab_Center = "┼";
            public const string Tab_ButtomToTop = "┴";
            public const string Tab_ButtomLeft = "└";
            public const string Tab_ButtomRight = "┘";

            public string ToFullString()
            {
                CheckNCurrect();
                TryReGenInnerRows();

                StringBuilder result = new StringBuilder();

                bool drawTableBorderFrame
                    = (TableBorderLineStyle & TableBorderStyles.OuterFrame) == TableBorderStyles.OuterFrame;
                // draw table header lines
                if (drawTableBorderFrame)
                {
                    DrawHorLine(result, 0);
                    result.Append(Environment.NewLine);
                }

                // draw header
                if (IsPrintHeader)
                {
                    DrawRow(result, ColumnHeadersInner, (TableHeaderLineStyle & TableBorderStyles.VerLines) == TableBorderStyles.VerLines);
                    result.Append(Environment.NewLine);
                }

                // draw table after-header lines
                if (IsPrintHeader && (TableHeaderLineStyle & TableBorderStyles.OuterFrame) == TableBorderStyles.OuterFrame)
                {
                    DrawHorLine(result, 1);
                    result.Append(Environment.NewLine);
                }

                bool drawDataRowHonLines
                    = (TableDataRowLineStyle & TableBorderStyles.HorLines) == TableBorderStyles.HorLines;
                bool drawDataRowVerLines
                    = (TableDataRowLineStyle & TableBorderStyles.VerLines) == TableBorderStyles.VerLines;
                for (int rowIdx = 0, rowV = FormattedRows.Count; rowIdx < rowV; rowIdx++)
                {

                    if (rowIdx != 0 && drawDataRowHonLines)
                    {
                        DrawHorLine(result, 2);
                        result.Append(Environment.NewLine);
                    }

                    DrawRow(result, FormattedRows[rowIdx], drawDataRowVerLines);
                    result.Append(Environment.NewLine);
                }

                // draw table end lines
                if (drawTableBorderFrame)
                {
                    DrawHorLine(result, 3);
                    //result.Append(Environment.NewLine);
                }

                return result.ToString();
            }


            /// <summary>
            /// draw row of table border
            /// </summary>
            /// <param name="location">0-top, 1-after header, 2-after data row, 3-buttom</param>
            private void DrawHorLine(StringBuilder strBdr, int location)
            {
                string lineEnd = null;
                // left border
                if (location == 0)
                {
                    if ((TableBorderLineStyle & TableBorderStyles.OuterFrame) == TableBorderStyles.OuterFrame)
                    {
                        strBdr.Append(Tab_TopLeft);
                        lineEnd = Tab_TopRight;
                    }
                }
                else if (location == 1)
                {
                    if ((TableBorderLineStyle & TableBorderStyles.OuterFrame) == TableBorderStyles.OuterFrame)
                    {
                        if ((TableHeaderLineStyle & TableBorderStyles.OuterFrame) == TableBorderStyles.OuterFrame)
                        {
                            strBdr.Append(Tab_LeftToRight);
                            lineEnd = Tab_RightToLeft;
                        }
                        else
                        {
                            strBdr.Append(Tab_LeftOrRight);
                            lineEnd = Tab_LeftOrRight;
                        }
                    }
                }
                else if (location == 2)
                {
                    if ((TableBorderLineStyle & TableBorderStyles.OuterFrame) == TableBorderStyles.OuterFrame)
                    {
                        if ((TableDataRowLineStyle & TableBorderStyles.HorLines) == TableBorderStyles.HorLines)
                        {
                            strBdr.Append(Tab_LeftToRight);
                            lineEnd = Tab_RightToLeft;
                        }
                        else
                        {
                            strBdr.Append(Tab_LeftOrRight);
                            lineEnd = Tab_LeftOrRight;
                        }
                    }
                }
                else // 3
                {
                    if ((TableBorderLineStyle & TableBorderStyles.OuterFrame) == TableBorderStyles.OuterFrame)
                    {
                        strBdr.Append(Tab_ButtomLeft);
                        lineEnd = Tab_ButtomRight;
                    }
                }

                // inner line
                bool drawHorLine
                        = ((location == 0 || location == 3)
                            && ((TableBorderLineStyle & TableBorderStyles.OuterFrame) == TableBorderStyles.OuterFrame))
                        || (location == 1 && ((TableHeaderLineStyle & TableBorderStyles.OuterFrame) == TableBorderStyles.OuterFrame)
                        || (location == 2 && ((TableDataRowLineStyle & TableBorderStyles.HorLines) == TableBorderStyles.HorLines)));
                bool drawHeaderVerLine
                        = (TableHeaderLineStyle & TableBorderStyles.VerLines) == TableBorderStyles.VerLines;
                bool drawDataRowVerLine
                        = (TableDataRowLineStyle & TableBorderStyles.VerLines) == TableBorderStyles.VerLines;
                for (int i = 0, i2,
                    iv = _ColumnHeaders.Count, i2v;
                    i < iv; i++)
                {
                    if (i != 0)
                    {
                        if (location == 1)
                        {
                            // after header
                            if (drawHorLine)
                            {
                                if (drawHeaderVerLine && drawDataRowVerLine)
                                {
                                    strBdr.Append(Tab_Center);
                                }
                                else if (drawHeaderVerLine)
                                {
                                    strBdr.Append(Tab_ButtomToTop);
                                }
                                else if (drawDataRowVerLine)
                                {
                                    strBdr.Append(Tab_TopToButtom);
                                }
                                else
                                    strBdr.Append(Tab_TopOrButtom);
                            }
                        }
                        else if (location == 2)
                        {
                            // after data row
                            if (drawHorLine)
                            {
                                if (drawDataRowVerLine)
                                    strBdr.Append(Tab_Center);
                                else
                                    strBdr.Append(Tab_TopOrButtom);
                            }
                        }
                        else if (location == 0)
                        {
                            // top
                            if (drawHorLine)
                            {
                                if (drawHeaderVerLine)
                                    strBdr.Append(Tab_TopToButtom);
                                else
                                    strBdr.Append(Tab_TopOrButtom);
                            }
                        }
                        else
                        {
                            // buttom
                            if (drawHorLine)
                            {
                                if (drawDataRowVerLine)
                                    strBdr.Append(Tab_ButtomToTop);
                                else
                                    strBdr.Append(Tab_TopOrButtom);
                            }
                        }
                    }

                    for (i2 = 0, i2v = _ColumnWidthes[i]; i2 < i2v; i2++)
                    {
                        if (drawHorLine)
                            strBdr.Append(Tab_TopOrButtom);
                        else
                            strBdr.Append(" ");
                    }

                }

                // right border
                strBdr.Append(lineEnd);
            }
            private void DrawRow(StringBuilder strBdr, FormattedCellData formattedRow, bool drawVerLines, int startInLineIdx = 0, int endInLineIdx = -1)
            {
                if (endInLineIdx < 0)
                    endInLineIdx = formattedRow.CellHeight - 1;

                int colFormatsCount = _ColumnFormats.Count;
                int formateCharLength, freeLength, halfFreeLength, leftSpaces, rightSpaces;
                string tmp1;
                FormattedCellData.InnerRowData[] block;
                FormattedCellData.InnerRowData blockPiece;
                CellFormats cellFormat;
                bool haveOuterFrame = (TableBorderLineStyle & TableBorderStyles.OuterFrame) == TableBorderStyles.OuterFrame;
                for (int r = startInLineIdx; r <= endInLineIdx; r++)
                {
                    if (r != startInLineIdx)
                        strBdr.Append(Environment.NewLine);

                    if (haveOuterFrame)
                    {
                        strBdr.Append(Tab_LeftOrRight);
                    }


                    for (int i = 0,
                        iv = _ColumnHeaders.Count;
                        i < iv; i++)
                    {
                        if (i != 0)
                        {
                            if (drawVerLines)
                                strBdr.Append(Tab_LeftOrRight);
                            else
                                strBdr.Append(" ");
                        }
                        if (formattedRow.innerRows.Count > i)
                            block = formattedRow.innerRows[i];
                        else
                            block = new FormattedCellData.InnerRowData[0];
                        if (block.Length > r)
                            blockPiece = block[r];
                        else
                            blockPiece = new FormattedCellData.InnerRowData();

                        formateCharLength = _ColumnWidthes[i] + blockPiece.rowString.Length - blockPiece.displayLength;
                        if (i >= colFormatsCount)
                        {
                            // go left
                            strBdr.Append(string.Format("{0," + (-formateCharLength) + "}", blockPiece.rowString));
                        }
                        else
                        {
                            // use format
                            cellFormat = _ColumnFormats[i];
                            switch (cellFormat)
                            {
                                default:
                                case CellFormats.AlignLeft:
                                case CellFormats.AlignLeftWrapping:
                                    strBdr.Append(string.Format("{0," + (-formateCharLength) + "}", blockPiece.rowString));
                                    break;
                                case CellFormats.AlignRight:
                                case CellFormats.AlignRightWrapping:
                                    strBdr.Append(string.Format("{0," + formateCharLength + "}", blockPiece.rowString));
                                    break;
                                case CellFormats.AlignCenter:
                                case CellFormats.AlignCenterWrapping:
                                    freeLength = formateCharLength - blockPiece.rowString.Length;

                                    leftSpaces = GetLeftSpaces(blockPiece.rowString);
                                    rightSpaces = GetRightSpaces(blockPiece.rowString);
                                    tmp1 = blockPiece.rowString.Trim();

                                    freeLength += leftSpaces + rightSpaces;
                                    halfFreeLength = freeLength / 2;

                                    AddSpace(strBdr, halfFreeLength);
                                    strBdr.Append(tmp1);
                                    AddSpace(strBdr, halfFreeLength);
                                    if (freeLength % 2 == 1)
                                        strBdr.Append(" ");
                                    break;
                            }
                        }
                    }


                    if (haveOuterFrame)
                    {
                        strBdr.Append(Tab_LeftOrRight);
                    }
                }
            }
            private void AddSpace(StringBuilder strBdr, int spaceCount)
            {
                for (int i = 0; i < spaceCount; i++)
                    strBdr.Append(" ");
            }
            private int GetLeftSpaces(string str)
            {
                int count = 0;
                for (int i = 0, iv = str.Length; i < iv; i++)
                {
                    if (str[i] == ' ')
                        count++;
                    else break;
                }
                return count;
            }
            private int GetRightSpaces(string str)
            {
                int count = 0;
                for (int i = str.Length - 1; i >= 0; i--)
                {
                    if (str[i] == ' ')
                        count++;
                    else break;
                }
                return count;
            }

            /// <summary>
            /// the full height contains table lines height and header height
            /// </summary>
            //public int PageFullHeight { set; get; }

            /// <summary>
            /// Give a partial table from page;
            /// set PageFullHeight first, or it will be one full table
            /// </summary>
            /// <param name="page">which page to get</param>
            /// <returns></returns>
            //public string GetPageString(int page)
            //{
            //    CheckNCurrect();
            //    TryReGenInnerRows();

            //    StringBuilder result = new StringBuilder();






            //    return result.ToString();
            //}
        }




        public class UnitsOfMeasure
        {
            public static string GetShortString(
                decimal baseValue, string unitStr,
                int kilo = 1000, int decimalDigits = 2,
                bool figureUpperCase = true,
                bool oneSpaceWhenNoFigure = true,
                bool figureFullName = false)
            {
                int kiloDivCount = 0;
                while (baseValue >= kilo)
                {
                    baseValue /= kilo;
                    kiloDivCount++;
                }
                StringBuilder result = new StringBuilder();
                string deciFormat = "0";
                if (decimalDigits > 0)
                {
                    deciFormat = "0.";
                    for (int i = decimalDigits; i > 0; i--)
                    {
                        deciFormat += "0";
                    }
                    result.Append(baseValue.ToString(deciFormat));
                }
                else if (decimalDigits == 0)
                {
                    result.Append(decimal.Truncate(baseValue).ToString(deciFormat));
                }
                else
                {
                    result.Append(baseValue.ToString());
                }
                string fig = "";
                switch (kiloDivCount)
                {
                    case 0: break;
                    case 1:
                        if (figureFullName) fig = "Kilo";
                        else fig = "K";
                        break;
                    case 2:
                        if (figureFullName) fig = "Mega";
                        else fig = "M";
                        break;
                    case 3:
                        if (figureFullName) fig = "Giga";
                        else fig = "G";
                        break;
                    case 4:
                        if (figureFullName) fig = "Tera";
                        else fig = "T";
                        break;
                    case 5:
                        if (figureFullName) fig = "Peta";
                        else fig = "P";
                        break;
                    case 6:
                        if (figureFullName) fig = "Exa";
                        else fig = "E";
                        break;
                    case 7:
                        if (figureFullName) fig = "Bronto";
                        else fig = "B";
                        break;
                    default:
                        fig = "[Err]";
                        break;
                }
                if (!figureUpperCase && fig.Length > 0)
                    fig = fig.Substring(0, 1).ToLower() + fig.Substring(1);
                if (fig.Length == 0 && oneSpaceWhenNoFigure)
                    fig = " ";


                bool haveFig = fig.Length > 0;
                bool haveUnitStr = !string.IsNullOrWhiteSpace(unitStr);
                if (haveFig || haveUnitStr)
                    result.Append(" ");

                if (haveFig)
                    result.Append(fig);
                if (haveUnitStr)
                    result.Append(unitStr);

                return result.ToString();
            }

            public static bool CheckShortString(string shortString, out decimal result, int kilo = 1000)
            {
                decimal kiloMultiple = 0;
                shortString = shortString.ToLower();
                if (shortString.EndsWith(" kilo") || shortString.EndsWith(" k"))
                    kiloMultiple = 1;
                else if (shortString.EndsWith(" mega") || shortString.EndsWith(" m"))
                    kiloMultiple = 2;
                else if (shortString.EndsWith(" giga") || shortString.EndsWith(" g"))
                    kiloMultiple = 3;
                else if (shortString.EndsWith(" tera") || shortString.EndsWith(" t"))
                    kiloMultiple = 4;
                else if (shortString.EndsWith(" peta") || shortString.EndsWith(" p"))
                    kiloMultiple = 5;
                else if (shortString.EndsWith(" exa") || shortString.EndsWith(" e"))
                    kiloMultiple = 6;
                else if (shortString.EndsWith(" bronto") || shortString.EndsWith(" b"))
                    kiloMultiple = 7;

                if (kiloMultiple > 0)
                {
                    shortString = shortString.Substring(0, shortString.LastIndexOf(" "));
                    if (decimal.TryParse(shortString, out result))
                    {
                        for (int i = 0; i < kiloMultiple; i++)
                            result *= kilo;
                        return true;
                    }
                    else return false;
                }
                else
                {
                    return decimal.TryParse(shortString, out result);
                }
            }



            /// <summary>
            /// 将数字转换为中文数字字符串；
            /// 例如输入"12,345.6"，返回为"壹贰叁肆伍陆"或"壹万贰千叁百肆拾伍点陆"
            /// </summary>
            /// <param name="value"></param>
            /// <param name="unitStr">(可选)数字后面的单位，如"个、只、束"等</param>
            /// <param name="decimalDigits">要保留的小数位数</param>
            /// <param name="fullChinese">是否使用进位字，如"拾、百、千、万"等；</param>
            /// <returns></returns>
            public static string GetChineseString(
                decimal value, string unitStr,
                int decimalDigits = -1, bool fullChinese = true)
            {
                StringBuilder result = new StringBuilder();

                string deciFormat = "0";
                if (decimalDigits > 0)
                {
                    deciFormat = "0.";
                    for (int i = decimalDigits; i > 0; i--)
                    {
                        deciFormat += "0";
                    }
                    result.Append(GetChineseString(value.ToString(deciFormat), fullChinese));
                }
                else if (decimalDigits == 0)
                {
                    result.Append(GetChineseString(decimal.Truncate(value).ToString(deciFormat), fullChinese));
                }
                else
                {
                    result.Append(GetChineseString(value.ToString(), fullChinese));
                }
                if (!string.IsNullOrWhiteSpace(unitStr))
                    result.Append(unitStr);
                return result.ToString();
            }


            /// <summary>
            /// 将数字字符串转换为中文数字字符串；
            /// 例如输入"12,345.6"，返回为"壹贰叁肆伍陆"或"壹万贰千叁百肆拾伍点陆"
            /// </summary>
            /// <param name="digitalString">形式如"12345.6"或"12,345.6"</param>
            /// <param name="fullChinese">是否使用进位字，如"拾、百、千、万"等；</param>
            /// <returns></returns>
            public static string GetChineseString(string digitalString, bool fullChinese = true)
            {
                string intStr, floStr = "";
                digitalString.Replace(",", "");
                if (digitalString.Contains("."))
                {
                    int dotIdx = digitalString.IndexOf(".");
                    intStr = digitalString.Substring(0, dotIdx);
                    floStr = digitalString.Substring(dotIdx + 1);
                }
                else
                {
                    intStr = digitalString;
                }

                StringBuilder result = new StringBuilder();
                result.Append(GetChineseString_IntStr(intStr, fullChinese));
                if (floStr.Length > 0)
                {
                    result.Append(ChineseDot);
                    result.Append(GetChineseString_IntStr(floStr, false));
                }
                return result.ToString();
            }
            private static string GetChineseString_IntStr(string intStr, bool fullChinese = true)
            {
                StringBuilder result = new StringBuilder();
                char c;
                if (fullChinese)
                {
                    int wanCount = intStr.Length / 4;
                    for (int ci, i = 0, step = intStr.Length % 4, sv = intStr.Length;
                        step <= sv; step += 4)
                    {
                        for (; i < step; i++)
                        {
                            c = intStr[i];
                            result.Append(GetChinese(c));
                            ci = i % 4;
                            switch (ci)
                            {
                                case 1: result.Append(ChineseFigure3); break;
                                case 2: result.Append(ChineseFigure2); break;
                                case 3: result.Append(ChineseFigure1); break;
                            }
                        }
                        switch (wanCount)
                        {
                            default: result.Append("[Err]"); break;
                            case 18: result.Append(ChineseFigure72); break;
                            case 17: result.Append(ChineseFigure68); break;
                            case 16: result.Append(ChineseFigure64); break;
                            case 15: result.Append(ChineseFigure60); break;
                            case 14: result.Append(ChineseFigure56); break;
                            case 13: result.Append(ChineseFigure52); break;
                            case 12: result.Append(ChineseFigure48); break;
                            case 11: result.Append(ChineseFigure44); break;
                            case 10: result.Append(ChineseFigure40); break;
                            case 9: result.Append(ChineseFigure36); break;
                            case 8: result.Append(ChineseFigure32); break;
                            case 7: result.Append(ChineseFigure28); break;
                            case 6: result.Append(ChineseFigure24); break;
                            case 5: result.Append(ChineseFigure20); break;
                            case 4: result.Append(ChineseFigure16); break;
                            case 3: result.Append(ChineseFigure12); break;
                            case 2: result.Append(ChineseFigure8); break;
                            case 1: result.Append(ChineseFigure4); break;
                            case 0: break;
                        }
                        wanCount--;
                    }
                }
                else
                {
                    for (int i = 0, iv = intStr.Length; i < iv; i++)
                    {
                        c = intStr[i];
                        result.Append(GetChinese(c));
                    }
                }
                return result.ToString();
            }

            private static string GetChinese(char numberChar)
            {
                switch (numberChar)
                {
                    case '0': return "零";
                    case '1': return "壹";
                    case '2': return "贰";
                    case '3': return "叁";
                    case '4': return "肆";
                    case '5': return "伍";
                    case '6': return "陆";
                    case '7': return "柒";
                    case '8': return "捌";
                    case '9': return "玖";
                    default: return "[Err]";
                }
            }

            private static string ChineseDot = "点";
            //十、百、千、万、亿（10 8）、兆（10 12）、
            //京（10 16）、垓（10 20）、秭（10 24）、
            //穰（10 28）、沟（10 32）、涧（10 36）、
            //正（10 40）、载（10 44），
            //此后随着佛教的传入和文化交流，又增加了
            //极（10 48）、恒河沙（10 52）、阿僧只（10 56）、
            //那由他（10 60）、不可思议（10 64）、无量（10 68）、
            //大数（10 72）
            private static string ChineseFigure1 = "拾";
            private static string ChineseFigure2 = "百";
            private static string ChineseFigure3 = "千";
            private static string ChineseFigure4 = "万";
            private static string ChineseFigure8 = "亿";
            private static string ChineseFigure12 = "兆";
            private static string ChineseFigure16 = "京";
            private static string ChineseFigure20 = "垓";
            private static string ChineseFigure24 = "秭";
            private static string ChineseFigure28 = "穰";
            private static string ChineseFigure32 = "沟";
            private static string ChineseFigure36 = "涧";
            private static string ChineseFigure40 = "正";
            private static string ChineseFigure44 = "载";
            private static string ChineseFigure48 = "极";
            private static string ChineseFigure52 = "恒河沙";
            private static string ChineseFigure56 = "阿僧只";
            private static string ChineseFigure60 = "那由他";
            private static string ChineseFigure64 = "不可思议";
            private static string ChineseFigure68 = "无量";
            private static string ChineseFigure72 = "大数";

            /// <summary>
            /// 将汉字数字字符串转换为数字字符串；
            /// 例如输入"壹贰叁肆伍陆"或"壹万贰千叁百肆拾伍点陆"，返回为"12,345.6"
            /// * 因中文汉字数字位数可超过70位，故可能无法用decimal承载；
            /// </summary>
            /// <param name="chineseString"></param>
            /// <param name="resultString"></param>
            /// <returns></returns>
            public static bool CheckChineseString(string chineseString, out string resultString)
            {
                resultString = null;
                string csInt, csFlo;
                if (chineseString.Contains(ChineseDot))
                {
                    int dotIdx = chineseString.IndexOf(ChineseDot);
                    csInt = chineseString.Substring(0, dotIdx);
                    csFlo = chineseString.Substring(dotIdx + 1);
                }
                else
                {
                    csInt = chineseString;
                    csFlo = null;
                }

                bool haveDS = false;
                #region 检查是否带有位数
                if (csInt.Contains(ChineseFigure1))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure2))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure3))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure4))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure8))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure12))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure16))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure20))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure24))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure28))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure32))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure36))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure40))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure44))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure48))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure52))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure56))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure60))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure64))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure68))
                    haveDS = true;
                if (csInt.Contains(ChineseFigure72))
                    haveDS = true;
                #endregion

                csInt = csInt.Replace(" ", "");
                if (haveDS)
                {
                    string intStr = "", intStr1;
                    int outNumber, digitLeng, digit;
                    while (csInt.Length > 0)
                    {
                        intStr1 = null;
                        if (CheckChineseString_headerIsNumber(csInt[0], out outNumber))
                        {
                            if (outNumber == 0)
                            {
                                csInt = csInt.Substring(1);
                                continue;
                            }
                            intStr1 = outNumber.ToString();
                        }
                        if (intStr1 == null)
                            return false;

                        if (CheckChineseString_headerIsDigitString(csInt, out digitLeng, out digit))
                        {
                            csInt = csInt.Substring(digitLeng);
                            if (!CheckChineseString_tryCombin(ref intStr, intStr1, digit))
                                return false;
                        }
                    }

                    StringBuilder floStr = new StringBuilder();
                    if (!string.IsNullOrWhiteSpace(csFlo))
                    {
                        floStr.Append(".");

                        csFlo = csFlo.Replace(" ", "");
                        while (csFlo.Length > 0)
                        {
                            if (CheckChineseString_headerIsNumber(csFlo[0], out outNumber))
                            {
                                floStr.Append(outNumber);
                                csFlo = csFlo.Substring(1);
                            }
                            else return false;
                        }
                    }
                    resultString = intStr + floStr.ToString();
                    return true;
                }
                else
                {
                    StringBuilder result = new StringBuilder();
                    int outNumber;
                    while (csInt.Length > 0)
                    {
                        if (CheckChineseString_headerIsNumber(csInt[0], out outNumber))
                        {
                            result.Append(outNumber);
                            csInt = csInt.Substring(1);
                        }
                        else return false;
                    }
                    if (!string.IsNullOrWhiteSpace(csFlo))
                    {
                        result.Append(".");

                        csFlo = csFlo.Replace(" ", "");
                        while (csFlo.Length > 0)
                        {
                            if (CheckChineseString_headerIsNumber(csFlo[0], out outNumber))
                            {
                                result.Append(outNumber);
                                csFlo = csFlo.Substring(1);
                            }
                            else return false;
                        }
                    }
                    resultString = result.ToString();
                    return true;
                }
            }
            private static bool CheckChineseString_headerIsNumber(char c, out int number)
            {
                number = 0;
                switch (c)
                {
                    case '零': number = 0; return true;
                    case '壹': number = 1; return true;
                    case '贰': number = 2; return true;
                    case '叁': number = 3; return true;
                    case '肆': number = 4; return true;
                    case '伍': number = 5; return true;
                    case '陆': number = 6; return true;
                    case '柒': number = 7; return true;
                    case '捌': number = 8; return true;
                    case '玖': number = 9; return true;
                }
                return false;
            }
            private static bool CheckChineseString_headerIsDigitString(string value, out int digitStrLen, out int digit)
            {
                digitStrLen = 0;
                digit = 0;
                if (value.StartsWith(ChineseFigure1))
                { digitStrLen = ChineseFigure1.Length; digit = 1; return true; }
                if (value.StartsWith(ChineseFigure2))
                { digitStrLen = ChineseFigure2.Length; digit = 2; return true; }
                if (value.StartsWith(ChineseFigure3))
                { digitStrLen = ChineseFigure3.Length; digit = 3; return true; }
                if (value.StartsWith(ChineseFigure4))
                { digitStrLen = ChineseFigure4.Length; digit = 4; return true; }
                if (value.StartsWith(ChineseFigure8))
                { digitStrLen = ChineseFigure8.Length; digit = 8; return true; }
                if (value.StartsWith(ChineseFigure12))
                { digitStrLen = ChineseFigure12.Length; digit = 12; return true; }
                if (value.StartsWith(ChineseFigure16))
                { digitStrLen = ChineseFigure16.Length; digit = 16; return true; }
                if (value.StartsWith(ChineseFigure20))
                { digitStrLen = ChineseFigure20.Length; digit = 20; return true; }
                if (value.StartsWith(ChineseFigure24))
                { digitStrLen = ChineseFigure24.Length; digit = 24; return true; }
                if (value.StartsWith(ChineseFigure28))
                { digitStrLen = ChineseFigure28.Length; digit = 28; return true; }
                if (value.StartsWith(ChineseFigure32))
                { digitStrLen = ChineseFigure32.Length; digit = 32; return true; }
                if (value.StartsWith(ChineseFigure36))
                { digitStrLen = ChineseFigure36.Length; digit = 36; return true; }
                if (value.StartsWith(ChineseFigure40))
                { digitStrLen = ChineseFigure40.Length; digit = 40; return true; }
                if (value.StartsWith(ChineseFigure44))
                { digitStrLen = ChineseFigure44.Length; digit = 44; return true; }
                if (value.StartsWith(ChineseFigure48))
                { digitStrLen = ChineseFigure48.Length; digit = 48; return true; }
                if (value.StartsWith(ChineseFigure52))
                { digitStrLen = ChineseFigure52.Length; digit = 52; return true; }
                if (value.StartsWith(ChineseFigure56))
                { digitStrLen = ChineseFigure56.Length; digit = 56; return true; }
                if (value.StartsWith(ChineseFigure60))
                { digitStrLen = ChineseFigure60.Length; digit = 60; return true; }
                if (value.StartsWith(ChineseFigure64))
                { digitStrLen = ChineseFigure64.Length; digit = 64; return true; }
                if (value.StartsWith(ChineseFigure68))
                { digitStrLen = ChineseFigure68.Length; digit = 68; return true; }
                if (value.StartsWith(ChineseFigure72))
                { digitStrLen = ChineseFigure72.Length; digit = 72; return true; }

                return false;
            }
            private static bool CheckChineseString_tryCombin(ref string baseStr, string subStr, int digit)
            {
                StringBuilder subStrFull = new StringBuilder();
                subStrFull.Append(subStr);
                for (int i = 0; i < digit; i++)
                    subStrFull.Append(0);
                subStrFull.ToString();

                int idxOff, j;
                char cB, cS;
                StringBuilder result = new StringBuilder();
                if (baseStr.Length > subStrFull.Length)
                {
                    idxOff = baseStr.Length - subStrFull.Length;
                    for (int i = baseStr.Length - 1; i >= 0; i--)
                    {
                        cB = baseStr[i];
                        j = i - idxOff;
                        if (j < 0)
                        {
                            result.Insert(0, cB);
                        }
                        else
                        {
                            cS = subStrFull[j];
                            if (cB != '0' && cS != '0')
                                return false;
                            if (cB == '0')
                                result.Insert(0, cS);
                            else
                                result.Insert(0, cB);
                        }
                    }
                }
                else
                {
                    idxOff = subStrFull.Length - baseStr.Length;
                    for (int i = subStrFull.Length - 1; i >= 0; i--)
                    {
                        cS = subStrFull[i];
                        j = i - idxOff;
                        if (j < 0)
                        {
                            result.Insert(0, cS);
                        }
                        else
                        {
                            cB = baseStr[j];
                            if (cB != '0' && cS != '0')
                                return false;
                            if (cB == '0')
                                result.Insert(0, cS);
                            else
                                result.Insert(0, cB);
                        }
                    }
                }
                baseStr = result.ToString();
                return true;
            }
        }



        public class CSVLikeRows
        {
            public static string RowToString(params string[] fields)
            {
                StringBuilder result = new StringBuilder();
                string raw;
                foreach (string fd in fields)
                {
                    if (fd.Contains("\""))
                        raw = fd.Replace("\"", "\"\"");
                    else
                        raw = fd;
                    if (raw.Contains("\r") || raw.Contains("\n")
                        || raw.Contains(",") || raw.Contains("\""))
                    {
                        result.Append("\"");
                        result.Append(raw);
                        result.Append("\"");
                    }
                    else
                    {
                        result.Append(raw);
                    }
                    result.Append(",");
                }
                if (result.Length > 0)
                    result.Remove(result.Length - 1, 1);
                return result.ToString();
            }
            public static string[] StringToRow(string rawRowString)
            {
                List<string> result = new List<string>();
                bool qtStart = false;
                StringBuilder field = new StringBuilder();
                char curC = '\0', nextC;
                bool isDoubleQt;
                for (int i = 0, iv = rawRowString.Length; i < iv; i++)
                {
                    curC = rawRowString[i];
                    if (qtStart)
                    {
                        if (curC == '\"')
                        {
                            isDoubleQt = false;
                            if (i < iv - 1)
                            {
                                nextC = rawRowString[i + 1];
                                if (nextC == '\"')
                                {
                                    isDoubleQt = true;
                                }
                            }
                            if (isDoubleQt)
                            {
                                field.Append("\"");
                            }
                            else
                            {
                                result.Add(field.ToString());
                                field.Clear();
                                qtStart = false;
                            }
                            i++;
                        }
                        else
                        {
                            field.Append(curC);
                        }
                    }
                    else  //  qtStart == false
                    {
                        if (curC == '\"')
                        {
                            qtStart = true;
                        }
                        else if (curC == ',')
                        {
                            result.Add(field.ToString());
                            field.Clear();
                        }
                        else
                        {
                            field.Append(curC);
                        }
                    }
                }
                if (field.Length > 0)
                    result.Add(field.ToString());
                if (curC == ',')
                    result.Add(field.ToString());
                return result.ToArray();
            }
        }

        public static class Chinese2PINYIN
        {


            /// <summary>
            /// 把汉字转换成拼音(全拼)
            /// 如，输入“你好”，得到"NiHao"
            /// </summary>
            /// <param name="hzString">汉字字符串</param>
            /// <returns>转换后的拼音(全拼)字符串</returns>
            public static string GetFullPINYIN(string hzString, bool withTone = false)
            {
                StringBuilder result = new StringBuilder();
                Microsoft.International.Converters.PinYinConverter.ChineseChar py;
                string p;
                foreach (char ch in hzString)
                {
                    if (Microsoft.International.Converters.PinYinConverter.ChineseChar.IsValidChar(ch))
                    {
                        py = new Microsoft.International.Converters.PinYinConverter.ChineseChar(ch);
                        if (py.Pinyins.Count > 0 && py.Pinyins[0].Length > 0)
                        {
                            p = py.Pinyins[0].ToString();
                            if (p.Length > 2)
                            {
                                if (withTone)
                                {
                                    result.Append(p.Substring(0, 1));
                                    result.Append(p.Substring(1, p.Length - 1).ToLower());
                                }
                                else
                                {
                                    result.Append(p.Substring(0, 1));
                                    result.Append(p.Substring(1, p.Length - 2).ToLower());
                                }
                            }
                            else
                            {
                                if (withTone)
                                {
                                    result.Append(p);
                                }
                                else
                                {
                                    result.Append(p.Substring(0, p.Length - 1));
                                }
                            }
                        }
                        else
                        {
                            result.Append(ch);
                        }
                    }
                    else
                    {
                        result.Append(ch);
                    }
                }


                return result.ToString();
            }

            /// <summary>
            /// 把汉字转换成简单拼音(拼音首字母)
            /// 如，输入“你好”，得到"NH"
            /// </summary>
            /// <param name="hzString"></param>
            /// <returns></returns>
            public static string GetSimplePINYIN(string hzString)
            {
                string result = GetFullPINYIN(hzString);
                return RemoveLowerPINYIN(result);
            }
            public static string RemoveLowerPINYIN(string fullPINYIN)
            {
                StringBuilder reBuild = new StringBuilder();
                char c;
                for (int i = 0, iv = fullPINYIN.Length; i < iv; i++)
                {
                    c = fullPINYIN[i];
                    if (c < 'a' || c > 'z')
                        reBuild.Append(c);
                }
                return reBuild.ToString();
            }

        }


    }
}
