using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Cyclotron2D.Network 
{

    /// <summary>
    /// Wrapper for Socket that handles received messages on a seperate thread
    /// </summary>
	public class NetworkClient {

		public const int MAX_BUFFER_SIZE = 1024;
        public Socket Socket { get; private set; }
		private NetworkClientThread ReceiveThread;

		/// <summary>
		/// Creates a network client object representing a player trying to connect to a game lobby.
		/// </summary>
		public NetworkClient() {
			//Does nothing for now
		}

		/// <summary>
		/// Temporary Debug Print Method
		/// </summary>
		/// <param name="msg"></param>
		private static void Print(String msg) {
			Console.WriteLine(msg);
			DebugMessages.Add(msg);
		}

		/// <summary>
		/// Attempts to connect to the game lobby server.
		/// </summary>
		public void ConnectToServer() {
			if (Socket != null && Socket.Connected) {
				//full disconnect or the listener thread crashes when the socked is closed -AL
                Disconnect();
			}

			try {
				Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				Socket.Connect(IPAddress.Loopback, GameLobby.GAME_PORT);
				Print("Client Connected");
				StartReceiving();
			} catch (Exception ex) {
				Console.WriteLine(ex);

				Print(ex.Message);
			}
		}

		public void StartReceiving() {
			ReceiveThread = new NetworkClientThread(Socket);
		}

		public void Disconnect() {
			if (ReceiveThread != null) {
				ReceiveThread.Kill();
			}
			if(Socket != null)Socket.Close();
		}


		/// <summary>
		/// Creates a new receiving thread to read from the socket without blocking the main thread.
		/// </summary>
		class NetworkClientThread {

			private Socket ClientSocket;
			private Thread ReceivingThread;
			private bool StayAlive;
		//	private Byte[] buffer;
			private String msg;

			public NetworkClientThread(Socket clientSocket) {
				ClientSocket = clientSocket;
				ReceivingThread = new Thread(Receive) {IsBackground = true};
			    StayAlive = true;
			//	buffer = new Byte[MAX_BUFFER_SIZE];
				ClearReceiveBuffer();
				ReceivingThread.Start();
			}

			/// <summary>
			/// Clears the receive buffer of the socket.
			/// </summary>
			private void ClearReceiveBuffer() {
				if (ClientSocket.Available > 0) {
					Byte[] throwaway = new Byte[ClientSocket.Available];
					ClientSocket.Receive(throwaway);
				}
			}

			/// <summary>
			/// When connected to a game lobby, listens to messages sent by the lobby.
			/// Runs until the thread dies.
			/// </summary>
			private void Receive() {

                byte[] buffer = new byte[MAX_BUFFER_SIZE];
				while (ReceivingThread.IsAlive) {
					Console.WriteLine("Client Receiving: ");
					//Wait for messages
					while (ClientSocket.Available <7 && StayAlive) ;

                    ClientSocket.Receive(buffer);

				    NetworkMessage message = NetworkMessage.Build(buffer);

				    while (message.Length > message.Content.Length)
				    {
                        Array.Clear(buffer, 0, MAX_BUFFER_SIZE);
                        ClientSocket.Receive(buffer);
                        message.AddContent(buffer);
				    }

				    if (message.Type == MessageType.Debug)
				    {
				        Print(message.Content);
				    }

					Array.Clear(buffer, 0, MAX_BUFFER_SIZE);
				}
			}

			/// <summary>
			/// Kills the running thread.
			/// </summary>
			public void Kill() {
				StayAlive = false;
				ReceivingThread.Abort();
			}

		}


	}
}
