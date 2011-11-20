using System;
using System.Net.Sockets;

namespace Cyclotron2D.Network
{
    public class MessageEventArgs : EventArgs
    {
        public NetworkMessage Message { get; private set; }

        public Socket Socket { get; set; }

        public MessageEventArgs(NetworkMessage message)
        {
            Message = message;
        }
    }
}
