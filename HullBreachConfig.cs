using System;
using UnityEngine;

namespace HullBreach
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class config : MonoBehaviour
    {

        public static string SettingsConfigUrl = "GameData/HullBreach/settings.cfg";

        public static double MinorFlooding { get; set; }
        public static double SeriousFlooding { get; set; }
        public static double FatalFlooding { get; set; }
        public static double MinorDmg { get; set; }
        public static double SeriousDmg { get; set; }
        public static double FatalDmg { get; set; }

        public static bool ecDrain { get; set; }
        public static bool isHullBreachEnabled { get; set; }

        public ModuleHullBreach vesselHullBreach = null;
        public static config Instance;

        void Awake()
        {
            LoadConfig();
            Instance = this;
        }

        public static void LoadConfig()
        {
            try
            {
                Debug.Log("[HullBreach]: Loading settings.cfg ");

                ConfigNode fileNode = ConfigNode.Load(SettingsConfigUrl);
                if (!fileNode.HasNode("HullBreachSettings")) return;

                ConfigNode settings = fileNode.GetNode("HullBreachSettings");

                MinorFlooding = double.Parse(settings.GetValue("MinorFlooding"));
                SeriousFlooding = double.Parse(settings.GetValue("SeriousFlooding"));
                FatalFlooding = double.Parse(settings.GetValue("FatalFlooding"));
                MinorDmg = double.Parse(settings.GetValue("MinorDmg"));
                SeriousDmg = double.Parse(settings.GetValue("SeriousDmg"));
                FatalDmg = double.Parse(settings.GetValue("FatalDmg"));

                ecDrain = bool.Parse(settings.GetValue("ecDrain"));
                isHullBreachEnabled = bool.Parse(settings.GetValue("isHullBreachEnabled"));

            }
            catch (Exception ex)
            {
                Debug.Log("[HullBreach]: Failed to load settings config:" + ex.Message);
            }
        }

        public static void SaveConfig()
        {
            try
            {
                Debug.Log("[HullBreach]: Saving settings.cfg ==");
                ConfigNode fileNode = ConfigNode.Load(SettingsConfigUrl);

                if (!fileNode.HasNode("HullBreachSettings")) return;

                ConfigNode settings = fileNode.GetNode("HullBreachSettings");

                settings.SetValue("MinorFlooding", MinorFlooding);
                settings.SetValue("SeriousFlooding", SeriousFlooding);
                settings.SetValue("FatalFlooding", FatalFlooding);
                settings.SetValue("MinorDmg", MinorDmg);
                settings.SetValue("SeriousDmg", SeriousDmg);
                settings.SetValue("FatalDmg", FatalDmg);
                settings.SetValue("ecDrain", ecDrain);
                settings.SetValue("isHullBreachEnabled", isHullBreachEnabled);

                fileNode.Save(SettingsConfigUrl);
            }
            catch (Exception ex)
            {
                Debug.Log("[HullBreach]: Failed to save settings config:" + ex.Message); throw;
            }
        }

    }
}

