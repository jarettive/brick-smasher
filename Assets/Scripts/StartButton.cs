using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Button that starts the game when clicked.
/// </summary>
[RequireComponent(typeof(Button))]
public class StartButton : MonoBehaviour
{
    [SerializeField]
    private Difficulty difficulty;

    [SerializeField]
    [Tooltip("Seconds to wait after the button is pressed before the game starts")]
    private float startDelay = 0.1f;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        StartCoroutine(StartAfterDelay());
    }

    private IEnumerator StartAfterDelay()
    {
        if (startDelay > 0f)
            yield return new WaitForSecondsRealtime(startDelay);

        GameManager.Instance.SetDifficulty(difficulty);
        GameManager.Instance.StartGame();
        gameObject.SetActive(false);
    }
}
