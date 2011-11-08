using System;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Helpers
{
    public static class Vector2Extention
    {
        public static Point RoundToPoint(this Vector2 v)
        {
            return new Point((int) Math.Round(v.X), (int) Math.Round(v.Y));
        }

        public static float Distance(this Vector2 v, Vector2 u)
        {
            return (float) Math.Sqrt(Math.Pow(v.X - u.X, 2) + Math.Pow(v.Y - u.Y, 2));
        }

        public static float Distance(this Vector2 v, Point u)
        {
            return (float)Math.Sqrt(Math.Pow(v.X - u.X, 2) + Math.Pow(v.Y - u.Y, 2));
        }
    }

    public static class PointExtention
    {
        public static int LengthSquared(this Point p)
        {
            return (int) (Math.Pow(p.X, 2) + Math.Pow(p.Y, 2));
        }

        public static Vector2 ToVector(this Point p)
        {
            return new Vector2(p.X, p.Y);
        }

        public static float Distance(this Point v, Point u)
        {
            return (float)Math.Sqrt(Math.Pow(v.X - u.X, 2) + Math.Pow(v.Y - u.Y, 2));
        }

        public static float Distance(this Point v, Vector2 u)
        {
            return (float)Math.Sqrt(Math.Pow(v.X - u.X, 2) + Math.Pow(v.Y - u.Y, 2));
        }
    }

    public static class RectanlgeExtention
    {
        public static Vector2 Size(this Rectangle r)
        {
            return new Vector2(r.Width, r.Height);
        }
    }


}