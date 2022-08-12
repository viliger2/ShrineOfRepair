using BepInEx;
using R2API;
using R2API.Utils;
using R2API.Networking;
using System.Reflection;
using System.Linq;
using ShrineOfRepair.Modules.Interactables;
using RoR2;
using System.Collections.Generic;

namespace ShrineOfRepair.Modules
{
	[BepInPlugin("com.Viliger.ShrineOfRepair", "ShrineOfRepair", "1.2.1")]
	[BepInDependency(R2API.R2API.PluginGUID)]
	[R2APISubmoduleDependency(nameof(LanguageAPI), nameof(PrefabAPI), nameof(NetworkingAPI), nameof(DirectorAPI))]

	public class ShrineOfRepairPlugin : BaseUnityPlugin
	{
		public static PluginInfo PInfo { get; private set; }

		internal static BepInEx.Logging.ManualLogSource MyLogger;

		private void Awake()
        {
			//#if DEBUG == true
			//On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
			//#endif

			PInfo = Info;

			MyLogger = this.Logger;

			new ShrineOfRepairLanguages().Init();

			new ShrineOfRepairConfigManager().Init(Paths.ConfigPath);

			var InteractableTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(InteractableBase)));

			foreach(var interactableType in InteractableTypes)
            {
				InteractableBase interactable = (InteractableBase)System.Activator.CreateInstance(interactableType);
				interactable.Init();
				MyLogger.LogInfo($"Interactable: {interactable.InteractableLangToken} loaded.");
            }
        }

	}
}
