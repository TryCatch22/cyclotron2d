using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cyclotron2D.Core.Players;

namespace Cyclotron2D.Core
{
    public enum CollisionType
    {
        Wall,
        Self,
        Player,
        Suicide
    }

    public class CycleCollisionEventArgs : EventArgs
    {
        public CollisionType Type { get; private set; }

        public Player Killer { get; private set; }

        public Player Victim { get; private set; }

        public bool AmbiguousCollision { get; private set; }

        public CycleCollisionEventArgs(CollisionType type, Player killer, bool ambiguous, Player victim)
        {
            Type = type;
            Killer = killer;
            Victim = victim;
            AmbiguousCollision = ambiguous;
        }
    }
}
