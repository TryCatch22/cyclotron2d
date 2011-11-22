using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Helpers;
using Cyclotron2D.Mod;
using Cyclotron2D.Network;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Screens.Main
{

    public class GameLobbyScreen : MainScreen
    {
        private PlayerPanel m_playersPanel;

        //   Button SpamButton;
        Button CancelButton;
        Button CloseButton;
        //LabelTextBox SpamTextBox;

        private List<Player> Players { get; set; } 

        public GameLobby Lobby { get; private set; }

        private GameScreen GameScreen { get; set; }

        public GameLobbyScreen(Game game)
            : base(game, GameState.GameLobbyHost | GameState.GameLobbyClient)
        {
            Players = new List<Player>();
            m_playersPanel = new PlayerPanel(game, this);

            //		   SpamButton = new Button(game, this);
            CancelButton = new Button(game, this);
            //           SpamTextBox = new LabelTextBox(game, this);
            CloseButton = new Button(game, this);
        }


        public override void Initialize()
        {
            base.Initialize();

            m_playersPanel.Background = new Color(20, 0, 90);

            //            SpamButton.Click += OnSpamButtonClicked;
            //            SpamButton.Text = "Send Spam";


            CancelButton.Click += OnCancelButtonClicked;
            CancelButton.Text = "Leave";


            CloseButton.Click += OnCloseButtonClick;
            CloseButton.Text = "Close Lobby";

            GameScreen = Game.ScreenManager.GetMainScreen<GameScreen>() as GameScreen;

            Debug.Assert(GameScreen != null, "GameScreen should not be null at Initialize.");

            SubscribeCommunicator();

            //            SpamTextBox.Label.Background = Color.Black;
            //            SpamTextBox.Label.TextColor = Color.White;
            //            SpamTextBox.LabelText = "Spam Message";
            //            SpamTextBox.BoxText = "GLHF";
            //            SpamTextBox.Background = Color.Gray;


        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Rectangle win = GraphicsDevice.Viewport.Bounds;

            CloseButton.Visible = CloseButton.Enabled = Game.State == GameState.GameLobbyHost;

            m_playersPanel.Rect = RectangleBuilder.TopLeft(win, new Vector2(0.35f, 0.5f), new Point(15, 15));

            //            SpamTextBox.Rect = new Rectangle(50, 150, (int)(win.Width * 0.7), (int)(win.Height * 0.2));
            //            SpamButton.Rect = new Rectangle(200, SpamTextBox.Rect.Bottom + 10, (int)(win.Width * 0.5), (int)(win.Height * 0.2));
            CancelButton.Rect = RectangleBuilder.BottomRight(win, new Vector2(0.15f, 0.1f), new Point(20, 10));
            CloseButton.Rect = RectangleBuilder.BottomRight(win, new Vector2(0.15f, 0.1f), new Point(25 + CancelButton.Rect.Width, 10));



        }

        /// <summary>
        /// adding a client player to another client player
        /// </summary>
        /// <param name="player"></param>
        public void AddPlayer(Player player)
        {
            GameScreen.AddPlayer(player);
            m_playersPanel.AddPlayer(player);
            Players.Add(player);
        }


        public void AddPlayer(Player player, Socket socket)
        {
            if(player is RemotePlayer)
            {
                Game.Communicator.Add(player as RemotePlayer, socket);

            }
            GameScreen.AddPlayer(player);
            m_playersPanel.AddPlayer(player);
            Players.Add(player);
            
        }


        /// <summary>
        /// this will also disconnect the players connection
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
        private void Cleanup()
        {
            foreach (var player in m_playersPanel.Players)
            {
                RemovePlayer(player);

                player.Dispose();
            }
        }

        #region Client Side

        public void AddLocalPlayer(LocalPlayer player)
        {
            GameScreen gameScreen = Game.ScreenManager.GetMainScreen<GameScreen>() as GameScreen;
            if (gameScreen != null)
            {
                gameScreen.AddPlayer(player);
                m_playersPanel.AddPlayer(player);
            }
        }

        public void AddHost(RemotePlayer player, NetworkConnection connection)
        {
            GameScreen gameScreen = Game.ScreenManager.GetMainScreen<GameScreen>() as GameScreen;
            if (gameScreen != null)
            {
                gameScreen.AddPlayer(player);
                Game.Communicator.AddHost(player, connection);
                m_playersPanel.AddPlayer(player);
            }
       }
        

        #endregion

        #region Server Side

        private void SubscribeLobby()
        {
            Lobby.NewConnection += OnNewConnection;
            Lobby.LostConnection += OnLostConnection;
        }

        private void OnLostConnection(object sender, ConnectionEventArgs e)
        {
            GameScreen gameScreen = Game.ScreenManager.GetMainScreen<GameScreen>() as GameScreen;

            if (gameScreen != null)
            {
                RemovePlayer(Game.Communicator.GetPlayer(e.Socket));
            }


        }

        private void OnNewConnection(object sender, ConnectionEventArgs e)
        {
            GameScreen gameScreen = Game.ScreenManager.GetMainScreen<GameScreen>() as GameScreen;
            if (gameScreen != null)
            {
                var rem = new RemotePlayer(Game, gameScreen) {PlayerID = Players.Count + 2};

                AddPlayer(rem, e.Socket);

                string content = rem.PlayerID.ToString() + "\n" + Settings.SinglePlayer.PlayerName.Value;

                Game.Communicator.MessagePlayer(rem, new NetworkMessage(MessageType.Welcome, content));
            }
        }


        private void OnCloseButtonClick(Object sender, EventArgs e)
        {
            Lobby.CloseGameLobby();
        }

        #endregion

        #region Event Handlers

        private void OnSpamButtonClicked(Object sender, EventArgs e)
        {
            //            Game.Communicator.SendDebugMessage(SpamTextBox.BoxText);
        }

        private void OnCancelButtonClicked(Object sender, EventArgs e)
        {
            if (Game.State == GameState.GameLobbyHost)
            {
                Lobby.Kill();
            }
            
            Cleanup();

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
                    AddLocalPlayer(new LocalPlayer(Game, GameScreen){PlayerID = 1});
                    break;
                case GameState.GameLobbyClient:
                    //setup client side info, maybe deal with initial control messages from server here
                    break;
                default:
                    return;
            }

        }

        #endregion


        public void SubscribeCommunicator()
        {
            Game.Communicator.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
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
                                if (otherPlayer != player)
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
                            }

                            


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
        }

        public override void Draw(GameTime gameTime)
        {


            if (CancelButton.Visible)
            {
                CancelButton.Draw(gameTime);
            }

            if (CloseButton.Visible)
            {
                CloseButton.Draw(gameTime);
            }

            //            if (SpamButton.Visible)
            //            {
            //                SpamButton.Draw(gameTime);
            //            }
            //
            //            if (SpamTextBox.Visible)
            //            {
            //                SpamTextBox.Draw(gameTime);
            //            }






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
