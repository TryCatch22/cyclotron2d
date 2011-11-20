using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Cyclotron2D.Components;
using Cyclotron2D.Core.Players;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Network
{
    public class NetworkCommunicator : CyclotronComponent
    {

        public Dictionary<RemotePlayer, NetworkConnection> Connections { get; private set; }


        public NetworkCommunicator(Game game)
            : base(game)
        {
            Connections = new Dictionary<RemotePlayer, NetworkConnection>();
        }

        public void Add(RemotePlayer player, Socket socket)
        {
            lock (Connections)
            {
                if (!Connections.ContainsKey(player))
                {
                    Connections.Add(player, new NetworkConnection(socket));
                }
            }
           
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var removed = (from kvp in Connections where !kvp.Value.IsConnected select kvp.Key).ToList();

            foreach (var remotePlayer in removed)
            {
                Connections[remotePlayer].Disconnect();
                Connections.Remove(remotePlayer);
            }
        }


        public void SendDebugMessage(string message)
        {
            foreach (var networkConnection in Connections.Values)
            {
                networkConnection.Send(new NetworkMessage(MessageType.Debug, message));
            }
        }

        /// <summary>
        /// Sends the message to the player, async
        /// </summary>
        /// <param name="player"></param>
        /// <param name="message"></param>
        public void MessagePlayer(RemotePlayer player, NetworkMessage message)
        {
            if (Connections.ContainsKey(player))
            {
                Connections[player].Send(message);
            }
        }
    }
}
