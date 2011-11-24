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
        private TimeSpan lastConnectionCheck;

        public Dictionary<RemotePlayer, NetworkConnection> Connections { get; private set; }

        public RemotePlayer Host { get; private set; }

        /// <summary>
        /// Id for the local player set here so that it can be added to all outgoing messages automatically
        /// </summary>
        public int LocalId { get; set; }

        public NetworkCommunicator(Game game)
            : base(game)
        {
            Connections = new Dictionary<RemotePlayer, NetworkConnection>();
            LocalId = 0;
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
                    connection.MessageReceived += OnMessageReceived;
                }
            }
        }

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            EventHandler<MessageEventArgs> handler = MessageReceived;
            if (handler != null) handler(sender, e);
        }

        public event EventHandler<ConnectionEventArgs> ConnectionLost;

        private void InvokeConnectionLost(ConnectionEventArgs e)
        {
            EventHandler<ConnectionEventArgs> handler = ConnectionLost;
            if (handler != null) handler(this, e);
        }


        public event EventHandler<MessageEventArgs> MessageReceived;


        public void Remove(RemotePlayer player)
        {
            lock (Connections)
            {
                if (Connections.ContainsKey(player))
                {
                    var connection = Connections[player];
                    connection.MessageReceived -= OnMessageReceived;
                    connection.Disconnect();
                    Connections.Remove(player);
                }
            }

        }



        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            List<NetworkConnection> connections;
            lock (Connections)
            {
                connections = Connections.Values.ToList();
            }

            if (gameTime.TotalGameTime - lastConnectionCheck > new TimeSpan(0, 0, 0, 0, 500))
            {
                foreach (var connection in connections)
                {
                    if (!connection.IsConnected)
                        InvokeConnectionLost(new ConnectionEventArgs(connection));
                }
                lastConnectionCheck = gameTime.TotalGameTime;
            }

           
        }

        public void SendDebugMessage(string message)
        {
            foreach (var networkConnection in Connections.Values)
            {
                networkConnection.Send(new NetworkMessage(MessageType.Debug, message){Source = (byte)LocalId});
            }
        }

        public void MessagePlayer(RemotePlayer player, NetworkMessage message)
        {
            MessagePlayer(player, message, (byte)LocalId);
        }

        /// <summary>
        /// Sends the message to the player, async
        /// </summary>
        /// <param name="player"></param>
        /// <param name="message"></param>
        /// <param name="source">overrides default local source to pretend to be another player</param>
        public void MessagePlayer(RemotePlayer player, NetworkMessage message, byte source)
        {
            if (Connections.ContainsKey(player))
            {
                message.Source = source;
                Connections[player].Send(message);
            }
        }

         public void MessageOtherPlayers(RemotePlayer player, NetworkMessage message)
         {
             MessageOtherPlayers(player, message, (byte)LocalId);
         }

        public void MessageOtherPlayers(RemotePlayer player, NetworkMessage message, byte source)
        {
            message.Source = source;
            foreach (RemotePlayer remotePlayer in Connections.Keys.Where(key => key != player))
            {
                Connections[remotePlayer].Send(message);
            }
        }

        public void MessageAll(NetworkMessage message)
        {
            MessageAll(message, (byte)LocalId);
        }

        public void MessageAll(NetworkMessage message, byte source)
        {
            message.Source = source;
            foreach (RemotePlayer remotePlayer in Connections.Keys)
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
                ClearAll();
            }
            base.Dispose(disposing);
        }


        public void ClearAll()
        {
            List<RemotePlayer> players = Connections.Keys.ToList();

            foreach (var key in players)
            {
                Remove(key);
            }
        }
    }
}
