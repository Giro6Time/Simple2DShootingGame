using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float maxHp;
    private void Awake()
    {
        PlayerContext.Forward = Direction.Right;
        PlayerContext.PlayerInstance = gameObject;
        PlayerContext.Hp = maxHp;
        PlayerContext.IsDead = false;
    }
}
