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

        #region Fields

        private List<byte> m_udpSetupConfirmations;

        private Engine m_engine;

        private StartRandomizer m_startRandomizer;
        private List<Player> m_lobbyPlayers;

        private bool isGameSetup;
       
        private NetworkMessage setupMsg;
      
		private TimeSpan gameScreenSwitch; 
        
        private bool m_gameStarted;

        private DateTime m_startTimeUtc;

        #endregion

        public List<Player> ActivePlayers { get { return m_engine.Players; } }


        public bool UseUdp { get; set; }

        public Settings GameSettings { get; set; }


        public TimeSpan GameStartTime { get { return m_engine.GameStartTime; } }


        public NetworkMessage SetupMessage { get; set; }


        public GameScreen(Game game)
            : base(game, GameState.PlayingAsClient | GameState.PlayingSolo | GameState.PlayingAsHost)
        {
            GameSettings = Settings.SinglePlayer;
            m_lobbyPlayers = new List<Player>();
            m_engine = new Engine(game, this);
            m_startRandomizer = new StartRandomizer(game);
            m_startTimeUtc = DateTime.MaxValue;
            isGameSetup = false;
            gameScreenSwitch = TimeSpan.MaxValue - new TimeSpan(0, 0, 10);
            m_udpSetupConfirmations = new List<byte>();
        }

        public Player GetPlayer(int id)
        {
            return m_engine.GetPlayer(id);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (isGameSetup && ActivePlayers.Aggregate(true, (ready, player) => ready && player.Ready) && (Game.RttService.UpdatePeriod == RttUpdateService.DefaultUpdatePeriod))
            {

                //send the allready message only once
                if(m_startTimeUtc == DateTime.MaxValue)
                {
                    SendAllReady();
                }

                //if game is ready to start, gogo
                if(DateTime.UtcNow > m_startTimeUtc)
                {
                    StartGame();

                }//if game is almost ready to start sleep the difference.
                else if (DateTime.UtcNow + gameTime.ElapsedGameTime > m_startTimeUtc)
                {
                    Thread.Sleep(m_startTimeUtc - DateTime.UtcNow);
                    StartGame();
                }
                
            }
            //timeout to get back to main menu if nothing happens after 10 seconds
			if (gameTime.TotalGameTime > gameScreenSwitch + new TimeSpan(0, 0, 10) && !m_gameStarted)
			{
                DebugMessages.Add("Setup Timeout!");
				Game.ChangeState(GameState.MainMenu);
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



        private void SendAllReady()
        {
            TimeSpan baseDelay = new TimeSpan(0, 0, 0, 1);
            TimeSpan maxRtt = Game.Communicator.MaximumRtt;

            foreach (var kvp in Game.Communicator.Connections)
            {
                string content = ((maxRtt - kvp.Value.RoundTripTime).Div(2) + baseDelay).ToString("G");
                Game.ReliableUdpSender.SendReliable(kvp.Key, new NetworkMessage(MessageType.AllReady, content));
            }

            m_startTimeUtc = DateTime.UtcNow + maxRtt.Div(2) + baseDelay;
        }

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

                        }
                        else if (e.Message.Type == MessageType.UdpReady)
                        {
                            if(!m_udpSetupConfirmations.Contains(e.Message.Source))
                            {
                                m_udpSetupConfirmations.Add(e.Message.Source);
                            }
                            if(m_udpSetupConfirmations.Count == ActivePlayers.Count -1)
                            {

                                AcceleratePings();

                                Game.ReliableUdpSender.SendReliableAll(new NetworkMessage(MessageType.StopTcp, ""));

                                Thread.Sleep(15);
                                Game.Communicator.StopTcp();

                                Thread.Sleep(Game.Communicator.MaximumRtt.Mult(2));
                                Game.Communicator.EndIgnoreDisconnect();


                                new Thread(() =>
                                { 
                                    //more ping time
                                    Thread.Sleep(TimeSpan.FromSeconds(1.5));
                                    
                                    DebugMessages.AddLogOnly("Decelarating pings");
                                    Game.RttService.UpdatePeriod = RttUpdateService.DefaultUpdatePeriod;
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
                        else if (e.Message.Type == MessageType.StopTcp)
                        {
                            Game.Communicator.StopTcp();

                            Thread.Sleep(Game.Communicator.MaximumRtt.Mult(2));

                            Game.Communicator.EndIgnoreDisconnect();

                            Game.RttService.Reset();

                            Game.ReliableUdpSender.SendReliable(Game.Communicator.Host, new NetworkMessage(MessageType.Ready, ""));
                           

                        }
                    }
                    break;
            }



        }


        private void AcceleratePings()
        {
            DebugMessages.Add("Accelerating Pings");
            Game.RttService.UpdatePeriod = TimeSpan.FromMilliseconds(300);
            Game.RttService.Reset();
        }

        private void StartGame()
        {
            if (IsValidState && !m_gameStarted && ActivePlayers.Count > 0)
            {
                m_engine.StartGame();
                m_gameStarted = true;
            }
        }



        private static int ComparePlayerID(Player x, Player y)
        {
            return x.PlayerID - y.PlayerID;
        }



        private void Cleanup()
        {
            StopGame();

            m_engine.Dispose();
            m_engine = null;

            isGameSetup = false;
            gameScreenSwitch = TimeSpan.MaxValue - new TimeSpan(0, 0, 10);

            m_lobbyPlayers.Clear();
            m_udpSetupConfirmations = new List<byte>();

            setupMsg = null;
            m_gameStarted = false;

            GameSettings = Settings.SinglePlayer;
            m_startTimeUtc = DateTime.MaxValue;
        }

        private void SetupGame(List<Player> players)
        {
            Sorter.Sort(players, ComparePlayerID);

            if (m_engine != null)
            {
                m_engine.Dispose();
            }

            m_engine = new Engine(Game, this);

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
                                var ep = kvp.Value.RemoteEP as IPEndPoint;
                                EndPoint newEp = new IPEndPoint(ep.Address, NetworkConnection.UDP_GAME_PORT);
                                content += kvp.Key.PlayerID + " " + newEp + "\n";
                            }

                            type = MessageType.SetupGameUdp;
                        }
                       //game setup message
                        setupMsg = new NetworkMessage(type, content);

                       // Game.ReliableUdpSender.Initialize(Game.Communicator.Connections.Keys.ToList());
                      //  m_pendingSetupConfirmations = Game.ReliableUdpSender.SendReliableAll(setupMsg);
                        Game.Communicator.MessageAll(setupMsg, 3 * ActivePlayers.Count);

                        Thread.Sleep(15);

                        Game.Communicator.StartIgnoreDisconnect();

                        Game.RttService.Pause();

                        Game.Communicator.StartUdp();

                     
                    }
                    break;
                case GameState.PlayingAsClient:
                    {
                        Debug.Assert(SetupMessage != null, "there should be a setup message ready for client play.");



                        UseUdp = SetupMessage.Type == MessageType.SetupGameUdp;
                        if(UseUdp)
                        {
                            DebugMessages.AddLogOnly("Joining Udp Game");
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
                                conditions.Add(new StartCondition(Vector2Extension.FromString(vector), (Direction)int.Parse(direction)));
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

                                var player = (from p in players where p.PlayerID == playerId select p).FirstOrDefault();

                                if(player is RemotePlayer)
                                {
                                    var ep = localEp as IPEndPoint;
                                    var newEp = new IPEndPoint(ep.Address, NetworkConnection.UDP_GAME_PORT);


                                    Game.Communicator.Add(player as RemotePlayer, new NetworkConnection(newEp, new IPEndPoint(ip, port)));
                                }
                               
                             }
                        }

                        Game.RttService.Reset();

                        m_engine.SetupGame(players, conditions);

                        Game.Communicator.StartUdp();

                        Game.ReliableUdpSender.SendReliable(Game.Communicator.Host, new NetworkMessage(MessageType.UdpReady, ""));
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

            if (e.NewState == GameState.PlayingSolo && e.OldState == GameState.MainMenu)
            {
                List<Player> players = new List<Player>
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
                GameSettings = Settings.Multiplayer;
                SetupGame(m_lobbyPlayers);
                //for timeout
				gameScreenSwitch = Game.GameTime.TotalGameTime;
				
            }
        }

        protected override void OnLeavingValidState()
        {
            base.OnLeavingValidState();
            Cleanup();
        }
    }
}