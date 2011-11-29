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


        /// <summary>
        /// sends notifications about bike state every NotifyPeriod gameloops for Udp
        /// </summary>
        public const int NotifyPeriod = 5;

        private int m_currentPeriod = 0;

        public LocalPlayer(Game game, Screen screen) : base(game, screen)
        {
            Name = Settings.SinglePlayer.PlayerName.Value;
        }


        public override string Name { get; set; }

        public override void Initialize(Cycle cycle)
        {
            base.Initialize(cycle);
            Ready = true;
        }

        protected override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);

            if (Cycle == null || gameTime.TotalGameTime < Cycle.GameStart) return;

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
            m_currentPeriod = (m_currentPeriod + 1)%(NotifyPeriod);
            if (Winner && !m_gameEnded && gameTime.TotalGameTime > Cycle.GameStart)
            {
                Game.ScreenManager.AddScreen(new EndGamePopup(Game, Screen as MainScreen, "Victory"));
                m_gameEnded = true;
            }
            
            if (Cycle != null && !m_gameEnded && gameTime.TotalGameTime > Cycle.GameStart && m_currentPeriod == 0 && Game.IsState(GameState.PlayingAsClient | GameState.PlayingAsHost))
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


        #endregion

        #region Event Handlers

        protected override void OnCycleCollided(object sender, CycleCollisionEventArgs e)
        {
            base.OnCycleCollided(sender, e);
            if (Game.IsState(GameState.PlayingAsClient | GameState.PlayingAsHost))
            {
                switch (e.Type)
                {
                    case CollisionType.Self:
                    case CollisionType.Suicide:
                    case CollisionType.Wall:
                        {
                            //we killed ourselves some way or another. we are the authoritative source for these cases
                            GameScreen.CollisionNotifier.NotifyRealDeath(this);
                            Cycle.Kill();
                        }
                        break;
                    case CollisionType.Player:
                        {
                            if (!e.AmbiguousCollision)
                            {
                                //we collided into a confirmed portion of the other players tail
                                GameScreen.CollisionNotifier.NotifyRealDeath(this);
                                Cycle.Kill();
                                
                            }
                        }
                        break;
                    default:
                        break;
                }

            }





            if (!m_gameEnded)
            {
                Game.ScreenManager.AddScreen(new EndGamePopup(Game, Screen as MainScreen, "Game Over"));
                m_gameEnded = true;
            }
            
        }

       
        #endregion
    }
}