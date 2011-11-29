﻿using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Cyclotron2D.Network
{


    internal class AsyncSocketState
    {
        public Socket Socket { get; set; }
        public byte[] Buffer { get; set; }
        public EndPoint RemoteEP { get; set; }

    }

    /// <summary>
    /// little helper class that checks if a socket is still connected. 
    /// It will currently falsly identify a socket where Listen has been called and a connection is pending as disconnected.
    /// this currently does not matter but if it one day does we might have to look into an alternative
    /// </summary>
    public static class SocketProbe
    {
        public static bool IsConnectedTcp(Socket socket)
        {
            if (socket != null && socket.Connected)
            {
                bool read = socket.Poll(1000, SelectMode.SelectRead);

                if (read && socket.Available == 0)
                {
                    return false;
                }

                return true;

            }
            return false;
        }


        public static bool IsConnectedUdp(Socket socket)
        {
//            if (socket != null)
//            {
//                bool read = socket.Poll(-1, SelectMode.SelectRead);
//                bool write = socket.Poll(-1, SelectMode.SelectWrite);
//                bool error = socket.Poll(-1, SelectMode.SelectWrite);
//
//                return ((read && socket.Available > 0) || write) && !error;
//
//            }
//            return false;
            return true;
        }
    }

    /// <summary>
    /// Wrapper for Socket that handles received messages on a seperate thread.
    /// </summary>
    public class NetworkConnection
    {

        #region Constants

        public const int MAX_BUFFER_SIZE = 512;

        #endregion

        #region Properties

        public Socket Socket { get; private set; }

        public bool IsConnected { get { return Mode == NetworkMode.Tcp ? SocketProbe.IsConnectedTcp(Socket) : SocketProbe.IsConnectedUdp(Socket); } }

        public EndPoint LocalEP { get; private set; }

        public EndPoint RemoteEP { get; private set; }

        public NetworkMode Mode { get; private set; }

        /// <summary>
        /// currently only the server has this information for everyone. clients only have it for the server
        /// </summary>
        public TimeSpan RoundTripTime { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// creates a new connection object using the provided socket
        /// </summary>
        /// <param name="socket">an already connected socket</param>
        /// <param name="mode"></param>
        public NetworkConnection(Socket socket)
        {
            Mode = NetworkMode.Tcp;
            Socket = socket;
            RoundTripTime = new TimeSpan(0);
            if (SocketProbe.IsConnectedTcp(socket))
            {

                LocalEP = Socket.LocalEndPoint;
                RemoteEP = Socket.RemoteEndPoint;
               // ClearReceiveBuffer();
                StartReceiving();
            }

        }

        public NetworkConnection()
        {
            Mode = NetworkMode.Tcp;
        }


        public NetworkConnection(EndPoint localEp, IPAddress ip, int port)
        {
            Mode = NetworkMode.Udp;
            LocalEP = localEp;
            RemoteEP = new IPEndPoint(ip, port);

        }

        #endregion

        #region Events

        /// <summary>
        /// This event is invoked on a seperate thread for each new message that comes in.
        /// The listening thread keeps going in the backround.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;

        private void InvokeMessageReceived(MessageEventArgs e)
        {
            Debug.Assert(Mode == NetworkMode.Tcp, "Individual Connections should not be receiving data under Udp");
            EventHandler<MessageEventArgs> handler = MessageReceived;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region Public Methods

        public void SwitchToUdp(Socket UdpSocket)
        {
            if(IsConnected)
            {
                Disconnect();

                Thread.Sleep(20);
            }



            Socket = UdpSocket;
            Mode = NetworkMode.Udp;


        }

//        public static void KillUdpListen()
//        {
//            if(UdpListenSocket != null)
//            {
//                UdpListenSocket.Close();
//                UdpListenSocket = null;
//            }
//        }




        /// <summary>
        /// Sends the message async
        /// </summary>
        /// <param name="message"></param>
        public void Send(NetworkMessage message)
        {
            DebugMessages.AddLogOnly("Sending Message: " + message.Type + "\n" + message.Content + "\n");
            try
            {
                switch (Mode)
                {
                    case NetworkMode.Tcp:
                        {
                            Socket.BeginSend(message.Data, 0, message.Data.Length, SocketFlags.None, (ar => Socket.EndSend(ar)), null);
                        }
                        break;
                    case NetworkMode.Udp:
                        {
                            Socket.BeginSendTo(message.Data, 0, message.Data.Length, SocketFlags.None, RemoteEP, (ar => Socket.EndSend(ar)), null);
                        }
                        break;
                }
            }
            catch (SocketException ex)
            {
                DebugMessages.Add("Socket exception on send: " + ex.Message);
                InvokeDisconnected(new ConnectionEventArgs(this));
            }
           
        }

        public event EventHandler<ConnectionEventArgs> Disconnected;

        private void InvokeDisconnected(ConnectionEventArgs e)
        {
            EventHandler<ConnectionEventArgs> handler = Disconnected;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Attempts to connect to the specified address. Creates a new Socket in the process
        /// </summary>
        public bool ConnectTo(IPAddress address)
        {

            bool connected = false;
            if (Socket != null && Socket.Connected)
            {
                throw new AlreadyConnectedException("Socket is Already Connected.");
            }

            try
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket.Connect(address, GameLobby.GAME_PORT);

                DebugMessages.Add("Client Connected");

                connected = true;


                LocalEP = Socket.LocalEndPoint;
                RemoteEP = Socket.RemoteEndPoint;

                StartReceiving();

            }
            catch (Exception ex)
            {
                DebugMessages.Add(ex.Message);
            }

            return connected;
        }


        public void Disconnect()
        {
            if (Socket != null)
            {
                Socket.Close();
                Socket = null;
            }
            
        }

        #endregion

        #region Private Methods

     
        private void StartReceiving()
        {

            byte[] buffer = new byte[MAX_BUFFER_SIZE];

            try
            {
                Socket.BeginReceive(buffer, 0, MAX_BUFFER_SIZE, SocketFlags.None, ReceiveCallback, buffer);
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
            catch (SocketException ex)
            {
                //forcibly disconnected from remote host?
                DebugMessages.Add(ex.Message);
                return;
            }
        }

       

        private void ReceiveCallback(IAsyncResult ar)
        {

            var buffer = ar.AsyncState as byte[];

            Debug.Assert(buffer != null, "Did the state object for the async receive callback change type?");

            NetworkMessage msg = null;

            try
            {
                Socket.EndReceive(ar);

                msg = NetworkMessage.Build(buffer);

                Debug.Assert(msg.Length < MAX_BUFFER_SIZE, "there should not be a message with more than " + MAX_BUFFER_SIZE + " bytes of content");

                while (msg.Length > msg.Content.Length)
                {
                    buffer = new byte[msg.Length - msg.Content.Length];
                    Socket.Receive(buffer);
                    msg.AddContent(buffer);
                }
                if(msg.Content.Length > msg.Length)
                {
                    Debug.Assert(false, "stealing data from next message much?");
                }

            }
            catch (ObjectDisposedException)
            {
                //disconnected, stop receive loop
                return;
            }
            catch(NullReferenceException)
            {
                //disconnected, stop receive loop
                return;
            }
            catch(SocketException ex)
            {
                //forcibly disconnected from remote host?
                DebugMessages.Add(ex.Message);
                return;
            }

            

            StartReceiving();

            InvokeMessageReceived(new MessageEventArgs(msg, this));
        }

        #endregion

    }


    public class AlreadyConnectedException : Exception
    {
        public AlreadyConnectedException(string message)
            : base(message)
        {

        }
    }
}