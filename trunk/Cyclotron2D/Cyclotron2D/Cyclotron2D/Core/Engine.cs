using System;
using System.Collections.Generic;
using System.Linq;
using Cyclotron2D.Components;
using Cyclotron2D.Core;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Helpers;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

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
    internal class Engine : DrawableScreenComponent
    {
        #region Fields

        public List<Player> Players { get { return m_playerMap.Keys.ToList(); } }

        private Grid m_grid;

        private Dictionary<Player, Cycle> m_playerMap;

        private StartRandomizer startRandomizer;

        private Countdown m_countdown;

        private TimeSpan m_delay;

        private TimeSpan m_gameStartDelay;  //GameStart + Delay for delaying the start

        #endregion

        #region Properties

        public TimeSpan GameStart { get; set; }
        
        public int Count;  // starting countdown number; determines delay     

        #endregion

        #region Constructor

        public Engine(Game game, Screen screen)
            : base(game, screen)
        {
            m_playerMap = new Dictionary<Player, Cycle>();
            startRandomizer = new StartRandomizer(game);

            m_grid = new Grid(Game, Screen, Game.GraphicsDevice.Viewport.Bounds.Size());
            m_countdown = new Countdown(Game, Screen);

            Count = 3;
            m_delay = new TimeSpan(0, 0, Count);
        }

        #endregion

        #region Public Methods

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);

            if (m_playerMap == null)
            {
                return;
            }

            int i = 0;
            Player winner = null;
            foreach (var player in m_playerMap.Keys)
            {
                var cycle = m_playerMap[player];
                if (cycle.Enabled)
                {
                    i++;
                    winner = player;
                }
            }
            if (i == 1)
            {
                winner.Winner = true;
                m_playerMap[winner].Enabled = false;
            }
        }

        public void StartGame(IEnumerable<Player> players)
        {
            GameStart = Game.GameTime.TotalGameTime;
            m_gameStartDelay = GameStart + m_delay;

            startRandomizer.Randomize(6, m_grid.PixelsPerInterval);
            int i = 0;
            foreach (var player in players)
            {
                Cycle c = new Cycle(Game, Screen, m_grid, startRandomizer.StartConditions[i++], player, m_gameStartDelay);
                m_playerMap.Add(player, c);
                player.Initialize(c);
            }

            m_grid.Initialize(m_playerMap.Values);
            m_countdown.Initialize(GameStart, Count);

            SubscribePlayers();
            SubscribeCycles();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (m_grid.Visible)
            {
                m_grid.Draw(gameTime);
            }

            foreach (var cycle in m_playerMap.Values.Where(cycle => cycle.Visible))
            {
                cycle.Draw(gameTime);
            }

            if (gameTime.TotalGameTime < m_gameStartDelay)
                m_countdown.Draw(gameTime);
        }

        #endregion

        #region Event Handlers

        private void OnPlayerDirectionChanged(object sender, DirectionChangeEventArgs e)
        {
            var player = sender as Player;
            if (player != null && m_playerMap[player].Enabled)
            {
                m_playerMap[player].TurnAt(e.Direction, e.Position);
            }
        }

        private void OnCycleCollided(object sender, EventArgs e)
        {
            var cycle = sender as Cycle;
            if (cycle != null)
            {
                cycle.Enabled = false;
            }
        }

        #endregion

        #region Event Subscribtion

        private void SubscribePlayers()
        {
            foreach (Player player in m_playerMap.Keys)
            {
                player.DirectionChange += OnPlayerDirectionChanged;
            }
        }


        private void UnsubscribePlayers()
        {
            foreach (Player player in m_playerMap.Keys)
            {
                player.DirectionChange -= OnPlayerDirectionChanged;
            }
        }

        private void SubscribeCycles()
        {
            foreach (var cycle in m_playerMap.Values)
            {
                cycle.Collided += OnCycleCollided;
            }
        }


        private void UnsubscribeCycles()
        {
            foreach (var cycle in m_playerMap.Values)
            {
                cycle.Collided += OnCycleCollided;
            }
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing && m_playerMap != null)
            {
                UnsubscribePlayers();
                UnsubscribeCycles();

                foreach (var cycle in m_playerMap.Values)
                {
                    cycle.Dispose();
                }

                foreach (var player in m_playerMap.Keys)
                {
                    player.Dispose();
                }


                m_grid.Dispose();

                m_playerMap = null;
                m_grid = null;
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}