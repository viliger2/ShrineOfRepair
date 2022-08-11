using RoR2;
using System.Collections.Generic;
using static ShrineOfRepair.Modules.ShrineOfRepairPlugin;

namespace ShrineOfRepair.Modules
{
    public class ModExtension
    {
        // if you want to add your own item just call AddListener with a method
        // that has ref Dictionary<ItemIndex, ItemIndex> as an only parameter
        // in method itself you can add itemIndexes of items you want to repair from and into
        // as key and value respectively, and don't forget to check for duplicates in dictionary,
        // since I am not sure at the moment on how you can implement it here
        public delegate void DictionaryFillDelegate(ref Dictionary<ItemIndex, ItemIndex> dict);

        private static DictionaryFillDelegate dictionaryFillHandler;

        private static Dictionary<ItemIndex, ItemIndex> ModdedItemsDictionary = new Dictionary<ItemIndex, ItemIndex>();

        public static void AddListener(DictionaryFillDelegate callback)
        {
            dictionaryFillHandler += callback;
            MyLogger.LogMessage($"Added {callback.Method} to dictionaryFillHandler");
        }

        public static Dictionary<ItemIndex, ItemIndex> FillDictionaryFromMods(Dictionary<ItemIndex, ItemIndex> dictionary)
        {
            dictionaryFillHandler(ref ModdedItemsDictionary);

            foreach (var item in ModdedItemsDictionary)
            {
                if (!dictionary.ContainsKey(item.Key))
                {
                    dictionary.Add(item.Key, item.Value);
                }
            }

            return dictionary;
        }
    }
}
