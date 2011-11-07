using System;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Core.Players
{
    public class DirectionChangeEventArgs : EventArgs
    {
        public DirectionChangeEventArgs(Direction direction, Vector2 position)
        {
            Position = position;
            Direction = direction;
        }

        public Vector2 Position { get; private set; }

        public Direction Direction { get; private set; }
    }
}