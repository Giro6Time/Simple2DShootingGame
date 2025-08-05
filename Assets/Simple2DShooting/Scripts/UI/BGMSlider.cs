using System.Collections;
using System.Collections.Generic;
using GiroFrame;
using UnityEngine;
using Slider = UnityEngine.UI.Slider;

public class BGMSlider : MonoBehaviour
{
    public Slider slider;

    public void SetBGMVolumn()
    {
        AudioManager.Instance.BGVolume = slider.value;
    }
}
