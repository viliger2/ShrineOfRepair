using RoR2;
using System.Collections.Generic;
using static ShrineOfRepair.Modules.ShrineOfRepairPlugin;

namespace ShrineOfRepair.Modules
{
    public class ModExtension
    {
        public delegate void DictionaryFillDelegate(ref List<RepairableItems> list);

        public class RepairableItems
        {
            public ItemIndex brokenItem;
            public ItemIndex repairedItem;
        }

        private static DictionaryFillDelegate fillDictionaryHandler;

        private static List<RepairableItems> ModdedItemsList = new List<RepairableItems>();

        public static void AddListener(DictionaryFillDelegate callback)
        {
            fillDictionaryHandler += callback;
            MyLogger.LogMessage($"Added {callback.Method} to dictionaryFillHandler");
        }

        public static Dictionary<ItemIndex, ItemIndex> FillDictionaryFromMods(Dictionary<ItemIndex, ItemIndex> dictionary)
        {
            if (ModdedItemsList.Count == 0)
            {
                fillDictionaryHandler?.Invoke(ref ModdedItemsList);
            }

            foreach (var item in ModdedItemsList)
            {
                if (!dictionary.ContainsKey(item.brokenItem))
                {
                    dictionary.Add(item.brokenItem, item.repairedItem);
                    MyLogger.LogMessage(string.Format("Added repairs from {0} to {1} from a mod.", ItemCatalog.GetItemDef(item.brokenItem)?.name, ItemCatalog.GetItemDef(item.repairedItem).name));
                }
            }

            return dictionary;
        }
    }
}
