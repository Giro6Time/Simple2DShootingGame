using UnityEngine;
namespace GiroFrame
{
    public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
    {
        public static T Instance;
        protected virtual void Awake()
        {
            Instance = this as T;
        }
    }
}