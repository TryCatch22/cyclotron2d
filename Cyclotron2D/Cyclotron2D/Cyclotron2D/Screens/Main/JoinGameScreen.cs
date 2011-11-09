using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UIElements;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Screens.Main
{
    public class JoinGameScreen : MainScreen
    {

        private TextBox m_hostIpBox;
        private TextElement m_label;
        private Button m_ok;

        public JoinGameScreen(Game game) : base(game, (int)GameState.JoiningGame)
        {
            m_hostIpBox = new TextBox(game, this);
            m_label = new TextElement(game, this);
            m_ok = new Button(game, this);

            SubscribeGame();
        }

        private void SubscribeGame()
        {
            Game.StateChanged += OnGameStateChanged;
        }

        private void UnsubscribeGame()
        {
            Game.StateChanged -= OnGameStateChanged;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Game != null)
            {
                UnsubscribeGame();
            }
            base.Dispose(disposing);
        }

        private void OnGameStateChanged(object sender, StateChangedEventArgs e)
        {
            if (IsValidState && !m_initialized)
            {
                InitializeUI();
            }
            
        }

        private bool m_initialized;

        public void InitializeUI()
        {
            var vp = Game.GraphicsDevice.Viewport.Bounds;



            //text box
            m_hostIpBox.Rect = new Rectangle(vp.Width*2/5, vp.Height*7/16,vp.Width/3,vp.Height/8);
            //label
            m_label.Text = "Host Ip Adress:";
            m_label.Rect = new Rectangle((int) (m_hostIpBox.Rect.X - Art.Font.MeasureString(m_label.Text).X -3),
                m_hostIpBox.Rect.Y, (int) (Art.Font.MeasureString(m_label.Text).X + 1), m_hostIpBox.Rect.Height);
            m_label.TextColor = Color.White;


            m_initialized = true;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (m_hostIpBox.Visible)
            {
                m_hostIpBox.Draw(gameTime);
            }

            if (m_label.Visible)
            {
                m_label.Draw(gameTime);
            }
        }
    }
}
