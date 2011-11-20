using System;
using System.Collections.Generic;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI;
using Microsoft.Xna.Framework;
using Cyclotron2D.Network;
using Cyclotron2D.UI.UIElements;
using Cyclotron2D.Helpers;

namespace Cyclotron2D.Screens.Main {

	public class GameLobbyScreen : MainScreen
	{
	  /*  private StretchPanel m_playersPanel;

	    private List<PlayerView> m_playerViews;
       */

        Button SpamButton;
		Button CancelButton;
		Button CloseButton;
		LabelTextBox SpamTextBox;

        public GameLobby Lobby { get; private set; }

		public GameLobbyScreen(Game game)
			: base(game, GameState.GameLobbyHost | GameState.GameLobbyClient) {

            /*m_playerViews = new List<PlayerView>();
            m_playersPanel = new StretchPanel(game, this);*/

           

			SpamButton = new Button(game, this);
			SpamButton.Click += OnSpamButtonClicked;
			SpamButton.Text = "Send Spam";

			CancelButton = new Button(game, this);
			CancelButton.Click += OnCancelButtonClicked;
			CancelButton.Text = "Main Menu";

			CloseButton = new Button(game, this);
			CloseButton.Click += OnCloseButtonClick;
			CloseButton.Text = "Close Lobby";

			SpamTextBox = new LabelTextBox(game, this);
			SpamTextBox.Label.Background = Color.Black;
			SpamTextBox.Label.TextColor = Color.White;
			SpamTextBox.LabelText = "Spam Message";
			SpamTextBox.BoxText = "GLHF";
			SpamTextBox.Background = Color.Gray;
		}


	    public override void Initialize()
        {
            base.Initialize();
            Rectangle win = GraphicsDevice.Viewport.Bounds;

            SpamTextBox.Rect = new Rectangle(50, 150, (int)(win.Width * 0.7), (int)(win.Height * 0.2));
            SpamButton.Rect = new Rectangle(200, SpamTextBox.Rect.Bottom + 10, (int)(win.Width * 0.5), (int)(win.Height * 0.2));
            CancelButton.Rect = RectangleBuilder.BottomRight(win, new Vector2(0.15f, 0.15f), new Point(20, 10));
            CloseButton.Rect = RectangleBuilder.BottomRight(win, new Vector2(0.15f, 0.15f), new Point(25 + CancelButton.Rect.Width, 10)); 

        }

        #region Event Handlers

        private void OnSpamButtonClicked(Object sender, EventArgs e) 
        {
            Game.Communicator.SendDebugMessage(SpamTextBox.BoxText);
		}

		private void OnCancelButtonClicked(Object sender, EventArgs e) {
				Lobby.Kill();
				Game.ChangeState(GameState.MainMenu);
		}

		private void OnCloseButtonClick(Object sender, EventArgs e) {
			Lobby.CloseGameLobby();
		}

        private void SubscribeLobby()
        {
            Lobby.NewConnection += OnNewConnection;
        }

        #endregion


        private void OnNewConnection(object sender, ConnectionEventArgs e)
        {
            var gameScreen = Game.ScreenManager.GameScreen;
            var rem = new RemotePlayer(Game, gameScreen);
 
            Game.Communicator.Add(rem, e.Socket);
            gameScreen.AddRemotePlayer(rem);

        }

        /// <summary>
		///  Starts the GameLobby instance and spawns threads to accept connections accordingly.
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected override void OnStateChanged(object sender, StateChangedEventArgs e) {
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
                    break;
                default:
                    return;
            }

		}


		public override void Draw(GameTime gameTime) {
			CancelButton.Draw(gameTime);
			CloseButton.Draw(gameTime);
			SpamButton.Draw(gameTime);
			SpamTextBox.Draw(gameTime);
		}




	}
}
