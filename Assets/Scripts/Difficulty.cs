using UnityEngine;

[CreateAssetMenu(fileName = "Difficulty", menuName = "Brick Smasher/Difficulty")]
public class Difficulty : ScriptableObject
{
    [SerializeField]
    [Tooltip("Multiplier for ball movement speed. 1.0 = normal, less than 1.0 = easier.")]
    private float gameSpeed = 1f;

    public float GameSpeed => gameSpeed;
}
