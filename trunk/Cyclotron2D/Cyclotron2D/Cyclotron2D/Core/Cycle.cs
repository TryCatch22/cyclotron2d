﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cyclotron2D.Components;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Helpers;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cyclotron2D.Core
{
    public enum Direction
    {
        Left = -1,
        Right = 1,
        Up = -2,
        Down = 2
    }


    public struct CycleInfo
    {
        public Color Color;
        public Direction Direction;
        public Vector2 GridPosition;

        /// <summary>
        /// Starting conditions for a Cycle
        /// </summary>
        /// <param name="pos">In Grid Coordinates</param>
        /// <param name="dir"></param>
        /// <param name="c"></param>
        public CycleInfo(Vector2 pos, Direction dir, Color c)
        {
            GridPosition = pos;
            Direction = dir;
            Color = c;
        }
    }

    public class Cycle : DrawableScreenComponent
    {
        #region Fields

        // We only turn on grid lines, so if the input is received early, we have to keep track of it.
        private Point? m_nextGridCrossing;
        private Player m_player;
        private Direction m_scheduledDirection;

        /// <summary>
        /// The list of positions on the map at which we've made turns.
        /// This is used to draw our trail.
        /// </summary>
        private List<Point> m_vertices;

        #endregion

        #region Properties

        public float Speed { get { return Settings.Current.CycleSpeed; } }

        public bool AllowSuicide { get { return Settings.Current.AllowSuicide; } }

        /// <summary>
        /// Screen position of Head in Pixels
        /// </summary>
        public Point Position { get; set; }

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
        /// total length of tail at the moment
        /// </summary>
        public int TailLength
        {
            get
            {
                int length = 0;
                var lines = GetLines();
                foreach (var line in lines)
                    length = length + line.Length;

                return length;
            }
        }

        /// <summary>
        /// max allowed tail length; set to 0 for unlimited tail length
        /// </summary>
        public static int MaxTailLength { get { return Settings.Current.MaxTailLength; } }


        #endregion

        #region Constructor

        public Cycle(Game game, Screen screen, Grid grid, CycleInfo info, Player player)
            : base(game, screen)
        {
            m_vertices = new List<Point>();
            TrailColor = info.Color;
            Grid = grid;
            Position = grid.ToWorldCoords(info.GridPosition);
            Direction = info.Direction;
            m_player = player;
            //add start position
            m_vertices.Add(Position);
        }

        #endregion

        #region Events

        public event EventHandler Collided;

        public void InvokeCollided()
        {
            EventHandler handler = Collided;
            if (handler != null) handler(this, new EventArgs());
        }

        #endregion

        #region Public Methods

        public bool CheckWalls(Point position)
        {
            return position.X < 0 || position.Y < 0 || position.X > Game.GraphicsDevice.Viewport.Width || position.Y > Game.GraphicsDevice.Viewport.Height;
        }

        public bool CheckHeadLine(Line myline, out Player killer)
        {
            //hiding the class scope position Vector here caus this method can be used to check for future colisions.
            var Position = myline.End;
            bool hasCollision = false;

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
                            killer = cycle.m_player;
                            break;
                        }
                    }
                    lines.RemoveAt(lines.Count - 1);
                }
                else
                {
                    //this is us we can remove the first 2 lines
                    lines.RemoveAt(lines.Count - 1);
                    if(lines.Count > 0)lines.RemoveAt(lines.Count - 1);
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

            //this can happen if we have turned but not yet moved
            if(Position != m_vertices.Last())
                lines.Add(new Line(m_vertices.Last(), Position));

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


            m_nextGridCrossing = gridCrossing;
            m_scheduledDirection = direction;
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

			Game.SpriteBatch.Draw(Art.Bike, Position.ToVector(), null, BikeColor, Velocity.Orientation(), new Vector2(Art.Bike.Width / 2, Art.Bike.Height / 2), 1f, SpriteEffects.None, 0);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Position = new Point((int) (Position.X + Velocity.X), (int) (Position.Y + Velocity.Y));

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

        #endregion

        #region Private Methods

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
            if (m_nextGridCrossing == null)
                return;

            bool turn = false;
            switch (Direction)
            {
                case Direction.Down:
                    if (Position.Y >= m_nextGridCrossing.Value.Y)
                        turn = true;
                    break;
                case Direction.Up:
                    if (Position.Y <= m_nextGridCrossing.Value.Y)
                        turn = true;
                    break;
                case Direction.Right:
                    if (Position.X >= m_nextGridCrossing.Value.X)
                        turn = true;
                    break;
                case Direction.Left:
                    if (Position.X <= m_nextGridCrossing.Value.X)
                        turn = true;
                    break;
                default:
                    throw new Exception("Invalid direction");
            }
            if (turn)
            {
                Turn();
                m_nextGridCrossing = null;
            }
        }

        
        // Turn now.
        private void Turn()
        {
            if (m_nextGridCrossing.HasValue)
            {
                if ((int) Direction == - (int) m_scheduledDirection)
                {
                    if (AllowSuicide)
                    {
                        DebugMessages.Add("Player " + m_player.PlayerID + " Suicide");
                        InvokeCollided();
                    }
                    else
                    {
                        //cancel turn
                        m_scheduledDirection = Direction;
                        m_nextGridCrossing = null;
                    }
                    return;
                }
                else if (CycleJustTurned() || m_vertices.Last() == m_nextGridCrossing)
                {
                    m_nextGridCrossing = null;
                    return;
                }

                Direction = m_scheduledDirection;
                Position = m_nextGridCrossing.Value;
                m_vertices.Add(Position);
            }
        }

        /// <summary>
        /// Checks if we have collided with any other cycle
        /// </summary>
        private void CheckForCollision()
        {
            var myline = new Line(m_vertices.Last(), Position);

            Player killer = null;

            bool hasCollision = CheckWalls(Position);
            
            if(!hasCollision)
                hasCollision = CheckHeadLine(myline, out killer);

            if (hasCollision)
            {
                string killerString = killer!=null?"Player "+killer.PlayerID:"the wall";

                DebugMessages.Add("Player " + m_player.PlayerID + " Crashed into " + killerString);
                InvokeCollided();
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
                    if (firstLine.Length >= difference)
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