using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using static ShrineOfRepair.Modules.ShrineofRepairAssets;
using static ShrineOfRepair.Modules.ShrineOfRepairConfigManager;

namespace ShrineOfRepair.Modules.Interactables
{
    public abstract class ShrineOfRepairBase<T> : InteractableBase<T> where T : ShrineOfRepairBase<T>
    {
        public override string InteractableLangToken => "SHRINE_REPAIR";

        internal abstract GameObject CreateInteractable(GameObject InteractableModel);

        private static Dictionary<ItemIndex, ItemIndex> RepairItemsDictionary = new Dictionary<ItemIndex, ItemIndex>();

        public static Dictionary<ItemIndex, ItemIndex> FillRepairItemsDictionary()
        {
            if (RepairItemsDictionary.Count != 0) { return RepairItemsDictionary; }

            RepairItemsDictionary.Add(DLC1Content.Items.FragileDamageBonusConsumed.itemIndex, DLC1Content.Items.FragileDamageBonus.itemIndex); // watch
            RepairItemsDictionary.Add(DLC1Content.Items.HealingPotionConsumed.itemIndex, DLC1Content.Items.HealingPotion.itemIndex); // potion
            RepairItemsDictionary.Add(RoR2Content.Items.ExtraLifeConsumed.itemIndex, RoR2Content.Items.ExtraLife.itemIndex); // dio
            RepairItemsDictionary.Add(DLC1Content.Items.ExtraLifeVoidConsumed.itemIndex, DLC1Content.Items.ExtraLifeVoid.itemIndex); // larva

            // ItemIndex enums are index numbers from "Item & Equipment IDs and Names" page
            var itemIds = Blacklist.Value.Split(',');
            foreach (var itemId in itemIds)
            {
                ItemIndex itemIndex;
                if (System.Enum.TryParse(itemId.Trim(), out itemIndex))
                {
                    RepairItemsDictionary.Remove(itemIndex);
                }
            }

            RepairItemsDictionary = ModExtension.FillDictionaryFromMods(RepairItemsDictionary);

            return RepairItemsDictionary;
        }

        internal void CreateInteractables()
        {
            var normalModel = CreateInteractable(UseBadModel.Value ? MainBundle.LoadAsset<GameObject>("ShrineRepair.prefab") : MainBundle.LoadAsset<GameObject>("ShrineRepairGood.prefab"));
            var sandyModel = CreateInteractable(UseBadModel.Value ? MainBundle.LoadAsset<GameObject>("ShrineRepair.prefab") : MainBundle.LoadAsset<GameObject>("ShrineRepairGood_sandy.prefab"));
            var snowyModel = CreateInteractable(UseBadModel.Value ? MainBundle.LoadAsset<GameObject>("ShrineRepair.prefab") : MainBundle.LoadAsset<GameObject>("ShrineRepairGood_snowy.prefab"));

            CreateInteractableSpawnCard(normalModel, "iscShrineRepair", GetNormalStageList());
            CreateInteractableSpawnCard(sandyModel, "iscShrineRepairSandy", GetSandyStageList());
            CreateInteractableSpawnCard(snowyModel, "iscShrineRepairSnowy", GetSnowyStageList());
        }

        private void CreateInteractableSpawnCard(GameObject InteractableModel, string name, List<DirectorAPI.Stage> stageList)
        {
            InteractableSpawnCard interactableSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            interactableSpawnCard.name = name;
            interactableSpawnCard.prefab = InteractableModel;
            interactableSpawnCard.sendOverNetwork = true;
            interactableSpawnCard.hullSize = HullClassification.Golem;
            interactableSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            interactableSpawnCard.requiredFlags = RoR2.Navigation.NodeFlags.None;
            interactableSpawnCard.forbiddenFlags = RoR2.Navigation.NodeFlags.NoShrineSpawn;
            interactableSpawnCard.directorCreditCost = DirectorCost.Value;
            interactableSpawnCard.occupyPosition = true;
            interactableSpawnCard.orientToFloor = false;
            interactableSpawnCard.skipSpawnWhenSacrificeArtifactEnabled = false;

            DirectorCard directorCard = new DirectorCard
            {
                selectionWeight = DirectorWeight.Value,
                spawnCard = interactableSpawnCard,
            };

            foreach (DirectorAPI.Stage stage in stageList)
            {
                DirectorAPI.Helpers.AddNewInteractableToStage(directorCard, DirectorCategory.Value, stage);
            }

        }

        private List<DirectorAPI.Stage> GetNormalStageList()
        {
            List<DirectorAPI.Stage> stageList = new List<DirectorAPI.Stage>();

            stageList.Add(DirectorAPI.Stage.DistantRoost);
            stageList.Add(DirectorAPI.Stage.AbyssalDepths);
            stageList.Add(DirectorAPI.Stage.TitanicPlains);
            stageList.Add(DirectorAPI.Stage.SunderedGrove);
            stageList.Add(DirectorAPI.Stage.SirensCall);
            stageList.Add(DirectorAPI.Stage.SkyMeadow);
            stageList.Add(DirectorAPI.Stage.SulfurPools);
            stageList.Add(DirectorAPI.Stage.ScorchedAcres);
            stageList.Add(DirectorAPI.Stage.AphelianSanctuary);

            stageList.Add(DirectorAPI.Stage.AbyssalDepthsSimulacrum);
            stageList.Add(DirectorAPI.Stage.TitanicPlainsSimulacrum);
            stageList.Add(DirectorAPI.Stage.SkyMeadowSimulacrum);
            stageList.Add(DirectorAPI.Stage.AphelianSanctuarySimulacrum);

            return stageList;

        }

        private List<DirectorAPI.Stage> GetSnowyStageList()
        {
            List<DirectorAPI.Stage> stageList = new List<DirectorAPI.Stage>();

            stageList.Add(DirectorAPI.Stage.SiphonedForest);
            stageList.Add(DirectorAPI.Stage.RallypointDelta);

            stageList.Add(DirectorAPI.Stage.RallypointDeltaSimulacrum);

            return stageList;

        }

        private List<DirectorAPI.Stage> GetSandyStageList()
        {
            List<DirectorAPI.Stage> stageList = new List<DirectorAPI.Stage>();

            stageList.Add(DirectorAPI.Stage.AbandonedAqueduct);

            stageList.Add(DirectorAPI.Stage.AbandonedAqueductSimulacrum);

            return stageList;

        }

    }
}
