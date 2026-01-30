using UnityEngine;

[CreateAssetMenu(fileName = "BallProps", menuName = "Brick Smasher/Ball Props")]
public class BallProps : ScriptableObject
{
    [SerializeField]
    private float minSpeed = 10f;

    [SerializeField]
    [Tooltip("Speed decay per second toward minSpeed")]
    private float speedDecay = 5f;

    [SerializeField]
    private Sprite sprite;

    [SerializeField]
    private float damage = 10f;

    [SerializeField]
    [Tooltip("Optional custom behavior for this ball type")]
    private BallBehavior behavior;

    public float MinSpeed => minSpeed;
    public float SpeedDecay => speedDecay;
    public Sprite Sprite => sprite;
    public float Damage => damage;
    public BallBehavior Behavior => behavior;
}
