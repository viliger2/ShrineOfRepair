using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static ShrineOfRepair.Modules.ShrineofRepairAssets;
using static ShrineOfRepair.Modules.ShrineOfRepairConfigManager;

namespace ShrineOfRepair.Modules.Interactables
{
    public abstract class ShrineOfRepairBase<T> : InteractableBase<T> where T : ShrineOfRepairBase<T>
    {
        public override string InteractableLangToken => "SHRINE_REPAIR";

        internal abstract GameObject CreateInteractable(GameObject InteractableModel);

        public static Dictionary<ItemIndex, ItemIndex> RepairItemsDictionary = new Dictionary<ItemIndex, ItemIndex>();

        public static Dictionary<EquipmentIndex, EquipmentIndex> RepairEquipmentsDictionary = new Dictionary<EquipmentIndex, EquipmentIndex>();

        public static InteractableSpawnCard shrineSpawnCard;

        public static void FillRepairItemsDictionary()
        {
            if (RepairItemsDictionary.Count != 0 || RepairEquipmentsDictionary.Count != 0) return;

            // ItemIndex enums are index numbers from "Item & Equipment IDs and Names" page
            var itemIds = RepairList.Value.Split(',');
            foreach (var itemId in itemIds)
            {
                var kv = itemId.Split('-');
                if (kv.Length == 2)
                {
                    ItemIndex broken = ItemCatalog.FindItemIndex(kv[0].Trim());
                    ItemIndex tofix = ItemCatalog.FindItemIndex(kv[1].Trim());
                    if (broken != ItemIndex.None && tofix != ItemIndex.None)
                    {
                        RepairItemsDictionary.Add(broken, tofix);
                        continue;
                    }
                    EquipmentIndex broken2 = EquipmentCatalog.FindEquipmentIndex(kv[0].Trim());
                    EquipmentIndex tofix2 = EquipmentCatalog.FindEquipmentIndex(kv[1].Trim());
                    if (broken2 != EquipmentIndex.None && tofix2 != EquipmentIndex.None)
                        RepairEquipmentsDictionary.Add(broken2, tofix2);
                }
            }

            RepairItemsDictionary = ModExtension.FillDictionaryFromMods(RepairItemsDictionary);
            ShrineOfRepairPlugin.MyLogger.LogDebug("Items");
            foreach (var kv in RepairItemsDictionary) ShrineOfRepairPlugin.MyLogger.LogDebug(ItemCatalog.GetItemDef(kv.Key).name + " -> " + ItemCatalog.GetItemDef(kv.Value).name);
            ShrineOfRepairPlugin.MyLogger.LogDebug("Equipments");
            foreach (var kv in RepairEquipmentsDictionary) ShrineOfRepairPlugin.MyLogger.LogDebug(EquipmentCatalog.GetEquipmentDef(kv.Key).name + " -> " + EquipmentCatalog.GetEquipmentDef(kv.Value).name);
        }

        internal void CreateInteractables()
        {
            var normalModel = CreateInteractable(UseBadModel.Value ? MainBundle.LoadAsset<GameObject>("ShrineRepair.prefab") : MainBundle.LoadAsset<GameObject>("ShrineRepairGood.prefab"));
            var sandyModel = CreateInteractable(UseBadModel.Value ? MainBundle.LoadAsset<GameObject>("ShrineRepair.prefab") : MainBundle.LoadAsset<GameObject>("ShrineRepairGood_sandy.prefab"));
            var snowyModel = CreateInteractable(UseBadModel.Value ? MainBundle.LoadAsset<GameObject>("ShrineRepair.prefab") : MainBundle.LoadAsset<GameObject>("ShrineRepairGood_snowy.prefab"));

            shrineSpawnCard = CreateInteractableSpawnCard(normalModel, "iscShrineRepair", GetNormalStageList());
            CreateInteractableSpawnCard(sandyModel, "iscShrineRepairSandy", GetSandyStageList());
            CreateInteractableSpawnCard(snowyModel, "iscShrineRepairSnowy", GetSnowyStageList());
        }

        private InteractableSpawnCard CreateInteractableSpawnCard(GameObject InteractableModel, string name, List<DirectorAPI.Stage> stageList)
        {
            InteractableSpawnCard interactableSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            interactableSpawnCard.name = name;
            interactableSpawnCard.prefab = InteractableModel;
            interactableSpawnCard.sendOverNetwork = true;
            interactableSpawnCard.hullSize = HullClassification.Golem;
            interactableSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            interactableSpawnCard.requiredFlags = RoR2.Navigation.NodeFlags.None;
            interactableSpawnCard.forbiddenFlags = RoR2.Navigation.NodeFlags.NoShrineSpawn;
            interactableSpawnCard.directorCreditCost = DirectorCost.Value;
            interactableSpawnCard.occupyPosition = true;
            interactableSpawnCard.orientToFloor = false;
            interactableSpawnCard.skipSpawnWhenSacrificeArtifactEnabled = false;

            DirectorCard directorCard = new DirectorCard
            {
                selectionWeight = DirectorWeight.Value,
                spawnCard = interactableSpawnCard,
            };

            foreach (DirectorAPI.Stage stage in stageList)
            {
                DirectorAPI.Helpers.AddNewInteractableToStage(directorCard, DirectorCategory.Value, stage);
            }

            return interactableSpawnCard;
        }

        private List<DirectorAPI.Stage> GetNormalStageList()
        {
            List<DirectorAPI.Stage> stageList = new List<DirectorAPI.Stage>();

            stageList.Add(DirectorAPI.Stage.DistantRoost);
            stageList.Add(DirectorAPI.Stage.AbyssalDepths);
            stageList.Add(DirectorAPI.Stage.TitanicPlains);
            stageList.Add(DirectorAPI.Stage.SunderedGrove);
            stageList.Add(DirectorAPI.Stage.SirensCall);
            stageList.Add(DirectorAPI.Stage.SkyMeadow);
            stageList.Add(DirectorAPI.Stage.SulfurPools);
            stageList.Add(DirectorAPI.Stage.ScorchedAcres);
            stageList.Add(DirectorAPI.Stage.AphelianSanctuary);

            stageList.Add(DirectorAPI.Stage.AbyssalDepthsSimulacrum);
            stageList.Add(DirectorAPI.Stage.TitanicPlainsSimulacrum);
            stageList.Add(DirectorAPI.Stage.SkyMeadowSimulacrum);
            stageList.Add(DirectorAPI.Stage.AphelianSanctuarySimulacrum);

            return stageList;

        }

        private List<DirectorAPI.Stage> GetSnowyStageList()
        {
            List<DirectorAPI.Stage> stageList = new List<DirectorAPI.Stage>();

            stageList.Add(DirectorAPI.Stage.SiphonedForest);
            stageList.Add(DirectorAPI.Stage.RallypointDelta);

            stageList.Add(DirectorAPI.Stage.RallypointDeltaSimulacrum);

            return stageList;

        }

        private List<DirectorAPI.Stage> GetSandyStageList()
        {
            List<DirectorAPI.Stage> stageList = new List<DirectorAPI.Stage>();

            stageList.Add(DirectorAPI.Stage.AbandonedAqueduct);

            stageList.Add(DirectorAPI.Stage.AbandonedAqueductSimulacrum);

            return stageList;

        }

        protected void SpawnShrine(Vector3 position, Vector3 angle)
        {
            if (!NetworkServer.active) return;
            DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule();
            directorPlacementRule.placementMode = 0;
            GameObject spawnedInstance = shrineSpawnCard.DoSpawn(position, Quaternion.identity, new DirectorSpawnRequest(shrineSpawnCard, directorPlacementRule, Run.instance.runRNG)).spawnedInstance;
            spawnedInstance.transform.eulerAngles = angle;
            NetworkServer.Spawn(spawnedInstance);
        }

        protected virtual void Hooks()
        {
            if (SpawnInBazaar.Value) On.RoR2.BazaarController.Awake += (orig, self) =>
            {
                orig(self);
                //SpawnShrine(new Vector3(-139.5f, -25.5f, -19.9f), new Vector3(0f, 0f, 0f));
                SpawnShrine(BazaarPosition.Value, BazaarAngle.Value);
            };

            if (SpawnInMoon.Value) On.RoR2.Stage.Start += (orig, self) =>
            {
                orig(self);
                //if (SceneCatalog.GetSceneDefForCurrentScene() == SceneCatalog.GetSceneDefFromSceneName("moon")) SpawnShrine(new Vector3(749.4f, 253f, -244.3f), new Vector3(0f, 143.2f, 0f));
                //else if (SceneCatalog.GetSceneDefForCurrentScene() == SceneCatalog.GetSceneDefFromSceneName("moon2")) SpawnShrine(new Vector3(-3.9f, -150.6f, -331.2f), new Vector3(-70f, 164f, 0f));
                if (SceneCatalog.GetSceneDefForCurrentScene() == SceneCatalog.GetSceneDefFromSceneName("moon")) SpawnShrine(MoonPosition.Value, MoonAngle.Value);
                else if (SceneCatalog.GetSceneDefForCurrentScene() == SceneCatalog.GetSceneDefFromSceneName("moon2")) SpawnShrine(Moon2Position.Value, Moon2Angle.Value);
            };
        }
    }
}
