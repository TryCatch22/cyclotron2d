using System;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Cyclotron2D.Screens.Popup
{
    public class EndGamePopup : PopupScreen
    {
        private TextElement m_message;

        private Button m_quit;



        public EndGamePopup(Game game, MainScreen parent, String text) : base(game, parent)
        {
            //gotta call explicitly caus this is created after the game.Initialize call
            Initialize(text);
        }

        public void Initialize(string text)
        {
            Rectangle vp = Game.GraphicsDevice.Viewport.Bounds;

            Rect = new Rectangle(vp.Width/7, vp.Height/4, vp.Width*5/7, vp.Height/2);
            Background = new Color(0, 0, 0, 200);

            InitializeMessage(text);
            InitializeButton();
        }

        private void InitializeButton()
        {
            m_quit = new Button(Game, this)
                         {
                             Text = "Leave Game",
                             Rect = new Rectangle(Rect.X + Rect.Width*11/16, Rect.Y + (int) (Rect.Height*0.7), Rect.Width/4, Rect.Height/5)
                         };

            m_quit.Click += OnQuitClicked;
        }

        protected override void HandleInupt(GameTime gameTime)
        {
            base.HandleInupt(gameTime);
            if (Game.InputState.IsNewKeyPress(Keys.Enter))
            {
                Quit();
            }
        }

        private void OnQuitClicked(object sender, EventArgs e)
        {
            Quit();
        }

        private void Quit()
        {
            var gs = Parent as GameScreen;
            gs.StopGame();
            Game.ChangeState(GameState.MainMenu);
            Game.ScreenManager.RemoveScreen(this);
        }

        private void InitializeMessage(string text)
        {
            m_message = new TextElement(Game, this)
                            {
                                Text = text,
                                TextColor = Color.Red,
                                TextScale = 3f,
                                Rect = new Rectangle(Rect.X + Rect.Width/4, Rect.Y + Rect.Height/8, Rect.Width/2, Rect.Height/3)
                            };
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (m_quit.Visible)
                m_quit.Draw(gameTime);

            if (m_message.Visible)
                m_message.Draw(gameTime);
        }
    }
}