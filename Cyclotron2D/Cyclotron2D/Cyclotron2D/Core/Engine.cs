using System;
using System.Collections.Generic;
using System.Linq;
using Cyclotron2D.Components;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Helpers;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Core
{
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

        private StartRandomizer starter;

        //these values are overwritten, except for colors
        private readonly List<CycleInfo> m_startConditions = new List<CycleInfo>
                                                        {
                                                            new CycleInfo(new Vector2(3, 3), Direction.Right, Color.Red), //1
                                                            new CycleInfo(new Vector2(30, 70), Direction.Up, Color.Yellow), //2
                                                            new CycleInfo(new Vector2(80, 28), Direction.Down, Color.Green), //3
                                                            new CycleInfo(new Vector2(120, 90), Direction.Left, Color.Teal), //4
                                                            new CycleInfo(new Vector2(150, 50), Direction.Up, Color.DarkOrange), //5
                                                            new CycleInfo(new Vector2(120, 12), Direction.Left, Color.WhiteSmoke), //6
                                                        };

        #endregion

        #region Properties

        public TimeSpan GameStart { get; set; }

        #endregion

        #region Constructor

        public Engine(Game game, Screen screen)
            : base(game, screen)
        {
            m_playerMap = new Dictionary<Player, Cycle>();
            starter = new StartRandomizer(game);

            m_grid = new Grid(Game, Screen, Game.GraphicsDevice.Viewport.Bounds.Size());
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
            if(i == 1)
            {
                winner.Winner = true;
                m_playerMap[winner].Enabled = false;
            }
        }

        public void StartGame(IEnumerable<Player> players)
        {
            RandomizeStart();
            int i = 0;
            foreach (var player in players)
            {
                Cycle c = new Cycle(Game, Screen, m_grid, m_startConditions[i], player);
                m_playerMap.Add(player, c);
                player.Initialize(c, ++i);
            }

            m_grid.Initialize(m_playerMap.Values);
            GameStart = Game.GameTime.TotalGameTime;
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
        }

        #endregion

        #region Private Methods

        private void RandomizeStart()
        {
            var values = starter.Randomize(6, m_grid.PixelsPerInterval);

            for (int i = 0; i < 6; i++)
            {
                m_startConditions[i].Direction = values[i].Dir;
                m_startConditions[i].GridPosition = values[i].Position;

            }
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