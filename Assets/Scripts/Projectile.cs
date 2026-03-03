using System;
using UnityEngine;

/// <summary>
/// A projectile that moves in a direction.
/// Collision behavior is determined by the callback set by the spawner.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : StageEntity
{
    [SerializeField]
    private float speed = 15f;

    [SerializeField]
    private float lifetime = 3f;

    private Rigidbody2D rb;
    private Vector2 direction;
    private float spawnTime;

    public Action<Projectile, Brick> OnBrickCollision { get; set; }

    public Vector2 Direction => direction;

    public float Lifetime
    {
        get => lifetime;
        set => lifetime = value;
    }

    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        collider.isTrigger = true;
    }

    protected override void Start()
    {
        base.Start();
        spawnTime = Time.time;
    }

    /// <summary>
    /// Initialize and launch the projectile in a direction.
    /// </summary>
    public void Launch(Vector2 launchDirection)
    {
        direction = launchDirection.normalized;
        spawnTime = Time.time;

        // Rotate to face direction of travel (assuming sprite faces up by default)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void FixedUpdate()
    {
        // Move projectile
        rb.MovePosition(rb.position + speed * stageScale * Time.fixedDeltaTime * direction);

        // Check lifetime
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Brick>(out var brick))
        {
            OnBrickCollision?.Invoke(this, brick);
            Destroy(gameObject, 4f / 60f);
        }
    }
}
