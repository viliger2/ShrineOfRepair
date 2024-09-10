using BepInEx;
using R2API;
using ShrineOfRepair.Modules.Interactables;
using System.Linq;
using System.Reflection;

namespace ShrineOfRepair.Modules
{
    [BepInPlugin("com.Viliger.ShrineOfRepair", "ShrineOfRepair", "1.4.4")]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInDependency(DirectorAPI.PluginGUID)]
    [BepInDependency("bubbet.bubbetsitems", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]

    public class ShrineOfRepairPlugin : BaseUnityPlugin
    {
        public static PluginInfo PInfo { get; private set; }

        internal static BepInEx.Logging.ManualLogSource MyLogger;

        private void Awake()
        {
            #if DEBUG == true
            On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
            #endif

            PInfo = Info;

            MyLogger = Logger;

            new ShrineofRepairAssets().Init();

            new ShrineOfRepairLanguages().Init();

            new ShrineOfRepairConfigManager().Init(Paths.ConfigPath);

            var InteractableTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(InteractableBase)));

            foreach (var interactableType in InteractableTypes)
            {
                InteractableBase interactable = (InteractableBase)System.Activator.CreateInstance(interactableType);
                interactable.Init();
                MyLogger.LogInfo($"Interactable: {interactable.InteractableLangToken} loaded.");
            }
        }
    }
}
