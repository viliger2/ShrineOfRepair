using System;

namespace ShrineOfRepair.Modules.Interactables
{

    public abstract class InteractableBase<T> : InteractableBase where T : InteractableBase<T>
    {
        public static T instance { get; private set; }

        public InteractableBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).FullName + "\" inhereting InteractableBase was instanced twice");
            instance = this as T;
        }
    }

    public abstract class InteractableBase
    {
        public abstract string InteractableLangToken { get; }

        public abstract void Init();

    }

}
