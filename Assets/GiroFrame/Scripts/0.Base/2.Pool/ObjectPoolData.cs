using System.Collections.Generic;

namespace GiroFrame
{
    /// <summary>
    /// 普通类 对象值数据
    /// </summary>
    public class ObjectPoolData
    {

        public ObjectPoolData(object obj)
        {
            PushObj(obj);
        }
        //对象容器
        public Queue<object> poolQueue = new Queue<object>();

        //存入对象池（隐藏起来）
        public void PushObj(object obj)
        {
            poolQueue.Enqueue(obj);//对象进容器

        }
        //从对象池取出
        public object GetObj()
        {
            return poolQueue.Dequeue();
        }
    }
}