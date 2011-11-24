using System;
using System.Collections.Generic;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Helpers;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UI
{
    public class EndGameTable : UIElement
    {
        private StretchPanel m_playerPanel;
        private List<PlayerView> m_playerViews;
        private TextElement m_playersHeader;

        private StretchPanel m_lifetimesPanel;
        private List<TextElement> m_lifetimes;
        private TextElement m_lifetimeHeader;


        public List<Player> Players { get; private set; }

        public EndGameTable(Game game, Screen screen) : base(game, screen)
        {
            m_playerViews = new List<PlayerView>();
            m_lifetimes = new List<TextElement>();

            m_playerPanel = new StretchPanel(game, screen);
            m_playersHeader = new TextElement(game, screen) {Text = "Players", Background = new Color(210, 210, 210)};
            m_lifetimesPanel = new StretchPanel(game, screen);
            m_lifetimeHeader = new TextElement(game, screen) {Text = "Time", Background = new Color(210, 210, 210)};



        }

        public void Initialize(List<Player> players)
        {
            Players = players;

            foreach (var player in Players)
            {
                m_playerViews.Add(new PlayerView(Game, Screen) { Player = player, Background = new Color(160, 160, 160)});
                m_lifetimes.Add(new TextElement(Game, Screen) { Background = new Color(160, 160, 160) });
            }

            m_playerPanel.AddItems(m_playersHeader);
            m_playerPanel.AddItems(m_playerViews.ToArray());

            m_lifetimesPanel.AddItems(m_lifetimeHeader);
            m_lifetimesPanel.AddItems(m_lifetimes.ToArray());

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            SortPlayers();

            for (int i = 0; i < m_lifetimes.Count; i++)
            {
                m_lifetimes[i].Text = m_playerViews[i].Player.SurvivalTime.ToString(@"m\:ss\.ff");     
            }

            m_playerPanel.Rect = new Rectangle(Rect.X, Rect.Y, Rect.Width * 4/5, Rect.Height);
            m_lifetimesPanel.Rect = new Rectangle(Rect.X + Rect.Width*4/5, Rect.Y, Rect.Width*1/5, Rect.Height);

            

        }

        private void SortPlayers()
        {
            m_playerPanel.RemoveItem(m_playerViews.ToArray());

            Sorter.Sort(m_playerViews, Compare);
            m_playerPanel.AddItems(m_playerViews.ToArray());


        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (m_playerPanel.Visible)
            {
                m_playerPanel.Draw(gameTime);
            }
            if (m_lifetimesPanel.Visible)
            {
                m_lifetimesPanel.Draw(gameTime);
            }
        }


        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        public int Compare(PlayerView x, PlayerView y)
        {
            int diff =  (int) Math.Ceiling(y.Player.SurvivalTime.TotalMilliseconds - x.Player.SurvivalTime.TotalMilliseconds);
            return diff;
        }
    }
}
