using Cyclotron2D.Components;
using Cyclotron2D.State;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Screens.Base
{
    /// <summary>
    /// A Visible Slice of the game
    /// contains components that can be drawn and/or handle input
    /// Only 1 Screen can have focus at the same time
    /// </summary>
    public abstract class Screen : DrawableCyclotronComponent
    {
        protected Screen(Game game) : base(game)
        {
            SubscribeGame();
        }

        /// <summary>
        /// Indicates wether this screen has focus, and therefore
        /// if its components get to handle input
        /// </summary>
        public bool HasFocus { get; internal set; }

        private void SubscribeGame()
        {
            Game.StateChanged += OnStateChanged;
        }

        private void UnsubscribeGame()
        {
            Game.StateChanged += OnStateChanged;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (HasFocus)
            {
                HandleInupt(gameTime);
            }
        }

        protected virtual void HandleInupt(GameTime gameTime)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeGame();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Override this if your screen needs to do any 
        /// non standard state change related logic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnStateChanged(object sender, StateChangedEventArgs e)
        {
        }
    }
}