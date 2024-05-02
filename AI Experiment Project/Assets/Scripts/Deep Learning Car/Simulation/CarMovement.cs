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

    // Movement constants
    private const float MAX_VEL = 20f;
    private const float ACCELERATION = 8f;
    private const float VEL_FRICT = 2f;
    private const float TURN_SPEED = 100;

    private CarController controller;

    public float Velocity { get; private set; }
    public Quaternion Rotation { get; private set; }

    private double horizontalInput, verticalInput;

    public double[] CurrentInputs 
    { 
        get { return new double[] { horizontalInput, verticalInput }; } 
    }

    #endregion

    #region Constructors

    private void Start()
    {
        controller = GetComponent<CarController>();
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
        if (car != null)
        {
            horizontalInput = car.Throttle;
            verticalInput = car.Steering;
        }
    }

    // Applies the currently set input
    private void ApplyInput()
    {
        // Cap input
        if (verticalInput > 1)
        {
            verticalInput = 1;
        } 
        else if (verticalInput < -1)
        {
            verticalInput = -1;
        }

        if(horizontalInput > 1)
        {
            horizontalInput = 1;
        }
        else if (horizontalInput < -1)
        {
            horizontalInput = -1;
        }

        // Car can accelerate furthur if velocity is lower than engineForce * MAX_VEL
        bool canAccelerate = false;

        if (verticalInput < 0)
        {
            canAccelerate = Velocity > verticalInput * MAX_VEL;
        }
        else if (verticalInput > 0)
        {
            canAccelerate = Velocity < verticalInput * MAX_VEL;
        }

        // Set velocity 
        if (canAccelerate)
        {
            Velocity += (float)verticalInput * ACCELERATION * Time.deltaTime;

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
        Rotation *= Quaternion.AngleAxis((float) - horizontalInput * TURN_SPEED * Time.deltaTime, new Vector3(0, 0, 1));
    }

    // Sets the engine and turning input according to the given values
    public void SetInputs(double[] input)
    {
        horizontalInput = input[0];
        verticalInput = input[1];
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
        if (verticalInput == 0)
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
    }

    #endregion
}
