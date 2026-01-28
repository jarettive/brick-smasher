using UnityEngine;

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

    private void Start()
    {
        paddleHalfWidth = GetPaddleWidth() / 2f;
    }

    private void Update()
    {
        Move();
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
