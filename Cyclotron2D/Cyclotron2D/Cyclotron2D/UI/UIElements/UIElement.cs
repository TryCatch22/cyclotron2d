using Cyclotron2D.Components;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UI.UIElements
{
    public abstract class UIElement : DrawableScreenComponent
    {
        protected UIElement(Game game, Screen screen) : base(game, screen)
        {
            Background = Color.Transparent;
        }

        public Rectangle Rect { get; set; }

        public Color Background { get; set; }

        public bool IsMouseOver { get; private set; }

        protected override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);
            IsMouseOver = Rect.Contains(Game.InputState.MousePosition);
            UpdateOrder = 200; //generally update ui after the rest
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Game.SpriteBatch.Draw(Art.Pixel, Rect, Background);
        }
    }
}