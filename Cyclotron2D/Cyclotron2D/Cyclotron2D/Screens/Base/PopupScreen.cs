using System;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Screens.Base
{
    /// <summary>
    /// This is a Popups Screen, It is associated to a MainScreen
    /// The relative ordering between popupScreens depends on the order of their creation.
    /// They will dissapear with their parents if game state changes.
    /// </summary>
    public class PopupScreen : Screen
    {
        private bool m_oldEnabled;
        private bool m_oldVisible;

        public PopupScreen(Game game, MainScreen parent) : base(game)
        {
            Parent = parent;
            SubscribeParent();
            Background = Color.Transparent;
        }



    /// <summary>
        /// Space occupied by the popupScreen
        /// </summary>
        public Rectangle Rect { get; set; }

        public Color Background { get; set; }

        public MainScreen Parent { get; private set; }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Game.SpriteBatch.Draw(Art.Pixel, Rect, Background);
        }

        public void Close()
        {
            Game.ScreenManager.RemoveScreen(this);
        }

        #region Subscribtion

        private void SubscribeParent()
        {
            Parent.VisibleChanged += OnParentVisibleChanged;
            Parent.EnabledChanged += OnParentEnabledChanged;
        }

        private void UnsubscribeParent()
        {
            Parent.VisibleChanged -= OnParentVisibleChanged;
            Parent.EnabledChanged -= OnParentEnabledChanged;
        }

        #endregion

        #region Event Handlers

        private void OnParentEnabledChanged(object sender, EventArgs e)
        {
            if (Parent.Enabled)
            {
                Enabled = m_oldEnabled;
            }
            else
            {
                m_oldEnabled = Enabled;
                Enabled = false;
            }
        }

        private void OnParentVisibleChanged(object sender, EventArgs e)
        {
            if (Parent.Visible)
            {
                Visible = m_oldVisible;
            }
            else
            {
                m_oldVisible = Visible;
                Visible = false;
            }
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing && Parent != null)
            {
                UnsubscribeParent();
                Parent = null;
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}