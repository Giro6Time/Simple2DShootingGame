using System.Collections.Generic;
using UnityEngine;
using System;

namespace GiroFrame
{
    public class PoolManager : ManagerBase<PoolManager>
    {
        //根节点
        [SerializeField]
        private GameObject poolRootObj;

        //GameObject对象容器
        public Dictionary<string, GameObjectPoolData> gameObjectPoolDic;

        //普通对象容器
        public Dictionary<string, ObjectPoolData> objectPoolDic;
        public override void Init()
        {
            gameObjectPoolDic = new Dictionary<string, GameObjectPoolData>();
            objectPoolDic = new Dictionary<string, ObjectPoolData>();
            base.Init();

        }



        #region GameObject对象相关操作
        /// <summary>
        /// 获取GameObject
        /// </summary>
        /// <param name="prefab">最终需要的组件</param>
        public T GetGameObject<T>(GameObject prefab, Transform parent = null) where T : UnityEngine.Object
        {
            GameObject obj = GetGameObject(prefab, parent);
            if (obj != null)
            {
                return obj.GetComponent<T>();
            }
            return null;
        }

        /// <summary>
        /// 获取字典中的GameObject（通过prefab的name，同时确认他的怕parent） 
        /// </summary>
        public GameObject GetGameObject(GameObject prefab, Transform parent = null)
        {
            GameObject obj = null;
            string name = prefab.name;
            //检查字典中是否储存了此类物体
            if (CheckGameObjectCache(prefab))//有则直接返回这个物体
            {
                obj = gameObjectPoolDic[name].GetObj(parent);
                return obj;
            }
            else//没有则实例化一个此类物体
            {
                //确保实例化的物体名字相同
                obj = GameObject.Instantiate(prefab, parent);
                obj.SetActive(true);
                obj.name = name;

            }
            return obj;
        }

        /// <summary>
        /// 把实例放入对象池
        /// </summary>
        public void PushGameObject(GameObject obj)
        {
            string name = obj.name;
            if (gameObjectPoolDic.ContainsKey(name))
            {
                gameObjectPoolDic[name].PushObj(obj);
            }
            else
            {
                gameObjectPoolDic.Add(name, new GameObjectPoolData(obj, poolRootObj));
            }
        }

        /// <summary>
        /// 检查有没有某一层对象池数据
        /// </summary>
        private bool CheckGameObjectCache(GameObject prefab)
        {
            string name = prefab.name;
            return gameObjectPoolDic.ContainsKey(name) && gameObjectPoolDic[name].poolQueue.Count > 0;
        }

        #endregion
        #region Object对象相关操作
        /// <summary>
        /// 获取object字典中的object（通过查找T的FullName）
        /// </summary>
        public T GetObject<T>() where T : class, new()
        {
            T obj;
            /* foreach (var t in objectPoolDic.Values)
            {
                Debug.Log(t + " GetObject前 " + t.poolQueue.Count);
            } */

            if (CheckObjectCache<T>())
            {
                string name = typeof(T).FullName;
                obj = (T)objectPoolDic[name].GetObj();

                /* foreach (var t in objectPoolDic.Values)
                {
                    Debug.Log(t + " GetObject后 " + t.poolQueue.Count);
                } */
                return obj;
            }
            else
            {
                /* foreach (var t in objectPoolDic.Values)
                {
                    Debug.Log(t + " GetObject前 " + t.poolQueue.Count);
                } */
                return new T();
            }
        }
        /// <summary>
        /// 把object放入对象池
        /// </summary>
        public void PushObject(object obj)
        {
            string name = obj.GetType().FullName;
            /* foreach (var t in objectPoolDic.Values)
            {
                Debug.Log(t + " PushObject前 " + t.poolQueue.Count);
            } */
            if (objectPoolDic.ContainsKey(name))
            {
                objectPoolDic[name].PushObj(obj);
            }
            else
            {
                objectPoolDic.Add(name, new ObjectPoolData(obj));
            }
            /* foreach (var t in objectPoolDic.Values)
            {
                Debug.Log(t + " PushObject后 " + t.poolQueue.Count);
            } */
        }
        /// <summary>
        /// 检查有没有某一层对象池数据
        /// </summary>
        protected bool CheckObjectCache<T>()
        {
            string name = typeof(T).FullName;
            return objectPoolDic.ContainsKey(name) && objectPoolDic[name].poolQueue.Count > 0;
        }
        /// <summary> 检查缓存 如果成功 加载游戏物体 否则返回NULL
        /// </summary>
        /// <returns></returns>
        public GameObject CheckCacheAndLoadGameObject(string path, Transform parent = null)
        {
            //通过路径获取最终预制体的名称
            string[] pathSplit = path.Split('/');
            string prefabName = pathSplit[pathSplit.Length - 1];
            if (gameObjectPoolDic.ContainsKey(prefabName) && gameObjectPoolDic[prefabName].poolQueue.Count > 0)
            {
                return gameObjectPoolDic[prefabName].GetObj(parent);
            }
            else
            {
                return null;
            }
        }
        #endregion


        #region  删除
        /// <summary>
        /// 删除全部
        /// </summary>
        /// <param name="ClearAllGameObject">是否删除所有游戏对象GameObject</param>
        /// <param name="ClearAllCObject">是否删除全部C#的object</param>
        public void ClearAll(bool ClearGameObject = true, bool ClearCObject = true)
        {
            if (ClearGameObject)
            {
                while (poolRootObj.transform.childCount != 0)
                {
                    Destroy(poolRootObj.transform.GetChild(0).gameObject);
                }
                gameObjectPoolDic.Clear();

            }

            if (ClearCObject) objectPoolDic.Clear();
        }
        public void ClearAllGameObject()
        {
            ClearAll(true, false);
        }
        public void ClearAllGameObject(string prefabName)
        {
            GameObject go = poolRootObj.transform.Find(prefabName).gameObject;
            if (go != null)
            {
                Destroy(go);
                gameObjectPoolDic.Remove(prefabName);
            }
        }
        public void ClearAllGameObject(GameObject prefab)
        {
            ClearAllGameObject(prefab.name);
        }
        public void ClearAllObject()
        {
            ClearAll(false, true);
        }
        public void ClearAllObject<T>()
        {
            objectPoolDic.Remove(typeof(T).FullName);

        }
        public void ClearAllObject(Type type)
        {
            objectPoolDic.Remove(type.FullName);
        }
        #endregion
    }
}