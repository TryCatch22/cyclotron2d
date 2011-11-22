using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UI
{

    public class LabelCheckBox : LabeledElement
    {
        public new CheckBox Element { get { return base.Element as CheckBox; } private set { base.Element = value; } }

        public bool IsChecked { get { return Element.IsChecked; } set { Element.IsChecked = value; }}
            
        public LabelCheckBox(Game game, Screen screen, int? labelWidth = null) : base(game, screen, labelWidth)
        {
            Element = new CheckBox(game, screen);
        }
    }
}

