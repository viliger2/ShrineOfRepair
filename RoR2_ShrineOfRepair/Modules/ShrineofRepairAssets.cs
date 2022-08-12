using UnityEngine;
using System.IO;

namespace ShrineOfRepair.Modules
{
    public class ShrineofRepairAssets
    {
        public static AssetBundle MainBundle;

        public const string BundleName = "shrinerepair";

        public const string BundleFolder = "Assets";

        public static string AssetBundlePath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(ShrineOfRepairPlugin.PInfo.Location), BundleFolder, BundleName);
            }
        }

        public void Init()
        {
            MainBundle = AssetBundle.LoadFromFile(AssetBundlePath);
        }
    }
}
