namespace Cyclotron2D.Network
{
    /// <summary>
    /// Defines a TCP message format used for communication with the lobby class.
    /// </summary>
    internal class TCPMessage
    {
        #region type enum

        public enum type
        {
            message,
            data
        } ;

        #endregion

        private const int typeSize = sizeof (type);

        private int dataLength;
        private type msgType;

        public TCPMessage(type msgType, int dataLength)
        {
            this.msgType = msgType;
            this.dataLength = dataLength;
            data = null;
        }

        public byte[] data { get; set; }
    }
}