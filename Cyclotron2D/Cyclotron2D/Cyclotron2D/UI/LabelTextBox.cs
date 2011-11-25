using System;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UI
{
    public class LabelTextBox : LabeledElement
    {
        //for convenience
        public String BoxText { get { return Element.Text; } set { Element.Text = value; } }

        public new TextBox Element { get { return base.Element as TextBox; } private set { base.Element = value; } }

		public LabelTextBox(Game game, Screen screen)
			: base(game, screen)
        {
			Element = new TextBox(game, screen);
        }

    }
}
