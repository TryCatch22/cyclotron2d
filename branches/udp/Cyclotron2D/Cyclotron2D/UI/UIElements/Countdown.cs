using System;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UI.UIElements
{
    public class Countdown : TextElement
    {
        private static readonly TimeSpan OneSecond = new TimeSpan(0, 0, 0, 1);

        /// <summary>
        /// Time in seconds left on the timer
        /// </summary>
        public int Value { get; set; }

        private int m_startValue;

        private TimeSpan m_startTime;

        public Countdown(Game game, Screen screen) : base(game, screen)
        {
            m_startTime = TimeSpan.MaxValue;
        }

        public void Start()
        {
            m_startTime = Game.GameTime.TotalGameTime;
            m_startValue = Value;
            Text = Value.ToString();
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
                Text = Value.ToString();
                TextScale = 3f;  // resets number size after incrementing
            }
            // increment size of number
            else
            {
                TextScale *= 1.05f;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (ShouldDrawText(typeof(Countdown)) && Value > 0)
            {
                DrawText();
            }
        }


       

    }
}
