using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cyclotron2D
{
	/// <summary>
	/// Defines a TCP message format used for communication with the lobby class.
	/// </summary>

	class TCPMessage
	{
		public enum type { message, data };
		private type msgType;
		private const int typeSize = sizeof(type);

		private int dataLength;
		public byte[] data { get; set; }

		public TCPMessage(type msgType, int dataLength){
			this.msgType = msgType;
			this.dataLength = dataLength;
			data = null;
		}

		
	}
}
