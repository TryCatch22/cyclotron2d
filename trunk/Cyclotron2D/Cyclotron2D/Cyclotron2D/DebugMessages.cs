using System.Collections.Generic;
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

		private static readonly object s_lock;

		private static readonly List<Message> s_messages;

		private static string s_logFile;

		#endregion

		#region Constructor

		static DebugMessages() {
			s_lock = new object();
			s_messages = new List<Message>();
			s_logFile = "log.txt";
		}

		#endregion



		#region Public Methods

		public static void Add(string message) {
#if DEBUG
			lock (s_lock)
				s_messages.Add(new Message(message, DisplayTime));
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

			lock (s_lock) {
				foreach (var message in s_messages) {
					var alpha = message.TimeLeft < 1f ? message.TimeLeft : 1f;
					var pos = new Vector2(5, message.Size.Y * i + 5);
					var rect = new Rectangle((int)pos.X, (int)pos.Y, (int)message.Size.X, (int)message.Size.Y);

					spriteBatch.Draw(Art.Pixel, rect, Color.White * 0.5f * alpha);
					spriteBatch.DrawString(Art.Font, s_messages[i++].Text, pos, Color.Purple * alpha);
				}
			}
#endif
		}

		public static void WriteLog()
		{
#if DEBUG
			lock (s_lock)
			{
				using (StreamWriter writer = new StreamWriter(s_logFile, false))
				{
					foreach (var message in s_messages)
					{
						writer.WriteLine(message.ToString());
					}
				}
			}
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