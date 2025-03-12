using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages player input and triggers corresponding events based on actions like moving, jumping, interacting, etc.
/// </summary>
public class PlayerInput : MonoBehaviour
{
    private static PlayerInputActions _playerInputActions;
    public static PlayerInput Instance { get; private set; }

    // Event delegates
    public delegate void SimpleButtonEventDelegate();

    // Player input events
    public event SimpleButtonEventDelegate OnJump;
    public event SimpleButtonEventDelegate OnInteract;
    public event SimpleButtonEventDelegate OnReload;
    public event SimpleButtonEventDelegate OnShootStarted;
    public event SimpleButtonEventDelegate OnShootCanceled;
    public event SimpleButtonEventDelegate OnSprintStarted;
    public event SimpleButtonEventDelegate OnSprintCanceled;
    public event SimpleButtonEventDelegate OnCrouchStarted;
    public event SimpleButtonEventDelegate OnCrouchCanceled;

    // Input values
    public Vector2 MovementInput { get; private set; }
    public Vector2 CameraMovementInput { get; private set; }

    #region Unity Callbacks
    private void Awake()
    {
        if (Instance)
        {
            Debug.LogError($"More than one {name} could not exist");
            Destroy(this);
        }

        Instance = this;
        _playerInputActions = new PlayerInputActions();

        SetUpInputActions();
    }

    /// <summary>
    /// Registers the input action callbacks to trigger corresponding events.
    /// </summary>
    private void SetUpInputActions()
    {
        // Movement and camera input
        _playerInputActions.Player.Move.performed += SetMovementInput;
        _playerInputActions.Player.Move.canceled += SetMovementInput;
        _playerInputActions.Player.CameraMovement.performed += SetCameraInput;
        _playerInputActions.Player.CameraMovement.canceled += SetCameraInput;

        // Button inputs to trigger events
        _playerInputActions.Player.Jump.started += _ => OnJump?.Invoke();
        _playerInputActions.Player.Interact.started += _ => OnInteract?.Invoke();
        _playerInputActions.Player.Reload.started += _ => OnReload?.Invoke();
        _playerInputActions.Player.Shoot.started += _ => OnShootStarted?.Invoke();
        _playerInputActions.Player.Shoot.canceled += _ => OnShootCanceled?.Invoke();
        _playerInputActions.Player.Sprint.started += _ => OnSprintStarted?.Invoke();
        _playerInputActions.Player.Sprint.canceled += _ => OnSprintCanceled?.Invoke();
        _playerInputActions.Player.Crouch.started += _ => OnCrouchStarted?.Invoke();
        _playerInputActions.Player.Crouch.canceled += _ => OnCrouchCanceled?.Invoke();
    }
    
    private void OnEnable()
    {
        EnableInputActions();
    }
    
    private void OnDisable()
    {
        DisableInputActions();
    }

    #endregion

    #region Input Setters
    private void SetMovementInput(InputAction.CallbackContext context)
    {
        MovementInput = context.ReadValue<Vector2>();
    }
    
    private void SetCameraInput(InputAction.CallbackContext context)
    {
        CameraMovementInput = context.ReadValue<Vector2>();
    }

    #endregion

    #region Input Action Management
    
    private void EnableInputActions()
    {
        _playerInputActions.Player.Move.Enable();
        _playerInputActions.Player.Jump.Enable();
        _playerInputActions.Player.Shoot.Enable();
        _playerInputActions.Player.Reload.Enable();
        _playerInputActions.Player.Interact.Enable();
        _playerInputActions.Player.CameraMovement.Enable();
        _playerInputActions.Player.Sprint.Enable();
        _playerInputActions.Player.Crouch.Enable();
    }
    
    private void DisableInputActions()
    {
        _playerInputActions.Player.Move.Disable();
        _playerInputActions.Player.Jump.Disable();
        _playerInputActions.Player.Shoot.Disable();
        _playerInputActions.Player.Reload.Disable();
        _playerInputActions.Player.Interact.Disable();
        _playerInputActions.Player.CameraMovement.Disable();
        _playerInputActions.Player.Sprint.Disable();
        _playerInputActions.Player.Crouch.Disable();
    }

    #endregion
}
