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

    public override void Smash(Ball ball)
    {
        ball.StartCoroutine(SmashRoutine(ball));
    }

    private IEnumerator SmashRoutine(Ball ball)
    {
        ball.EnforceMinSpeed = false;
        ball.CheckAndClearCollision(); // Clear any existing collision flag

        // Pause
        ball.Velocity = Vector2.zero;
        float pauseElapsed = 0f;
        while (pauseElapsed < pauseDuration)
        {
            pauseElapsed += Time.deltaTime;
            yield return null;

            if (ball.CheckAndClearCollision())
            {
                ball.EnforceMinSpeed = true;
                yield break;
            }
        }

        // Accelerate downward
        float elapsed = 0f;
        while (elapsed < accelerationTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / accelerationTime;
            ball.Velocity = Vector2.down * Mathf.Lerp(0f, smashSpeed, t);
            yield return null;

            if (ball.CheckAndClearCollision())
            {
                ball.EnforceMinSpeed = true;
                yield break;
            }
        }

        ball.Velocity = Vector2.down * smashSpeed;
        ball.EnforceMinSpeed = true;
    }
}
