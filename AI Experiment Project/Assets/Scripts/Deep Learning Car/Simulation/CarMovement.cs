using System.Collections;
using UnityEngine;
using VehicleBehaviour;

public class CarMovement : MonoBehaviour
{
    #region Members

    // Event when the car hit a wall
    public event System.Action HitWall;

    // Movement
    private WheelVehicle car;
    private WheelCollider wheel;

    // Movement constants
    private const float MAX_VEL = 10f;
    private const float ACCELERATION = 8f;
    private const float VEL_FRICT = 2f;
    private const float TURN_SPEED = 100;

    private CarController controller;

    public float Velocity { get; private set; }
    public Quaternion Rotation { get; private set; }

    private double horizontalInput, verticalInput;

    public double HorizontalInput
    {
        get => horizontalInput;
        set
        {
            horizontalInput = value;
            if (car != null) car.Throttle = (float)value; // Cast to float if necessary
        }
    }

    public double VerticalInput
    {
        get => verticalInput;
        set
        {
            verticalInput = value;
            if (car != null) car.Steering = (float)value; // Cast to float if necessary
        }
    }

    public double[] CurrentInputs 
    { 
        get { return new double[] { HorizontalInput, VerticalInput }; } 
    }

    #endregion

    #region Constructors

    private void Start()
    {
        controller = GetComponent<CarController>();
        car = GetComponent<WheelVehicle>();
        wheel = GetComponentInChildren<WheelCollider>();
    }

    #endregion

    #region Methods

    private void FixedUpdate()
    {
        if (controller != null && controller.UseUserInput)
        {
            CheckInput();
        }

        ApplyInput();

        ApplyVelocity();

        ApplyFriction();
    }

    // Check for user input
    private void CheckInput()
    {
        HorizontalInput = car.Throttle;
        VerticalInput = car.Steering;
    }

    // Applies the currently set input
    private void ApplyInput()
    { 

        // Cap input
        if (VerticalInput > 1)
        {
            VerticalInput = 1;
        } 
        else if (VerticalInput < -1)
        {
            VerticalInput = -1;
        }

        if(HorizontalInput > 1)
        {
            HorizontalInput = 1;
        }
        else if (HorizontalInput < -1)
        {
            HorizontalInput = -1;
        }

        // Car can accelerate furthur if velocity is lower than engineForce * MAX_VEL
        bool canAccelerate = false;

        if (VerticalInput < 0)
        {
            canAccelerate = Velocity > VerticalInput * MAX_VEL;
        }
        else if (VerticalInput > 0)
        {
            canAccelerate = Velocity < VerticalInput * MAX_VEL;
        }

        // Set velocity 
        if (canAccelerate)
        {
            Velocity += (float)VerticalInput * ACCELERATION * Time.deltaTime;

            // Cap velocity
            if (Velocity > MAX_VEL)
            {
                Velocity = MAX_VEL;
            }
            else if (Velocity < -MAX_VEL)
            {
                Velocity = -MAX_VEL;
            }
        }

        // Set rotation
        Rotation = transform.rotation;
        Rotation *= Quaternion.AngleAxis((float) - HorizontalInput * TURN_SPEED * Time.deltaTime, new Vector3(0, 0, 1));
    }

    // Sets the engine and turning input according to the given values
    public void SetInputs(double[] input)
    {
        HorizontalInput = input[0];
        VerticalInput = input[1];
    }

    // Applies the current velocity to the position of the car
    private void ApplyVelocity()
    {
        Vector3 direction = new Vector3(0, 1, 0);
        transform.rotation = Rotation;
        direction = Rotation * direction;

        this.transform.position += direction * Velocity * Time.deltaTime;
    }

    // Applies some friction to velocity
    private void ApplyFriction()
    {
        if (VerticalInput == 0)
        {
            if (Velocity > 0)
            {
                Velocity -= VEL_FRICT * Time.deltaTime;
                if(Velocity < 0)
                {
                    Velocity = 0;
                }
            }
            else if (Velocity < 0)
            {
                Velocity += VEL_FRICT * Time.deltaTime;
                if(Velocity > 0)
                {
                    Velocity = 0;
                }
            }
        }
    }

    // Triggered when collision was detected
    private void OnCollisionEnter()
    {
        if(HitWall != null)
        {
            HitWall();
        }
    }

    // Stop all current movement of the car
    public void Stop()
    {
        Velocity = 0;
        Rotation = Quaternion.AngleAxis(0, new Vector3(0, 0, 1));
        car.enabled = false;
        wheel.enabled = false;
    }

    #endregion
}
