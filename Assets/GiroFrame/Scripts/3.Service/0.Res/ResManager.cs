using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace GiroFrame
{
    public static class ResManager
    {
        //需要缓存的类型
        private static Dictionary<Type, bool> wantCacheDie;

        static ResManager()
        {
            //由于没有引用完整的GiroFrame（没有更新Odin插件所以无法引用），所以不提前加载cacheDic，直接初始化一个新的
            //wantCacheDie = GameRoot.Instance.GameSetting.cacheDic;
            wantCacheDie = new Dictionary<Type, bool>();
        }



        /// <summary>
        /// 检查一个type 是否需要保存
        /// </summary>
        private static bool CheckCacheDic(Type type)
        {
            return wantCacheDie.ContainsKey(type);
        }





        /// <summary>加载Unity配置，如AudioClip
        /// </summary>
        public static T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            return Resources.Load<T>(path);
        }
        /// <summary>获取实例--普通Class
        /// 如果类型需要缓存，会从对象池中获取
        /// </summary>
        public static T Load<T>() where T : class, new()
        {
            // 需要缓存
            if (CheckCacheDic(typeof(T)))
            {
                return PoolManager.Instance.GetObject<T>();
            }
            else
            {
                return new T();
            }
        }
        /// <summary>
        /// 获取实例（组件类型）
        /// </summary>
        public static T Load<T>(string path, Transform parent = null) where T : Component
        {

            // 需要缓存
            if (CheckCacheDic(typeof(T)))
            {
                return PoolManager.Instance.GetGameObject<T>(GetPrefab(path), parent);
            }
            else
            {
                return InstantiateForPrefab(path, parent).GetComponent<T>();
            }
        }
        /// <summary>
        /// 异步加载游戏物体
        /// </summary>
        public static void LoadGameObjectAsync<T>(string path, Action<T> callBack = null, Transform parent = null) where T : UnityEngine.Object
        {
            //考虑对象池的情况 
            if (CheckCacheDic(typeof(T)))//对象池里面有的话
            {

                GameObject go = PoolManager.Instance.CheckCacheAndLoadGameObject(path, parent);
                if (go != null)
                {
                    callBack?.Invoke(go.GetComponent<T>());
                }
                else //对象池没有的情况
                {
                    MonoManager.Instance.StartCoroutine(DoLoadGameObjectAsync<T>(path, callBack, parent));
                }
            }
            else //对象池没有的情况
            {
                MonoManager.Instance.StartCoroutine(DoLoadGameObjectAsync<T>(path, callBack, parent));
            }
        }
        static IEnumerator DoLoadGameObjectAsync<T>(string path, Action<T> callBack = null, Transform parent = null) where T : UnityEngine.Object
        {
            ResourceRequest request = Resources.LoadAsync<GameObject>(path);
            yield return request;
            GameObject go = InstantiateForPrefab(request.asset as GameObject, parent);
            callBack?.Invoke(go.GetComponent<T>());
        }
        /// <summary> 异步加载Unity资源 如AudioClip GameObject
        /// </summary>
        public static void LoadAssetAsync<T>(string path, Action<T> callBack) where T : UnityEngine.Object
        {
            MonoManager.Instance.StartCoroutine(DoLoadAssetAsync<T>(path, callBack));
        }
        static IEnumerator DoLoadAssetAsync<T>(string path, Action<T> callBack) where T : UnityEngine.Object
        {
            ResourceRequest request = Resources.LoadAsync<T>(path);
            yield return request;
            callBack?.Invoke(request.asset as T);
        }
        public static GameObject GetPrefab(string path)
        {
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab != null)
            {
                return prefab;

            }
            else
            {
                throw new Exception("ResManager:预制体路径有误，没有找到预制体");
            }


        }
        public static GameObject InstantiateForPrefab(string path, Transform parent = null)
        {
            return InstantiateForPrefab(GetPrefab(path), parent);
        }
        public static GameObject InstantiateForPrefab(GameObject prefab, Transform parent = null)
        {

            GameObject go = GameObject.Instantiate<GameObject>(prefab, parent);
            go.name = prefab.name;
            return go;
        }
    }
}