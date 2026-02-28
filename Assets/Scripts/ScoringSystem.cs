using System;
using UnityEngine;

/// <summary>
/// Tracks score based on brick knockouts.
/// Place in scene to enable scoring.
/// </summary>
public class ScoringSystem : MonoBehaviour
{
    private const string HighScorePrefix = "HighScore_";
    private const string BestTimePrefix = "BestTime_";

    [SerializeField]
    private int brickBasePoints = 100;

    [SerializeField]
    private int ballLostPenalty = 150;

    [SerializeField]
    private float pointsPerVelocity = 2f;

    [Header("Time Bonus")]
    [SerializeField]
    [Tooltip("Points awarded per second remaining")]
    private float timeBonusPerSecond = 10f;

    [SerializeField]
    [Tooltip("Seconds after which no time bonus is awarded")]
    private float timeBonusCutoff = 120f;

    public static ScoringSystem Instance { get; private set; }

    private int score;
    private int timeBonus;
    private int highScore;
    private float bestTime;
    private int brickCount;
    private float startTime;
    private float finalTime;
    private bool started;
    private bool ended;

    public int Score => score;
    public int TimeBonus => timeBonus;
    public float ElapsedTime =>
        ended ? finalTime
        : started ? Time.time - startTime
        : 0f;
    public int HighScore => highScore;
    public float BestTime => bestTime;
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
        GameManager.OnGameStarted += HandleGameStarted;
        Brick.OnBrickSpawned += HandleBrickSpawned;
        Brick.OnBrickKnockout += HandleBrickKnockout;
        Ball.OnBallLost += HandleBallLost;
        OnScoreChanged?.Invoke(this);
    }

    private void OnDisable()
    {
        GameManager.OnGameStarted -= HandleGameStarted;
        Brick.OnBrickSpawned -= HandleBrickSpawned;
        Brick.OnBrickKnockout -= HandleBrickKnockout;
        Ball.OnBallLost -= HandleBallLost;
    }

    private void HandleGameStarted()
    {
        started = true;
        startTime = Time.time;
        LoadSavedScores();
    }

    private string DifficultyKey =>
        GameManager.Difficulty != null ? GameManager.Difficulty.name : "default";

    private void LoadSavedScores()
    {
        highScore = PlayerPrefs.GetInt(HighScorePrefix + DifficultyKey, 0);
        bestTime = PlayerPrefs.GetFloat(BestTimePrefix + DifficultyKey, 0f);
        OnScoreChanged?.Invoke(this);
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
            finalTime = Time.time - startTime;
            ended = true;
            AwardTimeBonus();
            CheckHighScore();
            CheckBestTime();
            OnScoreChanged?.Invoke(this);
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

    private void AwardTimeBonus()
    {
        float secondsRemaining = timeBonusCutoff - ElapsedTime;

        if (secondsRemaining <= 0f)
        {
            timeBonus = 0;
        }
        else
        {
            timeBonus = Mathf.RoundToInt(timeBonusPerSecond * secondsRemaining);
        }

        score += timeBonus;
    }

    private void CheckBestTime()
    {
        float elapsed = ElapsedTime;
        if (bestTime <= 0f || elapsed < bestTime)
        {
            bestTime = elapsed;
            PlayerPrefs.SetFloat(BestTimePrefix + DifficultyKey, bestTime);
            PlayerPrefs.Save();
        }
    }

    private void CheckHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HighScorePrefix + DifficultyKey, highScore);
            PlayerPrefs.Save();
        }
    }

    public void ResetScore()
    {
        score = 0;
        timeBonus = 0;
        brickCount = 0;
        started = false;
        ended = false;
        startTime = 0f;
        finalTime = 0f;
        OnScoreChanged?.Invoke(this);
    }
}
