﻿using Cyclotron2D.State;
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
        protected MainScreen(Game game, GameState states) : base(game)
        {
            States = states;
        }

        /// <summary>
        /// The logical or of the States This Screen represents
        /// This Property need to be mutually exclusive among all MainScreens
        /// </summary>
        public GameState States { get; private set; }

        /// <summary>
        /// Indicates whether the current GameState is associated to this MainScreen
        /// </summary>
        public bool IsValidState { get { return (Game.State & States) == Game.State; } }


        protected override void OnStateChanged(object sender, StateChangedEventArgs e)
        {
            base.OnStateChanged(sender, e);
            if(!IsValidState && (States & (e.OldState)) == e.OldState)
            {
                //we are leaving one of our valid states for a non valid state
                OnLeavingValidState();
            }
        }

        protected virtual void OnLeavingValidState()
        {
        }
    }
}