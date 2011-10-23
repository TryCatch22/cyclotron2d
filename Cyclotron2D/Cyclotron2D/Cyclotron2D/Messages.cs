using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Cyclotron2D
{
	static class Messages
	{
		struct Message
		{
			public string Text;
			public float TimeLeft;

			public Message(string text, float timeLeft)
			{
				Text = text;
				TimeLeft = timeLeft;
			}
		}

		private static List<Message> messages = new List<Message>();
		private static object lockObject = new object();
		public static float DisplayTime = 3f;

		public static void Add(string message)
		{
			lock (lockObject)
				messages.Add(new Message(message, DisplayTime));
		}

		public static void Draw(SpriteBatch spriteBatch, GameTime gameTime)
		{
			for (int i = 0; i < messages.Count; i++)
			{
				float timeLeft = messages[i].TimeLeft - (float)gameTime.ElapsedGameTime.TotalSeconds;
				messages[i] = new Message(messages[i].Text, timeLeft);
				if (timeLeft <= 0)
					messages.RemoveAt(i--);
			}

			for (int i = 0; i < messages.Count; i++)
			{
				float alpha = messages[i].TimeLeft < 1f ? messages[i].TimeLeft : 1f;
				Vector2 stringSize = Art.Font.MeasureString(messages[i].Text);
				Vector2 pos = new Vector2(5, stringSize.Y * i + 5);

				spriteBatch.Draw(Art.Pixel, new Rectangle((int)pos.X, (int)pos.Y, (int)stringSize.X, (int)stringSize.Y), Color.White * 0.5f * alpha);
				spriteBatch.DrawString(Art.Font, messages[i].Text, pos, Color.Purple * alpha);
			}
		}
	}
}