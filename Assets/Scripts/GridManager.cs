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

        return new Vector2(worldX, worldY);
    }

    public Node GetNodeFromGridPos(Vector2Int gridPos)
    {
        if (IsValidGridPos(gridPos))
            return grid[gridPos.x, gridPos.y];
        return null;

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

    public bool IsWalkable(Vector2Int gridPos)
    {
        if (IsValidGridPos(gridPos))
        {
            return grid[gridPos.x, gridPos.y].walkable;
        }
        return false; // Or true, depending on what you want for out-of-bounds
    }

    public bool IsValidGridPos(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public bool IsClearPath(Vector2Int from, Vector2Int to)
    {
        float puff = 0.003f;

        Vector2 p1 = new Vector2(from.x + 0.5f, from.y + 0.5f);
        Vector2 p2 = new Vector2(to.x + 0.5f, to.y + 0.5f);

        for (int row = 0; row < height; ++row)
        {
            for (int col = 0; col < width; ++col)
            {
                Vector2Int gridCords = new Vector2Int(col, row);
                if (IsWalkable(gridCords))
                    continue;

                float left = col - puff;
                float right = col + 1f + puff;
                float top = row - puff;
                float bottom = row + 1f + puff;

                Vector2 topLeft = new Vector2(left, top);
                Vector2 topRight = new Vector2(right, top);
                Vector2 botRight = new Vector2(right, bottom);
                Vector2 botLeft = new Vector2(left, bottom);

                if (
                    LineIntersect(p1, p2, topLeft, topRight) ||
                    LineIntersect(p1, p2, topRight, botRight) ||
                    LineIntersect(p1, p2, botRight, botLeft) ||
                    LineIntersect(p1, p2, botLeft, topLeft))
                {
                    return false;
                }
            }
        }

        return true;
    }


    bool LineIntersect(Vector2 p0, Vector2 p1, Vector2 q0, Vector2 q1)
    {
        float y4y3 = q1.y - q0.y;
        float y1y3 = p0.y - q0.y;
        float y2y1 = p1.y - p0.y;
        float x4x3 = q1.x - q0.x;
        float x2x1 = p1.x - p0.x;
        float x1x3 = p0.x - q0.x;

        float divisor = y4y3 * x2x1 - x4x3 * y2y1;
        float dividend0 = x4x3 * y1y3 - y4y3 * x1x3;
        float dividend1 = x2x1 * y1y3 - y2y1 * x1x3;

        const float eps = 0.0001f;
        if (Mathf.Abs(dividend0) < eps && Mathf.Abs(dividend1) < eps && Mathf.Abs(divisor) < eps)
        {
            // Coincident lines
            return true;
        }

        if (Mathf.Abs(divisor) < eps)
        {
            // Parallel lines
            return false;
        }

        float t0 = dividend0 / divisor;
        float t1 = dividend1 / divisor;

        return t0 >= 0f && t0 <= 1f && t1 >= 0f && t1 <= 1f;
    }

    /*
     * On Draw Gizmo functions
     */ 
    //void OnDrawGizmos()
    //{
    //    if (grid == null) return;

    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < height; y++)
    //        {
    //            Node node = grid[x, y];
    //            Gizmos.color = node.walkable ? Color.green : Color.red;
    //            Vector2 worldPos = GetWorldFromNode(node);
    //            Gizmos.DrawWireCube(worldPos, Vector3.one * cellSize * 0.9f);
    //        }
    //    }
    //}

}

