using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Cyclotron2D.Network {
	internal class GameLobby {
		//Randomly chosen port number for game lobby
		public const int GAME_PORT = 9081;
		private const int MAX_CLIENTS = 5;
		private const int CONNECTION_BACKLOG = 10;

		private List<GameLobbyThread> AcceptThreads;
		private bool waitingForConnections;

		public Socket GameLobbySocket { get; private set; }
		public List<Socket> clients { get; private set; }

		/// <summary>
		/// Creates a new instance of the game lobby used to connect multiple clients.
		/// </summary>
		public GameLobby() {
			try {
				GameLobbySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				IPEndPoint localServer = new IPEndPoint(IPAddress.Loopback, GAME_PORT);
				GameLobbySocket.Bind(localServer);
				GameLobbySocket.Blocking = true;
				GameLobbySocket.Listen(CONNECTION_BACKLOG);
				print("Game Lobby Server started on port " + GAME_PORT);
			} catch (ArgumentOutOfRangeException ex) {
				print("Invalid Port Number");
				Console.WriteLine(ex.StackTrace);
			}

			clients = new List<Socket>();
			AcceptThreads = new List<GameLobbyThread>();
			waitingForConnections = true;
		}

		/// <summary>
		/// Temporary Debug Print Method
		/// </summary>
		/// <param name="msg"></param>
		private void print(String msg) {
			Console.WriteLine(msg);
			DebugMessages.Add(msg);
		}

		/// <summary>
		/// Accepts the next client connections to the lobby assuming the lobby isn't full.
		/// </summary>
		private void acceptClient(String threadName) {
			print("Waiting for a connection ...");
			if (!isFull() && waitingForConnections) {
				//Start a limited amount of threads from a thread pool.
				//When lobby is full, kill all leftover threads
				GameLobbyThread AcceptThread = new GameLobbyThread(this, threadName);
				AcceptThread.Start();
				AcceptThreads.Add(AcceptThread);
			} else {
				//Lobby is Full
				print("Lobby is Full");
				messageAllClients("Okay Lobby Closes. Game On");
			}
		}


		/// <summary>
		/// Verifies if the game lobby has reached the maximum number of clients.
		/// </summary>
		/// <returns></returns>
		public bool isFull() {
			return clients.Count >= MAX_CLIENTS;
		}

		/// <summary>
		/// Sends a message to all connected clients.  Removes all clients that are found disconnected
		/// </summary>
		/// <param name="msg"></param>
		public void messageAllClients(String msg) {
			Byte[] msgData = Encoding.Unicode.GetBytes(msg);
			List<Socket> disconnected = new List<Socket>();
			foreach (Socket client in clients) {
				try {
					client.Send(msgData);
				} catch (SocketException) {
					//Client disconnected -> remove client
					disconnected.Add(client);
				}
			}

			foreach (Socket removed in disconnected) {
				clients.Remove(removed);
			}
		}

		/// <summary>
		/// Starts the game lobby by accepting clients until stopped or full.
		/// Messages all clients with a message when the lobby becomes locked.
		/// </summary>
		public void start() {
			acceptClient("Listener1");
			acceptClient("Listener2");

			//if (isFull())
			//{
			//    waitingForConnections = false;
			//}
		}
	}
}