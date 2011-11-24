using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace Cyclotron2D.Network 
{
    /// <summary>
    /// Wrapper for Socket that handles received messages on a seperate thread.
    /// </summary>
	public class UdpNetworkConnection
    {

        #region Constants

        public const int MAX_BUFFER_SIZE = 1024;

        #endregion

        #region Properties

		public Socket Socket { get; private set; }

        #endregion

		#region Fields

		private EndPoint m_localEndpoint;

		private EndPoint m_remoteEndpoint;

		private byte[] m_buffer = new byte[MAX_BUFFER_SIZE];

		#endregion

		#region Constructor

		public UdpNetworkConnection()
		{
			Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			m_localEndpoint = new IPEndPoint(IPAddress.Any, GameLobby.GAME_PORT);
			Bind();
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Sends the message async
        /// </summary>
        /// <param name="message"></param>
		public void Send(NetworkMessage message)
		{
			Socket.BeginSendTo(m_buffer, 0, m_buffer.Length, SocketFlags.None, m_remoteEndpoint, new AsyncCallback(OnSend), Socket);
		}


        /// <summary>
        /// Attempts to connect.
        /// </summary>
        public bool Bind()
        {
			var connected = false;
            try
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				Socket.Bind(m_localEndpoint);

                Print("Client Connected");

                connected = true;

                StartReceiving();
            }
            catch (Exception ex)
            {
                Print(ex.Message);
            }

            return connected;
        }


        public void Disconnect()
        {
            if (Socket != null) Socket.Close();
        }

        #endregion

        #region Private Methods

        private void StartReceiving()
        {	
			Socket.BeginReceiveFrom(m_buffer, 0, m_buffer.Length, SocketFlags.None, ref m_remoteEndpoint, new AsyncCallback(OnReceive), Socket);
        }

		private void OnReceive(IAsyncResult result)
		{
			Socket remote = (Socket)result.AsyncState;
			int receivedBytes = remote.EndReceiveFrom(result, ref m_remoteEndpoint);
			string data = Encoding.ASCII.GetString(m_buffer, 0, receivedBytes);
			Print("UDP Received: " + data);
		}

		private void OnSend(IAsyncResult result)
		{
			Socket remote = (Socket)result.AsyncState;
			int receivedBytes = remote.EndSendTo(result);
			string data = Encoding.ASCII.GetString(m_buffer, 0, receivedBytes);
			Print("UDP Sent: " + data);
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

        #endregion
	}
}
