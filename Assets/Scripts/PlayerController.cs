using System;
using UnityEngine;

/// <summary>
/// Manages player actions with cooldowns and broadcasts events.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static event Action OnSmashTriggered;

    [SerializeField]
    private float smashCooldown = 5f;

    private float lastSmashTime = float.NegativeInfinity;

    public float SmashCooldown => smashCooldown;
    public float TimeSinceLastSmash => Time.time - lastSmashTime;
    public float SmashCooldownRemaining => Mathf.Max(0f, smashCooldown - TimeSinceLastSmash);
    public bool IsSmashOnCooldown => TimeSinceLastSmash < smashCooldown;

    private void Update()
    {
        if (InputController.Instance == null)
            return;

        if (InputController.Instance.IsSmashTriggered())
        {
            TrySmash();
        }
    }

    private void TrySmash()
    {
        if (Time.time - lastSmashTime < smashCooldown)
            return;

        lastSmashTime = Time.time;
        OnSmashTriggered?.Invoke();
    }
}
