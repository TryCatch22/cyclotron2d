using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cyclotron2D.Graphics;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UI.UIElements
{
    public class LabeledElement : UIElement
    {

        public String LabelText { get { return Label.Text; } set { Label.Text = value; } }

        //made these public for easy acces to their own properties like background or text color.
        public TextElement Label { get; private set; }
        public UIElement Element { get; set; }

		// In case of forced label width, so that a list of elements looks pretty.
		// If null, text size determines width of label.
		public int? LabelWidth { get; private set; }

        public LabeledElement(Game game, Screen screen, int? labelWidth = null, TextAlign labelTextAlign = TextAlign.Center)
			: base(game, screen)
        {
			Label = new TextElement(game, screen) { TextAlign = labelTextAlign };
			LabelWidth = labelWidth;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Vector2 lblsize = Art.Font.MeasureString(LabelText) + new Vector2(2, 2);
			if (LabelWidth.HasValue)
			{
				lblsize.X = (float)LabelWidth;
			}
            Label.Rect = new Rectangle(Rect.X, Rect.Y, (int)lblsize.X, Rect.Height);
            Element.Rect = new Rectangle(Rect.X + Label.Rect.Width, Rect.Y, Rect.Width - Label.Rect.Width, Rect.Height);

        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (Element.Visible)
            {
                Element.Draw(gameTime);
            }
            if (Label.Visible)
            {
                Label.Draw(gameTime);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Label != null && Element != null)
            {
                Label.Dispose();
                Element.Dispose();

                Element = null;
                Label = null;
            }
            base.Dispose(disposing);
        }

        
    }
}
