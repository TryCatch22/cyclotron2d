using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Cyclotron2D
{
	class Grid
	{
		public const int PixelsPerInterval = 50;

		public Vector2 Size { get; private set; }
		public Vector2 GridSize { get { return new Vector2((int)Math.Ceiling(Size.X / PixelsPerInterval), (int)Math.Ceiling(Size.Y / PixelsPerInterval)); } }

		public Grid(Vector2 size)
		{
			Size = size;
		}

		public Vector2 ToWorldCoords(Vector2 gridCoords)
		{
			return gridCoords * PixelsPerInterval;
		}

		public Vector2 ToGridCoords(Vector2 worldCoords)
		{
			return new Vector2((int)(worldCoords.X / PixelsPerInterval), (int)(worldCoords.Y / PixelsPerInterval));
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			for (int i = 0; i < Size.X; i += PixelsPerInterval)
				spriteBatch.Draw(Art.Pixel, new Rectangle((int)i, 0, 1, (int)Size.Y), Color.SteelBlue);
			for (int i = 0; i < Size.X; i += PixelsPerInterval)
				spriteBatch.Draw(Art.Pixel, new Rectangle(0, (int)i, (int)Size.X, 1), Color.SteelBlue);
		}

	}
}
