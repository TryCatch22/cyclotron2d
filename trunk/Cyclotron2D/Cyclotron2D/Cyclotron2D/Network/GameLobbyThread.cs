using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;


namespace Cyclotron2D.Network {
	class GameLobbyThread {

		//ManualResetEvent WaitHandle;
		List<Socket> Clients;
		//GameLobby Server;
		Socket ServerSocket;
		private Thread ConnectionThread;
		public bool IsAlive { get {return ConnectionThread.IsAlive;} }

		public GameLobbyThread(GameLobby server, String name = "Unnamed") {
			//WaitHandle = new ManualResetEvent(false);
		//	this.Server = server;
			Clients = server.Clients;
			ServerSocket = server.GameLobbySocket;

			ConnectionThread = new Thread(WaitForClient) {IsBackground = true, Name = name};
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
		/// Starts accepting incoming connections.
		/// This will block the thread until a connection is found.
		/// </summary>
		public void WaitForClient() {
			try {
				Socket client = ServerSocket.Accept();
				print("Accepting Connection ...");
				Clients.Add(client);
			    print("Accepted Client #" + Clients.Count);
			} catch (SocketException ex) {
				Console.WriteLine(ex);
			} catch (ThreadAbortException) {
				print("Listener Killed");
			}
				
		}

		public void Start() {
			ConnectionThread.Start();
			print("Listening Thread Started");
		}

		public void Kill() {
			ConnectionThread.Abort();
		}

	}
}
