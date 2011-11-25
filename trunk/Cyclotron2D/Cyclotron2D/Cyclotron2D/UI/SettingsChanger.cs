using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cyclotron2D.Helpers;
using Cyclotron2D.Mod;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UI
{
    public class SettingsChanger : UIElement
    {

        public Settings Settings { get; private set; }

        private LabelTextBox m_gridSize;
        private LabelTextBox m_cycleSpeed;
        private LabelTextBox m_maxTailLength;
        private LabelCheckBox m_suicide;
        private LabelCheckBox m_drawGrid;
        private LabelCheckBox m_plasmaGrid;
        private LabelTextBox m_playerName;


        private StretchPanel m_optionsPanel;

        private Button m_okButton;

        public SettingsChanger(Game game, Screen screen) : base(game, screen)
        {
			int labelWidth = 300;
			TextAlign align = TextAlign.Left;
			m_drawGrid = new LabelCheckBox(game, screen) {LabelWidth = labelWidth, LabelAlign = align};
            m_plasmaGrid = new LabelCheckBox(game, screen) { LabelWidth = labelWidth, LabelAlign = align };

			m_gridSize = new LabelTextBox(game, screen) {LabelWidth = labelWidth, LabelAlign = align};
			m_cycleSpeed = new LabelTextBox(game, screen) {LabelWidth = labelWidth, LabelAlign = align};
			m_maxTailLength = new LabelTextBox(game, screen) {LabelWidth = labelWidth, LabelAlign = align};
			m_suicide = new LabelCheckBox(game, screen) {LabelWidth = labelWidth, LabelAlign = align};
			m_playerName = new LabelTextBox(game, screen) {LabelWidth = labelWidth, LabelAlign = align};

            m_optionsPanel = new StretchPanel(game, screen);

            m_optionsPanel.AddItems(m_gridSize, m_cycleSpeed, m_maxTailLength, m_suicide, m_drawGrid, m_plasmaGrid, m_playerName);

            m_okButton = new Button(game, screen);
        }

        public void LoadSettings(Settings settings)
        {
            Settings = settings;



            m_gridSize.LabelText = settings.GridSize.ToString();
            m_cycleSpeed.LabelText = settings.CycleSpeed.ToString();
            m_maxTailLength.LabelText = settings.MaxTailLength.ToString();
            m_suicide.LabelText = settings.AllowSuicide.ToString();
            m_drawGrid.LabelText = settings.DrawGrid.ToString();
            m_playerName.LabelText = settings.PlayerName.ToString();
            m_plasmaGrid.LabelText = settings.PlasmaGrid.ToString();


            m_gridSize.BoxText = Settings.GridSize.Value.ToString();
            m_cycleSpeed.BoxText = Settings.CycleSpeed.Value.ToString();
            m_maxTailLength.BoxText = Settings.MaxTailLength.Value.ToString();
            m_suicide.IsChecked = Settings.AllowSuicide.Value;
            m_drawGrid.IsChecked = Settings.DrawGrid.Value;
            m_playerName.BoxText = Settings.PlayerName.Value;
            m_plasmaGrid.IsChecked = Settings.PlasmaGrid.Value;
        }

        public override void Initialize()
        {
            base.Initialize();

            m_cycleSpeed.Label.TextColor = Color.White;
            m_gridSize.Label.TextColor = Color.White;
            m_maxTailLength.Label.TextColor = Color.White;
            m_suicide.Label.TextColor = Color.White;
            m_drawGrid.Label.TextColor = Color.White;
            m_playerName.Label.TextColor = Color.White;
            m_plasmaGrid.Label.TextColor = Color.White;


            m_okButton.Click += OnOkClicked;
            m_okButton.Text = "Apply";


           
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            m_optionsPanel.Rect = RectangleBuilder.Top(Rect, new Vector2(1f, 0.8f));
            m_okButton.Rect = RectangleBuilder.BottomRight(Rect, new Vector2(0.2f, 0.17f), new Point(2, 2));
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (m_optionsPanel.Visible)
            {
                m_optionsPanel.Draw(gameTime);
            }

            if (m_okButton.Visible)
            {
                m_okButton.Draw(gameTime);
            }
        }

        #region Events

        public event EventHandler<SettingsChangedEventArgs> SettingsChanged;

        private void InvokeSettingsChanged(SettingsChangedEventArgs e)
        {
            EventHandler<SettingsChangedEventArgs> handler = SettingsChanged;
            if (handler != null) handler(this, e);
        }

        #endregion

        private void OnOkClicked(object sender, EventArgs e)
        {
            try
            {
                Settings.GridSize.TrySetValue(m_gridSize.BoxText);
                Settings.CycleSpeed.TrySetValue(m_cycleSpeed.BoxText);
                Settings.MaxTailLength.TrySetValue(m_maxTailLength.BoxText);
                Settings.AllowSuicide.TrySetValue(m_suicide.IsChecked);
                Settings.DrawGrid.TrySetValue(m_drawGrid.IsChecked);
                Settings.PlayerName.TrySetValue(m_playerName.BoxText);
                Settings.PlasmaGrid.TrySetValue(m_plasmaGrid.IsChecked);
                InvokeSettingsChanged(new SettingsChangedEventArgs(true));
            }
            catch (InvalidValueException)
            {
                InvokeSettingsChanged(new SettingsChangedEventArgs(false));
            }
        }
    }


    public class SettingsChangedEventArgs : EventArgs
    {
        public bool IsValid { get; private set; }

        public SettingsChangedEventArgs(bool isValid)
        {
            IsValid = isValid;
        }
    }
}
