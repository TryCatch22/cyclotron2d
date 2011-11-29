﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cyclotron2D.Core;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Helpers;
using Cyclotron2D.Mod;
using Cyclotron2D.Network;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.State;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Screens.Main
{
    /// <summary>
    /// Screen that runs the actual game
    /// </summary>
    public class GameScreen : MainScreen
    {
        private Engine m_engine;

        private StartRandomizer m_startRandomizer;

        public List<Player> ActivePlayers { get { return m_engine.Players; } }

        private List<Player> m_lobbyPlayers;

        private bool isGameSetup;

        public Settings GameSettings { get; set; }

        public TimeSpan GameStartTime { get { return m_engine.GameStartTime; } }

        public NetworkMessage SetupMessage { get; set; }

        private bool m_gameStarted;

        public GameScreen(Game game)
            : base(game, GameState.PlayingAsClient | GameState.PlayingSolo | GameState.PlayingAsHost)
        {
            GameSettings = Settings.SinglePlayer;
            m_lobbyPlayers = new List<Player>();
            m_engine = new Engine(game, this);
            m_startRandomizer = new StartRandomizer(game);
            isGameSetup = false;
        }



        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (isGameSetup && ActivePlayers.Aggregate(true, (ready, player) => ready && player.Ready))
            {
                StartGame();
            }
        }

        public Player GetPlayer(int id)
        {
            return (from player in ActivePlayers where player.PlayerID == id select player).FirstOrDefault();
        }

        #region Subscription

        private void SubscribeCommunicator()
        {
            Game.Communicator.MessageReceived += OnMessageReceived;
        }

        private void UnsubscribeCommunicator()
        {
            Game.Communicator.MessageReceived -= OnMessageReceived;
        }



        #endregion

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {

            switch (Game.State)
            {
                case GameState.PlayingAsHost:
                    {
                        if (e.Message.Type == MessageType.Ready)
                        {
                            Player player = GetPlayer(e.Message.Source);
                            if (player != null)
                            {
                                player.Ready = true;
                            }

                            //if all players are ready
                            if (ActivePlayers.Aggregate(true, (ready, p) => ready && p.Ready))
                            {
                                Game.Communicator.MessageAll(new NetworkMessage(MessageType.AllReady, ""));
                            }

                        }

                       
                    }
                    break;
                case GameState.PlayingAsClient:
                    {
                        if (e.Message.Type == MessageType.AllReady)
                        {
                            foreach (var activePlayer in ActivePlayers)
                            {
                                activePlayer.Ready = true;
                            }
                        }
                    }
                    break;
            }

         

        }

        private void StartGame()
        {
            if (IsValidState && !m_gameStarted && ActivePlayers.Count > 0)
            {
                m_engine.StartGame();
                m_gameStarted = true;
            }
        }

        private static int ComparePlayerID(Player x, Player y)
        {
            return x.PlayerID - y.PlayerID;
        }

        private void SetupGame(List<Player> players)
        {
            Sorter.Sort(players, ComparePlayerID);

            if (m_engine != null)
            {
                m_engine.Dispose();
            }

            m_engine = new Engine(Game, this);


            switch (Game.State)
            {
                case GameState.PlayingSolo:
                    {
                        m_startRandomizer.Randomize(players.Count(), m_engine.Grid.PixelsPerInterval);

                        m_engine.SetupGame(players, m_startRandomizer.StartConditions);
                        foreach (var activePlayer in ActivePlayers)
                        {
                            activePlayer.Ready = true;
                        }
                    }
                    break;
                case GameState.PlayingAsHost:
                    {
                        m_startRandomizer.Randomize(players.Count(), m_engine.Grid.PixelsPerInterval);
                        m_engine.SetupGame(players, m_startRandomizer.StartConditions);

                        string content = "";

                        foreach (StartCondition startCondition in m_startRandomizer.StartConditions)
                        {
                            content += (int) startCondition.Dir + " " + startCondition.Position + "\n";
                        }

                        Game.Communicator.MessageAll(new NetworkMessage(MessageType.SetupGame, content));


                    }
                    break;
                case GameState.PlayingAsClient:
                    {
                        Debug.Assert(SetupMessage != null, "there should be a setup message ready for client play.");

                        var lines = SetupMessage.ContentLines;

                        List<StartCondition> conditions = new List<StartCondition>();

                        foreach (string line in lines)
                        {
                            //var condition = line.Split(new char[] {' '});

                            int sep = line.IndexOf(' ');

                            string direction = line.Substring(0, sep);
                            string vector = line.Substring(sep + 1);


                            conditions.Add(new StartCondition(Vector2Extention.FromString(vector), (Direction)int.Parse(direction)));
                        }

                        m_engine.SetupGame(players, conditions);

                        Game.Communicator.MessagePlayer(Game.Communicator.Host, new NetworkMessage(MessageType.Ready, ""));
                        
                    }
                    break;
                    
            }

            isGameSetup = true;

        }

        public void StopGame()
        {
            m_gameStarted = false;
            isGameSetup = false;
            m_lobbyPlayers.Clear();
            UnsubscribeCommunicator();
        }

        public override void Draw(GameTime gameTime)
        {
            if (m_engine != null && m_engine.Visible)
            {
                m_engine.Draw(gameTime);
            }
        }

        public void AddPlayer(Player player)
        {
            m_lobbyPlayers.Add(player);
        }


        public void RemovePlayer(Player player)
        {
            m_lobbyPlayers.Remove(player);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && m_engine != null)
            {
                m_engine.Dispose();
                m_engine = null;
                UnsubscribeCommunicator();
            }
            base.Dispose(disposing);
        }



        protected override void OnStateChanged(object sender, StateChangedEventArgs e)
        {
            base.OnStateChanged(sender, e);

            //game screen disabled if not in its own state
            Enabled = IsValidState;

            if (!Enabled) return;

            if (Game.IsState(GameState.PlayingAsClient | GameState.PlayingAsHost))
            {
                SubscribeCommunicator();
            }

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

                SetupGame(players);
            }
            else if ( Game.IsState(GameState.PlayingAsHost | GameState.PlayingAsClient))
            {
                //get players from network and then start game
                players = new List<Player>();
                players.AddRange(m_lobbyPlayers);
                GameSettings = Settings.SinglePlayer;
                SetupGame(players);

            }
//            else if (e.NewState == GameState.PlayingAsClient && e.OldState == GameState.GameLobbyClient)
//            {
                //get players and settings from network and start game
//                players = new List<Player>();
//                players.AddRange(m_lobbyPlayers);
//                SetupGame(players);
//                GameSettings = Settings.Multiplayer;
//            }


        }
    }
}