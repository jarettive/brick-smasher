using UnityEngine;

public class StartingMenu : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.OnGameStarted += Hide;
    }

    private void OnDisable()
    {
        GameManager.OnGameStarted -= Hide;
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
