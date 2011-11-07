using System;
using System.Net;
using System.Net.Sockets;

namespace Cyclotron2D.Network
{
    internal class NetworkClient
    {
        public const int MAX_BUFFER_SIZE = 1024;
        private Socket client;

        public NetworkClient()
        {
            try
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(IPAddress.Loopback, GameLobby.GAME_PORT);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void Receive()
        {
            Byte[] buffer = new Byte[MAX_BUFFER_SIZE];
            String msg;
            while (client.Available > 0)
            {
                client.Receive(buffer);
                msg = buffer.ToString();
                Console.Write(msg);
                DebugMessages.Add(msg);
            }
        }
    }
}