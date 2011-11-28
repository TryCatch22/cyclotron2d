using System;
using Cyclotron2D.Graphics;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;
using Cyclotron2D.Sounds;

namespace Cyclotron2D.UI.UIElements
{
    public class Button : TextElement
    {

        private Color m_defaultBackground;

        public Button(Game game, Screen screen) : base(game, screen)
        {
            Click += OnClick;
            m_defaultBackground = new Color(0, 148, 255, 255);
            Background = m_defaultBackground;
        }

        protected virtual void OnClick(object sender, EventArgs e)
        {
			Sound.PlaySound(Sound.Clink, 0.5f);
        }

        public event EventHandler Click;

        private void InvokeClick()
        {
            EventHandler handler = Click;
            if (handler != null) handler(this, new EventArgs());
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if(IsMouseOver)Game.SpriteBatch.Draw(Art.Pixel, Rect, Art.NeonBlue);

            if(ShouldDrawText(typeof(Button))) DrawText();
        }

        protected override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);
            if (IsMouseOver && Game.InputState.IsNewLeftClick)
            {
                InvokeClick();
            }
        }
    }
}