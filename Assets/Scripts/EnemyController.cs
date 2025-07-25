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
    public List<Node> path = new List<Node>();
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
    public bool showRawPath = true;
    public bool showPathLine = true;
    [Range(0.1f, 1.0f)]
    public float pathLineWidth = 0.1f;

    // Visualize path
    public GameObject pathMarkerPrefab; // Assign this prefab in Inspector
    private List<GameObject> spawnedPathMarkers = new List<GameObject>();
    private LineRenderer pathLineRenderer;

    private EnemyStateMachine stateMachine;

    void Start()
    {
        currentState = EnemyState.Idle;

        lastPlayerTile = pathfinder.gridManager.GetNodeFromWorld(target.position).gridPos;
        path = pathfinder.FindPath(transform.position, target.position);
        pathIndex = 0;
        stateMachine = new EnemyStateMachine(this);  // Initialize FSM with reference to this controller
        SetupLineRenderer();
    }

    void Update()
    {
        stateMachine.FSMUpdate(currentState); // Let FSM handle all updates per frame
    }

    void SetupLineRenderer()
    {
        pathLineRenderer = gameObject.GetComponent<LineRenderer>();
        if (pathLineRenderer == null)
        {
            pathLineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineMaterial.color = Color.cyan;
        pathLineRenderer.material = lineMaterial;
        pathLineRenderer.startWidth = pathLineWidth;
        pathLineRenderer.endWidth = pathLineWidth;
        pathLineRenderer.positionCount = 0;
        pathLineRenderer.useWorldSpace = true;
        pathLineRenderer.sortingOrder = 1;
    }

    public void VisualizePath()
    {
        ClearPathMarkers();

        if (path == null) return;

        if (showRawPath && pathMarkerPrefab != null)
        {
            foreach (Node node in path)
            {
                Vector2 worldPos = pathfinder.gridManager.GetWorldFromNode(node);
                GameObject marker = Instantiate(pathMarkerPrefab, worldPos, Quaternion.identity);
                spawnedPathMarkers.Add(marker);
            }
        }

        DrawRawPathLine();
    }

    void DrawRawPathLine()
    {
        if (pathLineRenderer == null || path == null || path.Count < 2)
        {
            if (pathLineRenderer != null) pathLineRenderer.positionCount = 0;
            return;
        }

        Vector3[] positions = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++)
        {
            Vector2 worldPos = pathfinder.gridManager.GetWorldFromNode(path[i]);
            positions[i] = new Vector3(worldPos.x, worldPos.y, -0.1f);
        }

        pathLineRenderer.positionCount = positions.Length;
        pathLineRenderer.SetPositions(positions);
        pathLineRenderer.enabled = true;
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

