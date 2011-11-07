using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Core.Players
{
    /// <summary>
    /// Remotely connected player, Events are generated from incomming messages
    /// </summary>
    public class RemotePlayer : Player
    {
        public RemotePlayer(Game game, Screen screen) : base(game, screen)
        {
        }
    }
}