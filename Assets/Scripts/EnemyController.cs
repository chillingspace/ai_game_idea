using UnityEngine;
using System.Collections.Generic;
using BehaviorTree;

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
    private BTNode behaviorTree;

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

        // Create the behavior tree
        behaviorTree = CreateBehaviorTree();
    }

    void Update()
    {
        // Run the behavior tree to set current goal
        behaviorTree?.Tick();

        // FSM handle execution of goal
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

    bool HasLineOfSight(Vector2 agentPos, Vector2 agentForward, Vector2Int targetGridPos)
    {
        Vector2 forward = agentForward.normalized;

        Node targetNode = pathfinder.gridManager.GetNodeFromGridPos(targetGridPos);

        // Get target node world position as Vector2
        Vector2 targetWorld = pathfinder.gridManager.GetWorldFromNode(targetNode);

        Vector2 toTarget = targetWorld - agentPos;

        if (toTarget.magnitude == 0f)
        {
            // Draw a green dot if agent is on the target cell
            Debug.DrawLine(agentPos, agentPos + forward * 0.1f, Color.green, 0.1f);
            return true;
        }

        toTarget.Normalize();

        float fovDeg = 190.5f;
        float cosThreshold = Mathf.Cos(fovDeg * 0.5f * Mathf.Deg2Rad);

        float dot = Vector2.Dot(forward, toTarget);
        if (dot < cosThreshold)
        {
            // Draw red line indicating target outside FOV
            Debug.DrawLine(agentPos, targetWorld, Color.red, 0.1f);
            return false;
        }

        Vector2Int agentGrid = pathfinder.gridManager.GetNodeFromWorld(agentPos).gridPos;

        bool clearPath = pathfinder.gridManager.IsClearPath(agentGrid, targetGridPos);

        // Draw green line if clear LOS, else red
        Color lineColor = clearPath ? Color.green : Color.red;
        Debug.DrawLine(agentPos, targetWorld, lineColor, 0.1f);

        return clearPath;
    }



    /*
     * DRAWING AND VISLISUALISATION STUFF
     */

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

    // Create the behavior tree structure
    private BTNode CreateBehaviorTree()
    {
        return new Selector(new List<BTNode>
    {
        new Sequence(new List<BTNode>
        {
            new ConditionNode(() => Vector2.Distance(transform.position, target.position) <= rangeAttackRange),
            new ConditionNode(() => HasLineOfSight(transform.position, transform.right, pathfinder.gridManager.GetNodeFromWorld(target.position).gridPos)),
            new ActionNode(() => {
                Debug.Log("BT: Switching to RangeAttack");
                currentState = EnemyState.RangeAttack;
                return BTResult.Success;
            })
        }),
        new Sequence(new List<BTNode>
        {
            new ConditionNode(() => Vector2.Distance(transform.position, target.position) <= meleeRange),
            new ActionNode(() => {
                Debug.Log("BT: Switching to MeleeAttack");
                currentState = EnemyState.MeleeAttack;
                return BTResult.Success;
            })
        }),
        new Sequence(new List<BTNode>
        {
            new ConditionNode(() => Vector2.Distance(transform.position, target.position) <= detectionRadius),
            new ActionNode(() => {
                Debug.Log("BT: Switching to Chase");
                currentState = EnemyState.Chase;
                return BTResult.Success;
            })
        }),
        new ActionNode(() => {
            Debug.Log("BT: Switching to Patrol");
            currentState = EnemyState.Patrol;
            return BTResult.Success;
        })
    });
    }

}

