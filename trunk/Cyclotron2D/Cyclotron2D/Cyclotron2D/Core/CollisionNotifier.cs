using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cyclotron2D.Components;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Network;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Core
{

    internal class Confirmation
    {
        public bool Confirmed { get; set; }

        public TimeSpan LastNotification { get; set; }
    }

    //takes care of managing conflicts on collisions from host to clients
    public class CollisionNotifier: ScreenComponent
    {

        private Engine Engine { get; set; }

        public readonly TimeSpan MaxAckDelay =  new TimeSpan(0, 0, 0, 0, 200);

        private Dictionary<Player, Dictionary<RemotePlayer, Confirmation>> m_confirmations;


        public CollisionNotifier(Game game, Screen screen, Engine engine) : base(game, screen)
        {
            Engine = engine;
            m_confirmations = new Dictionary<Player, Dictionary<RemotePlayer, Confirmation>>();
            SubscribeCommunicator();

        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            List<Player> confirmed = new List<Player>();

            foreach (Player deadPlayer in m_confirmations.Keys)
            {
                bool allConfirmed = true;
                foreach (var kvp in m_confirmations[deadPlayer])
                {
                    if(!kvp.Value.Confirmed)
                    {
                        allConfirmed = false;
                        if(gameTime.TotalGameTime > kvp.Value.LastNotification + MaxAckDelay)
                        {
                            Game.Communicator.MessagePlayer(kvp.Key, new NetworkMessage(MessageType.RealDeath, deadPlayer.PlayerID.ToString()));
                            kvp.Value.LastNotification = gameTime.TotalGameTime;
                        }
                    }
                }

                if (allConfirmed)
                {
                    confirmed.Add(deadPlayer);
                }
            }

            foreach (Player player in confirmed)
            {
                m_confirmations.Remove(player);
            }

        }

        public void NotifyRealDeath(Player p)
        {           
            Game.Communicator.MessageAll(new NetworkMessage(MessageType.RealDeath, p.PlayerID.ToString()));

            Dictionary<RemotePlayer, Confirmation> confirmations = new Dictionary<RemotePlayer, Confirmation>();

            foreach (RemotePlayer remotePlayer in Game.Communicator.Connections.Keys)
            {
                confirmations.Add(remotePlayer, new Confirmation(){Confirmed = false, LastNotification = Game.GameTime.TotalGameTime});
            }

            m_confirmations.Add(p, confirmations);
        }

        private void SubscribeCommunicator()
        {
            Game.Communicator.MessageReceived += OnMessageReceived;
        }

        private void UnsubscribeCommunicator()
        {
            Game.Communicator.MessageReceived -= OnMessageReceived;
        }

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            if (e.Message.Type == MessageType.AckDeath)
            {
                var ackplayer = Game.Communicator.GetPlayer(e.Message.Source);

                int id = int.Parse(e.Message.Content);

                var deadplayer = Engine.GetPlayer(id);

                m_confirmations[deadplayer][ackplayer].Confirmed = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeCommunicator();
            }
            base.Dispose(disposing);
        }
    }
}
