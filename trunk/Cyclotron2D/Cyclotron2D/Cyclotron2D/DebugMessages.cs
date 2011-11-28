using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cyclotron2D.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Cyclotron2D {
	internal class DebugMessages {

		#region Constants

		private const float DisplayTime = 7f;

		#endregion

		#region Properties

		public static bool LogMessages { get; set; }

		#endregion

		#region Fields

		private static readonly object s_fileLock;

        private static readonly object s_msgLock;

		private static readonly List<Message> s_messages;

	    private static readonly Queue<string> s_logMessages;

		private static string s_logFile;

		#endregion

		#region Constructor

		static DebugMessages() {
#if DEBUG

			s_msgLock = new object();
            s_fileLock = new object();
			s_messages = new List<Message>();
            s_logMessages = new Queue<string>();
            
            SetupLog();

		    if (File.Exists(s_logFile))
		    {
		        File.Delete(s_logFile);
		    }
#endif
		}


        private static void SetupLog()
        {
             s_logFile = @"logs\";

            if(!Directory.Exists(s_logFile))
            {
                Directory.CreateDirectory(s_logFile);
            }

		    s_logFile += DateTime.Now.ToString("d").Replace('/', '.') + @"\";

            if (!Directory.Exists(s_logFile))
            {
                Directory.CreateDirectory(s_logFile);
            }


		    s_logFile += "log_" + DateTime.Now.ToString(@"HH.mm.ss") + ".txt";
        }

		#endregion



		#region Public Methods

		public static void Add(string message) {
#if DEBUG
			lock (s_msgLock)
				s_messages.Add(new Message(message, DisplayTime));

            AddLogOnly(message);
#endif
		}

        public static void AddLogOnly(string message)
        {
#if DEBUG

            if (LogMessages)
            {
                s_logMessages.Enqueue("[" + DateTime.Now.ToString(@"HH:mm:ss.ff") + "] " + message);
            }
#endif
        }

		public static void Update(GameTime gameTime) {
#if DEBUG
			foreach (var message in s_messages) {
				message.TimeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
			}
            s_messages.RemoveAll(msg => msg.TimeLeft <= 0);
#endif
		}


		public static void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
#if DEBUG
			int i = 0;

			lock (s_msgLock) {
				foreach (var message in s_messages) {
					var alpha = message.TimeLeft < 1f ? message.TimeLeft : 1f;
					var pos = new Vector2(5, message.Size.Y * i + 5);
					var rect = new Rectangle((int)pos.X, (int)pos.Y, (int)message.Size.X, (int)message.Size.Y);

					spriteBatch.Draw(Art.Pixel, rect, Color.White * 0.5f * alpha);
					spriteBatch.DrawString(Art.Font, s_messages[i++].Text, pos, Color.Purple * alpha, 0.0f, Vector2.Zero, 0.3f, SpriteEffects.None, 0.0f);
				}
			}
#endif
		}

		public static void FlushLog()
		{
#if DEBUG
			if (!LogMessages)
				return;

		    (new Thread(() =>
		        {
                    lock (s_fileLock)
                    {
                        using (StreamWriter writer = new StreamWriter(@s_logFile,true))
                        {
                            while(s_logMessages.Count > 0)
                            {
                                writer.WriteLine(s_logMessages.Dequeue());
                            }
                        }
                    }
		        })).Start();

#endif
        }

		#endregion
	}

	#region Sub Classes

	public class Message {
		public Message(string text, float timeLeft) {
			Text = text;
			TimeLeft = timeLeft;

		}

		public string Text { get; set; }

		public float TimeLeft { get; set; }

		public Vector2 Size { get { return Art.Font.MeasureString(Text); } }

		public override string ToString() { return Text; }
	}

	#endregion
}