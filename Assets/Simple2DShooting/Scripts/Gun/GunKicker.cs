using System;
using GiroFrame;
using UnityEngine;
using Random = UnityEngine.Random;

public class GunKicker : MonoBehaviour
{
    [Header("后坐力参数")]
    public float recoilRotationAngle = 10f;   // 最大后坐力旋转角度
    public float recoilBackDistance = 0.2f;   // 最大后坐力后退距离
    public float recoilRecoverySpeed = 5f;   // 后坐力恢复速度

    private float _currentRecoilRotation;      // 当前后坐力旋转值
    private float _currentRecoilBack;          // 当前后坐力位移值

    private void Start()
    {
        EventManager.AddEventListener("Shoot", GunKick);
    }

    private void OnDestroy()
    {
        EventManager.RemoveEventListener("Shoot", GunKick);
    }

    protected void Update()
    {

        // 后坐力恢复（每帧逐渐复位）
        if (_currentRecoilRotation != 0f || _currentRecoilBack != 0f)
        {
            _currentRecoilRotation = Mathf.Lerp(_currentRecoilRotation, 0f, Time.deltaTime * recoilRecoverySpeed);
            _currentRecoilBack = Mathf.Lerp(_currentRecoilBack, 0f, Time.deltaTime * recoilRecoverySpeed);

            // 应用后坐力效果（叠加在原有延迟位置上）
            PlayerContext.NextGunLocalPosition -= new Vector3(_currentRecoilBack, 0, 0);
            PlayerContext.NextGunLocalRotation *= Quaternion.Euler(0, 0, _currentRecoilRotation);
        }
    }

    protected void GunKick()
    {
        // 触发后坐力效果
        _currentRecoilRotation = Random.Range(-recoilRotationAngle, recoilRotationAngle);
        _currentRecoilBack = recoilBackDistance;
    }
}