using UnityEngine;
using VehicleBehaviour;

public class CarController : MonoBehaviour
{
    #region Members

    // Used for unique ID generation
    private static int idGenerator = 0;

    // Returns the next unique id in the sequence.
    private static int NextID { get { return idGenerator++; } }

    // Maximum delay in seconds between the collection of two checkpoints until this car dies
    private const float MAX_CHECKPOINT_DELAY = 5;

    public Agent Agent { get; set; }

    public float CurrentCompletionReward
    {
        get { return Agent.Genotype.Evaluation; }
        set { Agent.Genotype.Evaluation = value; }
    }

    // Whether this car is controllable by user
    public bool UseUserInput = false;

    public CarMovement Movement { get; private set; }
    public WheelCollider Wheel { get; private set; }
    public WheelVehicle Car { get; private set; }

    // The current inputs for controlling the CarMovement components
    public double[] CurrentControlInputs
    {
        get { return Movement.CurrentInputs; }
    }

    public Transform Transform { get; private set; }
    private Sensor[] sensors;
    private float timeSinceLastCheckpoint;

    #endregion

    #region Constructions

    private void Awake()
    {
        Movement = GetComponent<CarMovement>();
        Transform = GetComponent<Transform>();
        sensors = GetComponentsInChildren<Sensor>();
        Wheel = GetComponentInChildren<WheelCollider>();
        Car = GetComponent<WheelVehicle>();
    }

    private void Start()
    {
        Movement.HitWall += Die;

        // Set name to be unique
        this.name = "Car (" + NextID + ")";
    }

    #endregion

    #region Methods

    // Restart this car, making it movable again
    public void Restart()
    {
        Movement.enabled = true;
        timeSinceLastCheckpoint = 0;
        Wheel.enabled = true;
        Car.enabled = true;


        foreach (Sensor s in sensors)
        {
            s.Show();
        }

        Agent.Reset();
        this.enabled = true;
    }

    private void Update()
    {
        timeSinceLastCheckpoint += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        // Get control inputs from Agent
        if (!UseUserInput)
        {
            // Get readings from sensors
            double[] sensorOutput = new double[sensors.Length];

            for (int i = 0; i < sensors.Length; i++)
            {
                sensorOutput[i] = sensors[i].Output;
            }

            double[] controlInputs = Agent.FNN.ProcessInputs(sensorOutput);
            Movement.SetInputs(controlInputs);
        }

        if(timeSinceLastCheckpoint > MAX_CHECKPOINT_DELAY)
        {
            Die();
        }
    }

    // Makes this car die
    private void Die()
    {
        this.enabled = false;
        Movement.Stop();
        Movement.enabled = false;


        foreach (Sensor s in sensors)
        {
            s.Hide();
        }

        Agent.Kill();
    }

    public void CheckpointCaptured()
    {
        UnityEngine.Debug.Log("Checkpoint captured");
        timeSinceLastCheckpoint = 0;
    }

    #endregion
}
