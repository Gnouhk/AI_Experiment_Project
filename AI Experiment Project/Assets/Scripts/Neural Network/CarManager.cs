using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.InteropServices;
using System.Resources;

public class CarManager : MonoBehaviour
{
    public List<AgentMovement> allCars;
    private int carsCount = 0;
    private float bestReward = float.MinValue;
    private (float[,], float[])[] bestWeightsAndBiases;

    private int generationSize = 10;
    private int currentGenerationCount = 0;
    private List<float> generationRewards = new List<float>();
    private List<(float[,], float[])[]> generationWeightsAndBiases = new List<(float[,], float[])[]>();

    private void Start()
    {
        bestWeightsAndBiases = null;
    }

    private void Update()
    {
        //this only call the update method when 10 cars have been evaluated
        if(carsCount >= 10)
        {
            UpdateCarWeightsAndBiases();
            carsCount = 0; //reset the counter for the next gen.
        }
    }
    public void OnCarDestroyed (AgentMovement destroyedCar)
    {
        currentGenerationCount++;
        generationRewards.Add(destroyedCar.CalculatedReward());
        generationWeightsAndBiases.Add(destroyedCar.neuralNetwork.GetWeightsAndBiases());

        if(currentGenerationCount >= generationSize)
        {
            int bestIndex = generationReward.IndexOf(generationRewards.Max());
            (float[,], float[])[] bestWeightsAndBiases = generationWeightsAndBiases[bestIndex];
        }
    }

    private void UpdateCarWeightsAndBiases()
    {
        //find the best performing car based on the reward
        foreach(var car in allCars)
        {
            var currentReward = car.CalculatedReward();
            if(currentReward > bestReward)
            {
                bestReward = currentReward;
                bestWeightsAndBiases = car.neuralNetwork.GetWeightsAndBiases();
            }
        }

        //apply weights and biases from the best car to all other cars.
        if(bestWeightsAndBiases != null)
        {
            foreach (var car in allCars)
            {
                car.neuralNetwork.SetWeightsAndBiases(bestWeightsAndBiases);
            }
            //reset the best performance for the next gen
            bestReward = float.MinValue;
            bestWeightsAndBiases = null;
        }
    }
}
