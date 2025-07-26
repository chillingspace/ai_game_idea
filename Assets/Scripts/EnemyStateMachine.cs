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
            // Switch to melee attack...
            return;
        }

        // Face player
        Vector2 dir = (enemy.target.position - enemy.transform.position).normalized;
        enemy.transform.up = dir;

        // LOS check
        if (!enemy.HasLineOfSight(enemy.transform.position, dir,
            enemy.pathfinder.gridManager.GetNodeFromWorld(enemy.target.position).gridPos))
        {
            Debug.Log("RangeAttack: No line of sight, switching to Chase");
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
        enemy.shootLogic?.TryShoot();
        Debug.Log("Ranged attack!");
    }




    public void MeleeAttackUpdate()
    {
        Debug.Log("Melee Attack!");
        enemy.currentState = EnemyState.Chase;
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

    private void MoveAlongPath()
    {
        if (enemy.path == null || enemy.pathIndex >= enemy.path.Count)
            return;

              Vector3 targetPos = GetCurrentTargetPosition();
        Vector2 moveDir = (targetPos - (Vector2)enemy.transform.position).normalized;

        // Face movement direction
        if (moveDir.sqrMagnitude > 0.01f)
            enemy.transform.up = moveDir;

        // Move toward target
        enemy.transform.position = Vector2.MoveTowards(enemy.transform.position, targetPos, enemy.moveSpeed * Time.deltaTime);

        enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, targetPos, enemy.moveSpeed * Time.deltaTime);

        float tolerance = enemy.useSplineSmoothing ? 0.05f : 0.1f;

        if (Vector3.Distance(enemy.transform.position, targetPos) < tolerance)
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
