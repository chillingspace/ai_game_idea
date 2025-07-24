using UnityEngine;

public class ColorChange : MonoBehaviour
{
    public Color newColor = Color.red;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = newColor;
        }
    }
}

