using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Cyclotron2D.Network
{
	class NetworkClient
	{

		public const int MAX_BUFFER_SIZE = 1024;
		private Socket Client;
		private NetworkClientThread ReceiveThread;

		/// <summary>
		/// Creates a network client object representing a player trying to connect to a game lobby.
		/// </summary>
		public NetworkClient()
		{
			try
			{
				Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			} catch (Exception ex)
			{
				Console.WriteLine(ex.StackTrace);
			}
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
			try {
				Client.Connect(IPAddress.Loopback, GameLobby.GAME_PORT);
				Print("Client Connected");
				
			} catch (Exception ex) {
				Console.WriteLine(ex.StackTrace);
			}
		}

		public void startReceive() {
			ReceiveThread = new NetworkClientThread(Client);
		}

		class NetworkClientThread {

			private Socket ClientSocket;
			private Thread ReceivingThread;

			public NetworkClientThread(Socket clientSocket) {
				ClientSocket = clientSocket;
				ReceivingThread = new Thread(new ThreadStart(this.Receive));
				ReceivingThread.IsBackground = true;
				ReceivingThread.Start();
			}


			/// <summary>
			/// When connected to a game lobby, listens to messages sent by the lobby.
			/// Runs infinitely.
			/// </summary>
			public void Receive() {
				Byte[] buffer = new Byte[MAX_BUFFER_SIZE];
				String msg;
				while(ReceivingThread.IsAlive) {
					Console.WriteLine("Client Receiving: ");
					//Wait for messages
					DateTime startRcv = DateTime.UtcNow;
					while (ClientSocket.Available == 0) ;
					while (ClientSocket.Available > 0) {
						ClientSocket.Receive(buffer);
						msg = Encoding.Unicode.GetString(buffer).TrimEnd(new[] { '\0' });
						Print(msg);
					}
					Console.WriteLine("Client Done Receiving");
					Array.Clear(buffer, 0, MAX_BUFFER_SIZE);
				}
			}

			/// <summary>
			/// Kills the running thread.
			/// </summary>
			public void Kill() {
				ReceivingThread.Abort();
			}

		}


	}
}
