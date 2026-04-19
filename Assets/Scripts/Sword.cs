using System;
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

    private readonly Dictionary<Brick, float> lastHitTimes = new();

    public Action<Sword, Brick> OnBrickCollision { get; set; }

    public void HandleBrickCollision(Brick brick)
    {
        if (lastHitTimes.TryGetValue(brick, out float lastHit) && Time.time - lastHit < hitCooldown)
            return;

        lastHitTimes[brick] = Time.time;
        OnBrickCollision?.Invoke(this, brick);
    }
}
