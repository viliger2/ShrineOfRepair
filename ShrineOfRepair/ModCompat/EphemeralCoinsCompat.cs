using BubbetsItems;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShrineOfRepair.ModCompat
{
    internal class EphemeralCoinsCompat
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Varna.EphemeralCoins");
                }
                return (bool)_enabled;
            }
        }

        public static uint EphemeralCoinsCount(NetworkUser user)
        {
            if (EphemeralCoins.EphemeralCoins.instance)
            {
                return EphemeralCoins.EphemeralCoins.instance.getCoinsFromUser(user);
            }

            return 0;
        }
    }
}
