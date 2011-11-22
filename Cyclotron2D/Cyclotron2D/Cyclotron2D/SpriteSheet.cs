using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Cyclotron2D
{
	public class SpriteSheet
	{
		public Vector2 SpriteSize { get; private set; }
		public Texture2D Texture { get; private set; }
		public bool IsLooping { get; private set; }
		public int DrawsPerUpdate { get; private set; }
		public int FrameCount { get; private set; }
		public Vector2 Size { get; private set; }

		private int columnsCount;

		public SpriteSheet(Vector2 spriteSize, Texture2D spriteSheet, int frameCount = -1, Vector2? spriteSheetSize = null, bool loop = false, int drawsPerUpdate = 1)
		{
			SpriteSize = spriteSize;
			Texture = spriteSheet;
			IsLooping = loop;
			Size = spriteSheetSize ?? new Vector2(spriteSheet.Width, spriteSheet.Height);
			DrawsPerUpdate = drawsPerUpdate;

			if (frameCount == -1)
				this.FrameCount = (int)((Size.X / spriteSize.X) * (Size.Y / spriteSize.Y));
			else
				this.FrameCount = frameCount;

			columnsCount = (int)(Size.X / SpriteSize.X);
		}

		public void Draw(SpriteBatch spriteBatch, int updateCount, Vector2 position, float orientation = 0f, float scale = 1f, Color? color = null, Vector2? origin = null)
		{
			Draw(spriteBatch, updateCount, position, null, orientation, scale, color, origin);
		}

		public void Draw(SpriteBatch spriteBatch, int updateCount, Rectangle dest, float orientation = 0f, Color? color = null, Vector2? origin = null)
		{
			Draw(spriteBatch, updateCount, null, dest, orientation, 1f, color, origin);
		}

		public Rectangle GetSourceRect(int frame)
		{
			if (frame < 0)
				frame += FrameCount;

			int x = (int)(frame * SpriteSize.X) % (int)Size.X;
			int y = (int)((frame / columnsCount) * SpriteSize.Y);

			return new Rectangle(x, y, (int)SpriteSize.X, (int)SpriteSize.Y);
		}

		private void Draw(SpriteBatch spriteBatch, int updateCount, Vector2? position, Rectangle? dest, float orientation = 0f, float scale = 1f, Color? color = null, Vector2? origin = null)
		{
			// TODO: draw sprite, update frame based on gametime...
		}
	}
}
