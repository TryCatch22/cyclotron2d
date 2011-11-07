using Microsoft.Xna.Framework;

namespace Cyclotron2D.Screens.Base
{
    /// <summary>
    /// A Main Screen is the base class for the principal Screens associated to one or more states
    /// There is always exactly 1 Visible Main screen 
    /// The main screen always draws on the entire Viewport
    /// There can Be Popup Screen Partially covering A MainScreen
    /// </summary>
    public abstract class MainScreen : Screen
    {
        protected MainScreen(Game game, int states) : base(game)
        {
            States = states;
        }

        /// <summary>
        /// The logical or of the States This Screen represents
        /// This Property need to be mutually exclusive among all MainScreens
        /// </summary>
        public int States { get; private set; }

        /// <summary>
        /// Indicates whether the current GameState is associated to this MainScreen
        /// </summary>
        public bool IsValidState { get { return ((int) Game.State & States) == (int) Game.State; } }
    }
}