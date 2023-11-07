using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(EnemyAI))]
public class ProximitySensor : MonoBehaviour
{
    EnemyAI LinkedAI;

    void Start()
    {
        LinkedAI = GetComponent<EnemyAI>(); 
    }

    void Update()
    {
        for (int index = 0; index < DetectableTargetManager.instance.allTargets.Count; index++)
        {
            var candidateTarget = DetectableTargetManager.instance.allTargets[index];

            //skip if ourselves
            if(candidateTarget.gameObject == gameObject)
            {
                continue;
            }

            if(Vector3.Distance(LinkedAI.EyeLocation, candidateTarget.transform.position) <= LinkedAI.ProximityDetectionRange)
            {
                LinkedAI.ReportProximity(candidateTarget);
            }
        }
    }
}
