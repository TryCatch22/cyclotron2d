using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI;
using Microsoft.Xna.Framework;
using Cyclotron2D.Network;
using Cyclotron2D.UI.UIElements;
using Cyclotron2D.Helpers;

namespace Cyclotron2D.Screens.Main
{

    public class GameLobbyScreen : MainScreen
    {
        private PlayerPanel m_playersPanel;

        //   Button SpamButton;
        Button CancelButton;
        Button CloseButton;
        //LabelTextBox SpamTextBox;

        public GameLobby Lobby { get; private set; }

        public GameLobbyScreen(Game game)
            : base(game, GameState.GameLobbyHost | GameState.GameLobbyClient)
        {

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



        public void AddPlayer(Player player, Socket socket)
        {
               GameScreen gameScreen = Game.ScreenManager.GetMainScreen<GameScreen>() as GameScreen;
            if (gameScreen != null)
            {
                if(player is RemotePlayer)
                {
                    Game.Communicator.Add(player as RemotePlayer, socket);

                }
                gameScreen.AddPlayer(player);
                m_playersPanel.AddPlayer(player);
            }
        }



        public void RemovePlayer(Player player)
        {
            GameScreen gameScreen = Game.ScreenManager.GetMainScreen<GameScreen>() as GameScreen;

            if (gameScreen != null)
            {
                if (player is RemotePlayer)
                {
                    Game.Communicator.Remove(player as RemotePlayer);
                }
                gameScreen.RemovePlayer(player);
                m_playersPanel.RemovePlayer(player);
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
                var rem = new RemotePlayer(Game, gameScreen) {PlayerID = gameScreen.RemotePlayers.Count + 2};

                AddPlayer(rem, e.Socket);

                Game.Communicator.MessagePlayer(rem, new NetworkMessage(MessageType.NewID, rem.PlayerID.ToString()));
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
            if (Game.State == GameState.GameLobbyHost) Lobby.Kill();
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
                    break;
                case GameState.GameLobbyClient:
                    //setup client side info, maybe deal with initial control messages from server here
                    break;
                default:
                    return;
            }

        }

        #endregion


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
            }
            base.Dispose(disposing);
        }


    }
}
