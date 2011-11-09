using System.Net.Sockets;

namespace Cyclotron2D.Network
{

    enum Msg : byte
    {
        Probe,
        Ack,
        PingInit,
        Ping,
         
    }

    /// <summary>
    /// represents a connection between 2 players
    /// </summary>
    public class Connection
    {

        private Socket m_socket;

        public Connection()
        {
            m_socket =  new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void AcceptConnection()
        {
         //   m_socket.co
        }


    }
}
