using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Cyclotron2D
{
	class NetworkClient
	{

		public const int MAX_BUFFER_SIZE = 1024;
		Socket client;

		public NetworkClient()
		{
			try
			{
				client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				client.Connect(IPAddress.Loopback, GameLobby.GAME_PORT);
			} catch (Exception ex)
			{
				Console.WriteLine(ex.StackTrace);
			}
		}

		public void Receive(){
			Byte[] buffer = new Byte[MAX_BUFFER_SIZE];
			String msg;
			while(client.Available > 0) {
				client.Receive(buffer);
				msg = buffer.ToString();
				Console.Write(msg);
				Messages.Add(msg);
			}

		}

	}
}
