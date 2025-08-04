using System;
using System.Collections;
using System.Collections.Generic;
using GiroFrame;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public interface IBullet
{
    void HandleMovement();
    void Destroy();
}

[Pool]
public class MachineGunBullet : MonoBehaviour, IBullet
{
    [Header("命中后效果")]
    public GameObject hitEffect;
    public float explosionRate = 0.3f; // 30% 爆炸概率
    public float explosionRadius = 3f;
    public int explosionDamage = 50;
    public float explosionLifeTime = 0.05f;
    public float explosionScreenShakeIntensity = 0.05f;
    
    public GameObject explosionEffectPrefab; // 爆炸贴图预制体
    public GameObject smokePrefab; // 烟雾预制体
    public AudioClip explosionAudio;
    private Vector2 _direction;
    private float _speed;
    private bool valid = false;
    private float _damage;
    private Vector2 _velocity;
    
    public void SetProperty(Vector2 direction, float speed, float damage = 1)
    {
        _direction = direction.normalized;
        _speed = speed;
        _damage = damage;
        valid = true;
    }

    public void HandleMovement()
    {
        if (!valid)
        {
            return;
        }

        _velocity = _speed * Time.deltaTime * _direction;
        transform.Translate(_velocity, Space.World);
        
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0, 0, angle);
    }
    public void Destroy()
    {
        PoolManager.Instance.PushGameObject(this.gameObject);
    }
    private void Update()
    {
        HandleMovement();
    }

    void OnCollisionEnter2D(Collision2D collision) {
        /*
        TODO其实这里都应该设置成回调，通过Gun来传递具体子弹打中会有什么表现，但是这么小个Demo，不想把Gun脚本写太复杂
        在Explosion的时候就会有问题，如果想要自动算出radius的话，这个计算就应该又Gun来做，然后在发射子弹的时候，向子弹命中的回调函数中实现
        具体的爆炸效果以及半径到scale的转换，这样就不需要手动设置实际的radius大小，而且scale也不需要管理，在逻辑和性能上都会更好
        */
        if (collision.gameObject.CompareTag("Ground")) {
            // 获取碰撞点的法线（2D）
            ContactPoint2D contact = collision.contacts[0];
            SpawnBulletEffect(contact, LayerMask.GetMask("Tilemap"));
            Destroy();
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            ContactPoint2D contact = collision.contacts[0];
            IEnemy enemy = collision.gameObject.GetComponent<IEnemy>();

            
            // 爆炸概率检查
            if (Random.Range(0f, 1f) < explosionRate)
            {
                Explosion(contact.point);
                LevelContext.CameraGO.GetComponent<CameraController>().KickBack(explosionScreenShakeIntensity,
                    ((Vector2)PlayerContext.PlayerInstance.transform.position - contact.point).normalized);
            }
            else //不爆炸才造成伤害
            {
                enemy.TakeDamage(_damage, 0.3f,_velocity.x > 0 ? Direction.Right : Direction.Left);
                SpawnBulletEffect(contact, LayerMask.GetMask("Enemy"));
                LevelContext.timeFreezer.FreezeTime(LevelContext.timeFreezeLength);
            }
            Destroy();
        }
    }


    private void Explosion(Vector2 explosionPoint)
    {
        // 范围伤害
        Collider2D[] enemies = Physics2D.OverlapCircleAll(explosionPoint, explosionRadius, LayerMask.GetMask("Enemy"));
        foreach (Collider2D enemyCollider in enemies)
        {
            IEnemy enemy = enemyCollider.GetComponent<IEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(explosionDamage, 0.3f,explosionPoint.x < enemyCollider.transform.position.x ? Direction.Right : Direction.Left);
            }
        }
        LevelContext.timeFreezer.FreezeTime(LevelContext.timeFreezeLength);

        // 生成爆炸效果贴图
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = PoolManager.Instance.GetGameObject(explosionEffectPrefab, LevelContext.TempGoCollector.transform);
            explosion.transform.position = (Vector3)explosionPoint;
            var tpp = explosion.GetComponent<TimedPushPool>();
            if(explosionAudio)
                AudioManager.Instance.PlayOnShot(explosionAudio, tpp);
            tpp.SetLifeTime(explosionLifeTime);
        }
    
        // 生成烟雾效果
        if (smokePrefab != null)
        {
            GameObject smoke = PoolManager.Instance.GetGameObject(smokePrefab, LevelContext.TempGoCollector.transform);
            smoke.transform.position = (Vector3)explosionPoint;
            var tpp = smoke.GetComponent<TimedPushPool>();
            
            tpp.SetLifeTime(10f);// 烟雾会自己控制生命周期，或者设置更长的销毁时间
        }
        
        
        
    }
    private void SpawnBulletEffect(ContactPoint2D contact, int layerMask )
    {
        Vector2 hitPoint = contact.point;
        Vector2 hitNormal = contact.normal;

        // 补偿检测：从子弹位置向碰撞法线方向发射短距离射线
        RaycastHit2D raycastHit = Physics2D.Raycast(
            transform.position, 
            hitNormal, 
            1f, // 短距离检测
            layerMask
        );
        if (raycastHit.collider != null)
        {
            hitPoint = raycastHit.point; // 使用更精确的碰撞点
            hitNormal = raycastHit.normal;
        }
     
        BulletHitEffect effectInstance = PoolManager.Instance.GetGameObject<BulletHitEffect>(hitEffect, IGun.bulletParent);
        effectInstance.SetProperty(hitPoint,hitNormal);
    }
}
