using RoR2;
using ShrineOfRepair;
using ShrineOfRepair.ModCompat;
using ShrineOfRepair.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static ShrineOfRepair.Modules.ShrineOfRepairConfigManager;

namespace ShrineOfRepair.Behaviours
{
    public class ShrineOfRepairPickerManager : NetworkBehaviour
    {
        public PickupPickerController pickerController;

        public Transform iconTransform;

        [SyncVar]
        public float coefficient;

        private Interactor interactor;

        public bool useLunarCoins { get; private set; }

        private int uses;

        public void Start()
        {
            if (NetworkServer.active)
            {
                coefficient = Mathf.Pow(Run.instance.compensatedDifficultyCoefficient, 1.25f);
            }
            uses = 0;
            var scene = SceneCatalog.GetSceneDefForCurrentScene();
            useLunarCoins = PickerUseLunarByDefault.Value
                || BazaarUseLunar.Value && scene == SceneCatalog.GetSceneDefFromSceneName("bazaar")
                || UseLunarInMoon.Value && (scene == SceneCatalog.GetSceneDefFromSceneName("moon") || scene == SceneCatalog.GetSceneDefFromSceneName("moon2"));
        }

        public void HandleSelection(int selection)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            GetComponent<NetworkUIPromptController>().ClearParticipant();

            if (interactor)
            {
                PickupDef pickupDef = PickupCatalog.GetPickupDef(new PickupIndex(selection));
                CharacterBody body = interactor.GetComponent<CharacterBody>();
                var hasFreeUnlocks = body.GetBuffCount(DLC2Content.Buffs.FreeUnlocks) > 0;
                int numberOfItems = body.inventory.GetItemCount(pickupDef.itemIndex);

                // since broken items by default don't have tier
                // we use our dictionary to get itemTier of non-broken item
                bool isItem = ShrineOfRepairDictionary.RepairItemsDictionary.TryGetValue(pickupDef.itemIndex, out var itemIndex);
                ItemTier tier = ItemCatalog.GetItemDef(itemIndex).tier;

                if (isItem || ShrineOfRepairDictionary.RepairEquipmentsDictionary.ContainsKey(pickupDef.equipmentIndex))
                {
                    uint cost = isItem ? GetTotalStackCost(tier, numberOfItems) : GetEquipmentCost();
                    if (cost > (useLunarCoins ? body.master.playerCharacterMasterController.networkUser.lunarCoins : body.master.money) && !hasFreeUnlocks)
                    {
                        Log.Warning(string.Format("Somehow player {0} ({1}) has less currency than price of {2}x{3}, yet it was available at the start of interaction. Doing nothing...", body.GetUserName(), body.name, pickupDef.nameToken, numberOfItems));
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
                        pickupAmountString = numberOfItems == 1 ? "" : "<style=\"cEvent\">(" + numberOfItems + ")</style>";
                    }
                    else
                    {
                        body.inventory.SetEquipmentIndex(ShrineOfRepairDictionary.RepairEquipmentsDictionary[pickupDef.equipmentIndex]);
                        CharacterMasterNotificationQueue.PushEquipmentTransformNotification(body.master, pickupDef.equipmentIndex, ShrineOfRepairDictionary.RepairEquipmentsDictionary[pickupDef.equipmentIndex], CharacterMasterNotificationQueue.TransformationType.Default);

                        pickupColorHex = ColorCatalog.GetColorHexString(EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex).colorIndex);
                        pickupName = Language.GetString(EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex).nameToken);
                        pickupAmountString = "";
                    }

                    if (hasFreeUnlocks)
                    {
                        body.RemoveBuff(DLC2Content.Buffs.FreeUnlocks);
                        Util.PlaySound("Play_item_proc_onLevelUpFreeUnlock_activate", gameObject);
                    }
                    else
                    {
                        if (useLunarCoins) body.master.playerCharacterMasterController.networkUser.DeductLunarCoins(cost);
                        else body.master.money -= cost;
                    }

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

                    uses++;
                    if (uses == MaxUses.Value)
                    {
                        RpcHandleDeactivateClient();
                        pickerController.SetAvailable(false);
                        if (iconTransform)
                        {
                            iconTransform.gameObject.SetActive(false);
                        }
                    }
                }
            }

        }

        public void HandleInteraction(Interactor interactor)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            this.interactor = interactor;

            List<PickupPickerController.Option> options = new List<PickupPickerController.Option>();

            var charBody = interactor.GetComponent<CharacterBody>();
            var hasFreeUnlocks = charBody.GetBuffCount(DLC2Content.Buffs.FreeUnlocks) > 0;
            if (charBody && charBody.master)
            {
                var currentCurrency = useLunarCoins ? charBody.master.playerCharacterMasterController.networkUser.lunarCoins : charBody.master.money;

                foreach (KeyValuePair<ItemIndex, ItemIndex> pairedItems in ShrineOfRepairDictionary.RepairItemsDictionary)
                {
                    var itemCount = charBody.inventory.GetItemCount(pairedItems.Key);
                    if (itemCount > 0)
                    {
                        options.Add(new PickupPickerController.Option
                        {
                            available = currentCurrency >= GetTotalStackCost(ItemCatalog.GetItemDef(pairedItems.Value).tier, itemCount) || hasFreeUnlocks,
                            pickupIndex = PickupCatalog.FindPickupIndex(pairedItems.Key)
                        });
                    }
                }
                if (ShrineOfRepairDictionary.RepairEquipmentsDictionary.ContainsKey(charBody.equipmentSlot.equipmentIndex))
                {
                    options.Add(new PickupPickerController.Option
                    {
                        available = currentCurrency >= GetEquipmentCost() || hasFreeUnlocks,
                        pickupIndex = PickupCatalog.FindPickupIndex(charBody.equipmentSlot.equipmentIndex)
                    });
                }

                pickerController.SetOptionsServer(options.ToArray());
            }
        }

        public uint GetTotalStackCost(ItemTier tier, int numberOfItems)
        {
            if (GetCostFromItemTier(tier) <= 0) return 0;
            var res = (uint)(GetCostFromItemTier(tier) * numberOfItems * (useLunarCoins ? BazaarLunarMultiplier.Value : coefficient));
            if (res <= 0) return 1;
            return res;
        }

        public uint GetEquipmentCost()
        {
            if (PickerPanelGoldEquipCost.Value <= 0) return 0;
            var res = (uint)(PickerPanelGoldEquipCost.Value * (useLunarCoins ? BazaarLunarMultiplier.Value : coefficient));
            if (res <= 0) return 1;
            return res;
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

        [ClientRpc]
        public void RpcHandleDeactivateClient()
        {
            if (iconTransform) iconTransform.gameObject.SetActive(false);
            // lets brute force it because fuck it, what could possibly go wrong
            // we cant use SetAvailable() because it's not allowed on clients
            // and I guess PickupPickerController doesn't sync it for some reason
            // unlike PurchaseInteraction
            if (pickerController) pickerController.available = false;
        }
    }
}
