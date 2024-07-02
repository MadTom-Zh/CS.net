using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Color = System.Drawing.Color;
using Matrix = System.Drawing.Drawing2D.Matrix;

namespace MadTomDev.UI
{
    public class QuickCheckNCalculate
    {
        #region check point in window
        public static bool CheckPointInWindow(Point point, Rectangle windowRect)
        {
            if (windowRect.Left <= point.X)
            {
                if (point.X <= (windowRect.Left + windowRect.Width))
                {
                    // in ver range
                    if (windowRect.Top <= point.Y)
                    {
                        if (point.Y <= (windowRect.Top + windowRect.Height))
                        {
                            // in hor range
                            return true;
                        }
                    }

                }
            }
            return false;
        }
        public static bool CheckPointInWindowRow(Point point, Rectangle windowRect)
        { return (windowRect.Top <= point.Y && point.Y <= windowRect.Bottom); }
        public static bool CheckPointInWindowCol(Point point, Rectangle windowRect)
        { return (windowRect.Left <= point.X && point.X <= windowRect.Right); }

        #endregion

        #region check rectangle relation

        public enum GraphRelations
        { Mixed, Inside, Outside, Containing }
        public static GraphRelations CheckRectangleRelationRow(Rectangle mainRect, Rectangle refRect)
        {
            bool topPointInside = CheckPointInWindowRow(mainRect.Location, refRect),
                bottomPointInside = CheckPointInWindowRow(
                    new Point(mainRect.Location.X, mainRect.Location.Y + mainRect.Height), refRect);
            if (topPointInside)
            {
                if (bottomPointInside)
                    return GraphRelations.Inside;
                else return GraphRelations.Mixed;
            }
            else
            {
                if (bottomPointInside)
                    return GraphRelations.Mixed;
                else
                {
                    if (mainRect.Top > refRect.Bottom
                        || refRect.Top > mainRect.Bottom)
                        return GraphRelations.Outside;
                    else
                        return GraphRelations.Containing;
                }
            }
        }
        public static GraphRelations CheckRectangleRelationCol(Rectangle mainRect, Rectangle refRect)
        {
            bool leftPointInside = CheckPointInWindowCol(mainRect.Location, refRect),
                rihgtPointInside = CheckPointInWindowCol(
                    new Point(mainRect.Location.X + mainRect.Width, mainRect.Location.Y), refRect);
            if (leftPointInside)
            {
                if (rihgtPointInside)
                    return GraphRelations.Inside;
                else return GraphRelations.Mixed;
            }
            else
            {
                if (rihgtPointInside)
                    return GraphRelations.Mixed;
                else
                {
                    if (mainRect.Left > refRect.Right
                        || refRect.Left > mainRect.Right)
                        return GraphRelations.Outside;
                    else
                        return GraphRelations.Containing;
                }
            }
        }
        public static GraphRelations CheckRectangleRelation(Rectangle mainRect, Rectangle refRect)
        {
            GraphRelations verRela = CheckRectangleRelationRow(mainRect, refRect),
                horRela = CheckRectangleRelationCol(mainRect, refRect);
            if (verRela == GraphRelations.Outside
                || horRela == GraphRelations.Outside)
                return GraphRelations.Outside;
            else if (verRela == GraphRelations.Inside
                && horRela == GraphRelations.Inside)
                return GraphRelations.Inside;
            else if (verRela == GraphRelations.Containing
                && horRela == GraphRelations.Containing)
                return GraphRelations.Containing;
            else return GraphRelations.Mixed;

        }
        public static GraphRelations CheckRectangleRelation(Rectangle mainRect, Rectangle refRect,
            out GraphRelations verGR, out GraphRelations horGR)
        {
            verGR = CheckRectangleRelationRow(mainRect, refRect);
            horGR = CheckRectangleRelationCol(mainRect, refRect);
            if (verGR == GraphRelations.Outside
                || horGR == GraphRelations.Outside)
                return GraphRelations.Outside;
            else if (verGR == GraphRelations.Inside
                && horGR == GraphRelations.Inside)
                return GraphRelations.Inside;
            else if (verGR == GraphRelations.Containing
                && horGR == GraphRelations.Containing)
                return GraphRelations.Containing;
            else return GraphRelations.Mixed;

        }

        public static bool CheckRectangleIntersection(RectangleF rect1,RectangleF rect2)
        {
            double l1 = rect1.Left,r1 = rect1.Right,
                t1 = rect1.Top, b1 = rect1.Bottom,
                l2 = rect2.Left, r2 = rect2.Right,
                t2 = rect2.Top, b2 = rect2.Bottom;
            bool verIntersect = (l1 < l2 && l2 < r1)
                || (l1 < r2 && r2 < r1)
                || (l2 < l1 && l1 < r2)
                || (l2 < r1 && r1 < r2);
            bool horIntersect = (t1 < t2 && t2 < b1)
                || (t1 < b2 && b2 < b1)
                || (t2 < t1 && t1 < b2)
                || (t2 < b1 && b1 < b2); ;
            return verIntersect && horIntersect;
        }

        #endregion
        public static Rectangle MoveRectangle(Rectangle rectangle, Point firstPoint, Point nextPoint)
        {
            return new Rectangle(
                rectangle.Left + nextPoint.X - firstPoint.X,
                rectangle.Top + nextPoint.Y - firstPoint.Y,
                rectangle.Width, rectangle.Height);
        }

        #region check point in line, in polygon
        public static bool CheckPointInLine(Point start, Point end, Point testPt, double driftThreshold = 0.01)
        {
            return CheckPointInLine(new PointF(start.X, start.Y), new PointF(end.X, end.Y),
                new PointF(testPt.X, testPt.Y), driftThreshold);
        }


        public static bool CheckPointInLine(PointF start, PointF end, PointF testPt, double driftThreshold = 0.01)
        {
            // in hor
            if (start.Y == end.Y)
            {
                if (Math.Abs(testPt.Y - start.Y) > driftThreshold)
                    return false;
                if (start.X > end.X)
                {
                    if (testPt.X < end.X || testPt.X > start.X)
                        return false;
                }
                else
                {
                    if (testPt.X < start.X || testPt.X > end.X)
                        return false;
                }
                return true;
            }
            // in ver
            if (start.X == end.X)
            {
                if (Math.Abs(testPt.X - start.X) > driftThreshold)
                    return false;
                if (start.Y > end.Y)
                {
                    if (testPt.Y < end.Y || testPt.Y > start.Y)
                        return false;
                }
                else
                {
                    if (testPt.Y < start.Y || testPt.Y > end.Y)
                        return false;
                }
                return true;
            }



            // in angle
            double halfPi = Math.PI / 2;
            double ang1 = Math.Abs(CalculatePiAngle(testPt, start, end));
            if (ang1 > halfPi)
                return false;
            else
            {
                ang1 = Math.Abs(CalculatePiAngle(testPt, end, start));
                if (ang1 > halfPi)
                    return false;
                else
                {
                    double drift = Math.Sin(ang1) * CalculateDistance(testPt, end);
                    if (drift <= driftThreshold)
                        return true;
                }
            }
            return false;
        }

        public static bool CheckPointInPolygon(Point[] polygon, PointF testPoint)
        {
            int lastPointIndex = polygon.Length - 1;
            float total_angle = GetAngle(
                polygon[lastPointIndex].X, polygon[lastPointIndex].Y,
                testPoint.X, testPoint.Y,
                polygon[0].X, polygon[0].Y);

            for (int i = 0; i < lastPointIndex; i++)
            {
                total_angle += GetAngle(
                    polygon[i].X, polygon[i].Y,
                    testPoint.X, testPoint.Y,
                    polygon[i + 1].X, polygon[i + 1].Y);
            }

            return (Math.Abs(total_angle) > 1);
        }
        public static bool CheckPointInPolygon(List<Point> polygon, PointF testPoint)
        {
            int lastPointIndex = polygon.Count - 1;
            float total_angle = GetAngle(
                polygon[lastPointIndex].X, polygon[lastPointIndex].Y,
                testPoint.X, testPoint.Y,
                polygon[0].X, polygon[0].Y);

            for (int i = 0; i < lastPointIndex; i++)
            {
                total_angle += GetAngle(
                    polygon[i].X, polygon[i].Y,
                    testPoint.X, testPoint.Y,
                    polygon[i + 1].X, polygon[i + 1].Y);
            }

            return (Math.Abs(total_angle) > 1);
        }
        public static bool CheckPointInPolygon(List<PointF> polygon, PointF testPoint)
        {
            // from http://csharphelper.com/blog/2014/07/determine-whether-a-point-is-inside-a-polygon-in-c/#:~:text=Determine%20whether%20a%20point%20is%20inside%20a%20polygon,polygon%2C%20then%20you%20look%20at%20the%20angle%20APB.
            // Get the angle between the point and the
            // first and last vertices.
            int lastPointIndex = polygon.Count - 1;
            float total_angle = GetAngle(
                polygon[lastPointIndex].X, polygon[lastPointIndex].Y,
                testPoint.X, testPoint.Y,
                polygon[0].X, polygon[0].Y);

            // Add the angles from the point
            // to each other pair of vertices.
            for (int i = 0; i < lastPointIndex; i++)
            {
                total_angle += GetAngle(
                    polygon[i].X, polygon[i].Y,
                    testPoint.X, testPoint.Y,
                    polygon[i + 1].X, polygon[i + 1].Y);
            }

            // The total angle should be 2 * PI or -2 * PI if
            // the point is in the polygon and close to zero
            // if the point is outside the polygon.
            // The following statement was changed. See the comments.
            //return (Math.Abs(total_angle) > 0.000001);
            return (Math.Abs(total_angle) > 1);
        }
        public static bool CheckPointInPolygon(PointF[] polygon, PointF testPoint)
        {
            int lastPointIndex = polygon.Length - 1;
            float total_angle = GetAngle(
                polygon[lastPointIndex].X, polygon[lastPointIndex].Y,
                testPoint.X, testPoint.Y,
                polygon[0].X, polygon[0].Y);

            for (int i = 0; i < lastPointIndex; i++)
            {
                total_angle += GetAngle(
                    polygon[i].X, polygon[i].Y,
                    testPoint.X, testPoint.Y,
                    polygon[i + 1].X, polygon[i + 1].Y);
            }
            return (Math.Abs(total_angle) > 1);
        }
        public static bool CheckPointInPolygon(IList<System.Windows.Point> polygon, System.Windows.Point testPoint)
        {
            int lastPointIndex = polygon.Count - 1;
            double total_angle = GetAngle(
                polygon[lastPointIndex].X, polygon[lastPointIndex].Y,
                testPoint.X, testPoint.Y,
                polygon[0].X, polygon[0].Y);

            for (int i = 0; i < lastPointIndex; i++)
            {
                total_angle += GetAngle(
                    polygon[i].X, polygon[i].Y,
                    testPoint.X, testPoint.Y,
                    polygon[i + 1].X, polygon[i + 1].Y);
            }
            return (Math.Abs(total_angle) > 1);
        }
        private static float GetAngle(float Ax, float Ay,
            float Bx, float By, float Cx, float Cy)
        {
            // Get the dot product.
            float dot_product = DotProduct(Ax, Ay, Bx, By, Cx, Cy);

            // Get the cross product.
            float cross_product = CrossProductLength(Ax, Ay, Bx, By, Cx, Cy);

            // Calculate the angle.
            return (float)Math.Atan2(cross_product, dot_product);
        }
        private static double GetAngle(double Ax, double Ay,
            double Bx, double By, double Cx, double Cy)
        {
            // Get the dot product.
            double dot_product = DotProduct(Ax, Ay, Bx, By, Cx, Cy);

            // Get the cross product.
            double cross_product = CrossProductLength(Ax, Ay, Bx, By, Cx, Cy);

            // Calculate the angle.
            return (float)Math.Atan2(cross_product, dot_product);
        }
        private static double doubleGetAngle(double Ax, double Ay,
            double Bx, double By, double Cx, double Cy)
        {
            // Get the dot product.
            double dot_product = DotProduct(Ax, Ay, Bx, By, Cx, Cy);

            // Get the cross product.
            double cross_product = CrossProductLength(Ax, Ay, Bx, By, Cx, Cy);

            // Calculate the angle.
            return Math.Atan2(cross_product, dot_product);
        }
        private static float CrossProductLength(float Ax, float Ay,
            float Bx, float By, float Cx, float Cy)
        {
            // Get the vectors' coordinates.
            float BAx = Ax - Bx;
            float BAy = Ay - By;
            float BCx = Cx - Bx;
            float BCy = Cy - By;

            // Calculate the Z coordinate of the cross product.
            return (BAx * BCy - BAy * BCx);
        }
        private static double CrossProductLength(double Ax, double Ay,
            double Bx, double By, double Cx, double Cy)
        {
            // Get the vectors' coordinates.
            double BAx = Ax - Bx;
            double BAy = Ay - By;
            double BCx = Cx - Bx;
            double BCy = Cy - By;

            // Calculate the Z coordinate of the cross product.
            return (BAx * BCy - BAy * BCx);
        }
        private static float DotProduct(float Ax, float Ay,
            float Bx, float By, float Cx, float Cy)
        {
            // Get the vectors' coordinates.
            float BAx = Ax - Bx;
            float BAy = Ay - By;
            float BCx = Cx - Bx;
            float BCy = Cy - By;

            // Calculate the dot product.
            return (BAx * BCx + BAy * BCy);
        }
        private static double DotProduct(double Ax, double Ay,
            double Bx, double By, double Cx, double Cy)
        {
            // Get the vectors' coordinates.
            double BAx = Ax - Bx;
            double BAy = Ay - By;
            double BCx = Cx - Bx;
            double BCy = Cy - By;

            // Calculate the dot product.
            return (BAx * BCx + BAy * BCy);
        }

        #endregion

        #region check polygon is convex

        public static bool CheckPolygonIsConvex(List<Point> polygon)
        {
            // from http://csharphelper.com/blog/2014/07/determine-whether-a-polygon-is-convex-in-c/
            // For each set of three adjacent points A, B, C,
            // find the cross product AB · BC. If the sign of
            // all the cross products is the same, the angles
            // are all positive or negative (depending on the
            // order in which we visit them) so the polygon
            // is convex.
            bool got_negative = false;
            bool got_positive = false;
            int num_points = polygon.Count;
            int B, C;
            for (int A = 0; A < num_points; A++)
            {
                B = (A + 1) % num_points;
                C = (B + 1) % num_points;

                float cross_product =
                    CrossProductLength(
                        polygon[A].X, polygon[A].Y,
                        polygon[B].X, polygon[B].Y,
                        polygon[C].X, polygon[C].Y);
                if (cross_product < 0)
                {
                    got_negative = true;
                }
                else if (cross_product > 0)
                {
                    got_positive = true;
                }
                if (got_negative && got_positive) return false;
            }

            // If we got this far, the polygon is convex.
            return true;
        }

        #endregion

        #region polygon enlarge
        public static List<PointF> PolygonEnlarged(
            List<Point> oldPolygon, float offset)
        {
            List<PointF> opFloated = new List<PointF>();
            foreach (Point p in oldPolygon)
                opFloated.Add(new PointF(p.X, p.Y));
            return PolygonEnlarged(opFloated, offset);
        }
        public static List<PointF> PolygonEnlarged(
            List<PointF> oldPolygon, float offset)
        {
            offset = -offset;
            List<PointF> enlarged_points = new List<PointF>();
            int num_points = oldPolygon.Count;
            for (int j = 0; j < num_points; j++)
            {
                // Find the new location for point j.
                // Find the points before and after j.
                int i = (j - 1);
                if (i < 0) i += num_points;
                int k = (j + 1) % num_points;

                // Move the points by the offset.
                PointF v1 = new PointF(
                    oldPolygon[j].X - oldPolygon[i].X,
                    oldPolygon[j].Y - oldPolygon[i].Y);
                v1 = VectorNormalize(v1);
                v1 = new PointF(v1.X * offset, v1.Y * offset);
                //v1 *= offset;
                PointF n1 = new PointF(-v1.Y, v1.X);

                PointF pij1 = new PointF(
                    (float)(oldPolygon[i].X + n1.X),
                    (float)(oldPolygon[i].Y + n1.Y));
                PointF pij2 = new PointF(
                    (float)(oldPolygon[j].X + n1.X),
                    (float)(oldPolygon[j].Y + n1.Y));

                PointF v2 = new PointF(
                    oldPolygon[k].X - oldPolygon[j].X,
                    oldPolygon[k].Y - oldPolygon[j].Y);
                v2 = VectorNormalize(v2);
                v2 = new PointF(v2.X * offset, v2.Y * offset);
                //v2 *= offset;
                PointF n2 = new PointF(-v2.Y, v2.X);

                PointF pjk1 = new PointF(
                    (float)(oldPolygon[j].X + n2.X),
                    (float)(oldPolygon[j].Y + n2.Y));
                PointF pjk2 = new PointF(
                    (float)(oldPolygon[k].X + n2.X),
                    (float)(oldPolygon[k].Y + n2.Y));

                // See where the shifted lines ij and jk intersect.
                bool lines_intersect, segments_intersect;
                PointF poi, close1, close2;
                poi = FindIntersection(pij1, pij2, pjk1, pjk2,
                    out lines_intersect, out segments_intersect,
                    out close1, out close2);
                //Debug.Assert(lines_intersect,
                //    "Edges " + i + "-->" + j + " and " +
                //    j + "-->" + k + " are parallel");

                enlarged_points.Add(poi);
            }

            return enlarged_points;
        }

        #endregion


        #region normalize vector
        public static PointF VectorNormalize(PointF A)
        {
            double distance = Math.Sqrt(A.X * A.X + A.Y * A.Y);
            return new PointF((float)(A.X / distance), (float)(A.Y / distance));
        }

        #endregion

        /// <summary>
        /// 寻找两条线的交叉点
        /// * 需要验证
        /// </summary>
        /// <param name="l1start">第1条线段的起点</param>
        /// <param name="l1end">第1条线段的终点</param>
        /// <param name="l2start">第2条线段的起点</param>
        /// <param name="l2end">第2条线段的终点</param>
        /// <param name="lines_intersect">两条直线是否交叉（两线段不一定交叉）</param>
        /// <param name="segments_intersect">聊天线段是否交叉</param>
        /// <param name="close_p1">线段1上距离交叉点最近的点</param>
        /// <param name="close_p2">线段2上距离交叉点最近的点</param>
        /// <returns></returns>
        public static PointF FindIntersection(
            PointF l1start, PointF l1end, PointF l2start, PointF l2end,
            out bool lines_intersect, out bool segments_intersect,
            out PointF close_p1, out PointF close_p2)
        {
            // Get the segments' parameters.
            float dx12 = l1end.X - l1start.X;
            float dy12 = l1end.Y - l1start.Y;
            float dx34 = l2end.X - l2start.X;
            float dy34 = l2end.Y - l2start.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 =
                ((l1start.X - l2start.X) * dy34 + (l2start.Y - l1start.Y) * dx34)
                    / denominator;
            PointF intersection = new PointF(float.NaN, float.NaN);
            if (float.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                lines_intersect = false;
                segments_intersect = false;
                // intersection = new PointF(float.NaN, float.NaN);
                close_p1 = new PointF(float.NaN, float.NaN);
                close_p2 = new PointF(float.NaN, float.NaN);
                return intersection;
            }
            lines_intersect = true;

            float t2 =
                ((l2start.X - l1start.X) * dy12 + (l1start.Y - l2start.Y) * dx12)
                    / -denominator;

            // Find the point of intersection.
            intersection = new PointF(l1start.X + dx12 * t1, l1start.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            close_p1 = new PointF(l1start.X + dx12 * t1, l1start.Y + dy12 * t1);
            close_p2 = new PointF(l2start.X + dx34 * t2, l2start.Y + dy34 * t2);
            return intersection;
        }

        #region calculate pi angle

        public static double CalculatePiAngle(PointF sidePoint1, PointF anglePoint, PointF sidePoint2, bool fromNPiToPi = true)
        {
            PointF p1 = new PointF(sidePoint1.X - anglePoint.X, sidePoint1.Y - anglePoint.Y);
            PointF p2 = new PointF(sidePoint2.X - anglePoint.X, sidePoint2.Y - anglePoint.Y);
            return CalculatePiAngle(p1, p2, fromNPiToPi);
        }
        public static double CalculatePiAngle(PointF sidePoint1, PointF sidePoint2, bool fromNPiToPi = true)
        {
            double result = Math.Atan2(sidePoint2.Y, sidePoint2.X) - Math.Atan2(sidePoint1.Y, sidePoint1.X);

            if (fromNPiToPi)
            {
                // return a angle from -PI ~ PI
                double doublePi = Math.PI * 2;
                result = result % doublePi;
                if (result < -Math.PI)
                    return doublePi + result;
                else if (result > Math.PI)
                    return result - doublePi;
                else
                    return result;
            }
            return result;
        }

        public static double CalculatePiAngle(Point sidePoint1, Point anglePoint, Point sidePoint2, bool fromNPiToPi = true)
        {
            Point p1 = new Point(sidePoint1.X - anglePoint.X, sidePoint1.Y - anglePoint.Y);
            Point p2 = new Point(sidePoint2.X - anglePoint.X, sidePoint2.Y - anglePoint.Y);
            return CalculatePiAngle(p1, p2, fromNPiToPi);
        }
        public static double CalculatePiAngle(Point sidePoint1, Point sidePoint2, bool fromNPiToPi = true)
        {
            double result = Math.Atan2(sidePoint2.Y, sidePoint2.X) - Math.Atan2(sidePoint1.Y, sidePoint1.X);

            if (fromNPiToPi)
            {
                // return a angle from -PI ~ PI
                double doublePi = Math.PI * 2;
                result = result % doublePi;
                if (result < -Math.PI)
                    return doublePi + result;
                else if (result > Math.PI)
                    return result - doublePi;
                else
                    return result;
            }
            return result;
        }

        #endregion

        #region check point position

        public enum PointPositionTypes
        { None, Inside, Touching, UpLeft, Up, UpRight, Left, Right, DownLeft, Down, DownRight, }
        public static PointPositionTypes CheckPointPosition(Point point, Rectangle rect)
        {
            if (point.X < rect.Left)
            {
                if (point.Y < rect.Top)
                    return PointPositionTypes.UpLeft;
                else if (point.Y > rect.Bottom)
                    return PointPositionTypes.DownLeft;
                else
                    return PointPositionTypes.Left;
            }
            else if (point.X == rect.Left
                || point.X == rect.Right)
            {
                if (point.Y < rect.Top)
                    return PointPositionTypes.Up;
                else if (point.Y > rect.Bottom)
                    return PointPositionTypes.Down;
                else
                    return PointPositionTypes.Touching;
            }
            else if (rect.Left < point.X && point.X < rect.Right)
            {
                if (point.Y < rect.Top)
                    return PointPositionTypes.Up;
                else if (point.Y > rect.Bottom)
                    return PointPositionTypes.Down;
                else if (point.Y == rect.Top
                    || point.Y == rect.Bottom)
                    return PointPositionTypes.Touching;
                else
                    return PointPositionTypes.Inside;
            }
            else //if (rect.Right < point.X)
            {
                if (point.Y < rect.Top)
                    return PointPositionTypes.UpRight;
                else if (point.Y > rect.Bottom)
                    return PointPositionTypes.DownRight;
                else
                    return PointPositionTypes.Right;
            }
        }

        #endregion

        #region get rect center, reverse color

        public static Point GetCenterPoint(Rectangle rectangle)
        {
            return new Point(
                rectangle.X + (int)(rectangle.Width / 2 + 0.5),
                rectangle.Y + (int)(rectangle.Height / 2 + 0.5));
        }
        public static Color GetReverseColor(Color sourceColor, bool isReverseAlpha = false)
        {
            if (isReverseAlpha)
                return Color.FromArgb(
                    255 - sourceColor.A,
                    255 - sourceColor.R,
                    255 - sourceColor.G,
                    255 - sourceColor.B);
            else
                return Color.FromArgb(
                    255 - sourceColor.R,
                    255 - sourceColor.G,
                    255 - sourceColor.B);
        }

        #endregion

        #region calculate distance
        public static double CalculateDistance(Point p1, Point p2)
        {
            int dX = p1.X - p2.X, dY = p1.Y - p2.Y;
            return Math.Sqrt(dX * dX + dY * dY);
        }
        public static double CalculateDistance(PointF p1, PointF p2)
        {
            double dX = p1.X - p2.X, dY = p1.Y - p2.Y;
            return Math.Sqrt(dX * dX + dY * dY);
        }

        #endregion

        #region calculate polygon centroid, area
        public static PointF CalculateCentroidOfPolygon(List<Point> points)
        {
            Point[] pointArray = points.ToArray();
            int num_points = pointArray.Count();
            if (pointArray == null || num_points == 0)
                return Point.Empty;

            // Find the centroid.
            double X = 0;
            double Y = 0;
            float second_factor;
            for (int i = 0, ip1; i < num_points; i++)
            {
                ip1 = i + 1;
                if (ip1 == num_points) ip1 = 0;
                second_factor =
                    pointArray[i].X * pointArray[ip1].Y -
                    pointArray[ip1].X * pointArray[i].Y;
                X += (pointArray[i].X + pointArray[ip1].X) * second_factor;
                Y += (pointArray[i].Y + pointArray[ip1].Y) * second_factor;
            }

            // Divide by 6 times the polygon's area.
            double polygon_area = CalculateAreaOfPolygon(points);
            X /= (6 * polygon_area);
            Y /= (6 * polygon_area);

            // If the values are negative, the polygon is
            // oriented counterclockwise so reverse the signs.
            if (X < 0)
            {
                X = -X;
                Y = -Y;
            }

            return new PointF((float)X, (float)Y);
        }
        public static double CalculateAreaOfPolygon(List<Point> points)
        {
            Point[] pointArray = points.ToArray();
            int num_points = pointArray.Length;
            // Get the areas.
            double area = 0;
            for (int i = 0, ip1; i < num_points; i++)
            {
                ip1 = i + 1;
                if (ip1 == num_points) ip1 = 0;
                area +=
                    (pointArray[ip1].X - pointArray[i].X) *
                    (pointArray[ip1].Y + pointArray[i].Y) / 2;
            }

            // Return the result.
            return area;
        }
        public static PointF CalculateCentroidOfPolygon(List<PointF> points)
        {
            int num_points = points.Count;
            if (points == null || num_points == 0)
                return Point.Empty;

            // Find the centroid.
            double X = 0;
            double Y = 0;
            float second_factor;
            for (int i = 0, ip1; i < num_points; i++)
            {
                ip1 = i + 1;
                if (ip1 == num_points) ip1 = 0;
                second_factor =
                    points[i].X * points[ip1].Y
                    - points[ip1].X * points[i].Y;
                X += (points[i].X + points[ip1].X) * second_factor;
                Y += (points[i].Y + points[ip1].Y) * second_factor;
            }

            // Divide by 6 times the polygon's area.
            double polygon_area = CalculateAreaOfPolygon(points);
            X /= (6 * polygon_area);
            Y /= (6 * polygon_area);

            // If the values are negative, the polygon is
            // oriented counterclockwise so reverse the signs.
            if (X < 0)
            {
                X = -X;
                Y = -Y;
            }

            return new PointF((float)X, (float)Y);
        }
        public static double CalculateAreaOfPolygon(List<PointF> points)
        {
            int num_points = points.Count;
            // Get the areas.
            double area = 0;
            for (int i = 0, ip1; i < num_points; i++)
            {
                ip1 = i + 1;
                if (ip1 == num_points) ip1 = 0;
                area +=
                    (points[ip1].X - points[i].X) *
                    (points[ip1].Y + points[i].Y) / 2;
            }

            // Return the result.
            return area;
        }

        #endregion

        #region check image blank sides, text image blank sides


        /// <summary>
        /// 检查图片的实际内容区域，例如图片四周是透明的，则返回有内容的最小区域；
        /// </summary>
        /// <param name="oriImage"></param>
        /// <param name="alphaThreshold">小于等于这个值的透明颜色将会被算作空白</param>
        /// <returns></returns>
        public static Rectangle CheckImageContents(Bitmap oriImage, int alphaThreshold = 0)
        {
            Rectangle result = Rectangle.Empty;
            if (oriImage == null)
                return result;
            bool allNotEmpty = false;
            int top = 0;
            for (int r = 0, rv = oriImage.Height, c, cv = oriImage.Width; r < rv; r++)
            {
                allNotEmpty = false;
                for (c = 0; c < cv; c++)
                {
                    if (oriImage.GetPixel(c, r).A > alphaThreshold)
                    {
                        allNotEmpty = true;
                        break;
                    }
                }
                if (allNotEmpty)
                {
                    top = r;
                    break;
                }
            }
            if (allNotEmpty == false)
                return result;

            int bottom = 0;
            for (int r = oriImage.Height - 1, rn = top, c, cv = oriImage.Width; r > rn; r--)
            {
                allNotEmpty = false;
                for (c = 0; c < cv; c++)
                {
                    if (oriImage.GetPixel(c, r).A > alphaThreshold)
                    {
                        allNotEmpty = true;
                        break;
                    }
                }
                if (allNotEmpty)
                {
                    bottom = oriImage.Height - r;
                    break;
                }
            }

            int left = 0;
            allNotEmpty = false;
            for (int c = 0, cv = oriImage.Width, r, rv = oriImage.Height - bottom; c < cv; c++)
            {
                allNotEmpty = false;
                for (r = top; r < rv; r++)
                {
                    if (oriImage.GetPixel(c, r).A > alphaThreshold)
                    {
                        allNotEmpty = true;
                        break;
                    }
                }
                if (allNotEmpty)
                {
                    left = c;
                    break;
                }
            }
            if (allNotEmpty == false)
                return result;

            int right = 0;
            for (int c = oriImage.Width - 1, cn = left, r, rv = oriImage.Height - bottom; c > cn; c--)
            {
                allNotEmpty = false;
                for (r = top; r < rv; r++)
                {
                    if (oriImage.GetPixel(c, r).A > alphaThreshold)
                    {
                        allNotEmpty = true;
                        break;
                    }
                }
                if (allNotEmpty)
                {
                    right = oriImage.Width - c;
                    break;
                }
            }
            result = new Rectangle(left, top, oriImage.Width - right - left, oriImage.Height - bottom - top);
            return result;
        }


        public static void CheckSizedTextImageContents(string text, Font font, float maxWidth, float maxHeight,
            out Font finalFont, out Rectangle imageRegion)
        {
            Graphics txMeasure = Graphics.FromHwnd(IntPtr.Zero);
            SizeF txSize = txMeasure.MeasureString(text, font);
            Bitmap testImg = new Bitmap((int)txSize.Width + 1, (int)txSize.Height + 1);
            using (System.Drawing.Brush b = new SolidBrush(Color.Black))
            {
                Graphics g = Graphics.FromImage(testImg);
                g.DrawString(text, font, b, 0, 0);
                Rectangle area = CheckImageContents(testImg);
                testImg.Dispose();
                g.Dispose();

                float ratioW = maxWidth / area.Width;
                float ratioH = maxHeight / area.Height;
                float ratio = Math.Min(ratioW, ratioH);

                finalFont = new Font(font.FontFamily, font.Size * ratio, font.Style);
                txSize = txMeasure.MeasureString(text, finalFont);
                txMeasure.Dispose();
                testImg = new Bitmap((int)txSize.Width + 1, (int)txSize.Height + 1);
                g = Graphics.FromImage(testImg);
                g.DrawString(text, finalFont, b, 0, 0);
                imageRegion = CheckImageContents(testImg);
                testImg.Dispose();
                g.Dispose();
            }
        }
        #endregion

        /// <summary>
        /// 二维坐标系变换，以3点定位方法，获取另一个坐标系对应的坐标点；
        /// </summary>
        public class SpaceChange2D
        {
            private System.Windows.Point s1p1, s1p2, s1p3, s2p1, s2p2, s2p3;

            /// <summary>
            /// 初始化，以成对的左边点，确定变换关系
            /// </summary>
            /// <param name="s1p1">第1坐标系，第1点</param>
            /// <param name="s1p2">第1坐标系，第2点</param>
            /// <param name="s1p3">第1坐标系，第3点</param>
            /// <param name="s2p1">第2坐标系，第1点</param>
            /// <param name="s2p2">第2坐标系，第2点</param>
            /// <param name="s2p3">第2坐标系，第3点</param>
            /// <param name=""></param>
            public SpaceChange2D(System.Windows.Point s1p1,
                System.Windows.Point s1p2,
                System.Windows.Point s1p3,
                System.Windows.Point s2p1,
                System.Windows.Point s2p2,
                System.Windows.Point s2p3)
            {
                this.s1p1 = s1p1;
                this.s1p2 = s1p2;
                this.s1p3 = s1p3;
                this.s2p1 = s2p1;
                this.s2p2 = s2p2;
                this.s2p3 = s2p3;
            }

            public bool CheckPointInsideTriangle(bool isS1orS2, System.Windows.Point testPt)
            {
                if (isS1orS2)
                {
                    return CheckPointInPolygon(new System.Windows.Point[] { s1p1, s1p2, s1p3 }, testPt);
                }
                else
                {
                    return CheckPointInPolygon(new System.Windows.Point[] { s2p1, s2p2, s2p3 }, testPt);
                }
            }
            public System.Windows.Point GetOppositePoint(bool isS1PointOrS2Point, System.Windows.Point surPt)
            {
                double cX23, cY23, cX13, cY13, cXp3, cYp3;
                double factorA, factorB;
                if (isS1PointOrS2Point)
                {
                    cX23 = s1p2.X - s1p3.X;
                    cY23 = s1p2.Y - s1p3.Y;
                    cX13 = s1p1.X - s1p3.X;
                    cY13 = s1p1.Y - s1p3.Y;
                    cXp3 = surPt.X - s1p3.X;
                    cYp3 = surPt.Y - s1p3.Y;
                }
                else
                {
                    cX23 = s2p2.X - s2p3.X;
                    cY23 = s2p2.Y - s2p3.Y;
                    cX13 = s2p1.X - s2p3.X;
                    cY13 = s2p1.Y - s2p3.Y;
                    cXp3 = surPt.X - s2p3.X;
                    cYp3 = surPt.Y - s2p3.Y;
                }
                factorA = cY23 * cXp3 - cYp3 * cX23;
                factorA /= cY23 * cX13 - cY13 * cX23;
                factorB = cXp3 * cY13 - cX13 * cYp3;
                factorB /= cX23 * cY13 - cX13 * cY23;

                double x, y;
                if (isS1PointOrS2Point)
                {
                    x = factorA * (s2p1.X - s2p3.X) + factorB * (s2p2.X - s2p3.X) + s2p3.X;
                    y = factorA * (s2p1.Y - s2p3.Y) + factorB * (s2p2.Y - s2p3.Y) + s2p3.Y;
                }
                else
                {
                    x = factorA * (s1p1.X - s1p3.X) + factorB * (s1p2.X - s1p3.X) + s1p3.X;
                    y = factorA * (s1p1.Y - s1p3.Y) + factorB * (s1p2.Y - s1p3.Y) + s1p3.Y;
                }

                return new System.Windows.Point(x, y);
            }
        }
    }
}
