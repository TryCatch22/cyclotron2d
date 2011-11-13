using System;
using System.Collections.Generic;
using System.Linq;
using Cyclotron2D.Components;
using Cyclotron2D.Helpers;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.Screens.Main;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Core
{
    public class Grid : DrawableScreenComponent
    {
        public int PixelsPerInterval { get { return (Screen as GameScreen).GameSettings.GridSize.Value; } }

        public Grid(Game game, Screen screen, Vector2 size)
            : base(game, screen)
        {
            Size = size;
        }

        /// <summary>
        /// in Pixels
        /// </summary>
        public Vector2 Size { get; private set; }

        public Point GridSize { get { return new Point((int) Math.Ceiling(Size.X/PixelsPerInterval), (int) Math.Ceiling(Size.Y/PixelsPerInterval)); } }

        public List<Cycle> Cycles { get; private set; }


        public void Initialize(IEnumerable<Cycle> cycles)
        {
            Cycles = cycles.ToList();
        }

        /// <summary>
        /// Conversion from Grid coords to pixels, this value is rounded to the nearest pixel.
        /// </summary>
        /// <param name="gridCoords">Grid coordinates</param>
        /// <returns>Rounded Pixel Coordinates</returns>
        public Point ToWorldCoords(Vector2 gridCoords)
        {
            var v = gridCoords * PixelsPerInterval;
            return v.RoundToPoint();
        }

        /// <summary>
        /// Conversion fro pixels to Grid coordinates. Does not round to a crossing.
        /// </summary>
        /// <param name="worldCoords">Pixel coordinates</param>
        /// <returns></returns>
        public Vector2 ToGridCoords(Point worldCoords)
        {
            return new Vector2(worldCoords.X/(float)PixelsPerInterval, worldCoords.Y/(float)PixelsPerInterval);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!(Screen as GameScreen).GameSettings.DrawGrid.Value)
                return;
            
            base.Draw(gameTime);



            for (int i = 0; i < Size.X; i += PixelsPerInterval)
                Game.SpriteBatch.Draw(Art.Pixel, new Rectangle(i, 0, 1, (int) Size.Y), Color.SteelBlue);
            for (int i = 0; i < Size.X; i += PixelsPerInterval)
                Game.SpriteBatch.Draw(Art.Pixel, new Rectangle(0, i, (int) Size.X, 1), Color.SteelBlue);
        }
    }
}