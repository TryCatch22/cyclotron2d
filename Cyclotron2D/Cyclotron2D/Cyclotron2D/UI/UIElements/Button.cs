using System;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;
using Cyclotron2D.Sounds;

namespace Cyclotron2D.UI.UIElements
{
    public class Button : TextElement
    {
        public Button(Game game, Screen screen) : base(game, screen)
        {
            Click += OnClick;
            Background = new Color(0, 148, 255, 255);
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