using System;
using System.Collections.Generic;
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
        Left = 1,
        Right = -1,
        Up = 2,
        Down = -2
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
        #region Constants

        // Velocity is magnitude and direction.
        // Speed is magnitude, Direction is direction.
        public const float Speed = 2;

        #endregion

        #region Fields

        // We only turn on grid lines, so if the input is received early, we have to keep track of it.
        private Vector2? m_nextGridCrossing;
        private Player m_player;
        private Direction m_scheduledDirection;

        /// <summary>
        /// The list of positions on the map at which we've made turns.
        /// This is used to draw our trail.
        /// </summary>
        private List<Vector2> m_vertices;

        private bool m_wasColliding;

        #endregion

        #region Properties

        /// <summary>
        /// Screen position of Head in Pixels
        /// </summary>
        public Vector2 Position { get; set; }

        public Color TrailColor { get; set; }

        public Color CircleColor { get { return new Color(255 - TrailColor.R, 255 - TrailColor.G, 255 - TrailColor.B, 190); } }

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

        #endregion

        #region Constructor

        public Cycle(Game game, Screen screen, Grid grid, CycleInfo info, Player player)
            : base(game, screen)
        {
            m_vertices = new List<Vector2>();
            TrailColor = info.Color;
            Grid = grid;
            Position = grid.ToWorldCoords(info.GridPosition);
            Direction = info.Direction;
            m_player = player;

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

        public List<Line> GetLines()
        {
            List<Line> lines = new List<Line>();


            for (int i = 0; i < m_vertices.Count - 2; i++)
            {
                lines.Add(new Line(m_vertices[i], m_vertices[i + 1]));
            }

            lines.Add(new Line(Position, m_vertices.Last()));

            return lines;
        }

        /// <summary>
        /// computes the next Grid crossing in current direction
        /// </summary>
        /// <returns>next Grid crossing in current direction in pixels</returns>
        public Vector2 GetNextGridCrossing()
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
        public void TurnAt(Direction direction, Vector2 gridCrossing)
        {
            if (direction == Direction)
                return;

            var v = Grid.ToGridCoords(gridCrossing).Round(); //making sure the value stored in m_next... is exactly a grid line.

            m_nextGridCrossing = Grid.ToWorldCoords(v);
            m_scheduledDirection = direction;
        }


        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Vector2? lastVertex = null;
            foreach (var vertex in m_vertices)
            {
                if (lastVertex != null)
                    DrawLine(lastVertex.Value, vertex);
                lastVertex = vertex;
            }

            DrawLine(m_vertices.Last(), Position);

            Game.SpriteBatch.Draw(Art.Circle, Position, null, CircleColor, 0, new Vector2(Art.Circle.Width/2, Art.Circle.Height/2), 1f, SpriteEffects.None, 0);
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Position += Velocity;

            CheckScheduledTurn();
            CheckForCollision();
        }

        #endregion

        #region Private Methods

        private static Vector2 DirectionToVelocity(Direction direction)
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
                    DebugMessages.Add("Player " + m_player.PlayerID + " Suicide");
                    InvokeCollided();
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
            var myline = new Line(Position, m_vertices.Last());
            bool hasCollision = false;

            if (Position.X < 0 || Position.Y < 0 || Position.X > Game.GraphicsDevice.Viewport.Width || Position.Y > Game.GraphicsDevice.Viewport.Height)
            {
                hasCollision = true;
            }

            foreach (var cycle in Grid.Cycles)
            {
                if (hasCollision)
                    break;
                

                var lines = cycle.GetLines();

                if (cycle != this)
                {
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
                            break;
                        }
                    }
                }
                //remove first line either weve checked or its our own
                lines.RemoveAt(lines.Count - 1);

                if (lines.Aggregate(false, (current, line) => current || Line.FindIntersection(line, myline) != IntersectionType.None))
                {
                    hasCollision = true;
                    break;
                }
            }

            if (hasCollision)
            {
                DebugMessages.Add("Player " + m_player.PlayerID + " dies!");
                InvokeCollided();
            }
        }

        private void DrawLine(Vector2 start, Vector2 end)
        {
            var smaller = start.LengthSquared() < end.LengthSquared() ? start : end;
            var isHorizontal = start.Y == end.Y;

            var width = isHorizontal ? (int) Math.Abs((start - end).X) : 1;
            var height = isHorizontal ? 1 : (int) Math.Abs((start - end).Y);
            var rect = new Rectangle((int) smaller.X, (int) smaller.Y, width, height);
            rect.Inflate(isHorizontal ? 0 : 1, isHorizontal ? 1 : 0);

            Game.SpriteBatch.Draw(Art.Pixel, rect, TrailColor);
        }

        #endregion
    }
}