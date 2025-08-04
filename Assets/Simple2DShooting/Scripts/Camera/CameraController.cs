using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("相机偏移")] public Vector2 cameraOffset = new Vector2(0, 0);
    [Header("lerp camera 平滑程度")] public float smooth = 0.5f;

    [Header("Look Forward 距离")] public float lookForwardDistance;
    public Rigidbody2D playerRb;
    
    private float shakeTimer;
    private float shakeDuration;
    private float shakeIntensity;
    private float kickbackIntensity;
    private Vector2 kickbackDirection;
    

    void FixedUpdate()
    {
        HandleCameraMovement();
    }

    void HandleCameraMovement()
    {
        Vector3 target = playerRb.position;
        target.x += GetLookForwardDistance();
        target += new Vector3(cameraOffset.x, cameraOffset.y, 0);
        HandleShake();
        LerpCamera(target);
    }
    
    void LerpCamera(Vector3 target)
    {
        Vector3 pos = Vector3.Lerp(transform.position, target, smooth);
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }

    float GetLookForwardDistance()
    {
        float sign = PlayerContext.Forward == Direction.Right ? 1 : -1;
        return sign * lookForwardDistance;
    }
    


    void HandleShake()
    {
        if (shakeTimer > 0)
        {
            // 生成随机偏移
            Vector3 shakeOffset = Random.insideUnitSphere * (shakeIntensity * Mathf.Pow(shakeTimer/shakeDuration, 2));
            shakeOffset.z = 0; // 保持Z轴不变
            
            transform.position +=shakeOffset;
            
            shakeTimer -= Time.deltaTime;
        }

    }
    
    public void Shake(float duration, float intensity)
    {
        shakeTimer = shakeDuration = duration;
        shakeIntensity = intensity;
    }

    public void KickBack(float intensity, Vector2 direction)
    {
        kickbackDirection = direction;
        kickbackIntensity = intensity;
        transform.position += (Vector3)kickbackDirection * kickbackIntensity;
    }
    
    public void StopShake()
    {
        shakeTimer = 0;
    }
}
