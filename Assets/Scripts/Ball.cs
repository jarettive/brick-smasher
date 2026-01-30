using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ball that bounces off walls and applies knockback to bricks.
/// Uses manual physics with overlap detection after movement.
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

    private ContactFilter2D contactFilter;
    private readonly Collider2D[] overlapResults = new Collider2D[8];
    private readonly HashSet<Collider2D> processedThisFrame = new();

    public Vector2 Velocity => velocity;
    public Vector2 PreBounceVelocity => preBounceVelocity;

    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;

        contactFilter = new ContactFilter2D { useTriggers = false };
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;
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

        preBounceVelocity = velocity;

        // Decay speed toward minSpeed
        float currentSpeed = velocity.magnitude;
        if (currentSpeed > minSpeed)
        {
            float newSpeed = Mathf.Max(minSpeed, currentSpeed - speedDecay * Time.fixedDeltaTime);
            velocity = velocity.normalized * newSpeed;
        }

        // Move the ball
        Vector2 newPosition = rb.position + velocity * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
        rb.MoveRotation(rb.rotation + angularVelocity * Time.fixedDeltaTime);

        // Check for overlaps after moving and process collisions
        ProcessCollisions();
    }

    private void ProcessCollisions()
    {
        if (!isLaunched)
            return;

        processedThisFrame.Clear();

        int overlapCount = Physics2D.OverlapCollider(circleCollider, contactFilter, overlapResults);

        if (overlapCount == 0)
            return;

        Vector2 combinedDirection = Vector2.zero;
        float maxMagnitude = 0f;
        Vector2 combinedRelativeVelocity = Vector2.zero;
        int validCollisions = 0;

        for (int i = 0; i < overlapCount; i++)
        {
            Collider2D hitCollider = overlapResults[i];

            // Skip self
            if (hitCollider == circleCollider)
                continue;

            // Skip already processed
            if (processedThisFrame.Contains(hitCollider))
                continue;

            processedThisFrame.Add(hitCollider);

            GameObject hitObject = hitCollider.gameObject;

            // Apply cooldown for paddle collisions
            bool isPaddle = hitObject.layer == LayerMask.NameToLayer(Layers.PaddleSurface);
            if (isPaddle)
            {
                if (Time.time - lastPaddleCollisionTime < PaddleCollisionCooldown)
                    continue;
                lastPaddleCollisionTime = Time.time;
            }

            // Reset paddle cooldown when hitting a brick
            bool isBrick = hitObject.GetComponent<Brick>() != null;
            if (isBrick)
            {
                lastPaddleCollisionTime = float.NegativeInfinity;
            }

            // Get collision normal using Distance
            ColliderDistance2D distance = circleCollider.Distance(hitCollider);
            if (!distance.isOverlapped && distance.distance > 0.01f)
                continue;

            Vector2 normal = distance.normal;
            Vector2 contactPoint = distance.pointA;

            // Push ball out of collision
            if (distance.isOverlapped)
            {
                rb.MovePosition(rb.position + (1.1f * distance.distance * normal));
            }

            // Calculate bounce
            Surface surface = hitObject.GetComponent<Surface>();
            Vector2 bounceVelocity;
            if (surface != null)
            {
                bounceVelocity = surface.CalculateBounce(preBounceVelocity, normal, contactPoint);
            }
            else
            {
                bounceVelocity = Vector2.Reflect(preBounceVelocity, normal);
            }

            combinedDirection += bounceVelocity.normalized;
            maxMagnitude = Mathf.Max(maxMagnitude, bounceVelocity.magnitude);

            // Estimate relative velocity (paddle movement affects this)
            Rigidbody2D hitRb = hitCollider.attachedRigidbody;
            Vector2 hitVelocity = hitRb != null ? hitRb.linearVelocity : Vector2.zero;
            combinedRelativeVelocity += velocity - hitVelocity;

            validCollisions++;
        }

        if (validCollisions > 0)
        {
            velocity = combinedDirection.normalized * maxMagnitude;
            ApplyBounceRotation(combinedRelativeVelocity);
            EnsureMinimumVerticalVelocity();
            EnsureMinimumSpeed();
        }
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
