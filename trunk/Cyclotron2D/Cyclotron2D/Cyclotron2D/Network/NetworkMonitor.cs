using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Cyclotron2D.Components;
using Cyclotron2D.Core.Players;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Network
{
    class NetworkMonitor : CyclotronComponent
    {
       // public GameLobby Lobby { get; private set; }

        private Dictionary<RemotePlayer, NetworkClient> m_connections;

        private RemotePlayer GetPlayer(Socket socket)
        {
            return (from connection in m_connections where connection.Value.Socket == socket select connection.Key).FirstOrDefault();
        }


        public NetworkMonitor(Game game) : base(game)
        {
           // Lobby = new GameLobby();
            m_connections = new Dictionary<RemotePlayer, NetworkClient>();
        }


        /// <summary>
        /// Sends the message to the player, async
        /// </summary>
        /// <param name="player"></param>
        /// <param name="message"></param>
        public void MessagePlayer(RemotePlayer player, NetworkMessage message)
        {
            
        }
    }
}
