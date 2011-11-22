using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cyclotron2D.Core.Players;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UI
{
    public class PlayerPanel : UIElement
    {
        private FixedPanel m_playerPanel;
        private List<PlayerView> m_playerViews;

        public PlayerPanel(Game game, Screen screen) : base(game, screen)
        {
            m_playerViews = new List<PlayerView>();

            m_playerPanel = new FixedPanel(game, screen){SizeRatio = 1/6f};
        }

        public void Initialize(List<Player> players)
        {

            foreach (var player in players)
            {
                m_playerViews.Add(new PlayerView(Game, Screen) { Player = player});
            }

            m_playerPanel.AddItems(m_playerViews.ToArray());
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            m_playerPanel.Rect = Rect;
        }

        public void AddPlayer(Player player)
        {
            m_playerViews.Add(new PlayerView(Game, Screen) { Player = player, TextColor =  Color.White});
            m_playerPanel.AddItems(m_playerViews.Last());
        }

        public void RemovePlayer(Player player)
        {
            PlayerView view = (from playerView in m_playerViews where playerView.Player == player select playerView).FirstOrDefault() ;
            if (view != null)
            {
                m_playerViews.Remove(view);
                m_playerPanel.RemoveItem(view);
            }

        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (m_playerPanel.Visible)
            {
                m_playerPanel.Draw(gameTime);
            }
        }


    }
}
