using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Cyclotron2D
{
    public static class Art
    {
		private static ContentManager content;

        public static Texture2D Pixel, Circle, Bike;
		public static Animation Explosion;
        public static SpriteFont Font;

        private static Cyclotron s_game;

        public static void Initalize(Cyclotron game)
        {
            s_game = game;
        }

        public static void LoadContent(ContentManager content)
        {
			Art.content = content;

            //artificial texture
            Pixel = new Texture2D(s_game.GraphicsDevice, 1, 1);
            Pixel.SetData(new[] {Color.White});

            Circle = content.Load<Texture2D>("Circle");
			Bike = content.Load<Texture2D>("Bike");

			Explosion = LoadSpriteSheet("BlueExplosion", new Vector2(120), 12);

			Font = content.Load<SpriteFont>("Font");
        }

		private static Animation LoadSpriteSheet(string name, Vector2 spriteSize, int frameCount = -1, Vector2? sheetSize = null, bool loop = false, int drawsPerUpdate = 1)
		{
			string path = name;
			Texture2D spriteSheet = content.Load<Texture2D>(path);
			return new Animation(spriteSize, spriteSheet, frameCount, sheetSize, loop, drawsPerUpdate);
		}
		

        public static void UnloadContent(ContentManager content)
        {
            content.Unload();
        }
    }
}