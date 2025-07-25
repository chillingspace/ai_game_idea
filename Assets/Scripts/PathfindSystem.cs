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
                if (openSet[i].fCost < current.fCost || 
                    (openSet[i].fCost == current.fCost && openSet[i].hCost < current.hCost))
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
                
                float moveCost = GetMovementCost(current, neighbor);
                float newCost = current.gCost + moveCost;
                
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
        
        // Cardinal directions (always included)
        Vector2Int[] cardinalDirections = { 
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left 
        };
        
        foreach (Vector2Int dir in cardinalDirections)
        {
            Vector2Int pos = node.gridPos + dir;
            if (IsInBounds(pos))
            {
                neighbors.Add(gridManager.grid[pos.x, pos.y]);
            }
        }
        
        // Diagonal directions (only when cutting corners on straight paths)
        Vector2Int[] diagonalDirections = { 
            new Vector2Int(1, 1),   // up-right
            new Vector2Int(-1, 1),  // up-left
            new Vector2Int(1, -1),  // down-right
            new Vector2Int(-1, -1)  // down-left
        };
        
        foreach (Vector2Int dir in diagonalDirections)
        {
            Vector2Int pos = node.gridPos + dir;
            if (IsInBounds(pos) && CanMoveDiagonally(node, dir))
            {
                neighbors.Add(gridManager.grid[pos.x, pos.y]);
            }
        }
        
        return neighbors;
    }
    
    bool CanMoveDiagonally(Node from, Vector2Int direction)
    {
        // Check if both adjacent cardinal cells are walkable (no wall cutting)
        Vector2Int horizontal = new Vector2Int(direction.x, 0);
        Vector2Int vertical = new Vector2Int(0, direction.y);
        
        Vector2Int horizontalPos = from.gridPos + horizontal;
        Vector2Int verticalPos = from.gridPos + vertical;
        
        // Both adjacent cells must be in bounds and walkable
        if (!IsInBounds(horizontalPos) || !IsInBounds(verticalPos))
            return false;
            
        Node horizontalNode = gridManager.grid[horizontalPos.x, horizontalPos.y];
        Node verticalNode = gridManager.grid[verticalPos.x, verticalPos.y];
        
        if (!horizontalNode.walkable || !verticalNode.walkable)
            return false;
        
        // Additional check: only allow diagonal if it's part of a straight path
        // This means the diagonal should lead toward a more direct route
        return IsDiagonalOnStraightPath(from, direction);
    }
    
    bool IsDiagonalOnStraightPath(Node from, Vector2Int direction)
    {
        // Check if this diagonal move is part of a straight line path
        // by looking at the parent's direction and seeing if diagonal continues the trend
        if (from.parent == null) return true; // First move, allow diagonal
        
        Vector2Int parentDirection = from.gridPos - from.parent.gridPos;
        
        // If parent move was cardinal and diagonal continues in same general direction
        if (Mathf.Abs(parentDirection.x) != Mathf.Abs(parentDirection.y))
        {
            // Parent was cardinal, check if diagonal continues in compatible direction
            return (direction.x * parentDirection.x >= 0 && direction.y * parentDirection.y >= 0);
        }
        
        // If parent move was diagonal, only continue with cardinal moves
        return false;
    }
    
    float GetMovementCost(Node from, Node to)
    {
        Vector2Int direction = to.gridPos - from.gridPos;
        
        // Diagonal movement costs more (approximately √2)
        if (Mathf.Abs(direction.x) == 1 && Mathf.Abs(direction.y) == 1)
        {
            return 1.414f; // √2 for diagonal movement
        }
        
        return 1.0f; // Cardinal movement
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