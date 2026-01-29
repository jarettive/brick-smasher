using UnityEngine;

/// <summary>
/// Behavior script for knockout visual effect when bricks are destroyed.
/// </summary>
public class KnockoutVFX : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem particleSystem1;

    [SerializeField]
    private ParticleSystem particleSystem2;

    private void Awake()
    {
        Destroy(gameObject, 3f);
    }

    /// <summary>
    /// Initialize the VFX with position and travel angle.
    /// </summary>
    /// <param name="position">World position where the brick was destroyed</param>
    /// <param name="angle">Angle in degrees the brick was travelling when destroyed</param>
    public void Initialize(Vector3 position, float angle)
    {
        transform.position = position;

        SetParticleRotation(particleSystem1, angle);
        SetParticleRotation(particleSystem2, angle);
    }

    private void SetParticleRotation(ParticleSystem ps, float angle)
    {
        if (ps == null)
            return;

        // Convert input angle to particle rotation: input 90 -> -180, input 180 -> 90
        float adjustedAngle = -angle - 90f;

        var main = ps.main;
        main.startRotation = adjustedAngle * Mathf.Deg2Rad;

        var shape = ps.shape;
        shape.rotation = new Vector3(shape.rotation.x, shape.rotation.y, adjustedAngle);
    }
}
