using Microsoft.Xna.Framework;

namespace Cyclotron2D.Components
{
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
                Game.Components.Remove(this);
            }
            base.Dispose(disposing);
        }
    }

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
                Game.Components.Remove(this);
            }
            base.Dispose(disposing);
        }
    }
}