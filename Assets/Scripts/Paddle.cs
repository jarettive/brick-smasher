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

    [Header("Touch Area")]
    [SerializeField]
    private RectTransform paddleArea;

    [Header("Strike")]
    [SerializeField]
    private float strikeDistance = 0.5f;

    [SerializeField]
    private float strikeUpSpeed = 15f;

    [SerializeField]
    private float strikeFallSpeed = 8f;

    private enum StrikePhase
    {
        Idle,
        Rising,
        Falling,
    }

    private StrikePhase strikePhase = StrikePhase.Idle;
    private float strikeStartY;
    private bool strikeRequested;

    public bool IsStriking => strikePhase == StrikePhase.Rising;

    public void RequestStrike() => strikeRequested = true;

    private float currentVelocity;
    private Camera mainCamera;
    private Rigidbody2D rb;

    private InputAction pointerPositionAction;
    private InputAction pointerPressAction;
    private InputAction strikeAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;

        pointerPositionAction = new InputAction("PointerPosition", binding: "<Pointer>/position");
        pointerPressAction = new InputAction("PointerPress", binding: "<Pointer>/press");
        strikeAction = new InputAction("Strike", binding: "<Keyboard>/space");
        strikeAction.AddBinding("<Touchscreen>/touch1/press");
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        pointerPositionAction?.Enable();
        pointerPressAction?.Enable();
        strikeAction?.Enable();
    }

    private void OnDisable()
    {
        pointerPositionAction?.Disable();
        pointerPressAction?.Disable();
        strikeAction?.Disable();
    }

    private void Update()
    {
        UpdateStrike();

        if (!TryTouchMove())
        {
            Move();
        }
    }

    private void UpdateStrike()
    {
        // Start strike on input (keyboard/touch or button request)
        if (strikeAction.WasPressedThisFrame() || strikeRequested)
        {
            strikeRequested = false;
            if (strikePhase == StrikePhase.Idle)
            {
                strikePhase = StrikePhase.Rising;
                strikeStartY = transform.position.y;
            }
        }

        // Handle strike movement
        switch (strikePhase)
        {
            case StrikePhase.Rising:
                float newYUp = transform.position.y + strikeUpSpeed * Time.deltaTime;
                if (newYUp >= strikeStartY + strikeDistance)
                {
                    newYUp = strikeStartY + strikeDistance;
                    strikePhase = StrikePhase.Falling;
                }
                transform.position = new Vector3(
                    transform.position.x,
                    newYUp,
                    transform.position.z
                );
                break;

            case StrikePhase.Falling:
                float newYDown = transform.position.y - strikeFallSpeed * Time.deltaTime;
                if (newYDown <= strikeStartY)
                {
                    newYDown = strikeStartY;
                    strikePhase = StrikePhase.Idle;
                }
                transform.position = new Vector3(
                    transform.position.x,
                    newYDown,
                    transform.position.z
                );
                break;
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

        // Only respond to touches within the paddle area
        if (
            paddleArea != null
            && !RectTransformUtility.RectangleContainsScreenPoint(
                paddleArea,
                inputPosition,
                mainCamera
            )
        )
            return false;

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

        // Clamp movement to not overshoot target
        float maxMove = Mathf.Abs(diff);
        float actualMove = Mathf.Clamp(currentVelocity * Time.deltaTime, -maxMove, maxMove);
        float newX = transform.position.x + actualMove;
        rb.MovePosition(new Vector2(newX, transform.position.y));
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
