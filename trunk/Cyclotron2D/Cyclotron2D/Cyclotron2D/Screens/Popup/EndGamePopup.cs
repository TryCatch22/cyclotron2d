using Cyclotron2D.Screens.Base;
using Cyclotron2D.Screens.Main;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Screens.Popup
{
    class EndGamePopup : OkPopup
    {

        public EndGamePopup(Game game, MainScreen parent, string text) : base(game, parent)
        {
            OnOkClicked = Quit;
            MessageText = text;
            OkText = "Quit";
            Message.TextScale = 3f;
        }


        private void Quit()
        {
            var gs = Parent as GameScreen;
            gs.StopGame();
            Game.ChangeState(GameState.MainMenu);
        }
    }
}
