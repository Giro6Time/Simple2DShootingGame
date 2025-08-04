using System;
using System.Reflection;
using UnityEngine;

namespace GiroFrame
{
    /// <summary>
    /// 当值改变时调用指定方法的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class OnValueChangedAttribute : PropertyAttribute
    {
        public string methodName { get; private set; }
        public bool includeChildren { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="methodName">要调用的方法名称</param>
        /// <param name="includeChildren">是否包含子对象的变化</param>
        public OnValueChangedAttribute(string methodName, bool includeChildren = true)
        {
            this.methodName = methodName;
            this.includeChildren = includeChildren;
        }
    }
}