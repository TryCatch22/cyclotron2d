using System;
using System.Collections.Generic;
using System.Linq;
using Cyclotron2D.Components;
using Cyclotron2D.Graphics;
using Cyclotron2D.Helpers;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.Screens.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cyclotron2D.Core
{
    public class Grid : DrawableScreenComponent
    {
        #region Fields

		private float m_hue = 0;

        private float m_changeRate;

		private SpriteBatch m_spriteBatch;

		private RenderTarget2D m_renderTarget;
		
		private Effect m_effect;
		
		private VertexBuffer m_vertexBuffer;

		private VertexPositionTexture[] m_vertices = {	new VertexPositionTexture(new Vector3(-1, -1, 0), Vector2.UnitY),
														new VertexPositionTexture(new Vector3(-1, 1, 0), Vector2.Zero),
														new VertexPositionTexture(new Vector3(1, -1, 0), Vector2.One),
														new VertexPositionTexture(new Vector3(1, 1, 0), Vector2.UnitX) };

		Dictionary<Cycle, Queue<Vector2>> cycleVelocities = new Dictionary<Cycle, Queue<Vector2>>();

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
			foreach (var cycle in Cycles)
			{
				cycleVelocities[cycle] = new Queue<Vector2>();
			}
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
			return new Vector2(worldCoords.X / (float)PixelsPerInterval, worldCoords.Y / (float)PixelsPerInterval);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Visible = (Screen as GameScreen).GameSettings.DrawGrid.Value;

            if((Screen as GameScreen).GameSettings.PlasmaGrid.Value)
                UpdateColor(gameTime);

			if(Cycles != null)
			{
			    UpdateCycleData(gameTime);
			}


        }

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			//we draw the grid to it's own render target.
			Game.GraphicsDevice.SetRenderTarget(m_renderTarget);
			Game.GraphicsDevice.Clear(Color.Black);

			m_spriteBatch.Begin();

			for (int i = 0; i < Size.X; i += PixelsPerInterval)
				m_spriteBatch.Draw(Art.Pixel, new Rectangle(i, 0, 1, (int)Size.Y), GridColor);
			for (int i = 0; i < Size.X; i += PixelsPerInterval)
				m_spriteBatch.Draw(Art.Pixel, new Rectangle(0, i, (int)Size.X, 1), GridColor);

			m_spriteBatch.End();

			// (Put the render target back to the back buffer.)
			Game.GraphicsDevice.SetRenderTarget(null);

		    UpdateEffectData();

			// apply the warpy effect.
			foreach (EffectPass pass in m_effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
			}
		}

		protected override void LoadContent()
		{
			m_spriteBatch = new SpriteBatch(Game.GraphicsDevice);
			m_renderTarget = new RenderTarget2D(Game.GraphicsDevice, Game.GraphicsDevice.PresentationParameters.BackBufferWidth, Game.GraphicsDevice.PresentationParameters.BackBufferHeight);

			m_effect = Game.Content.Load<Effect>("Effect1");
			m_effect.CurrentTechnique = m_effect.Techniques["Technique1"];

			m_vertexBuffer = new VertexBuffer(Game.GraphicsDevice, VertexPositionTexture.VertexDeclaration, m_vertices.Length, BufferUsage.None);
			m_vertexBuffer.SetData<VertexPositionTexture>(m_vertices);
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

        private void UpdateEffectData()
        {
            // first, set up the data needed for the warpy effect
            var positions = new Vector2[Cycles.Count];
            var velocities = new Vector2[Cycles.Count];
            for (int i = 0; i < Cycles.Count && i < 6; i++)
            {
                var cycle = Cycles[i];

                var square = new Vector2(m_renderTarget.Bounds.Size().Y);

                // Get average velocity
                velocities[i] = cycleVelocities[cycle].Aggregate(Vector2.Zero, (x, y) => x + y, x => x / (cycleVelocities[cycle].Count * square));
                positions[i] = cycle.Position.ToVector() / m_renderTarget.Bounds.Size();
            }

            GraphicsDevice.SetVertexBuffer(m_vertexBuffer);
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            // second, we pass the data to the warpy effect.
            m_effect.Parameters["inputTex"].SetValue(m_renderTarget);
            m_effect.Parameters["numPlayers"].SetValue(Cycles.Count);
            m_effect.Parameters["cyclePos"].SetValue(positions);
            m_effect.Parameters["cycleVel"].SetValue(velocities);
        }

		private void UpdateCycleData(GameTime gameTime)
		{
			foreach (var cycle in Cycles)
			{
				// Because we define velocity solely in terms of direction (and not whether the cycle is
				// actually moving), we need to check whether the game has started in order to determine 
				// if the cycle is moving.
				var cycleVelocity = (gameTime.TotalGameTime < cycle.GameStart || !cycle.Enabled) ? Vector2.Zero : cycle.Velocity;

				var velocities = cycleVelocities[cycle];
				velocities.Enqueue(cycleVelocity);
				if (velocities.Count > 15)
					velocities.Dequeue();
			}
		}

        #endregion
    }
}