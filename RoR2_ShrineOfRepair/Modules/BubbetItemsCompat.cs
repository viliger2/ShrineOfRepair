using BubbetsItems;
using RoR2;

namespace ShrineOfRepair.Modules
{
    public static class BubbetItemsCompat
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("bubbet.bubbetsitems");
                }
                return (bool)_enabled;
            }
        }

        public static bool IsVoidLunar(ItemTier tier)
        {
            return tier == BubbetsItemsPlugin.VoidLunarTier.tier;
        }
    }
}
