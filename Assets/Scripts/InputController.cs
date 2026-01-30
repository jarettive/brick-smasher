using UnityEngine;
using UnityEngine.InputSystem;

public enum GameControl
{
    MOVE_LEFT,
    MOVE_RIGHT,
}

/// <summary>
/// Handles player input using Unity's new Input System.
/// </summary>
public class InputController : MonoBehaviour
{
    public static InputController Instance { get; private set; }

    [SerializeField]
    private InputActionReference moveActionRef;

    [SerializeField]
    private InputActionReference strikeActionRef;

    [SerializeField]
    private InputActionReference smashActionRef;

    private InputAction debugSlowAction;
    private InputAction debugSlowerAction;
    private InputAction debugFastAction;

    private KeyCode activeSpeedKey = KeyCode.None;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Debug controls defined in code
        debugSlowAction = new InputAction("DebugSlow", InputActionType.Button);
        debugSlowAction.AddBinding("<Keyboard>/l");
        debugSlowAction.performed += _ => SetDebugSpeed(KeyCode.L, 0.1f);

        debugSlowerAction = new InputAction("DebugSlower", InputActionType.Button);
        debugSlowerAction.AddBinding("<Keyboard>/m");
        debugSlowerAction.performed += _ => SetDebugSpeed(KeyCode.M, 0.05f);

        debugFastAction = new InputAction("DebugFast", InputActionType.Button);
        debugFastAction.AddBinding("<Keyboard>/k");
        debugFastAction.performed += _ => SetDebugSpeed(KeyCode.K, 1.5f);
    }

    private void OnEnable()
    {
        moveActionRef?.action?.Enable();
        strikeActionRef?.action?.Enable();
        smashActionRef?.action?.Enable();
        debugSlowAction?.Enable();
        debugSlowerAction?.Enable();
        debugFastAction?.Enable();
    }

    private void OnDisable()
    {
        moveActionRef?.action?.Disable();
        strikeActionRef?.action?.Disable();
        smashActionRef?.action?.Disable();
        debugSlowAction?.Disable();
        debugSlowerAction?.Disable();
        debugFastAction?.Disable();
    }

    /// <summary>
    /// Returns true if the control is currently held down.
    /// </summary>
    public bool IsControlHeld(GameControl control)
    {
        float value = GetHorizontalInput();
        return control switch
        {
            GameControl.MOVE_LEFT => value < -0.1f,
            GameControl.MOVE_RIGHT => value > 0.1f,
            _ => false,
        };
    }

    /// <summary>
    /// Returns true on the frame the control was pressed.
    /// </summary>
    public bool IsControlPressed(GameControl control)
    {
        // For axis-based input, this is less meaningful
        // Consider using IsControlHeld instead
        return IsControlHeld(control);
    }

    /// <summary>
    /// Gets horizontal input as a value from -1 (left) to 1 (right).
    /// </summary>
    public float GetHorizontalInput()
    {
        if (moveActionRef?.action == null)
            return 0f;

        return moveActionRef.action.ReadValue<float>();
    }

    /// <summary>
    /// Returns true if strike was triggered this frame (via input or button).
    /// </summary>
    public bool IsStrikeTriggered()
    {
        if (strikeActionRef != null && strikeActionRef.action != null)
            return strikeActionRef.action.WasPressedThisFrame();

        return false;
    }

    /// <summary>
    /// Returns true if smash was triggered this frame (via input or button).
    /// </summary>
    public bool IsSmashTriggered()
    {
        if (smashActionRef != null && smashActionRef.action != null)
            return smashActionRef.action.WasPressedThisFrame();

        return false;
    }

    private void SetDebugSpeed(KeyCode key, float speed)
    {
        if (activeSpeedKey == key)
        {
            Time.timeScale = 1f;
            activeSpeedKey = KeyCode.None;
        }
        else
        {
            Time.timeScale = speed;
            activeSpeedKey = key;
        }
    }
}
