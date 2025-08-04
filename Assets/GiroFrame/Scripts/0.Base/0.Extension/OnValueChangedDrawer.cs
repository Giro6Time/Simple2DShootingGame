
#if UNITY_EDITOR
using System;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GiroFrame
{
    /// <summary>
    /// OnValueChanged 特性的 PropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(OnValueChangedAttribute))]
    public class OnValueChangedDrawer : PropertyDrawer
    {
        // 存储上一次的值，用于比较
        private static Dictionary<string, object> previousValues = new Dictionary<string, object>();
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            OnValueChangedAttribute onValueChangedAttribute = (OnValueChangedAttribute)attribute;
            
            // 获取唯一标识符
            string propertyPath = property.serializedObject.targetObject.GetInstanceID() + "." + property.propertyPath;
            
            // 获取当前值
            object currentValue = GetPropertyValue(property);
            
            // 检查值是否改变
            bool valueChanged = false;
            if (previousValues.ContainsKey(propertyPath))
            {
                object previousValue = previousValues[propertyPath];
                valueChanged = !Equals(currentValue, previousValue);
            }
            else
            {
                // 第一次记录值
                previousValues[propertyPath] = currentValue;
            }
            
            // 开始检查变化
            EditorGUI.BeginChangeCheck();
            
            // 绘制默认的属性字段
            EditorGUI.PropertyField(position, property, label, onValueChangedAttribute.includeChildren);
            
            // 如果值发生了变化
            if (EditorGUI.EndChangeCheck())
            {
                // 应用修改
                property.serializedObject.ApplyModifiedProperties();
                
                // 更新存储的值
                object newValue = GetPropertyValue(property);
                previousValues[propertyPath] = newValue;
                
                // 调用回调方法
                InvokeCallback(property, onValueChangedAttribute.methodName);
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            OnValueChangedAttribute onValueChangedAttribute = (OnValueChangedAttribute)attribute;
            return EditorGUI.GetPropertyHeight(property, label, onValueChangedAttribute.includeChildren);
        }
        
        /// <summary>
        /// 获取属性值
        /// </summary>
        private object GetPropertyValue(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue;
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Float:
                    return property.floatValue;
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Color:
                    return property.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return property.intValue;
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex;
                case SerializedPropertyType.Vector2:
                    return property.vector2Value;
                case SerializedPropertyType.Vector3:
                    return property.vector3Value;
                case SerializedPropertyType.Vector4:
                    return property.vector4Value;
                case SerializedPropertyType.Rect:
                    return property.rectValue;
                case SerializedPropertyType.ArraySize:
                    return property.arraySize;
                case SerializedPropertyType.Character:
                    return property.intValue;
                case SerializedPropertyType.AnimationCurve:
                    return property.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return property.boundsValue;
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue;
                default:
                    // 对于复杂类型，返回序列化后的字符串作为比较
                    return property.serializedObject.ToString() + property.propertyPath;
            }
        }
        
        /// <summary>
        /// 调用回调方法
        /// </summary>
        private void InvokeCallback(SerializedProperty property, string methodName)
        {
            var target = property.serializedObject.targetObject;
            var type = target.GetType();
            
            // 查找方法（支持私有方法）
            MethodInfo method = type.GetMethod(methodName, 
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            
            if (method != null)
            {
                try
                {
                    // 调用方法
                    method.Invoke(target, null);
                    
                    // 如果是ScriptableObject，标记为脏数据
                    if (target is ScriptableObject)
                    {
                        EditorUtility.SetDirty(target);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"调用方法 '{methodName}' 时发生错误: {e.Message}\n{e.StackTrace}");
                }
            }
            else
            {
                Debug.LogWarning($"在 {type.Name} 中找不到方法 '{methodName}'");
            }
        }
    }
}
#endif