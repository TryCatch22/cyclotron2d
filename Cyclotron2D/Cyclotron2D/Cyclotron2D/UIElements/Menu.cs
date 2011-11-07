using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Cyclotron2D.UIElements
{
    public class Menu : UIElement
    {
        #region Fields

        private List<MenuItem> m_items;

        #endregion

        #region Properties

        public String Title { get; set; }

        public MenuItem SelectedItem { get; private set; }

        public int SelectedIndex { get; private set; }

        public MenuItem PreviewItem { get; set; }

        public int ItemSpacing { get; set; }

        /// <summary>
        /// Semy transparent color that will tint the previewed Item.
        /// </summary>
        public Color PreviewTint { get; set; }

        #endregion

        #region Constructor

        public Menu(Game game, Screen screen) : base(game, screen)
        {
            m_items = new List<MenuItem>();
            ItemSpacing = 2;
            PreviewTint = new Color(0, 100, 120, 120);
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

        public void AddItem(MenuItem item)
        {
            m_items.Add(item);
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
                int pIndex = m_items.IndexOf(item);
                pIndex = (pIndex + 1)%(m_items.Count - 1);
                PreviewItem = m_items[pIndex];
            }

            m_items.Remove(item);
            item.Dispose();
        }

        public void Reset()
        {
            SelectedItem = null;
            SelectedIndex = -1;
            PreviewItem = m_items.Count > 0 ? m_items[0] : null;
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
                SelectedIndex = m_items.IndexOf(SelectedItem);
                InvokeSelectionChanged();
            }
        }

        protected override void HandleInupt(GameTime gameTime)
        {
            base.HandleInupt(gameTime);
            int pIndex = m_items.IndexOf(PreviewItem);

            if (Game.InputState.IsNewKeyPress(Keys.Enter))
            {
                int sIndex = m_items.IndexOf(PreviewItem);
                if (sIndex != SelectedIndex)
                {
                    Select(m_items[sIndex]);
                }
            }
            else if (Game.InputState.IsNewKeyPress(Keys.Down))
            {
                pIndex = (pIndex + 1)%m_items.Count;
                PreviewItem = m_items[pIndex];
                m_waitTime = TimeSpan.Zero;
            }
            else if (Game.InputState.IsNewKeyPress(Keys.Up))
            {
                pIndex = (pIndex - 1)%m_items.Count;
                if (pIndex < 0) pIndex += m_items.Count; //for some reason C# does not know how to handle negative modulus
                PreviewItem = m_items[pIndex];
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
                pIndex = (pIndex + 1)%m_items.Count;
                PreviewItem = m_items[pIndex];
                m_waitTime = TimeSpan.Zero;
            }
            else if (Game.InputState.IsKeyDown(Keys.Up))
            {
                pIndex = (pIndex - 1)%m_items.Count;
                if (pIndex < 0) pIndex += m_items.Count; //for some reason C# does not know how to handle negative modulus
                PreviewItem = m_items[pIndex];
                m_waitTime = TimeSpan.Zero;
            }
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            int i = 0;
            foreach (MenuItem menuItem in m_items)
            {
                int height = (Rect.Height - ItemSpacing*(m_items.Count + 1))/m_items.Count;
                int dist = Rect.Height/m_items.Count;
                menuItem.Rect = new Rectangle(Rect.X, Rect.Y + i++*dist, Rect.Width, height);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            foreach (var item in m_items.Where(itm => itm.Visible))
            {
                item.Draw(gameTime);
            }
            //draw diff color for Preview item
            Game.SpriteBatch.Draw(Art.Pixel, PreviewItem.Rect, PreviewTint);
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing && m_items != null)
            {
                foreach (MenuItem menuItem in m_items)
                {
                    menuItem.Dispose();
                }
                m_items = null;
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}