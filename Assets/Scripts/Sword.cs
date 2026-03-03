using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sword hitbox that detects brick collisions during a swing.
/// Each brick is only hit once per swing.
/// </summary>
public class Sword : MonoBehaviour
{
    private readonly HashSet<Brick> hitBricks = new();

    public Action<Sword, Brick> OnBrickCollision { get; set; }

    public void HandleBrickCollision(Brick brick)
    {
        if (hitBricks.Contains(brick))
            return;

        hitBricks.Add(brick);
        OnBrickCollision?.Invoke(this, brick);
    }

    public void ResetHitTracking()
    {
        hitBricks.Clear();
    }
}
