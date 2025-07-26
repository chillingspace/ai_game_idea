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
        Debug.Log("Range Attack!");
        enemy.currentState = EnemyState.Chase;
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
        Vector3 targetPos = GetCurrentTargetPosition();
        if (targetPos == enemy.transform.position) return;

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
        if (distanceToPlayer <= enemy.meleeRange)
        {
            enemy.currentState = EnemyState.MeleeAttack;
        }
        else if (distanceToPlayer <= enemy.rangeAttackRange)
        {
            enemy.currentState = EnemyState.RangeAttack;
        }
    }
}
