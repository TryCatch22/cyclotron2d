using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Cyclotron2D
{
	public enum IntersectionType { None, Collinear, Point }

	class Line
	{
		Vector2 Start { get; set; }
		Vector2 End { get; set; }

		public Line(Vector2 start, Vector2 end)
		{
			Start = start;
			End = end;
		}

		public static IntersectionType FindIntersection(Line a, Line b, out Vector2? intersection)
		{
			intersection = null;

			Vector2 point1 = a.Start, point2 = a.End, point3 = b.Start, point4 = b.End;
			float ua = (point4.X - point3.X) * (point1.Y - point3.Y) - (point4.Y - point3.Y) * (point1.X - point3.X);
			float ub = (point2.X - point1.X) * (point1.Y - point3.Y) - (point2.Y - point1.Y) * (point1.X - point3.X);
			float denominator = (point4.Y - point3.Y) * (point2.X - point1.X) - (point4.X - point3.X) * (point2.Y - point1.Y);

			if (denominator == 0)
			{
				if (ua == 0 && ub == 0)
					if (AreProjectionsIntersecting(point1, point2, point3, point4))
						return IntersectionType.Collinear;
				return IntersectionType.None;
			}

			ua /= denominator;
			ub /= denominator;

			if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
			{
				intersection = new Vector2()
				{
					X = point1.X + ua * (point2.X - point1.X),
					Y = point1.Y + ua * (point2.Y - point1.Y)
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

		private static bool AreProjectionsIntersecting(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4)
		{
			float x1 = Math.Abs(point1.X), x2 = Math.Abs(point2.X), x3 = Math.Abs(point3.X), x4 = Math.Abs(point4.X);
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

			return ((x1 <= x3 && x2 >= x3) || (x1 <= x4 && x2 >= x4)) ;
		}

		private static void Swap(ref float a, ref float b)
		{
			float temp = a;
			a = b;
			b = temp;
		}
	}
}
