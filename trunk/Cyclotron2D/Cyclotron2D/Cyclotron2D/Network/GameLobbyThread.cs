using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;


namespace Cyclotron2D.Network {
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

			ConnectionThread = new Thread(new ThreadStart(this.WaitForClient));
			ConnectionThread.IsBackground = true;
			ConnectionThread.Name = name;
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
		/// Starts accepting incoming connections
		/// </summary>
		public void WaitForClient() {
			WaitHandle.Reset();
			ServerSocket.BeginAccept(new AsyncCallback(ConnectClientCallback), ServerSocket);
			//Wait for a connection (blocks current thread)
			WaitHandle.WaitOne();
		}

		/// <summary>
		/// Once a client is trying to connect, this connects it and gets the socket to communicate with it. 
		/// </summary>
		/// <param name="target"></param>
		public void ConnectClientCallback(IAsyncResult target) {
			//Unlocks current thread
			WaitHandle.Set();
			
			print("Accepting Connection ...");
			Socket lobby = (Socket)target.AsyncState;
			Socket client = lobby.EndAccept(target);
			//Add the client to the clients list
			Clients.Add(client);
			print("Accepted 1 Client");
			Server.messageAllClients("We got 1 more player");
		}

		public void Start() {
			ConnectionThread.Start();
		}

		public void Stop() {
			ConnectionThread.Abort();
		}

	}
}
