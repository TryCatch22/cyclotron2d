using System;
using Cyclotron2D.Helpers;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Screens.Main
{
    /// <summary>
    /// this class needs refactoring with a new class Setting to represent a single setting
    /// </summary>
    public class SettingsScreen : MainScreen
    {
        private LabelTextBox m_gridSize;
        private LabelTextBox m_cycleSpeed;
        private LabelTextBox m_maxTailLength;
        private LabelCheckBox m_suicide;


        private StretchPanel m_OptionsPanel;

        private CancelOk m_cancelOk;


        public SettingsScreen(Game game) : base(game, GameState.ChangingSettings)
        {
            m_OptionsPanel = new StretchPanel(game, this);
            m_gridSize = new LabelTextBox(game, this);
            m_cycleSpeed = new LabelTextBox(game, this);
            m_maxTailLength = new LabelTextBox(game, this);
            m_suicide = new LabelCheckBox(game, this);

            m_OptionsPanel.AddItems(m_gridSize, m_cycleSpeed, m_maxTailLength, m_suicide);

          
        }

        public override void Initialize()
        {
            base.Initialize();
            
            m_gridSize.LabelText = "Grid Size (2-15)";
            m_cycleSpeed.LabelText = "Cycle Speed (1-5)";
            m_maxTailLength.LabelText = "Max Tail Length (0-1500)";
            m_suicide.LabelText = "Allow Suicide";


            m_cycleSpeed.Label.TextColor = Color.White;
            m_gridSize.Label.TextColor = Color.White;
            m_maxTailLength.Label.TextColor = Color.White;
            m_suicide.Label.TextColor = Color.White;

            var vp = GraphicsDevice.Viewport.Bounds;

            m_OptionsPanel.Rect = RectangleBuilder.Centered(vp, new Vector2(0.8f, 0.35f));

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
                m_maxTailLength.BoxText = Settings.Current.MaxTailLength.ToString();
                m_suicide.IsChecked = Settings.Current.AllowSuicide;
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
            int value;
            float speed;
            bool ok = true;
            
            if(ok &= ValidateInt(m_gridSize.BoxText, 2, 15, out value))
            {
                Settings.Current.GridSize = value;
            }
            else
            {
                m_gridSize.BoxText = Settings.Current.GridSize.ToString();
            }

            if (ok &= ValidateInt(m_maxTailLength.BoxText, 0, 1500, out value))
            {
                Settings.Current.MaxTailLength = value;
            }
            else
            {
                m_maxTailLength.BoxText = Settings.Current.MaxTailLength.ToString();
            }
            

            if (ok &= ValidateFloat(m_cycleSpeed.BoxText, 2, 5, out speed))
            {
                Settings.Current.CycleSpeed = speed;
            }
            else
            {
                m_cycleSpeed.BoxText = Settings.Current.CycleSpeed.ToString();
            }


            Settings.Current.AllowSuicide = m_suicide.IsChecked;

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
