using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cyclotron2D.Network
{


    public enum MessageType : byte
    {
        Debug,

        Ping,

        Hello,

        Welcome,

        SetupGame,

        SetupGameUdp,

        PlayerJoined,

        PlayerLeft,

        Ready,

        AllReady,

        SignalTurn,

        PlayerInfoUpdate,
    
        RealDeath,

        AckDeath,
        AckUdpSetup,
        StopTcp
    }





    /// <summary>
    /// Defines the message format used for communication in Cyclotron
    /// </summary>
    public class NetworkMessage
    {

        public static Encoding MsgEncoding = Encoding.ASCII;//because its the shortest one and i dont think we will need other characters

        public static string EndOfHeader = "\n\n";

        public MessageType Type { get; set; }

        public byte Source { get; set; }

        public long SequenceNumber { get; set; }

        public string HeaderLine { get { return (byte) Type + " " + Source + " " + SequenceNumber +" "+Length; } }

        private int length = 0;

        public int Length { get { return length == 0 ? MsgEncoding.GetByteCount(Content) : length; } set { length = value; } }

        public string Content { get; set; }

        public List<string> ContentLines { get { return Content.Split(new [] {'\n'}).Where(line => !string.IsNullOrEmpty(line)).ToList(); } }

        public NetworkMessage(MessageType type, string content)
        {
            Type = type;
            Content = content;
        }

        public byte[] Data { get { return MsgEncoding.GetBytes(HeaderLine + EndOfHeader + Content); } }


        public void AddContent(byte[] data)
        {
            Content += MsgEncoding.GetString(data).TrimEnd(new []{'\0'});
        }

        public static NetworkMessage Build(byte[] data)
        {
            if(data.Length > NetworkConnection.MAX_BUFFER_SIZE)
            {
                throw new Exception("Data is too long");
            }

            try
            {
                string s = MsgEncoding.GetString(data).TrimEnd(new[] { '\0' });

                string header = s.Substring(0, s.IndexOf(EndOfHeader));
                string content = s.Substring(header.Length + EndOfHeader.Length);

                MessageType type = (MessageType)byte.Parse(header.Substring(0, header.IndexOf(' ')));
                header = header.Substring(header.IndexOf(' ') + 1);

                byte source = byte.Parse(header.Substring(0, header.IndexOf(' ')));
                header = header.Substring(header.IndexOf(' ') + 1);

                long seqnum = long.Parse(header.Substring(0, header.IndexOf(' ')));
                header = header.Substring(header.IndexOf(' ') + 1);

                int size = int.Parse(header);

                return new NetworkMessage(type, content) { Source = source, Length = size, SequenceNumber = seqnum};

            }
            catch (Exception e )
            {
                DebugMessages.AddLogOnly("Crash on Message Build: " + e.Message + e.StackTrace);
                DebugMessages.FlushLog();
                throw;
            }

           

        

          
        }



    }

}