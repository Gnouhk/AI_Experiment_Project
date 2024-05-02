using UnityEngine;

public class Sensor : MonoBehaviour
{
    #region Members

    [SerializeField] private LayerMask LayerToSense;
    [SerializeField] private GameObject Sphere;

    private const float MAX_DIST = 10f;
    private const float MIN_DIST = 0.01f;

    private Vector3 _sensorEndPosition;

    // The current sensor readings in percent of maximum distance
    public float Output { get; private set; }

    #endregion

    #region Constructor
    private void Start()
    {
        Sphere.gameObject.SetActive(true);
    }

    #endregion

    #region Methods

    private void FixedUpdate()
    {
        // Calculate direction of sensor
        Vector3 direction = Sphere.transform.position - this.transform.position;
        direction.Normalize();

        // Send raycast into the direction of the sensor
        RaycastHit hit;
        bool hasHit = Physics.Raycast(this.transform.position, direction, out hit, MAX_DIST, LayerToSense);

        // Check distance
        if (!hasHit)
        {
            hit.distance = MAX_DIST;
        }
        else if (hit.distance < MIN_DIST)
        {
            hit.distance = MIN_DIST;
        }

        this.Output = hit.distance; // Transform to percent of max distance if necessary
        Sphere.transform.position = this.transform.position + direction * hit.distance;
    }

    public void Hide()
    {
        Sphere.gameObject.SetActive(false);
    }

    public void Show()
    {
        Sphere.gameObject.SetActive(true);
    }

    #endregion
}
