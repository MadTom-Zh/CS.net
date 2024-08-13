using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    public class ClassJigsaw
    {
        private ClassSoEMatrix motherPanel;
        private List<ClassSoEMatrix> items;
        public ClassJigsaw(ClassSoEMatrix motherPanel, List<ClassSoEMatrix> items)
        {
            this.motherPanel = motherPanel;
            this.items = items;
        }

        public class PieceInfo
        {
            public ClassSoEMatrix item = null;
            public int index = -1;
            public int roClockwiseCount = 0;
            public ClassSoEMatrix.Point startPoint;
        }

        public List<PieceInfo> ResultInfoList = new List<PieceInfo>();
        public bool Start()
        {
            ClassSoEMatrix shadowPanel = motherPanel.Clone();
            List<PieceInfo> shadowList = CopyList(items);
            List<PieceInfo> resultShadow = new List<PieceInfo>();
            ResultInfoList.Clear();
            return NextTry(ref shadowPanel, ref shadowList, ref resultShadow);
        }
        private bool NextTry(ref ClassSoEMatrix curPanel, ref List<PieceInfo> spareItems, ref List<PieceInfo> resultShadow)
        {
            List<PieceInfo> shadowList;
            List<PieceInfo> tmpRS;
            ClassSoEMatrix shadowPanel;
            for (int tryCount = 0; tryCount < spareItems.Count; tryCount++)
            {
                // every item in the list should have a try
                shadowList = CopyList(spareItems);
                PieceInfo curPiece = shadowList[tryCount];
                //PieceInfo shadowItem;
                shadowList.RemoveAt(tryCount);

                for (int i = 0; i < 4; i++)
                {
                    // every item can be rotated(3 times max)
                    if (i > 0)
                    {
                        curPiece.item.RoClockwise();
                        curPiece.roClockwiseCount++;
                        curPiece.roClockwiseCount %= 4;
                    }

                    shadowPanel = curPanel.Clone();
                    if (TryPlaceItem(shadowPanel, curPiece) == true)
                    {
                        //curPanel = shadowPanel;
                        tmpRS = CopyList(resultShadow);
                        tmpRS.Add(curPiece);
                        if (shadowList.Count == 0)
                        {
                            ResultInfoList = tmpRS;
                            return true;
                        }
                        else
                        {
                            // if this one can be placed, try next
                            if (NextTry(ref shadowPanel, ref shadowList, ref tmpRS)
                                == true)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        private bool TryPlaceItem(ClassSoEMatrix curPanel, PieceInfo item)
        {
            int sPointLimitH = curPanel.Width - item.item.Width + 1;
            int sPointLimitV = curPanel.Height - item.item.Height + 1;
            ClassSoEMatrix.Point testPoint;
            for (int x = 0; x < sPointLimitH; x++)
            {
                for (int y = 0; y < sPointLimitV; y++)
                {
                    testPoint = new ClassSoEMatrix.Point(x, y);
                    if (ClassSoEMatrix.CanPlace(curPanel, item.item, testPoint)
                        == true)
                    {
                        item.startPoint = testPoint;
                        ClassSoEMatrix.Place(curPanel, item.item, testPoint);
                        return true;
                    }
                }
            }
            return false;
        }

        private List<PieceInfo> CopyList(List<ClassSoEMatrix> source)
        {
            List<PieceInfo> result = new List<PieceInfo>();
            PieceInfo iItem;
            for (int i = 0; i < source.Count; i++)
            {
                iItem = new PieceInfo()
                {
                    item = source[i],
                    index = i,
                };
                result.Add(iItem);
            }
            return result;
        }
        private List<PieceInfo> CopyList(List<PieceInfo> source)
        {
            List<PieceInfo> result = new List<PieceInfo>();
            PieceInfo iItem;
            for (int i = 0; i < source.Count; i++)
            {
                //iItem = new PieceInfo();
                iItem = source[i];
                result.Add(iItem);
            }
            return result;
        }
    }
}
