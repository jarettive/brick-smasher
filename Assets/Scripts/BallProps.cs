using UnityEngine;

public class BallProps : ScriptableObject
{
    [SerializeField]
    private float minSpeed = 5f;

    [SerializeField]
    [Tooltip("Speed decay per second toward minSpeed")]
    private float speedDecay = 5f;

    [SerializeField]
    private Sprite sprite;

    [SerializeField]
    private float collisionDamage = 3.6f;

    [Header("Rotation")]
    [SerializeField]
    [Tooltip("If true, ball rotates at a fixed speed regardless of collisions")]
    private bool fixedRotation;

    [SerializeField]
    [Tooltip("Rotation speed in degrees per second (used when fixedRotation is true)")]
    private float rotationSpeed = 360f;

    public float MinSpeed => minSpeed;
    public float SpeedDecay => speedDecay;
    public Sprite Sprite => sprite;
    public float Damage => collisionDamage;
    public bool FixedRotation => fixedRotation;
    public float RotationSpeed => rotationSpeed;

    public virtual void OnUpdate(Ball ball) { }

    public virtual void Smash(Ball ball) { }
}
