using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cyclotron2D.Network
{


    public enum MessageType : byte
    {
        Debug,

        Hello,

        Welcome,

        SetupGame,

        PlayerJoined,

        PlayerLeft,

        Ready,

        AllReady,

        SignalTurn
    }





    /// <summary>
    /// Defines the message format used for communication in Cyclotron
    /// </summary>
    public class NetworkMessage
    {

        protected static Encoding encoding = Encoding.ASCII;//because its the shortest one and i dont think we will need other characters

        public static string EndOfHeader = "\n\n";

        public MessageType Type { get; private set; }

        public byte Source { get; set; }

        private int length = 0;

        public int Length { get { return length == 0 ? encoding.GetByteCount(Content) : length; } set { length = value; } }

        public string Content { get; private set; }

        public List<string> ContentLines { get { return Content.Split(new [] {'\n'}).Where(line => !string.IsNullOrEmpty(line)).ToList(); } }

        public NetworkMessage(MessageType type, string content)
        {
            Type = type;
            Content = content;
        }

        public byte[] Data
        {
            get
            {
                string header = (byte) Type + " " + Source + " " + Length;

                return encoding.GetBytes(header + EndOfHeader + Content);

            }
        }


        public void AddContent(byte[] data)
        {
            Content += encoding.GetString(data).TrimEnd(new []{'\0'});
        }

        public static NetworkMessage Build(byte[] data)
        {
            string s = encoding.GetString(data).TrimEnd(new[] { '\0' });

            string header = s.Substring(0, s.IndexOf(EndOfHeader));
            string content = s.Substring(header.Length + EndOfHeader.Length);

            MessageType type = (MessageType)byte.Parse(header.Substring(0, header.IndexOf(' ')));
            header = header.Substring(header.IndexOf(' ') + 1);

            byte source = byte.Parse(header.Substring(0, header.IndexOf(' ')));
            header = header.Substring(header.IndexOf(' ') + 1);

            int size = int.Parse(header);


        

            return new NetworkMessage(type,content){Source = source, Length = size};
        }



    }

}