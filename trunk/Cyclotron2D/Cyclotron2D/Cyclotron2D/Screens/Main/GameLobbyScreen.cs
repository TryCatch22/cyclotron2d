using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;
using Cyclotron2D.Network;
using Cyclotron2D.UI.UIElements;
using Cyclotron2D.Helpers;

namespace Cyclotron2D.Screens.Main {

	class GameLobbyScreen : MainScreen {

		private const int X_DRAW = 10;
		private const int Y_DRAW = 10;

		Button spamButton;
		GameLobby Lobby;

		public GameLobbyScreen(Game game)
			: base(game, GameState.Hosting) {

				spamButton = new Button(game, this);
				spamButton.Click += OnSpamButtonClicked;
				spamButton.Text = "Send Spam";
		}

		private void OnSpamButtonClicked(Object sender, EventArgs e){
			Lobby.messageAllClients("Hello All Clients");
		}

		protected override void OnStateChanged(object sender, StateChangedEventArgs e) {
			base.OnStateChanged(sender, e);

			if (IsValidState) {
				spamButton.Rect = RectangleBuilder.Centered(GraphicsDevice.Viewport.Bounds, new Vector2(0.5f, 0.5f));
				Lobby = new GameLobby();
				Lobby.start();
			}
		}


		public override void Draw(GameTime gameTime) {

			spamButton.Draw(gameTime);
		}




	}
}
