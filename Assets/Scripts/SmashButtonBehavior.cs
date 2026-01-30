using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates a fill image to show the smash cooldown state.
/// </summary>
public class SmashButtonBehavior : MonoBehaviour
{
    [SerializeField]
    private Image fillImage;

    [SerializeField]
    private PlayerController playerController;

    private void Update()
    {
        if (fillImage == null || playerController == null)
            return;

        if (playerController.IsSmashOnCooldown)
        {
            float cooldownProgress =
                playerController.SmashCooldownRemaining / playerController.SmashCooldown;
            fillImage.fillAmount = cooldownProgress;
        }
        else
        {
            fillImage.fillAmount = 0f;
        }
    }
}
