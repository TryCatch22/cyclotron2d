using System.Linq;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UI.UIElements
{
    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// Contains
    /// </summary>
    public class StretchPanel : Panel
    {

        public Orientation Orientation { get; set; }


        public StretchPanel(Game game, Screen screen) : base(game, screen)
        {
            Orientation = Orientation.Vertical;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            int height = Items.Count == 0 ? 0 : (Rect.Height - (Items.Count + 1)*ItemSpacing)/Items.Count;
            int width = Items.Count == 0 ? 0 : (Rect.Width - (Items.Count + 1)*ItemSpacing)/Items.Count;
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Rect = Orientation == Orientation.Vertical ? 
                    new Rectangle(Rect.X, Rect.Y + i*(height + ItemSpacing), Rect.Width, height) : 
                    new Rectangle(Rect.X + i * (width + ItemSpacing),Rect.Y, width, Rect.Height);
            }

        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            DrawElements();
        }
    }
}
