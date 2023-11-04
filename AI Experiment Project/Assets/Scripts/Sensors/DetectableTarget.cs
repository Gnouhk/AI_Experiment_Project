using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectableTarget : MonoBehaviour
{

    void Start()
    {
        DetectableTargetManager.instance.Register(this);
    }

    void Update()
    {
        
    }

    private void OnDestroy()
    {
        if (DetectableTargetManager.instance != null)
        {
            DetectableTargetManager.instance.Deregister(this);
        }
    }
}
