﻿using System;
using System.Net.Sockets;

namespace Cyclotron2D.Network
{
    public class MessageEventArgs : EventArgs
    {
        public NetworkMessage Message { get; private set; }

        public MessageEventArgs(NetworkMessage message)
        {
            Message = message;
        }
    }

    public class ConnectionEventArgs: EventArgs
    {

        public Socket Socket { get; private set; }

        public ConnectionEventArgs(Socket socket)
        {
            Socket = socket;
        }
   

    }
}
