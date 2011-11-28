﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cyclotron2D.Components;
using Cyclotron2D.Graphics;
using Cyclotron2D.Helpers;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.Screens.Main;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Core
{
    public class Grid : DrawableScreenComponent
    {
        #region Fields

		private float m_hue = 0;

        private float m_changeRate;

        #endregion

        #region Properties

        public int PixelsPerInterval { get { return (Game.ScreenManager.GetMainScreen<GameScreen>() as GameScreen).GameSettings.GridSize.Value; } }

        /// <summary>
        /// in Pixels
        /// </summary>
        public Vector2 Size { get; private set; }

        public Color GridColor { get; set; }

        public Point GridSize { get { return new Point((int)Math.Ceiling(Size.X / PixelsPerInterval), (int)Math.Ceiling(Size.Y / PixelsPerInterval)); } }

        public List<Cycle> Cycles { get; private set; }

        #endregion

        #region Constructor

        public Grid(Game game, Screen screen, Vector2 size)
            : base(game, screen)
        {
            Size = size;
            Visible = (Screen as GameScreen).GameSettings.DrawGrid.Value;
        }

        #endregion

        #region Public Methods

        public void Initialize(IEnumerable<Cycle> cycles)
        {
            Cycles = cycles.ToList();

            GridColor = Color.SteelBlue;
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

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Visible = (Screen as GameScreen).GameSettings.DrawGrid.Value;

            if((Screen as GameScreen).GameSettings.PlasmaGrid.Value)
                UpdateColor(gameTime);
            
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            for (int i = 0; i < Size.X; i += PixelsPerInterval)
                Game.SpriteBatch.Draw(Art.Pixel, new Rectangle(i, 0, 1, (int) Size.Y), GridColor);
            for (int i = 0; i < Size.X; i += PixelsPerInterval)
                Game.SpriteBatch.Draw(Art.Pixel, new Rectangle(0, i, (int) Size.X, 1), GridColor);
        }

        #endregion

        #region Private Methods

        private void UpdateChangeRate(GameTime gameTime)
        {
			m_changeRate = 3f * ((float)new Random().NextDouble() + 0.1f);
        }

        private void UpdateColor(GameTime gameTime)
        {
			var oldHue = m_hue;
			m_hue += m_changeRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (m_hue == 0 || (oldHue < 2 && m_hue >= 2) || (oldHue < 4 && m_hue >= 4) || (oldHue < 6 && m_hue >= 6))
			{
				UpdateChangeRate(gameTime);
			}
			m_hue %= 6;
			GridColor = new Vector3(m_hue, 1f, 0.5f).HSVToColor();
        }

        #endregion
    }
}