using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Cyclotron2D.Network 
{


    public static class SocketProbe
    {
        public static bool IsConnected(Socket socket)
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
    }

    /// <summary>
    /// Wrapper for Socket that handles received messages on a seperate thread.
    /// </summary>
	public class NetworkConnection
    {

        #region Constants

        public const int MAX_BUFFER_SIZE = 1024;

        #endregion

        #region Properties

        public Socket Socket { get; private set; }

        public bool IsConnected { get { return SocketProbe.IsConnected(Socket); } }

        #endregion

        #region Fields

        private bool m_stayAlive;

        private Thread m_receivingThread;

        #endregion

        #region Constructor

        public NetworkConnection(Socket socket)
        {
            Socket = socket;
        }

        public NetworkConnection()
        {
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
            EventHandler<MessageEventArgs> handler = MessageReceived;
            if (handler != null) handler(this, e);
        }

//        public event EventHandler Disconnected;
//
//        private void InvokeDisconnected()
//        {
//            EventHandler handler = Disconnected;
//            if (handler != null) handler(this, new EventArgs());
//        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sends the message async
        /// </summary>
        /// <param name="message"></param>
        public void Send(NetworkMessage message)
        {
            new Thread(() =>
            {
                //so that second messages on the socket have to wait
                lock (this)
                {
                    Socket.Send(message.Data);
                }
            }).Start();

            
        }


        /// <summary>
        /// Attempts to connect to the specified address. Creates a new Socket in the process
        /// </summary>
        public void ConnectTo(IPAddress address)
        {
            if (Socket != null && Socket.Connected)
            {
                
                throw new AlreadyConnectedException("Socket is Already Connected.");
            }

            try
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket.Connect(address, GameLobby.GAME_PORT);

                Print("Client Connected");

                StartReceiving();
            }
            catch (Exception ex)
            {
                Print(ex.Message);
            }
        }


        public void Disconnect()
        {
            if (m_receivingThread == null) return;

            m_stayAlive = false;
            m_receivingThread.Abort();
            m_receivingThread = null;
            if (Socket != null) Socket.Close();

//            InvokeDisconnected();
        }

        #endregion

        #region Private Methods

        #region Receive Thread

        /// <summary>
        /// When connected to a game lobby, listens to messages sent by the lobby.
        /// Runs until the thread dies.
        /// </summary>
        private void Receive()
        {

            byte[] buffer = new byte[MAX_BUFFER_SIZE];
            while (true)
            {
                //Wait for messages
                while (Socket.Available == 0 && m_stayAlive) Thread.Yield();

                if (!m_stayAlive) break;//exit and let thread die

                Socket.Receive(buffer);

                NetworkMessage message = NetworkMessage.Build(buffer);

                while (message.Length > message.Content.Length)
                {
                    Array.Clear(buffer, 0, MAX_BUFFER_SIZE);
                    Socket.Receive(buffer);
                    message.AddContent(buffer);
                }

                if (message.Type == MessageType.Debug)
                {
                    DebugMessages.Add(message.Content);
                }

                else
                {
                    new Thread(() => InvokeMessageReceived(new MessageEventArgs(message))).Start();
                }

                Array.Clear(buffer, 0, MAX_BUFFER_SIZE);
            }
        }

        #endregion


        private void StartReceiving()
        {
            m_receivingThread = new Thread(Receive) { IsBackground = true };
            m_stayAlive = true;

            ClearReceiveBuffer();
            m_receivingThread.Start();
        }

        /// <summary>
        /// Temporary Debug Print Method
        /// </summary>
        /// <param name="msg"></param>
        private static void Print(String msg)
        {
            Console.WriteLine(msg);
            DebugMessages.Add(msg);
        }

        /// <summary>
        /// Clears the receive buffer of the socket.
        /// </summary>
        private void ClearReceiveBuffer()
        {
            if (Socket.Available > 0)
            {
                Byte[] throwaway = new Byte[Socket.Available];
                Socket.Receive(throwaway);
            }
        }

        #endregion

	}


    public class AlreadyConnectedException : Exception
    {
        public AlreadyConnectedException(string message):base(message)
        {
            
        }

        public AlreadyConnectedException()
        {
            
        }
    }
}
