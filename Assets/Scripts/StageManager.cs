using System;
using UnityEngine;

/// <summary>
/// Swaps stage prefabs in and out of a container.
/// </summary>
public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [SerializeField]
    private Transform stageContainer;

    [SerializeField]
    private Stage initialStagePrefab;

    public Stage ActiveStagePrefab { get; private set; }

    public static event Action<Stage> OnActiveStageChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (initialStagePrefab != null)
            LoadStage(initialStagePrefab);
    }

    public void LoadStage(Stage stagePrefab)
    {
        if (stageContainer == null || stagePrefab == null)
            return;

        if (stagePrefab == ActiveStagePrefab)
            return;

        for (int i = stageContainer.childCount - 1; i >= 0; i--)
            Destroy(stageContainer.GetChild(i).gameObject);

        Debug.Log($"Loading stage: {stagePrefab.name}");
        Instantiate(stagePrefab, stageContainer);
        ActiveStagePrefab = stagePrefab;
        OnActiveStageChanged?.Invoke(stagePrefab);
    }
}
