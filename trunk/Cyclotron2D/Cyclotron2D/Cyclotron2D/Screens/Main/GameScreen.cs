using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Cyclotron2D.Core;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Helpers;
using Cyclotron2D.Mod;
using Cyclotron2D.Network;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.State;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Screens.Main
{
    /// <summary>
    /// Screen that runs the actual game
    /// </summary>
    public class GameScreen : MainScreen
    {
        private Engine m_engine;

        private StartRandomizer m_startRandomizer;

        public List<Player> ActivePlayers { get { return m_engine.Players; } }

        private List<Player> m_lobbyPlayers;

        private bool isGameSetup;

        public bool UseUdp { get; set; }

        public Settings GameSettings { get; set; }


        public CollisionNotifier CollisionNotifier { get; private set; }

        public TimeSpan GameStartTime { get { return m_engine.GameStartTime; } }

		private TimeSpan gameScreenSwitch;

        public NetworkMessage SetupMessage { get; set; }

        private bool m_gameStarted;

        private DateTime m_startTimeUtc;

        public GameScreen(Game game)
            : base(game, GameState.PlayingAsClient | GameState.PlayingSolo | GameState.PlayingAsHost)
        {
            GameSettings = Settings.SinglePlayer;
            m_lobbyPlayers = new List<Player>();
            m_engine = new Engine(game, this);
            CollisionNotifier = new CollisionNotifier(game, this, m_engine);
            m_startRandomizer = new StartRandomizer(game);
            isGameSetup = false;
        }

        public Player GetPlayer(int id)
        {
            return m_engine.GetPlayer(id);
        }

        private TimeSpan m_lastReady;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (isGameSetup && ActivePlayers.Aggregate(true, (ready, player) => ready && player.Ready) && (Game.RttService.UpdatePeriod == RttUpdateService.DefaultUpdatePeriod))
            {
                if(DateTime.UtcNow > m_startTimeUtc)
                {
                    StartGame();
                }else if (DateTime.UtcNow + gameTime.ElapsedGameTime > m_startTimeUtc)
                {
                    Thread.Sleep(DateTime.UtcNow + gameTime.ElapsedGameTime - m_startTimeUtc);
                    StartGame();
                }
                
            }/*else if (isGameSetup && !m_gameStarted && Game.IsState(GameState.PlayingAsClient) && gameTime.TotalGameTime > m_lastReady + new TimeSpan(0, 0, 0, 0, 800))
            {
                //in case ready packet got dropped
                Game.Communicator.MessagePlayer(Game.Communicator.Host, new NetworkMessage(MessageType.Ready, ""));
                m_lastReady = gameTime.TotalGameTime;
            }*/

			if ((gameTime.TotalGameTime - gameScreenSwitch) > new TimeSpan(0, 0, 10) && !m_gameStarted)
			{
				Game.ChangeState(GameState.MainMenu);
			}

			if (Game.IsState(GameState.PlayingAsHost))
			{
				if (gameTime.TotalGameTime > setupSendTime + TimeSpan.FromMilliseconds(500) && confirmations != ActivePlayers.Count)
				{
					Game.Communicator.MessageAll(setupMsg);
					setupSendTime = gameTime.TotalGameTime;
				}
			}
        }
        


        #region Subscription

        private void SubscribeCommunicator()
        {
            Game.Communicator.MessageReceived += OnMessageReceived;
        }

        private void UnsubscribeCommunicator()
        {
            Game.Communicator.MessageReceived -= OnMessageReceived;
        }



        #endregion

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {

            switch (Game.State)
            {
                case GameState.PlayingAsHost:
                    {
                        if (e.Message.Type == MessageType.Ready)
                        {
                            Player player = m_engine.GetPlayer(e.Message.Source);
                            if (player != null)
                            {
                                player.Ready = true;
                            }

                            //if all players are ready 
                            if (ActivePlayers.Aggregate(true, (ready, p) => ready && p.Ready))
                            {

                                //if we are still in intensive rtt update mode then wait until we aer done before calculating and sending all ready.
                                while (Game.RttService.UpdatePeriod != RttUpdateService.DefaultUpdatePeriod) Thread.Yield();



                                TimeSpan baseDelay = new TimeSpan(0, 0, 0, 1);
                                TimeSpan maxRtt = Game.Communicator.MaximumRtt;

                                foreach (var kvp in Game.Communicator.Connections)
                                {
                                    string content = ((maxRtt - kvp.Value.RoundTripTime).Div(2) + baseDelay).ToString();
                                    Game.Communicator.MessagePlayer(kvp.Key, new NetworkMessage(MessageType.AllReady, content));
                                }

                                m_startTimeUtc = DateTime.UtcNow + maxRtt.Div(2) + baseDelay;

                            }

                        }
                        else if (e.Message.Type == MessageType.AckUdpSetup)
                        {
                            confirmations++;
                            if(confirmations == ActivePlayers.Count -1)
                            {
                                Game.Communicator.MessageAll(new NetworkMessage(MessageType.DoUdpSwitch, ""));
                                
                                
                                Thread.Sleep(TimeSpanExtention.Max(Game.Communicator.MaximumRtt, TimeSpan.FromMilliseconds(150)));

                                Game.Communicator.StopTcp();
                                Game.Communicator.StartUdp();

                                Thread.Sleep(Game.Communicator.MaximumRtt.Mult(2));

                                Game.Communicator.EndIgnoreDisconnect();
                                Game.RttService.Reset();
                                Game.RttService.Resume();

                                DebugMessages.Add("Accelerating Pings");
                                Game.RttService.UpdatePeriod = TimeSpanExtention.Max(RttUpdateService.DefaultUpdatePeriod.Div(5), new TimeSpan(0, 0, 0, 0, 100));
                                Game.RttService.TriggerPing();

                                new Thread(() =>
                                {
                                    Thread.Sleep(TimeSpanExtention.Max(RttUpdateService.DefaultUpdatePeriod.Mult(2), new TimeSpan(0, 0, 0, 0, 200)));

                                    Game.RttService.UpdatePeriod = RttUpdateService.DefaultUpdatePeriod;
                                    DebugMessages.Add("Deccelerating Pings");
                                }).Start();

                            }
                        }


                    }
                    break;
                case GameState.PlayingAsClient:
                    {
                        if (e.Message.Type == MessageType.AllReady)
                        {
                            foreach (var activePlayer in ActivePlayers)
                            {
                                activePlayer.Ready = true;
                            }
                            m_startTimeUtc = DateTime.UtcNow + TimeSpan.Parse(e.Message.Content);
                        }
                        else if (e.Message.Type == MessageType.DoUdpSwitch)
                        {

                            Game.Communicator.StopTcp();
                            Thread.Sleep(50);
                            Game.Communicator.StartUdp();

                            Thread.Sleep(Game.Communicator.MaximumRtt.Mult(2));

                            Game.Communicator.EndIgnoreDisconnect();

                            Game.RttService.Reset();
                            Game.RttService.Resume();
                            



                            Game.Communicator.MessagePlayer(Game.Communicator.Host, new NetworkMessage(MessageType.Ready, ""));
                            m_lastReady = Game.GameTime.TotalGameTime;
                        }
                    }
                    break;
            }



        }

        private void StartGame()
        {
            if (IsValidState && !m_gameStarted && ActivePlayers.Count > 0)
            {
                m_engine.StartGame();
                m_gameStarted = true;
            }
        }

        private int confirmations;

        private static int ComparePlayerID(Player x, Player y)
        {
            return x.PlayerID - y.PlayerID;
        }

        private TimeSpan setupSendTime = TimeSpan.MaxValue - TimeSpan.FromMilliseconds(500);
        private NetworkMessage setupMsg;

        private void SetupGame(List<Player> players)
        {
            Sorter.Sort(players, ComparePlayerID);

            if (m_engine != null)
            {
                m_engine.Dispose();
                CollisionNotifier.Dispose();
            }

            m_engine = new Engine(Game, this);
            CollisionNotifier = new CollisionNotifier(Game, this, m_engine);

            switch (Game.State)
            {
                case GameState.PlayingSolo:
                    {
                        m_startRandomizer.Randomize(players.Count(), m_engine.Grid.PixelsPerInterval);

                        m_engine.SetupGame(players, m_startRandomizer.StartConditions);
                        foreach (var activePlayer in ActivePlayers)
                        {
                            activePlayer.Ready = true;
                        }
                    }
                    break;
                case GameState.PlayingAsHost:
                    {
                        m_startRandomizer.Randomize(players.Count(), m_engine.Grid.PixelsPerInterval);
                        m_engine.SetupGame(players, m_startRandomizer.StartConditions);
                        MessageType type = MessageType.SetupGame;
                        string content = "";

                        foreach (StartCondition startCondition in m_startRandomizer.StartConditions)
                        {
                            content += (int) startCondition.Dir + " " + startCondition.Position + "\n";
                        }

                        if (UseUdp)
                        {
                            foreach (var kvp in Game.Communicator.Connections)
                            {
                                content += kvp.Key.PlayerID + " " + kvp.Value.RemoteEP + "\n";
                            }

                            type = MessageType.SetupGameUdp;
                        }
                       //game setup message
                        setupMsg = new NetworkMessage(type, content);
                        Game.Communicator.MessageAll(setupMsg);
                        setupSendTime = Game.GameTime.TotalGameTime;
                        Game.Communicator.StartIgnoreDisconnect();

                        Game.RttService.Pause();







                    }
                    break;
                case GameState.PlayingAsClient:
                    {
                        Debug.Assert(SetupMessage != null, "there should be a setup message ready for client play.");



                        UseUdp = SetupMessage.Type == MessageType.SetupGameUdp;
                        if(UseUdp)
                        {
                            DebugMessages.AddLogOnly("Joining Udp Game");
                            //start ignoring disconnects right away
                            Game.Communicator.StartIgnoreDisconnect();
                            Game.RttService.Pause();
                        }
                        else
                        {
                            DebugMessages.AddLogOnly("Joining Tcp Game");
                        }

                        var lines = SetupMessage.ContentLines;
                        var localEp = Game.Communicator.Connections[Game.Communicator.Host].LocalEP;
                        List<StartCondition> conditions = new List<StartCondition>();

                        for (int i = 0; i < lines.Count; i++)
                        {


                            int sep, sep2;
                            string direction;
                            string vector;
                            string ipString, portString;


                             if(i < players.Count || !UseUdp)
                             {
                                sep = lines[i].IndexOf(' ');
                                direction = lines[i].Substring(0, sep);
                                vector = lines[i].Substring(sep + 1);
                                conditions.Add(new StartCondition(Vector2Extention.FromString(vector), (Direction)int.Parse(direction)));
                             }
                             else if (UseUdp)
                             {
                                sep = lines[i].IndexOf(' ');
                                sep2 = lines[i].IndexOf(':');

                                int playerId = int.Parse(lines[i].Substring(0, sep));
                                ipString = lines[i].Substring(sep + 1, sep2 - (sep + 1));
                                portString = lines[i].Substring(sep2 + 1);

                                var ip = IPAddress.Parse(ipString);
                                int port = int.Parse(portString);

                                var player = m_engine.GetPlayer(playerId);

                                if(player is RemotePlayer)
                                {
                                    Game.Communicator.Add(player as RemotePlayer, new NetworkConnection(localEp, ip, port));
                                }
                               
                             }
                        }

                        m_engine.SetupGame(players, conditions);


                        Game.Communicator.MessagePlayer(Game.Communicator.Host, new NetworkMessage(MessageType.AckUdpSetup, ""));

                      

                    }
                    break;
                    
            }

            isGameSetup = true;

        }

        public void StopGame()
        {
            m_gameStarted = false;
            isGameSetup = false;
            m_lobbyPlayers.Clear();
            UnsubscribeCommunicator();
        }

        public override void Draw(GameTime gameTime)
        {
            if (m_engine != null && m_engine.Visible)
            {
                m_engine.Draw(gameTime);
            }
        }

        public void AddPlayer(Player player)
        {
            m_lobbyPlayers.Add(player);
        }


        public void RemovePlayer(Player player)
        {
            m_lobbyPlayers.Remove(player);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && m_engine != null)
            {
                m_engine.Dispose();
                m_engine = null;
                CollisionNotifier.Dispose();
                UnsubscribeCommunicator();
            }
            base.Dispose(disposing);
        }



        protected override void OnStateChanged(object sender, StateChangedEventArgs e)
        {
            base.OnStateChanged(sender, e);

            //game screen disabled if not in its own state
            Enabled = IsValidState;

            if (!Enabled) return;

            if (Game.IsState(GameState.PlayingAsClient | GameState.PlayingAsHost))
            {
                SubscribeCommunicator();
            }

            if (m_gameStarted)
                return;

            List<Player> players = null;

            if (e.NewState == GameState.PlayingSolo && e.OldState == GameState.MainMenu)
            {
                players = new List<Player>
                              {
                                  new LocalPlayer(Game, this),
                                  new AIPlayer(Game, this),
                                  new AIPlayer(Game, this),
                                  new AIPlayer(Game, this),
                                  new AIPlayer(Game, this),
                                  new AIPlayer(Game, this),
                              };
                int i = 0;
                foreach (var player in players)
                {
                    player.PlayerID = ++i;
                }

                GameSettings = Settings.SinglePlayer;

                SetupGame(players);
            }
            else if (Game.IsState(GameState.PlayingAsHost | GameState.PlayingAsClient))
            {
                //get players from network and then start game
                players = new List<Player>();
                players.AddRange(m_lobbyPlayers);
                GameSettings = Settings.Multiplayer;
                SetupGame(players);

				gameScreenSwitch = Game.GameTime.TotalGameTime;
				
            }
        }
    }
}