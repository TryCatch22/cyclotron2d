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
            SubscribeCycle();

        }


      

        /// <summary>
        /// The AI is getting smarter o.O
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

//            if (CycleJustTurned())
//            {
//                return;
//            }

            bool turned = LastMinuteSave();

            if (!turned)
            {
                turned =  AvoidWalls();
            }

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


        private Direction getRandDir(Direction[] dirs)
        {
            if (dirs.Length == 0)
            {
                return Cycle.Direction;
            }
            return dirs[m_rand.Next(0, dirs.Length)];
        }

       

        private bool LastMinuteSave()
        {
            bool turned = false;

            var lines = Cycle.GetLines();
            if (lines.Count == 0) return false;

            var headLine = lines[lines.Count - 1].Clone();

            lines = null;

            Direction[] dirs = new Direction[0];
            switch (Cycle.Direction)
            {
                case Direction.Down:
                    headLine.End = new Point(headLine.End.X, headLine.End.Y + 2*Grid.PixelsPerInterval);
                    dirs = new []{Direction.Left, Direction.Right};
                    break;
                case Direction.Up:
                    headLine.End = new Point(headLine.End.X, headLine.End.Y - 2 * Grid.PixelsPerInterval);
                    dirs = new[] { Direction.Left, Direction.Right };
                    break;
                case Direction.Right:
                    headLine.End = new Point(headLine.End.X + 2 * Grid.PixelsPerInterval, headLine.End.Y);
                    dirs = new[] { Direction.Up, Direction.Down };
                    break;
                case Direction.Left:
                    headLine.End = new Point(headLine.End.X - 2 * Grid.PixelsPerInterval, headLine.End.Y);
                    dirs = new[] { Direction.Up, Direction.Down };
                    break;
            }



            Player killer;
            turned = Cycle.CheckHeadLine(headLine, out killer);


            if (turned)
            {
                InvokeDirectionChange(new DirectionChangeEventArgs(getRandDir(dirs), Cycle.GetNextGridCrossing()));
            }

            return turned;
        }


        private bool AvoidWalls()
        {
            int safeLimit = 5;

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
                    InvokeDirectionChange(new DirectionChangeEventArgs(getRandDir(dirs), Cycle.GetNextGridCrossing()));
                }

                m_lastTurn = gameTime.TotalGameTime;
            }
        }

        #endregion


        #region Event Handlers

        private void OnCycleEnabledChanged(object sender, EventArgs e)
        {
            if(!Cycle.Enabled)
            {
                Enabled = false;
            }
        }

        #endregion

        #region Subscription

        private void SubscribeCycle()
        {
            Cycle.EnabledChanged += OnCycleEnabledChanged;
        }


        private void UnsubscribeCycle()
        {
            Cycle.EnabledChanged += OnCycleEnabledChanged;
        }


        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing && Cycle != null)
            {
                UnsubscribeCycle();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}