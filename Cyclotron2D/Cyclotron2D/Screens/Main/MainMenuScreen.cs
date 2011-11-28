using System;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.State;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;
using Cyclotron2D.Graphics;

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
					Game.ChangeState(GameState.GameLobbyHost);
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

        protected override void OnStateChanged(object sender, StateChangedEventArgs e)
        {
            base.OnStateChanged(sender, e);
            
            if (!IsValidState) return;

            Game.Communicator.ClearAll();


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
            m_mainMenu.Rect = new Rectangle(rect.Width/4, rect.Height/6 + 100, rect.Width/2, 2*rect.Height/3); // Hacked height by 100px to fit title art...
            m_mainMenu.Reset();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (m_mainMenu.Visible)
            {
                m_mainMenu.Draw(gameTime);
            }

			// Draw the title graphic
			var rect = Game.GraphicsDevice.Viewport.Bounds;
			Game.SpriteBatch.Draw(Art.Title, new Vector2(rect.Width / 2, 0), null, Color.White, 0.0f, new Vector2(Art.Title.Width / 2, 0), 0.4f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
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