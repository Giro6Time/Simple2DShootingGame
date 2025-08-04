using System;
using GiroFrame;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MachineGun : IGun
{
    public Transform bulletSpawn;
    public GameObject bulletPrefab;
    public float bulletSpeed;
    public int bulletCountPerShoot;
    [Tooltip("散射角度（欧拉角）")]
    public float maxBulletSpread = 15;
    public float cooldown = 0.1f;
    public float damage = 1;
    public AudioClip[] shootAudioClips;
    [Header("组件")]
    public MuzzleFlash muzzleFlash;
    public ParticleSystem shellEjector;
    private float cooldownCounter;
    

    void Update()
    {
        if (cooldownCounter > 0)
        {
            cooldownCounter -= Time.deltaTime;
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    /// 射击
    /// </summary>
    /// <returns>是否发射成功</returns>
    public override bool Shoot()
    {
        if (cooldownCounter > 0)
            return false;
        cooldownCounter = cooldown;
        for(var i = 0; i < bulletCountPerShoot; i++)
            SpawnBullet();

        AudioManager.Instance.PlayOnShot(GetShootAudioClip(), this);
        if(muzzleFlash)
            muzzleFlash.Flash();
        if(shellEjector)
            shellEjector.Emit(Random.Range(1,3));
        EventManager.EventTrigger("Shoot");
        return true;
    }

    protected override void SpawnBullet()
    {
        MachineGunBullet bullet =  PoolManager.Instance.GetGameObject<MachineGunBullet>(bulletPrefab, bulletParent);
        
        bullet.transform.position = bulletSpawn.transform.position;
        Vector2 direction = Vector2.zero;
        direction.x = PlayerContext.Forward == Direction.Right ? 1 : -1;
        float spread = Random.Range(-maxBulletSpread, maxBulletSpread);
        direction = direction.RotateVector(spread);
        
        bullet.SetProperty(direction,bulletSpeed,damage);
    }
    private AudioClip GetShootAudioClip()
    {
        int index = Random.Range(0,shootAudioClips.Length);
        return shootAudioClips[index];
    }
}
