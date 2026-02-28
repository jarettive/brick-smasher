using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Button that reloads the current scene when clicked.
/// </summary>
[RequireComponent(typeof(Button))]
public class RetryButton : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        GameManager.Instance?.RestartGame();
    }
}
