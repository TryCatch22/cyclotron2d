using System;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UIElements
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

        protected override void HandleInupt(GameTime gameTime)
        {
            base.HandleInupt(gameTime);

            if (IsMouseOver)
            {
                Menu.PreviewItem = this;
            }
        }

        protected override void OnClick(object sender, EventArgs e)
        {
            Menu.Select(this);
        }
    }
}