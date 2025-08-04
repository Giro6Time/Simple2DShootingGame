using UnityEngine;

public static class PlayerContext
{
    public static Direction Forward;
    public static GameObject PlayerInstance;
    public static Vector3 OriginGunLocalPosition;
    public static Vector3 NextGunLocalPosition;
    public static Quaternion OriginGunLocalRotation;
    public static Quaternion NextGunLocalRotation;
    public static float Hp;
    public static bool IsDead { get; set; }
}
