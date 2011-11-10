using System;
using Cyclotron2D.Helpers;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Screens.Main
{
    public class SettingsScreen : MainScreen
    {
        private LabelTextBox m_gridSize;
        private LabelTextBox m_cycleSpeed;
        private StretchPanel m_OptionsPanel;

        private CancelOk m_cancelOk;

        public SettingsScreen(Game game) : base(game, GameState.ChangingSettings)
        {
            m_OptionsPanel = new StretchPanel(game, this);
            m_gridSize = new LabelTextBox(game, this);
            m_cycleSpeed = new LabelTextBox(game, this);

            m_OptionsPanel.AddItems(m_gridSize, m_cycleSpeed);

          
        }

        public override void Initialize()
        {
            base.Initialize();
            m_gridSize.LabelText = "Grid Size (2-15)";
            m_cycleSpeed.LabelText = "Cycle Speed (1-5)";


            m_cycleSpeed.LabelElement.TextColor = Color.White;
            m_gridSize.LabelElement.TextColor = Color.White;


            var vp = GraphicsDevice.Viewport.Bounds;

            m_OptionsPanel.Rect = RectangleBuilder.Centered(vp, new Vector2(0.8f, 0.3f));

              m_cancelOk = new CancelOk(Game, this)
                             {
                                 OnOk = ApplySettings,
                                 OnCancel = () => Game.ChangeState(GameState.MainMenu),
                                 OkText = "Apply",
                                 Rect = RectangleBuilder.BottomRight(vp, new Vector2(0.4f, 0.1f), new Point(5,5))
                             };
        }

        protected override void OnStateChanged(object sender, StateChangedEventArgs e)
        {
            base.OnStateChanged(sender, e);
            if (IsValidState)
            {
                m_gridSize.BoxText = Settings.Current.GridSize.ToString();
                m_cycleSpeed.BoxText = Settings.Current.CycleSpeed.ToString();
            }
        }

        private bool ValidateInt(string s, int min, int max, out int result)
        {
            bool ok = false;
            if (int.TryParse(s, out result))
            {
                ok = result >= min && result <= max;
                
            }
            return ok;
        }

        private bool ValidateFloat(string s, float min, float max, out float result)
        {
            bool ok = false;
            if (float.TryParse(s, out result))
            {
                ok = result >= min && result <= max;

            }
            return ok;
        }

        private void ApplySettings()
        {
            int gsize;
            float speed;
            bool ok = true;
            
            if(ok &= ValidateInt(m_gridSize.BoxText, 2, 15, out gsize))
            {
                Settings.Current.GridSize = gsize;
            }
            else
            {
                m_gridSize.BoxText = Settings.Current.GridSize.ToString();
            }
            

            if (ok &= ValidateFloat(m_cycleSpeed.BoxText, 2, 5, out speed))
            {
                Settings.Current.CycleSpeed = speed;
            }
            else
            {
                m_cycleSpeed.BoxText = Settings.Current.CycleSpeed.ToString();
            }

            if (ok)
            {
                Game.ChangeState(GameState.MainMenu);
            }
            

        }


        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (m_OptionsPanel.Visible)
            {
                m_OptionsPanel.Draw(gameTime);
            }

            if (m_cancelOk.Visible)
            {
                m_cancelOk.Draw(gameTime);
            }
        }
    }
}
