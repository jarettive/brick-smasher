using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player-controlled paddle movement.
/// Add a PaddleSurface component for bounce behavior.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Paddle : StageEntity
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

    [SerializeField]
    [Range(0f, 1f)]
    private float strikeBoostRatio = 0.5f;

    private enum StrikePhase
    {
        Idle,
        Rising,
        Falling,
    }

    private StrikePhase strikePhase = StrikePhase.Idle;
    private float strikeStartY;

    public bool IsStriking => strikePhase == StrikePhase.Rising;

    public Vector2 StrikeVelocity =>
        strikePhase == StrikePhase.Rising
            ? new Vector2(0f, strikeUpSpeed * strikeBoostRatio)
            : Vector2.zero;

    private float currentVelocity;
    private Camera mainCamera;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private ContactFilter2D wallFilter;
    private readonly Collider2D[] overlapResults = new Collider2D[4];

    private InputAction pointerPositionAction;
    private InputAction pointerPressAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;

        boxCollider = GetComponent<BoxCollider2D>();
        wallFilter = new ContactFilter2D();
        wallFilter.SetLayerMask(LayerMask.GetMask(Layers.BrickWall));
        wallFilter.useLayerMask = true;

        pointerPositionAction = new InputAction("PointerPosition", binding: "<Pointer>/position");
        pointerPressAction = new InputAction("PointerPress", binding: "<Pointer>/press");
    }

    protected override void Start()
    {
        base.Start();
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
        float targetY = ComputeStrikeY();

        if (!TryTouchMove(targetY))
        {
            Move(targetY);
        }
    }

    private float ComputeStrikeY()
    {
        // Start strike on input
        if (InputController.Instance != null && InputController.Instance.IsStrikeTriggered())
        {
            if (strikePhase == StrikePhase.Idle)
            {
                strikePhase = StrikePhase.Rising;
                strikeStartY = rb.position.y;
            }
        }

        // Compute target Y based on strike phase
        switch (strikePhase)
        {
            case StrikePhase.Rising:
                float newYUp = rb.position.y + strikeUpSpeed * stageScale * Time.deltaTime;
                float strikeTargetY = strikeStartY + strikeDistance * stageScale;
                if (newYUp >= strikeTargetY)
                {
                    newYUp = strikeTargetY;
                    strikePhase = StrikePhase.Falling;
                }
                return newYUp;

            case StrikePhase.Falling:
                float newYDown = rb.position.y - strikeFallSpeed * stageScale * Time.deltaTime;
                if (newYDown <= strikeStartY)
                {
                    newYDown = strikeStartY;
                    strikePhase = StrikePhase.Idle;
                }
                return newYDown;

            default:
                return rb.position.y;
        }
    }

    private bool TryTouchMove(float targetY)
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
        float diff = targetX - rb.position.x;

        // Stop if close enough
        if (Mathf.Abs(diff) < 0.01f)
        {
            currentVelocity = 0f;
            ApplyMovement(0f, targetY);
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

        // Clamp movement to not overshoot target (convert world-space diff to local space)
        float maxMove = Mathf.Abs(diff) / stageScale;
        float actualMove = Mathf.Clamp(currentVelocity * Time.deltaTime, -maxMove, maxMove);
        ApplyMovement(actualMove, targetY);
        return true;
    }

    private void Move(float targetY)
    {
        if (InputController.Instance == null)
        {
            ApplyMovement(0f, targetY);
            return;
        }

        float input = InputController.Instance.GetHorizontalInput();
        float targetVelocity = input * moveSpeed;

        // Accelerate towards target velocity
        float acceleration = moveSpeed / accelerationTime;
        currentVelocity = Mathf.MoveTowards(
            currentVelocity,
            targetVelocity,
            acceleration * Time.deltaTime
        );

        ApplyMovement(currentVelocity * Time.deltaTime, targetY);
    }

    private void ApplyMovement(float deltaX, float targetY)
    {
        // Scale local-space delta to world space
        deltaX *= stageScale;

        // Apply X movement and strike Y in a single position update
        Vector2 targetPos = new(rb.position.x + deltaX, targetY);
        rb.MovePosition(targetPos);

        // Check for wall overlap at new position
        int overlapCount = Physics2D.OverlapCollider(boxCollider, wallFilter, overlapResults);

        if (overlapCount > 0)
        {
            for (int i = 0; i < overlapCount; i++)
            {
                ColliderDistance2D distance = boxCollider.Distance(overlapResults[i]);

                if (distance.isOverlapped)
                {
                    targetPos += distance.normal * distance.distance;
                    currentVelocity = 0f;
                    rb.MovePosition(targetPos);
                }
            }
        }
    }
}
