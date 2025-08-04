using GiroFrame;
using UnityEngine;

public class TimedPushPool : MonoBehaviour
{
    private float timer;
    private float lifeTime;
    
    public void SetLifeTime(float time)
    {
        lifeTime = time;
        timer = 0f;
    }
    
    void Update()
    {
        if (timer >= lifeTime)
        {
            gameObject.GiroGameObjectPushPool();
        }
        timer += Time.deltaTime;
    }
}