using BepInEx.Configuration;
using R2API;
using ShrineOfRepair.ModCompat;
using System.IO;
using UnityEngine;
using static ShrineOfRepair.ShrineOfRepairStuff;

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

        public static ConfigEntry<Vector3> BazaarPosition;
        public static ConfigEntry<Vector3> BazaarAngle;

        public static ConfigEntry<Vector3> MoonPosition;
        public static ConfigEntry<Vector3> MoonAngle;

        public static ConfigEntry<Vector3> Moon2Position;
        public static ConfigEntry<Vector3> Moon2Angle;

        public static ConfigEntry<bool> RepairVoidItems;
        public static ConfigEntry<bool> RepairTempItems;

        // for PurchaseInteraction
        public static ConfigEntry<CostTypes> PurchaseInteractionCurrencyType;

        public static ConfigEntry<int> PurchaseInteractionLunarCoinCost;

        public static ConfigEntry<int> PurchaseInteractionVoidCoinCost;

        public static ConfigEntry<int> PurchaseInteractionGoldBaseCost;
        public static ConfigEntry<bool> PurchaseInteractionGoldUseDefaultScaling;
        public static ConfigEntry<float> PurchaseInteractionGoldScalingModifier;

        // for PickupPickerPanel
        public static ConfigEntry<CostTypes> PickerInteractionCurrencyType;
        public static ConfigEntry<float> PickerVoidCoinMultiplier;

        public static ConfigEntry<int> PickerPanelGoldTier1Cost;
        public static ConfigEntry<int> PickerPanelGoldTier2Cost;
        public static ConfigEntry<int> PickerPanelGoldTier3Cost;
        public static ConfigEntry<int> PickerPanelGoldBossCost;
        public static ConfigEntry<int> PickerPanelGoldLunarCost;
        public static ConfigEntry<int> PickerPanelGoldEquipCost;

        public static ConfigEntry<int> PickerPanelGoldTempTier1Cost;
        public static ConfigEntry<int> PickerPanelGoldTempTier2Cost;
        public static ConfigEntry<int> PickerPanelGoldTempTier3Cost;
        public static ConfigEntry<int> PickerPanelGoldTempTierBossCost;
        public static ConfigEntry<int> PickerPanelGoldTempLunarCost;

        public static ConfigEntry<bool> BazaarUseLunar;
        public static ConfigEntry<float> PickerLunarCoinMultiplier;
        //public static ConfigEntry<bool> PickerUseLunarByDefault;
        public static ConfigEntry<bool> PickerShowFree;

        public void Init(string configPath)
        {
            var mainConfig = new ConfigFile(Path.Combine(configPath, "viliger-ShrineOfRepair-General.cfg"), true);

            UseBadModel = mainConfig.Bind("Model", "Use Shitty Model", false, "Use shitty model that I made myself. If you want to see what bad modeling by bad programmer looks like - be my guest. I made it, so might as well put it here.");

            DirectorCost = mainConfig.Bind("Director", "Director Cost", 20, "Cost of the shrine in director credits. By defeult equal to the cost of most shrines.");
            DirectorWeight = mainConfig.Bind("Director", "Director Weight", 1, "Weight of the shrine for director. The lower the value, the more rare the shrine is. By default has the same weight as Shrine of Order, the only difference is that it can spawn anywhere.");
            DirectorCategory = mainConfig.Bind("Director", "Director Category", DirectorAPI.InteractableCategory.Shrines, "Category of interactable. If you change this, then you should also change Director Cost and Director Weight, as default values for those are balanced around it being spawned as a shrine.");

            RepairList = mainConfig.Bind("RepairList", "Repair List", 
                "ExtraLifeConsumed - ExtraLife, " +
                "ExtraLifeVoidConsumed - ExtraLifeVoid, " +
                "FragileDamageBonusConsumed - FragileDamageBonus, " +
                "HealingPotionConsumed - HealingPotion, " +
                "RegeneratingScrapConsumed - RegeneratingScrap, " +
                "BossHunterConsumed - BossHunter, " +
                "LowerPricedChestsConsumed - LowerPricedChests, " +
                "TeleportOnLowHealthConsumed - TeleportOnLowHealth, " +
                "HealAndReviveConsumed - HealAndRevive",
                "Main Repair List, by default filled with pairs of breakable-original vanilla items, can be used to create custom pairs of brokenItem - repairedItem, including those from mods. Syntax: (broken) - (new), delimiter: ','");

            UsePickupPickerPanel = mainConfig.Bind("Interactable Type", "Use Scrapper-like variation", true, "Use scrapper-like variant, with separate cost for each broken item and ability to select what you want to repair. Scrapper-like variant only works with gold. Setting this to false will return the mod to its pre 1.2.0 function. Each variant has its own config file, AllInOne for pre-1.2.0 version and PerItem for newer.");

            MaxUses = mainConfig.Bind("General", "Max Uses", 1, "Amount of times a single shrine can repair before deactivating. Set to 0 for infinite.");
            UseLunarInMoon = mainConfig.Bind("General", "Use Lunar Coins in Moon", false, "Make the Commencement shrine act like Bazaar shrine.");

            SpawnInBazaar = mainConfig.Bind("Bazaar", "Spawn Shrine in Bazaar", false, "Spawn the shrine in the Bazaar Between Time.");
            BazaarPosition = mainConfig.Bind("Bazaar", "Shrine Position in Bazaar", new Vector3(-139.5f, -25.5f, -19.9f), "Position of the shrine in the Bazaar Between Time");
            BazaarAngle = mainConfig.Bind("Bazaar", "Shrine Angle in Bazaar", new Vector3(0f, 0f, 0f), "Angle (rotation) of the shrine in the Bazaar Between Time");

            SpawnInMoon = mainConfig.Bind("Commencement", "Spawn Shrine in Moon", true, "Spawn the shrine in Commencement.");
            MoonPosition = mainConfig.Bind("Commencement", "Shrine Position in Commencement (pre-Aniversary)", new Vector3(749.4f, 253f, -244.3f), "Position of the shrine in Commencement (pre-Aniversary)");
            MoonAngle = mainConfig.Bind("Commencement", "Shrine Angle in Commencement (pre-Aniversary)", new Vector3(0f, 143.2f, 0f), "Angle (rotation) of the shrine in Commencement (pre-Aniversary)");

            Moon2Position = mainConfig.Bind("Commencement", "Shrine Position in Commencement", new Vector3(-3.9f, -150.6f, -331.2f), "Position of the shrine in Commencement");
            Moon2Angle = mainConfig.Bind("Commencement", "Shrine Angle in Commencement", new Vector3(-70f, 164f, 0f), "Angle (rotation) of the shrine in Commencement");

            RepairVoidItems = mainConfig.Bind("Void Items", "Repair Void Items", true, "Enables the ability to repair void items back into their normal versions. If void item has multiple normal versions then it is skipped (like VoidRing or VoidBossItem). Items are filled dynamically from list of contagious items, so you don't need to fill the dictionary manually like with other breakable items.");

            RepairTempItems = mainConfig.Bind("Temp Items", "Repair Temp Items", true, "Enables the ability to repair temporary items. Repair items use their own gold cost values, usually equal to that of their respective chest.");

            var allInOneConfig = new ConfigFile(Path.Combine(configPath, "viliger-ShrineOfRepair-AllInOne.cfg"), true);

            PurchaseInteractionCurrencyType = allInOneConfig.Bind("Currency", "Currency Type", CostTypes.Gold, "Type of currency used to purchase shrine. Using anything other than \"Gold\" disables price scaling over time. Each currency has its own options.");

            PurchaseInteractionLunarCoinCost = allInOneConfig.Bind("Lunar Coins", "Shrine Base Cost", 2, "Base cost of the interactable in lunar coins. Does not scale with time. Can be used with EphemeralCoins.");

            PurchaseInteractionVoidCoinCost = allInOneConfig.Bind("Void Coins", "Shrine Base Cost", 2, "Base cost of the interactable in void coins. Does not scale with time. To be used with ReleasedFromTheVoid.");

            PurchaseInteractionGoldBaseCost = allInOneConfig.Bind("Gold", "Shrine Base Cost", 12, "Base cost of the interactable in gold that is used for scaling. Will spawn with this cost at the start of the run.");
            PurchaseInteractionGoldUseDefaultScaling = allInOneConfig.Bind("Gold", "Use Default Scaling", false, "Use default scaling formula instead of custom scaling formula for the shrine. Custom formula is diffCoef^customsScalingModifier * BaseCost, default formula is diffCoef^1.25 * BaseCost * ScalingModifier");
            PurchaseInteractionGoldScalingModifier = allInOneConfig.Bind("Gold", "Scaling Modifier", 1.35f, "Used for defining how cost of shrine scales throughout the run for both default and custom scaling formulas.");

            var perItemConfig = new ConfigFile(Path.Combine(configPath, "viliger-ShrineOfRepair-PerItem.cfg"), true);

            PickerInteractionCurrencyType = perItemConfig.Bind("Currency", "Currency Type", CostTypes.Gold, "Type of currency used to purchase shrine. Using anything other than \"Gold\" disables price scaling over time. Each currency has its own options.");
            PickerVoidCoinMultiplier = perItemConfig.Bind("Currency", "Void Coin Multiplier", 0.04f, "Void Coin cost multiplier. Disabled if currency type is not gold. 25 gold per 1 coin by default. set to 0 for free.");
            PickerLunarCoinMultiplier = perItemConfig.Bind("Currency", "Lunar Coin Multiplier", 0.04f, "Lunar Coin cost multiplier. Disabled if currency type is not gold. 25 gold per 1 coin by default. set to 0 for free.");

            PickerPanelGoldTier1Cost = perItemConfig.Bind("Per Item Repairs", "Tier 1 cost", 12, "Base cost of tier 1 (white) item repair. By default the cost is equal to the half of normal chest price, rounded down.");
            PickerPanelGoldTier2Cost = perItemConfig.Bind("Per Item Repairs", "Tier 2 cost", 25, "Base cost of tier 2 (green) item repair. By default the cost is equal to the half of large chest price.");
            PickerPanelGoldTier3Cost = perItemConfig.Bind("Per Item Repairs", "Tier 3 cost", 200, "Base cost of tier 3 (red) item repair. By default the cost is equal to the half of legendary chest price.");
            PickerPanelGoldBossCost = perItemConfig.Bind("Per Item Repairs", "Boss cost", 50, "Base cost of boss (yellow) item repair. By default the cost is equal to double tier 2 repair price.");
            PickerPanelGoldLunarCost = perItemConfig.Bind("Per Item Repairs", "Lunar cost", 25, "Base cost of lunar (blue) item repair. By default the cost is equal to the half of large chest price.");
            PickerPanelGoldEquipCost = perItemConfig.Bind("Per Item Repairs", "Equipment cost", 50, "Base cost of equipments (orange) repair. By default the cost is equal to double tier 2 repair price.");

            PickerPanelGoldTempTier1Cost = perItemConfig.Bind("Temp Items", "Tier 1 cost", 25, "Base cost of temporary tier 1 (white) item repair. By default the cost is equal to the normal chest price.");
            PickerPanelGoldTempTier2Cost = perItemConfig.Bind("Temp Items", "Tier 2 cost", 50, "Base cost of temporary tier 2 (green) item repair. By default the cost is equal to the large chest price.");
            PickerPanelGoldTempTier3Cost = perItemConfig.Bind("Temp Items", "Tier 3 cost", 400, "Base cost of temporary tier 3 (red) item repair. By default the cost is equal to the legenrady chest price.");
            PickerPanelGoldTempTierBossCost = perItemConfig.Bind("Temp Items", "Boss Tier cost", 100, "Base cost of temporary boss tier (yellow) item repair. By default the cost is equal to the double large chest price.");
            PickerPanelGoldTempLunarCost = perItemConfig.Bind("Temp Items", "Lunar Tier Cost", 50, "Base cost of temporary lunar (blue) item repair. By default the cost is equal to thelarge chest price.");

            BazaarUseLunar = perItemConfig.Bind("Bazaar Shrines", "Use Lunar Coins in Bazaar", true, "Shrine spawned in Bazaar uses lunar coins. If disabled it will use gold instead.");

            PickerShowFree = perItemConfig.Bind("Display", "Display Cost for Free", false, "Set to true to display $0 for free entries.");

            if (RiskOfOptionsCompat.enabled)
            {
                RiskOfOptionsCompat.SetDescription();

                RiskOfOptionsCompat.CreateNewOption(PickerPanelGoldTier1Cost);
                RiskOfOptionsCompat.CreateNewOption(PickerPanelGoldTier2Cost);
                RiskOfOptionsCompat.CreateNewOption(PickerPanelGoldTier3Cost, 0, 400);
                RiskOfOptionsCompat.CreateNewOption(PickerPanelGoldBossCost);
                RiskOfOptionsCompat.CreateNewOption(PickerPanelGoldLunarCost);
                RiskOfOptionsCompat.CreateNewOption(PickerPanelGoldEquipCost);

                RiskOfOptionsCompat.CreateNewOption(PickerLunarCoinMultiplier);

                RiskOfOptionsCompat.CreateNewOption(PickerShowFree);

                RiskOfOptionsCompat.CreateNewOption(MaxUses, 0, 50);
                RiskOfOptionsCompat.CreateNewOption(SpawnInBazaar);
                RiskOfOptionsCompat.CreateNewOption(SpawnInMoon);
                RiskOfOptionsCompat.CreateNewOption(BazaarUseLunar);
            }

        }
    }
}
