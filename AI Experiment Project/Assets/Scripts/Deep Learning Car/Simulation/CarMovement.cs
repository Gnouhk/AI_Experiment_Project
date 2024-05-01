using System.Collections;
using UnityEngine;
using VehicleBehaviour;

public class CarMovement : MonoBehaviour
{
    #region Members

    // Event when the car hit a wall
    public event System.Action HitWall;

    // Movement constants
    private WheelVehicle car;

    #endregion
}
