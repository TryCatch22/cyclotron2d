using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Cyclotron2D.Network
{
	class NetworkClient
	{

		public const int MAX_BUFFER_SIZE = 1024;
		Socket client;

		/// <summary>
		/// Creates a network client object representing a player trying to connect to a game lobby.
		/// </summary>
		public NetworkClient()
		{
			try
			{
				client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			} catch (Exception ex)
			{
				Console.WriteLine(ex.StackTrace);
			}
		}

		/// <summary>
		/// Temporary Debug Print Method
		/// </summary>
		/// <param name="msg"></param>
		private void print(String msg) {
			Console.WriteLine(msg);
			DebugMessages.Add(msg);
		}

		/// <summary>
		/// Attempts to connect to the game lobby server.
		/// </summary>
		public void ConnectToServer() {
			try {
				client.Connect(IPAddress.Loopback, GameLobby.GAME_PORT);
				print("Client Connected");
			} catch (Exception ex) {
				Console.WriteLine(ex.StackTrace);
			}
		}

		/// <summary>
		/// When connected to a game lobby, listens to messages sent by the lobby.
		/// </summary>
		public void Receive(){
			Byte[] buffer = new Byte[MAX_BUFFER_SIZE];
			String msg;
			print("Client Receiving: ");
			//Wait for messages
			DateTime startRcv = DateTime.UtcNow;
			while (client.Available == 0) {
				if (DateTime.UtcNow.Subtract(startRcv).Seconds > 4) {
					print("Client Receive Timeout");
					return;
				}
			}
			while(client.Available > 0) {
				client.Receive(buffer);
				msg = Encoding.Unicode.GetString(buffer).TrimEnd(new [] {'\0'});
				print(msg);
			}
			print("Client Done Receiving");

		}

	}
}
