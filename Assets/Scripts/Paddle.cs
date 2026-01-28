using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player-controlled paddle movement.
/// Add a PaddleSurface component for bounce behavior.
/// </summary>
public class Paddle : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 10f;

    [SerializeField]
    private float accelerationTime = 10f / 60f;

    [SerializeField]
    private RectTransform areaRect;

    private float currentVelocity;
    private float paddleHalfWidth;
    private Camera mainCamera;

    private InputAction pointerPositionAction;
    private InputAction pointerPressAction;

    private void Awake()
    {
        pointerPositionAction = new InputAction("PointerPosition", binding: "<Pointer>/position");
        pointerPressAction = new InputAction("PointerPress", binding: "<Pointer>/press");
    }

    private void Start()
    {
        paddleHalfWidth = GetPaddleWidth() / 2f;
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
        if (areaRect == null || mainCamera == null)
            return false;

        // Check for pointer press (unified touch/mouse)
        if (!pointerPressAction.IsPressed())
            return false;

        Vector2 inputPosition = pointerPositionAction.ReadValue<Vector2>();

        // Check if touch is inside the paddle area
        if (!RectTransformUtility.RectangleContainsScreenPoint(areaRect, inputPosition, mainCamera))
            return false;

        // Convert screen position to world position
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(inputPosition.x, inputPosition.y, mainCamera.nearClipPlane)
        );

        float targetX = worldPos.x;

        // Clamp target to area bounds
        Rect bounds = areaRect.rect;
        Vector3 areaCenter = areaRect.transform.position;
        float leftBound = areaCenter.x + bounds.xMin + paddleHalfWidth;
        float rightBound = areaCenter.x + bounds.xMax - paddleHalfWidth;
        targetX = Mathf.Clamp(targetX, leftBound, rightBound);

        // Calculate direction to target
        float diff = targetX - transform.position.x;
        float targetVelocity = Mathf.Sign(diff) * moveSpeed;

        // Stop if close enough
        if (Mathf.Abs(diff) < 0.01f)
        {
            currentVelocity = 0f;
            return true;
        }

        // Accelerate towards target velocity
        float acceleration = moveSpeed / accelerationTime;
        currentVelocity = Mathf.MoveTowards(
            currentVelocity,
            targetVelocity,
            acceleration * Time.deltaTime
        );

        float newX = transform.position.x + currentVelocity * Time.deltaTime;

        // Don't overshoot target
        if ((currentVelocity > 0 && newX > targetX) || (currentVelocity < 0 && newX < targetX))
        {
            newX = targetX;
            currentVelocity = 0f;
        }

        // Clamp to bounds and stop velocity if hitting them
        newX = Mathf.Clamp(newX, leftBound, rightBound);
        if (newX == leftBound || newX == rightBound)
        {
            currentVelocity = 0f;
        }

        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
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

        float newX = transform.position.x + currentVelocity * Time.deltaTime;

        // Clamp to area bounds if assigned
        if (areaRect != null)
        {
            Rect bounds = areaRect.rect;
            Vector3 areaCenter = areaRect.transform.position;
            float leftBound = areaCenter.x + bounds.xMin + paddleHalfWidth;
            float rightBound = areaCenter.x + bounds.xMax - paddleHalfWidth;
            newX = Mathf.Clamp(newX, leftBound, rightBound);

            // Stop velocity if hitting bounds
            if (newX == leftBound || newX == rightBound)
            {
                currentVelocity = 0f;
            }
        }

        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    private float GetPaddleWidth()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            return boxCollider.size.x * transform.localScale.x;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            return spriteRenderer.bounds.size.x;
        }

        return 2f;
    }
}
