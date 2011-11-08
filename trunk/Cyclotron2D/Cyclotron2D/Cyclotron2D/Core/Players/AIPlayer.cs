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

        #region Public Methods

        public override void Initialize(Cycle cycle, int id)
        {
            base.Initialize(cycle, id);

            m_rand = new Random(DateTime.Now.Millisecond / PlayerID);

        }


      

        /// <summary>
        /// The AI is getting smarter o.O
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            bool turned = AvoidWalls();

            if(!turned)
            {
                NonRetartedRandomTurn(gameTime);
            }
            else
            {
                m_lastTurn = gameTime.TotalGameTime;
            }
        }

        #endregion

        #region Private Methods

        private bool AvoidWalls()
        {
            int safeLimit = 2;

            int width = Game.GraphicsDevice.Viewport.Bounds.Width;
            int height = Game.GraphicsDevice.Viewport.Bounds.Height;
            bool turned = false;
            switch (Cycle.Direction)
            {
                case Direction.Up:
                    if (Cycle.Position.Y < safeLimit)
                    {

                        Direction turn = Cycle.Position.X > width / 2 ? Direction.Left : Direction.Right;

                        InvokeDirectionChange(new DirectionChangeEventArgs(turn, Cycle.GetNextGridCrossing()));
                        turned = true;
                    }
                    break;
                case Direction.Down:
                    if (Cycle.Position.Y > height-safeLimit)
                    {

                        Direction turn = Cycle.Position.X > width / 2 ? Direction.Left : Direction.Right;

                        InvokeDirectionChange(new DirectionChangeEventArgs(turn, Cycle.GetNextGridCrossing()));
                        turned = true;
                    }
                    break;
                case Direction.Left:
                    if (Cycle.Position.X < safeLimit)
                    {

                        Direction turn = Cycle.Position.Y > height / 2 ? Direction.Up : Direction.Down;

                        InvokeDirectionChange(new DirectionChangeEventArgs(turn, Cycle.GetNextGridCrossing()));
                        turned = true;
                    }
                    break;
                case Direction.Right:
                    if (Cycle.Position.Y > width - safeLimit)
                    {

                        Direction turn = Cycle.Position.Y > height / 2 ? Direction.Up : Direction.Down;

                        InvokeDirectionChange(new DirectionChangeEventArgs(turn, Cycle.GetNextGridCrossing()));
                        turned = true;
                    }
                    break;
            }

            return turned;

        }

        private void NonRetartedRandomTurn(GameTime gameTime)
        {
            var dirs = new[] { Direction.Left, Direction.Up, Direction.Down, Direction.Right };

            if (m_lastTurn == new TimeSpan(0))
                m_lastTurn = gameTime.TotalGameTime;

            if (gameTime.TotalGameTime - m_lastTurn > new TimeSpan(0, 0, 0, 0, 500))
            {
                if (Cycle.Direction == Direction.Up || Cycle.Direction == Direction.Down)
                {
                    if (Cycle.Position.X < 10)
                    {
                        dirs = new []{Direction.Right};
                    }else if (Cycle.Position.X > Game.GraphicsDevice.Viewport.Bounds.Width-10)
                    {
                        dirs = new [] { Direction.Left };
                    }
                    else
                    {
                        dirs = new [] { Direction.Right, Direction.Left };
                    }
                }
                else
                {
                    if (Cycle.Position.Y < 10)
                    {
                        dirs = new[] { Direction.Down };
                    }
                    else if (Cycle.Position.Y > Game.GraphicsDevice.Viewport.Bounds.Height - 10)
                    {
                        dirs = new[] { Direction.Up };
                    }
                    else
                    {
                        dirs = new[] { Direction.Down, Direction.Up };
                    }
                }

                int i = m_rand.Next(0, dirs.Length * 3);

                if (i < dirs.Length)
                {
                    InvokeDirectionChange(new DirectionChangeEventArgs(dirs[m_rand.Next(0, dirs.Length)], Cycle.GetNextGridCrossing()));
                }

                m_lastTurn = gameTime.TotalGameTime;
            }
        }

        #endregion
    }
}