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

        AckUdpSetup,

        StopTcp,

        MsgReceived
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

        public bool RequiresConfirmation { get; set; }

        public long SequenceNumber { get; set; }

        public string HeaderLine { get { return (byte) Type + " " + Source + " " + SequenceNumber + " " + Length + " " + RequiresConfirmation; } }

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

                string[] header = s.Substring(0, s.IndexOf(EndOfHeader)).Split(new []{' '});
                string content = s.Substring(s.IndexOf(EndOfHeader) + EndOfHeader.Length);

                return new NetworkMessage((MessageType)byte.Parse(header[0]), content)
                           {
                               Source = byte.Parse(header[1]),
                               SequenceNumber = long.Parse(header[2]), 
                               Length = int.Parse(header[3]), 
                               RequiresConfirmation = bool.Parse(header[4])
                           };

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