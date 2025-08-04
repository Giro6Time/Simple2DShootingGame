using System.Collections.Generic;
using UnityEngine;

public class TimeFreezer : ILevelContextComponent
{
    private Queue<float> _freezeQueue = new Queue<float>();
    private float _freezeEndTime; // 冻结结束的真实时间戳（毫秒）
    private bool _active;
    /// <summary>
    /// 冻结游戏指定毫秒（不暂停音频）
    /// </summary>
    /// <param name="freezeMilliseconds">冻结时长（毫秒）</param>
    public void FreezeTime(int freezeMilliseconds)
    {
        if (!_active) return;
        
        _freezeQueue.Enqueue(Time.realtimeSinceStartup + (freezeMilliseconds * 0.001f));
        Time.timeScale = 0f;
        AudioListener.pause = false;
    }

    public void Update()
    {
        if (!_active) return;
        if (_freezeQueue.Count > 0 && Time.realtimeSinceStartup >= _freezeQueue.Peek())
        {
            _freezeQueue.Dequeue();
            if (_freezeQueue.Count == 0) Time.timeScale = 1f;
        }
    }

    public bool SetActive(bool active)
    {
        _active = active;
        return _active;
    }

    public bool IsActive()
    {
        return _active;
    }
}