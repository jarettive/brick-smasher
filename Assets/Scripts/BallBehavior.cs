using UnityEngine;

/// <summary>
/// Base class for custom ball behaviors.
/// Override methods to add unique functionality to different ball types.
/// </summary>
public abstract class BallBehavior : ScriptableObject
{
    /// <summary>
    /// Called every frame while the ball is active.
    /// </summary>
    public virtual void OnUpdate(Ball ball) { }

    /// <summary>
    /// Called when the Smash button is pressed.
    /// </summary>
    public virtual void Smash(Ball ball) { }
}
