using System;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Helpers
{
    public static class Vector2Extention
    {
        public static Vector2 Round(this Vector2 v)
        {
            return new Vector2((float) Math.Round(v.X), (float) Math.Round(v.Y));
        }

        public static float Distance(this Vector2 v, Vector2 u)
        {
            return (float) Math.Sqrt(Math.Pow(v.X - u.X, 2) + Math.Pow(v.Y - u.Y, 2));
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