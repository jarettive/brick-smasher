using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sword hitbox that detects brick collisions during a swing.
/// The same brick can only be hit again after the cooldown has elapsed.
/// </summary>
public class Sword : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Seconds that must pass before the same brick can be hit again")]
    private float hitCooldown = 1f;

    [SerializeField]
    private SpriteRenderer bladeSpriteRenderer;

    [SerializeField]
    [Tooltip("Time for the blade sprite to fade in when the sword is spawned")]
    private float fadeInDuration = 0.1f;

    [SerializeField]
    [Tooltip("Time for the blade sprite to fade out before destruction")]
    private float fadeOutDuration = 0.1f;

    private readonly Dictionary<Brick, float> lastHitTimes = new();

    public Action<Sword, Brick> OnBrickCollision { get; set; }

    public void HandleBrickCollision(Brick brick)
    {
        if (lastHitTimes.TryGetValue(brick, out float lastHit) && Time.time - lastHit < hitCooldown)
            return;

        lastHitTimes[brick] = Time.time;
        OnBrickCollision?.Invoke(this, brick);
    }

    private void Awake()
    {
        if (bladeSpriteRenderer == null)
            bladeSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        StartCoroutine(FadeRoutine(0f, 1f, fadeInDuration));
    }

    public void FadeOutAndDestroy()
    {
        StartCoroutine(FadeOutAndDestroyRoutine());
    }

    private IEnumerator FadeOutAndDestroyRoutine()
    {
        yield return FadeRoutine(GetAlpha(), 0f, fadeOutDuration);
        Destroy(gameObject);
    }

    private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration)
    {
        if (bladeSpriteRenderer == null)
            yield break;

        SetAlpha(fromAlpha);

        if (duration <= 0f)
        {
            SetAlpha(toAlpha);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(fromAlpha, toAlpha, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }

        SetAlpha(toAlpha);
    }

    private float GetAlpha() => bladeSpriteRenderer != null ? bladeSpriteRenderer.color.a : 1f;

    private void SetAlpha(float a)
    {
        Color c = bladeSpriteRenderer.color;
        c.a = a;
        bladeSpriteRenderer.color = c;
    }
}
