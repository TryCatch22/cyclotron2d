﻿using System;
using System.Collections.Generic;
using Cyclotron2D.Components;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Helpers;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Network
{


    
    internal class Confirmation
    {
        public NetworkMessage Msg { get; set; }

        public TimeSpan LastNotification { get; set; }
    }


    public class ReliableUdpSender : CyclotronComponent
    {
        public TimeSpan MaxAckDelay { get { return TimeSpanExtension.Max(Game.Communicator.AverageRtt.Mult(2), Game.Communicator.MaximumRtt); } }

        private Dictionary<RemotePlayer, Dictionary<long, Confirmation>> m_confirmations;

        public ReliableUdpSender(Game game)
            : base(game)
        {
            m_confirmations = new Dictionary<RemotePlayer, Dictionary<long, Confirmation>>();
            SubscribeCommunicator();
        }

        #region Subscription

        public void SubscribeCommunicator()
        {
            Game.Communicator.MessageReceived += OnMessageReceived;
            Game.Communicator.ConnectionLost += OnConnectionLost;
        }

        public void UnsubscribeCommunicator()
        {
            Game.Communicator.MessageReceived -= OnMessageReceived;
            Game.Communicator.ConnectionLost -= OnConnectionLost;
        }

        #endregion

        #region Event Handlers

        private void OnConnectionLost(object sender, ConnectionEventArgs e)
        {
            var player = Game.Communicator.GetPlayer(e.Connection);

            if (m_confirmations.ContainsKey(player))
            {
                m_confirmations.Remove(player);
            }
        }

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            switch (e.Message.Type)
            {
                case MessageType.MsgReceived:
                    {
                        long seqId = long.Parse(e.Message.Content);
                        var player = Game.Communicator.GetPlayer(e.Message.Source);
                        if(m_confirmations.ContainsKey(player) && m_confirmations[player].ContainsKey(seqId))
                        {
                            m_confirmations[player].Remove(seqId);
                        }
                    }
                    break;
                default:
//                    {
//                        if(e.Message.RequiresConfirmation)
//                        {
//                            Game.Communicator.MessagePlayer(Game.Communicator.GetPlayer(e.Message.Source), 
//                                                    new NetworkMessage(MessageType.MsgReceived, e.Message.SequenceNumber.ToString()));
//                        }
//                    }
                    break;
            }
        }

        #endregion

        #region Public Methods

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            foreach (var key in m_confirmations.Keys)
            {
                foreach (var kvp in m_confirmations[key])
                {

                    if (gameTime.TotalGameTime > kvp.Value.LastNotification + MaxAckDelay)
                    {
                        DebugMessages.AddLogOnly("unconfirmed Msg. Resending: " + kvp.Value.Msg.Type);
                        Game.Communicator.MessagePlayer(key, kvp.Value.Msg);
                        m_confirmations[key][kvp.Key].LastNotification = gameTime.TotalGameTime;
                    }
                    
                }
            }
        }

        public void Initialize(List<RemotePlayer> players)
        {
            foreach (var remotePlayer in players)
            {
                if(!m_confirmations.ContainsKey(remotePlayer))
                {
                    m_confirmations.Add(remotePlayer, new Dictionary<long, Confirmation>());
                }
            }
        }

        public void ClearAll()
        {
            m_confirmations.Clear();
        }

        public List<long> SendReliableAll(NetworkMessage msg)
        {

            msg.RequiresConfirmation = true;

            List<long> nums = new List<long>();

            foreach (var key in m_confirmations.Keys)
            {
                Game.Communicator.MessagePlayer(key, msg);
                m_confirmations[key].Add(msg.SequenceNumber, new Confirmation(){Msg = msg, LastNotification = Game.GameTime.TotalGameTime});

            }

            return nums;
        }

        public long SendReliable(RemotePlayer player, NetworkMessage msg)
        {
            msg.RequiresConfirmation = true;
            Game.Communicator.MessagePlayer(player, msg);

            m_confirmations[player].Add(msg.SequenceNumber, new Confirmation() { Msg = msg, LastNotification = Game.GameTime.TotalGameTime });
            return msg.SequenceNumber;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeCommunicator();
                ClearAll();
            }
            base.Dispose(disposing);
        }

    }
}
