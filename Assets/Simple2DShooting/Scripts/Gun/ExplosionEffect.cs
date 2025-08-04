using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public Material mat;
    public float flashTime;
    
    private static readonly int FlashFactor = Shader.PropertyToID("_FlashFactor");
    private float flashTimeCounter;
    void OnEnable()
    {
        mat.SetFloat(FlashFactor, 1);
        flashTimeCounter = flashTime;
    }

    void Update()
    {
        if (flashTimeCounter < 0)
        {
            mat.SetFloat(FlashFactor, 0);
        }

        flashTimeCounter -= Time.deltaTime;
    }
    
}
