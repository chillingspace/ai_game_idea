using UnityEngine;
using System.Collections.Generic;

public enum EnemyState
{
    Patrol,
    Chase,
    RangeAttack,
    MeleeAttack,
    Idle
}

public class EnemyController : MonoBehaviour
{
    public Transform target;
    public PathfindSystem pathfinder;
    public float moveSpeed;

    // For enemy attacks
    public float detectionRadius = 5f;
    public float meleeRange = 1f;
    public float rangeAttackRange = 3f;

    [HideInInspector]
    public List<Node> path;
    [HideInInspector]
    public int pathIndex;
    [HideInInspector]
    public Vector2Int lastPlayerTile;
    public EnemyState currentState;

    // Patrol points for simple patrol behavior
    [HideInInspector]
    public List<Vector2> patrolPoints;
    [HideInInspector]
    public int patrolIndex = 0;

    // Visualize path
    public GameObject pathMarkerPrefab; // Assign this prefab in Inspector
    private List<GameObject> spawnedPathMarkers = new List<GameObject>();


    private EnemyStateMachine stateMachine;

    void Start()
    {
        currentState = EnemyState.Idle;

        lastPlayerTile = pathfinder.gridManager.GetNodeFromWorld(target.position).gridPos;
        path = pathfinder.FindPath(transform.position, target.position);
        pathIndex = 0;
        stateMachine = new EnemyStateMachine(this);  // Initialize FSM with reference to this controller
    }

    void Update()
    {
        stateMachine.FSMUpdate(currentState); // Let FSM handle all updates per frame
    }

    public void VisualizePath()
    {
        ClearPathMarkers();

        if (path == null) return;

        foreach (Node node in path)
        {
            Vector2 worldPos = pathfinder.gridManager.GetWorldFromNode(node);
            GameObject marker = Instantiate(pathMarkerPrefab, worldPos, Quaternion.identity);
            spawnedPathMarkers.Add(marker);
        }
    }

    public void ClearPathMarkers()
    {
        foreach (var marker in spawnedPathMarkers)
        {
            Destroy(marker);
        }
        spawnedPathMarkers.Clear();
    }

}

