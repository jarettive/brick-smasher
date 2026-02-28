using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ball that bounces off walls and applies knockback to bricks.
/// Uses manual physics with overlap detection after movement.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Ball : StageEntity
{
    public const float MinVerticalVelocity = 1.25f;
    private const float PaddleCollisionCooldown = 0.1f;
    private const float UnderSpawnDuration = 1.5f;
    private const float BlastZoneImmunityDuration = 0.35f;

    [SerializeField]
    private BallProps props;

    [SerializeField]
    private float rotationSpeedMultiplier = 50f;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    [Tooltip("Minimum launch angle in degrees (0 = right, 90 = up)")]
    private float minLaunchAngle = 20f;

    [SerializeField]
    [Tooltip("Maximum launch angle in degrees. If equal to min, uses exact angle.")]
    private float maxLaunchAngle = 20f;

    private Vector2 velocity;
    private Vector2 preBounceVelocity;
    private float angularVelocity;
    private CircleCollider2D circleCollider;
    private Rigidbody2D rb;
    private bool enforceMinSpeed = true;
    private float lastPaddleCollisionTime = float.NegativeInfinity;

    private ContactFilter2D contactFilter;
    private readonly Collider2D[] overlapResults = new Collider2D[8];
    private readonly HashSet<Collider2D> processedThisFrame = new();
    private bool collisionOccurredThisFrame;
    private bool isActive = true;
    private bool underSpawn;
    private float underSpawnEndTime;
    private float spawnTime;

    public Vector2 Velocity
    {
        get => velocity;
        set => velocity = value;
    }

    public Vector2 PreBounceVelocity => preBounceVelocity;
    public float Damage => props.Damage;
    public bool IsActive => isActive;
    public bool EnforceMinSpeed
    {
        get => enforceMinSpeed;
        set => enforceMinSpeed = value;
    }

    /// <summary>
    /// Fired when a ball is lost (enters BlastZone).
    /// </summary>
    public static event Action OnBallLost;

    /// <summary>
    /// Returns true if a collision occurred this frame. Resets the flag when called.
    /// </summary>
    public bool CheckAndClearCollision()
    {
        bool result = collisionOccurredThisFrame;
        collisionOccurredThisFrame = false;
        return result;
    }

    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;

        contactFilter = new ContactFilter2D { useTriggers = false };
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;
    }

    private void OnValidate()
    {
        if (props != null && props.Sprite != null)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = props.Sprite;
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        spawnTime = Time.time;
        float launchAngle = UnityEngine.Random.Range(minLaunchAngle, maxLaunchAngle);
        float angle = launchAngle * Mathf.Deg2Rad;
        Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
        Launch(direction);
    }

    /// <summary>
    /// Initialize the ball with props at runtime (for spawned balls).
    /// </summary>
    public void Initialize(BallProps newProps, Vector2 direction, bool startInactive = false)
    {
        props = newProps;

        // Check if spawned below paddle
        Paddle paddle = FindAnyObjectByType<Paddle>();
        bool isBelowPaddle = paddle != null && transform.position.y < paddle.transform.position.y;

        if (isBelowPaddle)
        {
            isActive = true;
            underSpawn = true;
            underSpawnEndTime = Time.time + UnderSpawnDuration;
        }
        else
        {
            isActive = !startInactive;
        }

        if (spriteRenderer != null && props.Sprite != null)
        {
            spriteRenderer.sprite = props.Sprite;
        }

        float exact = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        minLaunchAngle = exact;
        maxLaunchAngle = exact;
    }

    /// <summary>
    /// Launch the ball in a direction.
    /// </summary>
    public void Launch(Vector2 direction)
    {
        velocity = direction.normalized * props.MinSpeed;

        if (props.FixedRotation)
        {
            angularVelocity = props.RotationSpeed;
        }
    }

    private void FixedUpdate()
    {
        // Clear underSpawn after duration expires
        if (underSpawn && Time.time >= underSpawnEndTime)
        {
            underSpawn = false;
        }

        preBounceVelocity = velocity;

        // Decay speed toward minSpeed
        if (enforceMinSpeed)
        {
            float currentSpeed = velocity.magnitude;
            if (currentSpeed > props.MinSpeed)
            {
                float newSpeed = Mathf.Max(
                    props.MinSpeed,
                    currentSpeed - props.SpeedDecay * Time.fixedDeltaTime
                );
                velocity = velocity.normalized * newSpeed;
            }
        }

        // Move the ball
        Vector2 newPosition = rb.position + stageScale * Time.fixedDeltaTime * velocity;
        rb.MovePosition(newPosition);
        rb.MoveRotation(rb.rotation + angularVelocity * Time.fixedDeltaTime);

        // Check for overlaps after moving and process collisions
        ProcessCollisions();
    }

    private void OnEnable()
    {
        PlayerController.OnSmashTriggered += Smash;
    }

    private void OnDisable()
    {
        PlayerController.OnSmashTriggered -= Smash;
    }

    private void Update()
    {
        if (!isActive)
            return;

        props.Behavior?.OnUpdate(this);
    }

    /// <summary>
    /// Called when the Smash button is pressed.
    /// </summary>
    public void Smash()
    {
        if (!isActive)
            return;

        props.Behavior?.Smash(this);
    }

    private void ProcessCollisions()
    {
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

            // Destroyed by BlastZone (after immunity period)
            if (hitObject.layer == LayerMask.NameToLayer(Layers.BlastZone))
            {
                if (Time.time - spawnTime < BlastZoneImmunityDuration)
                    continue;

                OnBallLost?.Invoke();
                Destroy(gameObject);
                return;
            }

            // Check if this is a paddle or brick collision
            bool isPaddle = hitObject.layer == LayerMask.NameToLayer(Layers.PaddleSurface);
            bool isBrick = hitObject.GetComponent<Brick>() != null;

            // Skip non-paddle collisions if ball is inactive
            if (!isActive && !isPaddle)
                continue;

            // Skip paddle and brick collisions during underSpawn
            if (underSpawn && (isPaddle || isBrick))
                continue;

            // Activate ball on paddle hit
            if (isPaddle)
            {
                isActive = true;

                if (Time.time - lastPaddleCollisionTime < PaddleCollisionCooldown)
                    continue;
                lastPaddleCollisionTime = Time.time;
            }

            // Reset paddle cooldown when hitting a brick
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
            collisionOccurredThisFrame = true;
            velocity = combinedDirection.normalized * maxMagnitude;
            ApplyBounceRotation(combinedRelativeVelocity);
            EnsureMinimumVerticalVelocity();
            EnsureMinimumSpeed();
        }
    }

    private void ApplyBounceRotation(Vector2 relativeVelocity)
    {
        // Skip if using fixed rotation from props
        if (props.FixedRotation)
            return;

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
        if (currentSpeed < props.MinSpeed)
        {
            velocity = velocity.normalized * props.MinSpeed;
        }
    }
}
