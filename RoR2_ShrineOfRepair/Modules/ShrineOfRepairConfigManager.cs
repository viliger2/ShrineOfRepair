using BepInEx.Configuration;
using R2API;
using System.IO;
using static ShrineOfRepair.Modules.Interactables.ShrineOfRepairPurchase;

namespace ShrineOfRepair.Modules
{
    public class ShrineOfRepairConfigManager
    {
        // general values
        public static ConfigEntry<int> DirectorCost;
        public static ConfigEntry<int> DirectorWeight;
        public static ConfigEntry<DirectorAPI.InteractableCategory> DirectorCategory;

        public static ConfigEntry<bool> UseBadModel;

        public static ConfigEntry<string> RepairList;

        public static ConfigEntry<bool> UsePickupPickerPanel;
        public static ConfigEntry<bool> SpawnInBazaar;
        public static ConfigEntry<bool> SpawnInMoon;
        public static ConfigEntry<bool> UseLunarInMoon;
        public static ConfigEntry<int> MaxUses;

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
        public static ConfigEntry<int> PickerPanelGoldBossCost;
        public static ConfigEntry<int> PickerPanelGoldLunarCost;
        public static ConfigEntry<int> PickerPanelGoldEquipCost;

        public static ConfigEntry<float> BazaarLunarMultiplier;
        public static ConfigEntry<bool> PickerUseLunarByDefault;
        public static ConfigEntry<bool> PickerShowFree;

        public void Init(string configPath)
        {
            var mainConfig = new ConfigFile(Path.Combine(configPath, "viliger-ShrineOfRepair-General.cfg"), true);

            UseBadModel = mainConfig.Bind("Model", "Use Shitty Model", false, "Use shitty model that I made myself. If you want to see what bad modeling by bad programmer looks like - be my guest. I made it, so might as well put it here.");

            DirectorCost = mainConfig.Bind("Director", "Director Cost", 20, "Cost of the shrine in director credits. By defeult equal to the cost of most shrines.");
            DirectorWeight = mainConfig.Bind("Director", "Director Weight", 1, "Weight of the shrine for director. The lower the value, the more rare the shrine is. By default has the same weight as Shrine of Order, the only difference is that it can spawn anywhere.");
            DirectorCategory = mainConfig.Bind("Director", "Director Category", DirectorAPI.InteractableCategory.Shrines, "Category of interactable. If you change this, then you should also change Director Cost and Director Weight, as default values for those are balanced around it being spawned as a shrine.");

            RepairList = mainConfig.Bind("RepairList", "Repair List", "ExtraLifeConsumed - ExtraLife, ExtraLifeVoidConsumed - ExtraLifeVoid, FragileDamageBonusConsumed - FragileDamageBonus, HealingPotionConsumed - HealingPotion, RegeneratingScrapConsumed - RegeneratingScrap, BossHunterConsumed - BossHunter", 
                "Main Repair List, by default filled with pairs of breakable-original vanilla items, can be used to create custom pairs of brokenItem - repairedItem, including those from mods. Syntax: (broken) - (new), delimiter: ','");

            UsePickupPickerPanel = mainConfig.Bind("Interactable Type", "Use Scrapper-like variation", true, "Use scrapper-like variant, with separate cost for each broken item and ability to select what you want to repair. Scrapper-like variant only works with gold. Setting this to false will return the mod to its pre 1.2.0 function. Each variant has its own config file, AllInOne for pre-1.2.0 version and PerItem for newer.");

            SpawnInBazaar = mainConfig.Bind("General", "Spawn Shrine in Bazaar", false, "Spawn the shrine in the Bazaar Between Time.");
            SpawnInMoon = mainConfig.Bind("General", "Spawn Shrine in Moon", true, "Spawn the shrine in Commencement.");
            MaxUses = mainConfig.Bind("General", "Max Uses", 1, "Amount of times a single shrine can repair before deactivating. Set to 0 for infinite.");
            UseLunarInMoon = mainConfig.Bind("General", "Use Lunar Coins in Moon", false, "Make the Commencement shrine act like Bazaar shrine.");

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
            PickerPanelGoldTier3Cost = perItemConfig.Bind("Per Item Repairs", "Tier 3 cost", 200, "Base cost of tier 3 (red) item repair. By default the cost is equal to the half of legendary chest price.");
            PickerPanelGoldBossCost = perItemConfig.Bind("Per Item Repairs", "Boss cost", 50, "Base cost of boss (yellow) item repair. By default the cost is equal to double tier 2 repair price.");
            PickerPanelGoldLunarCost = perItemConfig.Bind("Per Item Repairs", "Lunar cost", 25, "Base cost of lunar (blue) item repair. By default the cost is equal to the half of large chest price.");
            PickerPanelGoldEquipCost = perItemConfig.Bind("Per Item Repairs", "Equipment cost", 50, "Base cost of equipments (orange) repair. By default the cost is equal to double tier 2 repair price.");

            BazaarLunarMultiplier = perItemConfig.Bind("Bazaar Shrines", "Bazaar Lunar Coin Multiplier", 0.04f, "Lunar Coin cost multiplier for Bazaar shrines. Disabled if currency type is not gold. 25 gold per 1 coin by default. set to 0 for free.");
            PickerUseLunarByDefault = perItemConfig.Bind("Bazaar Shrines", "Use Lunar Coins by Default", false, "Set to true to make every shrine act like Bazaar's shrines.");

            PickerShowFree = perItemConfig.Bind("Display", "Display Cost for Free", false, "Set to true to display $0 for free entries.");
        }

    }
}
