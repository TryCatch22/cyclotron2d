﻿using System;
using Cyclotron2D.Helpers;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;
using Cyclotron2D.Sounds;

namespace Cyclotron2D.UI.UIElements
{
    public class Countdown : TextElement
    {
        private static readonly TimeSpan OneSecond = new TimeSpan(0, 0, 0, 1);

        /// <summary>
        /// Time in seconds left on the timer
        /// </summary>
        public int Value { get; set; }

        public bool ScaleText { get; set; }

        public float MaxScaleFactor { get; set; }

        public float StartTextScale { get; set; }

        private int m_startValue;

        private TimeSpan m_startTime;

        public Countdown(Game game, Screen screen) : base(game, screen)
        {
            m_startTime = TimeSpan.MaxValue;
            ScaleText = false;
            MaxScaleFactor = 2;
        }

        public void Start()
        {
            m_startTime = Game.GameTime.TotalGameTime;
            m_startValue = Value;
            Text = Value.ToString();
			Sound.PlaySound(Sound.BlipLow, 1.0f);
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Value == 0)
            {
                return;
            }

            // decrement count every second

            var elapsedTime = gameTime.TotalGameTime - m_startTime;
            var nextTick = OneSecond.Mult(m_startValue - Value + 1);

            if (elapsedTime >= nextTick)
            {

                Value--;

				// play countdown sound
				if (Value > 0)
				{
					Sound.PlaySound(Sound.BlipLow, 1.0f);
				}
				else if (Value == 0)
				{
					Sound.PlaySound(Sound.BlipHigh, 1.0f);
				}

                Text = Value.ToString();
                TextScale = StartTextScale;  // resets number size after incrementing
            }
            // increment size of number
            else
            {
                TextScale = MaxScaleFactor - (MaxScaleFactor - 1)*((nextTick - elapsedTime).Ticks/(float)OneSecond.Ticks);
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
