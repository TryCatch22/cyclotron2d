using System;
using System.Collections.Generic;
using System.Linq;
using Cyclotron2D.Helpers;
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
            //SubscribeConnection();
        }

        public override string Name { get; set; } 



        #region Subscription

        public void SubscribeConnection()
        {
            Game.Communicator.MessageReceived += OnMessageReceived;
        }

        private void UnsubscribeConnection()
        {
            Game.Communicator.MessageReceived -= OnMessageReceived;
        }


        #endregion

        #region Event Handlers

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            //handle only messages from the player this instance represents
            if (e.Message.Source != PlayerID) return;

            switch (e.Message.Type)
            {
                case MessageType.SignalTurn:
                    {
                        int sep = e.Message.Content.IndexOf(' ');

                        string direction = e.Message.Content.Substring(0, sep);
                        string point = e.Message.Content.Substring(sep + 1);

                        InvokeDirectionChange(new DirectionChangeEventArgs((Direction)int.Parse(direction), PointExtention.FromString(point)));
                    }
                    break;
                case MessageType.PlayerInfoUpdate:
                    {
                        List<Point> vertices = e.Message.ContentLines.Select(line => PointExtention.FromString(line)).ToList();
                        Cycle.HandleUpdateInfo(vertices);
                    }
                    break;
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