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
            enemy.path = enemy.pathfinder.FindPath(enemy.transform.position, enemy.target.position);
            enemy.pathIndex = 0;

            enemy.VisualizePath();
        }

        if (enemy.path == null || enemy.pathIndex >= enemy.path.Count) return;

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
