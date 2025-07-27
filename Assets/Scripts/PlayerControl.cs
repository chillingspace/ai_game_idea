using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed = 5f;          // Units per second
    public float gridSize = 1f;           // Size of each grid cell
    public float moveDuration = 0.1f;

    private Rigidbody2D rb2D;
    private Vector2 moveInput;

    public bool player_hit = false;
    private float hit_timer = 0.0f;
    private const float hit_duration = 0.25f;
    private Color hit_color = Color.pink;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        rb2D.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    // Update is called once per frame
    void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float inputY = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        moveInput = new Vector2(inputX, inputY).normalized;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();

            // If you're testing in the Unity Editor, also stop play mode:
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleEnemyPathDebug();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ToggleEnemySmoothing();
        }

        //Player hit by enemy
        if(player_hit)
        {
            hit_timer = hit_duration;
            player_hit = false;
        }
        else
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();

            if (hit_timer > 0.0f)
            {
                hit_timer -= Time.deltaTime;
                if (sr != null)
                {
                    sr.color = hit_color;
                }
            }
            else
            {
                if (sr != null)
                {
                    sr.color = Color.white;
                }
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 newPosition = rb2D.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb2D.MovePosition(newPosition);

        if (moveInput.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
        }
    }

    private void ToggleEnemyPathDebug()
    {
        var enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.showDebugPath = !enemy.showDebugPath;
            enemy.VisualizePath();
        }
    }

    private void ToggleEnemySmoothing()
    {
        var enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.useSplineSmoothing = !enemy.useSplineSmoothing;
        }
    }

    private IEnumerator Move(Vector2 direction)
    {
        Vector2 start_pos = transform.position;
        Vector2 end_pos = start_pos + (direction * gridSize);

        float elapsed_time = 0;
        while (elapsed_time < moveDuration)
        {
            elapsed_time += Time.deltaTime;
            float percent = elapsed_time / moveDuration;
            transform.position = Vector2.Lerp(start_pos, end_pos, percent);
            yield return null;
        }

        transform.position = end_pos;
    }
}
