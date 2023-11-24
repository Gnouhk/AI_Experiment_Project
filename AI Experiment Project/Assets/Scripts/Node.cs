using UnityEngine;
using System.Collections;

// Node class representing a node in a grid for pathfinding
public class Node : IHeapItem<Node> 
{
	//Properties
	public bool walkable;				// Whether the node is walkable
	public Vector3 worldPosition;		// World position of the node
	public int gridX;					// X coord 
	public int gridY;					// Y coord 
	public int movementPenalty;         // Penalty for traversing this node (terrain cost)

	//Cost values for pathfinding
    public int gCost;					// Cost from the start node to this node
	public int hCost;					// Heuristic cost from this node to target node
	public Node parent;                 // Parent node in the path

    // Heap index for the node in the open set (used in pathfinding)
    int heapIndex;

    // Constructor to initialize a node
    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _penalty) {
		walkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
		movementPenalty = _penalty;
	}

    // F cost property (sum of gCost and hCost)
    public int fCost {
		get {
			return gCost + hCost;
		}
	}

    // HeapIndex property implementation from IHeapItem interface
    public int HeapIndex {
		get {
			return heapIndex;
		}
		set {
			heapIndex = value;
		}
	}

    // CompareTo method implementation from IHeapItem interface
    public int CompareTo(Node nodeToCompare) {
        // Compare nodes based on their total cost (fCost)
        int compare = fCost.CompareTo(nodeToCompare.fCost);

        // If fCost is equal, compare based on heuristic cost (hCost)
        if (compare == 0) {
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}

        // Return the negation of the comparison result (min heap ordering)
        return -compare;
	}
}