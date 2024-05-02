using System.Collections.Generic;
using UnityEngine;
using System;


public class TrackManager : MonoBehaviour
{
    #region Members

    public static TrackManager Instance { get; private set; }

    private CheckPoint[] checkpoints;

    // Car used to create new cars and to set start position
    public CarController PrototypeCar;

    private Vector3 startPosition;
    private Quaternion startRotation;

    // Struct for storing the current cars and their position on the track
    private class RaceCar
    {
        public RaceCar (CarController car = null, uint checkpointIndex = 1)
        {
            this.Car = car;
            this.CheckpointIndex = checkpointIndex;
        }

        public CarController Car;
        public uint CheckpointIndex;
    }

    private List<RaceCar> cars = new List<RaceCar>();

    // The amount of cars currently on the track
    public int CarCount { get { return cars.Count; } }

    #region Best and Second Best

    private CarController bestCar = null;

    // The current best car (furhest in the track)
    public CarController BestCar
    {
        get { return bestCar; }
        private set
        {
            if (bestCar != value)
            {
                // Set previous best to be second best now
                CarController previousBest = bestCar;
                bestCar = value;
                if (BestCarChanged != null) 
                {
                    BestCarChanged(bestCar);
                }

                SecondBestCar = previousBest;
            }
        }
    }

    // Event for when the best car has changed
    public event System.Action<CarController> BestCarChanged;

    private CarController secondBestCar = null;

    // The current second best car (furthest in the track)
    public CarController SecondBestCar
    {
        get { return secondBestCar; }
        private set
        {
            if (SecondBestCar != value)
            {
                secondBestCar = value;
                if (SecondBestCarChanged != null)
                {
                    SecondBestCarChanged(SecondBestCar);
                }
            }
        }
    }

    //Event for when the second best car has changed
    public event System.Action <CarController> SecondBestCarChanged;

    #endregion

    public float TrackLength { get; private set; }

    #endregion

    #region Constructors

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple instance of TrackManager are not allowed in one scene");
            return;
        }

        Instance = this;

        // Get all checkpoints
        checkpoints = GetComponentsInChildren<CheckPoint>();

        // Set start position and hide prototype
        startPosition = PrototypeCar.Transform.position;
        startRotation = PrototypeCar.Transform.rotation;
        PrototypeCar.gameObject.SetActive(false);

        CalculateCheckpointPercentage();
    }

    #endregion

    #region Methods

    private void Update()
    {
        // Update reward for each enabled car on the track
        for (int i = 0; i < cars.Count; i++)
        {
            RaceCar car = cars[i];  

            if (car.Car.enabled)
            {
                car.Car.CurrentCompletionReward = GetCompletePerc(car.Car, ref car.CheckpointIndex);

                // Update best
                if(BestCar == null || car.Car.CurrentCompletionReward >= BestCar.CurrentCompletionReward)
                {
                    BestCar = car.Car;
                } 
                else if (SecondBestCar == null || car.Car.CurrentCompletionReward >= SecondBestCar.CurrentCompletionReward)
                {
                    SecondBestCar = car.Car;
                }
            }
        }
    }

    public void SetCarAmount(int amount)
    {
        if (amount < 0) throw new ArgumentException("Amount may not be less than zero");

        if (amount == CarCount ) return;

        if (amount > cars.Count)
        {
            // Add new cars
            for (int toBeAdded = amount - cars.Count; toBeAdded > 0; toBeAdded--)
            {
                GameObject carCopy = Instantiate(PrototypeCar.gameObject);

                carCopy.transform.position = startPosition;
                carCopy.transform.rotation = startRotation;

                CarController controllerCopy = carCopy.GetComponent<CarController>();
                cars.Add(new RaceCar(controllerCopy, 1));

                carCopy.SetActive(true);
            }
        }
        else if (amount < cars.Count)
        {
            // Remove existing cars
            for (int toBeRemoved = cars.Count - amount; toBeRemoved > 0; toBeRemoved--)
            {
                RaceCar last = cars[cars.Count - 1];
                cars.RemoveAt(cars.Count - 1);

                Destroy(last.Car.gameObject);
            }
        }
    }

    // Restarts all cars and puts them at the track start
    public void Restart()
    {
        foreach (RaceCar car in cars)
        {
            car.Car.transform.position = startPosition;
            car.Car.transform.rotation = startRotation;
            car.Car.Restart();
            car.CheckpointIndex = 1;
        }

        BestCar = null;
        SecondBestCar = null;
    }

    // Returns an Enumerator for iterator through all cars currently on the track
    public IEnumerator<CarController> GetCarEnumerator()
    {
        for (int i = 0; i < cars.Count; i++)
        {
            yield return cars[i].Car;
        }
    }

    // Calculates the percentage of the complete track a checkpoint account for
    private void CalculateCheckpointPercentage()
    {
        checkpoints[0].AccumulatedDistance = 0; // Start

        // Iterate over remaining checkpoints and set distance to previous and accumulated track distance
        for(int i = 0; i < checkpoints.Length; i++)
        {
            checkpoints[i].DistanceToPrevious = Vector3.Distance(checkpoints[i].transform.position, checkpoints[i - 1].transform.position);
            checkpoints[i].AccumulatedDistance = checkpoints[i - 1].AccumulatedDistance + checkpoints[i].AccumulatedDistance;
        }

        // Set track length to accumulated distance of last checkpoint
        TrackLength = checkpoints[checkpoints.Length - 1].AccumulatedDistance;

        // Calculate reward value for each checkpoint
        for (int i = 1; i < checkpoints.Length; i++)
        {
            checkpoints[i].RewardValue = (checkpoints[i].AccumulatedDistance / TrackLength) - checkpoints[i - 1].AccumulatedReward;
            checkpoints[i].AccumulatedReward = checkpoints[i - 1].AccumulatedReward + checkpoints[i].RewardValue;
        }
    }

    // Calculates the completion percentage of given car with given completed last checkpoint
    private float GetCompletePerc(CarController car, ref uint curCheckpointIndex)
    {
        // Already all checkpoint captured
        if(curCheckpointIndex >= checkpoints.Length)
        {
            return 1;
        }

        // Calculate distance to next checkpoint
        float checkPointDistance = Vector3.Distance(car.transform.position, checkpoints[curCheckpointIndex].transform.position);
        
        // Check if checkpoint can be capture
        if (checkPointDistance <= checkpoints[curCheckpointIndex].CaptureRadius)
        {
            curCheckpointIndex++;
            car.CheckpointCaptured(); // Inform car that it captured a checkpoint
            return GetCompletePerc(car, ref curCheckpointIndex); // Recursively check next checkpoint
        }
        else
        {
            // Return accumulated reward of last checkpoint + reward of distance to next checkpoint
            return checkpoints[curCheckpointIndex - 1].AccumulatedDistance + checkpoints[curCheckpointIndex].GetRewardValue(checkPointDistance);
        }
    }

    #endregion
}
