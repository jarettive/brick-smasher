using UnityEngine;

/// <summary>
/// Ball that bounces off walls and applies knockback to bricks.
/// Uses manual physics with collision detection.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    public const float MinVerticalVelocity = 0.75f;

    [SerializeField]
    private float speed = 10f;

    [SerializeField]
    private float rotationSpeedMultiplier = 50f;

    private Vector2 velocity;
    private float angularVelocity;
    private CircleCollider2D circleCollider;
    private Rigidbody2D rb;
    private bool isLaunched;

    public Vector2 Velocity => velocity;

    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        // Kinematic so we control movement, but still get collision events
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
    }

    private void Start()
    {
        // Launch at 20 degrees (0 = right, 90 = up)
        float angle = 20f * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Launch(direction);
    }

    /// <summary>
    /// Launch the ball in a direction.
    /// </summary>
    public void Launch(Vector2 direction)
    {
        velocity = direction.normalized * speed;
        isLaunched = true;
    }

    private void FixedUpdate()
    {
        if (!isLaunched)
            return;

        Move();
    }

    private void Move()
    {
        // MovePosition ensures proper collision detection for kinematic bodies
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation + angularVelocity * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isLaunched)
            return;

        Vector2 normal = collision.contacts[0].normal;
        Vector2 contactPoint = collision.contacts[0].point;

        // Let the surface define bounce behavior if it has one
        Surface surface = collision.gameObject.GetComponent<Surface>();
        if (surface != null)
        {
            velocity = surface.CalculateBounce(velocity, normal, contactPoint);
        }
        else
        {
            // Default bounce for objects without Surface component
            velocity = Vector2.Reflect(velocity, normal);
        }

        // Apply rotation based on horizontal velocity change
        ApplyBounceRotation(collision.relativeVelocity);

        EnsureMinimumVerticalVelocity();
        velocity = velocity.normalized * speed;
    }

    private void ApplyBounceRotation(Vector2 relativeVelocity)
    {
        // Angular velocity influenced by horizontal component of impact
        angularVelocity = -relativeVelocity.x * rotationSpeedMultiplier;
    }

    private void EnsureMinimumVerticalVelocity()
    {
        // Prevent ball from going too horizontal (boring gameplay)
        if (Mathf.Abs(velocity.y) < MinVerticalVelocity)
        {
            // Use a small threshold to avoid sign flipping on near-zero values
            float sign = velocity.y < -0.01f ? -1f : 1f;
            velocity.y = MinVerticalVelocity * sign;
        }
    }
}
