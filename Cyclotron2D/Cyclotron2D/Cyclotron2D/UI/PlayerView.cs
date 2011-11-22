using Cyclotron2D.Core.Players;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UI
{
    public class PlayerView : UIElement
    {
        public Player Player { get; set; }

        public Color TextColor { get { return NameAndId.TextColor; } set { NameAndId.TextColor = value; } }

        private Rectangle PlayerColorRect { get; set; }

        private TextElement NameAndId { get; set; }

        public PlayerView(Game game, Screen screen) : base(game, screen)
        {
            NameAndId = new TextElement(game, screen);
        }

        public override void Update(GameTime gameTime)
        {
            NameAndId.Text = Player.Name + " (" + Player.PlayerID + ")";
            PlayerColorRect = new Rectangle(Rect.X + 2, Rect.Y + 2, Rect.Height-4, Rect.Height-4);
            NameAndId.Rect = new Rectangle(Rect.X + PlayerColorRect.Width + 2, Rect.Y, Rect.Width - PlayerColorRect.Width - 2, Rect.Height);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Draw(Art.Pixel, PlayerColorRect, Player.Color);

            if (NameAndId.Visible)
            {
                NameAndId.Draw(gameTime);
            }

        }


    }
}
