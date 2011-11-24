using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Cyclotron2D
{
	class Client
	{
		public int Port { get; private set; }
		public Socket Socket { get; private set; }

		public Client(int port)
		{
			Port = port;
		}

		public void Connect()
		{
			Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			Socket.Connect("localhost", Port);
			Messages.Set("Connecting...");
			Socket.Send(new NetworkMessage(NetworkCmd.Hello, Game1.Name));
		}
	}
}
