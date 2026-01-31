using System;
using UnityEngine;

/// <summary>
/// Singleton that tracks score based on brick knockouts.
/// Auto-initializes on game start.
/// </summary>
public class ScoringSystem
{
    private const string HighScoreKey = "HighScore";
    private const int BrickBasePoints = 100;
    private const int BallLostPenalty = 150;
    private const float pointsPerVelocity = 2f;

    public static ScoringSystem Instance { get; private set; }

    private int score;
    private int highScore;

    public int Score => score;
    public int HighScore => highScore;

    /// <summary>
    /// Fired when the score changes.
    /// </summary>
    public static event Action<ScoringSystem> OnScoreChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Instance = new ScoringSystem();
    }

    private ScoringSystem()
    {
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        Brick.OnBrickKnockout += HandleBrickKnockout;
        Ball.OnBallLost += HandleBallLost;
        OnScoreChanged?.Invoke(this);
    }

    private void HandleBrickKnockout(float rigidity, Vector2 knockbackVelocity)
    {
        // Rigidity multiplier: 100% at 10, 150% at 20
        float rigidityMultiplier = 1f + (rigidity - 10f) / 20f;

        float velocityMagnitude = knockbackVelocity.magnitude;
        int points = Mathf.RoundToInt(
            (BrickBasePoints * rigidityMultiplier) + (pointsPerVelocity * velocityMagnitude)
        );

        score += points;
        CheckHighScore();
        OnScoreChanged?.Invoke(this);
    }

    private void HandleBallLost()
    {
        score -= BallLostPenalty;
        OnScoreChanged?.Invoke(this);
    }

    private void CheckHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }
    }

    public void ResetScore()
    {
        score = 0;
        OnScoreChanged?.Invoke(this);
    }
}
