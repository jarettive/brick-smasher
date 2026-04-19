using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main game manager that handles global game settings.
/// Auto-instantiates on game start and persists across scenes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private const float RestartDelay = 1f;

    [SerializeField]
    private Difficulty difficulty;

    public static event Action OnGameStarted;

    public static Difficulty Difficulty => Instance != null ? Instance.difficulty : null;

    public static float GameSpeed =>
        Instance != null && Instance.difficulty != null ? Instance.difficulty.GameSpeed : 1f;

    private int ballCount;
    private Coroutine restartCoroutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null)
        {
            var go = new GameObject("GameManager");
            Instance = go.AddComponent<GameManager>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 60;
        Time.timeScale = 0f;
    }

    private void OnEnable()
    {
        Ball.OnBallSpawned += HandleBallSpawned;
        Ball.OnBallLost += HandleBallLost;
    }

    private void OnDisable()
    {
        Ball.OnBallSpawned -= HandleBallSpawned;
        Ball.OnBallLost -= HandleBallLost;
    }

    private void HandleBallSpawned()
    {
        ballCount++;
        if (restartCoroutine != null)
        {
            StopCoroutine(restartCoroutine);
            restartCoroutine = null;
        }
    }

    private void HandleBallLost()
    {
        ballCount--;

        if (ballCount <= 0)
        {
            restartCoroutine = StartCoroutine(RestartAfterDelay());
        }
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(RestartDelay);

        if (ballCount <= 0)
        {
            RestartGame();
        }
    }

    public void SetDifficulty(Difficulty newDifficulty)
    {
        difficulty = newDifficulty;
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        OnGameStarted?.Invoke();
    }

    public void RestartGame()
    {
        restartCoroutine = null;
        Time.timeScale = 0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
