using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Cyclotron2D
{
	static class Art
	{
		public static Texture2D Pixel, Circle;
		public static SpriteFont Font;

		public static void LoadContent(ContentManager content)
		{
			Circle = content.Load<Texture2D>("Circle");
			Pixel = content.Load<Texture2D>("Pixel");
			Font = content.Load<SpriteFont>("Font");
		}
	}
}
