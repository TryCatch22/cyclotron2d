using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UI
{

    public class LabelCheckBox : LabeledElement
    {
        public new CheckBox Element { get { return base.Element as CheckBox; } private set { base.Element = value; } }

        public bool IsChecked { get { return Element.IsChecked; } set { Element.IsChecked = value; }}
            
            
        public LabelCheckBox(Game game, Screen screen) : base(game, screen)
        {
            Element = new CheckBox(game, screen);
        }
    }
}

