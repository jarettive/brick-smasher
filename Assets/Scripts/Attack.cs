using UnityEngine;

/// <summary>
/// Describes an attack that deals damage and knockback to bricks.
/// baseKnockback applies a flat knockback regardless of damage %.
/// knockbackScaling multiplies with damage % for increasing knockback.
/// </summary>
public struct Attack
{
    public float Damage;
    public float BaseKnockback;
    public float KnockbackScaling;
    public Vector2 Direction;
}
