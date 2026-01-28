using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Button that starts the game when clicked.
/// Sets itself inactive and resumes game time.
/// </summary>
[RequireComponent(typeof(Button))]
public class StartButton : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}
