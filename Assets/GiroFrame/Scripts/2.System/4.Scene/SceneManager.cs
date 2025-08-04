using System.Collections;
using UnityEngine;
using System;
namespace GiroFrame
{
    public static class SceneManager
    {
        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="callBack"></param>
        public static void LoadScene(string sceneName, Action callBack = null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            callBack?.Invoke();
        }
        /// <summary>
        /// 异步加载场景
        /// 会自动分发到事件中心，事件名称"LoadingSceneProgress"
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="callBack"></param>
        public static void LoadSceneAsync(string sceneName, Action callBack = null)
        {
            MonoManager.Instance.StartCoroutine(DoLoadSceneAsync(sceneName, callBack));
        }
        private static IEnumerator DoLoadSceneAsync(string sceneName, Action callBack = null)
        {
            AsyncOperation ao = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            while (ao.isDone == false)
            {
                EventManager.EventTrigger("LoadingSceneProgress", ao.progress);
                yield return ao.progress;
            }
            EventManager.EventTrigger("LoadingSceneProgress", 1f);
            callBack?.Invoke();
        }
    }
}

