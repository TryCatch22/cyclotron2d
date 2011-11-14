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
        #region Fields

        private TimeSpan m_lastColorChange;

        private TimeSpan m_changeRate;

        private bool rateIncreasing, redUp, blueUp, greenUp;

        int maxColorByte = 200;

        #endregion

        #region Properties

        public int PixelsPerInterval { get { return (Screen as GameScreen).GameSettings.GridSize.Value; } }

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

            GridColor = Color.Maroon;
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
            TimeSpan min = new TimeSpan(0, 0, 0, 0, 10), max = new TimeSpan(0, 0, 0, 0, 50);


            if (rateIncreasing && m_changeRate < max)
            {
                m_changeRate += new TimeSpan(0, 0, 0, 0, 3);
            }
            else if (!rateIncreasing && m_changeRate > min)
            {
                m_changeRate -= new TimeSpan(0, 0, 0, 0, 4);
            }
            else
            {
                rateIncreasing = !rateIncreasing;
            }
        }

        private void UpdateColor(GameTime gameTime)
        {
      
            int inc = 1, xr = 3, xg = 5, xb = 2;
            if (m_lastColorChange == TimeSpan.Zero)
            {
                m_lastColorChange = gameTime.TotalGameTime;
                Random b = new Random((int)DateTime.Now.Ticks);
                GridColor = new Color(b.Next(0, maxColorByte), b.Next(0, maxColorByte), b.Next(0, maxColorByte), 180);
                m_changeRate = new TimeSpan(0, 0, 0, 0, 10);
            }

     

            if (gameTime.TotalGameTime - m_lastColorChange > m_changeRate)
            {
               // int ms = (int)gameTime.TotalGameTime.TotalMilliseconds;

                redUp = (redUp && GridColor.R > maxColorByte - inc * xr) || (!redUp && GridColor.R < inc * xr) ? !redUp : redUp;
                greenUp = (greenUp && GridColor.G > maxColorByte - inc * xg) || (!greenUp && GridColor.G < inc * xg) ? !greenUp : greenUp;
                blueUp = (blueUp && GridColor.B > maxColorByte - inc * xb) || (!blueUp && GridColor.B < inc * xb) ? !blueUp : blueUp;

                int r = redUp ? GridColor.R + (inc * xr) : GridColor.R - (inc * xr),
                    g = greenUp ? GridColor.G + (inc * xg) : GridColor.G - (inc * xg),
                    b = blueUp ? GridColor.B + (inc * xb) : GridColor.B - (inc * xb);

                GridColor = new Color(r, g, b, 180);
                UpdateChangeRate(gameTime);
                m_lastColorChange = gameTime.TotalGameTime;
            }

           
        }

        #endregion
    }
}