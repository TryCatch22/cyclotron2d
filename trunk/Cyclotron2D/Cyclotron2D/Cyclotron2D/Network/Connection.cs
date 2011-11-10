using System;
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
        //id for the remote player at the other end of this connection.
        public int PlayerID { get; set; }

        /// <summary>
        /// Round trip time for this connection, in ms estimated from initial pings
        /// </summary>
        public int RTT { get; set; }

        private Socket m_socket;

        public Connection()
        {
            m_socket =  new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// this is for accepting initial connections on the host 
        /// </summary>
        public void AcceptClientConnection()
        {
            m_socket.Accept();
            byte[] buf = new byte[1];
            m_socket.Receive(buf);
            if (buf[0] == (byte)Msg.PingInit)
            {
                m_socket.Send(new[] { (byte)Msg.Ping });
                var start = DateTime.Now;
                m_socket.Receive(buf);
                if (buf[0] == (byte)Msg.Ping)
                {
                    RTT = (int)Math.Ceiling((DateTime.Now - start).TotalMilliseconds);
                }
            }
        }

        /// <summary>
        /// this is for initial connections to the host Player
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void ConnectToHost(string ip, int port)
        {
            m_socket.Connect(ip, port);
            byte[] buf = new byte[1];
            m_socket.Send(new []{(byte)Msg.PingInit});
            var start = DateTime.Now;
            m_socket.Receive(buf);
            if (buf[0] == (byte)Msg.Ping)
            {
                RTT = (int)Math.Ceiling((DateTime.Now - start).TotalMilliseconds);
                m_socket.Send(new [] {(byte) Msg.Ping});
                

            }

        }



    }
}
