using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;


namespace Cyclotron2D {
	class GameLobbyThread {

		ManualResetEvent WaitHandle;
		List<Socket> Clients;
		GameLobby Server;
		Socket ServerSocket;
		Thread ConnectionThread;

		public GameLobbyThread(GameLobby server, String name = "Unnamed") {
			WaitHandle = new ManualResetEvent(false);
			this.Server = server;
			this.Clients = server.clients;
			this.ServerSocket = server.GameLobbySocket;

			ConnectionThread = new Thread(new ThreadStart(this.waitForClient));
			ConnectionThread.IsBackground = true;
			ConnectionThread.Name = name;
		}

		/// <summary>
		/// Starts accepting incoming connections
		/// </summary>
		public void waitForClient() {
			WaitHandle.Reset();
			ServerSocket.BeginAccept(new AsyncCallback(connectClientCallback), ServerSocket);
			//Wait for a connection (blocks current thread)
			WaitHandle.WaitOne();
		}

		/// <summary>
		/// Once a client is trying to connect, this connects it and gets the socket to communicate with it. 
		/// </summary>
		/// <param name="target"></param>
		public void connectClientCallback(IAsyncResult target) {
			Console.WriteLine("Accepting Connection ...");
			Socket lobby = (Socket)target.AsyncState;
			Socket client = lobby.EndAccept(target);
			//Add the client to the clients list
			Clients.Add(client);
			Console.WriteLine("Accepted 1 Client");

			//Unlocks current thread
			WaitHandle.Set();

		}

		public void start() {
			ConnectionThread.Start();
		}

		public void stop() {
			ConnectionThread.Abort();
		}

	}
}
