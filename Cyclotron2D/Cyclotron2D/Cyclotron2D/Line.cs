using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Tron
{
	class Line
	{
		Vector2 Start { get; set; }
		Vector2 End { get; set; }

		public Line(Vector2 start, Vector2 end)
		{
			Start = start;
			End = end;
		}

		public static Vector2? FindIntersection(Line a, Line b)
		{
			Vector2 point1 = a.Start, point2 = a.End, point3 = b.Start, point4 = b.End;
			float ua = (point4.X - point3.X) * (point1.Y - point3.Y) - (point4.Y - point3.Y) * (point1.X - point3.X);
			float ub = (point2.X - point1.X) * (point1.Y - point3.Y) - (point2.Y - point1.Y) * (point1.X - point3.X);
			float denominator = (point4.Y - point3.Y) * (point2.X - point1.X) - (point4.X - point3.X) * (point2.Y - point1.Y);

			if (denominator == 0)
				return null;

			ua /= denominator;
			ub /= denominator;

			if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
			{
				Vector2 intersect = new Vector2();
				intersect.X = point1.X + ua * (point2.X - point1.X);
				intersect.Y = point1.Y + ua * (point2.Y - point1.Y);
				return intersect;
			}

			return null;
		}
	}
}
