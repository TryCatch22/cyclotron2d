using System;
using Cyclotron2D.Helpers;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Cyclotron2D.Screens.Popup
{
    public class OkPopup : PopupScreen
    {
        protected TextElement Message { get; private set; }

        protected Button OkButton { get; private set; }

        public Action OnOkClicked { get; protected set; }

        public string MessageText { get { return Message.Text; } set { Message.Text = value; }}
       
        public string OkText { get { return OkButton.Text; } set { OkButton.Text = value; } }


        public OkPopup(Game game, MainScreen parent) : base(game, parent)
        {
            Rectangle vp = Game.GraphicsDevice.Viewport.Bounds;
            Rect = new Rectangle(vp.Width / 7, vp.Height / 5, vp.Width * 5 / 7, vp.Height / 2);
            Background = new Color(0, 0, 0, 230);
            OkButton = new Button(game, this);
            Message = new TextElement(game, this){TextColor = Color.White};
            OkButton.Click += OnOkBtnClicked;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            OkButton.Rect = RectangleBuilder.BottomRight(Rect, new Vector2(0.3f, 0.1f), new Point(5, 5));
            Message.Rect = RectangleBuilder.Centered(Rect, new Vector2(0.6f, 0.6f));
        }

        protected override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);
            if (Game.InputState.IsNewKeyPress(Keys.Enter))
            {
				if (OnOkClicked != null)
					OnOkClicked();
                Close();
            }
        }

        private void OnOkBtnClicked(object sender, EventArgs e)
        {
			if (OnOkClicked != null)
	            OnOkClicked();
            Close();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (OkButton.Visible)
                OkButton.Draw(gameTime);

            if (Message.Visible)
                Message.Draw(gameTime);
        }
    }
}