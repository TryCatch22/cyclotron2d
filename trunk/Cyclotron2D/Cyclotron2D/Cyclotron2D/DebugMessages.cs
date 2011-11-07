using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cyclotron2D
{
    internal class DebugMessages
    {
        #region Constants

        private const float DisplayTime = 3f;

        #endregion

        #region Fields

        private static readonly object s_lock;

        private static readonly List<Message> s_messages;

        #endregion

        #region Constructor

        static DebugMessages()
        {
            s_lock = new object();
            s_messages = new List<Message>();
        }

        #endregion

        #region Sub Classes

        private class Message
        {
            public Message(string text, float timeLeft)
            {
                Text = text;
                TimeLeft = timeLeft;
            }

            public string Text { get; set; }

            public float TimeLeft { get; set; }

            public Vector2 Size { get { return Art.Font.MeasureString(Text); } }
        }

        #endregion

        #region Public Methods

        public static void Add(string message)
        {
#if DEBUG
            lock (s_lock)
                s_messages.Add(new Message(message, DisplayTime));
#endif
        }


        public static void Update(GameTime gameTime)
        {
#if DEBUG
            foreach (var message in s_messages)
            {
                message.TimeLeft -= (float) gameTime.ElapsedGameTime.TotalSeconds;
            }

            s_messages.RemoveAll(msg => msg.TimeLeft <= 0);
#endif
        }

        public static void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
#if DEBUG
            int i = 0;

            foreach (var message in s_messages)
            {
                var alpha = message.TimeLeft < 1f ? message.TimeLeft : 1f;
                var pos = new Vector2(5, message.Size.Y*i + 5);
                var rect = new Rectangle((int) pos.X, (int) pos.Y, (int) message.Size.X, (int) message.Size.Y);

                spriteBatch.Draw(Art.Pixel, rect, Color.White*0.5f*alpha);
                spriteBatch.DrawString(Art.Font, s_messages[i++].Text, pos, Color.Purple*alpha);
            }
#endif
        }

        #endregion
    }
}