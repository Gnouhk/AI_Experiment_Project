using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using VehicleBehaviour;

public class AgentMovement : MonoBehaviour
{
    public WheelVehicle wheelVehicle;
    public GameObject carPrefab;
    public Transform spawnPoint;

    public float deadTimer = 7;
    public float viewDistance = 50f;
    public NN neuralNetwork;

    public float curentDeadTimer;

    //number of raycasts and spread angle
    int numRaycasts = 5;
    float angleBetweenRaycasts = 30f;

    //define other state components
    public float currentCarSpeed;
    public Vector3 relativeCheckpointPosition; //position of the next checkpoint
    public List<Transform> checkpoints;
    private int currentCheckpointIndex = 0;
    


    private void Awake()
    {
        wheelVehicle = GetComponent<WheelVehicle>();
        ResetDeadTimer();
    }

    public void Update()
    {
        UpdateDeadTimer();
    }

    public void FixedUpdate()
    {
        float[] currentState = GetState();

        //use currentState as input for the neural network
        //and use the output to control the car
        //placeholder to represent getting the output from the neural network
        float[] nnOutput = new float[2];

        //move the car
        ApplyActions(nnOutput);
    }
    
    //function to collect state info
    public float[] GetState()
    {
        List<float> state = new List<float>();

        //raycasts
        RaycastHit hit;
        for (int i = 0; i < numRaycasts; i++)
        {
            float angle = ((2 * i + 1 - numRaycasts) * angleBetweenRaycasts / 2);
            Quaternion rotation = Quaternion.AngleAxis(angle, transform.up);
            Vector3 rayDirection = rotation * transform.forward;

            Vector3 rayStart = transform.position + Vector3.up * 0.5f;

            if (Physics.Raycast(rayStart, rayDirection, out hit, viewDistance))
            {
                //draw raycast
                UnityEngine.Debug.DrawRay(rayStart, rayDirection * hit.distance, Color.blue);

                //normalize the distance and add to state
                state.Add(hit.distance / viewDistance);
            }
            else
            {
                //draw raycast
                UnityEngine.Debug.DrawRay(rayStart, rayDirection * hit.distance, Color.blue);

                //if nothing is hit, add maximum distance
                state.Add(1f);
            }
        }

        //Dunno what is the maximum speed of the car, but assuming 100 is the top speed.
        float normalizedSpeed = currentCarSpeed / 100f;
        state.Add(normalizedSpeed);

        //Add normalized relative checkpoint position
        Vector3 normalizedCheckpointPosition = relativeCheckpointPosition / 100f;
        state.Add(normalizedCheckpointPosition.x);
        state.Add(normalizedCheckpointPosition.y);
        state.Add(normalizedCheckpointPosition.z);

        return state.ToArray();
    }

    public void ApplyActions(float[] neuralNetworkOutput)
    {
        // clamp the values in case the outpud values outside the -1 and 1 range.
        float steering = Mathf.Clamp(neuralNetworkOutput[0], -1f, 1f);
        float throttle = Mathf.Clamp(neuralNetworkOutput[1], -1f, 1f);

        //apply actions to the car

        //move
        wheelVehicle.Steering = steering * wheelVehicle.SteerAngle;
        wheelVehicle.Throttle = throttle;

        UnityEngine.Debug.Log($"Moving car with Throttle: {throttle}, Steering: {steering}");

    }

    private void OnTriggerEnter(Collider other)
    {
        //if the agent collides with a checkpoint, renew the dead timer.
        if(other.CompareTag("Checkpoint"))
        {
            //check if the checkpoint is the next one we're expecting
            if(checkpoints[currentCheckpointIndex] == other.transform)
            {
                //update the checkpoint index to the next one
                currentCheckpointIndex = (currentCheckpointIndex + 1) % checkpoints.Count;

                //reset the dead timer.
                ResetDeadTimer();
            }
        }
    }

    public void RespawnCar()
    {
        GameObject newCar = Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);

        AgentMovement newCarMovement = newCar.GetComponent<AgentMovement>();
        if(newCarMovement != null)
        {
            newCarMovement.Initialize(carPrefab, spawnPoint);
        }
    }

    public void UpdateDeadTimer()
    {
        // count down dead timer
        curentDeadTimer -= Time.deltaTime;

        if(curentDeadTimer <= 0)
        {
            Destroy(gameObject);
            RespawnCar();
        }
    }

    public void ResetDeadTimer()
    {
        curentDeadTimer = deadTimer;
    }

    public void Initialize(GameObject prefab, Transform spawn)
    {
        carPrefab = prefab;
        spawnPoint = spawn;
    }
}
