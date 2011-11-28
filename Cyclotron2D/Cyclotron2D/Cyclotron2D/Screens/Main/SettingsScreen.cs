using Cyclotron2D.Helpers;
using Cyclotron2D.Mod;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.State;
using Cyclotron2D.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Cyclotron2D.Graphics;

namespace Cyclotron2D.Screens.Main
{
    /// <summary>
    /// Main screen for settings editing
    /// </summary>
    public class SettingsScreen : MainScreen
    {

        #region Fields

        private SettingsChanger m_settingsView;

        #endregion

        #region Constructor

        public SettingsScreen(Game game) : base(game, GameState.ChangingSettings)
        {
            m_settingsView = new SettingsChanger(game, this);

        }

        #endregion

        #region Public Methods

        public override void Initialize()
        {
            base.Initialize();

            m_settingsView.Initialize();

            m_settingsView.SettingsChanged += OnSettingsChanged;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            m_settingsView.Rect = RectangleBuilder.Centered(GraphicsDevice.Viewport.Bounds, new Vector2(0.6f, 0.6f));
        }


        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (m_settingsView.Visible)
            {
                m_settingsView.Draw(gameTime);
            }

			// Draw settings title art
			Game.SpriteBatch.Draw(Art.Settings, new Vector2(GraphicsDevice.Viewport.Bounds.Width/2, -25), null, Color.White, 0.0f, new Vector2(Art.Settings.Width/2, 0), 1.0f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
        }

        #endregion

        #region Private and Protected Methods

        protected override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);
            if (Game.InputState.IsNewKeyPress(Keys.Escape))
            {
                Game.ChangeState(GameState.MainMenu);
            }
        }

        private void OnSettingsChanged(object sender, SettingsChangedEventArgs e)
        {
            if (e.IsValid)
            {
                Settings.SinglePlayer.WriteToFile();
                Game.ChangeState(GameState.MainMenu);
            }
        }



        protected override void OnStateChanged(object sender, StateChangedEventArgs e)
        {
            base.OnStateChanged(sender, e);
            if (IsValidState)
            {
                m_settingsView.LoadSettings(Settings.SinglePlayer);
            }
        }

        #endregion

    }
}
