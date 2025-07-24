using UnityEngine;
using System.Collections.Generic;

public class PathfindSystem : MonoBehaviour
{
    public GridManager gridManager;

    public List<Node> FindPath(Vector2 startWorld, Vector2 endWorld)
    {
        Node start = gridManager.GetNodeFromWorld(startWorld);
        Node end = gridManager.GetNodeFromWorld(endWorld);

        List<Node> openSet = new List<Node> { start };
        HashSet<Node> closedSet = new HashSet<Node>();

        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < current.fCost || (openSet[i].fCost == current.fCost && openSet[i].hCost < current.hCost))
                {
                    current = openSet[i];
                }
            }

            openSet.Remove(current);
            closedSet.Add(current);

            if (current == end) return RetracePath(start, end);

            foreach (Node neighbor in GetNeighbors(current))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor)) continue;

                float newCost = current.gCost + Vector2Int.Distance(current.gridPos, neighbor.gridPos);
                if (newCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCost;
                    neighbor.hCost = Vector2Int.Distance(neighbor.gridPos, end.gridPos);
                    neighbor.parent = current;

                    if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                }
            }
        }

        return null;
    }

    List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int pos = node.gridPos + dir;
            if (IsInBounds(pos)) neighbors.Add(gridManager.grid[pos.x, pos.y]);
        }

        return neighbors;
    }

    bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridManager.width && pos.y >= 0 && pos.y < gridManager.height;
    }

    List<Node> RetracePath(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node current = end;

        while (current != start)
        {
            path.Add(current);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }
}

