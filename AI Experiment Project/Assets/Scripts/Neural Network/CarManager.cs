using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CarManager : MonoBehaviour
{
    public List<AgentMovement> allCars;

    private void Update()
    {
        UpdateCarWeightsAndBiases();
    }

    private void UpdateCarWeightsAndBiases()
    {
        //find the best performing car based on the reward
        AgentMovement bestCar = allCars.OrderByDescending(car => car.CalculatedReward()).FirstOrDefault();

        if (bestCar != null)
        {
            //retrive the weights and biases from the best car
            (float[,], float[]) [] bestWeightsAndBiases = bestCar.neuralNetwork.GetWeightsAndBiases();

            //apply weights and biases to all other cars
            foreach(var car in allCars)
            {
                if(car != bestCar)
                {
                    car.neuralNetwork.SetWeightsAndBiases(bestWeightsAndBiases);
                }
            }
        }
    }
}
