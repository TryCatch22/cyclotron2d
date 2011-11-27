using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cyclotron2D.Core;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Helpers
{
    public static class PointOffset
    {


        public static Point AddOffset(this Point p, Direction dir, int dist)
        {
            switch (dir)
            {
                case Direction.Up:
                    return new Point(p.X, p.Y - dist);
                case Direction.Down:
                    return new Point(p.X, p.Y + dist);
                case Direction.Left:
                    return new Point(p.X - dist, p.Y);
                case Direction.Right:
                    return new Point(p.X + dist, p.Y);
                default:
                    throw new Exception("Is there a 5th direction?");
            }
        }
    }
}
