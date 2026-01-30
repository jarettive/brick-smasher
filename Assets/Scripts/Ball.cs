using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ball that bounces off walls and applies knockback to bricks.
/// Uses manual physics with collision detection.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    public const float MinVerticalVelocity = 1.25f;
    private const float PaddleCollisionCooldown = 0.1f;

    [SerializeField]
    private float minSpeed = 10f;

    [SerializeField]
    [Tooltip("Speed decay per second toward minSpeed")]
    private float speedDecay = 5f;

    [SerializeField]
    private float rotationSpeedMultiplier = 50f;

    [SerializeField]
    [Tooltip("Initial launch angle in degrees (0 = right, 90 = up)")]
    private float initialLaunchAngle = 20f;

    private Vector2 velocity;
    private Vector2 preBounceVelocity;
    private float angularVelocity;
    private CircleCollider2D circleCollider;
    private Rigidbody2D rb;
    private bool isLaunched;
    private float lastPaddleCollisionTime = float.NegativeInfinity;

    // Queue collisions to process together and avoid interference
    private struct PendingCollision
    {
        public Vector2 normal;
        public Vector2 contactPoint;
        public Vector2 relativeVelocity;
        public Surface surface;
        public bool isWall;
    }

    private List<PendingCollision> pendingCollisions = new();

    public Vector2 Velocity => velocity;
    public Vector2 PreBounceVelocity => preBounceVelocity;

    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        // Kinematic so we control movement, but still get collision events
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        float angle = initialLaunchAngle * Mathf.Deg2Rad;
        Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
        Launch(direction);
    }

    /// <summary>
    /// Launch the ball in a direction.
    /// </summary>
    public void Launch(Vector2 direction)
    {
        velocity = direction.normalized * minSpeed;
        isLaunched = true;
    }

    private void FixedUpdate()
    {
        if (!isLaunched)
            return;

        ProcessPendingCollisions();
        Move();
    }

    private void Move()
    {
        preBounceVelocity = velocity;

        // Decay speed toward minSpeed
        float currentSpeed = velocity.magnitude;
        if (currentSpeed > minSpeed)
        {
            float newSpeed = Mathf.Max(minSpeed, currentSpeed - speedDecay * Time.fixedDeltaTime);
            velocity = velocity.normalized * newSpeed;
        }

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation + angularVelocity * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isLaunched)
            return;

        // Use collision.collider to get the actual collider hit (not the rigidbody's gameObject)
        GameObject hitObject = collision.collider.gameObject;

        // Apply cooldown for paddle collisions to prevent rapid repeat bounces
        bool isPaddle = hitObject.GetComponent<Paddle>() != null;
        if (isPaddle)
        {
            if (Time.time - lastPaddleCollisionTime < PaddleCollisionCooldown)
                return;
            lastPaddleCollisionTime = Time.time;
        }

        // Queue collision for processing - don't modify velocity immediately
        // This prevents multiple same-frame collisions from interfering
        Surface surface = hitObject.GetComponent<Surface>();
        bool isBrick = hitObject.GetComponent<Brick>() != null;

        if (isBrick)
        {
            lastPaddleCollisionTime = float.NegativeInfinity;
        }

        pendingCollisions.Add(
            new PendingCollision
            {
                normal = collision.contacts[0].normal,
                contactPoint = collision.contacts[0].point,
                relativeVelocity = collision.relativeVelocity,
                surface = surface,
                isWall = !isBrick,
            }
        );
    }

    private void ProcessPendingCollisions()
    {
        if (pendingCollisions.Count == 0)
            return;

        // Calculate each bounce independently, combine directions, use max magnitude
        Vector2 combinedDirection = Vector2.zero;
        Vector2 combinedRelativeVelocity = Vector2.zero;
        float maxMagnitude = 0f;

        foreach (var collision in pendingCollisions)
        {
            Vector2 bounceVelocity;
            if (collision.surface != null)
            {
                bounceVelocity = collision.surface.CalculateBounce(
                    preBounceVelocity,
                    collision.normal,
                    collision.contactPoint
                );
            }
            else
            {
                bounceVelocity = Vector2.Reflect(preBounceVelocity, collision.normal);
            }

            combinedDirection += bounceVelocity.normalized;
            maxMagnitude = Mathf.Max(maxMagnitude, bounceVelocity.magnitude);
            combinedRelativeVelocity += collision.relativeVelocity;
        }

        velocity = combinedDirection.normalized * maxMagnitude;

        ApplyBounceRotation(combinedRelativeVelocity);
        EnsureMinimumVerticalVelocity();
        EnsureMinimumSpeed();

        pendingCollisions.Clear();
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

    private void EnsureMinimumSpeed()
    {
        float currentSpeed = velocity.magnitude;
        if (currentSpeed < minSpeed)
        {
            velocity = velocity.normalized * minSpeed;
        }
    }
}
