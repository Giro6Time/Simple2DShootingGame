using UnityEngine;
using GiroFrame;
[Pool]
public class BulletHitEffect : MonoBehaviour
{
    [Range(0,360)]
    public float angleOffset;
    private float _existTime = 0.05f;
    private SpriteRenderer spriteRenderer;
    public void SetProperty(Vector3 hitPosition, Vector2 hitNormal,  float existTime = 0.05f)
    {
        _existTime = existTime;
        
        float angle = Mathf.Atan2(hitNormal.y, hitNormal.x) * Mathf.Rad2Deg;
        // 调整角度（假设特效默认朝右，需要让特效朝向碰撞面）
        angle += angleOffset; // 根据你的特效方向调整偏移量
        transform.eulerAngles = new Vector3(0, 0, angle);
        transform.position = hitPosition;
    }
    private void Update()
    {
        //这只是一个简单的一次性特效
        if (_existTime <= 0)
        {
            this.GiroGameObjectPushPool();
        }

        _existTime -= Time.deltaTime;
    }
}
