using System;
using System.Diagnostics;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.Screens.Main;
using Cyclotron2D.Screens.Popup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Cyclotron2D.Core.Players
{
    /// <summary>
    /// local player, Events are generated from parsing input.
    /// </summary>
    public class LocalPlayer : Player
    {

        private bool m_gameEnded;

        public LocalPlayer(Game game, Screen screen) : base(game, screen)
        {
        }

        public override string Name { get { return (Screen as GameScreen).GameSettings.PlayerName.Value; } set { } }

        public override void Initialize(Cycle cycle)
        {
            base.Initialize(cycle);
            SubscribeCycleCollision();
        }

        protected override void HandleInupt(GameTime gameTime)
        {
            base.HandleInupt(gameTime);
            Debug.Assert(Cycle != null, "Player has not been initialized.");

            InputState input = Game.InputState;

            var pos = Cycle.GetNextGridCrossing();
            if (input.IsNewKeyPress(Keys.Up))
            {
                InvokeDirectionChange(new DirectionChangeEventArgs(Direction.Up, pos));
            }
            else if (input.IsNewKeyPress(Keys.Down))
            {
                InvokeDirectionChange(new DirectionChangeEventArgs(Direction.Down, pos));
            }
            else if (input.IsNewKeyPress(Keys.Left))
            {
                InvokeDirectionChange(new DirectionChangeEventArgs(Direction.Left, pos));
            }
            else if (input.IsNewKeyPress(Keys.Right))
            {
                InvokeDirectionChange(new DirectionChangeEventArgs(Direction.Right, pos));
            }
        }



        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Winner && !m_gameEnded)
            {
                Game.ScreenManager.AddScreen(new EndGamePopup(Game, Screen as MainScreen, "Victory"));
                m_gameEnded = true;
            }
        }

        #region Subscription

        private void SubscribeCycleCollision()
        {
            Cycle.Collided += OnCollision;
        }

        private void UnsubscribeCycleCollision()
        {
            Cycle.Collided -= OnCollision;
        }

        #endregion

        #region Event Handlers

        private void OnCollision(object sender, EventArgs e)
        {
            if (!m_gameEnded)
            {
                Game.ScreenManager.AddScreen(new EndGamePopup(Game, Screen as MainScreen, "Game Over"));
                m_gameEnded = true;
            }
            
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing && Cycle != null)
            {
                UnsubscribeCycleCollision();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}