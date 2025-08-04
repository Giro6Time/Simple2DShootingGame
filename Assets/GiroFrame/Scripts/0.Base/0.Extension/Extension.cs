

using System;
using System.Reflection;
using UnityEngine;

namespace GiroFrame
{
    public static class GiroExtension
    {
        #region 通用

        /// <summary>
        /// 获取特性
        /// </summary>
        public static T GetAttribute<T>(this object obj) where T : Attribute
        {
            return obj.GetType().GetCustomAttribute<T>();
        }

        /// <summary>
        /// 获取特性
        /// </summary>
        /// <param name="type">特性所在的类型</param>
        /// <returns></returns>
        public static T GetAttribute<T>(this object obj, Type type) where T : Attribute
        {
            return type.GetCustomAttribute<T>();
        }

        /// <summary>
        /// 数组完全相等对比，包括数量，每个元素的位置等等
        /// </summary>
        public static bool ArrayEquals(this object[] objs, object[] others)
        {
            if (others == null || objs.GetType() != others.GetType())
            {
                return false;
            }

            if (objs.Length != others.Length)
            {
                return false;
            }

            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] != others[i])
                    return false;
            }

            return true;
        }

        #endregion
        
        #region 资源管理
        /// <summary>GameObject 放入对象池
        /// </summary>
        /// <param name="go"></param>
        public static void GiroGameObjectPushPool(this GameObject go)
        {
            PoolManager.Instance.PushGameObject(go);
        }
        public static void GiroGameObjectPushPool(this Component com)
        {
            PoolManager.Instance.PushGameObject(com.gameObject);
        }
        /// <summary>
        /// 普通类放进池子
        /// </summary>
        /// <param name="obj"></param>
        public static void GiroPushPool(this object obj)
        {
            PoolManager.Instance.PushObject(obj);
        }
        #endregion
    }
}