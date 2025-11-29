using BepInEx;
using R2API;
using RoR2.ContentManagement;
using System;

[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]
namespace ShrineOfRepair
{
    [BepInPlugin(GUID, ModName, Version)]
    [BepInDependency("bubbet.bubbetsitems", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInDependency(DirectorAPI.PluginGUID)]
    public class ShrineOfRepairPlugin : BaseUnityPlugin
    {
        public const string Author = "Viliger";
        public const string ModName = "ShrineOfRepair";
        public const string Version = "2.1.0";
        public const string GUID = "com." + Author + "." + ModName;

        public const string LanguageFolder = "Language";

        void Awake()
        {
#if DEBUG == true
            On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
#endif
            Log.Init(Logger);

            new Modules.ShrineOfRepairConfigManager().Init(Paths.ConfigPath);

            ShrineOfRepairStuff.Hooks();
            Modules.ShrineOfRepairDictionary.Hooks();

            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
            RoR2.Language.collectLanguageRootFolders += Language_collectLanguageRootFolders;
        }

        private void Language_collectLanguageRootFolders(System.Collections.Generic.List<string> folders)
        {
            folders.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(base.Info.Location), LanguageFolder));
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new ContentProvider());
        }
    }
}
