using System;

namespace Cyclotron2D
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

        WaitingForClients = 2,

        JoiningGame = 4,

        Hosting = 8,

        PlayingAsClient = 16,

        PlayingSolo = 32,
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