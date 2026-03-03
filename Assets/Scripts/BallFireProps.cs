using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fire ball behavior that shoots projectiles at intervals.
/// Smash launches a more powerful but short-lasting projectile.
/// </summary>
[CreateAssetMenu(fileName = "BallFireProps", menuName = "Brick Smasher/Balls/Fire")]
public class BallFireProps : BallProps
{
    [Header("Auto Fire")]
    [SerializeField]
    private Projectile projectilePrefab;

    [SerializeField]
    private float fireInterval = 1f;

    [SerializeField]
    private float projectileDamage = 10f;

    [SerializeField]
    private float projectileBaseKnockback = 20f;

    [SerializeField]
    private float projectileKnockbackScaling = 0.1f;

    [SerializeField]
    private float projectileLifetime = 5f;

    [SerializeField]
    private float projectileSpeed = 12f;

    [Header("Smash Fire")]
    [SerializeField]
    private float smashDamage = 35f;

    [SerializeField]
    private float smashBaseKnockback = 20f;

    [SerializeField]
    private float smashKnockbackScaling = 1f;

    [SerializeField]
    private float smashLifetime = 0.5f;

    [SerializeField]
    private float smashSpeed = 25f;

    [SerializeField]
    private float smashScale = 4f;

    // Track fire time per ball instance (ScriptableObjects share state)
    private readonly Dictionary<Ball, float> lastFireTimes = new();

    public override void OnUpdate(Ball ball)
    {
        if (projectilePrefab == null)
            return;

        if (!lastFireTimes.TryGetValue(ball, out float lastFireTime))
        {
            lastFireTime = float.NegativeInfinity;
        }

        if (Time.time - lastFireTime >= fireInterval)
        {
            lastFireTimes[ball] = Time.time;
            var attack = new Attack
            {
                Damage = projectileDamage,
                BaseKnockback = projectileBaseKnockback,
                KnockbackScaling = projectileKnockbackScaling,
            };
            FireProjectile(ball, attack, projectileLifetime, projectileSpeed, 1f);
        }
    }

    public override void Smash(Ball ball)
    {
        if (projectilePrefab == null)
            return;

        var attack = new Attack
        {
            Damage = smashDamage,
            BaseKnockback = smashBaseKnockback,
            KnockbackScaling = smashKnockbackScaling,
        };
        FireProjectile(ball, attack, smashLifetime, smashSpeed, smashScale);
    }

    private void FireProjectile(Ball ball, Attack attack, float lifetime, float speed, float scale)
    {
        Vector2 direction = ball.transform.up;

        Projectile projectile = Instantiate(
            projectilePrefab,
            ball.transform.position,
            Quaternion.identity,
            ball.transform.parent
        );

        projectile.transform.localScale = Vector3.one * scale;
        projectile.Lifetime = lifetime;
        projectile.Speed = speed;
        projectile.OnBrickCollision = (proj, brick) =>
        {
            attack.Direction = proj.Direction;
            brick.ReceiveAttack(attack);
        };
        projectile.Launch(direction);
    }
}
