using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NeutralBallBehavior",
    menuName = "Brick Smasher/Behaviors/Neutral Ball"
)]
public class NeutralBallBehavior : BallBehavior
{
    [SerializeField]
    private float pauseDuration = 0.15f;

    [SerializeField]
    private float smashSpeed = 20f;

    [SerializeField]
    private float accelerationTime = 0.1f;

    [SerializeField]
    [Tooltip("Fraction of horizontal velocity retained during smash (0-1)")]
    private float horizontalRetention = 0.25f;

    [SerializeField]
    [Tooltip("Minimum horizontal speed during smash to prevent straight-down slams")]
    private float minHorizontalSpeed = 1.5f;

    public override void Smash(Ball ball)
    {
        ball.StartCoroutine(SmashRoutine(ball));
    }

    private bool HasNonWallCollision(Ball ball)
    {
        var collisions = ball.FrameCollisions;
        if (collisions.Count == 0)
            return false;

        int wallLayer = LayerMask.NameToLayer(Layers.BallWall);
        for (int i = 0; i < collisions.Count; i++)
        {
            if (collisions[i].layer != wallLayer)
                return true;
        }

        return false;
    }

    private float GetSmashHorizontal(float preSmashVx)
    {
        float hx = preSmashVx * horizontalRetention;

        if (Mathf.Abs(hx) < minHorizontalSpeed)
        {
            float sign = hx < 0f ? -1f : 1f;
            // If pre-smash horizontal was near zero, pick a random direction
            if (Mathf.Abs(preSmashVx) < 0.01f)
                sign = Random.value < 0.5f ? -1f : 1f;
            hx = minHorizontalSpeed * sign;
        }

        return hx;
    }

    private IEnumerator SmashRoutine(Ball ball)
    {
        float preSmashVx = ball.Velocity.x;
        float hx = GetSmashHorizontal(preSmashVx);

        ball.EnforceMinSpeed = false;

        // Pause
        ball.Velocity = Vector2.zero;
        float pauseElapsed = 0f;
        while (pauseElapsed < pauseDuration)
        {
            pauseElapsed += Time.deltaTime;
            yield return null;

            if (HasNonWallCollision(ball))
            {
                ball.EnforceMinSpeed = true;
                yield break;
            }
        }

        // Accelerate downward with retained horizontal movement
        float elapsed = 0f;
        while (elapsed < accelerationTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / accelerationTime;
            float vy = Mathf.Lerp(0f, smashSpeed, t);
            ball.Velocity = new Vector2(hx, -vy);
            yield return null;

            if (HasNonWallCollision(ball))
            {
                ball.EnforceMinSpeed = true;
                yield break;
            }
        }

        ball.Velocity = new Vector2(hx, -smashSpeed);
        ball.EnforceMinSpeed = true;
    }
}
