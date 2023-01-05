using R2API;
using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static ShrineOfRepair.Modules.ShrineofRepairAssets;
using static ShrineOfRepair.Modules.ShrineOfRepairConfigManager;
using static ShrineOfRepair.Modules.ShrineOfRepairPlugin;

namespace ShrineOfRepair.Modules.Interactables
{
    public class ShrineOfRepairPicker : ShrineOfRepairBase<ShrineOfRepairPicker>
    {
        public override void Init()
        {
            if (UsePickupPickerPanel.Value)
            {
                CreateInteractables();
                Hooks();
            }
        }

        internal override GameObject CreateInteractable(GameObject InteractableModel)
        {
            InteractableModel.AddComponent<NetworkIdentity>();

            // picker controller, to make interactable show the PickerPanel upon interaction
            var pickerController = InteractableModel.AddComponent<PickupPickerController>();
            pickerController.contextString = $"INTERACTABLE_{InteractableLangToken}_CONTEXT";
            // you can use either ScrapperPickerPanel or CommandPickerPanel, either work for the component
            GameObject panelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Scrapper/ScrapperPickerPanel.prefab").WaitForCompletion();
            GameObject clonePrefab = panelPrefab.InstantiateClone("ShrineRepairPickerPanel");
            // game complains that it doesn't have networkidentity for some reason
            clonePrefab.AddComponent<NetworkIdentity>();
            pickerController.panelPrefab = clonePrefab;
            pickerController.cutoffDistance = 10f;

            // provides information and icon when you ping an object
            var pingInfoProvider = InteractableModel.AddComponent<PingInfoProvider>();
            pingInfoProvider.pingIconOverride = MainBundle.LoadAsset<Sprite>("Assets/RoR2/Base/Common/MiscIcons/texShrineIconOutlined.png");

            // provides a name
            var genericNameDisplay = InteractableModel.AddComponent<GenericDisplayNameProvider>();
            genericNameDisplay.displayToken = $"INTERACTABLE_{InteractableLangToken}_NAME";

            // provides collision with object
            var entityLocator = InteractableModel.GetComponentInChildren<MeshCollider>().gameObject.AddComponent<EntityLocator>();
            entityLocator.entity = InteractableModel;

            // i think it is used for it to be interactable?
            var modelLocator = InteractableModel.AddComponent<ModelLocator>();
            modelLocator.modelTransform = InteractableModel.transform.Find("mdlRepairShrine");
            modelLocator.modelBaseTransform = InteractableModel.transform.Find("Base");
            modelLocator.dontDetatchFromParent = true;
            modelLocator.autoUpdateModelTransform = true;

            // used to show outline when you come near the interactable
            var highlightController = InteractableModel.AddComponent<Highlight>();
            highlightController.targetRenderer = InteractableModel.GetComponentsInChildren<MeshRenderer>().Where(x => x.gameObject.name.Contains("mdlRepairShrine")).First();
            highlightController.strength = 1;
            highlightController.highlightColor = Highlight.HighlightColor.interactive;

            // shows an icon on top of the interactable
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

            // provides a manager than handles what happens when you interact with object
            var shrineManager = InteractableModel.AddComponent<ShrineRepairManager>();
            shrineManager.pickupPickerController = pickerController;
            shrineManager.iconTransform = icon;

            PrefabAPI.RegisterNetworkPrefab(InteractableModel);

            return InteractableModel;
        }

        protected override void Hooks()
        {
            base.Hooks();
            On.RoR2.UI.PickupPickerPanel.Awake += (orig, self) =>
            {
                orig(self);
                if (self.name.Contains("ShrineRepair"))
                {
                    var mainPanel = self.transform.Find("MainPanel");
                    if (mainPanel != null)
                    {
                        var juice = mainPanel.transform.Find("Juice");
                        if (juice != null)
                        {
                            var label = juice.transform.Find("Label");
                            if (label != null)
                            {
                                var text = label.GetComponent<LanguageTextMeshController>();
                                if (text != null)
                                {
                                    text.token = "INTERACTABLE_SHRINE_REPAIR_PICKERPANEL_HEADER";
                                    text.ResolveString();
                                }
                            }

                        }
                    }
                }
            };
            On.RoR2.PickupPickerController.GetInteractability += (orig, self, activator) =>
            {
                if (self.name.Contains("ShrineRepair") && activator)
                {
                    var body = activator.GetComponent<CharacterBody>();
                    if (body && body.master)
                    {
                        bool isShrineAvailable = false;

                        FillRepairItemsDictionary();
                        foreach (KeyValuePair<ItemIndex, ItemIndex> pairedItems in RepairItemsDictionary)
                            if (body.inventory.GetItemCount(pairedItems.Key) > 0)
                                isShrineAvailable = true;
                        foreach (KeyValuePair<EquipmentIndex, EquipmentIndex> pairedItems in RepairEquipmentsDictionary)
                            if (body.equipmentSlot.equipmentIndex == pairedItems.Key)
                                isShrineAvailable = true;
                        if (!isShrineAvailable) { return Interactability.ConditionsNotMet; }
                    }
                }
                return orig(self, activator);
            };
        }

        public class ShrineRepairManager : NetworkBehaviour
        {
            public PickupPickerController pickupPickerController;
            public Transform iconTransform;

            [SyncVar]
            public float coefficient;
            private int uses;
            private bool useLunarCoins;

            private Interactor interactor;

            private static int kRpcHandleInteractionClient;
            private static int kRpcHandleDeactivateClient;

            public void Start()
            {
                pickupPickerController.onPickupSelected.AddListener(HandleSelection);
                pickupPickerController.onServerInteractionBegin.AddListener(HandleInteraction);

                coefficient = Mathf.Pow(Run.instance.compensatedDifficultyCoefficient, 1.25f);
                uses = 0;
                var scene = SceneCatalog.GetSceneDefForCurrentScene();
                useLunarCoins = PickerUseLunarByDefault.Value || (scene == SceneCatalog.GetSceneDefFromSceneName("bazaar")) || (UseLunarInMoon.Value && (scene == SceneCatalog.GetSceneDefFromSceneName("moon") || scene == SceneCatalog.GetSceneDefFromSceneName("moon2")));

                On.RoR2.UI.PickupPickerPanel.OnCreateButton += (orig, self, index, button) =>
                {
                    orig(self, index, button);
                    if (self.name.Contains("ShrineRepair") && self == pickupPickerController.panelInstanceController)
                    {
                        CharacterMaster master = interactor ? interactor.GetComponent<CharacterBody>().master : LocalUserManager.GetFirstLocalUser().cachedMasterController.master;

                        PickupDef pickupDef = PickupCatalog.GetPickupDef(self.pickerController.options[index].pickupIndex);
                        FillRepairItemsDictionary();
                        bool isItem = RepairItemsDictionary.TryGetValue(pickupDef.itemIndex, out var itemIndex);
                        int count = master.inventory.GetItemCount(pickupDef.itemIndex);
                        ItemTier tier = ItemCatalog.GetItemDef(itemIndex).tier;

                        if (isItem || RepairEquipmentsDictionary.ContainsKey(pickupDef.equipmentIndex))
                        {
                            MyLogger.LogMessage(string.Format("Price for {0}x{1} is {2}", pickupDef.nameToken, count, isItem ? GetTotalStackCost(tier, count) : GetEquipmentCost()));

                            if (PickerShowFree.Value || ((isItem ? GetTotalStackCost(tier, count) : GetEquipmentCost()) != 0))
                            {
                                GameObject textGameObject = new GameObject("CostText");
                                textGameObject.transform.SetParent(button.transform);
                                textGameObject.layer = 5;

                                RectTransform counterRect = textGameObject.AddComponent<RectTransform>();

                                HGTextMeshProUGUI counterText = textGameObject.AddComponent<HGTextMeshProUGUI>();
                                counterText.enableWordWrapping = false;
                                counterText.alignment = TMPro.TextAlignmentOptions.Bottom;
                                counterText.fontSize = 20f;
                                counterText.faceColor = useLunarCoins ? Color.white : Color.yellow;
                                counterText.outlineWidth = 0.2f;
                                counterText.text = (useLunarCoins ? "<sprite name=\"LunarCoin\" tint=1>" : "$") + (isItem ? GetTotalStackCost(tier, count) : GetEquipmentCost());

                                counterRect.localPosition = Vector3.zero;
                                counterRect.anchorMin = Vector2.zero;
                                counterRect.anchorMax = Vector2.one;
                                counterRect.localScale = Vector3.one;
                                counterRect.sizeDelta = new Vector2(-10, -4);
                                counterRect.anchoredPosition = Vector2.zero;
                            }

                            //if (isItem && PickerDisplayStack.Value && (PickerShowOne.Value || count > 1))
                            //{
                            //    GameObject textGameObject = new GameObject("StackText");
                            //    textGameObject.transform.SetParent(button.transform);
                            //    textGameObject.layer = 5;

                            //    RectTransform stackerRect = textGameObject.AddComponent<RectTransform>();

                            //    HGTextMeshProUGUI stackerText = textGameObject.AddComponent<HGTextMeshProUGUI>();
                            //    stackerText.enableWordWrapping = false;
                            //    stackerText.alignment = TMPro.TextAlignmentOptions.TopRight;
                            //    stackerText.fontSize = 18f;
                            //    stackerText.faceColor = Color.white;
                            //    stackerText.text = (PickerShowX.Value ? "x" : "") + count;

                            //    stackerRect.localPosition = Vector3.zero;
                            //    stackerRect.anchorMin = Vector2.zero;
                            //    stackerRect.anchorMax = Vector2.one;
                            //    stackerRect.localScale = Vector3.one;
                            //    stackerRect.sizeDelta = new Vector2(-10, -3);
                            //    stackerRect.anchoredPosition = Vector2.zero;
                            //}
                        }
                    }
                };
            }

            public void OnDestroy()
            {
                pickupPickerController.onPickupSelected.RemoveListener(HandleSelection);
                pickupPickerController.onServerInteractionBegin.RemoveListener(HandleInteraction);
            }

            [Server]
            public void HandleSelection(int selection)
            {
                CallRpcHandleInteractionClient();
                if (!NetworkServer.active)
                {
                    MyLogger.LogWarning("[Server] function 'ShrineOfRepair.Interactables.ShrineOfRepairManager::HandleSelection(int)' called on client");
                    return;
                }

                MyLogger.LogMessage("Selected " + selection);

                if (interactor)
                {
                    PickupDef pickupDef = PickupCatalog.GetPickupDef(new PickupIndex(selection));
                    CharacterBody body = interactor.GetComponent<CharacterBody>();
                    int numberOfItems = body.inventory.GetItemCount(pickupDef.itemIndex);
                    FillRepairItemsDictionary();

                    // since broken items by default don't have tier
                    // we use our dictionary to get itemTier of non-broken item
                    bool isItem = RepairItemsDictionary.TryGetValue(pickupDef.itemIndex, out var itemIndex);
                    ItemTier tier = ItemCatalog.GetItemDef(itemIndex).tier;

                    if (isItem || RepairEquipmentsDictionary.ContainsKey(pickupDef.equipmentIndex))
                    {
                        uint cost = isItem ? GetTotalStackCost(tier, numberOfItems) : GetEquipmentCost();
                        if (cost > (useLunarCoins ? body.master.playerCharacterMasterController.networkUser.lunarCoins : body.master.money))
                        {
                            MyLogger.LogWarning(string.Format("Somehow player {0} ({1}) has less currency than price of {2}x{3}, yet it was available at the start of interaction. Doing nothing...", body.GetUserName(), body.name, pickupDef.nameToken, numberOfItems));
                            return;
                        }

                        string pickupColorHex, pickupName, pickupAmountString;
                        if (isItem)
                        {
                            body.inventory.RemoveItem(pickupDef.itemIndex, numberOfItems);
                            body.inventory.GiveItem(itemIndex, numberOfItems);
                            CharacterMasterNotificationQueue.SendTransformNotification(body.master, pickupDef.itemIndex, itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);

                            pickupColorHex = ColorCatalog.GetColorHexString(ItemTierCatalog.GetItemTierDef(tier).colorIndex);
                            pickupName = Language.GetString(ItemCatalog.GetItemDef(itemIndex).nameToken);
                            pickupAmountString = (numberOfItems == 1) ? "" : ("<style=\"cEvent\">(" + numberOfItems + ")</style>");
                        }
                        else
                        {
                            body.inventory.SetEquipmentIndex(RepairEquipmentsDictionary[pickupDef.equipmentIndex]);
                            CharacterMasterNotificationQueue.PushEquipmentTransformNotification(body.master, pickupDef.equipmentIndex, RepairEquipmentsDictionary[pickupDef.equipmentIndex], CharacterMasterNotificationQueue.TransformationType.Default);

                            pickupColorHex = ColorCatalog.GetColorHexString(EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex).colorIndex);
                            pickupName = Language.GetString(EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex).nameToken);
                            pickupAmountString = "";
                        }

                        if (useLunarCoins) body.master.playerCharacterMasterController.networkUser.DeductLunarCoins(cost);
                        else body.master.money -= cost;
                        MyLogger.LogMessage(string.Format("Player {0} ({1}) paid {2} {3} to repair {4}x{5}", body.GetUserName(), body.name, cost, useLunarCoins ? "lunar coins" : "money", pickupDef.nameToken, numberOfItems));

                        EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData()
                        {
                            origin = gameObject.transform.position,
                            rotation = Quaternion.identity,
                            scale = 1f,
                            color = (Color32)Color.red
                        }, true);

                        Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                        {
                            subjectAsCharacterBody = body,
                            baseToken = "INTERACTABLE_SHRINE_REPAIR_INTERACT_PICKER",
                            paramTokens = new string[] { "<color=#" + pickupColorHex + ">" + pickupName + "</color>", pickupAmountString }
                        });

                        Destroy(pickupPickerController.panelInstance);

                        uses++;
                        if (uses == MaxUses.Value)
                        {
                            CallRpcHandleDeactivateClient();
                            pickupPickerController.SetAvailable(false);
                            iconTransform.gameObject.SetActive(false);
                        }
                    }
                }
            }

            [Server]
            public void HandleInteraction(Interactor interactor)
            {
                if (!NetworkServer.active)
                {
                    MyLogger.LogWarning("[Server] function 'ShrineOfRepair.Interactables.ShrineOfRepairManager::HandleInteraction(RoR2.Interactor)' called on client");
                    return;
                }

                this.interactor = interactor;

                List<PickupPickerController.Option> options = new List<PickupPickerController.Option>();

                var charBody = interactor.GetComponent<CharacterBody>();
                if (charBody && charBody.master)
                {
                    var currentCurrency = useLunarCoins ? charBody.master.playerCharacterMasterController.networkUser.lunarCoins : charBody.master.money;

                    FillRepairItemsDictionary();
                    foreach (KeyValuePair<ItemIndex, ItemIndex> pairedItems in RepairItemsDictionary)
                    {
                        var itemCount = charBody.inventory.GetItemCount(pairedItems.Key);
                        if (itemCount > 0)
                        {
                            options.Add(new PickupPickerController.Option
                            {
                                available = currentCurrency >= GetTotalStackCost(ItemCatalog.GetItemDef(pairedItems.Value).tier, itemCount),
                                pickupIndex = PickupCatalog.FindPickupIndex(pairedItems.Key)
                            });
                        }
                    }
                    if (RepairEquipmentsDictionary.ContainsKey(charBody.equipmentSlot.equipmentIndex))
                    {
                        options.Add(new PickupPickerController.Option
                        {
                            available = currentCurrency >= GetEquipmentCost(),
                            pickupIndex = PickupCatalog.FindPickupIndex(charBody.equipmentSlot.equipmentIndex)
                        });
                    }

                    pickupPickerController.SetOptionsServer(options.ToArray());
                }
            }

            private int GetCostFromItemTier(ItemTier tier)
            {
                if (BubbetItemsCompat.enabled && BubbetItemsCompat.IsVoidLunar(tier)) return PickerPanelGoldLunarCost.Value;
                switch (tier)
                {
                    case ItemTier.Tier1:
                    case ItemTier.VoidTier1:
                        return PickerPanelGoldTier1Cost.Value;
                    case ItemTier.Tier2:
                    case ItemTier.VoidTier2:
                    default:
                        return PickerPanelGoldTier2Cost.Value;
                    case ItemTier.Tier3:
                    case ItemTier.VoidTier3:
                        return PickerPanelGoldTier3Cost.Value;
                    case ItemTier.Boss:
                    case ItemTier.VoidBoss:
                        return PickerPanelGoldBossCost.Value;
                    case ItemTier.Lunar:
                        return PickerPanelGoldLunarCost.Value;
                }
            }

            private uint GetTotalStackCost(ItemTier tier, int numberOfItems)
            {
                if (GetCostFromItemTier(tier) <= 0) return 0;
                var res = (uint)(GetCostFromItemTier(tier) * numberOfItems * (useLunarCoins ? BazaarLunarMultiplier.Value : coefficient));
                if (res <= 0) return 1;
                return res;
            }

            private uint GetEquipmentCost()
            {
                if (PickerPanelGoldEquipCost.Value <= 0) return 0;
                var res = (uint)(PickerPanelGoldEquipCost.Value * (useLunarCoins ? BazaarLunarMultiplier.Value : coefficient));
                if (res <= 0) return 1;
                return res;
            }

            private void CallRpcHandleInteractionClient()
            {
                if (!NetworkServer.active)
                {
                    MyLogger.LogWarning("RPC Function CallRpcHandleInteractionClient called on client.");
                    return;
                }

                MyLogger.LogMessage("RPC Function CallRpcHandleInteractionClient, sending message to clients");

                NetworkWriter writer = new NetworkWriter();
                writer.Write((short)0);
                writer.Write((short)2);
                writer.WritePackedUInt32((uint)kRpcHandleInteractionClient);
                writer.Write(GetComponent<NetworkIdentity>().netId);
                SendRPCInternal(writer, 0, "RpcHandleInteractionClient");
            }

            private void CallRpcHandleDeactivateClient()
            {
                if (!NetworkServer.active)
                {
                    MyLogger.LogWarning("RPC Function CallRpcHandleDeactivateClient called on client.");
                    return;
                }

                MyLogger.LogMessage("RPC Function CallRpcHandleDeactivateClient, sending message to clients");

                NetworkWriter writer = new NetworkWriter();
                writer.Write((short)0);
                writer.Write((short)2);
                writer.WritePackedUInt32((uint)kRpcHandleDeactivateClient);
                writer.Write(GetComponent<NetworkIdentity>().netId);
                SendRPCInternal(writer, 0, "RpcHandleDeactivateClient");
            }

            protected static void InvokeRpcHandleInteractionClient(NetworkBehaviour obj, NetworkReader reader)
            {
                if (!NetworkClient.active)
                {
                    MyLogger.LogWarning("RPC RpcHandleInteractionClient called on server.");
                }
                else
                {
                    ((ShrineRepairManager)obj).RpcHandleInteactionClient();
                }
            }

            protected static void InvokeRpcHandleDeactivateClient(NetworkBehaviour obj, NetworkReader reader)
            {
                if (!NetworkClient.active)
                {
                    MyLogger.LogWarning("RPC RpcHandleDeactivateClient called on server.");
                }
                else
                {
                    ((ShrineRepairManager)obj).RpcHandleDeactivateClient();
                }
            }

            [ClientRpc]
            public void RpcHandleInteactionClient()
            {
                MyLogger.LogMessage("RPC RpcHandleInteactionClient message recieved");
                if (pickupPickerController && pickupPickerController.panelInstance) Destroy(pickupPickerController.panelInstance);
            }

            [ClientRpc]
            public void RpcHandleDeactivateClient()
            {
                MyLogger.LogMessage("RPC RpcHandleDeactivateClient message recieved");
                if (iconTransform) iconTransform.gameObject.SetActive(false);
                // lets brute force it because fuck it, what could possibly go wrong
                // we cant use SetAvailable() because it's not allowed on clients
                // and I guess PickupPickerController doesn't sync it for some reason
                // unlike PurchaseInteraction
                if (pickupPickerController) pickupPickerController.available = false;
            }

            // RPC shamelessly stolen from MoreShrines by Evaisa
            static ShrineRepairManager()
            {
                kRpcHandleInteractionClient = 1268743049; // I guess that's id to separate messages by
                RegisterRpcDelegate(typeof(ShrineRepairManager), kRpcHandleInteractionClient, InvokeRpcHandleInteractionClient);
                kRpcHandleDeactivateClient = 1631732147;
                RegisterRpcDelegate(typeof(ShrineRepairManager), kRpcHandleDeactivateClient, InvokeRpcHandleDeactivateClient);
                NetworkCRC.RegisterBehaviour("ShrineRepairManager", 0);
            }
        }

    }
}