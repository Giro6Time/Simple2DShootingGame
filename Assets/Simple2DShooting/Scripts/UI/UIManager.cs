using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public UIWindow window;
    // Start is called before the first frame update
    void Start()
    {
        window.gameObject.SetActive(true);
        Invoke(nameof(CloseWindow), 2f);
    }

    void CloseWindow()
    {        
        window.gameObject.SetActive(false);
    }

}
