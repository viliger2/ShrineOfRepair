using R2API;
using RoR2;
using RoR2.Hologram;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static ShrineOfRepair.Modules.ShrineofRepairAssets;
using static ShrineOfRepair.Modules.ShrineOfRepairConfigManager;
using static ShrineOfRepair.Modules.ShrineOfRepairPlugin;

namespace ShrineOfRepair.Modules.Interactables
{
    public class ShrineOfRepairPurchase : ShrineOfRepairBase<ShrineOfRepairPurchase>
    {

        public enum CostTypes
        {
            Gold,
            VoidCoin,
            LunarCoin
        }

        public override void Init()
        {
            if (!UsePickupPickerPanel.Value)
            {
                new ShrineofRepairAssets().Init();
                CreateInteractables();
                Hooks();
            }
        }

        internal override GameObject CreateInteractable(GameObject InteractableModel)
        {
            InteractableModel.AddComponent<NetworkIdentity>();

            // provides purchase interaction for what we do with a shrine
            var purchaseInteraction = InteractableModel.AddComponent<PurchaseInteraction>();
            purchaseInteraction.displayNameToken = $"INTERACTABLE_{InteractableLangToken}_NAME";
            purchaseInteraction.contextToken = $"INTERACTABLE_{InteractableLangToken}_CONTEXT";
            purchaseInteraction.costType = GetCostTypeFromConfig(PurchaseInteractionCurrencyType.Value);
            purchaseInteraction.automaticallyScaleCostWithDifficulty = PurchaseInteractionCurrencyType.Value == CostTypes.Gold && PurchaseInteractionGoldUseDefaultScaling.Value;
            purchaseInteraction.cost = GetCostValueFromConfig(PurchaseInteractionCurrencyType.Value);
            purchaseInteraction.available = true;
            purchaseInteraction.setUnavailableOnTeleporterActivated = false; // it controlls that it becomes completely unavailable, not that you can't interact with it if it is outside of teleporter range
            purchaseInteraction.isShrine = true;
            purchaseInteraction.isGoldShrine = false;

            // provides information and icon when you ping an object
            var pingInfoProvider = InteractableModel.AddComponent<PingInfoProvider>();
            pingInfoProvider.pingIconOverride = MainBundle.LoadAsset<Sprite>("Assets/RoR2/Base/Common/MiscIcons/texShrineIconOutlined.png");

            // provides a name
            var genericNameDisplay = InteractableModel.AddComponent<GenericDisplayNameProvider>();
            genericNameDisplay.displayToken = $"INTERACTABLE_{InteractableLangToken}_NAME";

            // provides an interaction with object
            var shrineManager = InteractableModel.AddComponent<ShrineOfRepairPurchase.RepairShrineManager>();
            shrineManager.PurchaseInteraction = purchaseInteraction;
            shrineManager.ScalingModifier = PurchaseInteractionGoldScalingModifier.Value;
            shrineManager.UseDefaultScaling = PurchaseInteractionGoldUseDefaultScaling.Value;

            // provides collision with object
            var entityLocator = InteractableModel.GetComponentInChildren<MeshCollider>().gameObject.AddComponent<EntityLocator>();
            entityLocator.entity = InteractableModel;

            // i think it is used for it to be interactable?
            var modelLocator = InteractableModel.AddComponent<ModelLocator>();
            modelLocator.modelTransform = InteractableModel.transform.Find("mdlRepairShrine");
            modelLocator.modelBaseTransform = InteractableModel.transform.Find("Base");
            modelLocator.dontDetatchFromParent = true;
            modelLocator.autoUpdateModelTransform = true;

            // provides a coordinates on model to spawn floating price
            var hologramController = InteractableModel.AddComponent<HologramProjector>();
            hologramController.hologramPivot = InteractableModel.transform.Find("HologramPivot");
            hologramController.displayDistance = 10;
            hologramController.disableHologramRotation = false;

            // used to show outline when you come near the interactable
            var highlightController = InteractableModel.GetComponent<Highlight>();
            highlightController.targetRenderer = InteractableModel.GetComponentsInChildren<MeshRenderer>().Where(x => x.gameObject.name.Contains("mdlRepairShrine")).First();
            highlightController.strength = 1;
            highlightController.highlightColor = Highlight.HighlightColor.interactive;

            // this is probably an icon on top of the shrine
            var icon = InteractableModel.transform.Find("Icon");
            var billboard = icon.gameObject.AddComponent<Billboard>();

            // applying hopoo shader to the icon
            Material material = LegacyResourcesAPI.Load<SpawnCard>("spawncards/interactablespawncard/iscShrineBoss").prefab.transform.Find("Symbol").GetComponent<MeshRenderer>().material;
            SpriteRenderer component = icon.GetComponent<SpriteRenderer>();
            Texture texture = component.material.mainTexture;

            var color = component.color;

            component.material = new Material(material.shader);
            component.material.CopyPropertiesFromMaterial(material);
            component.material.mainTexture = texture;
            component.material.SetColor("_TintColor", color);

            PrefabAPI.RegisterNetworkPrefab(InteractableModel);

            return InteractableModel;
        }

        private CostTypeIndex GetCostTypeFromConfig(CostTypes currency)
        {
            switch (currency)
            {
                case CostTypes.LunarCoin:
                    return CostTypeIndex.LunarCoin;
                case CostTypes.VoidCoin:
                    return CostTypeIndex.VoidCoin;
                default:
                case CostTypes.Gold:
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

        private void Hooks()
        {
            On.RoR2.PurchaseInteraction.GetInteractability += (orig, self, activator) =>
            {
                if (self.displayNameToken == $"INTERACTABLE_{InteractableLangToken}_NAME" && activator)
                {
                    var body = activator.GetComponent<CharacterBody>();
                    if (body && body.master)
                    {
                        bool isShrineAvailable = false;

                        FillRepairItemsDictionary();
                        foreach (KeyValuePair<ItemIndex, ItemIndex> pairedItems in RepairItemsDictionary)
                        {
                            if (body.inventory.GetItemCount(pairedItems.Key) > 0)
                            {
                                isShrineAvailable = true;
                            }
                        }
                        if (RepairEquipmentsDictionary.ContainsKey(body.equipmentSlot.equipmentIndex)) isShrineAvailable = true;
                        if (!isShrineAvailable) { return Interactability.ConditionsNotMet; }
                    }
                }
                return orig(self, activator);
            };
        }

        public class RepairShrineManager : NetworkBehaviour
        {
            public PurchaseInteraction PurchaseInteraction;
            public float ScalingModifier;
            public bool UseDefaultScaling;

            [SyncVar]
            public int BaseCostDetermination;

            public void Start()
            {
                if (NetworkServer.active && Run.instance)
                {
                    PurchaseInteraction.SetAvailable(true);
                }

                PurchaseInteraction.onPurchase.AddListener(RepairPurchaseAttempt);
                if (PurchaseInteraction.costType == CostTypeIndex.Money)
                {
                    BaseCostDetermination = UseDefaultScaling
                        ? (int)(PurchaseInteraction.cost * ScalingModifier)
                        : (int)(Mathf.Pow(Run.instance.compensatedDifficultyCoefficient, ScalingModifier) * PurchaseInteraction.cost);

                    PurchaseInteraction.cost = BaseCostDetermination;
                }
                else
                {
                    // probably for networking, I dunno
                    BaseCostDetermination = PurchaseInteraction.cost;
                    PurchaseInteraction.cost = BaseCostDetermination;
                }
            }

            [Server]
            public void RepairPurchaseAttempt(Interactor interactor)
            {
                if (!NetworkServer.active)
                {
                    MyLogger.LogWarning("[Server] function 'ShrineOfRepair.Interactables.ShrineOfRepair::RepairPurchaseAttempt(RoR2.Interactor)' called on client");
                    return;
                }

                if (!interactor) { return; }
                var body = interactor.GetComponent<CharacterBody>();
                if (body && body.master)
                {
                    var inventory = body.inventory;
                    FillRepairItemsDictionary();
                    foreach (KeyValuePair<ItemIndex, ItemIndex> pairedItems in RepairItemsDictionary)
                    {
                        int numberOfItems = inventory.GetItemCount(pairedItems.Key);
                        if (numberOfItems > 0)
                        {
                            inventory.RemoveItem(pairedItems.Key, numberOfItems);
                            inventory.GiveItem(pairedItems.Value, numberOfItems);
                            CharacterMasterNotificationQueue.SendTransformNotification(body.master, pairedItems.Key, pairedItems.Value, CharacterMasterNotificationQueue.TransformationType.Default);
                        }
                    }
                    if (RepairEquipmentsDictionary.ContainsKey(body.equipmentSlot.equipmentIndex))
                    {
                        inventory.SetEquipmentIndex(RepairEquipmentsDictionary[body.equipmentSlot.equipmentIndex]);
                        CharacterMasterNotificationQueue.PushEquipmentTransformNotification(body.master, body.equipmentSlot.equipmentIndex, RepairEquipmentsDictionary[body.equipmentSlot.equipmentIndex], CharacterMasterNotificationQueue.TransformationType.Default);
                    }

                    EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData()
                    {
                        origin = gameObject.transform.position,
                        rotation = Quaternion.identity,
                        scale = 1f,
                        color = (Color32)Color.red
                    }, true);

                    var billboard = gameObject.transform.Find("Icon").gameObject;
                    billboard.SetActive(false);

                    Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                    {
                        subjectAsCharacterBody = interactor.GetComponent<CharacterBody>(),
                        baseToken = "INTERACTABLE_SHRINE_REPAIR_INTERACT"
                    });

                    if (NetworkServer.active)
                    {
                        PurchaseInteraction.SetAvailable(false);
                    }
                }
            }

        }

    }


}
