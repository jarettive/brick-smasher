using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Button that starts the game when clicked.
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
        GameManager.Instance.StartGame();
        gameObject.SetActive(false);
    }
}
