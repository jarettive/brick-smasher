using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Brick that takes percentage damage and gets knocked back when hit by the ball.
/// Knockback formula: (Percentage/10) + (Percentage * Damage)/20
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Image))]
public class Brick : StageEntity
{
    [SerializeField]
    private float knockbackDecay = 5f;

    [SerializeField]
    private float returnSpeed = 4f;

    [SerializeField]
    private float maxReturnDistance = 5f;

    [SerializeField]
    [Tooltip("Higher rigidity = less knockback. 10 = normal, 20 = half knockback")]
    private float rigidity = 10f;

    [SerializeField]
    private TextMeshProUGUI percentageText;

    [SerializeField]
    private KnockoutVFX knockoutVFXPrefab;

    [Header("Ball Contents")]
    [SerializeField]
    private BallProps ballProps;

    [SerializeField]
    private SpriteRenderer ballSpriteRenderer;

    [SerializeField]
    private Ball ballPrefab;

    private Image brickImage;

    private static readonly Color LightYellow = new(1f, 1f, 0.5f);
    private static readonly Color Orange = new(1f, 0.5f, 0f);
    private static readonly Color DarkRed = new(0.5f, 0f, 0f);
    private const float MaxPercentageForColor = 200f;
    private const float MaxHitLagShakeIntensity = 0.07f;
    private const float MinKnockbackForShake = 10f;
    private const float MaxKnockbackForShake = 45f;
    private const float HitLagKnockbackThreshold = 4.3f;
    private const int HitLagFramesPerThreshold = 1;
    private const int MaxHitLagFrames = 12;

    private Rigidbody2D rb;
    private float percentage;
    private Vector2 knockbackVelocity;
    private Vector2 originalPosition;

    // Hit lag state
    private int hitLagFramesRemaining;
    private Vector2 pendingKnockback;
    private Vector2 hitLagPosition;
    private float hitLagKnockbackForce;

    public float Percentage => percentage;

    /// <summary>
    /// Fired when a brick spawns.
    /// </summary>
    public static event Action OnBrickSpawned;

    /// <summary>
    /// Fired when a brick is knocked out. Parameters: rigidity, knockback velocity.
    /// </summary>
    public static event Action<float, Vector2> OnBrickKnockout;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        brickImage = GetComponent<Image>();
    }

    private void OnValidate()
    {
        UpdateBallSprite();
    }

    private void OnEnable()
    {
        UpdateBallSprite();
    }

    private void UpdateBallSprite()
    {
        if (ballSpriteRenderer == null)
            return;

        if (ballProps != null && ballProps.Sprite != null)
        {
            ballSpriteRenderer.sprite = ballProps.Sprite;
            ballSpriteRenderer.enabled = true;
        }
        else
        {
            ballSpriteRenderer.enabled = false;
        }
    }

    protected override void Start()
    {
        base.Start();
        originalPosition = transform.position;
        UpdatePercentageDisplay();
        OnBrickSpawned?.Invoke();
    }

    private void FixedUpdate()
    {
        // Handle hit lag
        if (hitLagFramesRemaining > 0)
        {
            hitLagFramesRemaining--;

            // Shake around hit lag position (only if knockback force >= MinKnockbackForShake)
            if (hitLagKnockbackForce >= MinKnockbackForShake)
            {
                float shakeT = Mathf.Clamp01(
                    (hitLagKnockbackForce - MinKnockbackForShake)
                        / (MaxKnockbackForShake - MinKnockbackForShake)
                );
                float shakeIntensity = shakeT * MaxHitLagShakeIntensity;
                Vector2 shake = UnityEngine.Random.insideUnitCircle * shakeIntensity;
                rb.MovePosition(hitLagPosition + shake);
            }

            // When hit lag ends, apply pending knockback
            if (hitLagFramesRemaining == 0)
            {
                knockbackVelocity += pendingKnockback;
                pendingKnockback = Vector2.zero;
                hitLagKnockbackForce = 0f;
            }
            return;
        }

        if (knockbackVelocity.sqrMagnitude > 0.01f)
        {
            rb.MovePosition(rb.position + stageScale * Time.fixedDeltaTime * knockbackVelocity);
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

        Vector2 movement = currentSpeed * stageScale * Time.fixedDeltaTime * toHome.normalized;

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
            OnKnockout();
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
        if (!collision.gameObject.TryGetComponent<Ball>(out var ball))
            return;

        // Ignore inactive balls
        if (!ball.IsActive)
            return;

        Vector2 ballVelocity = ball.PreBounceVelocity;
        float damage = ballVelocity.magnitude * ball.Damage;
        ReceiveAttack(
            new Attack
            {
                Damage = damage,
                BaseKnockback = 0f,
                KnockbackScaling = 1f,
                Direction = (ballVelocity.normalized + normal).normalized,
            }
        );
    }

    public void ReceiveAttack(Attack attack)
    {
        percentage += attack.Damage;
        UpdatePercentageDisplay();

        float scaledKnockback =
            attack.KnockbackScaling * ((percentage / 10f) + (percentage * attack.Damage) / 20f);
        float knockbackForce = (attack.BaseKnockback + scaledKnockback) / rigidity;
        Vector2 knockback = knockbackForce * attack.Direction.normalized;

        // Calculate hit lag frames based on knockback force
        int hitLagFrames =
            Mathf.FloorToInt(knockbackForce / HitLagKnockbackThreshold) * HitLagFramesPerThreshold;

        if (hitLagFrames > 0)
        {
            // Store position and pending knockback for hit lag
            if (hitLagFramesRemaining == 0)
            {
                hitLagPosition = rb.position;
            }
            hitLagFramesRemaining = Mathf.Min(
                hitLagFramesRemaining + hitLagFrames,
                MaxHitLagFrames
            );
            pendingKnockback += knockback;
            hitLagKnockbackForce += knockbackForce;
        }
        else
        {
            knockbackVelocity += knockback;
        }
    }

    private void UpdatePercentageDisplay()
    {
        if (percentageText != null)
        {
            percentageText.text = Mathf.RoundToInt(percentage) + "%";
        }
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (brickImage == null)
            return;

        float t = Mathf.Clamp01(percentage / MaxPercentageForColor);

        Color color;
        if (t < 0.5f)
        {
            // Light yellow to orange (0% to 100%)
            color = Color.Lerp(LightYellow, Orange, t * 2f);
        }
        else
        {
            // Orange to dark red (100% to 200%)
            color = Color.Lerp(Orange, DarkRed, (t - 0.5f) * 2f);
        }

        brickImage.color = color;
    }

    private void OnKnockout()
    {
        OnBrickKnockout?.Invoke(rigidity, knockbackVelocity);
        SpawnKnockoutVFX();
        SpawnBall();
        Destroy(gameObject);
    }

    private void SpawnBall()
    {
        if (ballProps == null || ballPrefab == null)
            return;

        // Find paddle to aim toward
        Paddle paddle = FindAnyObjectByType<Paddle>();
        Vector2 direction;
        if (paddle != null)
        {
            direction = (
                (Vector2)paddle.transform.position - (Vector2)transform.position
            ).normalized;
        }
        else
        {
            direction = Vector2.down;
        }

        Transform parent = Stage.Instance != null ? Stage.Instance.transform : null;

        Ball ball = Instantiate(ballPrefab, transform.position, Quaternion.identity, parent);
        ball.Initialize(ballProps, direction, startInactive: true);
    }

    private void SpawnKnockoutVFX()
    {
        if (knockoutVFXPrefab == null)
            return;

        GameObject canvasObj = GameObject.Find("WorldCanvas");
        if (canvasObj == null)
            return;

        KnockoutVFX vfx = Instantiate(knockoutVFXPrefab, canvasObj.transform);
        float angle = Mathf.Atan2(knockbackVelocity.y, knockbackVelocity.x) * Mathf.Rad2Deg;
        vfx.Initialize(transform.position, angle);
    }
}
