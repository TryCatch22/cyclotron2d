using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Cyclotron2D
{
	public enum NetworkCmd : byte { Hello, Goodbye, GameplayUpdate }

	class Server
	{
		public int Port { get; private set; }
		public List<ClientConnection> ClientConnections { get; private set; }

		private Socket socket;
		EndPoint endpoint;
		const int BufferSize = 100;

		public Server(int port)
		{
			Port = port;
			ClientConnections = new List<ClientConnection>();
		}

		public void WaitForConnection()
		{
			Messages.Set("Waiting for connections...");

			endpoint = new IPEndPoint(IPAddress.Any, Port);
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.Bind(endpoint);
			WaitForData();
		}

		private void WaitForData()
		{
			Messages.Set("connections", string.Join("\n", ClientConnections.Select(x => x.Name)));

			byte[] buffer = new byte[BufferSize];
			socket.BeginReceiveFrom(buffer, 0, BufferSize, SocketFlags.None, ref endpoint,
				x => OnDataReceived(buffer, x) , null);
		}

		private void OnDataReceived(byte[] buffer, IAsyncResult result)
		{
			Messages.Set("Received data: " + FilterReceivedBytes(buffer) + " from IP: " + ((IPEndPoint)endpoint).Address);
			RespondToReceivedData(buffer, (IPEndPoint)endpoint);
			WaitForData();
		}

		private void RespondToReceivedData(byte[] bytes, IPEndPoint endpoint)
		{
			var message = new NetworkMessage(bytes);
			switch (message.Command)
			{
				case NetworkCmd.Hello:
					AddClientConnection(endpoint.Address, message.Data);
					break;
			}
		}

		private void AddClientConnection(IPAddress ip, string name)
		{
			var sameIp = ClientConnections.FirstOrDefault(x => x.IP.Equals(ip));
			var sameName = ClientConnections.FirstOrDefault(x => x.Name == name);

			if (sameIp != null && sameIp == sameName)
				return; // It just wants to say "Hi" again

			if (sameIp != null)
				return; // Same IP, but different computer. Figure out how to handle this later.

			if (sameName != null)
			{
				// Same name but different IP. Rename the new user.
				string newName;
				int i = 2;
				do
				{
					newName = string.Format("{0} ({1})", name, i);
					i++;
				} while (ClientConnections.Any(x => x.Name == newName));
			}
		}

		private string FilterReceivedBytes(byte[] bytes)
		{
			var printable = Art.Font.Characters;
			return Network.BytesToString(bytes, x => printable.Contains(x));
		}
	}

	class ClientConnection
	{
		public IPAddress IP { get; private set; }
		public string Name { get; private set; }
	}
}
