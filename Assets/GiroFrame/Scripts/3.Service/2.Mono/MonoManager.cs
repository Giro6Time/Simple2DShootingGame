using System;
namespace GiroFrame
{
    public class MonoManager : ManagerBase<MonoManager>
    {
        private Action updateEvent;
        private Action lateUpdateEvent;
        private Action fixedUpdateEvent;
        /// <summary>
        ///添加Update监听 
        /// </summary>
        public void AddUpdateListener(Action action)
        {
            updateEvent += action;
        }
        /// <summary>
        ///移除Update监听 
        /// </summary>
        public void RemoveUpdateListener(Action action)
        {
            updateEvent -= action;
        }
        /// <summary>
        ///添加LateUpdate监听 
        /// </summary>
        public void AddLateUpdateListener(Action action)
        {
            lateUpdateEvent += action;
        }
        /// <summary>
        ///移除LateUpdate监听 
        /// </summary>
        public void RemoveLateUpdateListener(Action action)
        {
            lateUpdateEvent -= action;
        }
        /// <summary>
        ///添加FixedUpdate监听 
        /// </summary>
        public void AddFixedUpdateListener(Action action)
        {
            fixedUpdateEvent += action;
        }
        /// <summary>
        ///移除FixedUpdate监听 
        /// </summary>
        public void RemoveFixedUpdateListener(Action action)
        {
            fixedUpdateEvent -= action;
        }

        void Update()
        {
            updateEvent?.Invoke();
        }
        void LateUpdate()
        {
            lateUpdateEvent?.Invoke();
        }
        void FixedUpdate()
        {
            fixedUpdateEvent?.Invoke();
        }


    }
}
