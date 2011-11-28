using System;
using System.Diagnostics;
using Cyclotron2D.Graphics;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Cyclotron2D.UI.UIElements
{
    public class Menu : StretchPanel
    {

        #region Properties

        public String Title { get; set; }

        public MenuItem SelectedItem { get; private set; }

        public int SelectedIndex { get; private set; }

        public MenuItem PreviewItem { get; set; }

        /// <summary>
        /// Semy transparent color that will tint the previewed Item.
        /// </summary>
        public Color PreviewTint { get; set; }

        #endregion

        #region Constructor

        public Menu(Game game, Screen screen) : base(game, screen)
        {
            ItemSpacing = 2;
            PreviewTint = new Color(0, 148, 255, 100);
            Reset();
        }

        #endregion

        #region Events

        public event EventHandler SelectionChanged;

        public void InvokeSelectionChanged()
        {
            EventHandler handler = SelectionChanged;
            if (handler != null) handler(this, new EventArgs());
        }

        #endregion

        #region Public Methods

        private TimeSpan m_waitTime = TimeSpan.Zero;

        /// <summary>
        /// so that you cant add any old UIElement to a Menu
        /// </summary>
        /// <param name="items"></param>
        private new void AddItems(params UIElement[] items)
        {
        }

        private new void RemoveItem(params UIElement[] items)
        {
        }

        public void AddItems(params MenuItem[] item)
        {
            base.AddItems(item);
        }

        public void RemoveItem(MenuItem item)
        {
            if (SelectedItem == item)
            {
                SelectedItem = null;
                SelectedIndex = -1;
            }
            if (PreviewItem == item)
            {
                int pIndex = Items.IndexOf(item);
                pIndex = (pIndex + 1)%(Items.Count - 1);
                PreviewItem = Items[pIndex] as MenuItem;
            }

            base.RemoveItem(item);
        }

        public void Reset()
        {
            SelectedItem = null;
            SelectedIndex = -1;
            PreviewItem = Items.Count > 0 ? Items[0] as MenuItem : null;
        }

        public void Select(MenuItem menuItem)
        {
            Debug.Assert(menuItem.Menu == this, "Trying to select an item from another menu ...");

            if (SelectedItem == null || SelectedItem != menuItem)
            {
                if (SelectedItem != null)
                    SelectedItem.IsSelected = false;

                SelectedItem = menuItem;
                SelectedItem.IsSelected = true;
                SelectedIndex = Items.IndexOf(SelectedItem);
                InvokeSelectionChanged();
            }
        }

        protected override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);
            int pIndex = Items.IndexOf(PreviewItem);

            if (Game.InputState.IsNewKeyPress(Keys.Enter))
            {
                int sIndex = Items.IndexOf(PreviewItem);
                if (sIndex != SelectedIndex)
                {
                    Select(Items[sIndex] as MenuItem);
                }
            }
            else if (Game.InputState.IsNewKeyPress(Keys.Down))
            {
                pIndex = (pIndex + 1)%Items.Count;
                PreviewItem = Items[pIndex] as MenuItem;
                m_waitTime = TimeSpan.Zero;
            }
            else if (Game.InputState.IsNewKeyPress(Keys.Up))
            {
                pIndex = (pIndex - 1)%Items.Count;
                if (pIndex < 0) pIndex += Items.Count; //for some reason C# does not know how to handle negative modulus
                PreviewItem = Items[pIndex] as MenuItem;
                m_waitTime = TimeSpan.Zero;
            }

            //for holding down the inputs we needed a max refresh period
            if (m_waitTime < new TimeSpan(0, 0, 0, 0, 80))
            {
                m_waitTime += gameTime.ElapsedGameTime;
                return;
            }

            if (Game.InputState.IsKeyDown(Keys.Down))
            {
                pIndex = (pIndex + 1)%Items.Count;
                PreviewItem = Items[pIndex] as MenuItem;
                m_waitTime = TimeSpan.Zero;
            }
            else if (Game.InputState.IsKeyDown(Keys.Up))
            {
                pIndex = (pIndex - 1)%Items.Count;
                if (pIndex < 0) pIndex += Items.Count; //for some reason C# does not know how to handle negative modulus
                PreviewItem = Items[pIndex] as MenuItem;
                m_waitTime = TimeSpan.Zero;
            }
        }


        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            //draw diff color for Preview item
            Game.SpriteBatch.Draw(Art.Pixel, PreviewItem.Rect, PreviewTint);
        }

        #endregion

       
    }
}