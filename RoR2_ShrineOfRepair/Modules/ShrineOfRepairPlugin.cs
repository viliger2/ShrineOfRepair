using BepInEx;
using R2API;
using R2API.Utils;
using R2API.Networking;
using System.Reflection;
using System.Linq;
using ShrineOfRepair.Modules.Interactables;
using RoR2;

namespace ShrineOfRepair.Modules
{
	[BepInPlugin("com.Viliger.ShrineOfRepair", "ShrineOfRepair", "1.1.1")]
	[BepInDependency(R2API.R2API.PluginGUID)]
	[R2APISubmoduleDependency(nameof(LanguageAPI), nameof(PrefabAPI), nameof(NetworkingAPI), nameof(DirectorAPI))]

	public class ShrineOfRepairPlugin : BaseUnityPlugin
	{
		public static PluginInfo PInfo { get; private set; }

		internal static BepInEx.Logging.ManualLogSource MyLogger; 

		private void Awake()
        {
			PInfo = Info;

			MyLogger = this.Logger;

			new ShrineOfRepairLanguages().Init();

			var InteractableTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(InteractableBase)));

			foreach(var interactableType in InteractableTypes)
            {
				InteractableBase interactable = (InteractableBase)System.Activator.CreateInstance(interactableType);
				interactable.Init(Config);
				MyLogger.LogInfo($"Interactable: {interactable.InteractableLangToken} loaded.");
            }
        }

	}
}
