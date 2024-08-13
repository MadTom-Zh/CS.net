using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brush = System.Drawing.Brush;
using Color = System.Drawing.Color;
using LinearGradientBrush = System.Drawing.Drawing2D.LinearGradientBrush;
using Matrix = System.Drawing.Drawing2D.Matrix;
using Pen = System.Drawing.Pen;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

using FlowDirection = System.Windows.FlowDirection;
using System.Windows.Media.Media3D;
using Window = System.Windows.Window;
using System.Diagnostics;
using DataFormats = System.Windows.DataFormats;

namespace MadTomDev.UI
{
    public class QuickGraphics
    {
        public class Basics
        {
            public class PinedBitmap : IDisposable
            {
                public Bitmap bitmap;
                public PointF pinPoint;
                public void Dispose()
                {
                    bitmap?.Dispose();
                }
            }
            public class PinedBitmapSource
            {
                public BitmapSource bitmapSource;
                public System.Windows.Point pinPoint;
            }
        }

        #region private methords

        public static WriteableBitmap BitmapToBitmapSource(Bitmap bitmap, bool isDisposeBitmap = true)
        {
            if (bitmap != null)
            {
                WriteableBitmap wb;
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Seek(0, SeekOrigin.Begin);

                    BitmapDecoder bd = BitmapDecoder.Create(
                        ms,
                        BitmapCreateOptions.PreservePixelFormat,
                        BitmapCacheOption.OnLoad);

                    wb = new WriteableBitmap(bd.Frames.Single());
                    //wb.Freeze();
                }
                if (isDisposeBitmap)
                {
                    bitmap.Dispose();
                }
                return wb;
            }
            return null;
        }
        public static Bitmap BitmapSourceToBitmap(BitmapSource bitmapSource)
        {
            Bitmap bmp = new Bitmap(
                bitmapSource.PixelWidth,
                bitmapSource.PixelHeight,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(
                new Rectangle(Point.Empty, bmp.Size),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            bitmapSource.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }
        public static Point WinPointToPoint(System.Windows.Point winPoint, bool round = true)
        {
            if (round)
                return new Point((int)(winPoint.X + 0.5), (int)(winPoint.Y + 0.5));
            else
                return new Point((int)(winPoint.X), (int)(winPoint.Y));
        }
        public static PointF WinPointToPointF(System.Windows.Point winPoint)
        {
            return new PointF((float)winPoint.X, (float)winPoint.Y);
        }
        public static System.Windows.Point PointToWinPoint(Point point)
        {
            return new System.Windows.Point(point.X, point.Y);
        }
        public static System.Windows.Point[] PointsToWinPoints(Point[] points)
        {
            int iv = points.Length;
            System.Windows.Point[] result = new System.Windows.Point[iv];
            for (int i = 0; i < iv; i++)
            {
                result[i] = PointToWinPoint(points[i]);
            }
            return result;
        }
        public static System.Windows.Point PointFToWinPoint(PointF pointF)
        {
            return new System.Windows.Point(pointF.X, pointF.Y);
        }
        public static System.Windows.Point[] PointFsToWinPoints(PointF[] points)
        {
            int iv = points.Length;
            System.Windows.Point[] result = new System.Windows.Point[iv];
            for (int i = 0; i < iv; i++)
            {
                result[i] = PointFToWinPoint(points[i]);
            }
            return result;
        }
        public static Color WinColorToColor(System.Windows.Media.Color winColor)
        {
            return Color.FromArgb(winColor.A, winColor.R, winColor.G, winColor.B);
        }
        public static System.Windows.Media.Color ColorToWinColor(Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Rectangle Int32RectToRectangle(Int32Rect rect32)
        {
            return new Rectangle(rect32.X, rect32.Y, rect32.Width, rect32.Height);
        }
        public static RectangleF Int32RectToRectangleF(Int32Rect rect32)
        {
            return new RectangleF(rect32.X, rect32.Y, rect32.Width, rect32.Height);
        }
        public static Rectangle RectToRectangle(Rect rect32, bool round = true)
        {
            if (round)
                return new Rectangle((int)(rect32.X + 0.5), (int)(rect32.Y + 0.5),
                    (int)(rect32.Width + 0.5), (int)(rect32.Height + 0.5));
            else
                return new Rectangle((int)rect32.X, (int)rect32.Y, (int)rect32.Width, (int)rect32.Height);
        }
        public static RectangleF RectToRectangleF(System.Windows.Rect rect)
        {
            return new RectangleF((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
        }
        public static Int32Rect RectangleToInt32Rect(Rectangle rectangle)
        {
            return new Int32Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }
        public static Int32Rect RectangleFToInt32Rect(RectangleF rectangleF, bool round = true)
        {
            if (round)
                return new Int32Rect((int)(rectangleF.X + 0.5), (int)(rectangleF.Y + 0.5),
                    (int)(rectangleF.Width + 0.5), (int)(rectangleF.Height + 0.5));
            else
                return new Int32Rect((int)rectangleF.X, (int)rectangleF.Y,
                    (int)rectangleF.Width, (int)rectangleF.Height);
        }
        public static Rect RectangleToRect(Rectangle rectangle)
        {
            return new Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }
        public static Rect RectangleFToRect(RectangleF rectangleF)
        {
            return new Rect(rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
        }

        #endregion




        public class Image
        {
            #region generate rainbow
            /// <summary>
            /// 生成一个颜色线性渐变的条带
            /// </summary>
            /// <param name="posis">两个颜色的起点和终点</param>
            /// <param name="colors">两个颜色</param>
            /// <param name="width">条带宽度</param>
            /// <param name="height">条带高度</param>
            /// <param name="isVertical">是否为纵向</param>
            /// <returns></returns>
            public static BitmapSource GetSlideImage(float[] posis, System.Windows.Media.Color[] colors, int width = 64, int height = 256, bool isVertical = true)
            {
                Color[] oColors = new Color[colors.Length];
                for (int i = 0, iv = colors.Length; i < iv; i++)
                {
                    oColors[i] = WinColorToColor(colors[i]);
                }
                return BitmapToBitmapSource(
                    GetSlideImage_Bitmap(
                        posis,
                        oColors,
                        width,
                        height,
                        isVertical));
            }

            public static Bitmap GetSlideImage_Bitmap(float[] posis, Color[] colors, int width = 64, int height = 256, bool isVertical = true)
            {
                Bitmap result = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(result))
                {
                    Rectangle rect = new Rectangle(0, 0, width, height);

                    Point p0 = new Point(0, 0);
                    Point pZ;
                    if (isVertical)
                        pZ = new Point(0, height);
                    else
                        pZ = new Point(width, 0);

                    using (LinearGradientBrush stripBrush =
                        new LinearGradientBrush(p0, pZ, colors[0], colors[colors.Length - 1]))
                    {
                        ColorBlend cblend = new ColorBlend(colors.Length);
                        cblend.Colors = colors;
                        cblend.Positions = posis;
                        stripBrush.InterpolationColors = cblend;
                        g.FillRectangle(stripBrush, rect);
                    }
                }
                return result;
            }

            public static int[] values_1bit = new int[] { 0, 255 };
            public static int[] values_2bit = new int[] { 0, 85, 170, 255 };
            public static int[] values_3bit = new int[] { 0, 36, 73, 109, 146, 182, 219, 255 };
            public static int[] values_4bit = new int[] { 0, 17, 34, 51, 68, 85, 102, 119, 136, 153, 170, 187, 204, 221, 238, 255 };

            public static Bitmap GetColorStrip_12bit_Bitmap(bool isVertical = true, int colorBlockWidth = 5, int stripWidth = 20)
            {
                return _GetColorStrip(ref values_4bit, isVertical, ref colorBlockWidth, ref stripWidth);
            }
            public static Bitmap GetColorStrip_9bit_Bitmap(bool isVertical = true, int colorBlockWidth = 10, int stripWidth = 20)
            {
                return _GetColorStrip(ref values_3bit, isVertical, ref colorBlockWidth, ref stripWidth);
            }
            public static Bitmap GetColorStrip_6bit_Bitmap(bool isVertical = true, int colorBlockWidth = 100, int stripWidth = 20)
            {
                return _GetColorStrip(ref values_2bit, isVertical, ref colorBlockWidth, ref stripWidth);
            }
            public static Bitmap GetColorStrip_3bit_Bitmap(bool isVertical = true, int colorBlockWidth = 200, int stripWidth = 20)
            {
                return _GetColorStrip(ref values_1bit, isVertical, ref colorBlockWidth, ref stripWidth);
            }


            public static BitmapSource GetColorStrip_12bit(bool isVertical = true, int colorBlockWidth = 5, int stripWidth = 20)
            {
                return BitmapToBitmapSource(GetColorStrip_12bit_Bitmap(isVertical, colorBlockWidth, stripWidth));
            }
            public static BitmapSource GetColorStrip_9bit(bool isVertical = true, int colorBlockWidth = 10, int stripWidth = 20)
            {
                return BitmapToBitmapSource(GetColorStrip_9bit_Bitmap(isVertical, colorBlockWidth, stripWidth));
            }
            public static BitmapSource GetColorStrip_6bit(bool isVertical = true, int colorBlockWidth = 100, int stripWidth = 20)
            {
                return BitmapToBitmapSource(GetColorStrip_6bit_Bitmap(isVertical, colorBlockWidth, stripWidth));
            }
            public static BitmapSource GetColorStrip_3bit(bool isVertical = true, int colorBlockWidth = 200, int stripWidth = 20)
            {
                return BitmapToBitmapSource(GetColorStrip_3bit_Bitmap(isVertical, colorBlockWidth, stripWidth));
            }

            private static Bitmap _GetColorStrip(ref int[] valueArray, bool isVertical, ref int colorBlockWidth, ref int stripWidth)
            {
                Bitmap result;
                int colorCount = (valueArray.Length - 1) * 6;
                if (isVertical)
                    result = new Bitmap(stripWidth, colorBlockWidth * colorCount);
                else
                    result = new Bitmap(colorBlockWidth * colorCount, stripWidth);

                int curPosi = 0, addPosi = colorBlockWidth, sw = stripWidth;
                using (Graphics g = Graphics.FromImage(result))
                {
                    int i, iv = valueArray.Length - 1;
                    Color clr;
                    for (i = 0; i < iv; ++i) // 100 - 110
                    {
                        clr = Color.FromArgb(255, valueArray[i], 0);
                        FillNextBlock(g, ref clr);
                    }
                    for (i = iv; i > 0; --i) // 110 - 010
                    {
                        clr = Color.FromArgb(valueArray[i], 255, 0);
                        FillNextBlock(g, ref clr);
                    }
                    for (i = 0; i < iv; ++i) // 010 - 011
                    {
                        clr = Color.FromArgb(0, 255, valueArray[i]);
                        FillNextBlock(g, ref clr);
                    }
                    for (i = iv; i > 0; --i) // 011 - 001
                    {
                        clr = Color.FromArgb(0, valueArray[i], 255);
                        FillNextBlock(g, ref clr);
                    }
                    for (i = 0; i < iv; ++i) // 001 - 101
                    {
                        clr = Color.FromArgb(valueArray[i], 0, 255);
                        FillNextBlock(g, ref clr);
                    }
                    for (i = iv; i > 0; --i) // 101 - 100
                    {
                        clr = Color.FromArgb(255, 0, valueArray[i]);
                        FillNextBlock(g, ref clr);
                    }
                }
                return result;

                void FillNextBlock(Graphics g, ref Color clr)
                {
                    if (isVertical)
                    {
                        using (Brush b = new SolidBrush(clr))
                        {
                            g.FillRectangle(b, 0, curPosi, sw, addPosi);
                        }
                    }
                    else
                    {
                        using (Brush b = new SolidBrush(clr))
                        {
                            g.FillRectangle(b, curPosi, 0, addPosi, sw);
                        }
                    }
                    curPosi += addPosi;
                }
            }

            public static void FillRectangleGradient(ref WriteableBitmap bitmapSource, Int32Rect rect32, System.Windows.Media.Color[] clr_tl_tr_bl_br)
            {
                FillRectangleGradient(ref bitmapSource, rect32,
                    clr_tl_tr_bl_br[0], clr_tl_tr_bl_br[1], clr_tl_tr_bl_br[2], clr_tl_tr_bl_br[3]);
            }
            public static void FillRectangleGradient(ref WriteableBitmap bitmapSource, Int32Rect rect32,
                System.Windows.Media.Color colorTopLeft, System.Windows.Media.Color colorTopRight,
                System.Windows.Media.Color colorButtomLeft, System.Windows.Media.Color colorButtomRight)
            {
                Bitmap bitmap = BitmapSourceToBitmap(bitmapSource);
                Graphics g = Graphics.FromImage(bitmap);
                FillRectangleGradient_Bitmap(g, Int32RectToRectangle(rect32),
                    WinColorToColor(colorTopLeft), WinColorToColor(colorTopRight),
                    WinColorToColor(colorButtomLeft), WinColorToColor(colorButtomRight));

                g.Dispose();
                bitmapSource = BitmapToBitmapSource(bitmap);
            }
            public static void FillRectangleGradient_Bitmap(Graphics g, Rectangle rect, Color[] clr_tl_tr_bl_br)
            {
                FillRectangleGradient_Bitmap(g, rect, clr_tl_tr_bl_br[0], clr_tl_tr_bl_br[1], clr_tl_tr_bl_br[2], clr_tl_tr_bl_br[3]);
            }
            public static void FillRectangleGradient_Bitmap(Graphics g, Rectangle rect,
                Color colorTopLeft, Color colorTopRight, Color colorButtomLeft, Color colorButtomRight)
            {
                if (rect.Width >= rect.Height)
                {
                    // do hor brush, ver fill
                    // make 2 ver color list
                    Bitmap colorList1 = _MakeColorList(rect.Height, colorTopLeft, colorButtomLeft);
                    Bitmap colorList2 = _MakeColorList(rect.Height, colorTopRight, colorButtomRight);
                    Rectangle line;
                    for (int i = 0, iMax = colorList1.Width; i < iMax; i++)
                    {
                        line = new Rectangle(rect.X, rect.Y + i, rect.Width, 1);
                        using (LinearGradientBrush lgBrush
                            = new LinearGradientBrush(new Point(0, 0), new Point(rect.Width, 0),
                                colorList1.GetPixel(i, 0), colorList2.GetPixel(i, 0)))
                        {
                            g.FillRectangle(lgBrush, line);
                        }
                    }
                }
                else
                {
                    // ver brush, hor fill
                    Bitmap colorList1 = _MakeColorList(rect.Width, colorTopLeft, colorTopRight);
                    Bitmap colorList2 = _MakeColorList(rect.Width, colorButtomLeft, colorButtomRight);
                    Rectangle col;
                    for (int i = 0, iMax = colorList1.Width; i < iMax; i++)
                    {
                        col = new Rectangle(rect.X + i, rect.Y, 1, rect.Height);
                        using (LinearGradientBrush lgBrush
                            = new LinearGradientBrush(new Point(0, 0), new Point(0, rect.Height),
                                colorList1.GetPixel(i, 0), colorList2.GetPixel(i, 0)))
                        {
                            g.FillRectangle(lgBrush, col);
                        }
                    }
                }
            }


            private static Bitmap _MakeColorList(int length, Color c1, Color c2)
            {
                Bitmap result = new Bitmap(length, 1);
                using (Graphics g = Graphics.FromImage(result))
                {
                    Rectangle rect = new Rectangle(0, 0, length, 1);
                    using (LinearGradientBrush lgBrush
                        = new LinearGradientBrush(
                            new Point(0, 0), new Point(length, 0),
                            c1, c2))
                    {
                        g.FillRectangle(lgBrush, rect);
                    }
                }
                return result;
            }

            /// <summary>
            /// 生成横向的色谱，从左到右，波长逐渐变短，颜色从红到紫；
            /// 每个颜色为3位，每位8bit，颜色总长为1280;
            /// </summary>
            /// <returns></returns>
            public static BitmapSource MakeColourSpectrum_24bit()
            {
                return BitmapToBitmapSource(MakeColourSpectrum_24bit_Bitmap());
            }
            public static Bitmap MakeColourSpectrum_24bit_Bitmap()
            {
                Bitmap result = new Bitmap(1280, 1);
                int i, iv = 256;
                for (i = 0; i < iv; i++)
                    result.SetPixel(i, 0, Color.FromArgb(255, i, 0));
                for (i = 0; i < iv; i++)
                    result.SetPixel(256 + i, 0, Color.FromArgb(255 - i, 255, 0));
                for (i = 0; i < iv; i++)
                    result.SetPixel(512 + i, 0, Color.FromArgb(0, 255, i));
                for (i = 0; i < iv; i++)
                    result.SetPixel(768 + i, 0, Color.FromArgb(0, 255 - i, 255));
                for (i = 0; i < iv; i++)
                    result.SetPixel(1024 + i, 0, Color.FromArgb(i, 0, 255));
                return result;
            }

            /// <summary>
            /// get a color from 0-red to 1280-purple;
            /// if out of range, return black;
            /// </summary>
            /// <param name="v0to1279"></param>
            /// <returns></returns>
            public static System.Windows.Media.Color GetWinColorFromColourSpectrum_24bit(int v0to1279)
            {
                return ColorToWinColor(GetColorFromColourSpectrum_24bit(v0to1279));
            }
            public static Color GetColorFromColourSpectrum_24bit(int v0to1279)
            {
                if (v0to1279 < 0 || v0to1279 > 1279)
                    return Color.Black;

                int step = 0;
                while (v0to1279 > 255)
                {
                    v0to1279 -= 256;
                    step++;
                }
                switch (step)
                {
                    default:
                    case 0: return Color.FromArgb(255, 255, (byte)v0to1279, 0);
                    case 1: return Color.FromArgb(255, (byte)(255 - v0to1279), 255, 0);
                    case 2: return Color.FromArgb(255, 0, 255, (byte)v0to1279);
                    case 3: return Color.FromArgb(255, 0, (byte)(255 - v0to1279), 255);
                    case 4: return Color.FromArgb(255, (byte)v0to1279, 0, 255);
                }
            }

            #endregion

            #region Mosaic
            public static BitmapSource GetChessboardImage(int width, int height, int minBlocks = 2)
            {
                return BitmapToBitmapSource(GetChessboardImage_Bitmap(width, height, minBlocks));
            }
            public static Bitmap GetChessboardImage_Bitmap(int width, int height, int minBlocks = 2)
            {
                return GetChessboardImage_Bitmap(width, height, Color.Black, Color.White, minBlocks);
            }

            public static BitmapSource GetChessboardImage(int width, int height,
                System.Windows.Media.Color c1, System.Windows.Media.Color c2, int minBlocks)
            {
                return BitmapToBitmapSource(GetChessboardImage_Bitmap(
                    width, height,
                    WinColorToColor(c1), WinColorToColor(c2),
                    minBlocks
                    ));
            }
            public static Bitmap GetChessboardImage_Bitmap(int width, int height, Color c1, Color c2, int minBlocks)
            {
                Bitmap result = new Bitmap(width, height);
                int blockHight = (Math.Min(width, height) + 1) / minBlocks;

                using (Graphics g = Graphics.FromImage(result))
                {
                    using (SolidBrush sb = new SolidBrush(c2))
                    {
                        g.FillRectangle(sb, 0, 0, width, height);
                    }

                    List<Rectangle> blackBlocks = new List<Rectangle>();
                    Rectangle rect;
                    int vertiCount = (result.Width + blockHight - 1) / blockHight;
                    int horizCount = (result.Height + blockHight - 1) / blockHight;
                    for (int i = 0, j; i < vertiCount; i++)
                    {
                        for (j = 0; j < horizCount; j++)
                        {
                            if ((i + j) % 2 == 0)
                            {
                                rect = new Rectangle(i * blockHight, j * blockHight, blockHight, blockHight);
                                blackBlocks.Add(rect);
                            }
                        }
                    }
                    using (SolidBrush sb = new SolidBrush(c1))
                    {
                        g.FillRectangles(sb, blackBlocks.ToArray());
                    }
                }
                return result;
            }

            public static BitmapSource GetMosaic_Simple(BitmapSource oriImage, float aboutBlockWidth = 10, float aboutBlockHeight = 10)
            {
                return BitmapToBitmapSource(GetMosaic_Simple_Bitmap(
                    BitmapSourceToBitmap(oriImage), aboutBlockWidth, aboutBlockHeight
                    ));
            }
            public static Bitmap GetMosaic_Simple_Bitmap(Bitmap oriImage, float aboutBlockWidth = 10, float aboutBlockHeight = 10)
            {
                float tmp1, tmp2;
                return GetMosaic_Simple_Bitmap(oriImage, aboutBlockWidth, aboutBlockHeight, out tmp1, out tmp2);
            }

            public static BitmapSource GetMosaic_Simple(
                BitmapSource oriImage, float aboutBlockWidth, float aboutBlockHeight,
                out float newBlockWidth, out float newBlockHeight)
            {
                Bitmap bitmap = BitmapSourceToBitmap(oriImage);
                int horTilesCount = (int)Math.Round(bitmap.Width / aboutBlockWidth);
                int verTilesCount = (int)Math.Round(bitmap.Height / aboutBlockHeight);
                return BitmapToBitmapSource(GetMosaic_Simple_Bitmap(bitmap, horTilesCount, verTilesCount, out newBlockWidth, out newBlockHeight));
            }
            public static Bitmap GetMosaic_Simple_Bitmap(Bitmap oriImage, float aboutBlockWidth, float aboutBlockHeight, out float newBlockWidth, out float newBlockHeight)
            {
                int horTilesCount = (int)Math.Round(oriImage.Width / aboutBlockWidth);
                int verTilesCount = (int)Math.Round(oriImage.Height / aboutBlockHeight);

                return GetMosaic_Simple_Bitmap(oriImage, horTilesCount, verTilesCount, out newBlockWidth, out newBlockHeight);
            }

            public static BitmapSource GetMosaic_Simple_Bitmap(
                BitmapSource oriImage, int horTilesCount, int verTilesCount,
                out float blockWidth, out float blockHeight)
            {
                Bitmap bitmap = BitmapSourceToBitmap(oriImage);
                return BitmapToBitmapSource(GetMosaic_Simple_Bitmap(bitmap, horTilesCount, verTilesCount, out blockWidth, out blockHeight));
            }
            public static Bitmap GetMosaic_Simple_Bitmap(Bitmap oriImage, int horTilesCount, int verTilesCount, out float blockWidth, out float blockHeight)
            {
                blockWidth = (float)oriImage.Width / horTilesCount;
                blockHeight = (float)oriImage.Height / verTilesCount;

                Bitmap result = new Bitmap(oriImage.Width, oriImage.Height);
                Size smallSize = new Size(horTilesCount, verTilesCount);

                using (Bitmap smallImg = new Bitmap(oriImage, smallSize))
                using (Graphics g = Graphics.FromImage(result))
                using (SolidBrush sb = new SolidBrush(Color.Empty))
                {
                    for (int x, y = 0; y < smallImg.Height; y++)
                    {
                        for (x = 0; x < smallImg.Width; x++)
                        {
                            sb.Color = smallImg.GetPixel(x, y);
                            g.FillRectangle(sb, x * blockWidth, y * blockHeight, blockWidth, blockHeight);
                        }
                    }
                }

                return result;
            }



            public static BitmapSource GetMosaic_Precise_Bitmap(BitmapSource oriImage, float blockWidth = 10, float blockHeight = 10)
            {
                Bitmap bitmap = BitmapSourceToBitmap(oriImage);
                return BitmapToBitmapSource(GetMosaic_Precise_Bitmap(bitmap, blockWidth, blockHeight));
            }
            public static Bitmap GetMosaic_Precise_Bitmap(Bitmap oriImage, float blockWidth = 10, float blockHeight = 10)
            {
                Bitmap result = new Bitmap(oriImage.Width, oriImage.Height);
                int tileXV = (int)Math.Ceiling(oriImage.Width / blockWidth);
                int tileYV = (int)Math.Ceiling(oriImage.Height / blockHeight);
                using (oriImage = new Bitmap(oriImage))
                using (Graphics g = Graphics.FromImage(result))
                using (SolidBrush sb = new SolidBrush(Color.Empty))
                {
                    for (int tileX, tileY = 0; tileY < tileYV; tileY++)
                    {
                        for (tileX = 0; tileX < tileXV; tileX++)
                        {
                            sb.Color = _GetMosaic_Precise_GetTileColor(
                                oriImage,
                                tileX * blockWidth, tileY * blockHeight,
                                blockWidth, blockHeight);
                            g.FillRectangle(sb,
                                tileX * blockWidth, tileY * blockHeight,
                                blockWidth, blockHeight);
                        }
                    }
                }

                return result;
            }
            private static Color _GetMosaic_Precise_GetTileColor(Bitmap oriImage, float left, float top, float width, float height)
            {
                int xM = (int)Math.Floor(left);
                int yM = (int)Math.Floor(top);
                float leftPers = xM + 1 - left;
                float topPers = yM + 1 - top;
                float right = left + width;
                float buttom = top + height;
                int xV = (int)Math.Ceiling(right);
                int yV = (int)Math.Ceiling(buttom);
                float rightPers = 1 - xV + right;
                float buttomPers = 1 - yV + buttom;
                if (xV > oriImage.Width)
                {
                    xV = oriImage.Width;
                    rightPers = 1;
                }
                if (yV > oriImage.Height)
                {
                    yV = oriImage.Height;
                    buttomPers = 1;
                }
                int r = 0, g = 0, b = 0;
                Color pix;
                int xVs1 = xV - 1, yVs1 = yV - 1;
                for (int x, y = yM; y < yV; y++)
                {
                    for (x = xM; x < xV; x++)
                    {
                        pix = oriImage.GetPixel(x, y);
                        if (x == xM)
                        {
                            if (y == yM)
                            {
                                r += (int)(pix.R * leftPers * topPers);
                                g += (int)(pix.G * leftPers * topPers);
                                b += (int)(pix.B * leftPers * topPers);
                            }
                            else if (y == yVs1)
                            {
                                r += (int)(pix.R * leftPers * buttomPers);
                                g += (int)(pix.G * leftPers * buttomPers);
                                b += (int)(pix.B * leftPers * buttomPers);
                            }
                            else
                            {
                                r += (int)(pix.R * leftPers);
                                g += (int)(pix.G * leftPers);
                                b += (int)(pix.B * leftPers);
                            }
                        }
                        else if (x == xVs1)
                        {
                            if (y == yM)
                            {
                                r += (int)(pix.R * rightPers * topPers);
                                g += (int)(pix.G * rightPers * topPers);
                                b += (int)(pix.B * rightPers * topPers);
                            }
                            else if (y == yVs1)
                            {
                                r += (int)(pix.R * rightPers * buttomPers);
                                g += (int)(pix.G * rightPers * buttomPers);
                                b += (int)(pix.B * rightPers * buttomPers);
                            }
                            else
                            {
                                r += (int)(pix.R * rightPers);
                                g += (int)(pix.G * rightPers);
                                b += (int)(pix.B * rightPers);
                            }
                        }
                        else
                        {
                            if (y == yM)
                            {
                                r += (int)(pix.R * topPers);
                                g += (int)(pix.G * topPers);
                                b += (int)(pix.B * topPers);
                            }
                            else if (y == yVs1)
                            {
                                r += (int)(pix.R * buttomPers);
                                g += (int)(pix.G * buttomPers);
                                b += (int)(pix.B * buttomPers);
                            }
                            else
                            {
                                r += pix.R;
                                g += pix.G;
                                b += pix.B;
                            }
                        }
                    }
                }
                float area = (xV - 2 + rightPers - xM + leftPers) * (yV - 2 + buttomPers - yM + topPers);
                return Color.FromArgb(
                    (int)(r / area),
                    (int)(g / area),
                    (int)(b / area));
            }
            #endregion

        }

        public class Shape
        {
            #region draw frame

            public static void DrawFrame(ref WriteableBitmap bitmapSource,
                int left, int top, int width, int height,
                System.Windows.Media.Color lineColor, int lineThickness = 1)
            {
                Bitmap bitmap = BitmapSourceToBitmap(bitmapSource);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    DrawFrame_Bitmap(g, left, top, width, height, WinColorToColor(lineColor), lineThickness);
                }
                bitmapSource = BitmapToBitmapSource(bitmap);
            }
            public static void DrawFrame_Bitmap(Graphics g,
                int left, int top, int width, int height,
                Color lineColor, int lineThickness = 1)
            {
                using (SolidBrush sBr = new SolidBrush(lineColor))
                using (Pen pen = new Pen(sBr))
                {
                    Point ptTL = new Point(left, top);
                    Point ptTR = new Point(left + width, top);
                    Point ptBL = new Point(left, top + height);
                    Point ptBR = new Point(left + width, top + height);
                    g.DrawLine(pen, ptTL, ptTR);
                    g.DrawLine(pen, ptTL, ptBL);
                    g.DrawLine(pen, ptTR, ptBR);
                    g.DrawLine(pen, ptBL, ptBR);
                    g.DrawLine(pen, ptTL, ptBR);
                }
            }

            public static void DrawFrame(ref WriteableBitmap bitmapSource,
                double left, double top, double width, double height,
                System.Windows.Media.Color lineColor, double lineThickness = 1)
            {
                Bitmap bitmap = BitmapSourceToBitmap(bitmapSource);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    DrawFrame_Bitmap(g, (float)left, (float)top, (float)width, (float)height, WinColorToColor(lineColor), (float)lineThickness);
                }
                bitmapSource = BitmapToBitmapSource(bitmap);
            }

            public static void DrawFrame_Bitmap(Graphics g,
                float left, float top, float width, float height,
                Color lineColor, float lineThickness = 1)
            {
                using (SolidBrush sBr = new SolidBrush(lineColor))
                using (Pen pen = new Pen(sBr))
                {
                    PointF ptTL = new PointF(left, top);
                    PointF ptTR = new PointF(left + width, top);
                    PointF ptBL = new PointF(left, top + height);
                    PointF ptBR = new PointF(left + width, top + height);
                    g.DrawLine(pen, ptTL, ptTR);
                    g.DrawLine(pen, ptTL, ptBL);
                    g.DrawLine(pen, ptTR, ptBR);
                    g.DrawLine(pen, ptBL, ptBR);
                    g.DrawLine(pen, ptTL, ptBR);
                }
            }

            public static void DrawFrame(ref WriteableBitmap bitmapSource,
                System.Windows.Int32Rect region,
                System.Windows.Media.Color lineColor, int lineThickness = 1)
            {
                Bitmap bitmap = BitmapSourceToBitmap(bitmapSource);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    DrawFrame_Bitmap(g, region.X, region.Y,
                        region.Width, region.Height,
                        WinColorToColor(lineColor), lineThickness);
                }
                bitmapSource = BitmapToBitmapSource(bitmap);
            }
            public static void DrawFrame_Bitmap(Graphics g,
                Rectangle region,
                Color lineColor, int lineThickness = 1)
            {
                DrawFrame_Bitmap(g,
                    region.X, region.Y, region.Width, region.Height,
                    lineColor, lineThickness);
            }
            public static void DrawFrame(ref WriteableBitmap bitmapSource,
                System.Windows.Rect region,
                System.Windows.Media.Color lineColor, double lineThickness = 1)
            {
                Bitmap bitmap = BitmapSourceToBitmap(bitmapSource);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    DrawFrame_Bitmap(g, (float)region.X, (float)region.Y,
                        (float)region.Width, (float)region.Height,
                        WinColorToColor(lineColor), (float)lineThickness);
                }
                bitmapSource = BitmapToBitmapSource(bitmap);
            }
            public static void DrawFrame_Bitmap(Graphics g,
                RectangleF region,
                Color lineColor, float lineThickness = 1)
            {
                DrawFrame_Bitmap(g,
                    region.X, region.Y, region.Width, region.Height,
                    lineColor, lineThickness);
            }
            public static void DrawFrame_Bitmap(Graphics g,
                Point pointTopLeft, Size size,
                Color lineColor, int lineThickness = 1)
            {
                DrawFrame_Bitmap(g,
                    pointTopLeft.X, pointTopLeft.Y, size.Width, size.Height,
                    lineColor, lineThickness);
            }
            public static void DrawFrame_Bitmap(Graphics g,
                PointF pointTopLeft, SizeF size,
                Color lineColor, float lineThickness = 1)
            {
                DrawFrame_Bitmap(g,
                    pointTopLeft.X, pointTopLeft.Y, size.Width, size.Height,
                    lineColor, lineThickness);
            }

            #endregion

            #region draw cross

            public class Crosses
            {
                private static Basics.PinedBitmap imgCrossC1_Bitmap = null;
                private static Basics.PinedBitmapSource imgCrossC1 = null;
                /// <summary>
                /// return a instance of a cross image(with pin point);
                /// with a 4px spacing, straight, 2px black bar, 1px white bar
                /// </summary>
                /// <returns></returns>
                public static Basics.PinedBitmapSource GetImgCrossC1()
                {
                    if (imgCrossC1_Bitmap == null)
                    {
                        Basics.PinedBitmap innerCrs = _DrawImgCross(3, 1, 2, Color.Black);
                        Basics.PinedBitmap outerCrs = _DrawImgCross(7, 1, 1, Color.White);
                        using (Graphics g = Graphics.FromImage(outerCrs.bitmap))
                            g.DrawImage(innerCrs.bitmap, 1, 1);
                        innerCrs.Dispose();
                        imgCrossC1_Bitmap = new Basics.PinedBitmap()
                        {
                            bitmap = outerCrs.bitmap,
                            pinPoint = outerCrs.pinPoint,
                        };
                        imgCrossC1 = new Basics.PinedBitmapSource()
                        {
                            bitmapSource = BitmapToBitmapSource(outerCrs.bitmap, false),
                            pinPoint = PointFToWinPoint(outerCrs.pinPoint),
                        };
                    }
                    return imgCrossC1;
                }
                public static Basics.PinedBitmap GetImgCrossC1_Bitmap()
                {
                    if (imgCrossC1_Bitmap == null)
                    {
                        Basics.PinedBitmap innerCrs = _DrawImgCross(3, 1, 2, Color.Black);
                        Basics.PinedBitmap outerCrs = _DrawImgCross(7, 1, 1, Color.White);
                        using (Graphics g = Graphics.FromImage(outerCrs.bitmap))
                            g.DrawImage(innerCrs.bitmap, 1, 1);
                        innerCrs.Dispose();
                        imgCrossC1_Bitmap = new Basics.PinedBitmap()
                        {
                            bitmap = outerCrs.bitmap,
                            pinPoint = outerCrs.pinPoint,
                        };
                    }
                    return imgCrossC1_Bitmap;
                }

                private static Basics.PinedBitmap _DrawImgCross(int space,
                    int barWidth, int barLength, Color barColor, bool slanting = false)
                {
                    int fullWidth = space + barLength + barLength;
                    Bitmap img = new Bitmap(fullWidth, fullWidth);
                    bool isCenterAtBoundry = space % 2 == 0;
                    if (!slanting && isCenterAtBoundry != (barWidth % 2 == 0))
                        barWidth += 1;
                    int spaceRadius = space / 2;
                    int radius = fullWidth / 2;
                    Point center = new Point(radius, radius);
                    int barVHeight, x, y, yInc, ycab;
                    if (slanting)
                    {
                        int yTop, yBtm;
                        for (int i = spaceRadius, iv = spaceRadius + barLength; i < iv; i++)
                        {
                            barVHeight = barWidth + Math.Min(barWidth - 1, i - spaceRadius);
                            ycab = (isCenterAtBoundry ? 0 : 1);
                            for (int w = 0; w < barVHeight; w++)
                            {
                                // left, top bottom
                                yInc = i + w - Math.Min(i - spaceRadius, barWidth - 1);
                                yTop = center.Y - 1 - yInc;
                                if (yTop < 0)
                                    continue;
                                yBtm = center.Y + yInc + ycab;
                                x = center.X - 1 - i;
                                img.SetPixel(
                                    x,
                                    yTop,
                                    barColor
                                    );
                                img.SetPixel(
                                    x,
                                    yBtm,
                                    barColor
                                    );

                                // right, top bottom
                                x = center.X + i + ycab;
                                img.SetPixel(
                                    x,
                                    yTop,
                                    barColor
                                    );
                                img.SetPixel(
                                    x,
                                    yBtm,
                                    barColor
                                    );

                            }
                        }
                    }
                    else
                    {
                        int halfBarWidth = barWidth / 2;
                        for (int i = spaceRadius, iv = spaceRadius + barLength; i < iv; i++)
                        {
                            for (int w = 0; w < barWidth; w++)
                            {
                                // left
                                x = center.X - 1 - i;
                                y = center.Y - halfBarWidth + w;
                                img.SetPixel(
                                    x,
                                    y,
                                    barColor);
                                // top
                                img.SetPixel(
                                    center.X - halfBarWidth + w,
                                    center.Y - 1 - i,
                                    barColor);
                                // right
                                img.SetPixel(
                                    center.X + i + (isCenterAtBoundry ? 0 : 1),
                                    center.Y - halfBarWidth + w,
                                    barColor);
                                // bottom
                                img.SetPixel(
                                    center.X - halfBarWidth + w,
                                    center.Y + i + (isCenterAtBoundry ? 0 : 1),
                                    barColor);
                            }
                        }
                    }
                    float inc = isCenterAtBoundry ? 0 : 0.5f;
                    return new Basics.PinedBitmap()
                    { bitmap = img, pinPoint = new PointF(center.X + inc, center.Y + inc) };
                }

                public static void DrawGraCrossC1(ref WriteableBitmap bitmapSource, int x, int y)
                {
                    Bitmap bitmap = BitmapSourceToBitmap(bitmapSource);
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        DrawGraCrossC1_Bitmap(g, x, y);
                    }
                    bitmapSource = BitmapToBitmapSource(bitmap);
                }
                public static void DrawGraCrossC1_Bitmap(Graphics g, int x, int y)
                {
                    Rectangle[] blocks;
                    using (SolidBrush sBr = new SolidBrush(Color.Black))
                    {
                        // inner cross
                        blocks = new Rectangle[]
                        {
                    new Rectangle(x - 7, y - 1, 3, 2), // left
                    new Rectangle(x + 4, y - 1, 3, 2), // right
                    new Rectangle(x - 1, y - 7, 2, 3), // up
                    new Rectangle(x - 1, y + 4, 2, 3), // down
                        };
                        g.FillRectangles(sBr, blocks);
                    }
                    using (SolidBrush sBr = new SolidBrush(Color.White))
                    {
                        // outer cross
                        blocks = new Rectangle[]
                        {
                    new Rectangle(x - 10, y - 1, 3, 2), // left
                    new Rectangle(x + 7, y - 1, 3, 2), // right
                    new Rectangle(x - 1, y - 10, 2, 3), // up
                    new Rectangle(x - 1, y + 7, 2, 3), // down
                        };
                        g.FillRectangles(sBr, blocks);
                    }
                    g.Dispose();
                }
            }

            #endregion



            /// <summary>
            /// 获取正多边形的顶点，中心点坐标为(0,0)
            /// </summary>
            /// <param name="sides">多边形的变数</param>
            /// <param name="radius">半径</param>
            /// <param name="isCircumcircle">半径为多边形的外接圆半径，否则为内切圆半径</param>
            /// <param name="firstPointTopOrRight">第一个顶点在中心点的上方，或右方</param>
            /// <returns></returns>
            public static System.Windows.Point[] GenerateRegularPolygonVertexes(
                int sides,
                double radius, bool isCircumcircle = false,
                bool firstPointTopOrRight = true)
            {
                return PointFsToWinPoints(
                    GenerateRegularPolygonVertexes_Bitmap(
                        sides, radius, isCircumcircle, firstPointTopOrRight));
            }
            public static PointF[] GenerateRegularPolygonVertexes_Bitmap(
                int sides,
                double radius, bool isCircumcircle = false,
                bool firstPointTopOrRight = true)
            {
                double angInit;
                if (firstPointTopOrRight)
                    angInit = Math.PI * 1.5;
                else
                    angInit = 0;
                if (!isCircumcircle)
                {
                    radius /= Math.Cos(Math.PI / sides);
                }
                return GenerateRegularPolygonVertexes_Bitmap(sides, radius, angInit);
            }

            public static System.Windows.Point[] GenerateRegularPolygonVertexes(
                int sides,
                double radiusOfVertex, double startRadian)
            {
                return PointFsToWinPoints(
                    GenerateRegularPolygonVertexes_Bitmap(
                        sides, radiusOfVertex, startRadian));
            }
            public static PointF[] GenerateRegularPolygonVertexes_Bitmap(
                int sides,
                double radiusOfVertex, double startRadian)
            {
                PointF[] result = new PointF[sides];
                double ang;

                for (int i = 0; i < sides; i++)
                {
                    ang = startRadian + (float)((Math.PI + Math.PI) * ((double)i / sides));
                    result[i] = new PointF(
                        (float)(radiusOfVertex * Math.Cos(ang)),
                        (float)(radiusOfVertex * Math.Sin(ang))
                        );
                }
                return result;
            }


            public static System.Windows.Point[] GenerateRegularStarVertexes(
                int pinnacles, double outerRadius, double innerRadius, bool firstpinnacleTopOrRight)
            {
                return PointFsToWinPoints(
                    GenerateRegularStarVertexes_Bitmap(
                        pinnacles, outerRadius, innerRadius, firstpinnacleTopOrRight));
            }
            public static PointF[] GenerateRegularStarVertexes_Bitmap(
                int pinnacles, double outerRadius, double innerRadius, bool firstpinnacleTopOrRight)
            {
                double angInit;
                if (firstpinnacleTopOrRight)
                    angInit = Math.PI * 1.5;
                else
                    angInit = 0;
                return GenerateRegularStarVertexes_Bitmap(pinnacles, outerRadius, innerRadius, angInit);
            }

            public static System.Windows.Point[] GenerateRegularStarVertexes(
                int pinnacles, double outerRadius, double innerRadius, double startRadian = 0)
            {
                return PointFsToWinPoints(
                    GenerateRegularStarVertexes_Bitmap(
                        pinnacles, outerRadius, innerRadius, startRadian));
            }
            public static PointF[] GenerateRegularStarVertexes_Bitmap(
                int pinnacles, double outerRadius, double innerRadius, double startRadian = 0)
            {
                PointF[] outerPoints = GenerateRegularPolygonVertexes_Bitmap(pinnacles, outerRadius, startRadian);
                double innerStartRadian = startRadian + (Math.PI + Math.PI) / pinnacles;
                PointF[] innerPoints = GenerateRegularPolygonVertexes_Bitmap(pinnacles, innerRadius, innerStartRadian);
                PointF[] result = new PointF[pinnacles + pinnacles];
                for (int i = 0, ii = 0, iv = outerPoints.Length; i < iv; i++)
                {
                    result[ii++] = outerPoints[i];
                    result[ii++] = innerPoints[i];
                }
                return result;
            }
        }


        public class Rotate
        {
            #region rotate image
            public class RotatingImageSource : IDisposable
            {
                public static void Rotate(BitmapSource oriImg, System.Windows.Point rotateCenter, System.Windows.Point parentRotateCenter,
                    out BitmapSource img, out System.Windows.Point pin)
                {
                    RotatingImageSource ri = new RotatingImageSource(oriImg, rotateCenter, parentRotateCenter);

                    img = ri.rotatedImg;
                    System.Windows.Point drawPoint = ri.DrawStartPoint;
                    pin = new System.Windows.Point(-drawPoint.X, -drawPoint.Y);
                }

                RotatingImage helper;
                public Bitmap oriImg;
                public PointF rotateCenter, parentRotateCenter;
                public RotatingImageSource(BitmapSource oriImg, System.Windows.Point rotateCenter, System.Windows.Point parentRotateCenter)
                {
                    helper = new RotatingImage(BitmapSourceToBitmap(oriImg), WinPointToPointF(rotateCenter), WinPointToPointF(parentRotateCenter));
                }

                public System.Windows.Point DrawStartPoint
                {
                    get => PointFToWinPoint(helper.DrawStartPoint);
                }
                public BitmapSource rotatedImg
                {
                    get => BitmapToBitmapSource(helper.rotatedImg);
                }

                public System.Windows.Point rP
                {
                    get => PointFToWinPoint(helper.rP);
                }
                public void Rotate_Deg(double angleDeg = 0)
                {
                    helper.Rotate_Deg(angleDeg);
                }
                public void Rotate_PI(double anglePI = 0)
                {
                    helper.Rotate_PI(anglePI);
                }

                public void Dispose()
                {
                    helper.Dispose();
                }
            }
            public class RotatingImage : IDisposable
            {
                public static void Rotate(Bitmap oriImg, PointF rotateCenter, PointF parentRotateCenter,
                    out Bitmap img, out Point pin)
                {
                    RotatingImage ri = new RotatingImage(oriImg, rotateCenter, parentRotateCenter);
                    img = ri.rotatedImg;
                    Point drawPoint = ri.DrawStartPoint;
                    pin = new Point(-drawPoint.X, -drawPoint.Y);
                }

                public Bitmap oriImg;
                public PointF rotateCenter, parentRotateCenter;
                public RotatingImage(Bitmap oriImg, PointF rotateCenter, PointF parentRotateCenter)
                {
                    this.oriImg = oriImg;
                    this.rotateCenter = rotateCenter;
                    this.parentRotateCenter = parentRotateCenter;
                }

                public Point DrawStartPoint;
                public Bitmap rotatedImg;

                public PointF rP;
                public void Rotate_Deg(double angleDeg = 0)
                {
                    double anglePI = angleDeg / 180 * Math.PI;
                    _Rotate(anglePI, angleDeg);
                }
                public void Rotate_PI(double anglePI = 0)
                {
                    double angleDeg = anglePI / Math.PI * 180;
                    _Rotate(anglePI, angleDeg);
                }
                private void _Rotate(double anglePI = 0, double angleDeg = 0)
                {
                    anglePI = GetAngleInSingleRange_Pi(anglePI);
                    double sinAgl = Math.Sin(anglePI);
                    double cosAgl = Math.Cos(anglePI);

                    double widthMCos = oriImg.Width * cosAgl;
                    double widthMSin = oriImg.Width * sinAgl;
                    double heightMSin = oriImg.Height * sinAgl;
                    double heightMCos = oriImg.Height * cosAgl;

                    double newWidthDouble = Math.Abs(widthMCos) + Math.Abs(heightMSin);
                    double newHeightDouble = Math.Abs(widthMSin) + Math.Abs(heightMCos);
                    //double newWidthDouble = 200;
                    //double newHeightDouble = 200;
                    int newWidth = (int)(newWidthDouble + 2);
                    int newHeight = (int)(newHeightDouble + 2);

                    PointF tunedRC = new PointF(rotateCenter.X - 0.5f, rotateCenter.Y - 0.5f);
                    //PointF tunedRC = new PointF(rotateCenter.X , rotateCenter.Y );
                    rP = RotatePointF_PI(new PointF(), tunedRC, anglePI);

                    double transX, transY;
                    double PI = Math.PI;
                    double halfPI = PI / 2;
                    double oneAndHalfPI = PI + halfPI;
                    if (anglePI > oneAndHalfPI)
                    {   // in 4th quadrant
                        transX = 0;
                        transY = -widthMSin;
                    }
                    else if (anglePI > PI)
                    {   // in 3th quadrant
                        transX = -widthMCos;
                        transY = -widthMSin - heightMCos;
                    }
                    else if (anglePI > halfPI)
                    {   // in 2nd quadrant
                        transX = newWidthDouble;
                        transY = -heightMCos;
                    }
                    else
                    {   // in 1st quadrant
                        transX = heightMSin;
                        transY = 0;
                    }
                    rotatedImg?.Dispose();
                    rotatedImg = new Bitmap(newWidth, newHeight);

                    using (Graphics g = Graphics.FromImage(rotatedImg))
                    using (GraphicsPath gp = new GraphicsPath())
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;

                        //the coordinate of the polygon must be
                        //point 1 = left top corner
                        //point 2 = right top corner
                        //point 3 = right bottom corner

                        // if mirror image
                        //gp.AddPolygon(new Point[]{new Point(0,imgpic.Height),
                        //    new Point(imgpic.Width,imgpic.Height),
                        //    new Point(0,0)});
                        gp.AddPolygon(new Point[]{new Point(0,0),
                    new Point(oriImg.Width,0),
                    new Point(0,oriImg.Height)});


                        //double dyX = 0;
                        //double dyY = 0;
                        double dyX = GetSmallFlow(parentRotateCenter.X);
                        double dyY = GetSmallFlow(parentRotateCenter.Y);

                        rP = new PointF(rP.X + (float)transX, rP.Y + (float)transY);
                        double dyX1 = GetSmallFlow(rP.X);
                        double dyY1 = GetSmallFlow(rP.Y);
                        dyX = GetSmallFlow(dyX - dyX1, false);
                        dyY = GetSmallFlow(dyY - dyY1, false);


                        transX += dyX - 0.5;
                        transY += dyY - 0.5;
                        rP = new PointF(rP.X + (float)dyX, rP.Y + (float)dyY);
                        Matrix transM = new Matrix(1, 0, 0, 1, 0, 0);
                        //  ==  move x,y (not move to)
                        transM.Translate(
                            (float)transX,
                            (float)transY);

                        DrawStartPoint = new Point(
                            (int)(parentRotateCenter.X - rP.X),
                            (int)(parentRotateCenter.Y - rP.Y));

                        //transM.RotateAt(angle, new PointF());
                        transM.Rotate((float)angleDeg);

                        gp.Transform(transM);
                        PointF[] pts = gp.PathPoints;

                        // for testing
                        //using (Brush b = new SolidBrush(Color.Pink))
                        //    g.FillRectangle(b, 0, 0, newWidth, newHeight);

                        //draw on the picturebox content of imgpic using the local transformation
                        //using the resulting parralleogram described by pts
                        g.DrawImage(oriImg, pts);

                        // for testing
                        //QuickGraphics.DrawCrossC1(g, rP);
                    }
                }
                private double GetAngleInSingleRange_Deg(double angleDeg)
                {
                    if (angleDeg == 0)
                        return angleDeg;
                    else if (angleDeg > 0)
                        return angleDeg % 360;
                    else
                        return 360 + (angleDeg % 360);
                }
                private double GetAngleInSingleRange_Pi(double anglePi)
                {
                    double doublePI = Math.PI * 2;
                    if (anglePi == 0)
                        return anglePi;
                    else if (anglePi > 0)
                        return anglePi % doublePI;
                    else
                        return doublePI + (anglePi % doublePI);
                }
                private float GetSmallFlow(float value, bool forcePositive = true)
                {
                    if (value == 0)
                        return value;
                    else if (value > 0)
                        return value - (int)value;
                    else
                    {
                        if (forcePositive)
                            return value - (float)Math.Floor(value);
                        else
                            return value - (float)Math.Ceiling(value);
                    }
                }
                private double GetSmallFlow(double value, bool forcePositive = true)
                {
                    if (value == 0)
                        return value;
                    else if (value > 0)
                        return value - (int)value;
                    else
                    {
                        if (forcePositive)
                            return value - Math.Floor(value);
                        else
                            return value - Math.Ceiling(value);
                    }
                }

                public void Dispose()
                {
                    oriImg?.Dispose();
                    rotatedImg?.Dispose();
                }
            }

            #endregion

            #region rotate point
            public static System.Windows.Point RotatePoint_deg(System.Windows.Point rotateCenter, System.Windows.Point sourcePoint, double angleDeg)
            {
                return PointFToWinPoint(
                    RotatePointF_deg(
                        WinPointToPointF(rotateCenter),
                        WinPointToPointF(sourcePoint),
                        angleDeg
                    ));
            }
            public static PointF RotatePointF_deg(PointF rotateCenter, PointF sourcePoint, double angleDeg)
            { return RotatePointF_PI(rotateCenter, sourcePoint, angleDeg / 180 * Math.PI); }

            public static System.Windows.Point RotatePoint_PI(System.Windows.Point rotateCenter, System.Windows.Point sourcePoint, double anglePi)
            {
                return PointFToWinPoint(
                    RotatePointF_PI(
                        WinPointToPointF(rotateCenter),
                        WinPointToPointF(sourcePoint),
                        anglePi
                    ));
            }
            public static PointF RotatePointF_PI(PointF rotateCenter, PointF sourcePoint, double anglePi)
            {
                double spX = sourcePoint.X - rotateCenter.X;
                double spY = sourcePoint.Y - rotateCenter.Y;

                anglePi = -anglePi;
                double sinAgl = Math.Sin(anglePi);
                double cosAgl = Math.Cos(anglePi);

                double spX1 = spX * cosAgl + spY * sinAgl;
                double spY1 = -spX * sinAgl + spY * cosAgl;

                return new PointF((float)(spX1 + rotateCenter.X), (float)(spY1 + rotateCenter.Y));
            }
            #endregion
        }

        public class Mirror
        {
            public class MirrorImageSource
            {
                public static BitmapSource LeftToRight(BitmapSource img)
                {
                    return BitmapToBitmapSource(MirrorImage.LeftToRight(BitmapSourceToBitmap(img)));
                }
                public static BitmapSource TopToBottom(BitmapSource img)
                {
                    return BitmapToBitmapSource(MirrorImage.TopToBottom(BitmapSourceToBitmap(img)));
                }
                /// <summary>
                /// need to test
                /// </summary>
                /// <param name="img"></param>
                /// <returns></returns>
                public static BitmapSource LeftTopToBottomRight(BitmapSource img)
                {
                    return BitmapToBitmapSource(MirrorImage.LeftTopToBottomRight(BitmapSourceToBitmap(img)));
                }
                /// <summary>
                /// need to test
                /// </summary>
                /// <param name="img"></param>
                /// <returns></returns>
                public static BitmapSource LeftBottomToTopRight(BitmapSource img)
                {
                    return BitmapToBitmapSource(MirrorImage.LeftBottomToTopRight(BitmapSourceToBitmap(img)));
                }
            }
            public class MirrorImage
            {
                public static Bitmap LeftToRight(Bitmap img)
                {
                    Bitmap result = new Bitmap(img);
                    result.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    return result;
                }
                public static Bitmap TopToBottom(Bitmap img)
                {
                    Bitmap result = new Bitmap(img);
                    result.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    return result;
                }

                /// <summary>
                /// need to test
                /// </summary>
                /// <param name="sourceImage"></param>
                /// <returns></returns>
                public static Bitmap LeftTopToBottomRight(Bitmap img)
                {
                    Bitmap result = new Bitmap(img);
                    result.RotateFlip(RotateFlipType.RotateNoneFlipXY);
                    return result;
                }
                /// <summary>
                /// need to test
                /// </summary>
                /// <param name="sourceImage"></param>
                /// <returns></returns>
                public static Bitmap LeftBottomToTopRight(Bitmap img)
                {
                    Bitmap result = new Bitmap(img);
                    result.RotateFlip(RotateFlipType.Rotate270FlipX);
                    return result;
                }
            }
        }

        #region chop image

        public static BitmapSource ChopImage(BitmapSource oriImage, int left, int top, int right, int bottom)
        {
            Bitmap bitmap = BitmapSourceToBitmap(oriImage);
            return BitmapToBitmapSource(ChopImage_Bitmap(bitmap, left, top, right, bottom));
        }
        public static Bitmap ChopImage_Bitmap(Bitmap oriImage, int left, int top, int right, int bottom)
        {
            if (oriImage == null)
                return null;
            if (left + right >= oriImage.Width)
                return null;
            if (top + bottom >= oriImage.Height)
                return null;
            int width = oriImage.Width - left - right;
            int height = oriImage.Height - top - bottom;
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(oriImage, new Point(-left, -top));
                //g.DrawImage(oriImage, new Rectangle(-left, -top, width, height));
            }
            return result;
        }


        #endregion

        public class Screen
        {
            [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
            static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

            public static Bitmap GetSnapAt_Bitmap(Point location, int width, int height)
            {
                Bitmap result = new Bitmap(5, 5, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics gdest = Graphics.FromImage(result))
                {
                    using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        IntPtr hSrcDC = gsrc.GetHdc();
                        IntPtr hDC = gdest.GetHdc();
                        int retval = BitBlt(
                            hDC, 0, 0, width, height,
                            hSrcDC, location.X - width / 2, location.Y - height / 2,
                            (int)CopyPixelOperation.SourceCopy);
                        gdest.ReleaseHdc();
                        gsrc.ReleaseHdc();
                    }
                }
                return result;
            }
            public static BitmapSource GetSnapAt(System.Windows.Point location, int width, int height)
            {
                return QuickGraphics.BitmapToBitmapSource(
                    GetSnapAt_Bitmap(QuickGraphics.WinPointToPoint(location), width, height));
            }
            public static Bitmap GetScreenPrimaryShot_Bitmap()
            {
                Bitmap screenBmp = new Bitmap(
                    (int)SystemParameters.PrimaryScreenWidth,
                    (int)SystemParameters.PrimaryScreenHeight,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                using (var bmpGraphics = Graphics.FromImage(screenBmp))
                {
                    bmpGraphics.CopyFromScreen(0, 0, 0, 0, screenBmp.Size);
                }
                return screenBmp;
            }
            public static BitmapSource GetPrimaryScreenShot()
            {
                return QuickGraphics.BitmapToBitmapSource(GetScreenPrimaryShot_Bitmap());
            }
        }


        public class Text
        {
            public static double MeasureWidth(
                string text, double fontSize,
                System.Windows.Media.FontFamily fontFamily,
                System.Windows.FontStyle fontStyle,
                FontWeight fontWeight, FontStretch fontStretch,
                double pixelsPerDpi = 96)
            {
                FormattedText formattedText = new FormattedText(
                    text,
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                    fontSize,
                    System.Windows.Media.Brushes.Black,
                    pixelsPerDpi
                );
                return formattedText.WidthIncludingTrailingWhitespace;
            }
            public static double MeasureWidth(string text, System.Windows.Controls.TextBox textBox, double pixelsPerDpi = 96)
            {
                return MeasureWidth(text, textBox.FontSize,
                    textBox.FontFamily, textBox.FontStyle, textBox.FontWeight,
                    textBox.FontStretch, pixelsPerDpi);
            }
        }

    }
}
