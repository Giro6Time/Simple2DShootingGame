using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

namespace GiroFrame.Database
{
    #region 数据模型基类和接口
    
    /// <summary>
    /// 数据库条目接口，所有数据模型都需要实现此接口
    /// </summary>
    public interface IDataEntry
    {
        int ID { 
            get; 
            #if UNITY_EDITOR
            set; 
            #endif
        }
    }

    /// <summary>
    /// 数据模型基类
    /// </summary>
    [System.Serializable]
    public abstract class DataEntryBase : IDataEntry
    {
        [SerializeField] protected int id;
        public int ID
        {
            get => id;
            #if UNITY_EDITOR
            set => id = value;
            #endif
        }
    }
    
    #endregion

    #region ScriptableObject数据库基类

    public abstract class ScriptableDatabase : ScriptableObject
    {
#if UNITY_EDITOR
        /// <summary>
        /// 编辑器下验证数据完整性
        /// </summary>
        public abstract void ValidateData();
        /// <summary>
        /// 编辑器下根据所在List位置设置ID
        /// </summary>
        public abstract void ResetID();
#endif
    }
    
    /// <summary>
    /// ScriptableObject数据库基类
    /// </summary>
    /// <typeparam name="T">数据模型类型</typeparam>
    public abstract class ScriptableDatabase<T> : ScriptableDatabase where T : IDataEntry
    {
        [SerializeField] protected List<T> dataList = new List<T>();
        
        private Dictionary<int, T> _dataDict;
        
        /// <summary>
        /// 获取数据字典（懒加载）
        /// </summary>
        protected Dictionary<int, T> DataDict
        {
            get
            {
                if (_dataDict == null)
                {
                    RefreshDict();
                }
                return _dataDict;
            }
        }
        
        /// <summary>
        /// 刷新字典缓存
        /// </summary>
        public void RefreshDict()
        {
            _dataDict = new Dictionary<int, T>();
            foreach (var data in dataList)
            {
                if (data != null && !_dataDict.ContainsKey(data.ID))
                {
                    _dataDict[data.ID] = data;
                }
            }
        }
        
        /// <summary>
        /// 根据ID获取数据
        /// </summary>
        public T GetDataByID(int id)
        {
            return DataDict.TryGetValue(id, out T data) ? data : default(T);
        }
        
        /// <summary>
        /// 获取所有数据
        /// </summary>
        public List<T> GetAllData()
        {
            return new List<T>(dataList);
        }
        
        /// <summary>
        /// 根据条件查找数据
        /// </summary>
        public List<T> FindData(System.Func<T, bool> predicate)
        {
            return dataList.Where(predicate).ToList();
        }
        
        /// <summary>
        /// 检查是否存在指定ID的数据
        /// </summary>
        public bool ContainsID(int id)
        {
            return DataDict.ContainsKey(id);
        }
        
        /// <summary>
        /// 获取数据总数
        /// </summary>
        public int Count => dataList.Count;

        #if UNITY_EDITOR
        /// <summary>
        /// 编辑器下验证数据完整性
        /// </summary>
        public override void ValidateData()
        {
            HashSet<int> idSet = new HashSet<int>();
            List<string> errors = new List<string>();
            for (int i = 0; i < dataList.Count; i++)
            {
                var data = dataList[i];
                if (data == null)
                {
                    errors.Add($"Index {i}: Data is null");
                    continue;
                }
                
                if (data.ID <= 0)
                {
                    errors.Add($"Index {i}: Invalid ID ({data.ID})");
                }
                
                if (idSet.Contains(data.ID))
                {
                    errors.Add($"Index {i}: Duplicate ID ({data.ID})");
                }
                else
                {
                    idSet.Add(data.ID);
                }
            }

            
            if (errors.Count > 0)
            {
                Debug.LogError($"Data validation failed in {name}:\n" + string.Join("\n", errors));
            }
            else
            {
                Debug.Log($"Data validation passed for {name}. Total entries: {dataList.Count}");
            }
        }

        public override void ResetID()
        {
            for (int i = 0; i < dataList.Count; i++)
            {
                dataList[i].ID = i + 1;
            }
        }

#endif
    }
    
#if UNITY_EDITOR

    [CustomEditor(typeof(ScriptableDatabase<>), true)]
    public class ScriptableDatabaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        
            GUILayout.Space(10);
            if (GUILayout.Button("Reset ID"))
            {
                var database = target as ScriptableDatabase;
                database?.ResetID();
            }
            if (GUILayout.Button("Validate Data"))
            {
                var database = target as ScriptableDatabase;
                database?.ValidateData();
            }
        }
    }
#endif
    
    #endregion

}