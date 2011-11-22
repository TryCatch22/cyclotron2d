using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Cyclotron2D.Components;
using Cyclotron2D.Core.Players;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Network
{
    public class NetworkCommunicator : CyclotronComponent
    {

        public Dictionary<RemotePlayer, NetworkConnection> Connections { get; private set; }

        public RemotePlayer Host { get; private set; }

        public NetworkCommunicator(Game game)
            : base(game)
        {
            Connections = new Dictionary<RemotePlayer, NetworkConnection>();
        }

        public void Add(RemotePlayer player, Socket socket)
        {
            Add(player, new NetworkConnection(socket));
        }

        public void AddHost(RemotePlayer hostPlayer, NetworkConnection host)
        {
            Host = hostPlayer;
            Add(hostPlayer, host);
        }

        public void Add(RemotePlayer player, NetworkConnection connection)
        {
            lock (Connections)
            {
                if (!Connections.ContainsKey(player))
                {
                    Connections.Add(player, connection);
                    connection.MessageReceived += MessageReceived;
                }
            }
        }

        public event EventHandler<MessageEventArgs> MessageReceived;

        public void Remove(RemotePlayer player)
        {
            lock (Connections)
            {
                if (Connections.ContainsKey(player))
                {
                    var connection = Connections[player];
                    connection.MessageReceived -= MessageReceived;
                    connection.Disconnect();
                    Connections.Remove(player);
                }
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

        public void MessageOtherPlayers(RemotePlayer player, NetworkMessage message)
        {
            foreach (RemotePlayer remotePlayer in Connections.Keys.Where(key => key != player))
            {
                Connections[remotePlayer].Send(message);
            }
        }

        public RemotePlayer GetPlayer(Socket socket)
        {
            return (from kvp in Connections where kvp.Value.Socket == socket select kvp.Key).FirstOrDefault();
        }

        public RemotePlayer GetPlayer(NetworkConnection connection)
        {
            return (from kvp in Connections where kvp.Value == connection select kvp.Key).FirstOrDefault();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                List<RemotePlayer> players = Connections.Keys.ToList();

                foreach (var key in players)
                {
                    Remove(key);
                }
            }
            base.Dispose(disposing);
        }

    }
}
