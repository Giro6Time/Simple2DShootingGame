using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Linq;
using UnityEngine;

namespace GiroFrame
{
    /// <summary>
    /// 一个存档数据
    /// </summary>
    [Serializable]
    public class SaveItem
    {
        public int saveID { get; private set; }
        public DateTime lastSaveTime { get; private set; }

        public SaveItem(int saveID, DateTime lastSaveTime)
        {
            this.saveID = saveID;
            this.lastSaveTime = lastSaveTime;
        }

        public void UpdateTime(DateTime time)
        {
            this.lastSaveTime = time;
        }

    }

    /// <summary>
    /// 存档管理器
    /// </summary>
    public static class SaveManager
    {
        [Serializable]
        private class SaveManagerData
        {
            //当前的存档编号
            public int currentID = 0;
            // 所有的存档列表
            public List<SaveItem> saveItemList = new List<SaveItem>();
        }
        private static SaveManagerData saveManagerData;
        //存档的保存
        private const string saveDirName = "saveData";
        //设置的保存：1.全局数据的保存（分辨率，按键设置）2.存档的位置保存
        // 常规情况下，存档管理器自行维护
        private const string settingDirName = "setting";
        //存档文件夹路径
        private static readonly string saveDirPath;
        private static readonly string settingDirPath;

        // 存档中对象的缓存字典
        //<存档ID,<文件名称,实际的对象 >>
        private static Dictionary<int, Dictionary<string, object>> cacheDic = new Dictionary<int, Dictionary<string, object>>();



        static SaveManager()
        {
            saveDirPath = Application.persistentDataPath + "/" + saveDirName;
            settingDirPath = Application.persistentDataPath + "/" + settingDirName;
            //确保路径的存档
            if (Directory.Exists(saveDirPath) == false)
            {
                Directory.CreateDirectory(saveDirPath);
            }
            if (Directory.Exists(settingDirPath) == false)
            {
                Directory.CreateDirectory(settingDirPath);
            }
            // 获取SaveManagerDatas
            InitSaveManagerData();
            // Debug.Log(saveManagerData.currentID);
        }


        #region 存档设置
        private static SaveManagerData InitSaveManagerData()
        {
            saveManagerData = LoadFile<SaveManagerData>(saveDirPath + "/SaveManagerData");
            if (saveManagerData == null)
            {
                saveManagerData = new SaveManagerData();
                UpdateSaveManagerData();
            }
            return saveManagerData;
        }
        /// <summary>
        /// 更新存档管理器数据
        /// </summary>
        public static void UpdateSaveManagerData()
        {
            SaveFile(saveManagerData, saveDirPath + "/SaveManagerData");
        }

        /// <summary>
        /// 获取所有存档
        /// 时间久的在前面，最新的在后面
        /// </summary>
        /// <returns></returns>
        public static List<SaveItem> GetAllSaveItem()
        {
            return saveManagerData.saveItemList;
        }
        /// <summary>
        /// 获取所有存档
        /// 最新的在前面，最久远的在后面
        /// </summary>
        /// <returns></returns>
        public static List<SaveItem> GetAllSaveItemByCreateTime()
        {
            List<SaveItem> saveItems = new List<SaveItem>(saveManagerData.saveItemList.Count);
            for (int i = 0; i < saveManagerData.saveItemList.Count; i++)
            {
                saveItems.Add(saveManagerData.saveItemList[saveManagerData.saveItemList.Count - i - 1]);
            }

            return saveItems;
        }
        private class OrderByUpdateTimeComparer : IComparer<SaveItem>
        {
            public int Compare(SaveItem x, SaveItem y)
            {
                if (x.lastSaveTime > y.lastSaveTime)
                    return -1;
                return 1;
            }
        }

        public static List<SaveItem> GetAllSaveItemByUpdateTime()
        {
            List<SaveItem> saveItems = new List<SaveItem>(saveManagerData.saveItemList.Count);
            for (int i = 0; i < saveManagerData.saveItemList.Count; i++)
            {
                saveItems.Add(saveManagerData.saveItemList[i]);
            }
            OrderByUpdateTimeComparer orderBy = new OrderByUpdateTimeComparer();
            saveItems.Sort(orderBy);
            return saveItems;
        }
        /// <summary>
        /// 获取所有存档
        /// 万能解决方案     
        /// </summary>
        /// <returns></returns>
        public static List<SaveItem> GetAllSaveItems<T>(Func<SaveItem, T> orderFunc, bool isDescending = false)
        {
            if (isDescending)
                return saveManagerData.saveItemList.OrderByDescending(orderFunc).ToList();
            else
                return saveManagerData.saveItemList.OrderBy(orderFunc).ToList();
        }
        //使用方法：
        //传入参数为SaveItem，传出参数为所需要排序的类型
        /*string testfunc(SaveItem item)
        {
            return SaveManager.LoadObject<TestSave>(item).Name;
        }*/
        #endregion

        #region  关于存档

        /// <summary>
        /// 获取SaveItem
        /// </summary>
        /// <returns></returns>
        public static SaveItem GetSaveItem(int id)
        {
            for (int i = 0; i < saveManagerData.saveItemList.Count; i++)
            {
                if (saveManagerData.saveItemList[i].saveID == id)
                    return saveManagerData.saveItemList[i];
            }
            return null;
        }
        /// <summary>
        /// 添加一个存档
        /// </summary>
        /// <returns></returns>
        public static SaveItem CreateSaveItem()
        {
            SaveItem saveItem = new SaveItem(saveManagerData.currentID, DateTime.Now);
            saveManagerData.saveItemList.Add(saveItem);
            saveManagerData.currentID += 1;
            // SaveManagerData更新， 写入磁盘
            UpdateSaveManagerData();
            return saveItem;
        }
        /// <summary>
        /// 删除存档 
        /// </summary>
        public static void DeleteSaveItem(int saveID)
        {
            string itemDir = GetSavePath(saveID, false);
            //如果路径存在且有效
            if (itemDir != null)
            {
                //把这个存档下的文件递归删除
                Directory.Delete(itemDir, true);
            }
            saveManagerData.saveItemList.Remove(GetSaveItem(saveID));
            // 移除缓存
            RemoveCache(saveID);
            // 更新SaveManager 写入磁盘
            UpdateSaveManagerData();
        }
        public static void DeleteSaveItem(SaveItem saveItem)
        {
            string itemDir = GetSavePath(saveItem.saveID, false);
            //如果路径存在且有效
            if (itemDir != null)
            {
                //把这个存档下的文件递归删除
                Directory.Delete(itemDir, true);
            }
            saveManagerData.saveItemList.Remove(saveItem);
            RemoveCache(saveItem.saveID);
            // 更新SaveManager 写入磁盘
            UpdateSaveManagerData();
        }
        #endregion

        #region  关于缓存

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="saveID">存档ID</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="saveObject">缓存对象</param>
        private static void SetCache(int saveID, string fileName, object saveObject)
        {
            //缓存字典中是否有这个ID
            if (cacheDic.ContainsKey(saveID))
            {
                if (cacheDic[saveID].ContainsKey(fileName))
                {
                    cacheDic[saveID][fileName] = saveObject;
                }
                else
                {
                    cacheDic[saveID].Add(fileName, saveObject);
                }
            }
            else
            {
                cacheDic.Add(saveID, new Dictionary<string, object>() { { fileName, saveObject } });

            }
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="saveID">存档ID</param>
        /// <param name="fileName">文件名称</param>
        private static T GetCache<T>(int saveID, string fileName) where T : class
        {
            //缓存字典中是否有这个ID
            if (cacheDic.ContainsKey(saveID))
            {
                if (cacheDic[saveID].ContainsKey(fileName))
                {
                    return cacheDic[saveID][fileName] as T;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 移除缓存
        /// </summary>
        private static void RemoveCache(int saveID)
        {
            cacheDic.Remove(saveID);
        }

        #endregion

        #region  关于对象

        /// <summary>
        /// 保存对象至某个存档中
        /// </summary>
        /// <param name="saveObject">要保存的对象</param>
        /// <param name="saveFileName">保存的文件名称</param>
        /// <param name="saveID">存档的ID</param>
        public static void SaveObject(object saveObject, string saveFileName, int saveID = 0)
        {
            // 所在文件夹的路径
            string dirPath = GetSavePath(saveID, true);
            // 具体的对象要保存的路径
            string savePath = dirPath + "/" + saveFileName;
            //具体的保存
            SaveFile(saveObject, savePath);
            //更新存档时间
            GetSaveItem(saveID).UpdateTime(DateTime.Now);
            // 更新Save ManagerData写入磁盘
            UpdateSaveManagerData();
            //更新缓存
            SetCache(saveID, saveFileName, saveObject);


        }
        /// <summary>
        /// 保存对象至某个存档中
        /// </summary>
        /// <param name="saveObject">要保存的对象</param>
        /// <param name="saveFileName">保存的文件名称</param>
        /// <param name="saveItem">存档内容</param>
        public static void SaveObject(object saveObject, string saveFileName, SaveItem saveItem)
        {
            // 所在文件夹的路径
            string dirPath = GetSavePath(saveItem.saveID, true);
            // 具体的对象要保存的路径
            string savePath = dirPath + "/" + saveFileName;
            //具体的保存
            SaveFile(saveObject, savePath);
            //更新存档时间
            saveItem.UpdateTime(DateTime.Now);
            // 更新Save ManagerData写入磁盘
            UpdateSaveManagerData();
            //更新缓存
            SetCache(saveItem.saveID, saveFileName, saveObject);
        }
        /// <summary>
        /// 保存对象至某个存档中
        /// </summary>
        /// <param name="saveObject">要保存的对象</param>
        /// <param name="saveID">存档的ID</param>
        public static void SaveObject(object saveObject, int saveID = 0)
        {
            SaveObject(saveObject, saveObject.GetType().Name, saveID);
        }
        /// <summary>
        /// 保存对象至某个存档中
        /// </summary>
        /// <param name="saveObject">要保存的对象</param>
        /// <param name="saveItem">存档的内容</param>
        public static void SaveObject(object saveObject, SaveItem saveItem)
        {
            SaveObject(saveObject, saveObject.GetType().Name, saveItem);
        }


        /// <summary>
        /// 从某个具体的存档中加载某个对象
        /// </summary>
        /// <typeparam name="T">要返回的实际类型</typeparam>
        /// <param name = "saveFileName">文件名称</param>
        /// <param name = "SaveID">存档ID</param>
        public static T LoadObject<T>(string saveFileName, int saveID = 0) where T : class
        {
            T obj = GetCache<T>(saveID, saveFileName);
            if (obj == null)
            {
                // 所在文件夹的路径
                string dirPath = GetSavePath(saveID);
                if (dirPath == null) return null;
                //具体对象要保存的路径 
                string savePath = dirPath + "/" + saveFileName;
                obj = LoadFile<T>(savePath);
                SetCache(saveID, saveFileName, obj);
            }
            return obj;
        }
        /// <summary>
        /// 从某个具体的存档中加载某个对象
        /// </summary>
        /// <typeparam name="T">要返回的实际类型</typeparam>
        /// <param name = "saveFileName">文件名称</param>
        /// <param name = "SaveItem">存档内容</param>
        public static T LoadObject<T>(string saveFileName, SaveItem saveItem) where T : class
        {
            return LoadObject<T>(saveFileName, saveItem.saveID);
        }
        /// <summary>
        /// 从某个具体的存档中加载某个对象
        /// </summary>
        /// <typeparam name="T">要返回的实际类型</typeparam>
        /// <param name = "id">存档ID</param>
        public static T LoadObject<T>(int saveID = 0) where T : class
        {
            return LoadObject<T>(typeof(T).Name, saveID);
        }
        /// <summary>
        /// 从某个具体的存档中加载某个对象
        /// </summary>
        /// <typeparam name="T">要返回的实际类型</typeparam>
        /// <param name = "saveItem">存档信息</param>
        public static T LoadObject<T>(SaveItem saveItem) where T : class
        {
            return LoadObject<T>(typeof(T).Name, saveItem.saveID);
        }


        #endregion

        #region  全局数据
        /// <summary>
        /// 加载全局设置，不关乎存档
        /// </summary>
        public static T LoadSetting<T>(string fileName) where T : class
        {
            return LoadFile<T>(settingDirPath + "/" + fileName);
        }
        /// <summary>
        /// 加载全局设置，不关乎存档
        /// </summary>
        public static T LoadSetting<T>() where T : class
        {
            return LoadFile<T>(settingDirPath + "/" + typeof(T).Name);
        }
        /// <summary>
        /// 保存全局设置，不关乎存档
        /// </summary>
        public static void SaveSetting(object saveObject, string fileName)
        {
            SaveFile(saveObject, settingDirPath + "/" + fileName);
        }
        /// <summary>
        /// 保存全局设置，不关乎存档
        /// </summary>
        public static void SaveSetting(object saveObject)
        {
            SaveFile(saveObject, settingDirPath + "/" + saveObject.GetType().Name);
        }
        #endregion

        #region  工具函数
        private static BinaryFormatter binaryFormatter = new BinaryFormatter();
        /// <summary>
        /// 获取某个存档的路径
        /// </summary>
        /// <param name="saveID">存档ID</param>
        /// <param name="createDir">如果这个存档路径不存在，是否需要新建一个路径</param>
        /// <returns></returns>
        private static string GetSavePath(int saveID, bool createDir = true)
        {
            // 验证是否有某个存档
            if (GetSaveItem(saveID) == null) throw new Exception("Giro:存档ID不存在！");

            string saveDir = saveDirPath + "/" + saveID;
            //确定文件夹是否存在
            if (Directory.Exists(saveDir) == false)
            {
                if (createDir)
                {
                    Directory.CreateDirectory(saveDir);
                }
                else
                {
                    return null;
                }
            }

            return saveDir;
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="saveObject">保存的对象</param>
        /// <param name="path">保存的路径</param>
        private static void SaveFile(object saveObject, string path)
        {
            FileStream f = new FileStream(path, FileMode.OpenOrCreate);
            //二进制的方式把对象写进文件
            binaryFormatter.Serialize(f, saveObject);
            f.Dispose();

        }
        private static T LoadFile<T>(string path) where T : class
        {
            if (!File.Exists(path)) return null;
            FileStream file = new FileStream(path, FileMode.Open);
            //将内容解码成对象
            T obj = (T)binaryFormatter.Deserialize(file);
            file.Dispose();
            return obj;
        }
        #endregion
    }
}