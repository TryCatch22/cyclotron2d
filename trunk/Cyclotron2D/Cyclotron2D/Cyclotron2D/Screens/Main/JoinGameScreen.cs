using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;
using Cyclotron2D.Screens.Popup;
using System.Net;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Cyclotron2D.Network;


namespace Cyclotron2D.Screens.Main {
	public class JoinGameScreen : MainScreen {

		private LabelTextBox m_hostIp;
		private CancelOk m_ok;
		private NetworkClient Client;

		public JoinGameScreen(Game game)
			: base(game, GameState.JoiningGame) {

			m_hostIp = new LabelTextBox(game, this);
			m_ok = new CancelOk(game, this);
			Client = new NetworkClient();
		}

		protected override void Dispose(bool disposing) {
			if (disposing && m_hostIp != null) {
				m_hostIp.Dispose();
				m_ok.Dispose();

				m_hostIp = null;
				m_ok = null;
			}
			base.Dispose(disposing);
		}

		public override void Initialize() {
			var vp = Game.GraphicsDevice.Viewport.Bounds;

			m_hostIp.Rect = new Rectangle(vp.Width * 1 / 5, vp.Height * 7 / 16, vp.Width * 3 / 5, vp.Height / 8);

			m_hostIp.Label.TextColor = Color.White;
			m_hostIp.LabelText = "Host Ip Adress:";
			m_hostIp.Element.ValueChanged += (obj, args) => m_hostIp.BoxText = AutoCompleteIP(m_hostIp.BoxText, ((ValueChangedEventArgs)args).OldValue);

			m_ok.OkText = "Connect";
			m_ok.Rect = new Rectangle((int)(vp.Width * 3.2 / 5), vp.Height * 5 / 6, (int)(vp.Width / 3.7), vp.Height / 7);
			m_ok.OnCancel = () => Game.ChangeState(GameState.MainMenu);
			m_ok.OnOk = TryConnect;
		}

		private string AutoCompleteIP(string text, string oldText) {
			// Case 1: Text has been deleted
			if (oldText.Length > text.Length) {
				// If they deleted a ".", delete the character before it too.
				if (oldText[oldText.Length - 1] == '.' && oldText.Length >= 2)
					return oldText.Substring(0, oldText.Length - 2);
				return text;
			}

			// Case 2: Text has been entered

			// Case 2a: They started off by entering a ".". The fools...
			if (text.StartsWith("."))
				return "";

			// Get rid of any other unwanted crap.
			text = text.Replace("..", ".");
			text = Regex.Replace(text, @"[^\d.]", "");

			// Auto add the period if 3 digits have been typed, and if it isn't the last group of digits.
			if (text.Length >= 3 && Regex.IsMatch(text, @"\d\d\d$")) {
				var value = Int32.Parse(text.Substring(text.Length - 3));
				// Stop those bastards from typing too-big numbers
				if (value > 255)
					return oldText;

				if (text.Count(x => x == '.') < 3)
					text += ".";
			} else if (text.Length >= 2 && text.Count(x => x == '.') < 3 && Regex.IsMatch(text, @"\d\d$")) {
				// 2-digit numbers between 26 and 99 are already going to be greater than 255 if
				// allowed to add a third digit, so we'll add the period immediately.
				var value = Int32.Parse(text.Substring(text.Length - 2));
				if (value >= 26 && value <= 99)
					text += ".";
			}

			// Once it's valid, stop them from typing anything else.
			var ipRegex = @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$";
			if (Regex.IsMatch(oldText, ipRegex) && !Regex.IsMatch(text, ipRegex))
				return oldText;

			return text;
		}

		protected override void OnStateChanged(object sender, StateChangedEventArgs e) {
			base.OnStateChanged(sender, e);

			if (IsValidState) {
				Client.ConnectToServer();
			}
		}

		private void TryConnect() {
			//try
			//{
			//    var ip = IPAddress.Parse(m_hostIp.BoxText);
			//    Game.ScreenManager.AddScreen(new OkPopup(Game, this)
			//    {
			//        MessageText = "Connecting...",
			//        OkText = "Cancel"
			//    });
			//}
			//catch (FormatException e)
			//{
			//    DebugMessages.Add("Invalid IP Address, fool!");
			//}

			Client.startReceive();
		}

		public override void Draw(GameTime gameTime) {
			base.Draw(gameTime);
			if (m_hostIp.Visible) {
				m_hostIp.Draw(gameTime);
			}

			if (m_ok.Visible) {
				m_ok.Draw(gameTime);
			}


		}
	}
}
