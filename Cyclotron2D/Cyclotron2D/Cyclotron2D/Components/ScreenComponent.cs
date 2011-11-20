using System;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Components
{
    /// <summary>
    /// Base Class For any drawable component that belongs to a specific Screen, that is nearly all of them
    /// these components will be disabled or rendered invisible if their associated screen is, 
    /// they will revert to their previous state if their screen becomes visible or enabled again
    /// Please don't forget to dispose these when you are done with them
    /// </summary>
    public abstract class DrawableScreenComponent : DrawableCyclotronComponent
    {
        private bool m_oldEnabled;
        private bool m_oldVisible;

        protected DrawableScreenComponent(Game game, Screen screen)
            : base(game)
        {
            Screen = screen;
            m_oldVisible = Visible;
            m_oldEnabled = Enabled;
            SubscribeScreen();
        }

        public Screen Screen { get; private set; }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Screen.HasFocus)
            {
                HandleInupt(gameTime);
            }
        }

        protected virtual void HandleInupt(GameTime gameTime)
        {
        }

        #region Event Handlers

        private void OnScreenVisibleChanged(object sender, EventArgs e)
        {
            if (Screen.Visible)
            {
                Visible = m_oldVisible;
            }
            else
            {
                m_oldVisible = Visible;
                Visible = false;
            }
        }

        private void OnScreenEnabledChanged(object sender, EventArgs e)
        {
            if (Screen.Enabled)
            {
                Enabled = m_oldEnabled;
            }
            else
            {
                m_oldEnabled = Enabled;
                Enabled = false;
            }
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeScreen();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Subscribtion

        private void SubscribeScreen()
        {
            Screen.VisibleChanged += OnScreenVisibleChanged;
            Screen.EnabledChanged += OnScreenEnabledChanged;
        }

        private void UnsubscribeScreen()
        {
            Screen.VisibleChanged -= OnScreenVisibleChanged;
            Screen.EnabledChanged -= OnScreenEnabledChanged;
        }

        #endregion
    }

    /// <summary>
    /// Base Class For any component that belongs to a specific Screen
    /// these components will be disabled if their associated screen is, 
    /// they will revert to their previous state if their screen becomes enabled again
    /// Please don't forget to dispose these when you are done with them
    /// </summary>
    public abstract class ScreenComponent : CyclotronComponent
    {
        private bool m_oldEnabled;

        protected ScreenComponent(Game game, Screen screen)
            : base(game)
        {
            Screen = screen;
            Enabled = Screen.Enabled;
            m_oldEnabled = true;
            SubscribeScreen();
        }

        public Screen Screen { get; private set; }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Screen.HasFocus)
            {
                HandleInupt(gameTime);
            }
        }

        protected virtual void HandleInupt(GameTime gameTime)
        {
        }

        #region Event Handlers

        private void OnScreenEnabledChanged(object sender, EventArgs e)
        {
            if (Screen.Enabled)
            {
                Enabled = m_oldEnabled;
            }
            else
            {
                m_oldEnabled = Enabled;
                Enabled = false;
            }
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeScreen();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Subscribtion

        private void SubscribeScreen()
        {
            Screen.EnabledChanged += OnScreenEnabledChanged;
        }

        private void UnsubscribeScreen()
        {
            Screen.EnabledChanged -= OnScreenEnabledChanged;
        }

        #endregion
    }
}