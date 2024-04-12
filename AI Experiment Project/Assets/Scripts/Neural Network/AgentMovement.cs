using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehicleBehaviour;

public class AgentMovement : MonoBehaviour
{
    public WheelVehicle wheelVehicle;
    public float deadTimer = 7;
    public float FB = 0;
    public float LR = 0;
    public float viewDistance = 20;
    public NN neuralNetwork;
    public float[] distance = new float[6];

    public float curentDeadTimer;

    private void Awake()
    {
        wheelVehicle = GetComponent<WheelVehicle>();
        ResetDeadTimer();
    }

    public void Move(float FB, float LR)
    {
        LR = Mathf.Clamp(LR, -1, 1);
        FB = Mathf.Clamp(FB, -1, 1);

        Debug.Log($"Moving car with Throttle: {FB}, Steering: {LR}");

        //move the gawddamn car
        if (!wheelVehicle.isDead)
        {
            //move forward, backward
            wheelVehicle.Steering = LR * wheelVehicle.SteerAngle;
            wheelVehicle.Throttle = FB;
        }
    }

    public void Update()
    {
        UpdateDeadTimer();
    }

    public void FixedUpdate()
    {
        int numRaycasts = 5;
        float angleBetweenRaycasts = 30;

        RaycastHit hit;

        for(int i = 0; i < numRaycasts; i++)
        {
            float angle = ((2 * i + 1 - numRaycasts) * angleBetweenRaycasts / 2);
            
            //rotate the direction of the raycast by y - axis of the car
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 rayDirection = rotation * transform.forward * 1;

            Vector3 rayStart = transform.position + Vector3.up * 0.5f;

            if(Physics.Raycast(rayStart, rayDirection, out hit, viewDistance))
            {
                //draw raycast
                Debug.DrawRay(rayStart, rayDirection * hit.distance, Color.blue);

                if (hit.transform.gameObject.tag == "Wall")
                {
                    //use the length of the raycast as the distance to the wall
                    distance[i] = hit.distance / viewDistance;
                    Debug.Log("Raycast hit Wall");
                }
                else
                {
                    //if no wall is detected, set the distance to the maximum length of the raycast
                    distance[i] = 1;
                }
            }
            else
            {
                //draw raycast
                Debug.DrawRay(rayStart, rayDirection * viewDistance, Color.blue);

                distance[i] = 1;
            }
        }

        //setup inputs for the neural network
        float [] inputsToNN = distance;

        //setup outputs from the neural network
        float [] outputsFromNN = neuralNetwork.Brain(inputsToNN);

        //store the outputs from the neural network in a variables
        FB = outputsFromNN[0];
        LR = outputsFromNN[1];

        //move the car
        Move(FB, LR);
    }

    private void OnTriggerEnter(Collider col)
    {
        //if the agent collides with a checkpoint, renew the dead timer.
        if(col.gameObject.tag == "Checkpoint")
        {
            ResetDeadTimer();
        }
    }

    public void UpdateDeadTimer()
    {
        // count down dead timer
        curentDeadTimer -= Time.deltaTime;

        if(curentDeadTimer <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void ResetDeadTimer()
    {
        curentDeadTimer = deadTimer;
    }
}
