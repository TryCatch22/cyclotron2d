using System;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UI.UIElements
{
    public sealed class LabelTextBox : LabeledElement
    {
      //  public String LabelText { get { return LabelElement.Text; } set { LabelElement.Text = value; } }
        //for convenience
        public String BoxText { get { return (Element as TextBox).Text; } set { (Element as TextBox).Text = value; } }

       //made these public for easy acces to their own properties like background or text color.
        public new TextBox Element { get { return base.Element as TextBox; } private set { base.Element = value; } }
      //  public TextBox BoxElement { get; private set; }

        public LabelTextBox(Game game, Screen screen) : base(game, screen)
        {
          //  LabelElement = new TextElement(game, screen);
            Element = new TextBox(game, screen);
        }

    /*    public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Vector2 lblsize = Art.Font.MeasureString(LabelElement.Text) + new Vector2(2, 2);
            LabelElement.Rect = new Rectangle(Rect.X, Rect.Y, (int) lblsize.X, Rect.Height);
            BoxElement.Rect = new Rectangle(Rect.X + LabelElement.Rect.Width, Rect.Y, Rect.Width - LabelElement.Rect.Width, Rect.Height);

        }*/

      /*  public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (BoxElement.Visible)
            {
                BoxElement.Draw(gameTime);
            }
            if (LabelElement.Visible)
            {
                LabelElement.Draw(gameTime);
            }       
        }
*/
       /* protected override void Dispose(bool disposing)
        {
            if (disposing && BoxElement != null && LabelElement != null)
            {
                LabelElement.Dispose();
                BoxElement.Dispose();

                BoxElement = null;
                LabelElement = null;
            }
            base.Dispose(disposing);
        }
*/

    }
}
