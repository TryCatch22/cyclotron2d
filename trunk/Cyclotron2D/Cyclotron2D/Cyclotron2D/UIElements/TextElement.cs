using System;
using System.Reflection;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cyclotron2D.UIElements
{
    /// <summary>
    /// Slightly unconventional element this class does not always draw its content. 
    /// Since the text needs to be drawn last to be on top of things, it needs to be called at the end.
    /// Therefore we provide a DrawText() method and a ShouldDrawText bool 
    /// 
    /// This is done so that we can perserve the standard of calling base.draw at the start of each overriding 
    /// call so that the base clases can draw their own material thus simplifying the implementation
    /// of all further UIElements. 
    /// </summary>
    public class TextElement : UIElement
    {
        public TextElement(Game game, Screen screen) : base(game, screen)
        {
            //default value
            TextColor = Color.Black;
            TextScale = 1f;
            Text = "";
        }

        public string Text { get; set; }

        public float TextScale { get; set; }

        public Color TextColor { get; set; }

        /// <summary>
        /// Checks if the draw method has been overriden in the 
        /// current instance. if so  you should not call draw as it is assumed the child class will
        /// </summary>
        /// <returns>If you should call DrawText at the bottom of your Draw Call</returns>
        protected bool ShouldDrawText(Type type)
        {
            var method = GetType().GetMethod("Draw", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (method == null) return false;
            return method.DeclaringType.FullName.Equals(type.FullName, StringComparison.OrdinalIgnoreCase);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            /* if a descendent of TextElement overrides draw it is expected it will  handle text drawing as done here.
               if that class sealed, then it can just drawtext.
               Not doing it is not the end of the world, just stacking pointless draw calls*/
            if (ShouldDrawText(typeof (TextElement))) DrawText();
        }


        protected void DrawText()
        {
            var size = Art.Font.MeasureString(Text)*TextScale;
            Game.SpriteBatch.DrawString(
                Art.Font, Text, new Vector2(Rect.X + (Rect.Width - size.X)/2, Rect.Y + (Rect.Height - size.Y)/2),
                TextColor, 0f, Vector2.Zero, TextScale, SpriteEffects.None, 0);
        }
    }
}