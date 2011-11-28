﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Helpers;
using Cyclotron2D.Mod;
using Cyclotron2D.Network;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.State;
using Cyclotron2D.UI;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Screens.Main
{

    public class GameLobbyScreen : MainScreen
    {
        private PlayerPanel m_playersPanel;

        private Button LeaveButton;
        private Button CloseButton;
        private Button StartGameButton;
		private Button UDPButton;


        private List<Player> Players { get; set; } 

        public GameLobby Lobby { get; private set; }

        private GameScreen GameScreen { get; set; }

        public GameLobbyScreen(Game game)
            : base(game, GameState.GameLobbyHost | GameState.GameLobbyClient)
        {
            Players = new List<Player>();
            m_playersPanel = new PlayerPanel(game, this);

            LeaveButton = new Button(game, this);
            StartGameButton = new Button(game, this);
            CloseButton = new Button(game, this);
			UDPButton = new Button(game, this);
        }

        public Player GetPlayer(int id)
        {
            return (from player in Players where player.PlayerID == id select player).FirstOrDefault();
        }


        public override void Initialize()
        {
            base.Initialize();

            m_playersPanel.Background = new Color(20, 0, 90);


            LeaveButton.Click += OnLeaveButtonClicked;
            LeaveButton.Text = "Leave";


            CloseButton.Click += OnCloseButtonClicked;
            CloseButton.Text = "Close Lobby";

            StartGameButton.Text = "Start Game";
            StartGameButton.Click += OnStartGameClicked;

			UDPButton.Text = "UDP!!!";
			UDPButton.Click += OnUDPClicked;

            GameScreen = Game.ScreenManager.GetMainScreen<GameScreen>() as GameScreen;

            Debug.Assert(GameScreen != null, "GameScreen should not be null at Initialize.");

            SubscribeCommunicator();

        }



        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Rectangle win = GraphicsDevice.Viewport.Bounds;

            CloseButton.Visible = CloseButton.Enabled = Game.State == GameState.GameLobbyHost;
            StartGameButton.Visible = StartGameButton.Enabled = Game.State == GameState.GameLobbyHost;
			UDPButton.Visible = true;

            m_playersPanel.Rect = RectangleBuilder.TopLeft(win, new Vector2(0.35f, 0.5f), new Point(15, 15));

            LeaveButton.Rect = RectangleBuilder.BottomRight(win, new Vector2(0.15f, 0.1f), new Point(20, 10));
            CloseButton.Rect = RectangleBuilder.BottomRight(win, new Vector2(0.15f, 0.1f), new Point(25 + LeaveButton.Rect.Width, 10));
            StartGameButton.Rect = RectangleBuilder.Right(win, new Vector2(0.2f, 0.15f), 30);
			UDPButton.Rect = RectangleBuilder.Right(win, new Vector2(0.2f, 0f), 30);

        }


        /// <summary>
        /// this will also disconnect the player's connection
        /// </summary>
        /// <param name="player"></param>
        public void RemovePlayer(Player player)
        {
            if (player is RemotePlayer)
            {
                Game.Communicator.Remove(player as RemotePlayer);
            }
            GameScreen.RemovePlayer(player);
            m_playersPanel.RemovePlayer(player);
            Players.Remove(player);
        }

        /// <summary>
        /// for use when leaving a game lobby so that we can return to a clean one next time
        /// </summary>
        private void CancelGame()
        {
            if(Lobby != null)
            {
                Lobby.Dispose();
                Lobby = null;
            }


            foreach (var player in m_playersPanel.Players)
            {
                RemovePlayer(player);

                player.Dispose();

            }
        }

        /// <summary>
        /// for use when starting a new game
        /// </summary>
        private void Cleanup()
        {
            if(Lobby != null)
            {
                Lobby.ClearSockets();
                Lobby.CloseGameLobby();
            }


            foreach (var player in m_playersPanel.Players)
            {
                m_playersPanel.RemovePlayer(player);
                Players.Remove(player);
            }
        }

        #region Client Side

        public void AddHost(RemotePlayer player, NetworkConnection connection)
        {
            GameScreen.AddPlayer(player);
            Game.Communicator.AddHost(player, connection);
            m_playersPanel.AddPlayer(player);
            Players.Add(player);
            
       }
        

        #endregion

        #region Server Side

        private void SubscribeLobby()
        {
            Lobby.NewConnection += OnNewConnection;
        }

        private void OnStartGameClicked(object sender, EventArgs e)
        {
            Game.ChangeState(GameState.PlayingAsHost);
        }

		private void OnUDPClicked(object sender, EventArgs e)
		{
			
		}

        private void OnNewConnection(object sender, SocketEventArgs e)
        {
            var rem = new RemotePlayer(Game, GameScreen) {PlayerID = Players.Count + 1};
            AddPlayer(rem, e.Socket);
            rem.SubscribeConnection();
            string content = rem.PlayerID + "\n" + Settings.SinglePlayer.PlayerName.Value;
            Game.Communicator.MessagePlayer(rem, new NetworkMessage(MessageType.Welcome, content));
            
        }

        /// <summary>
        /// add client player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="socket"></param>
        public void AddPlayer(Player player, Socket socket = null)
        {
            if (player is RemotePlayer && socket != null)
            {
                Game.Communicator.Add(player as RemotePlayer, socket);
            }
            GameScreen.AddPlayer(player);
            m_playersPanel.AddPlayer(player);
            Players.Add(player);
        }

        private void OnCloseButtonClicked(Object sender, EventArgs e)
        {
            Lobby.CloseGameLobby();
        }

        #endregion

        #region Event Handlers

        private void OnLeaveButtonClicked(Object sender, EventArgs e)
        {
            CancelGame();
            Game.ChangeState(GameState.MainMenu);
        }


        /// <summary>
        ///  Starts the GameLobby instance and spawns threads to accept connections accordingly.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnStateChanged(object sender, StateChangedEventArgs e)
        {
            base.OnStateChanged(sender, e);


            switch (Game.State)
            {
                case GameState.GameLobbyHost:
                    if (Lobby == null)
                    {
                        Lobby = new GameLobby(Game);
                        SubscribeLobby();
                    }
                    Lobby.Start();
                    AddPlayer(new LocalPlayer(Game, GameScreen){PlayerID = 1});
                    Game.Communicator.LocalId = 1;
                    break;
                case GameState.GameLobbyClient:
                    //nothing here yet
                    break;
                case GameState.PlayingAsHost:
                case GameState.PlayingAsClient:
                    {
                        if (e.OldState == GameState.GameLobbyHost || e.OldState == GameState.GameLobbyClient)
                        {
                            Cleanup();
                        }
                    }
                    break;
                default:
                    return;
            }

        }

        #endregion


        public void SubscribeCommunicator()
        {
            Game.Communicator.MessageReceived += OnMessageReceived;
            Game.Communicator.ConnectionLost += OnConnectionLost;
        }

        private void OnConnectionLost(object sender, ConnectionEventArgs e)
        {
            if (!IsValidState)
            {
                //only handle connection lost events while in lobby
                return;
            }

            if (Game.State == GameState.GameLobbyHost)
            {
                var player = Game.Communicator.GetPlayer(e.Connection);
                    

                //inform other clients of the disconnect and give them a player ID mapping to update
                //warning: This method might fail if people join or leave very close together and the messages arrive out of order client side

                string content = "";

                foreach (Player t in Players)
                {
                    int id = t.PlayerID;
                    int newid = id == player.PlayerID ? -1 : (t.PlayerID < player.PlayerID ? t.PlayerID : t.PlayerID - 1);
                    content +=  id + " " + newid + "\n";
                    if(newid != -1)
                    {
                        t.PlayerID = newid;
                    }
                }

                Game.Communicator.MessageOtherPlayers(player, new NetworkMessage(MessageType.PlayerLeft, content));

                RemovePlayer(player);

                player.Dispose();

            }
            else
            {
                //client side, we lost connection to the host. we can leave the lobby
                DebugMessages.Add("Lost connection with host");
                CancelGame();
                Game.ChangeState(GameState.MainMenu);

            }
        }

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            //only handle messages if we are in one of our states
            if (!IsValidState) return;


            var connection = sender as NetworkConnection;
            if (connection != null)
            {
                switch (e.Message.Type)
                {
                    //for server side getting the name of the newly connected player
                    //once it has this it can announce the new player to the other players.
                    case MessageType.Hello:
                        {
                            RemotePlayer player = Game.Communicator.GetPlayer(connection);
                            player.Name = e.Message.Content;
                            string content = player.PlayerID + "\n" + player.Name;
                            Game.Communicator.MessageOtherPlayers(player, 
                                new NetworkMessage(MessageType.PlayerJoined, content));

                            foreach (Player otherPlayer in Players)
                            {
                                if (otherPlayer != player && otherPlayer.PlayerID != Game.Communicator.LocalId)
                                {
                                    content = otherPlayer.PlayerID + "\n" + otherPlayer.Name;
                                    Game.Communicator.MessagePlayer(player,
                                        new NetworkMessage(MessageType.PlayerJoined, content));
                                }
                            }
                        }
                        break;
                    //now on client side we can add the new player
                    case MessageType.PlayerJoined:
                        {
                            var lines = e.Message.Content.Split(new[] {'\n'});

                            int id;
                            if(int.TryParse(lines[0], out id))
                            {
                                var player = new RemotePlayer(Game, GameScreen) {PlayerID = id, Name = lines[1]};
                                AddPlayer(player);
                                player.SubscribeConnection();
                                DebugMessages.Add("Player " + player.PlayerID + " Joined");
                            }

                        }
                        break;
                    case MessageType.PlayerLeft:
                        {
                            //client got player left notice, now remap player id and remove useless player
                            var lines = e.Message.Content.Split(new[] {'\n'}).Where(line => !string.IsNullOrEmpty(line));
                            foreach (var line in lines)
                            {
                                var idstrings = line.Split(new [] {' '});
                                int oId, nId;

                                int.TryParse(idstrings[0], out oId);
                                int.TryParse(idstrings[1], out nId);
                                Player player = GetPlayer(oId);
                                
                                if (nId == -1)
                                {
                                    DebugMessages.Add("Player " + oId + " Left");
                                    RemovePlayer(player);
                                    player.Dispose();
                                }
                                else
                                {
                                    player.PlayerID = nId;
                                }

                            }
                        }
                        break;
                    case MessageType.SetupGame:
                        {
                            GameScreen.SetupMessage = e.Message;
                            Game.ChangeState(GameState.PlayingAsClient);
                        }
                        break;
                    default:
                        return;
                }
            }
            
        }


        public void UnsubscribeCommunicator()
        {
            Game.Communicator.MessageReceived -= OnMessageReceived;
            Game.Communicator.ConnectionLost -= OnConnectionLost;
        }

        public override void Draw(GameTime gameTime)
        {
            if (LeaveButton.Visible)
            {
                LeaveButton.Draw(gameTime);
            }

            if (CloseButton.Visible)
            {
                CloseButton.Draw(gameTime);
            }

            if (StartGameButton.Visible)
            {
                StartGameButton.Draw(gameTime);
            }

			if (UDPButton.Visible)
			{
				UDPButton.Draw(gameTime);
			}

            if (m_playersPanel.Visible)
            {
                m_playersPanel.Draw(gameTime);
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Lobby != null)
            {
                Lobby.Dispose();
                Lobby = null;
                UnsubscribeCommunicator();
            }
            base.Dispose(disposing);
        }


    }
}