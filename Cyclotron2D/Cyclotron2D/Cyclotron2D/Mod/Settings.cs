﻿
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cyclotron2D.Mod
{
    public class Settings
    {

        private static string s_fileName = "cyclotron.settings";

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
            PlayerName = new Setting<string>("Player Name", "You") { Validate = val => !string.IsNullOrWhiteSpace(val) }; 
        }

        private static Dictionary<string, string> ReadSettings()
        {
            //Dictionary<string, string> dict = new Dictionary<string, string>();
            if (File.Exists(s_fileName))
            {
                byte[] buf;
                using(var fileStream = File.OpenRead(s_fileName))
                {
                      buf = new byte[fileStream.Length];

                      fileStream.Read(buf, 0, buf.Length);
                    fileStream.Close();
                }

                string s = Encoding.Default.GetString(buf);

                var lines = s.Split('\n').Where(line => !string.IsNullOrEmpty(line));

                return lines.Select(line => line.Split('=')).ToDictionary(kvp => kvp[0], kvp => kvp[1]);
            }

            return null;
        }


        public void WriteToFile()
        {
            string[] lines = new[]
                                 {
                                     GridSize.ToFileString(),
                                     CycleSpeed.ToFileString(),
                                     MaxTailLength.ToFileString(),
                                     AllowSuicide.ToFileString(),
                                     DrawGrid.ToFileString(),
                                     PlayerName.ToFileString()
                                 };


                using (var fileStream = File.OpenWrite(s_fileName))
                {
                    foreach (var line in lines)
                    {
                        byte[] buf = Encoding.Default.GetBytes(line + "\n");
                        fileStream.Write(buf,0, buf.Length);
                    }
                    fileStream.Close();
                }
        }

        public void LoadFromFile()
        {
            var dict = ReadSettings();

            if (dict != null)
            {
                try
                {
                    GridSize.TrySetValue(dict[GridSize.Name]);
                    MaxTailLength.TrySetValue(dict[MaxTailLength.Name]);
                    CycleSpeed.TrySetValue(dict[CycleSpeed.Name]);
                    PlayerName.TrySetValue(dict[PlayerName.Name]);

                    bool b;
                    if (bool.TryParse(dict[AllowSuicide.Name], out b))
                    {
                        AllowSuicide.TrySetValue(b);
                    }
                    if (bool.TryParse(dict[DrawGrid.Name], out b))
                    {
                        DrawGrid.TrySetValue(b);
                    }
                }
                catch (InvalidValueException)
                {
                    DebugMessages.Add("Failed to load Settings from file");
                }
               
                
            }
        }

    }

   
}