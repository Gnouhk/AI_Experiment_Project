using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectableTargetManager : MonoBehaviour
{
    public static DetectableTargetManager instance { get; private set; } = null;

    public List<DetectableTarget> allTargets { get; private set; } = new List<DetectableTarget>();

    private void Awake()
    {
        if(instance != null)
        {
            Debug.LogError("Multiple DetectableTargetManager found. Destroying " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Register(DetectableTarget target)
    {
        allTargets.Add(target);
    }

    public void Deregister(DetectableTarget target)
    {
        allTargets.Remove(target);
    }
}
