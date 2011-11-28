using Cyclotron2D.Screens.Base;
using Cyclotron2D.Screens.Main;
using Cyclotron2D.State;
using Cyclotron2D.UI;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Screens.Popup
{
    class EndGamePopup : OkPopup
    {
        private EndGameTable m_table;

        public EndGamePopup(Game game, MainScreen parent, string text) : base(game, parent)
        {





            m_table = new EndGameTable(game, this);

            Rectangle vp = Game.GraphicsDevice.Viewport.Bounds;
            Rect = new Rectangle(vp.Width / 7, vp.Height / 6, vp.Width * 5 / 7, vp.Height * 3 /4);

            m_table.Rect = new Rectangle(Rect.X + Rect.Width/8, Rect.Y + Rect.Height/40, Rect.Width * 6/8, Rect.Height * 5/6);
            m_table.Initialize((parent as GameScreen).ActivePlayers);

            OnOkClicked = Quit;
            MessageText = text;
            OkText = "Quit";
            Message.TextScale = 3f;
            Message.Visible = false;
        }


        private void Quit()
        {
            var gs = Parent as GameScreen;
            gs.StopGame();
            Game.ChangeState(GameState.MainMenu);
        }

        public override void Draw(GameTime gameTime)
        {
            //temporarily for debugging


//            base.Draw(gameTime);
//            if (m_table.Visible)
//            {
//                m_table.Draw(gameTime);
//            }
        }
    }
}
