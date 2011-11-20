using Cyclotron2D.Helpers;
using Cyclotron2D.Mod;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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
            m_settingsView.Rect = RectangleBuilder.Centered(GraphicsDevice.Viewport.Bounds, new Vector2(0.7f, 0.6f));
        }


        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (m_settingsView.Visible)
            {
                m_settingsView.Draw(gameTime);
            }
        }

        #endregion

        #region Private and Protected Methods

        protected override void HandleInupt(GameTime gameTime)
        {
            base.HandleInupt(gameTime);
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
