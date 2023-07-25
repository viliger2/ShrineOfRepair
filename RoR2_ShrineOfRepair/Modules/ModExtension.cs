using RoR2;
using System.Collections.Generic;
using static ShrineOfRepair.Modules.ShrineOfRepairPlugin;

namespace ShrineOfRepair.Modules
{
    public class ModExtension
    {
        //public delegate void DictionaryFillDelegate(ref List<RepairableItems> list);
        public delegate void DictionaryFillItemsDelegate();
        public delegate void DictionaryFillEquipmentDelegate();

        private struct RepairableItems
        {
            public ItemIndex brokenItem;
            public ItemIndex repairedItem;
            public string modName;
        }

        private struct RepairableEquipment
        {
            public EquipmentIndex brokenEquipment;
            public EquipmentIndex repairedEquipment;
            public string modName;
        }

        private static DictionaryFillItemsDelegate fillItemsDictionaryHandler;
        private static DictionaryFillEquipmentDelegate fillEquipmentDictionaryHandler;

        //private static DictionaryFillDelegate fillDictionaryHandler;

        private static List<RepairableItems> ModdedItemsList = new List<RepairableItems>();
        private static List<RepairableEquipment> ModdedEquipmentList = new List<RepairableEquipment>();

        public static void AddItemsListener(DictionaryFillItemsDelegate callback)
        {
            fillItemsDictionaryHandler += callback;
            MyLogger.LogMessage($"Added {callback.Method} to fillItemsDictionaryHandler");
        }

        public static void AddItemsToList(ItemIndex brokentItem, ItemIndex repairedItem, string modName)
        {
            ModdedItemsList.Add(new RepairableItems { brokenItem = brokentItem, repairedItem = repairedItem, modName = modName});
        }

        public static void AddEquipmentListener(DictionaryFillEquipmentDelegate callback)
        {
            fillEquipmentDictionaryHandler += callback;
            MyLogger.LogMessage($"Added {callback.Method} to fillEquipmentDictionaryHandler");
        }

        public static void AddEquipmentToList(EquipmentIndex brokenEquipment, EquipmentIndex repairedEquipment, string modName)
        {
            ModdedEquipmentList.Add(new RepairableEquipment { brokenEquipment = brokenEquipment,repairedEquipment = repairedEquipment,modName = modName});
        }

        internal static Dictionary<ItemIndex, ItemIndex> FillItemDictionaryFromMods(Dictionary<ItemIndex, ItemIndex> dictionary)
        {
            if (ModdedItemsList.Count == 0)
            {
                fillItemsDictionaryHandler?.Invoke();
            }

            foreach (var item in ModdedItemsList)
            {
                if (!dictionary.ContainsKey(item.brokenItem))
                {
                    dictionary.Add(item.brokenItem, item.repairedItem);
                    MyLogger.LogMessage(string.Format("Added item repairs from {0} to {1} from mod {2}.", ItemCatalog.GetItemDef(item.brokenItem)?.name, ItemCatalog.GetItemDef(item.repairedItem)?.name, item.modName));
                }
            }

            return dictionary;
        }
     
        internal static Dictionary<EquipmentIndex, EquipmentIndex> FillEquipmentDictionaryFromMods(Dictionary<EquipmentIndex, EquipmentIndex> dictionary)
        {
            if (ModdedEquipmentList.Count == 0)
            {
                fillEquipmentDictionaryHandler?.Invoke();
            }

            foreach (var item in ModdedEquipmentList)
            {
                if (!dictionary.ContainsKey(item.brokenEquipment))
                {
                    dictionary.Add(item.brokenEquipment, item.repairedEquipment);
                    MyLogger.LogMessage(string.Format("Added equipment repairs from {0} to {1} from mod {2}.", EquipmentCatalog.GetEquipmentDef(item.brokenEquipment)?.name, EquipmentCatalog.GetEquipmentDef(item.repairedEquipment)?.name, item.modName));
                }
            }

            return dictionary;
        }
    }
}
