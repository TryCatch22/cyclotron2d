using System;
using System.Net;
using Cyclotron2D.Mod;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;
using Cyclotron2D.Network;


namespace Cyclotron2D.Screens.Main {
	public class JoinGameScreen : MainScreen
	{


	    private LabelTextBox m_playerName;
		private IpTextBox m_hostIp;

	    private StretchPanel m_panel;

		private CancelOk m_ok;
		private NetworkConnection Client;

		public JoinGameScreen(Game game)
			: base(game, GameState.JoiningGame) {

            m_panel = new StretchPanel(game, this);
            m_playerName = new LabelTextBox(game, this);
            m_hostIp = new IpTextBox(game, this);
			m_ok = new CancelOk(game, this);
			Client = new NetworkConnection();

            m_panel.AddItems(m_playerName, m_hostIp);
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

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!Client.IsConnected && Client.Socket!= null && Client.Socket.Connected)
            {
                Client.Disconnect();
                DebugMessages.Add("Connection Lost");
            }
        }

		public override void Initialize() {
			var vp = Game.GraphicsDevice.Viewport.Bounds;

			m_panel.Rect = new Rectangle(vp.Width * 1 / 5, vp.Height *6 / 16, vp.Width * 3 / 5, vp.Height * 2 / 9);
		    m_panel.Orientation = Orientation.Vertical;


		    m_playerName.Element.Text = Settings.SinglePlayer.PlayerName.Value;
		    m_playerName.Label.TextColor = Color.White;
		    m_playerName.LabelText = "Player Name:";

			m_hostIp.Label.TextColor = Color.White;
			m_hostIp.LabelText = "Host Ip Adress:";
		    m_hostIp.BoxText = "127.0.0.1";

			m_ok.OkText = "Connect";
			m_ok.Rect = new Rectangle((int)(vp.Width * 3.2 / 5), vp.Height * 5 / 6, (int)(vp.Width / 3.7), vp.Height / 7);
			m_ok.OnCancel = CancelConnection;
			m_ok.OnOk = TryConnect;
		}

		private void TryConnect() {
			try
			{
			    var ip = IPAddress.Parse(m_hostIp.BoxText);
                Client.ConnectTo(ip);
			}
			catch (FormatException)
			{
			    DebugMessages.Add("Invalid IP Address, fool!");
			}
            catch(AlreadyConnectedException e)
		    {
                DebugMessages.Add(e.Message);
		    }
		}

		private void CancelConnection() {
			Client.Disconnect();
			Game.ChangeState(GameState.MainMenu);
		}

		public override void Draw(GameTime gameTime) {
			base.Draw(gameTime);
			if (m_panel.Visible) {
				m_panel.Draw(gameTime);
			}

			if (m_ok.Visible) {
				m_ok.Draw(gameTime);
			}


		}
	}
}
