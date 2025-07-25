using UnityEngine;

public class Node
{
    public Vector2Int gridPos;
    public bool walkable;

    public float gCost, hCost;
    public Node parent;

    public float fCost => gCost + hCost;

    public Node(Vector2Int pos, bool walkable)
    {
        this.gridPos = pos;
        this.walkable = walkable;
    }
}

public class GridManager : MonoBehaviour
{
    public int width, height;
    public float cellSize;
    public Node[,] grid;

    public bool IsReady()
    {
        return grid != null;
    }

    void Awake()
    {
        grid = new Node[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Set to true for walkable
                grid[x, y] = new Node(new Vector2Int(x, y), true); 
            }
        }
    }

    public Vector2 GetWorldFromNode(Node node)
    {
        float gridWorldWidth = width * cellSize;
        float gridWorldHeight = height * cellSize;

        float worldX = node.gridPos.x * cellSize - gridWorldWidth / 2f + cellSize / 2f;
        float worldY = node.gridPos.y * cellSize - gridWorldHeight / 2f + cellSize / 2f;

        Debug.Log($"Node GridPos: {node.gridPos}, WorldPos: ({worldX}, {worldY})");

        return new Vector2(worldX, worldY);
    }


    public Node GetNodeFromWorld(Vector2 worldPos)
    {
        if (grid == null) return null;

        Vector2 gridOrigin = new Vector2(-width * cellSize / 2f, -height * cellSize / 2f);

        // Offset the world position relative to bottom-left corner of grid
        float offsetX = worldPos.x - gridOrigin.x;  // worldPos.x - (-halfWidth) = worldPos.x + halfWidth
        float offsetY = worldPos.y - gridOrigin.y;

        int x = Mathf.FloorToInt(offsetX / cellSize);
        int y = Mathf.FloorToInt(offsetY / cellSize);

        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            Debug.LogWarning($"Position {worldPos} out of grid bounds (calculated index {x},{y})");
            return null;
        }

        return grid[x, y];
    }

    public void SetWalkable(Vector2Int gridPos, bool isWalkable)
    {
        if (IsValidGridPos(gridPos))
        {
            grid[gridPos.x, gridPos.y].walkable = isWalkable;
        }
    }

    public void SetWalkableFromWorld(Vector2 worldPos, bool isWalkable)
    {
        Node node = GetNodeFromWorld(worldPos);
        if (node != null)
        {
            node.walkable = isWalkable;
        }
    }

    public bool IsValidGridPos(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    void OnDrawGizmos()
    {
        if (grid == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Node node = grid[x, y];
                Gizmos.color = node.walkable ? Color.green : Color.red;
                Vector2 worldPos = GetWorldFromNode(node);
                Gizmos.DrawWireCube(worldPos, Vector3.one * cellSize * 0.9f);
            }
        }
    }

}

