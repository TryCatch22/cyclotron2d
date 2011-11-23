using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cyclotron2D.Helpers;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Core
{
    class Countdown : UIElement
    {
        private StretchPanel m_countdownPanel;
        private TextElement m_countText;
        private TimeSpan m_seconds;
        private int m_count = 0;
        private Rectangle m_rect;

        public TimeSpan StartGameTime;

        public Countdown(Game game, Screen screen)
            : base(game, screen)
        {
            m_seconds = new TimeSpan(0, 0, 1);

            Rectangle vp = Game.GraphicsDevice.Viewport.Bounds;
            m_rect = new Rectangle(vp.Width * 45 / 100, vp.Height * 45 / 100, vp.Width / 10, vp.Height / 10);

            m_countdownPanel = new StretchPanel(game, screen);
            m_countText = new TextElement(game, screen)
                            {   Text = "0",
                                TextScale = 3f,
                                TextAlign = UI.UIElements.TextAlign.Center,
                                TextColor = Color.Red };
        }

        public void Initialize(TimeSpan startGameTime, int count)
        {
            StartGameTime = startGameTime;
            m_count = count;
            m_seconds = new TimeSpan(0, 0, 1);
            m_countText.Text = m_count.ToString();

            m_countdownPanel.AddItems(m_countText);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (m_countdownPanel.Visible)
                m_countdownPanel.Draw(gameTime);
            if (m_countText.Visible)
                m_countText.Draw(gameTime);
        }

        protected override void HandleInput(GameTime gameTime)
        {

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // decrement count every second
            if (StartGameTime != null && gameTime.TotalGameTime - StartGameTime >= m_seconds)
            {
                m_seconds = m_seconds + new TimeSpan(0, 0, 1);
                m_count--;

                m_countText.Text = m_count.ToString();
                m_countText.TextScale = 3f;  // resets number size after incrementing
            }
            // increment size of number
            else
            {
                m_countText.TextScale = m_countText.TextScale*1.05f;
            }

            m_countdownPanel.Rect = m_rect;
        }

    }
}
