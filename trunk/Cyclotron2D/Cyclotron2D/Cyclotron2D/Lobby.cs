using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Cyclotron2D {
	class Lobby {

		//Randomly chosen port number for game lobby
		public const int GAME_PORT = 9081;
		private const int MAX_CLIENTS = 1;
		private const int CONNECTION_BACKLOG = 10;

		private Socket GameLobby;
		private List<Socket> clients;
		private bool waitingForConnections;

		static ManualResetEvent waitHandle = new ManualResetEvent(false);

		/// <summary>
		/// Creates a new instance of the game lobby used to connect multiple clients.
		/// </summary>
		public Lobby() {

			try {
				this.GameLobby = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				IPEndPoint localServer = new IPEndPoint(IPAddress.Loopback, GAME_PORT);
				GameLobby.Bind(localServer);
				GameLobby.Blocking = false;
				GameLobby.Listen(CONNECTION_BACKLOG);
				Console.WriteLine("Game Lobby Server started on port " + GAME_PORT);
			} catch (ArgumentOutOfRangeException ex) {
				Console.WriteLine("Invalid Port Number");
				Console.WriteLine(ex.StackTrace);
			}

			clients = new List<Socket>();
			waitingForConnections = true;

		}

		/// <summary>
		/// Accepts the next client connections to the lobby assuming the lobby isn't full.
		/// </summary>
		private void acceptClient() {

			Console.WriteLine("Waiting for a connection ...");
			waitHandle.Reset();
			GameLobby.BeginAccept(new AsyncCallback(connectClient), GameLobby);
			//Wait for a connection (blocks current thread)
			waitHandle.WaitOne();
		}

		/// <summary>
		/// Once a client is found, this connects the client and gets the socket to communicate with it. 
		/// </summary>
		/// <param name="target"></param>
		public void connectClient(IAsyncResult target) {
			Console.WriteLine("Accepting Connection ...");

			Socket lobby = (Socket)target.AsyncState;
			Socket client = lobby.EndAccept(target);
			//Add the client to the clients list
			clients.Add(client);
			Console.WriteLine("Accepted 1 Client");
			//Unlocks current thread
			waitHandle.Set();
		}


		/// <summary>
		/// Verifies if the game lobby has reached the maximum number of clients.
		/// </summary>
		/// <returns></returns>
		public bool isFull() {
			return clients.Count >= MAX_CLIENTS;

		}

		public void messageAllClients(String msg) {
			Byte[] msgData = Encoding.Unicode.GetBytes(msg);
			foreach (Socket client in clients) {
				client.Send(msgData);
			}
		}

		/// <summary>
		/// Starts the game lobby by accepting clients until stopped or full.
		/// Messages all clients with a message when the lobby becomes locked.
		/// </summary>
		public void start() {

			while (!this.isFull() && waitingForConnections) {
				this.acceptClient();
			}

			GameLobby.Close();
			Console.WriteLine("Lobby Closed");
			this.isFull();
			this.messageAllClients("Okay Lobby Closes. Game On");
		}

	}

}


