using System;
using System.Collections.Generic;
using Cyclotron2D.Core;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Mod;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

//Temporary
using Cyclotron2D.Network;

namespace Cyclotron2D.Screens.Main
{
    /// <summary>
    /// Screen that runs the actual game
    /// </summary>
    public class GameScreen : MainScreen
    {
        private Engine m_engine;

        public List<Player> ActivePlayers { get { return m_engine.Players; } }

        public Settings GameSettings { get; set; }

        public TimeSpan GameStartTime { get { return m_engine.GameStart; } }

        private bool m_gameStarted;

        public GameScreen(Game game)
            : base(game, GameState.PlayingAsClient | GameState.PlayingSolo)
        {
            GameSettings = Settings.Current;
            m_engine = new Engine(game, this);
        }

        private void StartNewGame(IEnumerable<Player> players)
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
                GameSettings = Settings.Current;
            }
            else if (e.NewState == GameState.Hosting && e.OldState == GameState.WaitingForClients)
            {
                //get players from network and then start game
            }
            else if (e.NewState == GameState.PlayingAsClient && e.OldState == GameState.JoiningGame)
            {
                //get players and settings from network and start game
            }


            if (IsValidState && !m_gameStarted && players != null)
            {
                StartNewGame(players);
            }
        }
    }
}