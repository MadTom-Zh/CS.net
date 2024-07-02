using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using System.Runtime.InteropServices;

namespace MadTomDev.UI
{

    public class RichTextBoxScroller
    {
        /// <summary>
        /// 初始化 RichTextBox 自动滚动类型
        /// </summary>
        /// <param name="richTextBox">被控制 RichTextBox 控件</param>
        public RichTextBoxScroller(RichTextBox richTextBox)
        {
            this.richTextBox = richTextBox;
        }


        private RichTextBox richTextBox;

        public void FindReset(int start = 0, int maxStepForard = 10)
        {
            lastFindEnd = richTextBox.Document.ContentStart.GetPositionAtOffset(start);
            this.maxStepForard = maxStepForard;
        }
        private TextPointer lastFindEnd;
        private int maxStepForard = 10;
        public void FindNext(string wordOrSentence, out TextPointer start, out TextPointer end)
        {
            start = null;
            end = null;
            if (string.IsNullOrWhiteSpace(wordOrSentence))
                return;

            //if (wordOrSentence == "1")
            //    ;

            if (lastFindEnd == null)
            {
                lastFindEnd = richTextBox.Document.ContentStart;
            }
            char wordOne = wordOrSentence[0];
            int wordLength = wordOrSentence.Length;

            TextPointer tpStart = lastFindEnd;
            TextPointer tpEnd = tpStart.GetPositionAtOffset(wordLength);
            string testTx = new TextRange(tpStart, tpEnd).Text;
            int wordOneIdx = testTx.IndexOf(wordOne);

            int startOffset = 0;
            // 开头位置偏前，整体后移
            while (wordOneIdx > 0 || wordOneIdx < 0)
            {
                if (startOffset >= maxStepForard)
                {
                    lastFindEnd = tpStart.GetPositionAtOffset(wordLength);
                    return;
                }

                tpStart = lastFindEnd.GetPositionAtOffset(++startOffset);
                tpEnd = tpStart.GetPositionAtOffset(wordLength);
                testTx = new TextRange(tpStart, tpEnd).Text;
                wordOneIdx = testTx.IndexOf(wordOne);

                if (wordOneIdx == 0)
                    break;
            }

            // 继续前推，找到真开始边界
            while (true)
            {
                if (wordOrSentence.StartsWith(testTx) && testTx.Length > 0)
                {
                    tpStart = lastFindEnd.GetPositionAtOffset(++startOffset);
                    tpEnd = tpStart.GetPositionAtOffset(wordLength);
                    testTx = new TextRange(tpStart, tpEnd).Text;
                }
                else
                {
                    tpStart = lastFindEnd.GetPositionAtOffset(--startOffset);
                    tpEnd = tpStart.GetPositionAtOffset(wordLength);
                    testTx = new TextRange(tpStart, tpEnd).Text;
                    break;
                }
            }

            // 定位后边界
            int testLength = testTx.Length;
            int wordLengthPlus = wordLength;
            while (testLength < wordLength)
            {
                tpEnd = tpStart.GetPositionAtOffset(++wordLengthPlus);
                testTx = new TextRange(tpStart, tpEnd).Text;
                testLength = testTx.Length;

                if (testLength == wordLength)
                    break;
            }
            start = tpStart;
            end = tpEnd;
            lastFindEnd = tpEnd;
        }

        public void Set_Selection_AndScroll(int startIndex, int length)
        {
            Set_Selection(startIndex, length);
            ScrollTo_Selection();
        }
        public void Set_Selection_AndScroll(TextPointer startPt, TextPointer endPt)
        {
            Set_Selection(startPt, endPt);
            ScrollTo_Selection();
        }
        public void Set_Selection(int startIndex, int length)
        {
            TextPointer tpStart = richTextBox.Document.ContentStart;
            richTextBox.Selection.Select(
                tpStart.GetPositionAtOffset(startIndex),
                tpStart.GetPositionAtOffset(startIndex + length));
        }
        public void Set_Selection(TextPointer startPt, TextPointer endPt)
        {
            richTextBox.Selection.Select(startPt, endPt);
        }
        StringBuilder textBdr = new StringBuilder();
        public void ScrollTo_Selection()
        {
            Rect startRect = richTextBox.Selection.Start.GetCharacterRect(LogicalDirection.Forward);
            Rect endRect = richTextBox.Selection.End.GetCharacterRect(LogicalDirection.Forward);
            double tarPosi = 0;
            if (endRect.Y - startRect.Y > richTextBox.ActualHeight)
            {
                tarPosi = richTextBox.VerticalOffset + startRect.Y;
            }
            else
            {
                tarPosi = richTextBox.VerticalOffset + (startRect.Y + endRect.Y + endRect.Height - richTextBox.ActualHeight) / 2;
            }
            if (tarPosi < 0)
            {
                tarPosi = 0;
            }
            textBdr.Append(tarPosi + "\t" + startRect.Y + "\t" + endRect.Y + "\t" + endRect.Height + "\r\n");

            richTextBox.ScrollToVerticalOffset(tarPosi);
        }

    }
    public class TextBoxScroller
    {
    }
    public class WebBroswerScroller
    {
        //public WebBroswerScroller(
        //    ref WebBrowser webBrowser,
        //    bool isSmoothScroll,
        //    int smoothStrength,
        //    int scrollStrength)
        //{
        //    _webBrowser = webBrowser;
        //    _isSmoothScroll = isSmoothScroll;
        //    if (isSmoothScroll)
        //    {
        //        _timer = new Timer();
        //        _timer.Enabled = false;
        //        _timer.Tick += new EventHandler(_timer_Tick);
        //        if (scrollStrength < 0) scrollStrength = 0;
        //        else if (scrollStrength > 100) scrollStrength = 100;
        //        _timer.Interval = 1000 - (int)(9.9 * scrollStrength);

        //        if (smoothStrength < 0) smoothStrength = 0;
        //        else if (smoothStrength > 100) smoothStrength = 100;
        //        _smoothStrength = 101 - smoothStrength;
        //    }
        //}

        //private WebBrowser _webBrowser;
        //private bool _isSmoothScroll;
        //private int _smoothStrength;
        //private Timer _timer;

        //public void Set_Selection_AndScroll(int startIndex, int length)
        //{
        //    Set_Selection(startIndex, length);
        //    ScrollTo_Selection();
        //}
        //public string Get_SubText(int startIdx)
        //{
        //    return Get_SubText(startIdx, int.MaxValue);
        //}
        //public string Get_SubText(int startIdx, int length)
        //{
        //    if (_webBrowser.IsDisposed == true)
        //    {
        //        return null;
        //    }
        //    IHTMLDocument2 document = (IHTMLDocument2)_webBrowser.Document.DomDocument;
        //    IHTMLBodyElement body = (IHTMLBodyElement)document.body;

        //    IHTMLTxtRange textRange = (IHTMLTxtRange)body.createTextRange();

        //    //int allTextLength = _webBrowser.Document.Body.InnerText.Length;
        //    textRange.collapse(true);

        //    textRange.moveStart("character", startIdx);
        //    textRange.moveEnd("character", length);
        //    string result = textRange.text;
        //    if (result == null)
        //    {
        //        result = "";
        //    }
        //    //while (result.Length < orgLen)
        //    //{
        //    //    length++;
        //    //    Set_Selection(startIdx, length);
        //    //    result = Get_SelectionText(false);
        //    //}
        //    //while (result.Length > orgLen)
        //    //{
        //    //    length--;
        //    //    Set_Selection(startIdx, length);
        //    //    result = Get_SelectionText(false);
        //    //}
        //    return result;
        //}
        //public string Get_AllText()
        //{
        //    IHTMLDocument2 document = (IHTMLDocument2)_webBrowser.Document.DomDocument;
        //    IHTMLBodyElement body = (IHTMLBodyElement)document.body;
        //    IHTMLTxtRange textRange = (IHTMLTxtRange)body.createTextRange();
        //    return textRange.text;
        //    //return _webBrowser.Document.Body.OuterText;
        //}
        //public string Get_SelectionText(bool isAppendToEnd)
        //{
        //    string result = null;

        //    IHTMLDocument2 document = (IHTMLDocument2)_webBrowser.Document.DomDocument;

        //    if (document.selection.type.ToLower() != "none")
        //    {
        //        dynamic tmp = document.selection.createRange();
        //        IHTMLTxtRange textRange = tmp as IHTMLTxtRange;
        //        if (isAppendToEnd)
        //        {
        //            textRange.collapse(true);
        //            textRange.moveEnd("character", document.body.innerText.Length);
        //        }
        //        result = textRange.text;
        //    }
        //    return result;
        //}
        //public string Get_SelectionText_beforeStartPoint()
        //{
        //    string result = null;
        //    IHTMLDocument2 document = (IHTMLDocument2)_webBrowser.Document.DomDocument;

        //    if (document.selection.type.ToLower() != "none")
        //    {
        //        dynamic tmp = document.selection.createRange();
        //        IHTMLTxtRange textRange = tmp as IHTMLTxtRange;
        //        textRange.collapse(true);
        //        textRange.moveStart("character", -document.body.innerText.Length);

        //        result = textRange.text;
        //    }
        //    return result;
        //}
        //public void Set_Selection(int startIndex, int length)
        //{
        //    IHTMLDocument2 document = (IHTMLDocument2)_webBrowser.Document.DomDocument;
        //    IHTMLBodyElement body = (IHTMLBodyElement)document.body;

        //    IHTMLTxtRange textRange = (IHTMLTxtRange)body.createTextRange();

        //    int allTextLength = _webBrowser.Document.Body.InnerText.Length;
        //    textRange.collapse(true);

        //    //textRange.moveEnd("character", startIndex + length - _webBrowser.Document.Body.InnerText.Length);
        //    textRange.moveStart("character", startIndex);
        //    //textRange.move("character", _webBrowser.Document.Body.InnerText.Substring(startIndex, length));
        //    textRange.moveEnd("character", length);
        //    //selectedRange.moveToPoint(20,20);
        //    textRange.select();
        //}
        //public void ScrollTo_Selection()
        //{
        //    if (_isSmoothScroll)
        //    {
        //        _timer.Enabled = true;
        //    }
        //    else
        //    {
        //        _timer_Tick(null, new EventArgs());
        //    }
        //}
        //public void StopScroll()
        //{
        //    _timer.Enabled = false;
        //}

        //private int _tmpCurPos = -1;
        //private void _timer_Tick(object sender, EventArgs e)
        //{
        //    IHTMLDocument2 document = (IHTMLDocument2)_webBrowser.Document.DomDocument;
        //    IHTMLTxtRange range = (document.selection.createRange()) as IHTMLTxtRange;
        //    IHTMLTextRangeMetrics metrics = (IHTMLTextRangeMetrics)range;

        //    if (string.IsNullOrEmpty(range.text))
        //    {
        //        _timer.Enabled = false;
        //        return;
        //    }

        //    int curPos = _webBrowser.Document.Body.ScrollTop;
        //    int pos = _webBrowser.Document.Body.ScrollTop + metrics.offsetTop + metrics.boundingHeight / 2 - _webBrowser.Height / 2;
        //    if (_isSmoothScroll)
        //    {
        //        pos = timer_Tick_compNextPosi(curPos, pos);
        //    }
        //    _webBrowser.Document.Window.ScrollTo(0, pos);
        //    if (pos == curPos || _tmpCurPos == curPos)
        //    {
        //        _timer.Enabled = false;
        //    }
        //    _tmpCurPos = curPos;
        //}
        //private int timer_Tick_compNextPosi(int curPos, int tarPos)
        //{
        //    if (Math.Abs(curPos - tarPos) > _webBrowser.Height / 2)
        //    {
        //        return (tarPos + curPos) / 2;
        //    }
        //    else
        //    {
        //        if (Math.Abs(curPos - tarPos) < 3) return curPos;
        //        else return (tarPos > curPos) ? (curPos + _smoothStrength) : (curPos - _smoothStrength);
        //    }
        //}
    }
}
