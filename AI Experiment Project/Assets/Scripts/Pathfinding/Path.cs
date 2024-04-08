using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    // Array to store waypoints along the path
    public readonly Vector3[] lookPoints;

    // Array to store turn boundaries along the path
    public readonly Line[] turnBoundaries;

    // Index indicating the finish line in turn boundaries
    public readonly int finishLineIndex;

    // Index indicating where the unit should start slowing down
    public readonly int slowDownIndex;

    // Constructor for initializing the path
    public Path(Vector3[] waypoints, Vector3 startPos, float turnDst, float stoppingDst)
	{
		lookPoints = waypoints;
		turnBoundaries = new Line[lookPoints.Length];
		finishLineIndex = turnBoundaries.Length - 1;

        // Convert the starting position to a 2D vector																						//=====================================================================================//
        Vector2 previousPoint = V3ToV2(startPos);																							// The constructor initializes the Path object with an array of waypoints, a           //
																																			// starting position, a turn distance (turnDst), and a stopping distance (stoppingDst).//																														//
        // Iterate through waypoints to create turn boundaries																				//                                                                                     //
        for (int i = 0; i < lookPoints.Length; i++)                                                                                         // It calculates turn boundaries based on the waypoints and creates lines to represent //
        {                                                                                                                                   // these boundaries.                                                                   //
            Vector2 currentPoint = V3ToV2(lookPoints[i]);                                                                                   //=====================================================================================//
            Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;															
            // Calculate turn boundary point based on whether it's the finish line
            Vector2 turnBoundaryPoint = (i == finishLineIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDst;
			turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDst);

			previousPoint = turnBoundaryPoint;
		}

        // Calculate the distance from the end for slowing down
        float dstFromEndPoint = 0;
		for (int i = lookPoints.Length - 1; i > 0; i--)
		{
			dstFromEndPoint += Vector3.Distance(lookPoints[i], lookPoints[i - 1]);

            // Set the slowDownIndex when the distance exceeds the stopping distance
            if (dstFromEndPoint > stoppingDst)
			{
				slowDownIndex = i;
				break;
			}
		}
	}

    // Helper function to convert Vector3 to Vector2
    Vector2 V3ToV2(Vector3 v3)
	{
		return new Vector2(v3.x, v3.z);
	}

    // Draw the path with Gizmos for debugging
    public void DrawWithGizmos()
	{
		Gizmos.color = Color.black;

        // Draw cubes at each waypoint
        foreach (Vector3 p in lookPoints)
		{
			Gizmos.DrawCube(p + Vector3.up, Vector3.one);
		}
	}
}