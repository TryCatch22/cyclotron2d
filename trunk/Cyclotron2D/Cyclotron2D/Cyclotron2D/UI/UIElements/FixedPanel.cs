using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UI.UIElements
{
    public class FixedPanel : Panel
    {

        public Orientation Orientation { get; set; }

        /// <summary>
        /// Ratio from the rect size for the panel to the elements.
        /// </summary>
        public float SizeRatio { get; set; }

        public FixedPanel(Game game, Screen screen) : base(game, screen)
        {
            Orientation = Orientation.Vertical;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            int height = (int) (SizeRatio*Rect.Height - ItemSpacing);
            int width = (int)(SizeRatio * Rect.Width - ItemSpacing);
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Rect = Orientation == Orientation.Vertical ?
                    new Rectangle(Rect.X, Rect.Y + i * (height + ItemSpacing), Rect.Width, height) :
                    new Rectangle(Rect.X + i * (width + ItemSpacing), Rect.Y, width, Rect.Height);
            }

        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            DrawElements();
        }
    }
}
