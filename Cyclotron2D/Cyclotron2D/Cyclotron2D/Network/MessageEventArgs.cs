using System;
using System.Net.Sockets;

namespace Cyclotron2D.Network
{
    public class MessageEventArgs : EventArgs
    {
        public NetworkMessage Message { get; private set; }

        public NetworkConnection Connection { get; private set; }

        public MessageEventArgs(NetworkMessage message, NetworkConnection connection)
        {
            Message = message;
            Connection = connection;
        }
    }

    public class SocketEventArgs: EventArgs
    {

        public Socket Socket { get; private set; }

        public SocketEventArgs(Socket socket)
        {
            Socket = socket;
        }
   

    }

    public class ConnectionEventArgs : EventArgs
    {

        public NetworkConnection Connection { get; private set; }

        public ConnectionEventArgs(NetworkConnection socket)
        {
            Connection = socket;
        }


    }
}
