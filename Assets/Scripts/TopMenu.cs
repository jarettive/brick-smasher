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

    private void OnEnable()
    {
        ScoringSystem.OnScoreChanged += UpdateDisplay;
        UpdateDisplay(ScoringSystem.Instance);
    }

    private void OnDisable()
    {
        ScoringSystem.OnScoreChanged -= UpdateDisplay;
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
    }
}
