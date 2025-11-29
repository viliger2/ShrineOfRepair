using RoR2;
using RoR2.UI;
using ShrineOfRepair.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RoR2.ColorCatalog;
using static ShrineOfRepair.ShrineOfRepairStuff;

namespace ShrineOfRepairRewrite.Behaviours
{
    public class RepairInforPanelHelper : MonoBehaviour
    {
        [SerializeField]
        public InspectPanelController inspectPanelController;

        [SerializeField]
        public Image correspondingScrapImage;

        [SerializeField]
        public PickupPickerPanel panel;

        private MPEventSystem eventSystem;

        private Inventory cachedBodyInventory;

        private CharacterBody cachedBody;

        private void Awake()
        {
            MPEventSystemLocator component = GetComponent<MPEventSystemLocator>();
            eventSystem = component.eventSystem;
            if (eventSystem != null && eventSystem.localUser != null && eventSystem.localUser.cachedBody != null)
            {
                cachedBody = eventSystem.localUser.cachedBody;
                cachedBodyInventory = eventSystem.localUser.cachedBody.inventory;
            }
        }

        private void Update()
        {
            if (eventSystem.player.GetButtonDown(15))
            {
                UnityEngine.Object.Destroy(base.gameObject);
            }
        }

        public void ShowInfo(MPButton button, PickupDef pickupDef)
        {
            inspectPanelController.Show(pickupDef);

            var itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);

            if (itemDef != default(ItemDef))
            {
                if (ShrineOfRepair.Modules.ShrineOfRepairDictionary.RepairItemsDictionary.TryGetValue(itemDef.itemIndex, out var repairedItemIndex))
                {
                    var repairedItemDef = ItemCatalog.GetItemDef(repairedItemIndex);
                    if (repairedItemDef != default(ItemDef))
                    {
                        correspondingScrapImage.sprite = repairedItemDef.pickupIconSprite;
                        inspectPanelController.InspectTitle.token = repairedItemDef.nameToken;
                        inspectPanelController.InspectDescription.token = repairedItemDef.descriptionToken;
                        if (repairedItemDef._itemTierDef)
                        {
                            inspectPanelController.InspectTitleText.color = ColorCatalog.GetColor(repairedItemDef._itemTierDef.colorIndex);
                        } else
                        {
#pragma warning disable CS0618 // Type or member is obsolete
                            inspectPanelController.InspectTitleText.color = ColorCatalog.GetColor(repairedItemDef.colorIndex);
#pragma warning restore CS0618 // Type or member is obsolete
                        }
                    }
                }
            } else
            {
                var equipmentDef = EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex);
                if (equipmentDef != default(EquipmentDef))
                {
                    if (ShrineOfRepair.Modules.ShrineOfRepairDictionary.RepairEquipmentsDictionary.TryGetValue(equipmentDef.equipmentIndex, out var repairedEquipmentIndex))
                    {
                        var repairedEquipmentDef = EquipmentCatalog.GetEquipmentDef(repairedEquipmentIndex);
                        if (repairedEquipmentDef != default(EquipmentDef))
                        {
                            correspondingScrapImage.sprite = repairedEquipmentDef.pickupIconSprite;
                            inspectPanelController.InspectTitle.token = repairedEquipmentDef.nameToken;
                            inspectPanelController.InspectDescription.token = repairedEquipmentDef.descriptionToken;
                            inspectPanelController.InspectTitleText.color = ColorCatalog.GetColor(repairedEquipmentDef.colorIndex);
                        }
                    }
                }
            }
        }

        public void AddQuantityToPickerButton(MPButton button, PickupDef pickupDef)
        {
            if (!cachedBodyInventory)
            {
                return;
            }
            int itemCount = cachedBodyInventory.GetItemCountPermanent(pickupDef.itemIndex);
            TextMeshProUGUI textMeshProUGUI = button.GetComponent<ChildLocator>().FindChildComponent<TextMeshProUGUI>("Quantity");
            if ((bool)textMeshProUGUI)
            {
                if (itemCount > 1)
                {
                    textMeshProUGUI.SetText($"{itemCount}");
                }
                else
                {
                    textMeshProUGUI.gameObject.SetActive(value: false);
                }
            }

            if (!cachedBody)
            {
                return;
            }

            if (!panel || !panel.pickerController)
            {
                return;
            }

            var manager = panel.pickerController.gameObject.GetComponent<ShrineOfRepair.Behaviours.ShrineOfRepairPickerManager>();
            if (!manager)
            {
                return;
            }

            var hasFreeUnlocks = cachedBody.GetBuffCount(DLC2Content.Buffs.FreeUnlocks) > 0;
            bool isItem = ShrineOfRepairDictionary.RepairItemsDictionary.TryGetValue(pickupDef.itemIndex, out var itemIndex);
            int count = cachedBodyInventory.GetItemCountPermanent(pickupDef.itemIndex);
            ItemTier tier = ItemCatalog.GetItemDef(itemIndex).tier;

            GameObject textGameObject = new GameObject("CostText");
            textGameObject.transform.SetParent(button.transform);
            textGameObject.layer = 5;

            RectTransform counterRect = textGameObject.AddComponent<RectTransform>();

            HGTextMeshProUGUI counterText = textGameObject.AddComponent<HGTextMeshProUGUI>();
            counterText.enableWordWrapping = false;
            counterText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
            counterText.fontSize = 20f;
            counterText.faceColor = Color.white;
            counterText.outlineWidth = 0.2f;
            if (hasFreeUnlocks)
            {
                counterText.text = "FREE";
            }
            else
            {
                counterText.text = GetCurrencyText(manager.costType) + (isItem ? manager.GetTotalStackCost(tier, count) : manager.GetEquipmentCost());
            }

            counterRect.localPosition = Vector3.zero;
            counterRect.anchorMin = Vector2.zero;
            counterRect.anchorMax = Vector2.one;
            counterRect.localScale = Vector3.one;
            counterRect.sizeDelta = new Vector2(-10, -4);
            counterRect.anchoredPosition = Vector2.zero;
        }

        private string GetCurrencyText(CostTypes costType)
        {
            switch (costType)
            {
                default:
                case CostTypes.Gold:
                    return "$";
                case CostTypes.VoidCoin:
                    return "<sprite name=\"VoidCoin\" tint=1>";
                case CostTypes.LunarCoin:
                    return "<sprite name=\"LunarCoin\" tint=1>";
            }
        }
    }
}
