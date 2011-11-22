using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cyclotron2D.Components;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Network {

    /// <summary>
    /// The Lobby runs on the Host machine and waits for incomming connections.
    /// Throws an Event when a new connection arrives.
    /// 
    /// </summary>
	public class GameLobby : CyclotronComponent
    {
        #region Constants

        /// <summary>
        /// Randomly chosen port number for game lobby
        /// </summary>
		public const int GAME_PORT = 9081;

		private const int MAX_CLIENTS = 5;

		private const int CONNECTION_BACKLOG = 10;

        #endregion

        #region Fields

		private readonly List<Thread> m_acceptThreads;

		private bool m_waitingForConnections;

        #endregion

        #region Properties

        public Socket GameLobbySocket { get; private set; }

		public List<Socket> Clients { get; private set; }

        /// <summary>
        /// Verifies if the game lobby has reached the maximum number of clients.
        /// </summary>
        /// <returns></returns>
        public bool IsFull { get { return Clients.Count >= MAX_CLIENTS; } }

        #endregion

        #region Constructor

        /// <summary>
		/// Creates a new instance of the game lobby used to connect multiple clients.
		/// </summary>
		public GameLobby(Game game) : base(game)
        {
            Clients = new List<Socket>();
            m_acceptThreads = new List<Thread>();			
		}

        #endregion

        #region Events

	    public event EventHandler<ConnectionEventArgs> NewConnection;

        private void InvokeNewConnection(ConnectionEventArgs e)
	    {
            EventHandler<ConnectionEventArgs> handler = NewConnection;
	        if (handler != null) handler(this, e);
	    }

        public event EventHandler<ConnectionEventArgs> LostConnection;

        private void InvokeLostConnection(ConnectionEventArgs e)
        {
            EventHandler<ConnectionEventArgs> handler = LostConnection;
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
        /// Creates multiple conncetion threads to wait for incoming clients and verifies if clients have disconnected.
        /// Maintains this number of listening threads until the lobby is closed.
        /// </summary>
        private void SpawnConnectionThreads()
        {

            while (m_waitingForConnections)
            {
                if (!IsFull && m_acceptThreads.Count < MAX_CLIENTS)
                {
                    SpawnThread();
                }
                //Poll all connected clients to see if someone disconnected
//                if (Clients.Count > 0)
//                {
//                    Socket.Select(null, Clients, null, 1000);
//                }

                m_acceptThreads.RemoveAll(t => !t.IsAlive);

                Thread.Sleep(50);
            }

        }

        /// <summary>
        /// Accepts the next client connections to the lobby assuming the lobby isn't full.
        /// </summary>
        private void SpawnThread()
        {
            if (IsFull || !m_waitingForConnections) return;

            var thread = new Thread(WaitForClient) {IsBackground = true, Name = "Connection Listener"}; 
            thread.Start();
         //   print("Listening Thread Started");
            m_acceptThreads.Add(thread);

        }

        #endregion

        #region Public Methods



        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            lock (Clients)
            {
                foreach (var client in Clients)
                {
                    if (!SocketProbe.IsConnected(client))
                    {
                        DebugMessages.Add("Client Disconnected");
                        InvokeLostConnection(new ConnectionEventArgs(client));
                    }
                }

                Clients.RemoveAll(socket => !SocketProbe.IsConnected(socket));
            }
        }


		/// <summary>
		/// Starts the game lobby by accepting clients until stopped or full.
		/// Messages all clients with a message when the lobby becomes closed.
		/// </summary>
		public void Start()
		{
		    m_waitingForConnections = true;

            try
            {
                GameLobbySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint localServer = new IPEndPoint(IPAddress.Any, GAME_PORT);
                GameLobbySocket.Bind(localServer);
                GameLobbySocket.Blocking = true;
                GameLobbySocket.Listen(CONNECTION_BACKLOG);
                print("Game Lobby Server started on port " + GAME_PORT);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                print("Invalid Port Number " + GAME_PORT);
                Console.WriteLine(ex.StackTrace);
            }



			Thread listenerSpawner = new Thread(SpawnConnectionThreads) {Name = "ListenerSpawningThread"};
		    listenerSpawner.Start();

		}



		/// <summary>
		/// Stops accepting connections.
		/// </summary>
		public void CloseGameLobby() {
			m_waitingForConnections = false;
			GameLobbySocket.Close();
			foreach (var t in m_acceptThreads) {
				t.Abort();
			}
		    m_acceptThreads.Clear();
			print("Closed Lobby");
		}

		/// <summary>
		/// Disconnects all clients and stops all waiting threads.
		/// </summary>
		public void Kill() 
        {
			m_waitingForConnections = false;
            lock (Clients)
            {
                foreach (Socket client in Clients)
                {
                    client.Close();
                }
                Clients.Clear();
            }

			foreach(var thread in m_acceptThreads)
            {
				thread.Abort();
			}
            m_acceptThreads.Clear();
			GameLobbySocket.Close();
        }

        #endregion

        #region Connection Thread

		/// <summary>
		/// Starts accepting incoming connections.
		/// This will block the thread until a connection is found.
		/// </summary>
		private void WaitForClient() {
			try {
				Socket client = GameLobbySocket.Accept();
				print("Accepting Connection ...");
                lock (Clients) { Clients.Add(client); }
                InvokeNewConnection(new ConnectionEventArgs(client));
			    print("Accepted Client #" + Clients.Count);
			} catch (SocketException ex) {
				Console.WriteLine(ex);
			} catch (ThreadAbortException) {
				//print("Listener Killed");
			}
				
		}


        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Kill();
            }
            base.Dispose(disposing);
        }

        #endregion
    }





}