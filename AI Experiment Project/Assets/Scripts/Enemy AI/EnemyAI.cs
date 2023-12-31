using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro.EditorUtilities;

[RequireComponent(typeof(AwarenessSystem))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] float _visionConeAngle = 60f;
    [SerializeField] float _visionConeRange = 30f;
    [SerializeField] Color _visionConeColour = new Color(1f, 0f, 0f, 0.25f);

    [SerializeField] float _hearingRange = 20f;
    [SerializeField] Color _hearingRangeColour = new Color(1f, 1f, 0f, 0.25f);

    [SerializeField] float _proximityDetectionRange = 3f;
    [SerializeField] Color _proximityRangeColour = new Color(1f, 1f, 1f, 0.25f);

    public Vector3 EyeLocation => transform.position;
    public Vector3 EyeDirection => transform.forward;

    public float VisionConeAngle => _visionConeAngle;
    public float VisionConeRange => _visionConeRange;
    public Color VisionConeColour => _visionConeColour;

    public float HearingRange => _hearingRange;
    public Color HearingRangeColour => _hearingRangeColour;

    public float ProximityDetectionRange => _proximityDetectionRange;
    public Color ProximityDetectionColour => _proximityRangeColour;

    public float CosVisionConeAngle { get; private set; } = 0f;

    AwarenessSystem Awareness;

    private void Awake()
    {
        CosVisionConeAngle = Mathf.Cos(VisionConeAngle * Mathf.Deg2Rad);
        Awareness = GetComponent<AwarenessSystem>();
    }

    public void ReportCanSee(DetectableTarget seen)
    {
        Awareness.ReportCanSee(seen);
    }

    public void ReportCanHear(GameObject source, Vector3 location, EHeardSoundCategory category, float intensity)
    {
        Awareness.ReportCanHear(gameObject, location, category, intensity);
    }

    public void ReportProximity(DetectableTarget target)
    {
        Awareness.ReportProximity(target);
    }

    public void OnSus()
    {
        Debug.Log("I hear you");
    }

    public void OnDetected(GameObject target)
    {
        Debug.Log("I see you " + target.gameObject.name);
    }

    public void OnFullyDetected(GameObject target)
    {
        Debug.Log("!!! " + target.gameObject.name);
    }

    public void OnLostDetect(GameObject target)
    {
        Debug.Log("Where are you " + target.gameObject.name);
    }

    public void OnLostSus()
    {
        Debug.Log("Where did you go?");
    }

    public void OnFullyLost()
    {
        Debug.Log("Must be nothing");
    }
}

[CustomEditor(typeof(EnemyAI))]
public class EnemyAIEditor : Editor
{
    public void OnSceneGUI()
    {
        var ai = target as EnemyAI;

        //detection range
        Handles.color = ai.ProximityDetectionColour;
        Handles.DrawSolidDisc(ai.transform.position, Vector3.up, ai.ProximityDetectionRange);

        //hearing range
        Handles.color = ai.HearingRangeColour;
        Handles.DrawSolidDisc(ai.transform.position, Vector3.up, ai.HearingRange);

        //vision cone
        Vector3 startPoint = Mathf.Cos(-ai.VisionConeAngle * Mathf.Deg2Rad) * ai.transform.forward + 
                             Mathf.Sin(-ai.VisionConeAngle * Mathf.Deg2Rad) * ai.transform.right;

        //draw
        Handles.color = ai.VisionConeColour;
        Handles.DrawSolidArc(ai.transform.position, Vector3.up, startPoint, ai.VisionConeAngle * 2f, ai.VisionConeRange);
    }
}
