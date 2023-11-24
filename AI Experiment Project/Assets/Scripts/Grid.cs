using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegion;
    public int obstacleProximityPenalty = 10;

    // Dictionary to map terrain layer masks to movement penalties
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
    LayerMask walkableMask;

    // 2D array to store grid nodes
    Node[,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    int penaltyMin = int.MaxValue;
    int penaltyMax = int.MinValue;

    void Awake()
    {
        // Calculate grid properties
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        // Populate the walkable regions dictionary
        foreach (TerrainType region in walkableRegion)
        {
            walkableMask.value |= region.terrainMask.value;
            walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
        }

        CreateGrid();
    }

    // Property to get the maximum size of the grid
    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    // Method to create the grid of nodes
    void CreateGrid()
    {
        // Initialize the grid array
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        // Populate the grid with nodes
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);

                // Check if the node is walkable based on sphere cast
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

                // Initialize movement penalty
                int movementPenalty = 0;

                if(walkable)
                {
                    // Raycast to determine the terrain type and its penalty
                    Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                    RaycastHit hit;

                    if(Physics.Raycast(ray, out hit, 100, walkableMask))
                    {
                        walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    }

                    // Apply obstacle proximity penalty if not walkable
                    if (!walkable)
                    {
                        movementPenalty += obstacleProximityPenalty;
                    }
                }
                // Create a new node and add it to the grid
                grid[x,y] = new Node(walkable, worldPoint, x, y, movementPenalty);
            }
        }

        BlurPenaltyMap(3);
    }

    void BlurPenaltyMap(int blueSize)
    {
        // Calculate kernel size and extents
        int kernelSize = blueSize * 2 + 1;
        int kernelExtents = (kernelSize + 1) / 2;

        // Initialize arrays for horizontal and vertical passes
        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
        int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

        // Horizontal pass
        for (int y = 0; y < gridSizeY; y++)
        {
            // Accumulate penalties for each position in the row
            for (int x = -kernelExtents; x <= kernelExtents; x++)
            {
                int sampleX = Mathf.Clamp (x, 0, kernelExtents);
                penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;
            }

            // Compute penalties for each position in the row
            for (int x = 1; x < gridSizeX; x++)
            {
                int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
                int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);

                // Update penalty for each position
                penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].movementPenalty + grid [addIndex, y].movementPenalty;
            }
        }

        // Vertical pass
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = -kernelExtents; y <= kernelExtents; y++)
            {
                // Sample the penalties and accumulate
                int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass [x, sampleY];
            }

            // Calculate the blurred penalty for the first row by dividing the accumulated penalties by the square of the kernel size
            int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));

            // Update the movement penalty for the corresponding node in the grid with the calculated blurred penalty value
            grid[x, 0].movementPenalty = blurredPenalty;

            // Compute penalties for each position in the column
            for (int y = 1; y < gridSizeY; y++)
            {
                int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
                int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);

                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass [x, removeIndex] + penaltiesHorizontalPass [x, addIndex];

                // Calculate the blurred penalty for the current position by dividing the accumulated penalties by the square of the kernel size
                blurredPenalty = Mathf.RoundToInt((float) penaltiesVerticalPass[x,y] / (kernelSize * kernelSize));

                // Update the movement penalty for the corresponding node in the grid with the calculated blurred penalty value
                grid[x, y].movementPenalty = blurredPenalty;

                // Track the minimum and maximum penalties for normalization (if needed)
                if (blurredPenalty > penaltyMax)
                {
                    penaltyMax = blurredPenalty;
                }

                if(blurredPenalty < penaltyMin)
                {
                    penaltyMin = blurredPenalty;
                }
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if(x == 0 && y == 0)
                {
                    continue;
                }

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if(checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX,checkY]);
                }
            }
        }

        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt ((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt ((gridSizeY - 1) * percentY);

        return grid[x,y];
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null && displayGridGizmos)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));
                Gizmos.color = (n.walkable) ? Gizmos.color : Color.red;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter));
            }
        }
    }

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }
}
