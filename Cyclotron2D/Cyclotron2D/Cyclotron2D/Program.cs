using System;
using System.Diagnostics;

namespace Cyclotron2D
{
#if WINDOWS || XBOX
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            //yummy ui asserts!!
            var listener = Debug.Listeners[0] as DefaultTraceListener;
            if (listener != null)
            {
                listener.AssertUiEnabled = true;
            }

            try
            {
                    using (Cyclotron game = new Cyclotron())
                    {
                       game.Run();
                    }
            }
            catch (Exception e)
            {
                DebugMessages.AddLogOnly("Game Crashed: " + e.Message);
                DebugMessages.FlushLog();
                throw;
            }


            DebugMessages.FlushLog();
        }
    }
#endif
}