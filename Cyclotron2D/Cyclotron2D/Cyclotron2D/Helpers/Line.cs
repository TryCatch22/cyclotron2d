using System;
using Cyclotron2D.Core;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Cyclotron2D.Helpers
{
    public enum IntersectionType
    {
        None,
        Collinear,
        Point
    }

    public class Line
    {
        public Line(Point start, Point end)
        {
            Start = start;
            End = end;

        }

        public Orientation Orientation { get { return Start.X == End.X ? Orientation.Vertical : Orientation.Horizontal; }}

        public Direction Direction
        {
            get
            {
                if(Orientation == Orientation.Horizontal)
                {
                    Debug.Assert(End.X != Start.X, "same position!!");
                    return End.X > Start.X ? Direction.Right : Direction.Left;
                }
                Debug.Assert(End.Y != Start.Y, "same position!!");
                return End.Y > Start.Y ? Direction.Down : Direction.Up; 
            }
        }

        public Point Start { get; set; }
        public Point End { get; set; }
        public int Length { get { return (int)Start.Distance(End); } }

        public Line Clone()
        {
            return new Line(Start, End);
        }

        public static IntersectionType FindIntersection(Line a, Line b, out Vector2? intersection)
        {
            intersection = null;


            Point point1 = a.Start, point2 = a.End, point3 = b.Start, point4 = b.End;
            int ua = (point4.X - point3.X)*(point1.Y - point3.Y) - (point4.Y - point3.Y)*(point1.X - point3.X);
            int ub = (point2.X - point1.X)*(point1.Y - point3.Y) - (point2.Y - point1.Y)*(point1.X - point3.X);
            int denominator = (point4.Y - point3.Y)*(point2.X - point1.X) - (point4.X - point3.X)*(point2.Y - point1.Y);

            if (denominator == 0)
            {
                if (ua == 0 && ub == 0)
                    if (AreProjectionsIntersecting(point1, point2, point3, point4))
                        return IntersectionType.Collinear;
                return IntersectionType.None;
            }

            float fa = ua/(float)denominator;
            float fb = ub / (float)denominator;

            if (fa >= 0 && fa <= 1 && fb >= 0 && fb <= 1)
            {
                intersection = new Vector2
                                   {
                                       X = point1.X + fa*(point2.X - point1.X),
                                       Y = point1.Y + fa*(point2.Y - point1.Y)
                                   };
                return IntersectionType.Point;
            }

            return IntersectionType.None;
        }



        public static IntersectionType FindIntersection(Line a, Line b)
        {
            Vector2? intersection = null;
            return FindIntersection(a, b, out intersection);
        }

        private static bool AreProjectionsIntersecting(Point point1, Point point2, Point point3, Point point4)
        {
            int x1 = Math.Abs(point1.X), x2 = Math.Abs(point2.X), x3 = Math.Abs(point3.X), x4 = Math.Abs(point4.X);
            if (x1 == x2)
            {
                // If the lines are perfectly along the Y-axis.
                x1 = Math.Abs(point1.Y);
                x2 = Math.Abs(point2.Y);
                x3 = Math.Abs(point3.Y);
                x4 = Math.Abs(point4.Y);
            }

            if (x1 > x2)
                Swap(ref x1, ref x2);
            if (x3 > x4)
                Swap(ref x3, ref x4);

            return ((x1 <= x3 && x2 >= x3) || (x1 <= x4 && x2 >= x4));
        }

        public override bool Equals(object obj)
        {
            var line = obj as Line;
            if (line != null)
            {
                return line.Start == Start && line.End == End;
            }
            return false;
        }

        private static void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }
    }
}