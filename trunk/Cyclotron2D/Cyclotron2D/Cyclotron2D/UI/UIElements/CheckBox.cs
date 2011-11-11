using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UI.UIElements
{
    /// <summary>
    /// this is only a checkbox in the most traditional sense :)
    /// </summary>
    public class CheckBox : TextElement
    {
        public bool IsChecked { get; set; }

        public string CheckedText { get; set; }
        public string UncheckedText { get; set; }

        public CheckBox(Game game, Screen screen) : base(game, screen)
        {
            CheckedText = "Yes";
            UncheckedText = "No";
            Background = Color.BlanchedAlmond;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Text = IsChecked ? "Yes" : "No";
            TextColor = IsChecked ? Color.Green : Color.Red;


        }

        protected override void HandleInupt(GameTime gameTime)
        {
            base.HandleInupt(gameTime);
            if (IsMouseOver && Game.InputState.IsNewLeftClick)
            {
                IsChecked = !IsChecked;
            }
        }

       


    }
}
