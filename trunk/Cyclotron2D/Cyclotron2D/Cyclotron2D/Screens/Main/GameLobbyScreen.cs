using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;
using Cyclotron2D.Network;
using Cyclotron2D.UI.UIElements;
using Cyclotron2D.Helpers;
using Cyclotron2D.UI;

namespace Cyclotron2D.Screens.Main {

	class GameLobbyScreen : MainScreen {

		private const int X_DRAW = 10;
		private const int Y_DRAW = 10;

		Button SpamButton;
		Button CancelButton;
		LabelTextBox SpamTextBox;

		GameLobby Lobby;

		public GameLobbyScreen(Game game)
			: base(game, GameState.Hosting) {

			SpamButton = new Button(game, this);
			SpamButton.Click += OnSpamButtonClicked;
			SpamButton.Text = "Send Spam";

			CancelButton = new Button(game, this);
			CancelButton.Click += OnCancelButtonClicked;
			CancelButton.Text = "Cancel";

			SpamTextBox = new LabelTextBox(game, this);
			SpamTextBox.Label.Background = Color.Black;
			SpamTextBox.Label.TextColor = Color.White;
			SpamTextBox.LabelText = "Spam Message";
			SpamTextBox.BoxText = "GLHF";
			SpamTextBox.Background = Color.Gray;
		}

		private void OnSpamButtonClicked(Object sender, EventArgs e) {
			if (this.HasFocus) {
				Lobby.messageAllClients(SpamTextBox.BoxText);
			}
		}

		private void OnCancelButtonClicked(Object sender, EventArgs e) {
			if (this.HasFocus) {
				Lobby.Kill();
				Game.ChangeState(GameState.MainMenu);
			}
		}

		protected override void OnStateChanged(object sender, StateChangedEventArgs e) {
			base.OnStateChanged(sender, e);

			if (IsValidState) {
				Rectangle win = GraphicsDevice.Viewport.Bounds;

				SpamTextBox.Rect = new Rectangle(50, 150, (int)(win.Width * 0.7), (int)(win.Height * 0.2));
				SpamButton.Rect = new Rectangle(200, SpamTextBox.Rect.Bottom + 10, (int)(win.Width*0.5), (int)(win.Height*0.2));
				CancelButton.Rect = RectangleBuilder.BottomRight(GraphicsDevice.Viewport.Bounds, new Vector2(0.15f, 0.15f), new Point(20, 10));

				Lobby = new GameLobby();
				Lobby.Start();
			}
		}


		public override void Draw(GameTime gameTime) {
			CancelButton.Draw(gameTime);
			SpamButton.Draw(gameTime);
			SpamTextBox.Draw(gameTime);
		}




	}
}
