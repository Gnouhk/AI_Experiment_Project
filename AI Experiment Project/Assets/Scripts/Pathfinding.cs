using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour
{
	Grid grid;

	void Awake()
	{
		grid = GetComponent<Grid>();
	}

	// Findpath function initiates the A* algorithm to find the shortest path between two point
	public void FindPath(PathRequest request, Action<PathResult> callback)
	{
		// Stopwatch for performance measurement.
		Stopwatch sw = new Stopwatch();
		sw.Start();

		Vector3[] waypoints = new Vector3[0];
		bool pathSuccess = false;

		// Get start and target node from world points.
		Node startNode = grid.NodeFromWorldPoint(request.pathStart);
		Node targetNode = grid.NodeFromWorldPoint(request.pathEnd);
		startNode.parent = startNode;

		// Check if both start and target nodes are walkable
		if (startNode.walkable && targetNode.walkable)
		{
			Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
			HashSet<Node> closedSet = new HashSet<Node>();
			openSet.Add(startNode);

			// A* algorithm implementation.
			while (openSet.Count > 0)
			{
				Node currentNode = openSet.RemoveFirst();
				closedSet.Add(currentNode);

				// Check if the target node is reached.
				if (currentNode == targetNode)
				{
					sw.Stop();
					print("Path found: " + sw.ElapsedMilliseconds + " ms");
					pathSuccess = true;
					break;
				}

				// Explore neighboring nodes
				foreach (Node neighbour in grid.GetNeighbours(currentNode))
				{
					//Skip unwalkable or already closed nodes.
					if (!neighbour.walkable || closedSet.Contains(neighbour))
					{
						continue;
					}

					// Calculate tentative movement cost to the neighbour.
					int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;

                    // Update the neighbour if a better path is found.
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
					{
						neighbour.gCost = newMovementCostToNeighbour;
						neighbour.hCost = GetDistance(neighbour, targetNode);
						neighbour.parent = currentNode;

                        // Add or update the neighbour in the open set
                        if (!openSet.Contains(neighbour))
							openSet.Add(neighbour);
						else
							openSet.UpdateItem(neighbour);
					}
				}
			}
		}

        // If a path is found, retrace and simplify the path
        if (pathSuccess)
		{
			waypoints = RetracePath(startNode, targetNode);
			pathSuccess = waypoints.Length > 0;
		}

        // Callback with the result, including waypoints and success status
        callback(new PathResult(waypoints, pathSuccess, request.callback));
	}

    // Retrace the path from end to start
    Vector3[] RetracePath(Node startNode, Node endNode)
	{
		List<Node> path = new List<Node>();
		Node currentNode = endNode;

        // Traverse from the end node to the start node
        while (currentNode != startNode)
		{
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}

        // Reverse the path and convert nodes to world positions
        Vector3[] waypoints = SimplifyPath(path);
		Array.Reverse(waypoints);
		return waypoints;

	}

    // Simplify the path by removing redundant waypoints
    Vector3[] SimplifyPath(List<Node> path)
	{
		List<Vector3> waypoints = new List<Vector3>();
		Vector2 directionOld = Vector2.zero;

        // Iterate through the nodes and add waypoints
        for (int i = 1; i < path.Count; i++)
		{
			Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);

            // Add waypoint if the direction changes
            if (directionNew != directionOld)
			{
				waypoints.Add(path[i].worldPosition);
			}
			directionOld = directionNew;
		}
		return waypoints.ToArray();
	}

    // Calculate the heuristic distance between two nodes
    int GetDistance(Node nodeA, Node nodeB)
	{
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        // Use diagonal distance calculation (14 times shorter distance for diagonals)
        if (dstX > dstY)
            if (dstX > dstY)
			return 14 * dstY + 10 * (dstX - dstY);
		return 14 * dstX + 10 * (dstY - dstX);
	}
}