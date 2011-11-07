using System.Collections.Generic;
using Cyclotron2D.Core;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Screens.Main
{
    /// <summary>
    /// Screen that runs the actual game
    /// </summary>
    public class GameScreen : MainScreen
    {
        private Engine m_engine;

        private bool m_gameStarted;

        public GameScreen(Game game)
            : base(game, (int) (GameState.Hosting | GameState.PlayingAsClient | GameState.PlayingSolo))
        {
            m_engine = new Engine(game, this);
        }

        public void StartNewGame(List<Player> players)
        {
            if (m_engine != null)
            {
                m_engine.Dispose();
            }

            m_engine = new Engine(Game, this);

            m_engine.StartGame(players);
            m_gameStarted = true;
        }

        public void StopGame()
        {
            m_gameStarted = false;
        }

        public override void Draw(GameTime gameTime)
        {
            if (m_engine != null && m_engine.Visible)
            {
                m_engine.Draw(gameTime);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && m_engine != null)
            {
                m_engine.Dispose();
                m_engine = null;
            }
            base.Dispose(disposing);
        }

        protected override void OnStateChanged(object sender, StateChangedEventArgs e)
        {
            base.OnStateChanged(sender, e);
            if (m_gameStarted)
                return;

            List<Player> players = null;

            if (e.NewState == GameState.PlayingSolo && e.OldState == GameState.MainMenu)
            {
                players = new List<Player>
                              {
                                  new LocalPlayer(Game, this),
                                  new AIPlayer(Game, this),
                                  new AIPlayer(Game, this),
                                  new AIPlayer(Game, this),
                                  new AIPlayer(Game, this),
                                  new AIPlayer(Game, this),
                              };
            }
            else if (e.NewState == GameState.Hosting && e.OldState == GameState.WaitingForClients)
            {
                //get players from network and then start game
            }
            else if (e.NewState == GameState.PlayingAsClient && e.OldState == GameState.SearchingForHost)
            {
                //get players from network and start game
            }


            if (IsValidState && !m_gameStarted && players != null)
            {
                StartNewGame(players);
            }
        }
    }
}