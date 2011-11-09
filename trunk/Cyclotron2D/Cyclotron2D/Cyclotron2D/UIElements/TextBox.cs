using System;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Cyclotron2D.UIElements
{
    public class TextBox :TextElement
    {

        private bool m_readingText;

        public TextBox(Game game, Screen screen) : base(game, screen)
        {
            Background = Color.White;
        }

        private TimeSpan m_lastDelete;

        private TimeSpan m_repeatTime = new TimeSpan(0, 0, 0, 0, 70);

        private void ReadText(GameTime gameTime)
        {
            var input = Game.InputState;
            char? c = null;
            if (input.IsNewCharPress(out c))
            {
                Text += c.Value;
            }

            if (Text != "")
            {
                if (input.IsNewKeyPress(Keys.Back))
                {
                     Text = Text.Remove(Text.Length - 1);
                    m_lastDelete = gameTime.TotalGameTime;
                }
                else if (input.IsKeyDown(Keys.Back) && gameTime.TotalGameTime - m_lastDelete > m_repeatTime)
                {
                    Text = Text.Remove(Text.Length - 1);
                    m_lastDelete = gameTime.TotalGameTime;
                }
            }

          

        }

        protected override void HandleInupt(GameTime gameTime)
        {
            base.HandleInupt(gameTime);

            var input = Game.InputState;
            if (input.IsNewLeftClick)
            {
                m_readingText = IsMouseOver;
            }


            if (m_readingText)
            {
                ReadText(gameTime);
            }


        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);


            if (!m_readingText)
            {
                Game.SpriteBatch.Draw(Art.Pixel, Rect, new Color(50, 50, 50, 170));
            }

            if (ShouldDrawText(typeof(TextBox)))
            {
                DrawText();
            }
        }

    }
}
