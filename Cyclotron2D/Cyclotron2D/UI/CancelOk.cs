using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cyclotron2D.Helpers;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Cyclotron2D.UI
{
    public class CancelOk :UIElement
    {

        public Action OnCancel { get; set; }

        public Action OnOk { get; set; }

        public string OkText
        {
            get { return OkButton.Text; }
            set { OkButton.Text = value; }
        }

        public string CancelText
        {
            get { return CancelButton.Text; }
            set { CancelButton.Text = value; }
        }

        private StretchPanel m_buttonsPanel;

        protected Button CancelButton { get; private set; }
        protected Button OkButton { get; private set; }

        public CancelOk(Game game, Screen screen) : base(game, screen)
        {
            OkButton = new Button(game, screen);
            CancelButton = new Button(game, screen);
            m_buttonsPanel = new StretchPanel(game, screen);

            Initialize();
        }

        private new void Initialize()
        {
            m_buttonsPanel.AddItems(CancelButton, OkButton);

            OnCancel = OnOk = () => { };

            OkButton.Text = "OK";
            CancelButton.Text = "Cancel";

            OkButton.Click += OnBtnOkClicked;
            CancelButton.Click += OnBtnCancelClicked;

            m_buttonsPanel.Orientation = Orientation.Horizontal;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            m_buttonsPanel.Rect = Rect;
        }

        protected override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);
            if (Game.InputState.IsNewKeyPress(Keys.Enter))
            {
                OnOk();
            }
            else if (Game.InputState.IsNewKeyPress(Keys.Escape))
            {
                OnCancel();
            }
        }

        private void OnBtnCancelClicked(object sender, EventArgs e)
        {
            OnCancel();
        }

        private void OnBtnOkClicked(object sender, EventArgs e)
        {
            OnOk();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (m_buttonsPanel.Visible)
            {
                m_buttonsPanel.Draw(gameTime);
            }
        }
    }
}
