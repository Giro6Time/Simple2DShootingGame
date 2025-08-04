using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public abstract class IGun : MonoBehaviour
{
    public static Transform bulletParent;
    public float playerKnockBack;
    public abstract bool Shoot();

    protected abstract void SpawnBullet();

    public void Init()
    {
        //这里应该区分枪是被谁拿的，不过demo里面只有Player会拿枪
        PlayerContext.NextGunLocalPosition = PlayerContext.OriginGunLocalPosition = transform.localPosition;
        PlayerContext.NextGunLocalRotation = PlayerContext.OriginGunLocalRotation = transform.localRotation;
        bulletParent = LevelContext.TempGoCollector.transform;
    }

    protected virtual void LateUpdate()
    {
        transform.localPosition = PlayerContext.NextGunLocalPosition;
        PlayerContext.NextGunLocalPosition = PlayerContext.OriginGunLocalPosition;
        transform.localRotation = PlayerContext.NextGunLocalRotation;
        PlayerContext.NextGunLocalRotation = PlayerContext.OriginGunLocalRotation;
    }
}
