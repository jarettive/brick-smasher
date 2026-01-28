using UnityEngine;

/// <summary>
/// Attach to any object that the ball can bounce off.
/// Defines how the ball should react when hitting this surface.
/// </summary>
public class Surface : MonoBehaviour
{
    [SerializeField]
    private float bounceMultiplier = 1f;

    [SerializeField]
    private float maxAngleAdjustment = 0f;

    /// <summary>
    /// Calculate the resulting velocity when a ball bounces off this surface.
    /// </summary>
    /// <param name="incomingVelocity">The ball's velocity before impact</param>
    /// <param name="normal">The surface normal at the contact point</param>
    /// <param name="contactPoint">The world position of the contact</param>
    /// <returns>The ball's velocity after bouncing</returns>
    public virtual Vector2 CalculateBounce(
        Vector2 incomingVelocity,
        Vector2 normal,
        Vector2 contactPoint
    )
    {
        // Default reflection
        Vector2 reflected = Vector2.Reflect(incomingVelocity, normal);

        // Apply speed multiplier
        reflected *= bounceMultiplier;

        // Optional angle adjustment (useful for paddles)
        if (maxAngleAdjustment > 0f)
        {
            float adjustment = Random.Range(-maxAngleAdjustment, maxAngleAdjustment);
            reflected = Quaternion.Euler(0, 0, adjustment) * reflected;
        }

        return reflected;
    }
}
