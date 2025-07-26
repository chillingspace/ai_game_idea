using UnityEngine;
using System.Collections.Generic;

public class EnemyStateMachine
{
    private EnemyController enemy;

    public EnemyStateMachine(EnemyController enemyController)
    {
        enemy = enemyController;
    }

   public void FSMUpdate()
    {
        switch (enemy.currentState)
        {
            case EnemyState.Patrol:
                PatrolUpdate();
                CheckForPlayer();
                break;

            case EnemyState.Chase:
                ChaseUpdate();
                CheckAttackRange();
                break;

            case EnemyState.RangeAttack:
                RangeAttackUpdate();
                break;

            case EnemyState.MeleeAttack:
                MeleeAttackUpdate();
                break;

            case EnemyState.Idle:
                IdleUpdate();
                CheckForPlayer();
                break;
        }
    }


    // --- FSM State Updates ---

    public void PatrolUpdate()
    {
        if (enemy.patrolPoints == null || enemy.patrolPoints.Count == 0)
        {
            Debug.LogWarning("No patrol points assigned to enemy!");
            return;
        }

        if (enemy.path == null || enemy.pathIndex >= enemy.path.Count)
        {
            enemy.patrolIndex = (enemy.patrolIndex + 1) % enemy.patrolPoints.Count;
            SetPathToPatrolPoint();
        }
        else
        {
            MoveAlongPath();
        }
    }

    public void ChaseUpdate()
    {
        Node playerNode = enemy.pathfinder.gridManager.GetNodeFromWorld(enemy.target.position);
        if (playerNode == null) return;

        Vector2Int currentPlayerTile = playerNode.gridPos;

        if (currentPlayerTile != enemy.lastPlayerTile)
        {
            enemy.lastPlayerTile = currentPlayerTile;
            var newPath = enemy.pathfinder.FindPath(enemy.transform.position, enemy.target.position);
            enemy.SetPath(newPath);

            enemy.VisualizePath();
        }

        MoveAlongPath();
    }

    public void RangeAttackUpdate()
    {
        float distanceToPlayer = Vector2.Distance(enemy.transform.position, enemy.target.position);

        // Attempt melee if player is close enough
        if (distanceToPlayer <= enemy.meleeRange + 2f)
        {
            Vector2Int favorableTile = FindFavorableMeleeTile();
            if (favorableTile != new Vector2Int(-1, -1))
            {
                Node favorableNode = enemy.pathfinder.gridManager.GetNodeFromGridPos(favorableTile);
                Vector2 targetWorldPos = enemy.pathfinder.gridManager.GetWorldFromNode(favorableNode);

                // Set path to melee position
                enemy.path = enemy.pathfinder.FindPath(enemy.transform.position, targetWorldPos);
                enemy.pathIndex = 0;

                Debug.DrawLine(enemy.transform.position, targetWorldPos, Color.magenta, 0.5f);

                enemy.currentState = EnemyState.MeleeAttack;
                return;
            }
        }

        // Face player
        Vector2 dir = (enemy.target.position - enemy.transform.position).normalized;
        enemy.transform.up = dir;

        // LOS check
        if (!enemy.HasLineOfSight(enemy.transform.position, dir,
            enemy.pathfinder.gridManager.GetNodeFromWorld(enemy.target.position).gridPos))
        {
            enemy.currentState = EnemyState.Chase;
            return;
        }

        // Distance check
        if (distanceToPlayer > enemy.rangeAttackRange)
        {
            enemy.currentState = EnemyState.Chase;
            return;
        }

        // Only here is it safe to shoot
        enemy.shootLogic.TryShoot();
    }

    public void MeleeAttackUpdate()
    {
        if (enemy.path != null && enemy.pathIndex < enemy.path.Count)
        {
            Node targetNode = enemy.path[enemy.pathIndex];
            Vector2 target = enemy.pathfinder.gridManager.GetWorldFromNode(targetNode);

            // Move toward target
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, target, enemy.moveSpeed * Time.deltaTime);

            if (Vector2.Distance(enemy.transform.position, target) < 0.1f)
            {
                enemy.pathIndex++;
            }
        }
        else
        {
            // Arrived at melee position, attack if in range
            if (Vector2.Distance(enemy.transform.position, enemy.target.position) <= enemy.meleeRange)
            {
                Debug.Log("Perform Melee Attack!");
                // Attack logic
                Vector2 pos = transform.position;
                Debug.DrawLine(pos, target.position, Color.yellow, 0.3f);
            }
            else
            {
                // Player moved away � re-evaluate or fallback
                enemy.currentState = EnemyState.Chase;
            }
        }
    }


    public void IdleUpdate()
    {
        // Idle logic if needed
    }

    // --- Helper Methods ---

    private Vector3 GetCurrentTargetPosition()
    {
        if (enemy.useSplineSmoothing && enemy.smoothedPath != null && enemy.smoothedPathIndex < enemy.smoothedPath.Count)
        {
            return enemy.smoothedPath[enemy.smoothedPathIndex];
        }
        else if (enemy.path != null && enemy.pathIndex < enemy.path.Count)
        {
            Vector2 pos2D = enemy.pathfinder.gridManager.GetWorldFromNode(enemy.path[enemy.pathIndex]);
            return new Vector3(pos2D.x, pos2D.y, enemy.transform.position.z);
        }
        return enemy.transform.position; // fallback
    }


    private void SetPathToPatrolPoint()
    {
        if (enemy.patrolPoints == null || enemy.patrolPoints.Count == 0) return;

        var newPath = enemy.pathfinder.FindPath(enemy.transform.position, enemy.patrolPoints[enemy.patrolIndex]);
        enemy.SetPath(newPath);

        enemy.VisualizePath();
    }

    private Vector2Int FindFavorableMeleeTile()
    {
        Vector2Int playerGridPos = enemy.pathfinder.gridManager.GetNodeFromWorld(enemy.target.position).gridPos;

        Vector2Int[] directions = {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

        Vector2Int bestTile = new Vector2Int(-1, -1);
        int shortestPathLength = int.MaxValue;

        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = playerGridPos + dir;

            if (!enemy.pathfinder.gridManager.IsValidGridPos(checkPos)) continue;
            if (!enemy.pathfinder.gridManager.IsWalkable(checkPos)) continue;

            Node checkNode = enemy.pathfinder.gridManager.GetNodeFromGridPos(checkPos);
            Vector2 worldPos = enemy.pathfinder.gridManager.GetWorldFromNode(checkNode);
            var path = enemy.pathfinder.FindPath(enemy.transform.position, worldPos);

            if (path != null && path.Count > 0 && path.Count < shortestPathLength)
            {
                shortestPathLength = path.Count;
                bestTile = checkPos;
            }
        }

        if (bestTile != new Vector2Int(-1, -1))
        {
            Vector2 debugPos = enemy.pathfinder.gridManager.GetWorldFromNode(
                enemy.pathfinder.gridManager.GetNodeFromGridPos(bestTile));
            Debug.DrawLine(debugPos, debugPos + Vector2.up * 0.5f, Color.magenta, 0.5f);
        }

        return bestTile;
    }


    private void MoveAlongPath()
    {
        // Choose appropriate path based on smoothing toggle
        List<Vector3> currentPath = enemy.useSplineSmoothing ? enemy.smoothedPath : 
            new List<Vector3> { enemy.pathfinder.gridManager.GetWorldFromNode(enemy.path[enemy.pathIndex]) };
        int index = enemy.useSplineSmoothing ? enemy.smoothedPathIndex : enemy.pathIndex;

        // Early exit if no valid path
        if (currentPath == null || index >= currentPath.Count)
            return;

        Vector3 targetPos = currentPath[index];
        Vector3 currentPos = enemy.transform.position;
        Vector2 moveDir = (targetPos - currentPos).normalized;

        // Face movement direction
        if (moveDir.sqrMagnitude > 0.01f)
            enemy.transform.up = moveDir;

        // Move toward target
        enemy.transform.position = Vector3.MoveTowards(currentPos, targetPos, enemy.moveSpeed * Time.deltaTime);

        // Advance to next point if close enough
        float tolerance = enemy.useSplineSmoothing ? 0.05f : 0.1f;
        if (Vector3.Distance(currentPos, targetPos) < tolerance)
        {
            if (enemy.useSplineSmoothing)
                enemy.smoothedPathIndex++;
            else
                enemy.pathIndex++;
        }
    }



    public void CheckForPlayer()
    {
        float distanceToPlayer = Vector2.Distance(enemy.transform.position, enemy.target.position);
        if (distanceToPlayer <= enemy.detectionRadius)
        {
            enemy.currentState = EnemyState.Chase;
            enemy.lastPlayerTile = enemy.pathfinder.gridManager.GetNodeFromWorld(enemy.target.position).gridPos;
            var newPath = enemy.pathfinder.FindPath(enemy.transform.position, enemy.target.position);
            enemy.SetPath(newPath);
        }
    }

    public void CheckAttackRange()
    {
        float distanceToPlayer = Vector2.Distance(enemy.transform.position, enemy.target.position);
        Vector2 dir = (enemy.target.position - enemy.transform.position).normalized;

        bool hasLOS = enemy.HasLineOfSight(enemy.transform.position, dir,
            enemy.pathfinder.gridManager.GetNodeFromWorld(enemy.target.position).gridPos);

        if (distanceToPlayer <= enemy.meleeRange)
        {
            enemy.currentState = EnemyState.MeleeAttack;
        }
        else if (distanceToPlayer <= enemy.rangeAttackRange && hasLOS)
        {
            enemy.currentState = EnemyState.RangeAttack;
        }
    }
}
