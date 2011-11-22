using System;
using Cyclotron2D.Network;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Core.Players
{
    /// <summary>
    /// Remotely connected player, Events are generated from incomming messages
    /// </summary>
    public class RemotePlayer : Player
    {
        public RemotePlayer(Game game, Screen screen) : base(game, screen)
        {
        }

        public override string Name { get; set; } 



        #region Subscription

        private void SubscribeConnection()
        {
            Game.Communicator.Connections[this].MessageReceived += OnMessageReceived;
        }

        private void UnsubscribeConnection()
        {
            if(Game.Communicator.Connections.ContainsKey(this))Game.Communicator.Connections[this].MessageReceived -= OnMessageReceived;
        }


        #endregion

        #region Event Handlers

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            switch (e.Message.Type)
            {
                default:
                    return;
            }
        }


        #endregion


        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeConnection();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}