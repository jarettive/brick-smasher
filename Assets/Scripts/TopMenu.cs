using TMPro;
using UnityEngine;

/// <summary>
/// Displays score and high score in the top menu.
/// </summary>
public class TopMenu : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI scoreText;

    [SerializeField]
    private TextMeshProUGUI highScoreText;

    [SerializeField]
    private TextMeshProUGUI timeText;

    [SerializeField]
    private TextMeshProUGUI bestTimeText;

    private void Start()
    {
        UpdateDisplay(ScoringSystem.Instance);
    }

    private void OnEnable()
    {
        ScoringSystem.OnScoreChanged += UpdateDisplay;
    }

    private void OnDisable()
    {
        ScoringSystem.OnScoreChanged -= UpdateDisplay;
    }

    private void Update()
    {
        if (timeText != null && ScoringSystem.Instance != null)
        {
            timeText.text = "Time: " + FormatTime(ScoringSystem.Instance.ElapsedTime);
        }
    }

    private void UpdateDisplay(ScoringSystem scoringSystem)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + scoringSystem.Score.ToString();
        }

        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + scoringSystem.HighScore.ToString();
        }

        if (bestTimeText != null)
        {
            float best = scoringSystem.BestTime;
            bestTimeText.text = best > 0f ? "Best: " + FormatTime(best) : "Best: -:--.-";
        }
    }

    private static string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60f);
        int secs = (int)(seconds % 60f);
        int tenths = (int)((seconds * 10f) % 10f);
        return $"{minutes}:{secs:D2}.{tenths}";
    }
}
