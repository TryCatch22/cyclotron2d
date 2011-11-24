using Microsoft.Xna.Framework;

namespace Cyclotron2D.Components
{
    /// <summary>
    /// Base Class for all our non drawable Game components, don't forget dispose 
    /// </summary>
    public abstract class CyclotronComponent : GameComponent
    {
        protected CyclotronComponent(Game game) : base(game)
        {
            Game.Components.Add(this);
            UpdateOrder = 100; // default value 
        }

        protected new Cyclotron Game { get { return base.Game as Cyclotron; } }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Enabled = false;
                Game.Components.Remove(this);
            }
            base.Dispose(disposing);
        }
    }


    /// <summary>
    /// Base Class for all our drawable Game components, don't forget dispose 
    /// </summary>
    public abstract class DrawableCyclotronComponent : DrawableGameComponent
    {
        protected DrawableCyclotronComponent(Game game) : base(game)
        {
            Game.Components.Add(this);
            UpdateOrder = 100; // default value
        }

        protected new Cyclotron Game { get { return base.Game as Cyclotron; } }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Enabled = Visible = false;
                Game.Components.Remove(this);
            }
            base.Dispose(disposing);
        }
    }
}