using UnityEngine;

/// <summary>
/// Relay trigger events from the blade collider to the parent Sword.
/// </summary>
public class SwordBlade : MonoBehaviour
{
    private Sword sword;

    private void Awake()
    {
        sword = GetComponentInParent<Sword>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Brick>(out var brick))
        {
            sword.HandleBrickCollision(brick);
        }
    }
}
