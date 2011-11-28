using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Cyclotron2D.Graphics
{
    public static class Art
    {
		private static ContentManager content;

        public static Texture2D Pixel, Circle, Bike, Title, Settings;
		public static Texture2D ExplosionSheet;
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
			Title = content.Load<Texture2D>("title");
			Settings = content.Load<Texture2D>("settings");

			ExplosionSheet = content.Load<Texture2D>("GrayExplosion");

			Font = content.Load<SpriteFont>("Font");
        }
		

        public static void UnloadContent(ContentManager content)
        {
            content.Unload();
        }
    }
}