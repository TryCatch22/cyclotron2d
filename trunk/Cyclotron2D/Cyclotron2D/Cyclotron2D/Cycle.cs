using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tron
{
	public enum Direction { Left, Right, Up, Down }
	class Cycle
	{
		public const float Speed = 2;

		public Vector2 Position { get; set; }
		public Vector2 Velocity { get; private set; }
		public Grid Grid { get; set; }
		public List<Vector2> Vertices = new List<Vector2>();

		public Vector2 GridPosition { get { return Grid.ToGridCoords(Position); } }
		public Direction Direction
		{
			get
			{
				if (Velocity == Vector2.UnitX * Speed)
					return Direction.Right;
				else if (Velocity == -Vector2.UnitX * Speed)
					return Direction.Left;
				else if (Velocity == Vector2.UnitY * Speed)
					return Direction.Down;
				else if (Velocity == -Vector2.UnitY * Speed)
					return Direction.Up;
				throw new Exception("Invalid velocity");
			}
		}

		public bool Paused { get; set; }

		Vector2? nextGridCrossing = null;
		Direction scheduledDirection;

		public Cycle()
		{
			Velocity = DirectionToVelocity(Direction.Right);
			Vertices.Add(Position);
		}

		public Vector2 DirectionToVelocity(Direction direction)
		{
			switch (direction)
			{
				case Direction.Down:
					return Vector2.UnitY * Speed;
				case Direction.Left:
					return -Vector2.UnitX * Speed;
				case Direction.Right:
					return Vector2.UnitX * Speed;
				case Direction.Up:
					return -Vector2.UnitY * Speed;
			}
			throw new Exception("Is there a fifth direction?");
		}

		public void ScheduleTurn(Direction direction)
		{
			// No scheduling of turns if there's already one scheduled, or if we're going in the right direction already,
			// or if it's a turn in the opposite direction of that we're already traveling.
			if (nextGridCrossing != null || Direction == direction || DirectionToVelocity(direction) + Velocity == Vector2.Zero)
				return;

			if (Grid.ToWorldCoords(GridPosition) == Position)
			{
				// Some funny things happen if the velocity gets updated before the turn gets scheduled.
				// To avoid that possibility, we're going to account for that case specifically.
				// Essentially, if the cycle is exactly at the intersection when the turn is scheduled,
				// the turn will happed here immediately, instead of going through proper scheduling channels.
				Turn(direction);
				return;
			}

			nextGridCrossing = Grid.ToWorldCoords(GridPosition);
			if (Velocity.X > 0 || Velocity.Y > 0)
				nextGridCrossing += Grid.ToWorldCoords(Vector2.Normalize(Velocity));
			scheduledDirection = direction;
		}

		private void Turn(Direction direction)
		{
			Velocity = DirectionToVelocity(direction);
			Position = Grid.ToWorldCoords(GridPosition);
			Vertices.Add(Position);
		}

		public void HandleInput(KeyboardState keyboard)
		{
			if (Paused)
				return;

			if (keyboard.IsKeyDown(Keys.Down))
				ScheduleTurn(Direction.Down);
			else if (keyboard.IsKeyDown(Keys.Left))
				ScheduleTurn(Direction.Left);
			else if (keyboard.IsKeyDown(Keys.Right))
				ScheduleTurn(Direction.Right);
			else if (keyboard.IsKeyDown(Keys.Up))
				ScheduleTurn(Direction.Up);
		}

		public void Update()
		{
			if (Paused)
				return;

			Position += Velocity;
			if (nextGridCrossing != null)
			{
				bool turn = false;
				switch (Direction)
				{
					case Direction.Down:
						if (Position.Y >= nextGridCrossing.Value.Y)
							turn = true;
						break;
					case Direction.Up:
						if (Position.Y <= nextGridCrossing.Value.Y)
							turn = true;
						break;
					case Direction.Right:
						if (Position.X >= nextGridCrossing.Value.X)
							turn = true;
						break;
					case Direction.Left:
						if (Position.X <= nextGridCrossing.Value.X)
							turn = true;
						break;
					default:
						throw new Exception("Invalid direction");
				}
				if (turn)
				{
					nextGridCrossing = null;
					Turn(scheduledDirection);
				}
			}

			CheckForCollision();
		}

		private void CheckForCollision()
		{
			var travelledLine = new Line(Position, Vertices.Last());
			for (int i = 0; i < Vertices.Count - 2; i++)
			{
				var line = new Line(Vertices[i], Vertices[i + 1]);
				if (Line.FindIntersection(line, travelledLine) != null)
				{
					throw new Exception("You die!");
				}
			}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Art.Circle, Position, null, Color.Blue, 0, new Vector2(Art.Circle.Width / 2, Art.Circle.Height / 2), 1f, SpriteEffects.None, 0);
			Vector2? lastVertex = null;
			foreach (var vertex in Vertices)
			{
				if (lastVertex != null)
					DrawLine(lastVertex.Value, vertex, spriteBatch);
				lastVertex = vertex;
			}
			DrawLine(Vertices.Last(), Position, spriteBatch);
		}

		private void DrawLine(Vector2 start, Vector2 end, SpriteBatch spriteBatch)
		{
			var smaller = start.LengthSquared() < end.LengthSquared() ? start : end;
			var isHorizontal = start.Y == end.Y;

			var width = isHorizontal ? (int)Math.Abs((start - end).X) : 1;
			var height = isHorizontal ? 1 : (int)Math.Abs((start - end).Y);
			var rect = new Rectangle((int)smaller.X, (int)smaller.Y, width, height);
			rect.Inflate(isHorizontal ? 0 : 1, isHorizontal ? 1 : 0);

			spriteBatch.Draw(Art.Pixel, rect, Color.Red);
		}
	}
}
