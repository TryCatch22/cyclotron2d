using System;
using System.Diagnostics;
using Cyclotron2D.Mod;
using Cyclotron2D.Network;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.Screens.Main;
using Cyclotron2D.Screens.Popup;
using Cyclotron2D.State;
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
            Name = Settings.SinglePlayer.PlayerName.Value;

        }

        private GameScreen GameScreen { get { return Screen as GameScreen; } }

        public override string Name { get; set; }

        public override void Initialize(Cycle cycle)
        {
            base.Initialize(cycle);
            SubscribeCycleCollision();
            Ready = true;
        }

        protected override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);

            if (Cycle == null || gameTime.TotalGameTime < Cycle.GameStart) return;

           // Debug.Assert(Cycle != null, "Player has not been initialized.");

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
            if (Winner && !m_gameEnded && gameTime.TotalGameTime > Cycle.GameStart)
            {
                Game.ScreenManager.AddScreen(new EndGamePopup(Game, Screen as MainScreen, "Victory"));
                m_gameEnded = true;
            }
            else if (Cycle != null && !m_gameEnded && gameTime.TotalGameTime > Cycle.GameStart)
            {
                NotifyPeers();
            }
        }


        #region Private Methods


        private void NotifyPeers()
        {
            if (!GameScreen.UseUdp) return;
            Game.Communicator.MessageAll(Cycle.GetInfoMessage());

        }
           
        
        #endregion

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