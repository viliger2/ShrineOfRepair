using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
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

            RepairItemsDictionary = ModExtension.FillItemDictionaryFromMods(RepairItemsDictionary);
            RepairItemsDictionary = ModExtension.FillDictionaryFromMods(RepairItemsDictionary);
            RepairEquipmentsDictionary = ModExtension.FillEquipmentDictionaryFromMods(RepairEquipmentsDictionary);
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
            List<DirectorAPI.Stage> stageList = new List<DirectorAPI.Stage>
            {
                DirectorAPI.Stage.DistantRoost,
                DirectorAPI.Stage.AbyssalDepths,
                DirectorAPI.Stage.TitanicPlains,
                DirectorAPI.Stage.SunderedGrove,
                DirectorAPI.Stage.SirensCall,
                DirectorAPI.Stage.SkyMeadow,
                DirectorAPI.Stage.SulfurPools,
                DirectorAPI.Stage.ScorchedAcres,
                DirectorAPI.Stage.AphelianSanctuary,

                DirectorAPI.Stage.AbyssalDepthsSimulacrum,
                DirectorAPI.Stage.TitanicPlainsSimulacrum,
                DirectorAPI.Stage.SkyMeadowSimulacrum,
                DirectorAPI.Stage.AphelianSanctuarySimulacrum
            };

            return stageList;

        }

        private List<DirectorAPI.Stage> GetSnowyStageList()
        {
            List<DirectorAPI.Stage> stageList = new List<DirectorAPI.Stage>
            {
                DirectorAPI.Stage.SiphonedForest,
                DirectorAPI.Stage.RallypointDelta,

                DirectorAPI.Stage.RallypointDeltaSimulacrum
            };

            return stageList;

        }

        private List<DirectorAPI.Stage> GetSandyStageList()
        {
            List<DirectorAPI.Stage> stageList = new List<DirectorAPI.Stage>
            {
                DirectorAPI.Stage.AbandonedAqueduct,

                DirectorAPI.Stage.AbandonedAqueductSimulacrum
            };

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
            On.RoR2.BazaarController.Awake += (orig, self) =>
            {
                orig(self);
                if (SpawnInBazaar.Value)
                {
                    //SpawnShrine(new Vector3(-139.5f, -25.5f, -19.9f), new Vector3(0f, 0f, 0f));
                    SpawnShrine(BazaarPosition.Value, BazaarAngle.Value);
                }
            };

            RoR2.SceneDirector.onPostPopulateSceneServer += SceneDirector_onPostPopulateSceneServer;
            On.RoR2.UI.ScrapperInfoPanelHelper.ShowInfo += ScrapperInfoPanelHelper_ShowInfo;
        }

        private void ScrapperInfoPanelHelper_ShowInfo(On.RoR2.UI.ScrapperInfoPanelHelper.orig_ShowInfo orig, RoR2.UI.ScrapperInfoPanelHelper self, RoR2.UI.MPButton button, PickupDef pickupDef)
        {
            orig(self, button, pickupDef);
            var parent = self.gameObject.transform;
            if (!parent.name.Contains("ShrineRepair"))
            {
                while (parent.parent)
                {
                    parent = parent.parent;
                    if (parent.name.Contains("ShrineRepair"))
                    {
                        break;
                    };
                }
            }
            if (parent.name.Contains("ShrineRepair"))
            {
                var itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
                if(itemDef != default(ItemDef))
                {
                    if (RepairItemsDictionary.TryGetValue(itemDef.itemIndex, out var repairedItemIndex))
                    {
                        var repairedItemDef = ItemCatalog.GetItemDef(repairedItemIndex);
                        if (repairedItemDef != default(ItemDef))
                        {
                            self.correspondingScrapImage.sprite = repairedItemDef.pickupIconSprite;
                        }
                    }
                } else
                {
                    var equipmentDef = EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex);
                    if (equipmentDef != default(EquipmentDef))
                    {
                        if (RepairEquipmentsDictionary.TryGetValue(equipmentDef.equipmentIndex, out var repairedEquipmentIndex))
                        {
                            var repairedEquipmentDef = EquipmentCatalog.GetEquipmentDef(repairedEquipmentIndex);
                            if (repairedEquipmentDef != default(EquipmentDef))
                            {
                                self.correspondingScrapImage.sprite = repairedEquipmentDef.pickupIconSprite;
                            }
                        }
                    }
                }
            }
        }

        private void SceneDirector_onPostPopulateSceneServer(SceneDirector obj)
        {
            if (SpawnInMoon.Value)
            {
                if (SceneCatalog.GetSceneDefForCurrentScene() == SceneCatalog.GetSceneDefFromSceneName("moon")) SpawnShrine(MoonPosition.Value, MoonAngle.Value);
                else if (SceneCatalog.GetSceneDefForCurrentScene() == SceneCatalog.GetSceneDefFromSceneName("moon2")) SpawnShrine(Moon2Position.Value, Moon2Angle.Value);
            }
        }
    }
}
