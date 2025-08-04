using System;
using System.Collections.Generic;
using GiroFrame;
using UnityEngine;

public interface  ILevelContextComponent
{
    bool SetActive(bool enable);
    bool IsActive();
}

public static class LevelContext
{
    public static int timeFreezeLength;
    public static TimeFreezer timeFreezer;
    public static GameObject TempGoCollector;
    public static GameObject CameraGO;
}

public class LevelManager: MonoBehaviour
{
    [Serializable]
    public struct LevelSetting
    {
        public int timeFreezeLength;
    }

    [Header("关卡内部全局组件")][SerializeField] private GameObject TempGoCollector;
    [SerializeField] private GameObject CameraGO;
    [SerializeField] private LevelSetting levelSetting;
    
    private void Awake()
    {
        LevelContext.timeFreezer = new TimeFreezer();
        LevelContext.timeFreezeLength = levelSetting.timeFreezeLength;
        LevelContext.TempGoCollector = TempGoCollector;
        LevelContext.CameraGO = CameraGO;
    }

    private void Start()
    {
        LevelContext.timeFreezer.SetActive(true);
        MonoManager.Instance.AddUpdateListener(LevelContext.timeFreezer.Update);
        Time.timeScale = 1;
    }

    private void OnDestroy()
    {
        LevelContext.timeFreezer.SetActive(false);
        MonoManager.Instance.RemoveUpdateListener(LevelContext.timeFreezer.Update);
        LevelContext.timeFreezer = null;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene("Simple2DShooting/Scenes/Level");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
