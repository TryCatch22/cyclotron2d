using System;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UI.UIElements
{
    public class Countdown : UIElement
    {
        private static readonly TimeSpan OneSecond = new TimeSpan(0, 0, 0, 1);

        private TextElement m_countText;

        public int Value { get; set; }

        private int m_startValue;

        private TimeSpan m_startTime;

        public Countdown(Game game, Screen screen) : base(game, screen)
        {
            m_countText = new TextElement(game, screen) {TextScale = 3f, TextColor = Color.Red};
        }

        public void Start()
        {
            m_startTime = Game.GameTime.TotalGameTime;
            m_startValue = Value;
            m_countText.Text = Value.ToString();
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Value == 0)
            {
                return;
            }

            // decrement count every second
            if (gameTime.TotalGameTime - m_startTime >= new TimeSpan((m_startValue - Value + 1) * OneSecond.Ticks))
            {
                Value--;
                m_countText.Text = Value.ToString();
                m_countText.TextScale = 3f;  // resets number size after incrementing
            }
            // increment size of number
            else
            {
                m_countText.TextScale = m_countText.TextScale * 1.05f;
            }
            //update rect
            m_countText.Rect = Rect;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (m_countText.Visible && Value > 0)
                m_countText.Draw(gameTime);
        }


       

    }
}
