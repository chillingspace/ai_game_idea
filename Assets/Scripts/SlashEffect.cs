using UnityEngine;
using System.Collections;

public class SlashEffect : MonoBehaviour
{
    public float lifetime = 2f;      // Time before fading starts
    public float fadeTime = 2f;      // Duration of fade-out

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        Color c = sr.color;
        c.a = 1f;
        sr.color = c;
        Debug.Log($"[Awake] Initial alpha: {sr.color.a}");
    }



    public void BeginFadeAndDestroy()
    {
        
        StartCoroutine(FadeAndDestroy());
    }

    private IEnumerator FadeAndDestroy()
    {        
        yield return new WaitForSeconds(lifetime);
        
        float elapsed = 0f;
        Color originalColor = sr.color;       

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeTime);
            float alpha = Mathf.SmoothStep(1f, 0f, t);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}
