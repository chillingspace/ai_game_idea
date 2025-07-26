using UnityEngine;
using System.Collections.Generic;

public class EnemyStateMachine
{
    private EnemyController enemy;

    public EnemyStateMachine(EnemyController enemyController)
    {
        enemy = enemyController;
    }

   public void FSMUpdate(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Patrol:
                PatrolUpdate();
                CheckForPlayer();
                CheckAttackRange();
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
            enemy.path = enemy.pathfinder.FindPath(enemy.transform.position, enemy.target.position);
            enemy.pathIndex = 0;

            enemy.VisualizePath();
        }

        if (enemy.path == null || enemy.pathIndex >= enemy.path.Count) return;

        MoveAlongPath();
    }

    public void RangeAttackUpdate()
    {
        float distanceToPlayer = Vector2.Distance(enemy.transform.position, enemy.target.position);

        // Try melee only if player is somewhat close
        if (distanceToPlayer <= enemy.meleeRange + 2f)
        {
            Vector2Int favorableTile = FindFavorableMeleeTile();
            if (favorableTile != new Vector2Int(-1, -1))
            {
                Node favorableTileNode = enemy.pathfinder.gridManager.GetNodeFromGridPos(favorableTile);

                // Transition to melee and set path
                Vector2 targetWorld = enemy.pathfinder.gridManager.GetWorldFromNode(favorableTileNode);
                enemy.path = enemy.pathfinder.FindPath(enemy.transform.position, targetWorld);
                enemy.pathIndex = 0;

                Debug.DrawLine(enemy.transform.position, targetWorld, Color.magenta, 0.2f);

                enemy.currentState = EnemyState.MeleeAttack;
                return;
            }
        }

        // Otherwise continue ranged attack logic (or return to chase if out of range)
        if (distanceToPlayer > enemy.rangeAttackRange)
        {
            enemy.currentState = EnemyState.Chase;
        }
        else
        {
            // Perform ranged attack
            Debug.Log("Ranged attack!");
            // TODO: Launch projectile, etc.
        }
    }


    public void MeleeAttackUpdate()
    {
        Debug.Log("Melee Attack!");
    }

    public void IdleUpdate()
    {
        // Idle logic if needed
    }

    // --- Helper Methods ---

    private void SetPathToPatrolPoint()
    {
        if (enemy.patrolPoints == null || enemy.patrolPoints.Count == 0) return;

        enemy.path = enemy.pathfinder.FindPath(enemy.transform.position, enemy.patrolPoints[enemy.patrolIndex]);
        enemy.pathIndex = 0;

        enemy.VisualizePath();
    }

    private void MoveAlongPath()
    {
        Vector2 targetPos = enemy.pathfinder.gridManager.GetWorldFromNode(enemy.path[enemy.pathIndex]);
        enemy.transform.position = Vector2.MoveTowards(enemy.transform.position, targetPos, enemy.moveSpeed * Time.deltaTime);

        if (Vector2.Distance(enemy.transform.position, targetPos) < 0.1f)
        {
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
            enemy.path = enemy.pathfinder.FindPath(enemy.transform.position, enemy.target.position);
            enemy.pathIndex = 0;
        }
    }

    private Vector2Int FindFavorableMeleeTile()
    {
        Vector2Int playerGridPos = enemy.pathfinder.gridManager.GetNodeFromWorld(enemy.target.position).gridPos;

        // Directions around the player: up, down, left, right
        Vector2Int[] directions = {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = playerGridPos + dir;

            Node checkPosNode = enemy.pathfinder.gridManager.GetNodeFromGridPos(checkPos);

            if (!enemy.pathfinder.gridManager.IsValidGridPos(checkPos)) continue;
            if (!enemy.pathfinder.gridManager.IsWalkable(checkPos)) continue;

            var path = enemy.pathfinder.FindPath(enemy.transform.position,
                enemy.pathfinder.gridManager.GetWorldFromNode(checkPosNode));

            if (path != null && path.Count > 0)
            {
                // Visualize it
                Vector2 world = enemy.pathfinder.gridManager.GetWorldFromNode(checkPosNode);
                Debug.DrawLine(world, world + Vector2.up * 0.5f, Color.magenta, 0.5f);

                return checkPos;
            }
        }

        // No valid adjacent tile found
        return new Vector2Int(-1, -1);
    }



    public void CheckAttackRange()
    {
        float distanceToPlayer = Vector2.Distance(enemy.transform.position, enemy.target.position);

        if (distanceToPlayer <= enemy.meleeRange)
        {
            enemy.currentState = EnemyState.MeleeAttack;
        }
        else if (distanceToPlayer <= enemy.rangeAttackRange)
        {
            enemy.currentState = EnemyState.RangeAttack;
        }
        else
        {
            enemy.currentState = EnemyState.Chase;
        }
    }


}
