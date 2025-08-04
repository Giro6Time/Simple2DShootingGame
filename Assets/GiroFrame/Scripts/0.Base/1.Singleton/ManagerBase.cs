using UnityEngine;

namespace GiroFrame
{
    public abstract class ManagerBase : MonoBehaviour
    {
        public virtual void Init()
        { }
    }
    public abstract class ManagerBase<T> : ManagerBase where T : ManagerBase<T>
    {
        public static T Instance;
        public override void Init()
        {
            Instance = this as T; 
        }
    }
}