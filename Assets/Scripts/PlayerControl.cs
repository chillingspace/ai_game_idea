using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed = 5f;          // Units per second
    public float gridSize = 1f;           // Size of each grid cell
    public float moveDuration = 0.1f;

    private Rigidbody2D rb2D;
    private Vector2 moveInput;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float inputY = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        moveInput = new Vector2(inputX, inputY).normalized;
    }

    void FixedUpdate()
    {
        rb2D.MovePosition(rb2D.position + moveInput * moveSpeed * Time.fixedDeltaTime);
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
