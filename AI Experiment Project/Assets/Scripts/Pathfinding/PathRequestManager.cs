using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

public class PathRequestManager : MonoBehaviour
{
    // Queue to store pathfinding results
    Queue<PathResult> results = new Queue<PathResult>();

    // Static instance of PathRequestManager for easy access
    static PathRequestManager instance;
	Pathfinding pathfinding;

    // Initialize the static instance and reference to Pathfinding
    void Awake()
	{
		instance = this;
		pathfinding = GetComponent<Pathfinding>();
	}

    // Update function to process pathfinding results
    private void Update()
    {
        // Check if there are results in the queue
        if (results.Count > 0)
		{
			int itemsInQueue = results.Count;

            // Lock the queue to avoid conflicts with multithreading
            lock (results)
			{

                // Dequeue and process each result
                for (int i = 0; i < itemsInQueue; i++)
				{
					PathResult result = results.Dequeue();
					result.callback(result.path, result.success);
				}
			}
		}
    }

    // Static function to request a path
    public static void RequestPath(PathRequest request)
	{
        // Create a thread to run the pathfinding process
        ThreadStart threadStart = delegate
		{
			instance.pathfinding.FindPath(request, instance.FinishedProcessingPath);
		};

        // Invoke the thread
        threadStart.Invoke();
	}

    // Callback function called when pathfinding is finished
    public void FinishedProcessingPath(PathResult result)
	{
        // Lock the results queue and enqueue the result
        lock (results)
		{
			results.Enqueue(result);
		}
	}


}

// Struct to store pathfinding results
	public struct PathResult
	{
		public Vector3[] path;
		public bool success;
        public Action<Vector3[], bool> callback;

		// Constructor for initializing PathResult
		public PathResult(Vector3[] path, bool success, Action<Vector3[], bool> callback)
		{
			this.path = path;
			this.success = success;
			this.callback = callback;
		}
    }

// Struct to represent a pathfinding request
	public struct PathRequest
	{
		public Vector3 pathStart;
		public Vector3 pathEnd;
		public Action<Vector3[], bool> callback;

		// Constructor for initializing PathRequest
		public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback)
		{
			pathStart = _start;
			pathEnd = _end;
			callback = _callback;
		}

	}