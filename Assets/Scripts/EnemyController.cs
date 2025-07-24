using UnityEngine;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    public Transform target;
    public PathfindSystem pathfinder;
    public float moveSpeed;

    private List<Node> path;
    private int pathIndex;

    private Vector2Int lastPlayerTile;

    void Start()
    {
        lastPlayerTile = pathfinder.gridManager.GetNodeFromWorld(target.position).gridPos;
        path = pathfinder.FindPath(transform.position, target.position);
        pathIndex = 0;
    }

    void Update()
    {
        Node playerNode = pathfinder.gridManager.GetNodeFromWorld(target.position);
        if (playerNode == null)
        {
            Debug.LogWarning("Player is out of bounds or grid not ready!");
            return;
        }

        Vector2Int currentPlayerTile = playerNode.gridPos;

        // If the player moved to a new tile, recalculate path
        if (currentPlayerTile != lastPlayerTile)
        {
            lastPlayerTile = currentPlayerTile;
            path = pathfinder.FindPath(transform.position, target.position);
            pathIndex = 0;
        }

        // Movement logic
        if (path == null || pathIndex >= path.Count) return;

        Vector2 targetPos = pathfinder.gridManager.GetWorldFromNode(path[pathIndex]);
        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;

        transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPos) < 0.1f)
        {
            pathIndex++;
        }
    }


}
