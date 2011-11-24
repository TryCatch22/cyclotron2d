using System;

namespace Cyclotron2D.Mod
{
    public class InvalidValueException : Exception
    {
        public InvalidValueException(string message)
            : base(message)
        {

        }
    }
}
