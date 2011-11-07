using System;
using System.Collections.Generic;
using System.Linq;
using Cyclotron2D.Components;
using Cyclotron2D.Helpers;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Core
{
    public class Grid : DrawableScreenComponent
    {
        public const int PixelsPerInterval = 5;

        public Grid(Game game, Screen screen, Vector2 size)
            : base(game, screen)
        {
            Size = size;
        }

        public Vector2 Size { get; private set; }

        public Vector2 GridSize { get { return new Vector2((int) Math.Ceiling(Size.X/PixelsPerInterval), (int) Math.Ceiling(Size.Y/PixelsPerInterval)); } }

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
        public Vector2 ToWorldCoords(Vector2 gridCoords)
        {
            return ToWorldCoords(gridCoords, true);
        }

        /// <summary>
        /// Conversion from Grid coords to pixels
        /// </summary>
        /// <param name="gridCoords">Grid coordinates</param>
        /// <param name="round">If the value should be rounded to the nearest pixel</param>
        /// <returns>Pixel Coordinates</returns>
        public Vector2 ToWorldCoords(Vector2 gridCoords, bool round)
        {
            var v = gridCoords*PixelsPerInterval;
            return round ? v.Round() : v;
        }

        /// <summary>
        /// Conversion fro pixels to Grid coordinates. Does not round to a crossing.
        /// </summary>
        /// <param name="worldCoords">Pixel coordinates</param>
        /// <returns></returns>
        public Vector2 ToGridCoords(Vector2 worldCoords)
        {
            return new Vector2(worldCoords.X/PixelsPerInterval, worldCoords.Y/PixelsPerInterval);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            for (int i = 0; i < Size.X; i += PixelsPerInterval)
                Game.SpriteBatch.Draw(Art.Pixel, new Rectangle(i, 0, 1, (int) Size.Y), Color.SteelBlue);
            for (int i = 0; i < Size.X; i += PixelsPerInterval)
                Game.SpriteBatch.Draw(Art.Pixel, new Rectangle(0, i, (int) Size.X, 1), Color.SteelBlue);
        }
    }
}