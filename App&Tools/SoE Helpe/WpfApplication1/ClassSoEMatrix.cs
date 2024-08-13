using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace WpfApplication1
{
    public class ClassSoEMatrix
    {
        // 0 empty, 1 block
        public int[][] Data;
        public ClassSoEMatrix(int cols, int rows)
        {
            ReInitData(cols, rows);
        }
        public int Width
        {
            get
            {
                return Data[0].Length;
            }
        }
        public int Height
        {
            get
            {
                return Data.Length;
            }
        }
        public int Blocks
        {
            get
            {
                int result = 0;
                for (int i = Data.Length - 1; i >= 0; i--)
                {
                    for (int j = Data[0].Length - 1; j >= 0; j--)
                    {
                        if (Data[i][j] > 0) result++;
                    }
                }
                return result;
            }
        }

        public int[][] ReInitData(int cols, int rows)
        {
            Data = new int[rows][];
            for (int i = rows - 1; i >= 0; i--)
            {
                Data[i] = new int[cols];
                for (int j = cols - 1; j >= 0; j--)
                {
                    Data[i][j] = 0;
                }
            }
            return Data;
        }
        public ClassSoEMatrix Clone()
        {
            int rows = Data.Length;
            int cols = Data[0].Length;
            ClassSoEMatrix newOne = new ClassSoEMatrix(cols, rows);
            for (int i = rows - 1; i >= 0; i--)
            {
                for (int j = cols - 1; j >= 0; j--)
                {
                    newOne.Data[i][j] = Data[i][j];
                }
            }

            return newOne;
        }

        public event EventHandler MxTransposed;
        public int[][] Transpose()
        {
            int tCols = Data.Length;
            int tRows = Data[0].Length;
            ClassSoEMatrix tmp = new ClassSoEMatrix(tCols, tRows);
            for (int i = tRows; i >= 0; i--)
            {
                for (int j = tCols; j >= 0; j--)
                {
                    tmp.Data[i][j] = Data[j][i];
                }
            }
            Data = tmp.Data;
            if (MxTransposed != null)
            {
                MxTransposed(this, new EventArgs());
            }
            return Data;
        }
        public int[][] RoClockwise()
        {
            int tCols = Data.Length;
            int tRows = Data[0].Length;
            ClassSoEMatrix tmp = new ClassSoEMatrix(tCols, tRows);
            for (int i = tRows - 1; i >= 0; i--)
            {
                for (int j = tCols - 1; j >= 0; j--)
                {
                    tmp.Data[i][j] = Data[tCols - 1 - j][i];
                }
            }
            Data = tmp.Data;
            return Data;
        }
        public int[][] RoCounterClockwise()
        {
            int tCols = Data.Length;
            int tRows = Data[0].Length;
            ClassSoEMatrix tmp = new ClassSoEMatrix(tCols, tRows);
            for (int i = tRows; i >= 0; i--)
            {
                for (int j = tCols; j >= 0; j--)
                {
                    tmp.Data[i][j] = Data[j][tRows - 1 - i];
                }
            }
            Data = tmp.Data;
            return Data;
        }

        public class Point
        {
            public int X;
            public int Y;
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
        public static bool CanPlace(ClassSoEMatrix panelMx, ClassSoEMatrix blockItem, Point startPoint)
        {
            int panelHight = panelMx.Data.Length;
            int panelWidth = panelMx.Data[0].Length;
            int itemHight = blockItem.Data.Length;
            int itemWidth = blockItem.Data[0].Length;

            if (startPoint.X < 0 || startPoint.Y < 0
                || itemHight + startPoint.Y > panelHight
                || itemWidth + startPoint.X > panelWidth)
            {
                return false;
            }

            int valuePanel, valueItem;
            for (int rowIdx = 0; rowIdx < itemHight; rowIdx++)
            {
                for (int colIdx = 0; colIdx < itemWidth; colIdx++)
                {
                    valuePanel = panelMx.Data[rowIdx + startPoint.Y][colIdx + startPoint.X];
                    valueItem = blockItem.Data[rowIdx][colIdx];
                    if (valuePanel > 0 && valueItem > 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        public static void Place(ClassSoEMatrix panelMx, ClassSoEMatrix blockItem, Point startPoint)
        {
            int itemHight = blockItem.Data.Length;
            int itemWidth = blockItem.Data[0].Length;

            for (int rowIdx =0; rowIdx < itemHight; rowIdx++)
            {
                for (int colIdx = 0; colIdx < itemWidth; colIdx++)
                {
                    panelMx.Data[rowIdx+ startPoint.Y][colIdx+startPoint.X]
                        += blockItem.Data[rowIdx][colIdx];
                }
            }
        }
    }
}
