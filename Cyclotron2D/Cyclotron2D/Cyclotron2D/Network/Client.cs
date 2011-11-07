using System.Net.Sockets;
using System.Text;

namespace Cyclotron2D.Network
{
    internal class Client
    {
        public Client(int port)
        {
            Port = port;
        }

        public int Port { get; private set; }
        public Socket Socket { get; private set; }

        public void Connect()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Socket.Connect("localhost", Port);
            DebugMessages.Add("Connecting...");
            Socket.Send(Encoding.Unicode.GetBytes("Hello"));
        }
    }
}