using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player-controlled paddle movement.
/// Add a PaddleSurface component for bounce behavior.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Paddle : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 10f;

    [SerializeField]
    private float accelerationTime = 10f / 60f;

    private float currentVelocity;
    private Camera mainCamera;
    private Rigidbody2D rb;

    private InputAction pointerPositionAction;
    private InputAction pointerPressAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;

        pointerPositionAction = new InputAction("PointerPosition", binding: "<Pointer>/position");
        pointerPressAction = new InputAction("PointerPress", binding: "<Pointer>/press");
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        pointerPositionAction?.Enable();
        pointerPressAction?.Enable();
    }

    private void OnDisable()
    {
        pointerPositionAction?.Disable();
        pointerPressAction?.Disable();
    }

    private void Update()
    {
        if (!TryTouchMove())
        {
            Move();
        }
    }

    private bool TryTouchMove()
    {
        if (mainCamera == null)
            return false;

        // Check for pointer press (unified touch/mouse)
        if (!pointerPressAction.IsPressed())
            return false;

        Vector2 inputPosition = pointerPositionAction.ReadValue<Vector2>();

        // Convert screen position to world position
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(inputPosition.x, inputPosition.y, mainCamera.nearClipPlane)
        );

        float targetX = worldPos.x;

        // Calculate direction to target
        float diff = targetX - transform.position.x;

        // Stop if close enough
        if (Mathf.Abs(diff) < 0.01f)
        {
            currentVelocity = 0f;
            return true;
        }

        float targetVelocity = Mathf.Sign(diff) * moveSpeed;

        // Accelerate towards target velocity
        float acceleration = moveSpeed / accelerationTime;
        currentVelocity = Mathf.MoveTowards(
            currentVelocity,
            targetVelocity,
            acceleration * Time.deltaTime
        );

        ApplyMovement();
        return true;
    }

    private void Move()
    {
        if (InputController.Instance == null)
            return;

        float input = InputController.Instance.GetHorizontalInput();
        float targetVelocity = input * moveSpeed;

        // Accelerate towards target velocity
        float acceleration = moveSpeed / accelerationTime;
        currentVelocity = Mathf.MoveTowards(
            currentVelocity,
            targetVelocity,
            acceleration * Time.deltaTime
        );

        ApplyMovement();
    }

    private void ApplyMovement()
    {
        float newX = transform.position.x + currentVelocity * Time.deltaTime;
        rb.MovePosition(new Vector2(newX, transform.position.y));
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleBrickWallCollision(collision);
    }

    private void HandleBrickWallCollision(Collision2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer(Layers.BrickWall))
            return;

        // Push paddle back out of the wall
        ContactPoint2D contact = collision.contacts[0];
        float penetration = contact.separation;

        // separation is negative when overlapping
        if (penetration < 0)
        {
            Vector2 pushBack = contact.normal * -penetration * 1f;
            transform.position += (Vector3)pushBack;
        }
        currentVelocity = 0f;
    }
}
