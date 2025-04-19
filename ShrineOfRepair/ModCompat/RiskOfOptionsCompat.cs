using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RoR2;
using UnityEngine;

namespace ShrineOfRepair.ModCompat
{
    public static class RiskOfOptionsCompat
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
                }
                return (bool)_enabled;
            }
        }

        public static void SetDescription()
        {
            ModSettingsManager.SetModDescription("Shrine to repair your broken items.", "com.Viliger.ShrineOfRepair", "ShrineOfRepair");
        }

        public static void SetIcon(Sprite icon)
        {
            ModSettingsManager.SetModIcon(icon, "com.Viliger.ShrineOfRepair", "ShrineOfRepair");
        }

        public static void CreateNewOption(ConfigEntry<float> entry)
        {
            ModSettingsManager.AddOption(new StepSliderOption(entry, new StepSliderConfig() { min = 0, max = 5, increment = 0.01f } ));
        }

        public static void CreateNewOption(ConfigEntry<bool> entry)
        {
            ModSettingsManager.AddOption(new CheckBoxOption(entry));
        }

        public static void CreateNewOption(ConfigEntry<int> entry, int min = 0, int max = 200)
        {
            ModSettingsManager.AddOption(new IntSliderOption(entry, new IntSliderConfig() { min = min, max = max }));
        }
    }
}