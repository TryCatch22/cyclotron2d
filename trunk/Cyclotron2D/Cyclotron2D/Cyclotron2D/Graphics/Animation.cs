using System;
using Cyclotron2D.Components;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Graphics
{
    /// <summary>
    /// Special type of drawable Component that plays with its own visibility
    /// </summary>
	public class Animation : DrawableScreenComponent
    {

        #region Fields

        private TimeSpan m_lastUpdate;

        #endregion

        #region Properties

        public Texture2D SpriteSheet { get; private set; }

        public Vector2 Position { get; set; }

        public TimeSpan UpdateDelay { get; set; }

        public bool IsLooping { get; set; }

        private int Frame { get; set; }

        private int FrameCount { get { return FrameLayout.X*FrameLayout.Y; } }

        public Point FrameLayout { get; private set; }

		public float Scale { get; set; }

		public Color Color { get; set; }

        /// <summary>
        /// size of a single rectangle in the spriteSheet
        /// </summary>
        public Point Size { get; private set; }

        #endregion

        #region Constructor

        public Animation(Game game, Screen screen, Texture2D spriteSheet, Point frameLayout) : base(game, screen)
	    {
	        SpriteSheet = spriteSheet;
            Size = new Point(spriteSheet.Width / frameLayout.X, spriteSheet.Height / frameLayout.Y);
			Scale = 3f;
			Color = Color.White;
	        IsLooping = false;
            UpdateDelay = new TimeSpan(0, 0, 0, 0, 20);
	        FrameLayout = frameLayout;
	        m_lastUpdate = Game.GameTime.TotalGameTime;
            Frame = 0;
	    }

        #endregion

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (gameTime.TotalGameTime - m_lastUpdate > UpdateDelay)
            {
                Frame++;

                if(Frame > FrameCount)
                {
                    if (IsLooping)
                    {
                        Frame = Frame % FrameCount;
                    }
                    else
                    {
						Enabled = Visible = false;
                        return;
                    }
                }

                m_lastUpdate = gameTime.TotalGameTime;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
			var sourceRect = GetSourceRect(Frame);
            Game.SpriteBatch.Draw(SpriteSheet, Position, sourceRect, Color, 0, new Vector2(sourceRect.Width, sourceRect.Height) / 2, Scale, SpriteEffects.None, 0);
        }

		private Rectangle GetSourceRect(int frame)
		{
			int x = frame % FrameLayout.X;
			int y = (int)Math.Floor((float)frame / FrameLayout.X);

            return new Rectangle(x * Size.X, y * Size.Y, Size.X, Size.Y);
		}
	}
}
