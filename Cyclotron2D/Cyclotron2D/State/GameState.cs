using System;

namespace Cyclotron2D.State
{
    /// <summary>
    /// States the Game can be in.
    /// Numbering ensures one can use shortcuts like 
    /// 
    /// GameState state = getState();
    /// if(state & (GameState.Hosting | GameState.PlayingAsClient) == state)
    ///     do something ...
    /// 
    /// </summary>
    [Flags]
    public enum GameState
    {
        MainMenu = 1,

        PlayingSolo = 2,

        GameLobbyHost = 4,

        JoiningGame = 8,

        GameLobbyClient = 16,

        PlayingAsClient = 32,

        PlayingAsHost = 64,

        ChangingSettings = 128,
    }

    public class StateChangedEventArgs : EventArgs
    {
        public StateChangedEventArgs(GameState oldState, GameState newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public GameState NewState { get; private set; }

        public GameState OldState { get; private set; }
    }
}