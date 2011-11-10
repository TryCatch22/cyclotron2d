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

        private Grid m_grid;
        private Dictionary<Player, Cycle> m_playerMap;

        //we might want to randomize this at some point but for now its plenty
        private readonly List<CycleInfo> m_startConditions = new List<CycleInfo>
                                                        {
                                                            new CycleInfo(new Vector2(3, 3), Direction.Right, Color.Red),
                                                            new CycleInfo(new Vector2(30, 70), Direction.Up, Color.Yellow),
                                                            new CycleInfo(new Vector2(80, 28), Direction.Down, Color.Green),
                                                            new CycleInfo(new Vector2(120, 90), Direction.Left, Color.Purple),
                                                            new CycleInfo(new Vector2(150, 50), Direction.Up, Color.DarkOrange),
                                                            new CycleInfo(new Vector2(120, 12), Direction.Left, Color.WhiteSmoke),
                                                        };

        #endregion

        #region Constructor

        public Engine(Game game, Screen screen)
            : base(game, screen)
        {
            m_playerMap = new Dictionary<Player, Cycle>();

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
            int i = 0;
            foreach (var player in players)
            {
                Cycle c = new Cycle(Game, Screen, m_grid, m_startConditions[i], player);
                m_playerMap.Add(player, c);
                player.Initialize(c, ++i);
            }

            m_grid.Initialize(m_playerMap.Values);

            SubscribePlayers();
            SubscribeCycles();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            m_grid.Draw(gameTime);

            foreach (var cycle in m_playerMap.Values.Where(cycle => cycle.Visible))
            {
                cycle.Draw(gameTime);
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