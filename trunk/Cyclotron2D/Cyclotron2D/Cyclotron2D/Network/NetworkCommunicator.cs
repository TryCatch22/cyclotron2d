using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cyclotron2D.Components;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Helpers;
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

        private bool m_ignoreDisconnects;



        #endregion

        private List<RemotePlayer> m_disconnected;

        private ConcurrentQueue<MessageEventArgs> m_receivedMessages;

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

            m_receivedMessages = new ConcurrentQueue<MessageEventArgs>();
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

        private void RemoveAllDisconnected()
        {
            lock (Connections)
            {
                foreach (RemotePlayer player in m_disconnected)
                {

                    if (Connections.ContainsKey(player))
                    {
                        var connection = Connections[player];
                        connection.MessageReceived -= OnMessageReceived;
                        connection.Disconnected -= OnConnectionDisconnected;
                        connection.DisconnectTcp();
                        Connections.Remove(player);

                        DebugMessages.Add("Removing " + player + " from Communicator.");
                    }
                }
            }

        }

        public void Remove(RemotePlayer player)
        {
            m_disconnected.Add(player);
        }



        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            List<NetworkConnection> connections;
            lock (Connections)
            {
                connections = Connections.Values.ToList();
            }

            RemoveAllDisconnected();

            m_disconnected.Clear();

			Dictionary<byte,MessageEventArgs> pingArgs = new Dictionary<byte, MessageEventArgs>();

            while (m_receivedMessages.Count > 0)
            {
                MessageEventArgs e;
                m_receivedMessages.TryDequeue(out e);
				if (e != null)
				{
                    //keep only most recent ping in message from each player
					if (e.Message.Type == MessageType.Ping && e.Message.Content == "in")
					{
					    if(!pingArgs.ContainsKey(e.Message.Source))
					    {
					        pingArgs.Add(e.Message.Source, e);
					    }
					    else if(pingArgs[e.Message.Source].Message.SequenceNumber < e.Message.SequenceNumber)
					    {
					        pingArgs[e.Message.Source] = e;
					    }
					}
					else // not a "ping in" msg
					{
					    InvokeMessageReceived(e);
					}
						
				}
            }
            //invoke all valid ping ins.
            foreach (var messageEventArgs in pingArgs.Values)
            {
                InvokeMessageReceived(messageEventArgs);
            }

            if (gameTime.TotalGameTime - lastConnectionCheck > new TimeSpan(0, 0, 0, 0, 500))
            {
                foreach (var connection in connections)
                {
                    if (!connection.IsConnected)
                    {
                        var player = GetPlayer(connection);
                        if (player != null && !m_ignoreDisconnects)
                        {

                            InvokeConnectionLost(new ConnectionEventArgs(connection));
                            Remove(player);
                        }

                    }

                }



                lastConnectionCheck = gameTime.TotalGameTime;
            }


        }

        public void SendDebugMessage(string message)
        {

            foreach (var kvp in Connections)
            {
                kvp.Value.Send(new NetworkMessage(MessageType.Debug, message) { Source = (byte)LocalId }, kvp.Key.ToString());
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
			if (player == null)
			{
				return;
			}

            if (Connections.ContainsKey(player))
            {
                message.Source = source;
               // Thread.Sleep(5);
                Connections[player].Send(message, player.ToString());
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
                //Thread.Sleep(5);
                Connections[remotePlayer].Send(message, remotePlayer.ToString());
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
                 //   Thread.Sleep(5);
                    Connections[remotePlayer].Send(message, remotePlayer.ToString());
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

            Game.ReliableUdpSender.ClearAll();

        }

        public void StopTcp()
        {
            DebugMessages.AddLogOnly("Stopping tcp");
            foreach (var networkConnection in Connections.Values)
            {
                networkConnection.DisconnectTcp();
            }
        }

        public void StartUdp()
        {
            DebugMessages.AddLogOnly("Starting Udp");
            SetupUdpSocket();

            foreach (var networkConnection in Connections.Values)
            {
                networkConnection.StartUdp(UdpSocket);
            }

            Game.ReliableUdpSender.Initialize(Connections.Keys.ToList());

            Mode = NetworkMode.Udp;
        }

        public void StartIgnoreDisconnect()
        {
            DebugMessages.AddLogOnly("Ignoring disconnects");
            m_ignoreDisconnects = true;
        }

        public void EndIgnoreDisconnect()
        {
            m_ignoreDisconnects = false;
            DebugMessages.AddLogOnly("Not ignoring disconnects");
        }



        #endregion

        #region Event Handlers



        private void OnConnectionDisconnected(object sender, ConnectionEventArgs e)
        {
            if (!m_ignoreDisconnects)
            {
                InvokeConnectionLost(e);
                m_disconnected.Add(GetPlayer(e.Connection));
            }

        }

        private void InvokeMessageReceived(MessageEventArgs e)
        {
            EventHandler<MessageEventArgs> handler = MessageReceived;
            if (handler != null) handler(e.Connection, e);
        }

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            if(e.Message.Type != MessageType.Ping)
            {
                DebugMessages.AddLogOnly("Received Message: " + e.Message.Type +", From: " + GetPlayer(e.Message.Source) +  ", SeqId: " + e.Message.SequenceNumber + "\n" + e.Message.Content + "\n");
            }
            m_receivedMessages.Enqueue(e);

            if (e.Message.Type == MessageType.Ping && e.Message.Content == "in")
            {
                e.Message.Content += "\n" + Game.GameTime.TotalGameTime;
            }
            //send confirmation right away.
            if (e.Message.RequiresConfirmation)
            {
                MessagePlayer(GetPlayer(e.Message.Source), new NetworkMessage(MessageType.MsgReceived, e.Message.SequenceNumber.ToString()));
            }
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

                var endPoint = first.LocalEP as IPEndPoint;

                UdpSocket.Bind(new IPEndPoint(endPoint.Address, NetworkConnection.UDP_GAME_PORT));

                StartReceivingUdp();
            }


        }


        private byte[] oldData;


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

                Debug.Assert(msg.Length < NetworkConnection.MAX_BUFFER_SIZE, "there should not be a message longer than " + NetworkConnection.MAX_BUFFER_SIZE + " bytes");

                while (msg.Length > msg.Content.Length)
                {
                    buffer = new byte[msg.Length - msg.Content.Length];
                    UdpSocket.ReceiveFrom(buffer, ref remote);
                    msg.AddContent(buffer);
                }

                if (msg.Content.Length > msg.Length)
                {
                    string content = msg.Content;
                    msg.Content = content.Substring(0, msg.Length);

                    string extraData = content.Substring(msg.Length);

                    oldData = NetworkMessage.MsgEncoding.GetBytes(extraData);
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
                DebugMessages.FlushLog();
                return;
            }



            StartReceivingUdp();


            if (msg != null)
            {
                OnMessageReceived(GetConnection(msg.Source), new MessageEventArgs(msg, GetConnection(msg.Source)));
            }

        }

        private void StartReceivingUdp()
        {
            byte[] buffer = new byte[NetworkConnection.MAX_BUFFER_SIZE];

            int k = 0;
            if (oldData != null)
            {
                k = oldData.Length;
                for (int i = 0; i < oldData.Length; i++)
                {
                    buffer[i] = oldData[i];
                }

                oldData = null;
            }

            EndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
            UdpSocket.BeginReceiveFrom(buffer, k, NetworkConnection.MAX_BUFFER_SIZE - k, SocketFlags.None, ref endpoint, ReceiveCallbackUdp, buffer);
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
