using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Cyclotron2D.Network {
	class NetworkClient {

		public const int MAX_BUFFER_SIZE = 1024;
		private Socket Client;
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
			if (Client != null && Client.Connected) {
				//Closes the open socket and creates a new one
				Client.Close();
			}

			try {
				Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				Client.Connect(IPAddress.Loopback, GameLobby.GAME_PORT);
				Print("Client Connected");
				StartReceiving();
			} catch (Exception ex) {
				Console.WriteLine(ex);

				Print(ex.Message);
			}
		}

		public void StartReceiving() {
			ReceiveThread = new NetworkClientThread(Client);
		}

		public void Disconnect() {
			if (ReceiveThread != null) {
				ReceiveThread.Kill();
			}
			Client.Close();
		}


		/// <summary>
		/// Creates a new receiving thread to read from the socket without blocking the main thread.
		/// </summary>
		class NetworkClientThread {

			private Socket ClientSocket;
			private Thread ReceivingThread;
			private bool StayAlive;
			private Byte[] buffer;
			private String msg;

			public NetworkClientThread(Socket clientSocket) {
				ClientSocket = clientSocket;
				ReceivingThread = new Thread(new ThreadStart(this.Receive));
				ReceivingThread.IsBackground = true;
				StayAlive = true;
				buffer = new Byte[MAX_BUFFER_SIZE];
				ClearReceiveBuffer();
				ReceivingThread.Start();
			}

			/// <summary>
			/// Clears the receive buffer of the socket.
			/// </summary>
			public void ClearReceiveBuffer() {
				if (ClientSocket.Available > 0) {
					Byte[] throwaway = new Byte[ClientSocket.Available];
					ClientSocket.Receive(throwaway);
				}
			}

			/// <summary>
			/// When connected to a game lobby, listens to messages sent by the lobby.
			/// Runs until the thread dies.
			/// </summary>
			public void Receive() {
				while (ReceivingThread.IsAlive) {
					Console.WriteLine("Client Receiving: ");
					//Wait for messages
					DateTime startRcv = DateTime.UtcNow;
					while (ClientSocket.Available == 0 && StayAlive) ;
					while (ClientSocket.Available > 0) {
						ClientSocket.Receive(buffer);
						msg = Encoding.Unicode.GetString(buffer).TrimEnd(new[] { '\0' });
						Print(msg);
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
