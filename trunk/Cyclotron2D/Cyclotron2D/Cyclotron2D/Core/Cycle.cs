using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cyclotron2D.Components;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Graphics;
using Cyclotron2D.Helpers;
using Cyclotron2D.Network;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.Screens.Main;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Cyclotron2D.Sounds;

namespace Cyclotron2D.Core
{
    public enum Direction
    {
        Left = -1,
        Right = 1,
        Up = -2,
        Down = 2
    }

    public class Cycle : DrawableScreenComponent
    {
        #region Fields


        private List<Point> m_lastUpdateInfo;
        private Direction m_lastUpdateDir;
        private int msgsDuringFeignDeath;

        private Player m_player;
        private Direction m_scheduledDirection;

        /// <summary>
        /// The list of positions on the map at which we've made turns.
        /// This is used to draw our trail.
        /// </summary>
        private List<Point> m_vertices;
		
        /// <summary>
        /// for udp connections, the average distance from our simulated position to the message position.
        /// </summary>
        private int m_averageLag;

        #endregion

        #region Properties


        // We only turn on grid lines, so if the input is received early, we have to keep track of it.
        public Point? NextTurnIntersection { get; private set; }

        public float Speed { get { return (Screen as GameScreen).GameSettings.CycleSpeed.Value; } }

        public bool AllowSuicide { get { return (Screen as GameScreen).GameSettings.AllowSuicide.Value; } }

        /// <summary>
        /// Screen position of Head in Pixels
        /// </summary>
        public Point Position { get; set; }

        public bool Dead { get; set; }

        public TimeSpan FeigningDeathStart { get; private set; }

        public Color TrailColor { get; set; }

       // public Color BikeColor { get { return new Color(255 - TrailColor.R, 255 - TrailColor.G, 255 - TrailColor.B, 190); } }
        public Color BikeColor { get { return new Color(TrailColor.R, TrailColor.G, TrailColor.B, 100); } }
        /// <summary>
        /// Current heading
        /// </summary>
        public Direction Direction { get; set; }

        /// <summary>
        ///  A reference to the grid we're playing on, to do coordinate conversions
        /// </summary>
        public Grid Grid { get; set; }

        /// <summary>
        /// measured in pixels/game loop
        /// </summary>
        public Vector2 Velocity { get { return DirectionToVelocity(Direction); } }

        /// <summary>
        /// Conversion from Pixels to Grid coords
        /// </summary>
        public Vector2 GridPosition { get { return Grid.ToGridCoords(Position); } }

        /// <summary>
        /// delay before game start
        /// </summary>
        public TimeSpan GameStart { get; set; }

        /// <summary>
        /// total length of tail at the moment
        /// </summary>
        public int TailLength
        {
            get
            {
                return GetLines().Aggregate(0, (current, line) => current + line.Length);
            }
        }

        /// <summary>
        /// max allowed tail length; set to 0 for unlimited tail length
        /// </summary>
        public int MaxTailLength { get { return (Screen as GameScreen).GameSettings.MaxTailLength.Value; } }


        private List<Animation> m_explosionsList;

     //   private GameScreen GameScreen { get { return Screen as GameScreen; } }

        #endregion

        #region Constructor

        public Cycle(Game game, Screen screen, Grid grid, StartCondition info, Player player, List<Animation> explosionsList)
            : base(game, screen)
        {
            GameStart = TimeSpan.MaxValue;
            m_vertices = new List<Point>();
            TrailColor = player.Color;
            Grid = grid;
            Position = grid.ToWorldCoords(info.Position);
            Direction = info.Dir;
            m_player = player;
            m_explosionsList = explosionsList;
            //add start position
            m_vertices.Add(Position);
        }

        #endregion

        #region Events

        public event EventHandler<CycleCollisionEventArgs> Collided;

        public void InvokeCollided(CycleCollisionEventArgs e)
        {
            EventHandler<CycleCollisionEventArgs> handler = Collided;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region Public Methods

        public bool IsOutsideGrid(Point position)
        {
            return position.X < 0 || position.Y < 0 || position.X > Game.GraphicsDevice.Viewport.Width || position.Y > Game.GraphicsDevice.Viewport.Height;
        }



        public void HandleUpdateInfo(Direction dir, List<Point> vertices, bool revive = false)
        {

            m_lastUpdateInfo = vertices;
            m_lastUpdateDir = dir;
            
            //hide class property for revive calls
            Point Position;
            if (!Enabled && !Dead)//feign death
            {
                msgsDuringFeignDeath++;	//Counts incoming msgs while fake "dead"
            }
            else
            {
                msgsDuringFeignDeath = 0;
            }


            if (vertices == null || Dead || !Enabled)
            {
                return;
            }

            if (revive)
            {
                //if reviving, pretend the position is the one from the last update + average lag
                Position = vertices[0].AddOffset(dir, m_averageLag);
            }
            else
            {
                Position = this.Position;
            }


            Point lastTurn = m_vertices[m_vertices.Count - 1];
            int i = 0;

            while(i < vertices.Count && vertices[i] != lastTurn) i++;

          //  Debug.Assert(i < vertices.Count, "have we missed more than 4 turns since the last message ??");
            

            switch (i)
            {
                case 0:
                    //this will most likely never happen maybe at the very start...
                    DebugMessages.AddLogOnly("Investigate matching last turn at current position from message");
                    break;
                case 1:
                    {
                        Debug.Assert(
                            ((Direction == Direction.Down || Direction == Direction.Up) && Position.X == vertices[0].X && Position.X == vertices[1].X) ||
                            ((Direction == Direction.Left || Direction == Direction.Right) && Position.Y == vertices[0].Y && Position.Y == vertices[1].Y), "Something is wrong noob.");

                        //this should be the most common case (all is well), update the average lag here 

                        int lag = (int) Position.Distance(vertices[0]);
                        
                        switch (Direction)
                        {
                            case Direction.Up:
                                if (Position.Y > vertices[0].Y) lag = -lag;
                                break;
                            case Direction.Down:
                                if (Position.Y < vertices[0].Y) lag = -lag;
                                break;
                            case Direction.Right:
                                if (Position.X < vertices[0].X) lag = -lag;
                                break;
                            case Direction.Left:
                                if (Position.X > vertices[0].X) lag = -lag;
                                break;

                        }


                        m_averageLag = (m_averageLag + lag)/2;

                        /*
                         Experimental Code. Reducing the average lag gradually by 'tweaking' the position
                         */

                        Position.AddOffset(Direction, -m_averageLag/(Math.Abs(m_averageLag)));

                        //End Experimental

                        DebugMessages.AddLogOnly(m_player + " handling update info, all normal, avg delay: " + m_averageLag + " pixels");

                    }
                    break;
                case 2:
                    {
                        // the player must have turned since the last message.

                     //   Line l = new Line(vertices[1], vertices[0]);

                        TurnAt(dir, vertices[1]);

                        DebugMessages.AddLogOnly(m_player + "Detected turn, handling ...");

                    }
                    break;
                case 3:
                case 4:
                    {
                        string msg = i == 3 ? "Missed a turn from " + m_player + " trying to catch up ..." 
                                            : "Missed two turns from " + m_player + " pls lag less";

                        DebugMessages.Add(msg);



                        int length =(int)Position.Distance(vertices[i]);

                  

                        int offset = length;

                        int addedPoints = 0;

                        for (int j = i; j > 0; j--)
                        {
                            offset -= (int)vertices[j - 1].Distance(vertices[j]);
                            if(j-1 > 0 && offset > 0)
                            {
                                m_vertices.Add(vertices[j-1]);
                                addedPoints++;
                            }
                        }

                        this.Position = vertices[i - addedPoints].AddOffset(dir, offset);
                        Direction = dir;




                    }
                    break;
                default:
                    DebugMessages.Add("player " + m_player.PlayerID +" in SERIOUS trouble here");
                    break;

            }
            
        }

        public NetworkMessage GetInfoMessage()
        {
            string content = "";

            content += (int) Direction + "\n";

            content += Position + "\n";

            for (int i = m_vertices.Count - 1; i >= 0 && i >= m_vertices.Count - 4; i--)
            {
                content += m_vertices[i] + "\n";
            }

            return new NetworkMessage(MessageType.PlayerInfoUpdate, content);
        }

        public bool CheckHeadLine(Line myline, out Player killer, out bool headlineCollision)
        {
            //hiding the class scope position Vector here caus this method can be used to check for future colisions.
            var Position = myline.End;
            bool hasCollision = false;
            headlineCollision = false;

            killer = null;

            foreach (var cycle in Grid.Cycles)
            {
                var lines = cycle.GetLines();

                if (lines.Count == 0) continue;

                if (cycle != this)
                {
                    Debug.Assert(cycle.m_player.PlayerID != m_player.PlayerID, "Cycle to player association fuckup. Or PlayerID fuckup");
                    
                    //special case for comparing 2 head lines so only 1 player dies

                    Line head = lines[lines.Count - 1];
                    Vector2? intersection;
                    var intersectType = Line.FindIntersection(head, myline, out intersection);

                    if (intersectType == IntersectionType.Point)
                    {
                        float mydist = intersection.Value.Distance(Position);
                        float hisdist = intersection.Value.Distance(cycle.Position);
                        if (mydist <= hisdist)
                        {
                            hasCollision = true;
                            headlineCollision = true;
                            killer = cycle.m_player;
                            break;
                        }
                    }
                    else if (intersectType == IntersectionType.Collinear)
                    {
                        float distheads = Position.Distance(cycle.Position);
                        float distMe = Position.Distance(head.Start);
                        float distHim = cycle.Position.Distance(myline.Start);

                        if ((distheads <= distMe && distheads <= distHim) || distMe <= distHim)
                        {
                            hasCollision = true;
                            headlineCollision = true;
                            killer = cycle.m_player;
                            break;
                        }
                    }
                    lines.RemoveAt(lines.Count - 1);
                }
                else
                {
                    //this is us we can remove the first 3 lines


                    /*it could be that this is the AI checking a future line, if that is the case it could be an extension
                     or a line at an angle if it is at an angle it is not in lines so it does not need to be removed*/
                    if (lines[lines.Count - 1].Orientation == myline.Orientation) lines.RemoveAt(lines.Count-1);

                     if(lines.Count > 0)lines.RemoveAt(lines.Count - 1);
                     if (lines.Count > 0) lines.RemoveAt(lines.Count - 1);
                }


                if (lines.Aggregate(false, (current, line) => current || Line.FindIntersection(line, myline) != IntersectionType.None))
                {
                    hasCollision = true;
                    killer = cycle.m_player;
                    break;
                }
            }

            return hasCollision;
        }

        public List<Line> GetLines()
        {
            List<Line> lines = new List<Line>();

            for (int i = 0; i < m_vertices.Count - 1; i++)
            {
                lines.Add(new Line(m_vertices[i], m_vertices[i + 1]));
            }
            lines.Add(new Line(m_vertices.Last(), Position));
            
            //can happen if not careful elsewhere
            lines.RemoveAll(line => line.End == line.Start);

            return lines;
        }

        /// <summary>
        /// computes the next Grid crossing in current direction
        /// </summary>
        /// <returns>next Grid crossing in current direction in pixels</returns>
        public Point GetNextGridCrossing()
        {
            Vector2 next;

            switch (Direction)
            {
                case Direction.Up:
                    next = new Vector2(GridPosition.X, (float) Math.Floor(GridPosition.Y));
                    break;
                case Direction.Down:
                    next = new Vector2(GridPosition.X, (float) Math.Ceiling(GridPosition.Y));
                    break;
                case Direction.Right:
                    next = new Vector2((float) Math.Ceiling(GridPosition.X), GridPosition.Y);
                    break;
                case Direction.Left:
                    next = new Vector2((float) Math.Floor(GridPosition.X), GridPosition.Y);
                    break;
                default:
                    throw new Exception("Is there a 5th direction?");
            }

            return Grid.ToWorldCoords(next);
        }

        /// <summary>
        /// Tells the Cycle it will need to turn at the specified Position
        /// </summary>
        /// <param name="direction">new cycle direction</param>
        /// <param name="gridCrossing">In World coordinates (Pixels)</param>
        public void TurnAt(Direction direction, Point gridCrossing)
        {
            if (direction == Direction)
                return;

            var lines = GetLines();
            if (lines.Count == 0)
            {
                return;
            }

            var headLine = lines[lines.Count - 1];

            switch (headLine.Orientation)
            {
                case Orientation.Horizontal:
                    {
                        if(gridCrossing.Y != headLine.End.Y)
                        {
                            DebugMessages.Add("Tcp Lag, or UDp wtf???");
                        }

                    }
                    break;
                case Orientation.Vertical:
                    {
                        if (gridCrossing.X != headLine.End.X)
                        {
                            DebugMessages.Add("Tcp Lag or Udp wtf??");
                        }

                    }
                    break;

            }

            NextTurnIntersection = gridCrossing;
            m_scheduledDirection = direction;
        }

        /// <summary>
        /// to kill a player and update the clients with a final position info
        /// </summary>
        /// <param name="vertices"></param>
        public void Kill(List<Point> vertices)
        {
            Point last = m_vertices[m_vertices.Count - 1];
            int i = 0;
            while (i < vertices.Count && vertices[i] != last) i++;

            if(i > vertices.Count)
            {
                DebugMessages.Add(m_player + " SERIOUS Problem on kill");
            }
            else if (i > 1)
            {
                for (int j = i-1; j > 0; j--)
                {
                    m_vertices.Add(vertices[j]);
                }

                Position = vertices[0];
            }
            else if (i == 1)
            {
                Position = vertices[0];
            }

            Kill();

        }

        /// <summary>
        /// final kill for confirmed dead players
        /// </summary>
        public void Kill()
        {
            Dead = true;
            Enabled = false;
            CreateExplosion();
			Sound.PlaySound(Sound.Boom, 1.0f);

            DebugMessages.Add(m_player + " confirmed dead.");
        }

        /// <summary>
        /// used for ambiguous collisions while waiting for revive or death confirmation
        /// </summary>
        public void FeignDeath()
        {
            FeigningDeathStart = Game.GameTime.TotalGameTime;
            Enabled = false;
            DebugMessages.Add(m_player + " feigns death.");
        }

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);
			Point? lastVertex = null;

			foreach (var vertex in m_vertices)
			{
				if (lastVertex != null)
					DrawLine(lastVertex.Value, vertex);
				lastVertex = vertex;
			}

			DrawLine(m_vertices.Last(), Position);

			if (!Dead)
				Game.SpriteBatch.Draw(Art.Bike, Position.ToVector(), null, BikeColor, (float)Math.PI + Velocity.Orientation(), new Vector2(Art.Bike.Width / 2, Art.Bike.Height / 2), 1f, SpriteEffects.None, 0);
		}

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (gameTime.TotalGameTime < GameStart)
            {
                return;
            }



            Position = new Point((int)(Position.X + Velocity.X), (int)(Position.Y + Velocity.Y));

            // working on adding finite length tails
            LimitTailLength();

            CheckForCollision();
            if (Enabled)
            {//could have been disabled during collision check
                CheckScheduledTurn();
            }
        }

        public bool CycleJustTurned()
        {
            var lines = GetLines();
            if (lines.Count == 0) return false;

            var headLine = lines[lines.Count - 1];

            lines = null;
            Direction lineDirection;
            if (headLine.Start.X == headLine.End.X)
            {
                lineDirection = headLine.Start.Y > headLine.End.Y ? Direction.Up : Direction.Down;
            }
            else
            {
                lineDirection = headLine.Start.X > headLine.End.X ? Direction.Left : Direction.Right;
            }
            //we just turned but have not moved forward yet. Lines are not compatible with heading.
            if (lineDirection != Direction)
            {
                return true;
            }
            return false;
        }

        public void Revive()
        {
            if (!Dead && !Enabled && !m_player.Winner && msgsDuringFeignDeath > 0)
            {
                Enabled = true;
                DebugMessages.Add(m_player + " Reviving");
                HandleUpdateInfo(m_lastUpdateDir, m_lastUpdateInfo, true);
            }

        }

	

        #endregion

        #region Private Methods


        private void CreateExplosion()
        {
            m_explosionsList.Add(new Animation(Game, Screen, Art.ExplosionSheet, new Point(3, 4))
            {
                Position = Position.ToVector(),
                Color = BikeColor,
                Scale = 2f,
                UpdateDelay = new TimeSpan(0, 0, 0, 0, 30)
            });
        }

        private Vector2 DirectionToVelocity(Direction direction)
        {
            switch (direction)
            {
                case Direction.Down:
                    return Vector2.UnitY*Speed;
                case Direction.Left:
                    return -Vector2.UnitX*Speed;
                case Direction.Right:
                    return Vector2.UnitX*Speed;
                case Direction.Up:
                    return -Vector2.UnitY*Speed;
                default:
                    throw new Exception("Is there a fifth direction?");
            }
        }

        private void CheckScheduledTurn()
        {
            if (NextTurnIntersection == null)
                return;

            bool turn = false;
            switch (Direction)
            {
                case Direction.Down:
                    if (Position.Y >= NextTurnIntersection.Value.Y)
                        turn = true;
                    break;
                case Direction.Up:
                    if (Position.Y <= NextTurnIntersection.Value.Y)
                        turn = true;
                    break;
                case Direction.Right:
                    if (Position.X >= NextTurnIntersection.Value.X)
                        turn = true;
                    break;
                case Direction.Left:
                    if (Position.X <= NextTurnIntersection.Value.X)
                        turn = true;
                    break;
                default:
                    throw new Exception("Invalid direction");
            }
            if (turn)
            {
                Turn();
                NextTurnIntersection = null;
            }
        }


        
        // Turn now.
        private void Turn()
        {
            if (NextTurnIntersection.HasValue)
            {
				if ((int)Direction == -(int)m_scheduledDirection)
				{
					if (AllowSuicide)
					{
						DebugMessages.Add(m_player + "committed suicide");
						InvokeCollided(new CycleCollisionEventArgs(CollisionType.Suicide, m_player, false, m_player));
					}
					else
					{
						//cancel turn
						m_scheduledDirection = Direction;
						NextTurnIntersection = null;
					}
					return;
				}
				else if (CycleJustTurned() || m_vertices.Last() == NextTurnIntersection)
				{
					NextTurnIntersection = null;
					return;
				}



                int elapsedDistance = (int)Position.Distance(NextTurnIntersection.Value);

                Direction = m_scheduledDirection;


                m_vertices.Add(NextTurnIntersection.Value);

                Position = NextTurnIntersection.Value.AddOffset(m_scheduledDirection, elapsedDistance);

                
            }
        }

        /// <summary>
        /// Checks if we have collided with any other cycle
        /// </summary>
        private void CheckForCollision()
        {
            var myline = new Line(m_vertices.Last(), Position);

            Player killer = null;

            bool hasCollision = IsOutsideGrid(Position);

            bool headCollision = false;

            if(!hasCollision)
                hasCollision = CheckHeadLine(myline, out killer, out headCollision);

            if (hasCollision)
            {
                string killerString = killer!=null?killer.ToString():"the wall";

                CollisionType type = killer == null ? CollisionType.Wall : killer == m_player ? CollisionType.Self : CollisionType.Player;

                DebugMessages.Add(m_player + " Crashed into " + killerString);

                InvokeCollided(new CycleCollisionEventArgs(type, killer, headCollision, m_player));
            }
        }

        private void DrawLine(Point start, Point end)
        {
            var smaller = start.LengthSquared() < end.LengthSquared() ? start : end;
            var isHorizontal = start.Y == end.Y;

            int width = isHorizontal ?  Math.Abs(start.X - end.X) : 1;
            int height = isHorizontal ? 1 :  Math.Abs(start.Y - end.Y);
            var rect = new Rectangle( smaller.X, smaller.Y, width, height);
            rect.Inflate(isHorizontal ? 0 : 1, isHorizontal ? 1 : 0);

            Game.SpriteBatch.Draw(Art.Pixel, rect, TrailColor);
        }

        /// <summary>
        /// modifieds m_vertices to make sure TailLength not longer than MaxTailLength
        /// </summary>
        private void LimitTailLength()
        {
            int difference;
            Line firstLine;
            Point nextVertice;

            if (MaxTailLength > 0)
            {
                // deletes or shifts the first vertice until TailLength < MaxTailLength
                while (TailLength > MaxTailLength)
                {
                    difference = TailLength - MaxTailLength;

                    //get first line
                    if (m_vertices.Count > 1)
                        nextVertice = m_vertices[1];
                    else
                        nextVertice = Position;

                    firstLine = new Line(m_vertices[0], nextVertice);

                    //if first line is longer than the difference, then just move the last vertice
                    if (firstLine.Length > difference)
                    {
                        //horizontal shift
                        if (m_vertices[0].Y == nextVertice.Y)
                        {
                            if (nextVertice.X > m_vertices[0].X)
                                m_vertices[0] = new Point(m_vertices[0].X + difference, m_vertices[0].Y);
                            else
                                m_vertices[0] = new Point(m_vertices[0].X - difference, m_vertices[0].Y);
                        }
                        //vertical shift
                        else
                        {
                            if (nextVertice.Y > m_vertices[0].Y)
                                m_vertices[0] = new Point(m_vertices[0].X, m_vertices[0].Y + difference);
                            else
                                m_vertices[0] = new Point(m_vertices[0].X, m_vertices[0].Y - difference);
                        }
                    }
                    //otherwise just delete the last vertice
                    else
                        m_vertices.RemoveAt(0);
                }
            }
        }

        #endregion

    }

}