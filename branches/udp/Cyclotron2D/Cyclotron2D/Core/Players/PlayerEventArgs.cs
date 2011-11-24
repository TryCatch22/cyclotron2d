using System;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Core.Players
{
    public class DirectionChangeEventArgs : EventArgs
    {
        public DirectionChangeEventArgs(Direction direction, Point position)
        {
            Position = position;
            Direction = direction;
        }

        public Point Position { get; private set; }

        public Direction Direction { get; private set; }
    }
}