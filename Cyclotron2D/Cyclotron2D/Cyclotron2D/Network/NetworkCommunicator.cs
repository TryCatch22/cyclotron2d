﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cyclotron2D.Components;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Helpers;
using Cyclotron2D.State;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Network
{

    public enum NetworkMode
    {
        Tcp, Udp
    }

    public class NetworkCommunicator : CyclotronComponent
    {
        #region Fields

        private TimeSpan lastConnectionCheck;

        #endregion

        private List<RemotePlayer> m_disconnected;


        #region Properties

        public Dictionary<RemotePlayer, NetworkConnection> Connections { get; private set; }

        private Socket UdpSocket { get; set; }

        public NetworkMode Mode { get; private set; }

        public RemotePlayer Host { get; private set; }

        public TimeSpan AverageRtt
        {
            get
            {
                TimeSpan sum = new TimeSpan(0);
                int nonZero = 0;
                foreach (NetworkConnection networkConnection in Connections.Values.Where(c => c.RoundTripTime != new TimeSpan(0)))
                {
                    sum += networkConnection.RoundTripTime;
                    nonZero++;
                }

                return nonZero == 0 ? new TimeSpan(0) : sum.Div(nonZero);
            }
        }

        public TimeSpan MaximumRtt
        {
            get
            {
                TimeSpan max = new TimeSpan(0);
                foreach (NetworkConnection networkConnection in Connections.Values.Where(c => c.RoundTripTime != new TimeSpan(0)))
                {
                    max = networkConnection.RoundTripTime > max ? networkConnection.RoundTripTime : max;
                }

                return max;
            }
        }

        /// <summary>
        /// Id for the local player set here so that it can be added to all outgoing messages automatically
        /// </summary>
        public int LocalId { get; set; }

        #endregion

        #region Constructor

        public NetworkCommunicator(Game game)
            : base(game)
        {
            Connections = new Dictionary<RemotePlayer, NetworkConnection>();
            LocalId = 0;
            m_disconnected = new List<RemotePlayer>();
            Mode = NetworkMode.Tcp;
        }

        #endregion

        #region Public Methods

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
                    connection.Disconnected += OnConnectionDisconnected;
                }
            }
        }

        public void Remove(RemotePlayer player)
        {
            lock (Connections)
            {
                if (Connections.ContainsKey(player))
                {
                    var connection = Connections[player];
                    connection.MessageReceived -= OnMessageReceived;
                    connection.Disconnected -= OnConnectionDisconnected;
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

            foreach (RemotePlayer remotePlayer in m_disconnected)
            {
                Remove(remotePlayer);
            }


            if (gameTime.TotalGameTime - lastConnectionCheck > new TimeSpan(0, 0, 0, 0, 500))
            {
                foreach (var connection in connections)
                {
                    if (!connection.IsConnected)
                    {
                        Remove(GetPlayer(connection));
                        InvokeConnectionLost(new ConnectionEventArgs(connection));
                    }

                }

               
                lastConnectionCheck = gameTime.TotalGameTime;
            }


        }

        public void SendDebugMessage(string message)
        {
            foreach (var networkConnection in Connections.Values)
            {
                networkConnection.Send(new NetworkMessage(MessageType.Debug, message) { Source = (byte)LocalId });
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
            lock (Connections)
            {
                foreach (RemotePlayer remotePlayer in Connections.Keys)
                {
                    Connections[remotePlayer].Send(message);
                }
            }

        }

        public RemotePlayer GetPlayer(Socket socket)
        {
            RemotePlayer player;

            lock (Connections)
            {
                player = (from kvp in Connections where kvp.Value.Socket == socket select kvp.Key).FirstOrDefault();
            }

            return player;
        }

        public NetworkConnection GetConnection(int playerId)
        {
            NetworkConnection connection;

            lock (Connections)
            {
                connection = (from kvp in Connections where kvp.Key.PlayerID == playerId select kvp.Value).FirstOrDefault();
            }

            return connection;
        }

        public RemotePlayer GetPlayer(int playerId)
        {

            RemotePlayer player;

            lock (Connections)
            {
                player = (from kvp in Connections where kvp.Key.PlayerID == playerId select kvp.Key).FirstOrDefault();
            }

            return player;
        }


        public RemotePlayer GetPlayer(NetworkConnection connection)
        {
            RemotePlayer player;

            lock (Connections)
            {
                player = (from kvp in Connections where kvp.Value == connection select kvp.Key).FirstOrDefault();
            }

            return player;
        }

        public void ClearAll()
        {
            List<RemotePlayer> players = Connections.Keys.ToList();

            Game.RttService.Reset();

            foreach (var key in players)
            {
                Remove(key);
            }
            if (UdpSocket != null)
            {
                UdpSocket.Close();
                UdpSocket = null;
            }

        }

        public void SwitchToUdp()
        {

            SetupUdpSocket();


            Mode = NetworkMode.Udp;

        }



        #endregion

        #region Event Handlers



        private void OnConnectionDisconnected(object sender, ConnectionEventArgs e)
        {
            m_disconnected.Add(GetPlayer(e.Connection));
            InvokeConnectionLost(e);
        }


        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            //            try
            //            {
            DebugMessages.AddLogOnly("Received Message: " + e.Message.Type + "\n" + e.Message.Content + "\n");
            EventHandler<MessageEventArgs> handler = MessageReceived;
            if (handler != null) handler(sender, e);
            //            }
            //            catch (Exception ex)
            //            {
            //                DebugMessages.AddLogOnly("Game Crashed On Message Handle: " + ex.Message + "\n" + ex.StackTrace);
            //                DebugMessages.FlushLog();
            //                throw;
            //            }

        }

        #endregion

        #region Private Methods

        private void SetupUdpSocket()
        {
            if (UdpSocket != null) return;

            UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            if (Connections.Count > 0)
            {

                var first = Connections.Values.ToArray()[0];

                UdpSocket.Bind(first.LocalEP);

                StartReceivingUdp();
            }


        }

        private void ReceiveCallbackUdp(IAsyncResult ar)
        {
            var buffer = ar.AsyncState as byte[];

            Debug.Assert(buffer != null, "Did the state object for the async receive callback change type?");

            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);

            NetworkMessage msg = null;

            try
            {
                UdpSocket.EndReceiveFrom(ar, ref  remote);

                msg = NetworkMessage.Build(buffer);

                while (msg.Length > msg.Content.Length)
                {
                    Array.Clear(buffer, 0, NetworkConnection.MAX_BUFFER_SIZE);
                    UdpSocket.ReceiveFrom(buffer, ref remote);
                    msg.AddContent(buffer);
                }
            }
            catch (ObjectDisposedException)
            {
                //disconnected, stop receive loop
                return;
            }
            catch (NullReferenceException)
            {
                //disconnected, stop receive loop
                return;
            }
            catch (SocketException e)
            {
                DebugMessages.Add("Udp Socket Exception: " + e.Message);
                return;
            }



            StartReceivingUdp();


            OnMessageReceived(GetConnection(msg.Source), new MessageEventArgs(msg));

        }

        private void StartReceivingUdp()
        {
            byte[] buffer = new byte[NetworkConnection.MAX_BUFFER_SIZE];
            EndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
            UdpSocket.BeginReceiveFrom(buffer, 0, NetworkConnection.MAX_BUFFER_SIZE, SocketFlags.None, ref endpoint, ReceiveCallbackUdp, buffer);
        }


        #endregion

        #region Events

        public event EventHandler<ConnectionEventArgs> ConnectionLost;

        private void InvokeConnectionLost(ConnectionEventArgs e)
        {
            EventHandler<ConnectionEventArgs> handler = ConnectionLost;
            if (handler != null) handler(this, e);
        }


        public event EventHandler<MessageEventArgs> MessageReceived;

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearAll();
            }
            base.Dispose(disposing);
        }

        #endregion

    }
}