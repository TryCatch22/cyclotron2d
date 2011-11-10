﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Cyclotron2D.Network
{
    internal class GameLobby
    {
        //Randomly chosen port number for game lobby
        public const int GAME_PORT = 9081;
        private const int MAX_CLIENTS = 1;
        private const int CONNECTION_BACKLOG = 10;

        private static ManualResetEvent waitHandle = new ManualResetEvent(false);
        private GameLobbyThread ClientAcceptThread; //Demo thread to accept clients without blocking the application
        private bool waitingForConnections;

        /// <summary>
        /// Creates a new instance of the game lobby used to connect multiple clients.
        /// </summary>
        public GameLobby()
        {
            try
            {
                GameLobbySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint localServer = new IPEndPoint(IPAddress.Loopback, GAME_PORT);
                GameLobbySocket.Bind(localServer);
                GameLobbySocket.Blocking = false;
                GameLobbySocket.Listen(CONNECTION_BACKLOG);
                Console.WriteLine("Game Lobby Server started on port " + GAME_PORT);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine("Invalid Port Number");
                Console.WriteLine(ex.StackTrace);
            }

            clients = new List<Socket>();
            waitingForConnections = true;
        }

        public Socket GameLobbySocket { get; private set; }
        public List<Socket> clients { get; private set; }

        /// <summary>
        /// Accepts the next client connections to the lobby assuming the lobby isn't full.
        /// </summary>
        private void acceptClient()
        {
            Console.WriteLine("Waiting for a connection ...");
            //GameLobby.BeginAccept(new AsyncCallback(connectClient), GameLobby);
            //Wait for a connection (blocks current thread)
            if (!isFull() && waitingForConnections)
            {
                //Start a limited amount of threads from a thread pool.
                //When lobby is full, kill all leftover threads
                ClientAcceptThread = new GameLobbyThread(this);
                ClientAcceptThread.start();
                Console.WriteLine("Listening Thread Started");
            }
            else
            {
                //Lobby Full
                Console.WriteLine("Lobby is Full");
                messageAllClients("Okay Lobby Closes. Game On");
            }
        }

        /// <summary>
        /// Once a client is found, this connects the client and gets the socket to communicate with it. 
        /// </summary>
        /// <param name="target"></param>
        public void connectClient(IAsyncResult target)
        {
            Console.WriteLine("Accepting Connection ...");
            if (!isFull())
            {
                Socket lobby = (Socket) target.AsyncState;
                Socket client = lobby.EndAccept(target);
                //Add the client to the clients list
                clients.Add(client);
                Console.WriteLine("Accepted 1 Client");
            }
            else
            {
                //Not accepting the connection
                Console.WriteLine("Connection Refused, Lobby Full");
                waitingForConnections = false;
            }
        }


        /// <summary>
        /// Verifies if the game lobby has reached the maximum number of clients.
        /// </summary>
        /// <returns></returns>
        public bool isFull()
        {
            return clients.Count >= MAX_CLIENTS;
        }

        public void messageAllClients(String msg)
        {
            Byte[] msgData = Encoding.Unicode.GetBytes(msg);
            foreach (Socket client in clients)
            {
                client.Send(msgData);
            }
        }

        /// <summary>
        /// Starts the game lobby by accepting clients until stopped or full.
        /// Messages all clients with a message when the lobby becomes locked.
        /// </summary>
        public void start()
        {
            acceptClient();

            if (isFull())
            {
                waitingForConnections = false;
            }
        }
    }
}