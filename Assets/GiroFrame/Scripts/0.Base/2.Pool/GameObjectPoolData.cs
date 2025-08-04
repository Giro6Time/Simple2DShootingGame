using System.Collections.Generic;
using UnityEngine;


namespace GiroFrame
{
    public class GameObjectPoolData
    {
        //对象池中父节点
        public GameObject fatherObj;
        //对象容器
        public Queue<GameObject> poolQueue;

        public GameObjectPoolData(GameObject obj, GameObject poolRoot)
        {
            //创建父节点 并且设置到对象池根节点下方
            this.fatherObj = new GameObject(obj.name);
            fatherObj.transform.SetParent(poolRoot.transform);
            poolQueue = new Queue<GameObject>();
            //首次创建的时候要放入第一个物体
            PushObj(obj);
        }

        //存入对象池（隐藏起来）
        public void PushObj(GameObject obj)
        {
            poolQueue.Enqueue(obj);//对象进容器
            obj.transform.SetParent(fatherObj.transform);//设置父物体
            obj.SetActive(false);//设置为隐藏

        }
        //取出对象池（重新显示）
        public GameObject GetObj(Transform parent = null)
        {
            GameObject obj = poolQueue.Dequeue();//取出对象

            obj.SetActive(true);//显示物体
            obj.transform.SetParent(parent);//父物体置空（我个人觉得这里可以改成置入Anchor

            if (parent == null)
            {
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj, UnityEngine.SceneManagement.SceneManager.GetActiveScene());//转入主场景
            }

            return obj;
        }

    }
}