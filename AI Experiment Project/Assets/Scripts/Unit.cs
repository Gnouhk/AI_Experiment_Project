using UnityEngine;
using System.Collections;

// Represents a unit that can navigate a path in the game world
public class Unit : MonoBehaviour
{
    // Constants for path update settings
    const float minPathUpdateTime = .2f;
	const float pathUpdateMoveThreshold = .5f;

    // Public properties
    public Transform target;			// Target for the unit to move forward.
	public float speed = 20;			// Movement speed of the unit
	public float turnSpeed = 3;			// Rotation speed of the unit
	public float turnDst = 5;			// Distance at which the unit starts to turn
	public float stoppingDst = 10;		// Distance at which the unit stops.

	// Calculated path for the unit
	Path path;

	void Start()
	{
		StartCoroutine(UpdatePath());
	}

    // Callback method for when a path is found
    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
	{
        // Check if a valid path was found
        if (pathSuccessful)
		{
            // Create a new path object using the waypoints and path details
            path = new Path(waypoints, transform.position, turnDst, stoppingDst);

            // Stop the previous path-following coroutine and start a new one
            StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

    // Coroutine for updating the path at regular intervals
    IEnumerator UpdatePath()
	{
        // Delay the path request for a short time to avoid unnecessary requests at startup
        if (Time.timeSinceLevelLoad < .3f)
		{
			yield return new WaitForSeconds(.3f);
		}

        // Request a path from the current position to the target position
        PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));

        // Variables for tracking movement threshold
        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
		Vector3 targetPosOld = target.position;

        // Loop continuously to check for movement and update the path
        while (true)
		{
			yield return new WaitForSeconds(minPathUpdateTime);

            // Check if the target has moved beyond the movement threshold
            if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
			{
                // If so, request a new path
                PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
				targetPosOld = target.position;
			}
		}
	}

    // Coroutine for following the calculated path
    IEnumerator FollowPath()
	{
        // Variable for tracking whether the unit is still following the path
        bool followingPath = true;

        // Index to track the current waypoint in the path
        int pathIndex = 0;

        // Rotate towards the first waypoint in the path
        transform.LookAt(path.lookPoints[0]);

        // Speed percentage for dynamic speed adjustment
        float speedPercent = 1;

        // Loop while the unit is still following the path
        while (followingPath)
		{
			Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);

            // Check if the unit has crossed the current turn boundary
            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
			{
				if (pathIndex == path.finishLineIndex)
				{
					followingPath = false;
					break;
				}
				else
				{
                    // Move to the next turn boundary
                    pathIndex++;
				}
			}

            // Continue following the path if the unit is still on track
            if (followingPath)
			{
                // If the unit is near the end of the path and has a stopping distance, adjust speed
                if (pathIndex >= path.slowDownIndex && stoppingDst > 0)
				{
                    // Calculate speed percentage based on distance to stopping point
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);

                    // If the speed is very low, stop following the path
                    if (speedPercent < 0.01f)
					{
						followingPath = false;
					}
				}

                // Rotate towards the next waypoint in the path
                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

                // Move the unit forward based on speed and speed percentage
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
			}
			yield return null;

		}
	}

    // Gizmos method for drawing path-related information in the editor
    public void OnDrawGizmos()
	{
		if (path != null)
		{
			path.DrawWithGizmos();
		}
	}
}