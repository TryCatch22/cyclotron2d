using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cyclotron2D.Core.Players;

namespace Cyclotron2D.Network
{
    /// <summary>
    /// The player monitor will handle the turn events from the player object and pass them along through the network.
    /// </summary>
    internal class PlayerMonitor
    {
        private Player Player { get; set; }

        private GameLobby Lobby { get; set; }

        public PlayerMonitor(Player player, GameLobby lobby)
        {
            Player = player;
            Lobby = lobby;
            Player.DirectionChange += OnPlayerDirectionChanged;
        }

        private void OnPlayerDirectionChanged(object sender, DirectionChangeEventArgs e)
        {
            //send direction change data to server and/or other players 
            //don't forget to do it async
        }
    }
}
