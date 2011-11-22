using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.Helpers
{
    public static class RectangleBuilder
    {
        /// <summary>
        /// builds a rectangle centered on another and with size relative to the source rectangle
        /// </summary>
        /// <param name="source">source rectangle to build from</param>
        /// <param name="dimR">ratios for width and height from source to new</param>
        /// <returns>A Rectangle centered on source</returns>
        public static Rectangle Centered(Rectangle source, Vector2 dimR)
        {
            int w = (int) (source.Width*dimR.X), h = (int) (source.Height*dimR.Y);
            return new Rectangle(source.X + (source.Width - w)/2, source.Y + (source.Height - h)/2, w, h);
        }

        /// <summary>
        /// builds a rectangle inside another, centered at the top and with size relative to the source rectangle
        /// </summary>
        /// <param name="source">source rectangle to build from</param>
        /// <param name="dimR">ratios for width and height from source to new</param>
        /// <returns>A Rectangle at the top of the source</returns>
        public static Rectangle Top(Rectangle source, Vector2 dimR)
        {
            int w = (int)(source.Width * dimR.X), h = (int)(source.Height * dimR.Y);
            return new Rectangle(source.X + (source.Width - w) / 2, source.Y, w, h);
        }





        public static Rectangle BottomRight(Rectangle source, Vector2 dimR, Point padding)
        {
            int w = (int)(source.Width * dimR.X), h = (int)(source.Height * dimR.Y);
            return new Rectangle(   source.X + source.Width - w - padding.X, 
                                    source.Y + source.Height - h - padding.Y, w, h);
        }

        public static Rectangle TopLeft(Rectangle source, Vector2 dimR, Point padding)
        {
            int w = (int)(source.Width * dimR.X), h = (int)(source.Height * dimR.Y);
            return new Rectangle(source.X + padding.X, source.Y + padding.Y, w, h);
        }
    }
}
