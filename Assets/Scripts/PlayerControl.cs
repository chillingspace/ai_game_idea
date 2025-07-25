using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed = 5f;          // Units per second
    public float gridSize = 1f;           // Size of each grid cell
    private bool isMoving = false;

    void Update()
    {
        if (!isMoving)
        {
            Vector2 input = Vector2.zero;

            if (Input.GetKeyDown(KeyCode.W)) input = Vector2.up;
            else if (Input.GetKeyDown(KeyCode.S)) input = Vector2.down;
            else if (Input.GetKeyDown(KeyCode.A)) input = Vector2.left;
            else if (Input.GetKeyDown(KeyCode.D)) input = Vector2.right;

            if (input != Vector2.zero)
            {
                Vector2 targetPos = (Vector2)transform.position + input * gridSize;

                // Optional: check collision here before moving
                if (CanMoveTo(targetPos))
                {
                    StartCoroutine(MoveToPosition(targetPos));
                }
            }
        }
    }

    private IEnumerator MoveToPosition(Vector2 target)
    {
        isMoving = true;
        Vector2 start = transform.position;
        float elapsed = 0f;
        float duration = gridSize / moveSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector2.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }

    private bool CanMoveTo(Vector2 targetPos)
    {
        // Optional: Add wall/layer collision check
        RaycastHit2D hit = Physics2D.Raycast(targetPos, Vector2.zero);
        return hit.collider == null; // no wall or blocker at target
    }
}
