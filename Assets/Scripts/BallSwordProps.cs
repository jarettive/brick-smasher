using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BallSwordProps", menuName = "Brick Smasher/Balls/Sword")]
public class BallSwordProps : BallProps
{
    [Header("Sword")]
    [SerializeField]
    private Sword swordPrefab;

    [Header("Auto Attack")]
    [SerializeField]
    private float swingInterval = 1.5f;

    [SerializeField]
    [Tooltip("Delay before the very first swing. Subsequent swings use swingInterval.")]
    private float firstSwingInterval = 0.5f;

    [SerializeField]
    private float swingDamage = 15f;

    [SerializeField]
    private float swingBaseKnockback = 5f;

    [SerializeField]
    private float swingKnockbackScaling = 0.5f;

    [SerializeField]
    private float swingDuration = 0.3f;

    [SerializeField]
    private float swingArc = 120f;

    [SerializeField]
    [Tooltip("Pause before the swing starts")]
    private float swingWindUp = 0.1f;

    [SerializeField]
    [Tooltip("Pause after the swing ends")]
    private float swingWindDown = 0.1f;

    [Header("Smash Attack")]
    [SerializeField]
    private float smashDamage = 25f;

    [SerializeField]
    private float smashBaseKnockback = 15f;

    [SerializeField]
    private float smashKnockbackScaling = 1f;

    [SerializeField]
    private float smashDuration = 2f;

    [SerializeField]
    private float smashRevolutions = 1f;

    [SerializeField]
    [Tooltip("Ball movement is scaled by this value during a Smash (e.g. 0.1667 = 1/6 speed)")]
    private float smashSpeedMultiplier = 1f / 6f;

    private class SwordState
    {
        public float lastSwingTime;
        public bool hasSwung;
        public bool isSmashing;
        public Sword activeSword;
        public Coroutine activeRoutine;
    }

    private readonly Dictionary<Ball, SwordState> states = new();

    private SwordState GetState(Ball ball)
    {
        if (!states.TryGetValue(ball, out var state))
        {
            state = new SwordState { lastSwingTime = Time.time };
            states[ball] = state;
        }
        return state;
    }

    public override void OnUpdate(Ball ball)
    {
        if (swordPrefab == null)
            return;

        var state = GetState(ball);

        if (state.isSmashing)
            return;

        float interval = state.hasSwung ? swingInterval : firstSwingInterval;
        if (Time.time - state.lastSwingTime < interval)
            return;

        Brick nearest = FindNearestBrick(ball.transform.position);
        if (nearest == null)
            return;

        state.lastSwingTime = Time.time;
        state.hasSwung = true;

        Vector2 toBrick = (Vector2)nearest.transform.position - (Vector2)ball.transform.position;
        float dirAngle = Mathf.Atan2(toBrick.y, toBrick.x) * Mathf.Rad2Deg - 90f;

        var attack = new Attack
        {
            Damage = swingDamage,
            BaseKnockback = swingBaseKnockback,
            KnockbackScaling = swingKnockbackScaling,
        };

        float startAngle = dirAngle + swingArc / 2f;
        float endAngle = dirAngle - swingArc / 2f;

        state.activeRoutine = ball.StartCoroutine(
            SwingRoutine(ball, state, attack, startAngle, endAngle, swingDuration)
        );
    }

    public override void Smash(Ball ball)
    {
        if (swordPrefab == null)
            return;

        var state = GetState(ball);

        // Interrupt auto-attack
        if (state.activeRoutine != null)
        {
            ball.StopCoroutine(state.activeRoutine);
            state.activeRoutine = null;
        }
        if (state.activeSword != null)
        {
            Destroy(state.activeSword.gameObject);
            state.activeSword = null;
        }

        state.isSmashing = true;

        var attack = new Attack
        {
            Damage = smashDamage,
            BaseKnockback = smashBaseKnockback,
            KnockbackScaling = smashKnockbackScaling,
        };

        state.activeRoutine = ball.StartCoroutine(SmashRoutine(ball, state, attack));
    }

    private IEnumerator SwingRoutine(
        Ball ball,
        SwordState state,
        Attack attack,
        float startAngle,
        float endAngle,
        float duration
    )
    {
        Sword sword = SpawnSword(ball, attack, startAngle);
        state.activeSword = sword;

        // Wind-up: sword visible but stationary
        if (swingWindUp > 0f)
            yield return new WaitForSeconds(swingWindUp);

        // Swing with ease-in-out
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (sword == null)
                break;

            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            sword.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        // Wind-down: sword visible at end position
        if (sword != null && swingWindDown > 0f)
            yield return new WaitForSeconds(swingWindDown);

        if (sword != null)
            Destroy(sword.gameObject);

        state.activeSword = null;
        state.activeRoutine = null;
    }

    private IEnumerator SmashRoutine(Ball ball, SwordState state, Attack attack)
    {
        ball.MoveSpeedMultiplier = smashSpeedMultiplier;

        Sword sword = SpawnSword(ball, attack, 0f);
        state.activeSword = sword;

        // Wind-up: sword visible but stationary
        if (swingWindUp > 0f)
            yield return new WaitForSeconds(swingWindUp);

        // Spin with ease-out (starts fast, decelerates)
        float elapsed = 0f;
        while (elapsed < smashDuration)
        {
            if (sword == null)
                break;

            elapsed += Time.deltaTime;
            float linear = Mathf.Clamp01(elapsed / smashDuration);
            float t = 1f - (1f - linear) * (1f - linear);
            float angle = Mathf.Lerp(0f, 360f * smashRevolutions, t);
            sword.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        // Wind-down: sword visible at end position
        if (sword != null && swingWindDown > 0f)
            yield return new WaitForSeconds(swingWindDown);

        if (sword != null)
            Destroy(sword.gameObject);

        state.activeSword = null;
        state.activeRoutine = null;
        state.isSmashing = false;
        ball.MoveSpeedMultiplier = 1f;
    }

    private Sword SpawnSword(Ball ball, Attack attack, float initialAngle)
    {
        Sword sword = Instantiate(swordPrefab, ball.transform);
        sword.transform.localPosition = Vector3.zero;
        sword.transform.rotation = Quaternion.Euler(0f, 0f, initialAngle);

        sword.OnBrickCollision = (s, brick) =>
        {
            Vector2 dir = (
                (Vector2)brick.transform.position - (Vector2)ball.transform.position
            ).normalized;
            attack.Direction = dir;
            brick.ReceiveAttack(attack);
        };

        return sword;
    }

    private static Brick FindNearestBrick(Vector3 position)
    {
        Brick[] bricks = FindObjectsByType<Brick>(FindObjectsSortMode.None);
        Brick nearest = null;
        float nearestDist = float.MaxValue;

        for (int i = 0; i < bricks.Length; i++)
        {
            float dist = Vector2.Distance(position, bricks[i].transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = bricks[i];
            }
        }

        return nearest;
    }
}
