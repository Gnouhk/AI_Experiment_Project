using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using VehicleBehaviour;

public class AgentMovement : MonoBehaviour
{
    //script references
    public WheelVehicle wheelVehicle;
    public GameObject carPrefab;
    public Transform spawnPoint;
    public NN neuralNetwork;

    public float deadTimer = 7;
    public float viewDistance = 50f;
    public float curentDeadTimer;

    //number of raycasts and spread angle
    int numRaycasts = 5;
    float angleBetweenRaycasts = 30f;

    //define other state components
    public float currentCarSpeed;
    private bool hitObstacle = false;
    public float learningRate = 0.01f;
    public bool isBeingDestroyed = false;

    //checkpoints variable
    public List<Transform> checkpoints;
    public int currentCheckpointIndex = 0;
    public float lastDistanceToCheckpoint;
    private bool justPassedCheckpoint = false;
    public Vector3 relativeCheckpointPosition; //position of the next checkpoint

    private void Awake()
    {
        wheelVehicle = GetComponent<WheelVehicle>();
        ResetDeadTimer();
    }

    private void Start()
    {
        if (checkpoints.Count > 0)
        {
            lastDistanceToCheckpoint = Vector3.Distance(transform.position, checkpoints[currentCheckpointIndex].position);
        }
    }

    public void Update()
    {
        UpdateDeadTimer();
        hitObstacle = false;
    }

    public void FixedUpdate()
    {
        //update the relative position of the Snext checkpoint
        if(currentCheckpointIndex < checkpoints.Count)
        {
            Transform nextCheckpoint = checkpoints[currentCheckpointIndex];
            relativeCheckpointPosition = nextCheckpoint.position - transform.position;
        }

        float[] currentState = GetState();

        //use currentState as input for the neural network
        //and use the output to control the car
        //placeholder to represent getting the output from the neural network
        float[] nnOutput = neuralNetwork.Brain(currentState);

        //move the car
        ApplyActions(nnOutput);

        //update the reward
        CalculatedReward();
    }

    public void TrainAgent()
    {
        if(isBeingDestroyed)
        {
            UnityEngine.Debug.Log("Attempted to train during destruction process, skipping.");
            return; 
        }

        //get state, action, and reward
        float[] state = GetState();
        float[] action = neuralNetwork.Brain(state);
        float reward = CalculatedReward();

        // define the target output based on the reward
        float[] targetOutput = new float[] { reward };

        // train the neural network
        neuralNetwork.Train(state, targetOutput, learningRate);
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

    public void ApplyActions(float[] nnOutput)
    {
        // clamp the values in case the outpud values outside the -1 and 1 range.
        float steeringOutput = Mathf.Clamp(nnOutput[0], -1f, 1f);
        float throttleOutput = Mathf.Clamp(nnOutput[1], 0f, 1f);

        //map the max steering angle to 30
        float maxSteeringAngle = 30f;
        float steeringAngle = steeringOutput * maxSteeringAngle;
        
        //apply actions to the car
        //move
        wheelVehicle.Steering = steeringAngle;
        wheelVehicle.Throttle = throttleOutput;

        // Debug to check the applied values
        UnityEngine.Debug.Log($"Applied Steering: {steeringAngle}, Applied Throttle: {throttleOutput}");
    }

    public float CalculatedReward()
    {
        float reward = 0f;

        //calculated the reward
        float currentDistanceToCheckpoint = Vector3.Distance(transform.position, checkpoints[currentCheckpointIndex].position);
        float progress = lastDistanceToCheckpoint - currentDistanceToCheckpoint;

        //reward progress towards the checkpoint
        if(progress > 0)
        {
            reward += progress;
        }

        //update last distance for the next calculation
        lastDistanceToCheckpoint = currentDistanceToCheckpoint;

        //checkpoint passed reward
        if (JustPassedCheckpoint())
        {
            reward += 100f;
        }

        //time penalty
        reward -= 0.1f;

        //collision penalty
        if (HitObstacles())
        {
            reward -= 50f;
        }

        return reward;
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
                justPassedCheckpoint = true;

                //reset the dead timer.
                ResetDeadTimer();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Obstacles"))
        {
            hitObstacle = true;

            
        }
    }

    public bool JustPassedCheckpoint()
    {
        if(justPassedCheckpoint)
        {
            //reset the flag before returning true
            justPassedCheckpoint = false;
            return true;
        }
        return false;
    }

    public bool HitObstacles()
    {
        //reports whether the car hit an obstacles then reset the flag
        if(hitObstacle)
        {
            hitObstacle = false;
            return true;
        }
        return false;
    }

    public void RespawnCar()
    {
        if (isBeingDestroyed)
        {
            GameObject newCar = Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);

            AgentMovement newCarMovement = newCar.GetComponent<AgentMovement>();
            if (newCarMovement != null)
            {
                newCarMovement.Initialize(carPrefab, spawnPoint);
            }
        }
    }

    public void UpdateDeadTimer()
    {
        // count down dead timer
        curentDeadTimer -= Time.deltaTime;

        if(curentDeadTimer <= 0 && !isBeingDestroyed)
        {
            isBeingDestroyed = true;
            TrainAgent();
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
