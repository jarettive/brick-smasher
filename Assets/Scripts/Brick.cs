using TMPro;
using UnityEngine;

/// <summary>
/// Brick that takes percentage damage and gets knocked back when hit by the ball.
/// Knockback formula: (Percentage/10) + (Percentage * Damage)/20
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Brick : MonoBehaviour
{
    [SerializeField]
    private float knockbackDecay = 5f;

    [SerializeField]
    private float returnSpeed = 3f;

    [SerializeField]
    private float maxReturnDistance = 5f;

    [SerializeField]
    private TextMeshProUGUI percentageText;

    private Rigidbody2D rb;
    private float percentage;
    private Vector2 knockbackVelocity;
    private Vector2 originalPosition;

    public float Percentage => percentage;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void Start()
    {
        originalPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (knockbackVelocity.sqrMagnitude > 0.01f)
        {
            rb.MovePosition(rb.position + knockbackVelocity * Time.fixedDeltaTime);
            knockbackVelocity = Vector2.Lerp(
                knockbackVelocity,
                Vector2.zero,
                knockbackDecay * Time.fixedDeltaTime
            );
        }
        else
        {
            ReturnToOriginalPosition();
        }
    }

    private void ReturnToOriginalPosition()
    {
        Vector2 toHome = originalPosition - (Vector2)transform.position;
        float distance = toHome.magnitude;

        if (distance < 0.01f)
            return;

        // Speed scales from 1x to 2x based on distance
        float distanceRatio = Mathf.Clamp01(distance / maxReturnDistance);
        float speedMultiplier = 1f + distanceRatio;
        float currentSpeed = returnSpeed * speedMultiplier;

        Vector2 movement = toHome.normalized * currentSpeed * Time.fixedDeltaTime;

        // Don't overshoot
        if (movement.magnitude > distance)
        {
            rb.MovePosition(originalPosition);
        }
        else
        {
            rb.MovePosition(rb.position + movement);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int layer = collision.gameObject.layer;

        // Destroyed by BlastZone
        if (layer == LayerMask.NameToLayer(Layers.BlastZone))
        {
            Destroy(gameObject);
            return;
        }

        Vector2 normal = collision.contacts[0].normal;

        // Bounce off BrickWall layer
        if (layer == LayerMask.NameToLayer(Layers.BrickWall))
        {
            knockbackVelocity = Vector2.Reflect(knockbackVelocity, normal);
            return;
        }

        // Take damage from ball
        Ball ball = collision.gameObject.GetComponent<Ball>();
        if (ball == null)
            return;

        float damage = ball.Velocity.magnitude * 2;
        ApplyDamage(damage, normal);
    }

    /// <summary>
    /// Apply damage to the brick and knock it back.
    /// </summary>
    /// <param name="damage">Amount of damage to apply</param>
    /// <param name="direction">Direction of the knockback (normalized)</param>
    public void ApplyDamage(float damage, Vector2 direction)
    {
        percentage += damage;
        UpdatePercentageDisplay();

        // Knockback formula: (Percentage/10) + (Percentage * Damage)/20
        float knockbackForce = (percentage / 10f) + (percentage * damage) / 20f;
        knockbackVelocity += direction.normalized * knockbackForce * .25f;
    }

    private void UpdatePercentageDisplay()
    {
        if (percentageText != null)
        {
            percentageText.text = Mathf.RoundToInt(percentage) + "%";
        }
    }
}
