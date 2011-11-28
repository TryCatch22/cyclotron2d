using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;
using Cyclotron2D.Sounds;

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
			Background = new Color(0, 148, 255, 255);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Text = IsChecked ? "Yes" : "No";
            TextColor = IsChecked ? Color.White : Color.Black;


        }

        protected override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);
            if (IsMouseOver && Game.InputState.IsNewLeftClick)
            {
                IsChecked = !IsChecked;
				Sound.PlaySound(Sound.Clink, 0.5f);
            }
        }

       


    }
}
