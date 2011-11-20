using System;
using System.Collections.Generic;
using System.Linq;
using Cyclotron2D.Core;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Mod;
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

        public List<Player> ActivePlayers { get { return m_engine.Players; } }

        public List<RemotePlayer> RemotePlayers { get { return m_engine.Players.Where(player => player is RemotePlayer).Select( player => player as RemotePlayer).ToList(); } }

        private List<RemotePlayer> m_remotePlayers;

        public Settings GameSettings { get; set; }

        public TimeSpan GameStartTime { get { return m_engine.GameStart; } }

        private bool m_gameStarted;

        public GameScreen(Game game)
            : base(game, GameState.PlayingAsClient | GameState.PlayingSolo | GameState.PlayingAsHost)
        {
            GameSettings = Settings.SinglePlayer;
            m_remotePlayers = new List<RemotePlayer>();
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
            m_remotePlayers.Clear();
        }

        public override void Draw(GameTime gameTime)
        {
            if (m_engine != null && m_engine.Visible)
            {
                m_engine.Draw(gameTime);
            }
        }

        public void AddRemotePlayer(RemotePlayer player)
        {
            m_remotePlayers.Add(player);
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

            //game screen disabled if not in its own state
            Enabled = IsValidState;


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
                int i = 0;
                foreach (var player in players)
                {
                    player.PlayerID = ++i;
                }

                GameSettings = Settings.SinglePlayer;
            }
            else if (e.NewState == GameState.PlayingAsHost && e.OldState == GameState.GameLobbyHost)
            {
                //get players from network and then start game
                players = new List<Player>();
                players.AddRange(m_remotePlayers);

                GameSettings = Settings.Multiplayer;
            }
            else if (e.NewState == GameState.PlayingAsClient && e.OldState == GameState.GameLobbyClient)
            {
                //get players and settings from network and start game
                players = new List<Player>();
                players.AddRange(m_remotePlayers);
                GameSettings = Settings.Multiplayer;
            }


            if (IsValidState && !m_gameStarted && players != null)
            {
                StartNewGame(players);
            }
        }
    }
}