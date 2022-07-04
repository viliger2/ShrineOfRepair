using BepInEx.Configuration;
using R2API;
using static ShrineOfRepair.Modules.Interactables.ShrineOfRepairPurchase;
using System.IO;

namespace ShrineOfRepair.Modules
{
    public static class ShrineOfRepairConfigManager
    {

        // general values
        public static ConfigEntry<int> DirectorCost;
        public static ConfigEntry<int> DirectorWeight;
        public static ConfigEntry<DirectorAPI.InteractableCategory> DirectorCategory;

        public static ConfigEntry<bool> UseBadModel;

        public static ConfigEntry<string> Blacklist;

        public static ConfigEntry<bool> UsePickupPickerPanel;

        // for PurchaseInteraction
        public static ConfigEntry<CostTypes> PurchaseInteractionCurrencyType;

        public static ConfigEntry<int> PurchaseInteractionLunarCoinCost;

        public static ConfigEntry<int> PurchaseInteractionVoidCoinCost;

        public static ConfigEntry<int> PurchaseInteractionGoldBaseCost;
        public static ConfigEntry<bool> PurchaseInteractionGoldUseDefaultScaling;
        public static ConfigEntry<float> PurchaseInteractionGoldScalingModifier;

        // for PickupPickerPanel
        public static ConfigEntry<int> PickerPanelGoldTier1Cost;
        public static ConfigEntry<int> PickerPanelGoldTier2Cost;
        public static ConfigEntry<int> PickerPanelGoldTier3Cost;

        public static void Init(string configPath)
        {
            var mainConfig = new ConfigFile(Path.Combine(configPath, "viliger-ShrineOfRepair-General.cfg"), true);

            UseBadModel = mainConfig.Bind("Model", "Use Shitty Model", false, "Use shitty model that I made myself. If you want to see what bad modeling by bad programmer looks like - be my guest. I made it, so might as well put it here.");

            DirectorCost = mainConfig.Bind("Director", "Director Cost", 20, "Cost of the shrine in director credits. By defeult equal to the cost of most shrines.");
            DirectorWeight = mainConfig.Bind("Director", "Director Weight", 1, "Weight of the shrine for director. The lower the value, the more rare the shrine is. By default has the same weight as Shrine of Order, the only difference is that it can spawn anywhere.");
            DirectorCategory = mainConfig.Bind("Director", "Director Category", DirectorAPI.InteractableCategory.Shrines, "Category of interactable. If you change this, then you should also change Director Cost and Director Weight, as default values for those are balanced around it being spawned as a shrine.");

            Blacklist = mainConfig.Bind("Blacklist", "Blacklist", "", "Blacklist for items. Adding an item to the list will make them unrepairable. List should consists of item ids separated by commas and everything else will be (hopefully) ignored. IDs for Hoopo items:\nDelicate Watch (Broken) - 68, Empty Bottle - 81, Dio's Best Friend (Consumed) - 57, Pluripotent Larva (Consumed) - 59");

            UsePickupPickerPanel = mainConfig.Bind("Interactable Type", "Use Scrapper-like variation", true, "Use scrapper-like variant, with separate cost for each broken item and ability to select what you want to repair. Scrapper-like variant only works with gold. Setting this to false will return the mod to its pre 1.2.0 function. Each variant has its own config file, AllInOne for pre-1.2.0 version and PerItem for newer.");

            var allInOneConfig = new ConfigFile(Path.Combine(configPath, "viliger-ShrineOfRepair-AllInOne.cfg"), true);

            PurchaseInteractionCurrencyType = allInOneConfig.Bind("Currency", "Currency Type", CostTypes.Gold, "Type of currency used to purchase shrine. Using anything other than \"Gold\" disables price scaling over time. Each currency has its own options.");

            PurchaseInteractionLunarCoinCost = allInOneConfig.Bind("Lunar Coins", "Shrine Base Cost", 2, "Base cost of the interactable in lunar coins. Does not scale with time. Can be used with EphemeralCoins.");

            PurchaseInteractionVoidCoinCost = allInOneConfig.Bind("Void Coins", "Shrine Base Cost", 2, "Base cost of the interactable in void coins. Does not scale with time. To be used with ReleasedFromTheVoid.");

            PurchaseInteractionGoldBaseCost = allInOneConfig.Bind("Gold", "Shrine Base Cost", 12, "Base cost of the interactable in gold that is used for scaling. Will spawn with this cost at the start of the run.");
            PurchaseInteractionGoldUseDefaultScaling = allInOneConfig.Bind("Gold", "Use Default Scaling", false, "Use default scaling formula instead of custom scaling formula for the shrine. Custom formula is diffCoef^customsScalingModifier * BaseCost, default formula is diffCoef^1.25 * BaseCost * ScalingModifier");
            PurchaseInteractionGoldScalingModifier = allInOneConfig.Bind("Gold", "Scaling Modifier", 1.35f, "Used for defining how cost of shrine scales throughout the run for both default and custom scaling formulas.");

            var perItemConfig = new ConfigFile(Path.Combine(configPath, "viliger-ShrineOfRepair-PerItem.cfg"), true);

            PickerPanelGoldTier1Cost = perItemConfig.Bind("Per Item Repairs", "Tier 1 cost", 12, "Base cost of tier 1 (white) item repair. By default the cost is equal to the half of normal chest price, rounded down.");
            PickerPanelGoldTier2Cost = perItemConfig.Bind("Per Item Repairs", "Tier 2 cost", 25, "Base cost of tier 2 (green) item repair. By default the cost is equal to the half of large chest price.");
            PickerPanelGoldTier3Cost = perItemConfig.Bind("Per Item Repairs", "Tier 3 cost", 200, "Base cost of tier 3 (red) item repair. By default the cost is eqyal to the half of legendary chest price.");
        }

    }
}
