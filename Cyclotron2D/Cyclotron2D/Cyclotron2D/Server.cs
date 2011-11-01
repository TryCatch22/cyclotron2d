using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Cyclotron2D
{
	class Server
	{
		public int Port { get; private set; }
		public List<Socket> ClientConnections { get; private set; }

		private Socket socket;
		const int BufferSize = 100;
		private byte[] buffer = new byte[BufferSize];

		public Server(int port)
		{
			Port = port;
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		}

		public void WaitForConnection()
		{
			Messages.Add("Waiting for connections...");

			IPEndPoint ep = new IPEndPoint(IPAddress.Any, Port);
			socket.Bind(ep);
			WaitForData();
		}

		private void WaitForData()
		{
			socket.BeginReceive(buffer, 0, BufferSize, SocketFlags.None, OnDataReceived, null);
		}

		private void OnDataReceived(IAsyncResult result)
		{
			Messages.Add("Received data: " + FilterReceivedBytes(buffer));
			WaitForData();
		}

		private string FilterReceivedBytes(byte[] bytes)
		{
			var printable = Art.Font.Characters;
			return new string(Encoding.Unicode.GetChars(bytes).Where(x => printable.Contains(x)).ToArray());
		}
	}
}
