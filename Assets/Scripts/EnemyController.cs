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
    [SerializeField] private GameObject meleeHitboxVisual;

    private BTNode behaviorTree;

    public Transform target;
    public PathfindSystem pathfinder;
    public float moveSpeed;

    // For enemy attacks
    public float detectionRadius = 5f;
    public float meleeRange = 1f;
    public float rangeAttackRange = 3f;

    [HideInInspector] public EnemyShootLogic shootLogic;

    [HideInInspector]
    public List<Node> path = new List<Node>();
    [HideInInspector]
    public int pathIndex;
    [HideInInspector]
    public List<Vector3> smoothedPath = new List<Vector3>();

    [HideInInspector]
    public int smoothedPathIndex = 0;


    [HideInInspector]
    public Vector2Int lastPlayerTile;
    public EnemyState currentState;

    // Patrol points for simple patrol behavior
    [HideInInspector]
    public List<Vector2> patrolPoints;
    [HideInInspector]
    public int patrolIndex = 0;
    public bool showDebugPath= true;

    [Range(0.1f, 1.0f)]
    public float pathLineWidth = 0.1f;

    // Visualize path
    public bool useSplineSmoothing = true;
    public GameObject pathMarkerPrefab; // Assign this prefab in Inspector
    private List<GameObject> spawnedPathMarkers = new List<GameObject>();
    private LineRenderer pathLineRenderer;

    private EnemyStateMachine stateMachine;

    void Start()
    {
        currentState = EnemyState.Idle;

        path = pathfinder.FindPath(transform.position, target.position);
        pathIndex = 0;
        stateMachine = new EnemyStateMachine(this);  // Initialize FSM with reference to this controller
        SetupLineRenderer();

        // Create the behavior tree
        behaviorTree = CreateBehaviorTree();

        shootLogic = GetComponent<EnemyShootLogic>();
        shootLogic.enemy = this;

    }

    void Update()
    {
        // Run the behavior tree to set current goal
        if (currentState == EnemyState.Idle || currentState == EnemyState.Patrol)
            // Only evaluate new goals when idle/patrolling, issue was behavior tree keeps resetting 
            // state 
            behaviorTree.Tick(); 

        // FSM handle execution of goal
        stateMachine.FSMUpdate(); 
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

    public void SetPath(List<Node> newPath)
    {
        path = newPath;
        pathIndex = 0;
        smoothedPathIndex = 0;

        if (useSplineSmoothing && path != null && path.Count >= 2)
        {

            List<Vector3> rawPoints = new List<Vector3>();
            foreach (var node in path)
            {
                Vector2 worldPos = pathfinder.gridManager.GetWorldFromNode(node);
                rawPoints.Add(new Vector3(worldPos.x, worldPos.y, transform.position.z));
            }

            smoothedPath = CatmullRomSpline(rawPoints, 10);
        }
        else
        {
            smoothedPath = null; // no smoothing
        }
    }

    public bool HasLineOfSight(Vector2 agentPos, Vector2 agentForward, Vector2Int targetGridPos)
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
     * CATMUL-ROM SMOOTHING 
     */

    public List<Vector3> CatmullRomSpline(List<Vector3> rawPoints, int samplesPerSegment = 10)
    {
        List<Vector3> smoothPoints = new List<Vector3>();

        if (rawPoints == null || rawPoints.Count < 2)
            return rawPoints;  // Not enough points to smooth

        // Pad points for spline (duplicate first and last points)
        List<Vector3> paddedPoints = new List<Vector3>();
        paddedPoints.Add(rawPoints[0]);  // p0
        paddedPoints.AddRange(rawPoints);
        paddedPoints.Add(rawPoints[rawPoints.Count - 1]);  // pn+1

        // Iterate over each segment
        for (int i = 0; i < paddedPoints.Count - 3; i++)
        {
            Vector3 p0 = paddedPoints[i];
            Vector3 p1 = paddedPoints[i + 1];
            Vector3 p2 = paddedPoints[i + 2];
            Vector3 p3 = paddedPoints[i + 3];

            // Sample points on this segment
            for (int j = 0; j < samplesPerSegment; j++)
            {
                float t = j / (float)samplesPerSegment;
                Vector3 point = CatmullRom(p0, p1, p2, p3, t);
                smoothPoints.Add(point);
            }
        }

        return smoothPoints;
    }

    // Catmull-Rom interpolation between 4 points for parameter t in [0,1]
    public Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        // Catmull-Rom spline formula
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }

    /*
     * DRAWING AND VISLISUALISATION STUFF
     */

    public void VisualizePath()
    {
        ClearPathMarkers();

        if (path == null) return;

        if (showDebugPath && pathMarkerPrefab != null)
        {
            foreach (Node node in path)
            {
                Vector2 worldPos = pathfinder.gridManager.GetWorldFromNode(node);
                GameObject marker = Instantiate(pathMarkerPrefab, worldPos, Quaternion.identity);
                spawnedPathMarkers.Add(marker);
            }
            DrawRawPathLine();
        }


    }

    void DrawRawPathLine()
    {
        if (pathLineRenderer == null || path == null || path.Count < 2)
        {
            if (pathLineRenderer != null) pathLineRenderer.positionCount = 0;
            return;
        }

        List<Vector3> rawPoints = new List<Vector3>();
        for (int i = 0; i < path.Count; i++)
        {
            Vector2 worldPos = pathfinder.gridManager.GetWorldFromNode(path[i]);
            rawPoints.Add(new Vector3(worldPos.x, worldPos.y, -0.1f));
        }

        if (!useSplineSmoothing || rawPoints.Count < 2)
        {
            pathLineRenderer.positionCount = rawPoints.Count;
            pathLineRenderer.SetPositions(rawPoints.ToArray());
            pathLineRenderer.enabled = true;
            return;
        }

        List<Vector3> smoothPoints = CatmullRomSpline(rawPoints, 10);

        pathLineRenderer.positionCount = smoothPoints.Count;
        pathLineRenderer.SetPositions(smoothPoints.ToArray());
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
            new ConditionNode(() => HasLineOfSight(transform.position, (target.position - transform.position).normalized, pathfinder.gridManager.GetNodeFromWorld(target.position).gridPos)),
            new ActionNode(() => {
                //Debug.Log("BT: Switching to RangeAttack");
                currentState = EnemyState.RangeAttack;
                return BTResult.Success;
            })
        }),
        new Sequence(new List<BTNode>
        {
            new ConditionNode(() => Vector2.Distance(transform.position, target.position) <= meleeRange),
            new ActionNode(() => {
                //Debug.Log("BT: Switching to MeleeAttack");
                currentState = EnemyState.MeleeAttack;
                return BTResult.Success;
            })

        }),
        new Sequence(new List<BTNode>
        {
            new ConditionNode(() => Vector2.Distance(transform.position, target.position) <= detectionRadius),
            new ActionNode(() => {
                //Debug.Log("BT: Switching to Chase");
                currentState = EnemyState.Chase;
                return BTResult.Success;
            })
        }),
        new ActionNode(() => {
            //Debug.Log("BT: Switching to Patrol");
            currentState = EnemyState.Patrol;
            return BTResult.Success;
        })
    });
    }
    public void PerformMeleeAttack()
    {
        //if (meleeHitboxVisual != null)
        //{
        //    meleeHitboxVisual.SetActive(true);
        //    // Optionally: Deal damage to player here
        //    Invoke(nameof(HideMeleeVisual), 0.3f);  // Show effect briefly
        //}

        Vector2 pos = transform.position;
        Debug.DrawLine(pos, target.position, Color.yellow, 0.3f);
    }

    private void HideMeleeVisual()
    {
        meleeHitboxVisual.SetActive(false);
    }

}

