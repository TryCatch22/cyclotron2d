using System;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Core.Players
{
    /// <summary>
    /// Automated Player generates his own turning decisions based on game state
    /// </summary>
    public class AIPlayer : Player
    {
        private TimeSpan m_lastTurn = new TimeSpan(0);
        private Random m_rand;

        public AIPlayer(Game game, Screen screen) : base(game, screen)
        {
            
        }

        public override void Initialize(Cycle cycle, int id)
        {
            base.Initialize(cycle, id);

            m_rand = new Random(DateTime.Now.Millisecond / PlayerID);

        }

        /// <summary>
        /// The AI is currently very stupid, it randomly turns every 3 seconds
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var dirs = new[] {-1, 1, -2, 2};

            if (m_lastTurn == new TimeSpan(0))
                m_lastTurn = gameTime.TotalGameTime;

            if (gameTime.TotalGameTime - m_lastTurn > new TimeSpan(0, 0, 0, 0, 500))
            {
                if (Cycle.Direction == Direction.Up || Cycle.Direction == Direction.Down)
                {
                    dirs = new[] {-1, 1};
                }
                else
                {
                    dirs = new[] {-2, 2};
                }

                int i = m_rand.Next(0, dirs.Length*2);

                if (i < dirs.Length)
                {
                     InvokeDirectionChange(new DirectionChangeEventArgs((Direction) dirs[m_rand.Next(0, dirs.Length)], Cycle.GetNextGridCrossing()));
                }
               
                m_lastTurn = gameTime.TotalGameTime;
            }
        }
    }
}