using UnityEngine;

/// <summary>
/// Ball that bounces off walls and applies knockback to bricks.
/// Uses manual physics with collision detection.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    [SerializeField]
    private float speed = 10f;

    [SerializeField]
    private float minVerticalVelocity = 2f;

    private Vector2 velocity;
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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isLaunched)
            return;

        // Get the contact normal for reflection
        Vector2 normal = collision.contacts[0].normal;
        Bounce(normal);
    }

    private void Bounce(Vector2 normal)
    {
        // Reflect velocity off the surface normal
        velocity = Vector2.Reflect(velocity, normal);

        EnsureMinimumVerticalVelocity();

        // Maintain speed
        velocity = velocity.normalized * speed;
    }

    private void EnsureMinimumVerticalVelocity()
    {
        // Prevent ball from going too horizontal (boring gameplay)
        if (Mathf.Abs(velocity.y) < minVerticalVelocity)
        {
            float sign = velocity.y >= 0 ? 1f : -1f;
            velocity.y = minVerticalVelocity * sign;
        }
    }
}
