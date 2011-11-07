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


            using (Cyclotron game = new Cyclotron())
            {
                game.Run();
            }
        }
    }
#endif
}