using System;
using Cyclotron2D.Components;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.Screens.Main;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Core.Players
{
    /// <summary>
    /// Players provide events that the game engine listens to in order to simulate the game. 
    /// This is be base class for all types of players.
    /// </summary>
    public abstract class Player : ScreenComponent
    {

        private static ColorMap s_map = new ColorMap();

        protected Player(Game game, Screen screen) : base(game, screen)
        {
        }

        public abstract string Name { get; set; }

        public Cycle Cycle { get; protected set; }

        public Color Color { get {return s_map[PlayerID];}}

        /// <summary>
        /// Unique player ID.
        /// There must be an identical mapping from PlayerID's to people on all connected machines
        /// </summary>
        public int PlayerID { get; set; }

        public bool Winner { get; set; }

        public TimeSpan SurvivalTime { get; set; }

        #region Events

        public event EventHandler<DirectionChangeEventArgs> DirectionChange;

        protected void InvokeDirectionChange(DirectionChangeEventArgs e)
        {
            EventHandler<DirectionChangeEventArgs> handler = DirectionChange;
            if (handler != null) handler(this, e);
        }

        #endregion


        public override string ToString()
        {
            return Name + "(" + PlayerID + ")";
        }

        public virtual void Initialize(Cycle cycle)
        {
            Winner = false;
            Cycle = cycle;

            SubscribeCycle();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if(Cycle.Enabled)SurvivalTime = gameTime.TotalGameTime - (Screen as GameScreen).GameStartTime;

        }

        protected virtual void OnCycleEnabledChanged(object sender, EventArgs e)
        {
        }

        protected void SubscribeCycle()
        {
            Cycle.EnabledChanged += OnCycleEnabledChanged;
        }

        private void UnsubscribeCycle()
        {
            Cycle.EnabledChanged += OnCycleEnabledChanged;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Cycle != null)
            {
                UnsubscribeCycle();
            }
            base.Dispose(disposing);
        }
    }
}