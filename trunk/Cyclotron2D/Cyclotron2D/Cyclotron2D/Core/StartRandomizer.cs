using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cyclotron2D.Components;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Core
{
    public class StartRandomizer : CyclotronComponent
    {
        private Random m_rand;

        public struct StartCond
        {
            /// <summary>
            /// In Grid coordinates.
            /// </summary>
            public Vector2 Position;

            public Direction Dir;

            public StartCond(Vector2 pos, Direction dir)
            {
                Position = pos;
                Dir = dir;
            }
        }


        public StartRandomizer(Game game) : base(game)
        {
            m_rand = new Random((int)DateTime.Now.Ticks);
        }

        public List<StartCond> Randomize(int count, int gridRatio)
        {
            var list = new List<StartCond>();
            int i = 0;
          
            int dx = (Game.GraphicsDevice.Viewport.Bounds.Width / gridRatio) / 3;
            int dy = (Game.GraphicsDevice.Viewport.Bounds.Height / gridRatio) / 2;
            List<int> p = new List<int>();
            for (int j = 0; j < count; j++)
            {
                p.Add(j);
            }

            while(i < count)
            {
                int zone = p[m_rand.Next(0, p.Count)];
                p.Remove(zone);

                int a = (int) Math.Floor((double)(zone)/3);
                int b = zone%3;

                int x = m_rand.Next(b * dx + 3, (b + 1) * dx -3);
                int y = m_rand.Next(a * dy + 3, (a + 1) * dy -3);

                var dirs = GetDirections(zone);
               
                list.Add(new StartCond(new Vector2(x, y), dirs[m_rand.Next(0, dirs.Length)]));
                i++;
            }

            return list;
        }

        private Direction[] GetDirections(int zone)
        {
            switch (zone)
            {
                case 0:
                    return  new[] { Direction.Right, Direction.Down };
                case 1:
                    return new[] { Direction.Down, Direction.Left, Direction.Right };
                case 2:
                    return new[] { Direction.Left, Direction.Down };
                case 3:
                    return new []{Direction.Up, Direction.Right};
                case 4:
                    return new []{Direction.Up, Direction.Left, Direction.Right};
                default:
                    return new []{Direction.Up, Direction.Left};
            }

        }
    }
}
