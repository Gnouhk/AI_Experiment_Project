using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

    private void Awake()
    {
        CosVisionConeAngle = Mathf.Cos(VisionConeAngle * Mathf.Deg2Rad);
    }

    public void ReportCanSee(DetectableTarget seen)
    {
        Debug.Log("Can see " + seen.gameObject.name);
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
