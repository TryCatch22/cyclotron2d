using System;
using Microsoft.Xna.Framework;
using Cyclotron2D.Sounds;

namespace Cyclotron2D.UI.UIElements
{
    public class MenuItem : Button
    {
        public MenuItem(Menu menu, String text) : base(menu.Game, menu.Screen)
        {
            Menu = menu;
            Text = text;
        }

        public Menu Menu { get; private set; }

        public bool IsSelected { get; set; }

        protected override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);

            if (IsMouseOver)
            {
                Menu.PreviewItem = this;
            }
        }

        protected override void OnClick(object sender, EventArgs e)
        {
            Menu.Select(this);
			Sound.PlaySound(Sound.Clink, 0.5f);
        }
    }
}