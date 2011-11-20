using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Cyclotron2D.Network {

	public class GameLobby
    {
        #region Constants

        /// <summary>
        /// Randomly chosen port number for game lobby
        /// </summary>
		public const int GAME_PORT = 9081;

		private const int MAX_CLIENTS = 3;

		private const int CONNECTION_BACKLOG = 10;

        #endregion

        #region Fields

        private int ThreadNumber;

		private List<GameLobbyThread> AcceptThreads;

		private bool waitingForConnections;

        #endregion

        #region Properties

        public Socket GameLobbySocket { get; private set; }

		public List<Socket> Clients { get; private set; }

        #endregion

        #region Constructor

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
				ThreadNumber = 0;
				print("Game Lobby Server started on port " + GAME_PORT);
			} catch (ArgumentOutOfRangeException ex) {
				print("Invalid Port Number " + GAME_PORT);
				Console.WriteLine(ex.StackTrace);
			}

			Clients = new List<Socket>();
			AcceptThreads = new List<GameLobbyThread>();
			waitingForConnections = true;
		}

        #endregion

        #region Events

	    public event EventHandler<MessageEventArgs> MessageReceived;

	    private void InvokeMessageReceived(MessageEventArgs e)
	    {
	        EventHandler<MessageEventArgs> handler = MessageReceived;
	        if (handler != null) handler(this, e);
	    }

	    #endregion

        #region Private Methods

        /// <summary>
        /// Temporary Debug Print Method
        /// </summary>
        /// <param name="msg"></param>
        private void print(String msg)
        {
            Console.WriteLine(msg);
            DebugMessages.Add(msg);
        }



        /// <summary>
        /// Accepts the next client connections to the lobby assuming the lobby isn't full.
        /// </summary>
        private void acceptClient()
        {
            if (!isFull() && waitingForConnections)
            {
                //Start a limited amount of threads
                //When lobby is full, kill all leftover threads
                GameLobbyThread AcceptThread = new GameLobbyThread(this, "Listener" + ThreadNumber);
                ThreadNumber++;
                AcceptThread.Start();
                AcceptThreads.Add(AcceptThread);
            }
            else
            {
                //Lobby is Full
                print("Lobby is Full");
                
                messageAllClients(new NetworkMessage(MessageType.Debug, "Okay Lobby Closes. Game On"));
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
		/// Verifies if the game lobby has reached the maximum number of clients.
		/// </summary>
		/// <returns></returns>
		public bool isFull() {
			return Clients.Count >= MAX_CLIENTS;
		}

		/// <summary>
		/// Sends a message to all connected clients.  Removes all clients that are found disconnected
		/// </summary>
		/// <param name="msg"></param>
		public void messageAllClients(NetworkMessage msg) {
			//Byte[] msgData = Encoding.Unicode.GetBytes(msg);
			List<Socket> disconnected = new List<Socket>();
			foreach (Socket client in Clients) {
				try {
					client.Send(msg.Data);
				} catch (SocketException) {
					//Client disconnected -> remove client
					disconnected.Add(client);
				}
			}
			//Remove all disabled clients
			foreach (Socket removed in disconnected) {
				Clients.Remove(removed);
			}
		}


		/// <summary>
		/// Starts the game lobby by accepting clients until stopped or full.
		/// Messages all clients with a message when the lobby becomes closed.
		/// </summary>
		public void Start() {
			Thread listenerSpawner = new Thread(SpawnConnectionThreads) {Name = "ListenerSpawningThread"};
		    listenerSpawner.Start();

		}

		/// <summary>
		/// Creates multiple conncetion threads to wait for incoming clients and verifies if clients have disconnected.
		/// Maintains this number of listening threads until the lobby is closed.
		/// </summary>
		public void SpawnConnectionThreads() {

			while (waitingForConnections) {
				if (!isFull() && AcceptThreads.Count < MAX_CLIENTS) {
					acceptClient();
				}
				//Poll all connected clients to see if someone disconnected
				if (Clients.Count > 0) {
					Socket.Select(null, Clients, null, 1000);
				}
				List<GameLobbyThread> removed = new List<GameLobbyThread>();
				foreach (GameLobbyThread t in AcceptThreads) {
					if (!t.IsAlive) {
						removed.Add(t);
					}
				}
				if (removed.Count > 0) {
					foreach (GameLobbyThread t in removed) {
						AcceptThreads.Remove(t);
					}
				}
				Thread.Sleep(50);
			}

		}

		/// <summary>
		/// Stops accepting connections.
		/// </summary>
		public void CloseGameLobby() {
			waitingForConnections = false;
			GameLobbySocket.Close();
			foreach (GameLobbyThread t in AcceptThreads) {
				t.Kill();
			}
			print("Closed Lobby");
		}

		/// <summary>
		/// Disconnects all clients and stops all waiting threads.
		/// </summary>
		public void Kill() {
			waitingForConnections = false;
			foreach(Socket client in Clients){
				client.Close();
			}
			foreach(GameLobbyThread thread in AcceptThreads){
				thread.Kill();
			}
			GameLobbySocket.Close();
        }

        #endregion
    }


   
}