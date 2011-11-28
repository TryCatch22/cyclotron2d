using System;
using System.Collections.Generic;
using System.Linq;
using Cyclotron2D.Components;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Helpers;
using Cyclotron2D.Network;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.Screens.Main;
using Cyclotron2D.State;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;
using Cyclotron2D.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Cyclotron2D.Core
{
    public struct ColorMap
    {
        public Color this[int i]
        {
            get
            {
                switch (i)
                {
                    case 1:
                        return Color.Red;
                    case 2:
                        return Color.Yellow;
                    case 3:
                        return Color.Green;
                    case 4:
                        return Color.Teal;
                    case 5:
                        return Color.DarkOrange;
                    case 6:
                        return Color.WhiteSmoke;
                    default:
                        return Color.Transparent;
                }
            }
        }
    }

    /// <summary>
    /// The Game Engine handles events from Player objects in order to simulate the game. It has no concept of the difference
    /// between a local and remote player.
    /// </summary>
    public class Engine : DrawableScreenComponent
    {
        #region Fields

        public Grid Grid { get; private set; }

        private Dictionary<Player, Cycle> m_playerCycleMap;

        private Countdown m_countdown;

        private bool m_started;

        public List<Animation> ExplosionAnimations { get; private set; }

        #endregion

        #region Properties

        public GameScreen GameScreen { get { return Screen as GameScreen;  } }

        public List<Player> Players { get { return m_playerCycleMap.Keys.ToList(); } }

        public TimeSpan GameStartTime { get; set; }

        public int Countdown { get; set; }  // starting countdown number; determines delay     

        #endregion

        #region Constructor

        public Engine(Game game, Screen screen)
            : base(game, screen)
        {
            GameStartTime = TimeSpan.MaxValue;

            Countdown = 3;
            m_playerCycleMap = new Dictionary<Player, Cycle>();
           
            var vp = Game.GraphicsDevice.Viewport.Bounds;
            Grid = new Grid(Game, Screen, vp.Size());

            m_countdown = new Countdown(Game, Screen) { Value = Countdown, TextColor = Color.Red, ScaleText = true, MaxScaleFactor =  3, StartTextScale =  0.5f };
            m_countdown.Rect = RectangleBuilder.Centered(vp, new Vector2(0.2f, 0.2f));

			ExplosionAnimations = new List<Animation>();
        }

        #endregion

        #region Public Methods

        public Player GetPlayer(int id)
        {
            return (from player in Players where player.PlayerID == id select player).FirstOrDefault();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (m_playerCycleMap == null)
            {
                return;
            }

            if(gameTime.TotalGameTime > GameStartTime && !m_started)
            {
                m_started = true;
                DebugMessages.AddLogOnly("Game Starting");
            }
            else if(!m_started)
            {
                //game has not started yet no need to check the rest of the stuff
                return;
            }

            int i = 0;
            Player winner = null;
            foreach (var player in m_playerCycleMap.Keys)
            {
                var cycle = m_playerCycleMap[player];
                if (!cycle.Dead)
                {
                    i++;
                    winner = player;
                }
            }
            if (i == 1)
            {
                winner.Winner = true;
                m_playerCycleMap[winner].Enabled = false;
            }

			ExplosionAnimations.RemoveAll(x => !x.Enabled);
        }

        public void StartGame()
        {
            GameStartTime = Game.GameTime.TotalGameTime + new TimeSpan(0, 0, 0, Countdown);

            foreach (var cycle in m_playerCycleMap.Values)
            {
                cycle.GameStart = GameStartTime;
            }
            DebugMessages.AddLogOnly("Countdown Starting");
            m_countdown.Start();
        }

        public void SetupGame(IEnumerable<Player> players, List<StartCondition> startConditions)
        {
           
            int i = 0;
            foreach (var player in players)
            {
                Cycle c = new Cycle(Game, Screen, Grid, startConditions[i++], player, ExplosionAnimations);
                m_playerCycleMap.Add(player, c);
                player.Initialize(c);
            }

            Grid.Initialize(m_playerCycleMap.Values);

            SubscribePlayers();
            SubscribeCycles();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            try
            {
                if (Grid.Visible)
                {
                    Grid.Draw(gameTime);
                }

                foreach (var cycle in m_playerCycleMap.Values.Where(cycle => cycle.Visible))
                {
                    cycle.Draw(gameTime);
                }

                if (m_countdown.Visible)
                {
                    m_countdown.Draw(gameTime);
                }

				foreach (var explosion in ExplosionAnimations.Where(ex => ex.Visible))
				{
			        explosion.Draw(gameTime);
				}
            }
            catch (NullReferenceException)
            {
                //engine was disposed on seperate thread in the middle of draw call. 
                //dont need to do anything, this object is about to no longer exist
            }
        }

        #endregion

        #region Event Handlers

        private void OnPlayerDirectionChanged(object sender, DirectionChangeEventArgs e)
        {
            if (Game.GameTime.TotalGameTime < GameStartTime) return;

            var player = sender as Player;
            var cycle = m_playerCycleMap[player];
            if (player != null && cycle.Enabled && cycle.Direction != e.Direction &&(cycle.NextTurnIntersection == null || cycle.NextTurnIntersection!=e.Position))
            {
                string content = (int)e.Direction + " " + e.Position;
                
                if(!GameScreen.UseUdp)
                {
                    if(Game.State == GameState.PlayingAsHost)
                    {
                        if (player is RemotePlayer)
                        {
                            Game.Communicator.MessageOtherPlayers(player as RemotePlayer, new NetworkMessage(MessageType.SignalTurn, content), (byte)player.PlayerID);
                        }
                        else
                        {
                            Game.Communicator.MessageAll(new NetworkMessage(MessageType.SignalTurn, content), (byte)player.PlayerID);
                        }
                    }
                    else if (Game.State == GameState.PlayingAsClient && player is LocalPlayer)
                    {
                        Game.Communicator.MessagePlayer(Game.Communicator.Host, new NetworkMessage(MessageType.SignalTurn, content));
                    }
                }



                m_playerCycleMap[player].TurnAt(e.Direction, e.Position);
            }
        }

        private void OnCycleCollided(object sender, CycleCollisionEventArgs e)
        {
            var cycle = sender as Cycle;
            if (cycle == null) return;

            if (Game.State == GameState.PlayingSolo)
            {
                cycle.Kill();
            }
            else if (e.Type == CollisionType.Player && e.AmbiguousCollision && Game.State == GameState.PlayingAsHost)
            {
                //currently the host will decide on ambiguous collisions.
                GameScreen.CollisionNotifier.NotifyRealDeath(e.Victim);
                cycle.Kill();
            }
            else if(e.Victim is RemotePlayer || e.AmbiguousCollision)
            {
                cycle.FeignDeath();
            }
          
        }

        #endregion

        #region Event Subscribtion

        private void SubscribePlayers()
        {
            foreach (Player player in m_playerCycleMap.Keys)
            {
                player.DirectionChange += OnPlayerDirectionChanged;
            }
        }


        private void UnsubscribePlayers()
        {
            foreach (Player player in m_playerCycleMap.Keys)
            {
                player.DirectionChange -= OnPlayerDirectionChanged;
            }
        }

        private void SubscribeCycles()
        {
            foreach (var cycle in m_playerCycleMap.Values)
            {
                cycle.Collided += OnCycleCollided;
            }
        }


        private void UnsubscribeCycles()
        {
            foreach (var cycle in m_playerCycleMap.Values)
            {
                cycle.Collided += OnCycleCollided;
            }
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing && m_playerCycleMap != null)
            {
                UnsubscribePlayers();
                UnsubscribeCycles();

                foreach (var cycle in m_playerCycleMap.Values)
                {
                    cycle.Dispose();
                }

                foreach (var player in m_playerCycleMap.Keys)
                {
                    player.Dispose();
                }


                Grid.Dispose();

                m_playerCycleMap = null;
                Grid = null;
            }

            base.Dispose(disposing);
        }

        #endregion


    }
}