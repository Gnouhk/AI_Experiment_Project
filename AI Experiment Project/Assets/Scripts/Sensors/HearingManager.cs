using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EHeardSoundCategory
{
    Footstep
}

public class HearingManager : MonoBehaviour
{
    public static HearingManager Instance { get; private set; } = null;

    public List<HearingSensor> AllSensors { get; private set; } = new List<HearingSensor>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple HearingManager found. Destroying " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public void Register(HearingSensor sensor)
    {
        AllSensors.Add(sensor);
    }

    public void Deregister(HearingSensor sensor)
    {
        AllSensors.Remove(sensor);
    }

    public void OnSoundEmited(Vector3 location, EHeardSoundCategory category, float intesify) 
    {

        // notify sensor
        foreach (var sensor in AllSensors)
        {
            sensor.OnHeardSound(location, category, intesify);
        }
    }
}
