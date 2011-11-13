using System;
using Cyclotron2D.Components;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Core.Players
{
    /// <summary>
    /// Players provide events that the game engine listens to in order to simulate the game. 
    /// This is be base class for all types of players.
    /// </summary>
    public abstract class Player : ScreenComponent
    {
        protected Player(Game game, Screen screen) : base(game, screen)
        {
        }

        public abstract string Name { get; set; }

        public Cycle Cycle { get; protected set; }

        /// <summary>
        /// Unique player ID.
        /// There must be an identical mapping from PlayerID's to people on all connected machines
        /// </summary>
        public int PlayerID { get; private set; }

        public bool Winner { get; set; }

        #region Events

        public event EventHandler<DirectionChangeEventArgs> DirectionChange;

        protected void InvokeDirectionChange(DirectionChangeEventArgs e)
        {
            EventHandler<DirectionChangeEventArgs> handler = DirectionChange;
            if (handler != null) handler(this, e);
        }

        #endregion

        public virtual void Initialize(Cycle cycle, int id)
        {
            Winner = false;
            Cycle = cycle;
            PlayerID = id;
        }
    }
}