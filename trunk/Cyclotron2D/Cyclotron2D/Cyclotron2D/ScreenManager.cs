using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cyclotron2D.Components;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D
{
    /// <summary>
    /// Maintains a Collection of screens.
    /// Is responsible for making sure the draw calls to the 
    /// various concurrent screens are properly ordered.
    /// 
    /// Also takes care of disabling and hiding screens as the game state changes.
    /// </summary>
    public class ScreenManager : DrawableCyclotronComponent
    {
        #region Fields

        private bool m_disposed;

        #endregion

        #region Properties

        /// <summary>
        /// The currently active Main Screen based on the Game State
        /// </summary>
        public MainScreen ActiveScreen { get; private set; }

        /// <summary>
        /// Screen with Focus (Topmost screen)
        /// </summary>
        public Screen FocusedScreen { get; set; }

        /// <summary>
        /// Screens mapping
        /// </summary>
        private Dictionary<MainScreen, List<PopupScreen>> Screens { get; set; }

        #endregion

        #region Constructor

        public ScreenManager(Game game) : base(game)
        {
            Screens = new Dictionary<MainScreen, List<PopupScreen>>();
            SubscribeGame();
        }

        #endregion

        #region Subscribtion

        private void SubscribeGame()
        {
            Game.StateChanged += OnGameStateChanged;
        }

        private void UnsubscribeGame()
        {
            Game.StateChanged -= OnGameStateChanged;
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing && !m_disposed)
            {
                UnsubscribeGame();

                foreach (var screen in Screens.Keys)
                {
                    screen.Dispose();

                    foreach (PopupScreen popupScreen in Screens[screen])
                    {
                        popupScreen.Dispose();
                    }
                }

                Screens = null;
                m_disposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Event Handlers

        private void OnGameStateChanged(object sender, StateChangedEventArgs e)
        {
            var mainScreens = Screens.Keys.Where(screen => screen.IsValidState).ToArray();

            Debug.Assert(mainScreens.Length == 1, "There should be Exactly 1 Main Screen associated to the current GameState.");

            ActiveScreen = mainScreens[0];

            ActiveScreen.Enabled = true;
            ActiveScreen.Visible = true;

            foreach (var screen in Screens.Keys.Where(screen => screen != mainScreens[0]))
            {
                screen.Enabled = false;
                screen.Visible = false;
            }
        }

        #endregion

        #region Public Methods

        public void AddScreen(Screen screen)
        {
            if (screen is MainScreen)
            {
                Screens.Add(screen as MainScreen, new List<PopupScreen>());
            }
            else
            {
                var popup = screen as PopupScreen;
                if (popup != null)
                {
                    Debug.Assert(Screens.ContainsKey(popup.Parent));

                    int n = Screens[popup.Parent].Count;
                    Screens[popup.Parent].Add(popup);

                    if (n > 0)
                    {
                        Screens[popup.Parent][n - 1].HasFocus = false;
                    }
                    else
                    {
                        popup.Parent.HasFocus = false;
                    }

                    popup.HasFocus = true;
                }
            }
        }

        /// <summary>
        /// removes and disposes a screen.
        /// if it is a main screen it also removes and disposes all associated popupscreens
        /// </summary>
        /// <param name="screen"></param>
        public void RemoveScreen(Screen screen)
        {
            if (screen is MainScreen)
            {
                var ms = screen as MainScreen;
                foreach (var popupScreen in Screens[ms])
                {
                    popupScreen.Dispose();
                }

                Screens.Remove(ms);
                ms.Dispose();
            }
            else
            {
                var popup = screen as PopupScreen;
                if (popup != null)
                {
                    Debug.Assert(Screens.ContainsKey(popup.Parent));
                    Screens[popup.Parent].Remove(popup);
                    popup.Dispose();
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            // make sure focus is properly allocated
            foreach (MainScreen screen in Screens.Keys)
            {
                screen.HasFocus = screen == ActiveScreen && Game.IsActive;

                bool popupFound = false;
                //tranverse popup list in reverse order
                for (int i = Screens[screen].Count - 1; i >= 0; i--)
                {
                    var popup = Screens[screen][i];
                    if (popup.Visible && screen.HasFocus && !popupFound)
                    {
                        popup.HasFocus = true;
                        popupFound = true;
                    }
                    else
                    {
                        popup.HasFocus = false;
                    }
                }
                //remove focus from main screen if there is a visible popup above it.
                screen.HasFocus &= !popupFound;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var mainScreen in Screens.Keys)
            {
                if (!mainScreen.Visible) continue;

                mainScreen.Draw(gameTime);

                foreach (PopupScreen popupScreen in Screens[mainScreen].Where(popupScreen => popupScreen.Visible))
                {
                    popupScreen.Draw(gameTime);
                }
            }
        }

        #endregion

    }
}