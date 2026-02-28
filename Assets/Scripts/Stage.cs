using UnityEngine;

/// <summary>
/// Singleton component attached to the Stage object.
/// Provides centralized access to the stage scale factor.
/// </summary>
public class Stage : MonoBehaviour
{
    public static Stage Instance { get; private set; }

    /// <summary>
    /// The stage's local scale factor (uniform).
    /// Falls back to 1 if no Stage exists.
    /// </summary>
    public static float Scale => Instance != null ? Instance.transform.localScale.x : 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
