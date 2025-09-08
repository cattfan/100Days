// ItemPickupVisuals.cs: Xử lý tất cả hiệu ứng visual
using System.Collections;
using UnityEngine;

public class ItemPickupVisuals : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void StartDelayEffect(float delayTime)
    {
        if (delayTime > 0)
        {
            StartCoroutine(PickupDelayEffect(delayTime));
        }
    }

    public void ShowReadyEffect()
    {
        StartCoroutine(ReadyToPickupEffect());
    }

    public void StopAllEffects()
    {
        StopAllCoroutines();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private IEnumerator PickupDelayEffect(float delayTime)
    {
        if (spriteRenderer == null) yield break;

        float elapsed = 0f;

        while (elapsed < delayTime)
        {
            // Hiệu ứng nhấp nháy
            float alpha = 0.3f + 0.7f * (0.5f + 0.5f * Mathf.Sin(elapsed * 8f));
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = originalColor;
    }

    private IEnumerator ReadyToPickupEffect()
    {
        if (spriteRenderer == null) yield break;

        Color brightColor = originalColor * 1.3f;
        brightColor.a = originalColor.a;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            spriteRenderer.color = Color.Lerp(brightColor, originalColor, progress);
            yield return null;
        }

        spriteRenderer.color = originalColor;
    }
}