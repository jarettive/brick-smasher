using System;
using UnityEngine;

/// <summary>
/// Tracks score based on brick knockouts.
/// Place in scene to enable scoring.
/// </summary>
public class ScoringSystem : MonoBehaviour
{
    private const string HighScoreKey = "HighScore";

    [SerializeField]
    private int brickBasePoints = 100;

    [SerializeField]
    private int ballLostPenalty = 150;

    [SerializeField]
    private float pointsPerVelocity = 2f;

    public static ScoringSystem Instance { get; private set; }

    private int score;
    private int highScore;
    private int brickCount;

    public int Score => score;
    public int HighScore => highScore;
    public int BrickCount => brickCount;
    public bool IsGameWon => brickCount == 0;

    /// <summary>
    /// Fired when the score changes.
    /// </summary>
    public static event Action<ScoringSystem> OnScoreChanged;

    /// <summary>
    /// Fired when all bricks are knocked out.
    /// </summary>
    public static event Action OnGameWon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        Brick.OnBrickSpawned += HandleBrickSpawned;
        Brick.OnBrickKnockout += HandleBrickKnockout;
        Ball.OnBallLost += HandleBallLost;
        OnScoreChanged?.Invoke(this);
    }

    private void OnDisable()
    {
        Brick.OnBrickSpawned -= HandleBrickSpawned;
        Brick.OnBrickKnockout -= HandleBrickKnockout;
        Ball.OnBallLost -= HandleBallLost;
    }

    private void HandleBrickSpawned()
    {
        brickCount++;
    }

    private void HandleBrickKnockout(float rigidity, Vector2 knockbackVelocity)
    {
        brickCount--;

        // Rigidity multiplier: 100% at 10, 150% at 20
        float rigidityMultiplier = 1f + (rigidity - 10f) / 20f;

        float velocityMagnitude = knockbackVelocity.magnitude;
        int points = Mathf.RoundToInt(
            (brickBasePoints * rigidityMultiplier) + (pointsPerVelocity * velocityMagnitude)
        );

        score += points;
        CheckHighScore();
        OnScoreChanged?.Invoke(this);

        if (brickCount == 0)
        {
            OnGameWon?.Invoke();
        }
    }

    private void HandleBallLost()
    {
        if (IsGameWon)
            return;

        score -= ballLostPenalty;
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
        brickCount = 0;
        OnScoreChanged?.Invoke(this);
    }
}
