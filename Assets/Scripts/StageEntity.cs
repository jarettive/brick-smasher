using UnityEngine;

/// <summary>
/// Base class for objects whose movement scales with the stage.
/// Caches the stage scale on Start for use in movement calculations.
/// </summary>
public abstract class StageEntity : MonoBehaviour
{
    protected float stageScale;

    protected virtual void Start()
    {
        stageScale = Stage.Scale;
    }
}
