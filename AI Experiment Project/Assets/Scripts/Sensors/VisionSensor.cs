using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyAI))]

public class VisionSensor : MonoBehaviour
{
    [SerializeField] LayerMask detectionMask = ~0;

    EnemyAI linkedAI;

    private void Start()
    {
        linkedAI = GetComponent<EnemyAI>();
    }

    private void Update()
    {
        //check all candidates
        for (int index = 0; index < DetectableTargetManager.instance.allTargets.Count; index++)
        {
            var candidateTarget = DetectableTargetManager.instance.allTargets[index];

            //skip if the candidate is ourselves
            if (candidateTarget.gameObject == gameObject)
            {
                continue;
            }

            var vectorToTarget = candidateTarget.transform.position - linkedAI.EyeLocation;

            //if outside of range - enemy cannot see
            if(vectorToTarget.sqrMagnitude > (linkedAI.VisionConeRange * linkedAI.VisionConeRange))
            {
                continue;
            }

            //if outside of vision cone - enemy cannot see too.
            if(Vector3.Dot(vectorToTarget.normalized, linkedAI.EyeLocation) < linkedAI.CosVisionConeAngle)
            {
                continue;
            }

            //raycast
            RaycastHit hitResult;
            if (Physics.Raycast(linkedAI.EyeLocation, linkedAI.EyeDirection, out hitResult, 
                                linkedAI.VisionConeRange, detectionMask, QueryTriggerInteraction.Collide))
            {
                linkedAI.ReportCanSee(candidateTarget);
            }
        }
    }
}
