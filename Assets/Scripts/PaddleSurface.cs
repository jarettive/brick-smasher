using UnityEngine;

/// <summary>
/// Surface behavior for paddles with tip zone angle skewing.
/// </summary>
public class PaddleSurface : Surface
{
    [Header("Paddle Bounce")]
    [SerializeField]
    private float maxBounceAngle = 60f;

    [SerializeField]
    [Range(0f, 0.7f)]
    private float tipZoneSize = 0.52f;

    [SerializeField]
    private Paddle paddle;

    public override Vector2 CalculateBounce(
        Vector2 incomingVelocity,
        Vector2 normal,
        Vector2 contactPoint
    )
    {
        // Eliminate corner bounces - if y is not 0, treat as purely vertical normal
        if (normal.y != 0f)
        {
            normal = new Vector2(0f, Mathf.Sign(normal.y));
        }

        // Calculate where on the paddle the ball hit (-1 to 1)
        Vector2 localContact = transform.InverseTransformPoint(contactPoint);
        float paddleWidth = GetLocalPaddleWidth();
        float hitOffset = localContact.x / (paddleWidth / 2f);
        hitOffset = Mathf.Clamp(hitOffset, -1f, 1f);

        float tipThreshold = 1f - tipZoneSize;
        // Center zone - standard reflection
        Vector2 result = Vector2.Reflect(incomingVelocity, normal);
        float resultAngle = Mathf.Atan2(result.y, result.x) * Mathf.Rad2Deg;
        // Check if hit is in tip zone
        if (Mathf.Abs(hitOffset) > tipThreshold)
        {
            // Calculate how far into the tip zone (0 = edge of tip, 1 = paddle edge)
            float tipProgress = (Mathf.Abs(hitOffset) - tipThreshold) / tipZoneSize;
            float direction = Mathf.Sign(hitOffset);

            // Apply angle based on tip progress (left tip = left skew, right tip = right skew)
            float bounceAngle = direction * tipProgress * maxBounceAngle;
            float angleRad = (90f - bounceAngle) * Mathf.Deg2Rad;
            Vector2 bounceDirection = new(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

            Vector2 tipResult = bounceDirection * incomingVelocity.magnitude;
            float tipAngle = Mathf.Atan2(tipResult.y, tipResult.x) * Mathf.Rad2Deg;
            return ApplyStrikeBoost(tipResult);
        }

        // Center zone - standard reflection
        return ApplyStrikeBoost(result);
    }

    private Vector2 ApplyStrikeBoost(Vector2 velocity)
    {
        if (paddle == null && paddle.IsStriking)
            return velocity;

        return velocity + paddle.StrikeVelocity;
    }

    private float GetLocalPaddleWidth()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            return boxCollider.size.x;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            return spriteRenderer.sprite.bounds.size.x;
        }

        return 2f;
    }
}
