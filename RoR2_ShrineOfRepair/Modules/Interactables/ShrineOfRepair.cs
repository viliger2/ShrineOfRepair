using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.Hologram;
using R2API;
using System.Linq;
using static ShrineOfRepair.Modules.ShrineofRepairAssets;
using static ShrineOfRepair.Modules.ShrineOfRepairPlugin;

namespace ShrineOfRepair.Modules.Interactables
{
    public class ShrineOfRepair : InteractableBase<ShrineOfRepair>
    {
        public override string InteractableLangToken => "SHRINE_REPAIR";

        public enum CostTypes
        {
            Gold,
            VoidCoin,
            LunarCoin
        }

        public static ConfigEntry<int> DirectorCost;
        public static ConfigEntry<int> DirectorWeight;
        public static ConfigEntry<DirectorAPI.InteractableCategory> DirectorCategory;

        public static ConfigEntry<bool> UseBadModel;

        public static ConfigEntry<string> Blacklist;

        public static ConfigEntry<CostTypes> CurrencyType;

        public static ConfigEntry<int> LunarCoinCost;

        public static ConfigEntry<int> VoidCoinCost;

        public static ConfigEntry<int> GoldBaseCost;
        public static ConfigEntry<bool> GoldUseDefaultScaling;
        public static ConfigEntry<float> GoldScalingModifier;

        public static Dictionary<ItemIndex, ItemIndex> FillRepairItemsDictionary()
        {
            Dictionary<ItemIndex, ItemIndex> dictionary = new Dictionary<ItemIndex, ItemIndex>();

            dictionary.Add(DLC1Content.Items.FragileDamageBonusConsumed.itemIndex, DLC1Content.Items.FragileDamageBonus.itemIndex); // watch
            dictionary.Add(DLC1Content.Items.HealingPotionConsumed.itemIndex, DLC1Content.Items.HealingPotion.itemIndex); // potion
            dictionary.Add(RoR2Content.Items.ExtraLifeConsumed.itemIndex, RoR2Content.Items.ExtraLife.itemIndex); // dio
            dictionary.Add(DLC1Content.Items.ExtraLifeVoidConsumed.itemIndex, DLC1Content.Items.ExtraLifeVoid.itemIndex); // larva

            // ItemIndex enums are index numbers from "Item & Equipment IDs and Names" page
            var itemIds = Blacklist.Value.Split(',');
            foreach (var itemId in itemIds)
            {
                ItemIndex itemIndex;
                if (System.Enum.TryParse(itemId.Trim(), out itemIndex))
                {
                    dictionary.Remove(itemIndex);
                }
            }

            return dictionary;
        }

        public override void Init(ConfigFile config)
        {
            ShrineofRepairAssets.Init();

            InitConfig(config);
            CreateInteractables();
            Hooks();
        }

        private void CreateInteractables()
        {
            var normalModel = CreateInteractable(UseBadModel.Value ? MainBundle.LoadAsset<GameObject>("ShrineRepair.prefab") : MainBundle.LoadAsset<GameObject>("ShrineRepairGood.prefab"));
            var sandyModel = CreateInteractable(UseBadModel.Value ? MainBundle.LoadAsset<GameObject>("ShrineRepair.prefab") : MainBundle.LoadAsset<GameObject>("ShrineRepairGood_sandy.prefab"));
            var snowyModel = CreateInteractable(UseBadModel.Value ? MainBundle.LoadAsset<GameObject>("ShrineRepair.prefab") : MainBundle.LoadAsset<GameObject>("ShrineRepairGood_snowy.prefab"));

            CreateInteractableSpawnCard(normalModel, "iscShrineRepair", GetNormalStageList());
            CreateInteractableSpawnCard(sandyModel, "iscShrineRepairSandy", GetSandyStageList());
            CreateInteractableSpawnCard(snowyModel, "iscShrineRepairSnowy", GetSnowyStageList());
        }

        private void InitConfig(ConfigFile config)
        {
            UseBadModel = config.Bind("Model", "Use Shitty Model", false, "Use shitty model that I made myself. If you want to see what bad modeling by bad programmer looks like - be my guest. I made it, so might as well put it here.");

            DirectorCost = config.Bind("Director", "Director Cost", 20, "Cost of the shrine in director credits. By defeult equal to the cost of most shrines.");
            DirectorWeight = config.Bind("Director", "Director Weight", 1, "Weight of the shrine for director. The lower the value, the more rare the shrine is. By default has the same weight as Shrine of Order, the only difference is that it can spawn anywhere.");
            DirectorCategory = config.Bind("Director", "Director Category", DirectorAPI.InteractableCategory.Shrines, "Category of interactable. If you change this, then you should also change Director Cost and Director Weight, as default values for those are balanced around it being spawned as a shrine.");

            Blacklist = config.Bind("Blacklist", "Blacklist", "", "Blacklist for items. Adding an item to the list will make them unrepairable. List should consists of item ids separated by commas and everything else will be (hopefully) ignored. IDs for Hoopo items:\nDelicate Watch (Broken) - 68, Empty Bottle - 81, Dio's Best Friend (Consumed) - 57, Pluripotent Larva (Consumed) - 59");

            CurrencyType = config.Bind("Currency", "Currency Type", CostTypes.Gold, "Type of currency used to purchase shrine. Using anything other than \"Gold\" disables price scaling over time. Each currency has its own options.");

            LunarCoinCost = config.Bind("Lunar Coins", "Shrine Base Cost", 2, "Base cost of the interactable in lunar coins. Does not scale with time. Can be used with EphemeralCoins.");

            VoidCoinCost = config.Bind("Void Coins", "Shrine Base Cost", 2, "Base cost of the interactable in void coins. Does not scale with time. To be used with ReleasedFromTheVoid.");

            GoldBaseCost = config.Bind("Gold", "Shrine Base Cost", 12, "Base cost of the interactable in gold that is used for scaling. Will spawn with this cost at the start of the run.");
            GoldUseDefaultScaling = config.Bind("Gold", "Use Default Scaling", false, "Use default scaling formula instead of custom scaling formula for the shrine. Custom formula is diffCoef^customsScalingModifier * BaseCost, default formula is diffCoef^1.25 * BaseCost * ScalingModifier");
            GoldScalingModifier = config.Bind("Gold", "Scaling Modifier", 1.35f, "Used for defining how cost of shrine scales throughout the run for both default and custom scaling formulas.");
        }

        private GameObject CreateInteractable(GameObject InteractableModel)
        {
            InteractableModel.AddComponent<NetworkIdentity>();

            // provides purchase interaction for what we do with a shrine
            var purchaseInteraction = InteractableModel.AddComponent<PurchaseInteraction>();
            purchaseInteraction.displayNameToken = $"INTERACTABLE_{InteractableLangToken}_NAME";
            purchaseInteraction.contextToken = $"INTERACTABLE_{InteractableLangToken}_CONTEXT";
            purchaseInteraction.costType = GetCostTypeFromConfig(CurrencyType.Value);
            purchaseInteraction.automaticallyScaleCostWithDifficulty = CurrencyType.Value == CostTypes.Gold && GoldUseDefaultScaling.Value;
            purchaseInteraction.cost = GetCostValueFromConfig(CurrencyType.Value);
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
            var shrineManager = InteractableModel.AddComponent<RepairShrineManager>();
            shrineManager.PurchaseInteraction = purchaseInteraction;
            shrineManager.ScalingModifier = GoldScalingModifier.Value;
            shrineManager.UseDefaultScaling = GoldUseDefaultScaling.Value;

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
                    return LunarCoinCost.Value;
                case CostTypes.VoidCoin:
                    return VoidCoinCost.Value;
                default:
                case CostTypes.Gold:
                    return GoldBaseCost.Value;
            }
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

            stageList.Add(DirectorAPI.Stage.AbandonedAqueduct);;

            stageList.Add(DirectorAPI.Stage.AbandonedAqueductSimulacrum);

            return stageList;

        }
      
        private void CreateInteractableSpawnCard(GameObject InteractableModel, string name, List<DirectorAPI.Stage> stageList)
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
            
            foreach(DirectorAPI.Stage stage in stageList)
            {
                DirectorAPI.Helpers.AddNewInteractableToStage(directorCard, DirectorCategory.Value, stage);
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

                        var dictionary = FillRepairItemsDictionary();
                        foreach (KeyValuePair<ItemIndex, ItemIndex> pairedItems in dictionary)
                        {
                            if (body.inventory.GetItemCount(pairedItems.Key) > 0)
                            {
                                isShrineAvailable = true;
                            }
                        }
                        if (!isShrineAvailable) { return Interactability.ConditionsNotMet; }
                    }
                }
                return orig(self, activator);
            };
        }

    }

    public class RepairShrineManager : NetworkBehaviour
    {
        public PurchaseInteraction PurchaseInteraction;
        public float ScalingModifier;
        public bool UseDefaultScaling;

        [SyncVar]
        public int BaseCostDetermination;

        public Dictionary<ItemIndex, ItemIndex> RepairItemsDictionary;

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

            RepairItemsDictionary = ShrineOfRepair.FillRepairItemsDictionary();
        }

        [Server]
        public void RepairPurchaseAttempt(Interactor interactor)
        {
            if (!NetworkServer.active)
            {
                MyLogger.LogWarning("[Server] function 'SrhineOfRepair.Interactables.ShrineOfRepair::RepairPurchaseAttempt(RoR2.Interactor)' called on client");
                return;
            }

            if (!interactor) { return; }
            var body = interactor.GetComponent<CharacterBody>();
            if (body && body.master)
            {
                var inventory = body.inventory;
                foreach (KeyValuePair<ItemIndex, ItemIndex> pairedItems in RepairItemsDictionary)
                {
                    int numberOfItems = inventory.GetItemCount(pairedItems.Key);
                    if (numberOfItems > 0)
                    {
                        inventory.RemoveItem(pairedItems.Key, numberOfItems);
                        inventory.GiveItem(pairedItems.Value, numberOfItems);
                    }
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
