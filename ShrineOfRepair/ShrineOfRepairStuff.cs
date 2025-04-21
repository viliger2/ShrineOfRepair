using R2API;
using RoR2;
using RoR2.Hologram;
using RoR2.UI;
using ShrineOfRepair.Behaviours;
using ShrineOfRepair.Modules;
using ShrineOfRepairRewrite.Behaviours;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;
using static ShrineOfRepair.Modules.ShrineOfRepairConfigManager;

[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]
namespace ShrineOfRepair
{
    public class ShrineOfRepairStuff
    {
        public static InteractableSpawnCard normalShrineSpawnCard; 

        public enum CostTypes
        {
            Gold,
            VoidCoin,
            LunarCoin
        }

        public static void Hooks()
        {
            RoR2.SceneDirector.onPostPopulateSceneServer += SceneDirector_onPostPopulateSceneServer;
            RoR2.Stage.onServerStageBegin += Stage_onServerStageBegin;
            On.RoR2.PurchaseInteraction.GetInteractability += PurchaseInteraction_GetInteractability;
            On.RoR2.PickupPickerController.GetInteractability += PickupPickerController_GetInteractability;
        }

        private static Interactability PickupPickerController_GetInteractability(On.RoR2.PickupPickerController.orig_GetInteractability orig, PickupPickerController self, Interactor activator)
        {
            if (self.name.Contains("ShrineRepair") && activator)
            {
                var body = activator.GetComponent<CharacterBody>();
                if (body && body.master)
                {
                    bool isShrineAvailable = false;

                    foreach (KeyValuePair<ItemIndex, ItemIndex> pairedItems in ShrineOfRepairDictionary.RepairItemsDictionary)
                        if (body.inventory.GetItemCount(pairedItems.Key) > 0)
                            isShrineAvailable = true;
                    foreach (KeyValuePair<EquipmentIndex, EquipmentIndex> pairedItems in ShrineOfRepairDictionary.RepairEquipmentsDictionary)
                        if (body.equipmentSlot.equipmentIndex == pairedItems.Key)
                            isShrineAvailable = true;
                    if (!isShrineAvailable) { return Interactability.ConditionsNotMet; }
                }
            }
            return orig(self, activator);
        }

        private static Interactability PurchaseInteraction_GetInteractability(On.RoR2.PurchaseInteraction.orig_GetInteractability orig, PurchaseInteraction self, Interactor activator)
        {
            if (self.displayNameToken == "INTERACTABLE_SHRINE_REPAIR_NAME" && activator)
            {
                var body = activator.GetComponent<CharacterBody>();
                if (body && body.master)
                {
                    bool isShrineAvailable = false;

                    foreach (KeyValuePair<ItemIndex, ItemIndex> pairedItems in ShrineOfRepairDictionary.RepairItemsDictionary)
                    {
                        if (body.inventory.GetItemCount(pairedItems.Key) > 0)
                        {
                            isShrineAvailable = true;
                        }
                    }
                    if (ShrineOfRepairDictionary.RepairEquipmentsDictionary.ContainsKey(body.equipmentSlot.equipmentIndex)) isShrineAvailable = true;
                    if (!isShrineAvailable) { return Interactability.ConditionsNotMet; }
                }
            }
            return orig(self, activator);
        }

        private static void Stage_onServerStageBegin(Stage stage)
        {
            if (stage.sceneDef.cachedName != "bazaar")
            {
                return;
            }

            if (SpawnInBazaar.Value)
            {
                SpawnShrine(BazaarPosition.Value, BazaarAngle.Value);
            }
        }

        private static void SceneDirector_onPostPopulateSceneServer(SceneDirector obj)
        {
            if (SpawnInMoon.Value)
            {
                if (SceneCatalog.GetSceneDefForCurrentScene() == SceneCatalog.GetSceneDefFromSceneName("moon")) SpawnShrine(MoonPosition.Value, MoonAngle.Value);
                else if (SceneCatalog.GetSceneDefForCurrentScene() == SceneCatalog.GetSceneDefFromSceneName("moon2")) SpawnShrine(Moon2Position.Value, Moon2Angle.Value);
            }
        }

        protected static void SpawnShrine(Vector3 position, Vector3 angle)
        {
            if (!NetworkServer.active) return;
            DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule();
            directorPlacementRule.placementMode = 0;
            GameObject spawnedInstance = normalShrineSpawnCard.DoSpawn(position, Quaternion.identity, new DirectorSpawnRequest(normalShrineSpawnCard, directorPlacementRule, Run.instance.runRNG)).spawnedInstance;
            spawnedInstance.transform.eulerAngles = angle;
            NetworkServer.Spawn(spawnedInstance);
        }

        public GameObject CreateShrineOfRepairPickerPrefab(GameObject prefab)
        {
            prefab = CreateShrineOfRepairPrefabInternal(prefab);

            var iconTransform = prefab.transform.Find("Symbol");

            var uiPromptController = prefab.AddComponent<NetworkUIPromptController>();

            var manager = prefab.AddComponent<ShrineOfRepairPickerManager>();
            manager.costType = Modules.ShrineOfRepairConfigManager.PickerInteractionCurrencyType.Value;
            manager.iconTransform = iconTransform;

            var pickerController = prefab.AddComponent<PickupPickerController>();
            pickerController.panelPrefab = CreateRepairPickerPanel();
            pickerController.onPickupSelected = new PickupPickerController.PickupIndexUnityEvent();
            pickerController.onPickupSelected.AddPersistentListener(manager.HandleSelection);
            pickerController.onServerInteractionBegin = new GenericInteraction.InteractorUnityEvent();
            pickerController.onServerInteractionBegin.AddPersistentListener(manager.HandleInteraction);
            pickerController.cutoffDistance = 10f;
            pickerController.contextString = "INTERACTABLE_SHRINE_REPAIR_CONTEXT";

            manager.pickerController = pickerController;

            var inspectDef = ScriptableObject.CreateInstance<InspectDef>();
            (inspectDef as ScriptableObject).name = "idShrineOfRepair";
            inspectDef.Info = new RoR2.UI.InspectInfo
            {
                Visual = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texShrineIconOutlined.png").WaitForCompletion(),
                TitleToken = "INTERACTABLE_SHRINE_REPAIR_NAME",
                DescriptionToken = "INTERACTABLE_SHRINE_REPAIR_DESCRIPTION_PICKER",
                FlavorToken = "INTERACTABLE_SHRINE_REPAIR_LORE",
                TitleColor = UnityEngine.Color.white,
                isConsumedItem = false
            };
            prefab.AddComponent<GenericInspectInfoProvider>().InspectInfo = inspectDef;

            return prefab;
        }

        public GameObject CreateShrineOfRepairPurchasePrefab(GameObject prefab)
        {
            prefab = CreateShrineOfRepairPrefabInternal(prefab);

            var hologramController = prefab.AddComponent<HologramProjector>();
            hologramController.hologramPivot = prefab.transform.Find("HologramPivot");
            hologramController.displayDistance = 10;
            hologramController.disableHologramRotation = false;

            // provides purchase interaction for what we do with a shrine
            var purchaseInteraction = prefab.AddComponent<PurchaseInteraction>();
            purchaseInteraction.displayNameToken = "INTERACTABLE_SHRINE_REPAIR_NAME";
            purchaseInteraction.contextToken = "INTERACTABLE_SHRINE_REPAIR_CONTEXT";
            purchaseInteraction.costType = GetCostTypeFromConfig(PurchaseInteractionCurrencyType.Value);
            purchaseInteraction.automaticallyScaleCostWithDifficulty = PurchaseInteractionCurrencyType.Value == CostTypes.Gold && PurchaseInteractionGoldUseDefaultScaling.Value;
            purchaseInteraction.cost = GetCostValueFromConfig(PurchaseInteractionCurrencyType.Value);
            purchaseInteraction.available = true;
            purchaseInteraction.setUnavailableOnTeleporterActivated = false; // it controlls that it becomes completely unavailable, not that you can't interact with it if it is outside of teleporter range
            purchaseInteraction.isShrine = true;
            purchaseInteraction.isGoldShrine = false;

            var shrineManager = prefab.AddComponent<ShrineOfRepairPurchaseManager>();
            shrineManager.PurchaseInteraction = purchaseInteraction;
            shrineManager.ScalingModifier = PurchaseInteractionGoldScalingModifier.Value;
            shrineManager.UseDefaultScaling = PurchaseInteractionGoldUseDefaultScaling.Value;

            var inspectDef = ScriptableObject.CreateInstance<InspectDef>();
            (inspectDef as ScriptableObject).name = "idShrineOfRepair";
            inspectDef.Info = new RoR2.UI.InspectInfo
            {
                Visual = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texShrineIconOutlined.png").WaitForCompletion(),
                TitleToken = "INTERACTABLE_SHRINE_REPAIR_NAME",
                DescriptionToken = "INTERACTABLE_SHRINE_REPAIR_DESCRIPTION_PURCHASE",
                FlavorToken = "INTERACTABLE_SHRINE_REPAIR_LORE",
                TitleColor = UnityEngine.Color.white,
                isConsumedItem = false
            };
            prefab.AddComponent<GenericInspectInfoProvider>().InspectInfo = inspectDef;

            return prefab;
        }

        private GameObject CreateRepairPickerPanel()
        {
            var prefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Scrapper/ScrapperPickerPanel.prefab").WaitForCompletion().InstantiateClone("RepairPickerPanel", false);

            var label = prefab.transform.Find("MainPanel/Juice/Label");
            if (label != null)
            {
                var text = label.GetComponent<LanguageTextMeshController>();
                if (text != null)
                {
                    text.token = "INTERACTABLE_SHRINE_REPAIR_PICKERPANEL_HEADER";
                }
            }

            var scrapperInfo = prefab.GetComponent<ScrapperInfoPanelHelper>();
            var repairInfo = prefab.AddComponent<RepairInforPanelHelper>();
            repairInfo.inspectPanelController = scrapperInfo.inspectPanelController;
            repairInfo.correspondingScrapImage = scrapperInfo.correspondingScrapImage;
            UnityEngine.Object.DestroyImmediate(scrapperInfo);

            var panel = prefab.GetComponent<PickupPickerPanel>();
            panel.pickupSelected.AddPersistentListener(repairInfo.ShowInfo);
            panel.pickupBaseContentReady.AddPersistentListener(repairInfo.AddQuantityToPickerButton);

            repairInfo.panel = panel;

            return prefab;
        }

        private GameObject CreateShrineOfRepairPrefabInternal(GameObject prefab)
        {
            var modelTransform = prefab.transform.Find("Base/mdlRepairShrine");
            var iconTransform = prefab.transform.Find("Symbol");

            prefab.AddComponent<NetworkIdentity>();

            var highlight = prefab.AddComponent<Highlight>();
            highlight.targetRenderer = modelTransform.GetComponent<Renderer>();
            highlight.strength = 1f;
            highlight.highlightColor = Highlight.HighlightColor.interactive;

            var modelLocator = prefab.AddComponent<ModelLocator>();
            modelLocator.modelTransform = modelTransform;
            modelLocator.modelBaseTransform = prefab.transform.Find("Base");
            modelLocator.autoUpdateModelTransform = true;
            modelLocator.dontDetatchFromParent = true;
            modelLocator.noCorpse = false;
            modelLocator.dontReleaseModelOnDeath = false;
            modelLocator.preserveModel = false;
            modelLocator.normalizeToFloor = false;

            prefab.AddComponent<GenericDisplayNameProvider>().displayToken = "INTERACTABLE_SHRINE_REPAIR_NAME";

            // model
            modelTransform.gameObject.AddComponent<EntityLocator>().entity = prefab;

            // icon
            iconTransform.gameObject.AddComponent<Billboard>();
            var iconMeshRenderer = iconTransform.gameObject.GetComponent<MeshRenderer>();

            if(!ContentProvider.MaterialCache.TryGetValue("matShrineRepairSymbolCopy", out var newMaterial))
            {
                newMaterial = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/ShrineBoss/matShrineBossSymbol.mat").WaitForCompletion());
                newMaterial.name = "matShrineRepairSymbolCopy";
                newMaterial.mainTexture = iconMeshRenderer.material.mainTexture;
                newMaterial.SetColor("_TintColor", iconMeshRenderer.material.GetColor("_TintColor"));

                ContentProvider.MaterialCache.Add(newMaterial.name, newMaterial);
            }
            iconMeshRenderer.material = newMaterial;

            var dither = prefab.AddComponent<DitherModel>();
            dither.bounds = modelTransform.gameObject.GetComponent<Collider>();
            dither.renderers = prefab.GetComponentsInChildren<Renderer>();

            prefab.RegisterNetworkPrefab();

            return prefab;
        }

        public DirectorAPI.DirectorCardHolder CreateCardHolder(GameObject prefab, string name)
        {
            InteractableSpawnCard interactableSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            (interactableSpawnCard as ScriptableObject).name = "isc" + name;
            interactableSpawnCard.prefab = prefab;
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

            var holderShrine1 = new DirectorAPI.DirectorCardHolder();
            holderShrine1.Card = directorCard;
            holderShrine1.InteractableCategory = DirectorCategory.Value;

            return holderShrine1;
        }

        public List<DirectorAPI.Stage> GetNormalStageList()
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

        public List<DirectorAPI.Stage> GetSnowyStageList()
        {
            List<DirectorAPI.Stage> stageList = new List<DirectorAPI.Stage>
            {
                DirectorAPI.Stage.SiphonedForest,
                DirectorAPI.Stage.RallypointDelta,

                DirectorAPI.Stage.RallypointDeltaSimulacrum
            };

            return stageList;

        }

        public List<DirectorAPI.Stage> GetSandyStageList()
        {
            List<DirectorAPI.Stage> stageList = new List<DirectorAPI.Stage>
            {
                DirectorAPI.Stage.AbandonedAqueduct,

                DirectorAPI.Stage.AbandonedAqueductSimulacrum
            };

            return stageList;

        }

        private CostTypeIndex GetCostTypeFromConfig(CostTypes currency)
        {
            switch (currency)
            {
                case CostTypes.LunarCoin:
                    if (PurchaseInteractionLunarCoinCost.Value == 0) return CostTypeIndex.None;
                    return CostTypeIndex.LunarCoin;
                case CostTypes.VoidCoin:
                    if (PurchaseInteractionVoidCoinCost.Value == 0) return CostTypeIndex.None;
                    return CostTypeIndex.VoidCoin;
                default:
                case CostTypes.Gold:
                    if (PurchaseInteractionGoldBaseCost.Value == 0) return CostTypeIndex.None;
                    return CostTypeIndex.Money;
            }
        }

        private int GetCostValueFromConfig(CostTypes currency)
        {
            switch (currency)
            {
                case CostTypes.LunarCoin:
                    return PurchaseInteractionLunarCoinCost.Value;
                case CostTypes.VoidCoin:
                    return PurchaseInteractionVoidCoinCost.Value;
                default:
                case CostTypes.Gold:
                    return PurchaseInteractionGoldBaseCost.Value;
            }
        }

#if DEBUG
        [ConCommand(commandName = "sor_give_void", flags = ConVarFlags.ExecuteOnServer)]
        private static void CCGiveVoidCoins(ConCommandArgs args)
        {
            if(args.Count == 0)
            {
                return;
            }

            if (!TextSerialization.TryParseInvariant(args[0], out int result))
            {
                return;
            }

            var localPlayers = LocalUserManager.readOnlyLocalUsersList;
            var localPlayer = localPlayers[0].cachedBody;

            if (localPlayer)
            {
                localPlayer.master.voidCoins += (uint)result;
            }
        }
#endif
    }
}
