using UnityEngine;

public class WallBlocker : MonoBehaviour
{
    private GridManager gridManager;

    public Vector2Int wallSize = Vector2Int.one;

    void Start()
    {
        gridManager = FindAnyObjectByType<GridManager>();
        if (gridManager == null) return;

        Node originNode = gridManager.GetNodeFromWorld(transform.position);
        if (originNode == null) return;

        Vector2Int origin = originNode.gridPos;

        // Shift origin to bottom-left corner if wallSize > 1
        origin -= new Vector2Int(wallSize.x / 2, wallSize.y / 2);

        for (int x = 0; x < wallSize.x; x++)
        {
            for (int y = 0; y < wallSize.y; y++)
            {
                Vector2Int pos = origin + new Vector2Int(x, y);
                gridManager.SetWalkable(pos, false);
            }
        }
    }
}
