using System;

using System.Collections.Generic;
using System.Text;

namespace MadTomDev.Data
{
    public class WPSWordHelper
    {
        Word.Application wordApp;
        object MissingValue = Type.Missing;
        public WPSWordHelper()
        {
            wordApp = new Word.Application();

        }

        public bool WindowVisible
        {
            get => wordApp.Visible;
            set => wordApp.Visible = value;
        }

        public Document NewDoc()
        {
            Word.Document doc = wordApp.Documents.Add(ref MissingValue, ref MissingValue, Word.WdDocumentType.wdTypeDocument, true);
            Document newDoc = new Document(this, doc);
            documents.Add(newDoc);
            return newDoc;
        }
        public Document OpenDoc(string docFile, bool isReadOnly = false)
        {
            object docFileObj = docFile;
            Word.Document doc = wordApp.Documents.Open(ref docFileObj, ref MissingValue, isReadOnly);
            Document newDoc = new Document(this, doc);
            documents.Add(newDoc);
            return newDoc;
        }

        public List<Document> documents = new List<Document>();
        public class Document
        {
            public WPSWordHelper parent;
            public Word.Document doc;
            public Document(WPSWordHelper parent, Word.Document doc)
            {
                this.parent = parent;
                this.doc = doc;
            }
            #region active, print view, save, save as, close
            public void Activate()
            {
                doc.Activate();
            }
            public void PrintPreview()
            {
                doc.PrintPreview();
            }
            public void Save()
            {
                doc.Save();
            }
            public void SaveAs()
            {
                doc.SaveAs2();
            }
            public void Close()
            {
                doc.Close();
                parent.documents.Remove(this);
            }
            #endregion

            #region select by tag or title, select all
            public void SelectAll()
            {
                doc.SelectAllEditableRanges();
            }
            public void SelectByTag(string tag)
            {
                doc.SelectContentControlsByTag(tag);
            }
            public void SelectByTitle(string title)
            {
                doc.SelectContentControlsByTitle(title);
                //doc.Bookmarks[0].Range.Text
            }
            #endregion

            public void Close(bool saveChanges = true)
            {
                doc.Close(saveChanges);
                parent.documents.Remove(this);
            }
        }

        public void Quit(bool saveAllDocs = true)
        {
            if (saveAllDocs)
            {
                foreach (Document doc in documents)
                    doc.Save();
            }
            object bSave = false;
            wordApp.Quit(ref bSave, ref MissingValue, ref MissingValue);

            System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
            wordApp = null;
        }
    }
    public class WPSExcelHelper
    {
        Excel.Application excelApp;
        public static object MissingValue = Type.Missing;

        public WPSExcelHelper()
        {
            excelApp = new Excel.Application();
        }

        public Book NewBook()
        {
            Book book = new Book(
                this,
                excelApp.Workbooks.Add(MissingValue)
                );
            books.Add(book);
            return book;
        }
        public Book OpenBook(string excelFile, bool isReadOnly = false)
        {
            Book book = new Book(
                this,
                excelApp.Workbooks.Open(excelFile, MissingValue, isReadOnly)
                );
            books.Add(book);
            return book;
        }

        public List<Book> books = new List<Book>();
        public class Book
        {
            WPSExcelHelper parent;
            private Excel.Workbook book;
            public Book(WPSExcelHelper parent, Excel.Workbook book)
            {
                this.parent = parent;
                this.book = book;
                foreach (Excel.Worksheet sheet in book.Sheets)
                {
                    sheets.Add(new Sheet(this, sheet));
                }
            }
            public Sheet NewSheet(int insertAt)
            {
                Sheet newSheet;
                if (insertAt < sheets.Count)
                {
                    newSheet = new Sheet(
                        this,
                        book.Sheets.Add(sheets[insertAt].GetOriSheet())
                        );
                    sheets.Insert(insertAt, newSheet);
                }
                else
                {
                    newSheet = new Sheet(
                        this,
                        book.Sheets.Add(WPSExcelHelper.MissingValue, sheets[sheets.Count - 1].GetOriSheet())
                        );
                    sheets.Add(newSheet);
                }
                return newSheet;
            }
            public List<Sheet> sheets = new List<Sheet>();
            public Sheet this[int index]
            {
                get => sheets[index];
            }
            public class Sheet
            {
                public Book parent;
                private Excel.Worksheet sheet;
                public Excel.Worksheet GetOriSheet()
                {
                    return sheet;
                }
                public Sheet(Book parent, Excel.Worksheet sheet)
                {
                    this.parent = parent;
                    this.sheet = sheet;
                }
                public void Delete()
                {
                    sheet.Delete();
                    parent.sheets.Remove(this);
                }

                public class Position
                {
                    public int col;
                    public int row;
                }
                public class Boundry
                {
                    public Position topLeft;
                    public Position bottomRight;
                }

                public Boundry GetUsedRangeBoundry()
                {
                    return GetBoundry((string)sheet.UsedRange.Address);
                }
                public List<Boundry> GetUsedAreaLocations()
                {
                    List<Boundry> result = new List<Boundry>();

                    foreach (Excel.Range a in sheet.UsedRange.Areas)
                    {
                        result.Add(GetBoundry((string)a.Address));
                    }
                    return result;
                }
                public static Boundry GetBoundry(string address)
                {
                    string[] pStrs = address.Split(':');
                    return new Boundry()
                    {
                        topLeft = GetPosition(pStrs[0]),
                        bottomRight = GetPosition(pStrs[1]),
                    };
                }
                public static Position GetPosition(string addressPosi)
                {
                    addressPosi = addressPosi.Replace("$", "");
                    StringBuilder colTxBdr = new StringBuilder();
                    int i = 0;
                    char c;
                    for (int iv = addressPosi.Length; i < iv; i++)
                    {
                        c = addressPosi[i];
                        if (c < '0' || c > '9')
                            colTxBdr.Append(c);
                        else
                            break;
                    }
                    return new Position()
                    {
                        col = GetColIndex(colTxBdr.ToString()),
                        row = int.Parse(addressPosi.Substring(i)),
                    };
                }
                public static int GetColIndex(string colTx)
                {
                    colTx = colTx.ToLower();
                    int carry = 1, tmp;
                    int result = 0;
                    char c;
                    for (int i = colTx.Length - 1; i >= 0; i--)
                    {
                        c = colTx[i];
                        tmp = c - 'a' + 1;
                        if (tmp > 0)
                        {
                            result += tmp * carry;
                        }
                        if (carry == 1)
                            carry = 26;
                        else
                            carry *= 26;
                    }
                    return result - 1;
                }
                public static string GetColTx(int colIndex)
                {
                    StringBuilder txBdr = new StringBuilder();
                    colIndex++;
                    while (colIndex > 0)
                    {
                        txBdr.Insert(0, (char)('A' + (colIndex % 26 - 1)));
                        colIndex /= 26;
                    }
                    return txBdr.ToString();
                }

                public string this[int x, int y]
                {
                    set
                    {
                        string address = GetColTx(x) + y;
                        sheet.Range[address].Value2 = value;
                    }
                    get
                    {
                        string address = GetColTx(x) + y;
                        return (string)sheet.Range[address].Value2;
                    }
                }
                public List<string[]> GetBoundryValue(Boundry boundry)
                {
                    return GetBoundryValue(boundry.topLeft, boundry.bottomRight);
                }
                public List<string[]> GetBoundryValue(Position topLeft, Position bottomRight)
                {
                    List<string[]> result = new List<string[]>();
                    int i, c, r = topLeft.row, cols = bottomRight.col - topLeft.col + 1;
                    string[] rowS;
                    object test;
                    for (int rv = bottomRight.row, cv = bottomRight.col; r <= rv; r++)
                    {
                        rowS = new string[cols];
                        i = 0;
                        for (c = topLeft.col; c <= cv; c++, i++)
                        {
                            test = sheet.Range[GetColTx(c) + r].Value2;
                            if (test != null)
                                rowS[i] = test.ToString();                            
                        }
                        result.Add(rowS);
                    }
                    return result;
                }
            }
            public void Close(bool saveChanges = true)
            {
                book.Close(saveChanges);
                parent.books.Remove(this);
            }
        }

        public void Quit(bool saveAllDocs = true)
        {

        }
    }
}
