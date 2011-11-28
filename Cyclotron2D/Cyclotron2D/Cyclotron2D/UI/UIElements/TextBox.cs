using System;
using Cyclotron2D.Graphics;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Cyclotron2D.Sounds;

namespace Cyclotron2D.UI.UIElements
{
    public class TextBox :TextElement
    {
		public event EventHandler<ValueChangedEventArgs> ValueChanged;

        public void InvokeValueChanged(ValueChangedEventArgs e)
        {
            EventHandler<ValueChangedEventArgs> handler = ValueChanged;
            if (handler != null) handler(this, e);
        }

        private bool m_readingText;

        public TextBox(Game game, Screen screen) : base(game, screen)
        {
            Background = Color.White;
        }

        private TimeSpan m_lastDelete;

        private TimeSpan m_repeatTime = new TimeSpan(0, 0, 0, 0, 70);

        private void ReadText(GameTime gameTime)
        {
			var oldText = Text;

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

			if (oldText != Text)
                InvokeValueChanged(new ValueChangedEventArgs(oldText));
        }

        protected override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);

            var input = Game.InputState;
            if (input.IsNewLeftClick)
            {
                m_readingText = IsMouseOver;
				Sound.PlaySound(Sound.Clink, 0.5f);
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

	public class ValueChangedEventArgs : EventArgs
	{
		public readonly string OldValue;

		public ValueChangedEventArgs(string oldValue)
		{
			OldValue = oldValue;
		}
	}
}
