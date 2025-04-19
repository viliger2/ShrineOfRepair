using RoR2;
using ShrineOfRepair.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static ShrineOfRepair.Modules.ShrineOfRepairConfigManager;

namespace ShrineOfRepair.Behaviours
{
    public class ShrineOfRepairPurchaseManager : NetworkBehaviour
    {
        public PurchaseInteraction PurchaseInteraction;
        public float ScalingModifier;
        public bool UseDefaultScaling;

        [SyncVar]
        public int BaseCostDetermination;

        public int uses;

        public void Start()
        {
            if (NetworkServer.active && Run.instance)
            {
                PurchaseInteraction.SetAvailable(true);
            }

            PurchaseInteraction.onPurchase.AddListener(RepairPurchaseAttempt);

            if (PurchaseInteraction.costType == CostTypeIndex.Money)
            {
                var scene = SceneCatalog.GetSceneDefForCurrentScene();
                if ((BazaarUseLunar.Value && scene == SceneCatalog.GetSceneDefFromSceneName("bazaar"))
                    || (UseLunarInMoon.Value && ((scene == SceneCatalog.GetSceneDefFromSceneName("moon")) || (scene == SceneCatalog.GetSceneDefFromSceneName("moon2")))))
                {
                    PurchaseInteraction.costType = CostTypeIndex.LunarCoin;
                    PurchaseInteraction.automaticallyScaleCostWithDifficulty = false;
                    BaseCostDetermination = PurchaseInteractionLunarCoinCost.Value;
                    PurchaseInteraction.cost = BaseCostDetermination;
                }
                else
                {
                    BaseCostDetermination = UseDefaultScaling
                        ? (int)(PurchaseInteraction.cost * ScalingModifier)
                        : (int)(Mathf.Pow(Run.instance.compensatedDifficultyCoefficient, ScalingModifier) * PurchaseInteraction.cost);

                    PurchaseInteraction.cost = BaseCostDetermination;
                }
            }
            else
            {
                // probably for networking, I dunno
                BaseCostDetermination = PurchaseInteraction.cost;
                PurchaseInteraction.cost = BaseCostDetermination;
            }
            uses = 0;
        }

        [Server]
        public void RepairPurchaseAttempt(Interactor interactor)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (!interactor) { return; }
            var body = interactor.GetComponent<CharacterBody>();
            if (body && body.master)
            {
                var inventory = body.inventory;
                foreach (KeyValuePair<ItemIndex, ItemIndex> pairedItems in ShrineOfRepairDictionary.RepairItemsDictionary)
                {
                    int numberOfItems = inventory.GetItemCount(pairedItems.Key);
                    if (numberOfItems > 0)
                    {
                        inventory.RemoveItem(pairedItems.Key, numberOfItems);
                        inventory.GiveItem(pairedItems.Value, numberOfItems);
                        CharacterMasterNotificationQueue.SendTransformNotification(body.master, pairedItems.Key, pairedItems.Value, CharacterMasterNotificationQueue.TransformationType.Default);
                    }
                }
                if (ShrineOfRepairDictionary.RepairEquipmentsDictionary.ContainsKey(body.equipmentSlot.equipmentIndex))
                {
                    inventory.SetEquipmentIndex(ShrineOfRepairDictionary.RepairEquipmentsDictionary[body.equipmentSlot.equipmentIndex]);
                    CharacterMasterNotificationQueue.PushEquipmentTransformNotification(body.master, body.equipmentSlot.equipmentIndex, ShrineOfRepairDictionary.RepairEquipmentsDictionary[body.equipmentSlot.equipmentIndex], CharacterMasterNotificationQueue.TransformationType.Default);
                }

                EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData()
                {
                    origin = gameObject.transform.position,
                    rotation = Quaternion.identity,
                    scale = 1f,
                    color = (Color32)Color.red
                }, true);

                uses++;
                if (uses == MaxUses.Value)
                {
                    var billboard = gameObject.transform.Find("Symbol").gameObject;
                    billboard.SetActive(false);
                }

                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody = interactor.GetComponent<CharacterBody>(),
                    baseToken = "INTERACTABLE_SHRINE_REPAIR_INTERACT"
                });


                if (NetworkServer.active && uses == MaxUses.Value)
                {
                    PurchaseInteraction.SetAvailable(false);
                }
            }
        }
    }
}
