using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace ShrineOfRepair
{
    public static class Utils
    {
        public static void AddPersistentListener<T>(this UnityEvent<T> unityEvent, UnityAction<T> action)
        {
            unityEvent.m_PersistentCalls.AddListener(new PersistentCall
            {
                m_Target = action.Target as UnityEngine.Object,
                m_TargetAssemblyTypeName = UnityEventTools.TidyAssemblyTypeName(action.Method.DeclaringType.AssemblyQualifiedName),
                m_MethodName = action.Method.Name,
                m_CallState = UnityEventCallState.RuntimeOnly,
                m_Mode = PersistentListenerMode.EventDefined,
            });
        }

        public static void AddPersistentListener<T0, T1>(this UnityEvent<T0, T1> unityEvent, UnityAction<T0, T1> action)
        {
            unityEvent.m_PersistentCalls.AddListener(new PersistentCall
            {
                m_Target = action.Target as UnityEngine.Object,
                m_TargetAssemblyTypeName = UnityEventTools.TidyAssemblyTypeName(action.Method.DeclaringType.AssemblyQualifiedName),
                m_MethodName = action.Method.Name,
                m_CallState = UnityEventCallState.RuntimeOnly,
                m_Mode = PersistentListenerMode.EventDefined,
            });
        }

        public static Sprite GetIconSprite(this ItemDef itemDef)
        { 
            if(itemDef.pickupIconSpriteReference != null && itemDef.pickupIconSpriteReference.RuntimeKeyIsValid())
#pragma warning disable CS0618 // Type or member is obsolete
            {
                return Addressables.LoadAssetAsync<Sprite>(itemDef.pickupIconSpriteReference.AssetGUID).WaitForCompletion();
            } else if(itemDef.pickupIconSprite != null)
            {
                return itemDef.pickupIconSprite;
            } else
            {
                return null;
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public static Sprite GetIconSprite(this EquipmentDef itemDef)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (itemDef.pickupIconSpriteReference != null && itemDef.pickupIconSpriteReference.RuntimeKeyIsValid())
            {
                return Addressables.LoadAssetAsync<Sprite>(itemDef.pickupIconSpriteReference.AssetGUID).WaitForCompletion();
            }
            else if (itemDef.pickupIconSprite != null)
            {
                return itemDef.pickupIconSprite;
            }
            else
            {
                return null;
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public static int GetTotalRentedItemCount(this Inventory inventory)
        {
            return inventory.rentalItemStacks.GetTotalItemStacks();
        }

    }
}
