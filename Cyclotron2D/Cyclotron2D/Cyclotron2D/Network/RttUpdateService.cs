using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Cyclotron2D.Components;
using Cyclotron2D.Helpers;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.State;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Network
{
    public class RttUpdateService : CyclotronComponent
    {




        #region Fields

        public static readonly TimeSpan DefaultUpdatePeriod = new TimeSpan(0, 0, 0, 1);

        private TimeSpan m_lastPingRound;

        private Dictionary<NetworkConnection, TimeSpan> m_pingOutTimes;

        #endregion

        #region Properties

        public TimeSpan UpdatePeriod { get; set; }


        #endregion

        #region Constructor

        public RttUpdateService(Game game)
            : base(game)
        {
            m_pingOutTimes = new Dictionary<NetworkConnection, TimeSpan>();
        }

        #endregion

        #region Public Methods

        public override void Initialize()
        {
            base.Initialize();
            SubscribeCommunicator();
            UpdatePeriod = DefaultUpdatePeriod;
        }


        public void Reset()
        {
            lock (m_pingOutTimes)
            {
                m_pingOutTimes = new Dictionary<NetworkConnection, TimeSpan>();
                foreach (NetworkConnection networkConnection in Game.Communicator.Connections.Values)
                {
                    m_pingOutTimes.Add(networkConnection, new TimeSpan(0));
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Game.IsState(GameState.PlayingAsHost | GameState.PlayingAsClient | GameState.GameLobbyHost | GameState.GameLobbyClient))
            {
                if (gameTime.TotalGameTime > m_lastPingRound + UpdatePeriod && m_pingOutTimes.Count > 0)
                {
                    lock (m_pingOutTimes)
                    {
                        foreach (var key in m_pingOutTimes.Keys.ToList())
                        {
                            m_pingOutTimes[key] = gameTime.TotalGameTime;
                        }
                    }
                    try
                    {
                        Game.Communicator.MessageAll(new NetworkMessage(MessageType.Ping, "out"));
                        m_lastPingRound = gameTime.TotalGameTime;
                    }
                    catch (SocketException )
                    {
                        //a player has probably disconnected.
                        //do nothing, the Communicator will inform us of this shortly and the connection will be removed from the list
                    }

                }
            }
        }


        #endregion


        #region Event Handlers

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            if (Game.IsState(GameState.PlayingAsHost | GameState.PlayingAsClient | GameState.GameLobbyHost | GameState.GameLobbyClient))
            {
                if (e.Message.Type == MessageType.Ping)
                {
                    if(e.Message.Content == "out")
                    {
                        Game.Communicator.MessagePlayer(Game.Communicator.GetPlayer(e.Message.Source), new NetworkMessage(MessageType.Ping, "in"));
                    }
                    else if (e.Message.Content == "in")
                    {
                        var connection = sender as NetworkConnection;
                        {
                            if (connection != null && m_pingOutTimes.ContainsKey(connection))
                            {
                                //Debug.Assert(m_pingOutTimes.ContainsKey(connection), "update too soon");
                                var newRtt = Game.GameTime.TotalGameTime - m_pingOutTimes[connection];
                                connection.RoundTripTime = (connection.RoundTripTime + newRtt).Div(2);

                                DebugMessages.AddLogOnly("Updated Rtt for " + Game.Communicator.GetPlayer(e.Message.Source) + " Rtt: " + connection.RoundTripTime);
                                
                            }
                        }
                    }
                }

            }
        }


        private void OnConnectionLost(object sender, ConnectionEventArgs e)
        {
            lock (m_pingOutTimes)
            {
                if(m_pingOutTimes.ContainsKey(e.Connection))
                {
                    m_pingOutTimes.Remove(e.Connection);
                }
            }
        }


        #endregion

        #region Private Methods
        #endregion

        #region Subscription

        private void SubscribeCommunicator()
        {
            Game.Communicator.MessageReceived += OnMessageReceived;
            Game.Communicator.ConnectionLost += OnConnectionLost;
        }


        private void UnsubscribeCommunicator()
        {
            Game.Communicator.MessageReceived -= OnMessageReceived;
            Game.Communicator.ConnectionLost -= OnConnectionLost;
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeCommunicator();
//                UnsubscribeGame();
            }
            base.Dispose(disposing);
        }

        #endregion

      
    }
}
