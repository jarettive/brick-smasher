using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI button that loads its assigned stage prefab via StageManager when clicked,
/// and tints itself when that stage is the active one.
/// </summary>
[RequireComponent(typeof(Button))]
public class StageButton : MonoBehaviour
{
    [SerializeField]
    private Stage stagePrefab;

    [SerializeField]
    private Color activeColor = Color.yellow;

    [SerializeField]
    private Color inactiveColor = Color.white;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(HandleClick);
        StageManager.OnActiveStageChanged += HandleActiveStageChanged;
        RefreshColor();
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(HandleClick);
        StageManager.OnActiveStageChanged -= HandleActiveStageChanged;
    }

    private void HandleClick()
    {
        if (stagePrefab != null && StageManager.Instance != null)
            StageManager.Instance.LoadStage(stagePrefab);
    }

    private void HandleActiveStageChanged(Stage _)
    {
        RefreshColor();
    }

    private void RefreshColor()
    {
        bool isActive =
            StageManager.Instance != null && StageManager.Instance.ActiveStagePrefab == stagePrefab;

        ColorBlock colors = button.colors;
        Color target = isActive ? activeColor : inactiveColor;
        colors.normalColor = target;
        colors.selectedColor = target;
        button.colors = colors;
    }
}
