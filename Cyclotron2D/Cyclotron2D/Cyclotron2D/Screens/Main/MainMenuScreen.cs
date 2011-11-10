using System;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Screens.Main
{
    /// <summary>
    /// This is the Main menu Screen its the first screen you see  
    /// it offers A menu that transitions you to other MainScreens
    /// </summary>
    public class MainMenuScreen : MainScreen
    {
        private Menu m_mainMenu;

        #region Constructor

        public MainMenuScreen(Game game) : base(game, GameState.MainMenu)
        {
            m_mainMenu = new Menu(game, this);
            LoadMenuItems();
            SubscribeMenu();
        }

        #endregion

        #region Subscribtion

        private void SubscribeMenu()
        {
            m_mainMenu.SelectionChanged += OnMenuSelectionChanged;
        }

        private void UnsubscribeMenu()
        {
            m_mainMenu.SelectionChanged -= OnMenuSelectionChanged;
        }

        #endregion

        #region Event Handlers

        private void OnMenuSelectionChanged(object sender, EventArgs e)
        {
            switch (m_mainMenu.SelectedIndex)
            {
                case 0:
                    Game.ChangeState(GameState.PlayingSolo);
                    break;
				case 1:
					Game.ChangeState(GameState.Hosting);
					break;
                case 2:
                    Game.ChangeState(GameState.JoiningGame);
                    break;
                case 3:
                    Game.ChangeState(GameState.ChangingSettings);
                    break;
                case 4:
                    Game.Exit();
                    break;
                default:
                    break;
            }
        }

        protected override void OnEnabledChanged(object sender, EventArgs args)
        {
            base.OnEnabledChanged(sender, args);
            if (Enabled)
            {
                m_mainMenu.Reset();
            }
        }

        #endregion

        #region Public Methods

        public override void Initialize()
        {
            base.Initialize();
            var rect = Game.GraphicsDevice.Viewport.Bounds;
            m_mainMenu.Rect = new Rectangle(rect.Width/4, rect.Height/6, rect.Width/2, 2*rect.Height/3);
            m_mainMenu.Reset();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (m_mainMenu.Visible)
            {
                m_mainMenu.Draw(gameTime);
            }
        }

        #endregion

        #region Private Methods

        private void LoadMenuItems()
        {
            m_mainMenu.AddItems(new MenuItem(m_mainMenu, "Single Player")); //index 0
            m_mainMenu.AddItems(new MenuItem(m_mainMenu, "Host Game")); //index 1 etc...
            m_mainMenu.AddItems(new MenuItem(m_mainMenu, "Join Game"));
            m_mainMenu.AddItems(new MenuItem(m_mainMenu, "Settings"));
            m_mainMenu.AddItems(new MenuItem(m_mainMenu, "RageQuit"));
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing && m_mainMenu != null)
            {
                UnsubscribeMenu();
                m_mainMenu.Dispose();
                m_mainMenu = null;
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}