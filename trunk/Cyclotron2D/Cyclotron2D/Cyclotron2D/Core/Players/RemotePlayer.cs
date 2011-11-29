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

        private long m_lastReceivedSeqNum;

        public RemotePlayer(Game game, Screen screen)
            : base(game, screen)
        {
            m_lastReceivedSeqNum = 0;
        }

        public override string Name { get; set; }


        #region Event Handlers

        protected override void OnMessageReceived(object sender, MessageEventArgs e)
        {
            base.OnMessageReceived(sender, e);
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

                        if (e.Message.SequenceNumber > m_lastReceivedSeqNum)
                        {
                            var lines = e.Message.ContentLines;
                            string sDir = lines[0];
                            lines.RemoveAt(0);

                            Direction dir = (Direction)int.Parse(sDir);

                            List<Point> vertices = lines.Select(PointExtention.FromString).ToList();
                            Cycle.HandleUpdateInfo(dir, vertices);

                            m_lastReceivedSeqNum = e.Message.SequenceNumber;
                        }


                    }
                    break;
                default:
                    return;
            }
        }


        #endregion

    }
}