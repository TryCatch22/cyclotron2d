using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Cyclotron2D
{
    public static class Art
    {
        public static Texture2D Pixel, Circle;
        public static SpriteFont Font;

        private static Cyclotron s_game;

        public static void Initalize(Cyclotron game)
        {
            s_game = game;
        }

        public static void LoadContent(ContentManager content)
        {
            //artificial texture
            Pixel = new Texture2D(s_game.GraphicsDevice, 1, 1);
            Pixel.SetData(new[] {Color.White});

            Circle = content.Load<Texture2D>("Circle");

            Font = content.Load<SpriteFont>("Font");
        }


        public static void UnloadContent(ContentManager content)
        {
            content.Unload();
        }
    }
}