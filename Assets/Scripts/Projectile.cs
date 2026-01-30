using UnityEngine;

/// <summary>
/// A projectile that moves in a direction and damages bricks on contact.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField]
    private float speed = 15f;

    [SerializeField]
    private float damage = 20f;

    [SerializeField]
    private float lifetime = 3f;

    private Rigidbody2D rb;
    private Vector2 direction;
    private float spawnTime;

    public float Damage
    {
        get => damage;
        set => damage = value;
    }

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

    private void Start()
    {
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
        // transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void FixedUpdate()
    {
        // Move projectile
        rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * direction);

        // Check lifetime
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Damage bricks
        if (other.TryGetComponent<Brick>(out var brick))
        {
            brick.ApplyDamage(damage, direction);
            Destroy(gameObject);
            return;
        }
    }
}
