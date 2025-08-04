using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public float existTime = 0.1f;
    [Tooltip("两次闪烁之间至少间隔的时间长度")]
    public float flashInterval;
    [Header("组件")]
    public SpriteRenderer flashRenderer;
    
    private float flashTimeCounter;

    public void Flash()
    {
        flashTimeCounter = existTime;
    }

    private void Start()
    {
        
        flashRenderer.enabled = false;
    }

    private void Update()
    {
        HandleFlash();
    }
    
    private void HandleFlash()
    {
        if (flashTimeCounter > 0)
        {
            if (flashTimeCounter > existTime - flashInterval )
            {
                flashTimeCounter -= Time.deltaTime;
                flashRenderer.enabled = false;

                return;
            }
            flashTimeCounter -= Time.deltaTime;
            flashRenderer.enabled = true;
        }
        else if(flashRenderer.enabled)
        {
            flashRenderer.enabled = false;
        }
    }
}
