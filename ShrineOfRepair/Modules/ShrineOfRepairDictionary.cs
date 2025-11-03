using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using static ShrineOfRepair.Modules.ShrineOfRepairConfigManager;

namespace ShrineOfRepair.Modules
{
    public static class ShrineOfRepairDictionary
    {
        public static Dictionary<ItemIndex, ItemIndex> RepairItemsDictionary = new Dictionary<ItemIndex, ItemIndex>();

        public static Dictionary<EquipmentIndex, EquipmentIndex> RepairEquipmentsDictionary = new Dictionary<EquipmentIndex, EquipmentIndex>();

        [SystemInitializer(new Type[] { typeof(ItemCatalog), typeof(EquipmentCatalog) })]
        public static void Init()
        {
            FillRepairItemsDictionary();
        }

        public static void Hooks()
        {
            if (Modules.ShrineOfRepairConfigManager.RepairVoidItems.Value)
            {
                On.RoR2.Items.ContagiousItemManager.Init += ContagiousItemManager_Init;
            }
        }

        private static void ContagiousItemManager_Init(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            orig();
            foreach(var info in RoR2.Items.ContagiousItemManager.transformationInfos)
            {
                if (ItemCatalog.GetItemDef(info.transformedItem) == DLC1Content.Items.VoidMegaCrabItem)
                {
                    continue;
                };

                if (ItemCatalog.GetItemDef(info.transformedItem) == DLC1Content.Items.ElementalRingVoid)
                {
                    continue;
                };

                if (RepairItemsDictionary.TryGetValue(info.transformedItem, out _))
                {
                    var transformedItemDef = ItemCatalog.GetItemDef(info.transformedItem);
                    var originalItemDef = ItemCatalog.GetItemDef(info.originalItem);
                    Log.Warning($"RepairDictionary already has key {transformedItemDef} and value {originalItemDef}. Skipping...");
                    continue;
                }

                RepairItemsDictionary.Add(info.transformedItem, info.originalItem);
            }
        }

        public static void FillRepairItemsDictionary()
        {
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
                        if (RepairItemsDictionary.TryGetValue(broken, out var value))
                        {
                            Log.Warning($"Shrine Repair Dictionary already contains item {ItemCatalog.GetItemDef(broken)} that repairs into {ItemCatalog.GetItemDef(value)}. Skipping {kv[0]}->{kv[1]}");
                        }
                        else
                        {
                            RepairItemsDictionary.Add(broken, tofix);
                            continue;
                        }
                    }
                    EquipmentIndex broken2 = EquipmentCatalog.FindEquipmentIndex(kv[0].Trim());
                    EquipmentIndex tofix2 = EquipmentCatalog.FindEquipmentIndex(kv[1].Trim());
                    if (broken2 != EquipmentIndex.None && tofix2 != EquipmentIndex.None)
                    {
                        if (RepairEquipmentsDictionary.TryGetValue(broken2, out var value))
                        {
                            Log.Warning($"Shrine Repair Dictionary already contains equipment {EquipmentCatalog.GetEquipmentDef(broken2)} that repairs into {EquipmentCatalog.GetEquipmentDef(value)}. Skipping {kv[0]}->{kv[1]}");
                        }
                        else
                        {
                            RepairEquipmentsDictionary.Add(broken2, tofix2);
                            continue;
                        }
                    }
                }
            }

            RepairItemsDictionary = ModExtension.FillItemDictionaryFromMods(RepairItemsDictionary);
            RepairItemsDictionary = ModExtension.FillDictionaryFromMods(RepairItemsDictionary);
            RepairEquipmentsDictionary = ModExtension.FillEquipmentDictionaryFromMods(RepairEquipmentsDictionary);
#if DEBUG 
            Log.Debug("Items");
            foreach (var kv in RepairItemsDictionary) Log.Debug(ItemCatalog.GetItemDef(kv.Key).name + " -> " + ItemCatalog.GetItemDef(kv.Value).name);
            Log.Debug("Equipments");
            foreach (var kv in RepairEquipmentsDictionary) Log.Debug(EquipmentCatalog.GetEquipmentDef(kv.Key).name + " -> " + EquipmentCatalog.GetEquipmentDef(kv.Value).name);
#endif
        }

    }
}
