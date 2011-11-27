using System;
using System.Collections.Generic;
using System.Linq;
using Cyclotron2D.Helpers;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.Screens.Main;
using Cyclotron2D.State;
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

        public override string Name { get { return "AIPlayer"; } set { } }

        public override void Initialize(Cycle cycle)
        {
            base.Initialize(cycle);

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

            if(!LastMinuteSave())
            {
                SemiSmartRandomTurn(gameTime, 0.5f);
            }
        }


        #endregion

        #region Private Methods


        private Direction GetSafestDirection(Direction[] dirs)
        {
            if (dirs.Length == 0)
            {
                return Cycle.Direction;
            }

            dirs = safestHelper(dirs.ToList(), 1);

            if (dirs.Length > 0 && (!dirs.Contains(Cycle.Direction) || m_rand.Next(0, 3) == 0))
            {
                return dirs[m_rand.Next(0, dirs.Length)];
            }

            //you are probably about to die
            return Cycle.Direction;
            

            
        }

        private Direction[] safestHelper(List<Direction> dirs, int depth)
        {
            int maxdepth = Cycle.Grid.GridSize.X*2/3;

            var newDirs = dirs.Where(direction => IsSafe(direction, depth)).ToList();

            if (newDirs.Count == 1)
            {
                return newDirs.ToArray();
            }

            if (newDirs.Count > 1 && depth <= maxdepth)
            {
                return safestHelper(newDirs, depth*2);
            }
            return dirs.ToArray();
        }

       private static Direction[] Perpendicular(Direction dir)
       {
           if (dir == Direction.Up || dir == Direction.Down)
           {
               return new[] { Direction.Left, Direction.Right };
           }
           return new[] { Direction.Up, Direction.Down };
       }

       private static Direction[] NotBack(Direction dir)
       {
           if (dir == Direction.Up || dir == Direction.Down)
           {
               return new[] { Direction.Left, Direction.Right, dir };
           }
           return new[] { Direction.Up, Direction.Down, dir };
       }

        private bool LastMinuteSave()
        {
            if (!IsSafe(Cycle.Direction, 3))
            {
                CallTurn(GetSafestDirection(Perpendicular(Cycle.Direction)));
                return true;
            }

            return false;
        }

        private void CallTurn(Direction dir)
        {
            InvokeDirectionChange(new DirectionChangeEventArgs(dir, Cycle.GetNextGridCrossing()));
            m_lastTurn = Game.GameTime.TotalGameTime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dir">direction to head in</param>
        /// <param name="distance">distance in grid coordinates</param>
        /// <returns></returns>
        private bool IsSafe(Direction dir, int distance)
        {
            distance *= Cycle.Grid.PixelsPerInterval;
            var lines = Cycle.GetLines();

            if (lines.Count == 0) return true;

            var headLine = lines[lines.Count - 1];
            if (dir == Cycle.Direction)
            {
                switch (Cycle.Direction)
                {
                    case Direction.Down:
                        headLine.End = new Point(headLine.End.X, headLine.End.Y + distance);
                        break;
                    case Direction.Up:
                        headLine.End = new Point(headLine.End.X, headLine.End.Y - distance);
                        break;
                    case Direction.Right:
                        headLine.End = new Point(headLine.End.X + distance, headLine.End.Y);
                        break;
                    case Direction.Left:
                        headLine.End = new Point(headLine.End.X - distance, headLine.End.Y);
                        break;
                }
            }
            else
            {
                switch (dir)
                {
                    case Direction.Up:
                        headLine = new Line(Cycle.Position, new Point(Cycle.Position.X, Cycle.Position.Y - distance));
                        break;
                    case Direction.Down:
                        headLine = new Line(Cycle.Position, new Point(Cycle.Position.X, Cycle.Position.Y + distance));
                        break;
                    case Direction.Left:
                        headLine = new Line(Cycle.Position, new Point(Cycle.Position.X - distance, Cycle.Position.Y));
                        break;
                    case Direction.Right:
                        headLine = new Line(Cycle.Position, new Point(Cycle.Position.X + distance, Cycle.Position.Y));
                        break;
                }
            }
            Player killer;
            bool hh;
            return !(Cycle.IsOutsideGrid(headLine.End) || Cycle.CheckHeadLine(headLine, out killer, out hh));
        }



        private void SemiSmartRandomTurn(GameTime gameTime, float turnOdds)
        {
            turnOdds = MathHelper.Clamp(0, turnOdds, 1);

            if (m_lastTurn == new TimeSpan(0))
                m_lastTurn = gameTime.TotalGameTime;

            if (gameTime.TotalGameTime - m_lastTurn > new TimeSpan(0, 0, 0, 0, 500))
            {

                if (m_rand.Next(0, (int)(1/turnOdds)) == 0)
                {
                    CallTurn(GetSafestDirection(NotBack(Cycle.Direction)));
                }
                m_lastTurn = gameTime.TotalGameTime;
            }
        }

        #endregion


        protected override void OnCycleCollided(object sender, CycleCollisionEventArgs e)
        {
            base.OnCycleCollided(sender, e);
            if (Game.IsState(GameState.PlayingAsClient | GameState.PlayingAsHost))
            {
                switch (e.Type)
                {
                    case CollisionType.Self:
                    case CollisionType.Suicide:
                    case CollisionType.Wall:
                        {
                            //we killed ourselves some way or another. we are the authoritative source for these cases
                            GameScreen.CollisionNotifier.NotifyRealDeath(this);
                            Cycle.Kill();
                        }
                        break;
                    case CollisionType.Player:
                        {
                            if (!e.AmbiguousCollision)
                            {
                                //we collided into a confirmed portion of the other players tail
                                GameScreen.CollisionNotifier.NotifyRealDeath(this);
                                Cycle.Kill();

                            }
                        }
                        break;
                    default:
                        break;
                }

            }
        }

        protected override void OnCycleEnabledChanged(object sender, EventArgs e)
        {
            Enabled = Cycle.Enabled;
        }
    }
}