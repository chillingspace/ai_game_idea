using UnityEngine;
using System.Collections;
public class PlayerControl : MonoBehaviour
{
    [SerializeField] private bool is_repeated_move = false;
    [SerializeField] private float move_duration = 0.1f;
    [SerializeField] private float grid_size = 1f;

    private bool is_moving = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (is_moving) return;

        System.Func<KeyCode, bool> inputFunction;

        if (is_repeated_move)
        {
            inputFunction = Input.GetKey;
        }
        else
        {
            inputFunction = Input.GetKeyDown;
        }

        if (inputFunction(KeyCode.W))
        {
            StartCoroutine(Move(Vector2.up));
        }
        else if (inputFunction(KeyCode.S))
        {
            StartCoroutine(Move(Vector2.down));
        }
        else if (inputFunction(KeyCode.A))
        {
            StartCoroutine(Move(Vector2.left));
        }
        else if (inputFunction(KeyCode.D))
        {
            StartCoroutine(Move(Vector2.right));
        }

    }

    private IEnumerator Move(Vector2 direction)
    {
        is_moving = true;

        Vector2 start_pos = transform.position;
        Vector2 end_pos = start_pos + (direction * grid_size);

        float elapsed_time = 0;
        while (elapsed_time < move_duration) 
        {
            elapsed_time += Time.deltaTime;
            float percent = elapsed_time / move_duration;
            transform.position = Vector2.Lerp(start_pos, end_pos, percent); 
            yield return null;
        }

        transform.position = end_pos;

        is_moving = false;
    }
}
