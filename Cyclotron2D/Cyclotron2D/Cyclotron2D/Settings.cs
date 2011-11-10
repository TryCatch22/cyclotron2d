
namespace Cyclotron2D
{
    public class Settings
    {
        public static Settings Default { get; set; }

        public static Settings Current { get; set; }

        static Settings()
        {
            Default = new Settings();
            Current = new Settings();
        }

        /// <summary>
        /// pixels per interval
        /// </summary>
        public int GridSize { get; set; }

        public float CycleSpeed { get; set; }

        public Settings()
        {
            GridSize = 5;
            CycleSpeed = 2;
        }

    }
}
