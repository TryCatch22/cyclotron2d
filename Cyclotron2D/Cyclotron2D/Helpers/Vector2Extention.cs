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

		public static float Orientation(this Vector2 v)
		{
			return (float)Math.Atan2(v.Y, v.X);
		}

        public static Vector2 FromString(string s)
        {
            int xs = s.IndexOf("X:") + 2, xe = s.IndexOf(' ');
            int ys = s.IndexOf("Y:") + 2, ye = s.IndexOf('}');
            string x = s.Substring(xs, xe-xs);
            string y = s.Substring(ys, ye-ys);

            return new Vector2(float.Parse(x), float.Parse(y));
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

        public static Point FromString(string s)
        {
            int xs = s.IndexOf("X:") + 2, xe = s.IndexOf(' ');
            int ys = s.IndexOf("Y:") + 2, ye = s.IndexOf('}');
            string x = s.Substring(xs, xe - xs);
            string y = s.Substring(ys, ye - ys);

            return new Point(int.Parse(x), int.Parse(y));
        }
    }

    public static class RectanlgeExtention
    {
        public static Vector2 Size(this Rectangle r)
        {
            return new Vector2(r.Width, r.Height);
        }
    }

	public static class ColorExtension
	{
		public static Vector3 ToHSV(this Color color)
		{
			Vector3 c = color.ToVector3();
			float v = Math.Max(c.X, Math.Max(c.Y, c.Z));
			float chroma = v - Math.Min(c.X, Math.Min(c.Y, c.Z));

			if (chroma == 0f)
				return new Vector3(0, 0, v);

			float s = chroma / v;

			if (c.X >= c.Y && c.Y >= c.Z)
			{
				float h = (c.Y - c.Z) / chroma;
				if (h < 0)
					h += 6;
				return new Vector3(h, s, v);
			}
			else if (c.Y >= c.Z && c.Y >= c.X)
				return new Vector3((c.Z - c.X) / chroma + 2, s, v);
			else
				return new Vector3((c.X - c.Y) / chroma + 4, s, v);

		}

		public static Color HSVToColor(this Vector3 hsv)
		{
			if (hsv.X == 0 && hsv.Y == 0)
				return new Color(hsv.Z, hsv.Z, hsv.Z);

			float c = hsv.Y * hsv.Z;
			float x = c * (1 - Math.Abs(hsv.X % 2 - 1));
			float m = hsv.Z - c;

			if (hsv.X < 1) return new Color(c + m, x + m, m);
			else if (hsv.X < 2) return new Color(x + m, c + m, m);
			else if (hsv.X < 3) return new Color(m, c + m, x + m);
			else if (hsv.X < 4) return new Color(m, x + m, c + m);
			else if (hsv.X < 5) return new Color(x + m, m, c + m);
			else return new Color(c + m, m, x + m);
		}
	}


}