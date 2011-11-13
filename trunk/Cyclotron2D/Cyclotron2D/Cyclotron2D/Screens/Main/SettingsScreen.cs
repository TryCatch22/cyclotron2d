using System;
using System.Collections.Generic;
using Cyclotron2D.Helpers;
using Cyclotron2D.Mod;
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
        private LabelCheckBox m_drawGrid;
        private LabelTextBox m_playerName;


        private StretchPanel m_OptionsPanel;

        private CancelOk m_cancelOk;


        public SettingsScreen(Game game) : base(game, GameState.ChangingSettings)
        {

            m_drawGrid = new LabelCheckBox(game, this);
            m_OptionsPanel = new StretchPanel(game, this);
            m_gridSize = new LabelTextBox(game, this);
            m_cycleSpeed = new LabelTextBox(game, this);
            m_maxTailLength = new LabelTextBox(game, this);
            m_suicide = new LabelCheckBox(game, this);
            m_playerName = new LabelTextBox(game, this);

            m_OptionsPanel.AddItems(m_gridSize, m_cycleSpeed, m_maxTailLength, m_suicide, m_drawGrid, m_playerName);

          
        }

        public override void Initialize()
        {
            base.Initialize();
            
            m_gridSize.LabelText = Settings.Current.GridSize.ToString();
            m_cycleSpeed.LabelText = Settings.Current.CycleSpeed.ToString();
            m_maxTailLength.LabelText = Settings.Current.MaxTailLength.ToString();
            m_suicide.LabelText = Settings.Current.AllowSuicide.ToString();
            m_drawGrid.LabelText = Settings.Current.DrawGrid.ToString();
            m_playerName.LabelText = Settings.Current.PlayerName.ToString();


            m_cycleSpeed.Label.TextColor = Color.White;
            m_gridSize.Label.TextColor = Color.White;
            m_maxTailLength.Label.TextColor = Color.White;
            m_suicide.Label.TextColor = Color.White;
            m_drawGrid.Label.TextColor = Color.White;
            m_playerName.Label.TextColor = Color.White;

            var vp = GraphicsDevice.Viewport.Bounds;

            m_OptionsPanel.Rect = RectangleBuilder.Centered(vp, new Vector2(0.6f, 0.45f));

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
                m_gridSize.BoxText = Settings.Current.GridSize.Value.ToString();
                m_cycleSpeed.BoxText = Settings.Current.CycleSpeed.Value.ToString();
                m_maxTailLength.BoxText = Settings.Current.MaxTailLength.Value.ToString();
                m_suicide.IsChecked = Settings.Current.AllowSuicide.Value;
                m_drawGrid.IsChecked = Settings.Current.DrawGrid.Value;
                m_playerName.BoxText = Settings.Current.PlayerName.Value;
            }
        }

        private void ApplySettings()
        {


            try
            {
                Settings.Current.GridSize.TrySetValue(m_gridSize.BoxText);
                Settings.Current.CycleSpeed.TrySetValue(m_cycleSpeed.BoxText);
                Settings.Current.MaxTailLength.TrySetValue(m_maxTailLength.BoxText);
                Settings.Current.AllowSuicide.TrySetValue(m_suicide.IsChecked);
                Settings.Current.DrawGrid.TrySetValue(m_drawGrid.IsChecked);
                Settings.Current.PlayerName.TrySetValue(m_playerName.BoxText);
                
                Game.ChangeState(GameState.MainMenu);
            }
            catch (InvalidValueException)
            {
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
