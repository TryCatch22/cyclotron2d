
namespace Cyclotron2D.Mod
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
        public RangedIntegerSetting GridSize { get; private set; }

        public RangedFloatSetting CycleSpeed { get; private set; }

        public RangedIntegerSetting MaxTailLength { get; private set; }

        public Setting<bool> AllowSuicide { get; set; }

        public Setting<bool> DrawGrid { get; set; }

        public Setting<string> PlayerName { get; set; }

        public Settings()
        {
            GridSize = new RangedIntegerSetting("Grid Size",5){MinValue = 2, MaxValue = 15};
            CycleSpeed = new RangedFloatSetting("Cycle Speed", 3) { MinValue = 1, MaxValue = 5 };
            MaxTailLength = new RangedIntegerSetting("Max Tail Length", 0) { MinValue = 0, MaxValue = 1500 };
            AllowSuicide = new Setting<bool>("Allow Suicide", false) {Validate = val => true };
            DrawGrid = new Setting<bool>("Draw Grid", true) { Validate = val => true };
            PlayerName = new Setting<string>("Player Name", "Player") { Validate = val => !string.IsNullOrWhiteSpace(val) }; 
        }

    }

   
}
