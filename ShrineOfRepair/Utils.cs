using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
