using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Screens.Main
{
    public class JoinGameScreen : MainScreen
    {

        private LabelTextBox m_hostIp;
        private CancelOk m_ok;

        public JoinGameScreen(Game game) : base(game, GameState.JoiningGame)
        {

            m_hostIp = new LabelTextBox(game, this);
            m_ok = new CancelOk(game, this);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && m_hostIp != null)
            {
                m_hostIp.Dispose();
                m_ok.Dispose();

                m_hostIp = null;
                m_ok = null;
            }
            base.Dispose(disposing);
        }

        public override void Initialize()
        {
            var vp = Game.GraphicsDevice.Viewport.Bounds;

            m_hostIp.Rect = new Rectangle(vp.Width * 1/5, vp.Height * 7/16, vp.Width* 3/5, vp.Height/8);

            m_hostIp.LabelElement.TextColor = Color.White;
            m_hostIp.LabelText = "Host Ip Adress:";

            m_ok.OkText = "Connect";
            m_ok.Rect = new Rectangle((int) (vp.Width * 3.2/5), vp.Height * 5/6, (int) (vp.Width / 3.7), vp.Height /7);
            m_ok.OnCancel = () => Game.ChangeState(GameState.MainMenu);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (m_hostIp.Visible)
            {
                m_hostIp.Draw(gameTime);
            }

            if (m_ok.Visible)
            {
                m_ok.Draw(gameTime);
            }

        
        }
    }
}
