using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

class TrackedTarget
{
    public DetectableTarget Detectable;
    public Vector3 RawPosition;

    public float LastSensedTime;
    public float Awareness; // 0 - not aware; 0 - 1 rough idea; 1 - 2 likely target; 2 fully detected

    public bool UpdateAwareness(DetectableTarget target, Vector3 position, float awareness, float minAwareness)
    {
        var oldAwareness = Awareness;

        if(target != null)
        {
            Detectable = target;
        }

        Detectable = target;
        RawPosition = position;
        LastSensedTime = Time.time;
        Awareness = Mathf.Clamp(Mathf.Max(Awareness, minAwareness) + awareness, 0f, 2f);

        if(oldAwareness < 2f && Awareness >= 2f)
        {
            return true;
        }

        if(oldAwareness < 1f && Awareness >= 1f)
        {
            return true;
        }

        return false;
    }

    public bool DecayAwareness (float amount)
    {
        var oldAwareness = Awareness;
        Awareness -= amount;

        if (oldAwareness >= 2f && Awareness < 2f)
        {
            return true;
        }

        if (oldAwareness >= 1f && Awareness < 1f)
        {
            return true;
        }

        return Awareness <= 0f;
    }
}


[RequireComponent(typeof(EnemyAI))]
public class AwarenessSystem : MonoBehaviour
{
    [SerializeField] AnimationCurve VisionSensitivity;
    [SerializeField] float VisionMinimumAwareness = 1f;
    [SerializeField] float VisionAwarenessBuildRate = 10f;

    [SerializeField] float HearingMinimumAwareness = 0f;
    [SerializeField] float HearingAwarenessBuildRate = 0.5f;

    [SerializeField] float ProximityMinimumAwarness = 0f;
    [SerializeField] float ProximityAwarenessBuildRate = 1f;

    [SerializeField] float AwarenessDecayRate = 0.1f;

    Dictionary <GameObject, TrackedTarget> Targets = new Dictionary<GameObject, TrackedTarget>();
    EnemyAI LinkedAI;

    private void Start()
    {
        LinkedAI = GetComponent<EnemyAI>();    
    }

    private void Update()
    {
        List<GameObject> toCleanup = new List<GameObject>();

        foreach(var targetGO in Targets.Keys)
        {
            if(Targets[targetGO].DecayAwareness(AwarenessDecayRate * Time.deltaTime))
            {
                if (Targets[targetGO].Awareness <= 0f)
                {
                    toCleanup.Add(targetGO);
                }
                else
                {
                    Debug.Log("Threshold change for " + targetGO.name + " " + Targets[targetGO].Awareness);
                }
            }
        }

        //cleanup targets that are no longer detected
        foreach (var target in toCleanup)
        {
            Targets.Remove(target);
        }
    }

    void UpdateAwareness(GameObject targetGO, DetectableTarget target, Vector3 position, float awareness, float minAwareness)
    {
        //not in target
        if (!Targets.ContainsKey(targetGO))
        {
            Targets[targetGO] = new TrackedTarget();
        }

        //update target awareness
        if (Targets[targetGO].UpdateAwareness(target, position, awareness, minAwareness))
        {
            Debug.Log("Threshold change for " + targetGO.name + " " + Targets[targetGO].Awareness);
        }
    }

    public void ReportCanSee(DetectableTarget seen)
    {
        //determine where the target is in the FOV.
        var vectorToTarget = (seen.transform.position - LinkedAI.EyeLocation).normalized;
        var dotProduct = Vector3.Dot(vectorToTarget, LinkedAI.EyeDirection);

        //determine the awareness contribution.
        var awareness = VisionSensitivity.Evaluate(dotProduct) * VisionAwarenessBuildRate * Time.deltaTime;

        UpdateAwareness(seen.gameObject, seen, seen.transform.position, awareness, VisionMinimumAwareness);
    }

    public void ReportCanHear(GameObject source, Vector3 location, EHeardSoundCategory category, float intensity)
    {
        var awareness = intensity * HearingAwarenessBuildRate * Time.deltaTime;

        UpdateAwareness(source, null, location, awareness, HearingMinimumAwareness);
    }

    public void ReportProximity(DetectableTarget target)
    {
        var awareness = ProximityAwarenessBuildRate * Time.deltaTime;

        UpdateAwareness(target.gameObject, target, target.transform.position, awareness, ProximityMinimumAwarness);
    }
}
