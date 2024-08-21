using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;
using System.Collections;

namespace FlowerzHelper
{
    public class Looper
    {
        public class Item
        {
            public enum Type
            {
                stone,
                flowerSingle,
                flowerDouble,
                toolButterfly,
                toolShovel,
            }
            public Type myType = Type.flowerSingle;

            public Item(Type type)
            {
                myType = type;
            }

            public static Color COLOR_GRAY = Colors.Gray;

            public static Color COLOR_RED = Colors.Red;
            public static Color COLOR_MAGENTA = Colors.Magenta;
            public static Color COLOR_YELLOW = Colors.Yellow;
            public static Color COLOR_WHITE = Colors.White;
            public static Color COLOR_CYAN = Colors.Cyan;
            public static Color COLOR_BLUE = Colors.Blue;

            public static Color GetNxtColor(Color curClr)
            {
                if (curClr == COLOR_RED) return COLOR_MAGENTA;
                else if (curClr == COLOR_MAGENTA) return COLOR_YELLOW;
                else if (curClr == COLOR_YELLOW) return COLOR_WHITE;
                else if (curClr == COLOR_WHITE) return COLOR_CYAN;
                else if (curClr == COLOR_CYAN) return COLOR_BLUE;
                else if (curClr == COLOR_BLUE) return COLOR_RED;
                else return COLOR_RED;
            }
            public static Color GetPrvColor(Color curClr)
            {
                if (curClr == COLOR_BLUE) return COLOR_CYAN;
                else if (curClr == COLOR_CYAN) return COLOR_WHITE;
                else if (curClr == COLOR_WHITE) return COLOR_YELLOW;
                else if (curClr == COLOR_YELLOW) return COLOR_MAGENTA;
                else if (curClr == COLOR_MAGENTA) return COLOR_RED;
                else if (curClr == COLOR_RED) return COLOR_BLUE;
                else return COLOR_BLUE;
            }

            public Color flowerMainColor = COLOR_GRAY;
            public Color flowerSubColor = COLOR_GRAY;

            public Color butterflyColor = COLOR_GRAY;

            public Ctrls.SubBlock.FlowerBlock UIFlower;
            public Ctrls.SubBlock.ToolBlock UITool;

            public Item Clone()
            {
                Item result = new Item(this.myType);
                result.flowerMainColor = this.flowerMainColor;
                result.flowerSubColor = this.flowerSubColor;
                result.butterflyColor = this.butterflyColor;

                result.UIFlower = this.UIFlower;
                result.UITool = this.UITool;

                return result;
            }
            public Item Clone(bool removeUI)
            {
                Item result = Clone();
                if (removeUI == true)
                {
                    result.UIFlower = null;
                    result.UITool = null;
                }
                return result;
            }
        }

        public List<Item> conveyor = new List<Item>();
        private List<Item> CloneConveyor(List<Item> conveyor)
        {
            List<Item> result = new List<Item>();
            for (int i = 0; i < conveyor.Count; i++)
            {
                result.Add(conveyor[i].Clone());
            }
            return result;
        }

        public class IntSize
        {
            private int _width;
            public int Width
            {
                set
                {
                    if (value < 0) _width = 0;
                    else _width = value;
                }
                get
                {
                    return _width;
                }
            }
            private int _height;
            public int Height
            {
                set
                {
                    if (value < 0) _height = 0;
                    else _height = value;
                }
                get
                {
                    return _height;
                }
            }

            public IntSize()
            {
            }
            public IntSize(int width, int height)
            {
                this.Width = width;
                this.Height = height;
            }
        }
        public IntSize gardenSize
        {
            get
            {
                if (gardenItemMatrix == null) return new IntSize(0, 0);
                if (gardenItemMatrix[0] == null) return new IntSize(0, 0);
                return new IntSize(gardenItemMatrix[0].Length, gardenItemMatrix.Length);
            }
            set
            {
                Item[][] tmp = gardenItemMatrix;
                gardenItemMatrix = new Item[value.Height][];
                for (int i = gardenItemMatrix.Length - 1; i >= 0; i--)
                {
                    gardenItemMatrix[i] = new Item[value.Width];
                }

                if (tmp == null || tmp.Length == 0 || (value.Height == 0 || value.Width == 0))
                {
                    return;
                }
                for (int i = 0; i < tmp[0].Length && i < gardenItemMatrix[0].Length; i++)
                {
                    for (int j = 0; j < tmp.Length && j < gardenItemMatrix.Length; j++)
                    {
                        gardenItemMatrix[j][i] = tmp[j][i];
                    }
                }
            }
        }

        public Item[][] gardenItemMatrix;
        private int CountMatrixItems(Item[][] matrix, bool nullOnly, bool flowerOnly)
        {
            if (nullOnly == true && flowerOnly == true)
            {
                throw new Exception("Can't count null and flowers at the same time!");
            }
            int result = 0;
            Item[] row;
            Item item;
            for (int i = matrix.Length - 1; i >= 0; i--)
            {
                row = matrix[i];
                for (int j = row.Length - 1; j >= 0; j--)
                {
                    item = row[j];
                    if (nullOnly == true)
                    {
                        if (item == null)
                        {
                            result++;
                        }
                    }
                    else if (flowerOnly == true)
                    {
                        if (item != null &&
                            (item.myType == Item.Type.flowerSingle || item.myType == Item.Type.flowerDouble))
                        {
                            result++;
                        }
                    }
                }
            }
            return result;
        }
        private int CountMatrixItems(Item[][] matrix, Item.Type itemType)
        {
            int result = 0;
            Item[] row;
            Item item;
            for (int i = matrix.Length - 1; i >= 0; i--)
            {
                row = matrix[i];
                for (int j = row.Length - 1; j >= 0; j--)
                {
                    item = row[j];
                    if (item != null && item.myType == itemType)
                    {
                        result++;
                    }
                }
            }
            return result;
        }
        private int CountMatrixAllFlowerColors(Item[][] matrix)
        {
            int result = 0;
            Item[] row;
            Item item;
            for (int i = matrix.Length - 1; i >= 0; i--)
            {
                row = matrix[i];
                for (int j = row.Length - 1; j >= 0; j--)
                {
                    item = row[j];
                    if (item != null)
                    {
                        if (item.myType == Item.Type.flowerSingle)
                            result++;
                        else if (item.myType == Item.Type.flowerDouble)
                            result += 2;
                    }
                }
            }
            return result;
        }
        public static Item[][] CloneMatrix(Item[][] matrix)
        {
            Item[][] result = new Item[matrix.Length][];
            for (int i = matrix.Length - 1; i >= 0; i--)
            {
                result[i] = new Item[matrix[0].Length];
            }

            Item item;
            for (int i = result.Length - 1; i >= 0; i--)
            {
                for (int j = result[i].Length - 1; j >= 0; j--)
                {
                    item = matrix[i][j];
                    if (item != null)
                    {
                        result[i][j] = item.Clone(true);
                    }
                }
            }
            return result;
        }

        public void SetGarden(int x, int y, Item item)
        {
            //if ((y < 0 || y >= gardenItemMatrix.Length)
            //    || (x < 0 || x >= gardenItemMatrix[0].Length))
            //{
            //    throw new Exception();
            //}
            gardenItemMatrix[y][x] = item;
        }

        public static int MaxResultCount = 3;
        public class IntPoint : IntSize
        {
            public int X
            {
                get
                {
                    return this.Width;
                }
                set
                {
                    this.Width = value;
                }
            }
            public int Y
            {
                get
                {
                    return this.Height;
                }
                set
                {
                    this.Height = value;
                }
            }

            public IntPoint()
            {
            }
            public IntPoint(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }
        public class Result
        {
            public IntPoint firstPosition;
            public Item[][] resultMap;
            public int flowerColorsLeft;
        }
        public List<Result> finalResult = new List<Result>();

        public List<int> loopingProgress = new List<int>();
        public event EventHandler LoopingPorgressStart;
        public event EventHandler LoopingPorgressChanged;
        public event EventHandler LoopingPorgressEnd;
        public event EventHandler LoopingPorgressAbort;
        public bool isLoopingAbort = false;
        private IntPoint firstStepPoint;

        public void LoopAbort()
        {
            isLoopingAbort = true;
        }
        public void Loop()
        {
            isLoopingAbort = false;
            if (LoopingPorgressStart != null)
            {
                LoopingPorgressStart(this, null);
            }
            finalResult.Clear();

            loopingProgress.Clear();
            int curProgress = gardenSize.Width * gardenSize.Height;
            int loopingProgressPosiIndex = loopingProgress.Count;
            loopingProgress.Add(curProgress);
            if (LoopingPorgressChanged != null)
            {
                LoopingPorgressChanged(this, null);
            }

            Item[] row;
            for (int i = gardenItemMatrix.Length - 1; i >= 0; i--)
            {
                row = gardenItemMatrix[i];
                for (int j = row.Length - 1; j >= 0; j--)
                {
                    if (isLoopingAbort == true)
                    {
                        finalResult.Clear();
                        if (LoopingPorgressAbort != null)
                        {
                            LoopingPorgressAbort(this, null);
                        }
                        return;
                    }
                    firstStepPoint = new IntPoint(j, i);
                    Loop_InStep(gardenItemMatrix, conveyor, j, i);


                    curProgress--;
                    if (curProgress == 0)
                    {
                        loopingProgress.RemoveAt(loopingProgressPosiIndex);
                    }
                    else
                    {
                        loopingProgress[loopingProgressPosiIndex]
                            = curProgress;
                    }
                    if (LoopingPorgressChanged != null)
                    {
                        LoopingPorgressChanged(this, null);
                    }
                }
            }

            if (finalResult.Count > 0)
            {
                ;
            }
            if (LoopingPorgressEnd != null)
            {
                LoopingPorgressEnd(this, null);
            }
        }
        private void Loop_InStep(Item[][] matrix, List<Item> conveyor, int x, int y)
        {
            if (isLoopingAbort == true)
            {
                finalResult.Clear();
                if (LoopingPorgressAbort != null)
                {
                    LoopingPorgressAbort(this, null);
                }
                return;
            }



            //if (conveyor.Count == 0)
            //{
            //    return;
            //}

            Item[][] matrixClone = CloneMatrix(matrix);
            List<Item> conveyorClone = CloneConveyor(conveyor);
            Item takenItem = conveyorClone[0];
            conveyorClone.RemoveAt(0);

            Item item = matrixClone[y][x];
            // check if can put, or change or dig
            if (item != null)
            {
                if (item.myType == Item.Type.stone)
                {
                    return;
                }
                else if (item.myType == Item.Type.flowerSingle || item.myType == Item.Type.flowerDouble)
                {
                    if (takenItem.myType == Item.Type.flowerSingle || takenItem.myType == Item.Type.flowerDouble)
                    {
                        return;
                    }
                }
            }

            if (takenItem.myType != Item.Type.toolShovel)
            {
                // put the item, flower or butterfly
                if (takenItem.myType == Item.Type.toolButterfly)
                {
                    if (item != null
                        && (item.myType == Item.Type.flowerSingle || item.myType == Item.Type.flowerDouble))
                    {
                        item.myType = Item.Type.flowerSingle;
                        item.flowerMainColor = takenItem.butterflyColor;
                        matrixClone[y][x] = item;
                    }
                }
                else if (item == null)
                {
                    matrixClone[y][x] = takenItem;
                }

                // check and shape matrix
                Loop_CheckAndShapeMatrix(ref matrixClone);
            }
            else
            {
                // shovel flower up
                if (item != null
                    && (item.myType == Item.Type.flowerSingle || item.myType == Item.Type.flowerDouble))
                {
                    conveyorClone.Insert(0, item.Clone());
                    matrixClone[y][x] = null;
                }
            }






            if (conveyorClone.Count == 0)
            {
                Result newResult = new Result();
                newResult.firstPosition = firstStepPoint;
                newResult.resultMap = matrixClone;

                newResult.flowerColorsLeft = CountMatrixAllFlowerColors(newResult.resultMap);

                // add new result or not
                if (finalResult.Count < MaxResultCount)
                {
                    finalResult.Add(newResult);
                }
                else
                {
                    int curValue;
                    int minValue = 999;
                    int minValueIdx = 0;
                    int maxValue = 0;
                    int maxValueIdx = 0;
                    for (int i = finalResult.Count - 1; i >= 0; i--)
                    {
                        curValue = finalResult[i].flowerColorsLeft;
                        if (maxValue < curValue)
                        {
                            maxValue = curValue;
                            maxValueIdx = i;
                        }
                    }
                    for (int i = 0; i < finalResult.Count; i++)
                    {
                        curValue = finalResult[i].flowerColorsLeft;
                        if (minValue > curValue)
                        {
                            minValue = curValue;
                            minValueIdx = i;
                        }
                    }
                    if (newResult.flowerColorsLeft < maxValue)
                    {
                        finalResult.RemoveAt(maxValueIdx);
                        finalResult.Add(newResult);
                    }
                    else if (newResult.flowerColorsLeft == minValue)
                    {
                        finalResult.RemoveAt(minValueIdx);
                        finalResult.Add(newResult);
                    }
                }

                // sort, from min to max
                Result tmpResult;
                for (int i = finalResult.Count - 1; i > 0; i--)
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (finalResult[j].flowerColorsLeft > finalResult[i].flowerColorsLeft)
                        {
                            tmpResult = finalResult[j];
                            finalResult[j] = finalResult[i];
                            finalResult[i] = tmpResult;
                        }
                    }
                }
                return;
            }


            // start next loop
            int curProgress = gardenSize.Width * gardenSize.Height;
            int loopingProgressPosiIndex = loopingProgress.Count;
            loopingProgress.Add(curProgress);
            if (LoopingPorgressChanged != null)
            {
                LoopingPorgressChanged(this, null);
            }

            Item[] row;
            for (int i = matrixClone.Length - 1; i >= 0; i--)
            {
                row = matrixClone[i];
                for (int j = row.Length - 1; j >= 0; j--)
                {
                    Loop_InStep(matrixClone, conveyorClone, j, i);


                    curProgress--;
                    if (curProgress == 0)
                    {
                        loopingProgress.RemoveAt(loopingProgressPosiIndex);
                    }
                    else
                    {
                        loopingProgress[loopingProgressPosiIndex]
                            = curProgress;
                    }
                    if (LoopingPorgressChanged != null)
                    {
                        LoopingPorgressChanged(this, null);
                    }
                }
            }
        }
        #region Loop_CheckAndShapeMatrix, shrink flowers
        public bool Loop_CheckAndShapeMatrix()
        {
            return Loop_CheckAndShapeMatrix(ref gardenItemMatrix);
        }
        public bool Loop_CheckAndShapeMatrix(ref Item[][] matrix)
        {
            bool isShaped = false;

            int rows = matrix.Length;
            int cols = matrix[0].Length;
            Item item, itemNext;

            int i, j;
            int shiftLeft, shiftRight, shiftTop, shiftBottom;
            int intTmp1;
            List<IntPoint> pointChain = new List<IntPoint>();

            for (j = rows - 1; j >= 0; j--)
            {
                for (i = cols - 1; i >= 0; i--)
                {
                    pointChain.Clear();
                    item = matrix[j][i];

                    if (item != null
                        && (item.myType == Item.Type.flowerSingle || item.myType == Item.Type.flowerDouble))
                    {
                        #region check to left
                        shiftLeft = 0;
                        for (int k = i - 1; k >= 0; k--)
                        {
                            itemNext = matrix[j][k];
                            if (itemNext != null
                                && (itemNext.myType == Item.Type.flowerSingle || itemNext.myType == Item.Type.flowerDouble))
                            {
                                if (itemNext.flowerMainColor == item.flowerMainColor)
                                {
                                    shiftLeft = i - k;
                                }
                                else break;
                            }
                            else break;
                        }
                        #endregion

                        #region check to right
                        shiftRight = 0;
                        for (int k = i + 1; k < matrix[j].Length; k++)
                        {
                            itemNext = matrix[j][k];
                            if (itemNext != null
                                && (itemNext.myType == Item.Type.flowerSingle || itemNext.myType == Item.Type.flowerDouble))
                            {
                                if (itemNext.flowerMainColor == item.flowerMainColor)
                                {
                                    shiftRight = k - i;
                                }
                                else break;
                            }
                            else break;
                        }
                        #endregion


                        intTmp1 = shiftRight + shiftLeft;
                        if (intTmp1 > 1)
                        {
                            for (int k = i - shiftLeft; k <= i + shiftRight; k++)
                            {
                                pointChain.Add(new IntPoint(k, j));
                            }
                            Loop_CheckAndShapeMatrix_extendPoints_V(ref matrix, ref pointChain);
                        }
                        else
                        {
                            #region check to top
                            shiftTop = 0;
                            for (int k = j - 1; k >= 0; k--)
                            {
                                itemNext = matrix[k][i];
                                if (itemNext != null
                                    && (itemNext.myType == Item.Type.flowerSingle || itemNext.myType == Item.Type.flowerDouble))
                                {
                                    if (itemNext.flowerMainColor == item.flowerMainColor)
                                    {
                                        shiftTop = j - k;
                                    }
                                    else break;
                                }
                                else break;
                            }
                            #endregion

                            #region check to bottom
                            shiftBottom = 0;
                            for (int k = j + 1; k < matrix.Length; k++)
                            {
                                itemNext = matrix[k][i];
                                if (itemNext != null
                                    && (itemNext.myType == Item.Type.flowerSingle || itemNext.myType == Item.Type.flowerDouble))
                                {
                                    if (itemNext.flowerMainColor == item.flowerMainColor)
                                    {
                                        shiftBottom = k - j;
                                    }
                                    else break;
                                }
                                else break;
                            }
                            #endregion

                            intTmp1 = shiftTop + shiftBottom;
                            if (intTmp1 > 1)
                            {

                                for (int k = j - shiftTop; k <= j + shiftBottom; k++)
                                {
                                    pointChain.Add(new IntPoint(i, k));
                                }
                                Loop_CheckAndShapeMatrix_extendPoints_H(ref matrix, ref pointChain);
                            }
                        }
                        if (pointChain.Count > 0)
                        {
                            Loop_shrinkFlowers(ref matrix, pointChain);
                            isShaped = true;
                        }
                    }
                }
            }

            if (isShaped == true)
            {
                Loop_CheckAndShapeMatrix(ref matrix);
                return true;
            }
            return false;
        }

        private void Loop_CheckAndShapeMatrix_extendPoints_H(ref Item[][] matrix, ref List<IntPoint> pointChain)
        {
            IntPoint curPoint;
            Item item;
            Item itemNext;
            int shiftLeft, shiftRight;
            int intTmp;

            for (int k = pointChain.Count - 1; k >= 0; k--)
            {
                curPoint = pointChain[k];
                item = matrix[curPoint.Y][curPoint.X];

                #region check to left
                shiftLeft = 0;
                for (int j = curPoint.X - 1; j >= 0; j--)
                {
                    itemNext = matrix[curPoint.Y][j];
                    if (itemNext != null
                        && (itemNext.myType == Item.Type.flowerSingle || itemNext.myType == Item.Type.flowerDouble))
                    {
                        if (itemNext.flowerMainColor == item.flowerMainColor)
                        {
                            shiftLeft = curPoint.X - j;
                        }
                        else break;
                    }
                    else break;
                }
                #endregion

                #region check to right
                shiftRight = 0;
                for (int j = curPoint.X + 1; j < matrix[0].Length; j++)
                {
                    itemNext = matrix[curPoint.Y][j];
                    if (itemNext != null
                        && (itemNext.myType == Item.Type.flowerSingle || itemNext.myType == Item.Type.flowerDouble))
                    {
                        if (itemNext.flowerMainColor == item.flowerMainColor)
                        {
                            shiftRight = j - curPoint.X;
                        }
                        else break;
                    }
                    else break;
                }
                #endregion

                intTmp = shiftLeft + shiftRight;
                if (intTmp > 1)
                {
                    for (int j = curPoint.X - shiftLeft; j <= curPoint.X + shiftRight; j++)
                    {
                        if (curPoint.X != j)
                        {
                            pointChain.Add(new IntPoint(j, curPoint.Y));
                        }
                    }
                }
            }












        }
        private void Loop_CheckAndShapeMatrix_extendPoints_V(ref Item[][] matrix, ref List<IntPoint> pointChain)
        {
            IntPoint curPoint;
            Item item;
            Item itemNext;
            int shiftTop, shiftBottom;
            int intTmp;

            for (int k = pointChain.Count - 1; k >= 0; k--)
            {
                curPoint = pointChain[k];
                item = matrix[curPoint.Y][curPoint.X];

                #region check to top
                shiftTop = 0;
                for (int i = curPoint.Y - 1; i >= 0; i--)
                {
                    itemNext = matrix[i][curPoint.X];
                    if (itemNext != null
                        && (itemNext.myType == Item.Type.flowerSingle || itemNext.myType == Item.Type.flowerDouble))
                    {
                        if (itemNext.flowerMainColor == item.flowerMainColor)
                        {
                            shiftTop = curPoint.Y - i;
                        }
                        else break;
                    }
                    else break;
                }
                #endregion

                #region check to bottom
                shiftBottom = 0;
                for (int i = curPoint.Y + 1; i < matrix.Length; i++)
                {
                    itemNext = matrix[i][curPoint.X];
                    if (itemNext != null
                        && (itemNext.myType == Item.Type.flowerSingle || itemNext.myType == Item.Type.flowerDouble))
                    {
                        if (itemNext.flowerMainColor == item.flowerMainColor)
                        {
                            shiftBottom = i - curPoint.Y;
                        }
                        else break;
                    }
                    else break;
                }
                #endregion

                intTmp = shiftTop + shiftBottom;
                if (intTmp > 1)
                {
                    for (int i = curPoint.Y - shiftTop; i <= curPoint.Y + shiftBottom; i++)
                    {
                        if (curPoint.Y != i)
                        {
                            pointChain.Add(new IntPoint(curPoint.X, i));
                        }
                    }
                }
            }
        }
        private void Loop_shrinkFlowers(ref Item[][] matrix, List<IntPoint> pointChain)
        {
            IntPoint curPoint;
            Item item;
            for (int i = pointChain.Count - 1; i >= 0; i--)
            {
                curPoint = pointChain[i];
                item = matrix[curPoint.Y][curPoint.X];
                matrix[curPoint.Y][curPoint.X] = Loop_shrinkFlowers_reduce(item);
            }
        }
        private Item Loop_shrinkFlowers_reduce(Item item)
        {
            if (item.myType == Item.Type.flowerSingle)
            {
                item = null;
            }
            else if (item.myType == Item.Type.flowerDouble)
            {
                item.myType = Item.Type.flowerSingle;
                item.flowerMainColor = item.flowerSubColor;
            }
            return item;
        }
        //private void Loop_shrinkFlowers_Top2Buttom(ref Item[][] matrix, IntPoint startPoint, int count)
        //{
        //    Item item;
        //    int rowIdx = startPoint.Y;
        //    int colIdx = startPoint.X;
        //    for (; count > 0; count--)
        //    {
        //        item = matrix[rowIdx][colIdx];

        //        if (item.myType == Item.Type.flowerSingle)
        //        {
        //            matrix[rowIdx][colIdx] = null;
        //        }
        //        else if (item.myType == Item.Type.flowerDouble)
        //        {
        //            item.flowerMainColor = item.flowerSubColor;
        //            item.myType = Item.Type.flowerSingle;
        //            matrix[rowIdx][colIdx] = item;
        //        }

        //        rowIdx++;
        //    }
        //}
        //private void Loop_shrinkFlowers_Left2Right(ref Item[][] matrix, IntPoint startPoint, int count)
        //{
        //    Item item;
        //    int rowIdx = startPoint.Y;
        //    int colIdx = startPoint.X;
        //    for (; count > 0; count--)
        //    {
        //        item = matrix[rowIdx][colIdx];

        //        if (item.myType == Item.Type.flowerSingle)
        //        {
        //            matrix[rowIdx][colIdx] = null;
        //        }
        //        else if (item.myType == Item.Type.flowerDouble)
        //        {
        //            item.flowerMainColor = item.flowerSubColor;
        //            item.myType = Item.Type.flowerSingle;
        //            matrix[rowIdx][colIdx] = item;
        //        }

        //        colIdx++;
        //    }
        //}
        #endregion
    }
}
