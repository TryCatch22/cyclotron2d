#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Cyclotron2D;
using System.Threading;
#endregion

namespace GameStateManagement
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MainMenuScreen : MenuScreen
    {
        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
            : base("Main Menu")
        {
            // Create our menu entries.
            MenuEntry startGameMenuEntry = new MenuEntry("Start Game");
            MenuEntry joinGameMenuEntry = new MenuEntry("Join Game");
			MenuEntry startServerGameMenuEntry = new MenuEntry("Start Server (local)");
			MenuEntry joinServerGameMenuEntry = new MenuEntry("Join Server (local)");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            // Hook up menu event handlers.
            startGameMenuEntry.Selected += StartGameMenuEntrySelected;
            joinGameMenuEntry.Selected += JoinGameMenuEntrySelected;
			startServerGameMenuEntry.Selected += startServerMenuEntrySelected;
			joinServerGameMenuEntry.Selected += joinServerMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
			MenuEntries.Add(startGameMenuEntry);
			MenuEntries.Add(joinGameMenuEntry);
			MenuEntries.Add(startServerGameMenuEntry);
			MenuEntries.Add(joinServerGameMenuEntry);
            MenuEntries.Add(exitMenuEntry);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void StartGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
			var server = new Server(9876);
			server.WaitForConnection();
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
                               new GameplayScreen());
        }


        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        void JoinGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
			var client = new Client(9876);
			client.Connect();
        }

		void startServerMenuEntrySelected(object sender, PlayerIndexEventArgs e) {
			Lobby gameServer = new Lobby();
			gameServer.start();
		}

		void joinServerMenuEntrySelected(object sender, PlayerIndexEventArgs e) {
			NetworkClient client = new NetworkClient();
			client.Receive();
		}


        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            const string message = "Are you sure you want to exit this sample?";

            MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(message);

            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.Game.Exit();
        }

        #endregion
	}
}
