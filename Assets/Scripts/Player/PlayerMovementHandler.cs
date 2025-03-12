using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles player movement, including walking, jumping, crouching, and sliding mechanics.
/// </summary>
public class PlayerMovementHandler : MonoBehaviour
{
    #region Movement Settings

    [SerializeField] private SpeedLinesAlphaMap _speedLinesAlphaMap;

    [Header("Movement")]
    [Tooltip("How fast player will accelerate")]
    [SerializeField] private float _basicMovementForce = 400;

    [SerializeField] private float _maxMovementForce = 2500;
    [SerializeField] private float _slowBack = 10;

    [Tooltip("How strongly player will jump")]
    [SerializeField] private float _jumpForce = 8f;

    [Tooltip("Layers that should be ignored during ground check")]
    [SerializeField] private LayerMask _notGroundLayers;

    [Space]
    [Header("Crouch")]
    [Tooltip("How input should be handled. Press - press to start crouch, press again uncrouch; Hold = crouch while holding button")]
    [SerializeField] private InputMode _crouchInputMode;

    [Tooltip("How fast player will crouch and un crouch")]
    [SerializeField] private float _crouchSpeed;

    [Tooltip("How much collider y scale should be multiplied")]
    [SerializeField] private float _crouchColliderMultiplier;

    [Tooltip("How much collider y scale should be multiplied")]
    [SerializeField] private float _crouchSpeedMultiplier;

    [Space]
    [Header("Slide")]
    [Tooltip("Sets how fast player will decrease speed while sliding")]
    [SerializeField] private float _slideSlowBack;

    [Tooltip("How much will start speed multiplied")]
    [SerializeField] private float _slideStartSpeedMultiplier;

    #endregion

    private Rigidbody _rigidbody;
    private Vector3 _currentGravityForce;
    private float _movementForce;
    private float _slideSpeedScale;

    private CapsuleCollider _collider;
    private float _colliderBasicHeight;

    private bool _isCrouching;
    private bool _isCanMove;
    private bool _isSliding;

    #region Unity Callbacks

    /// <summary>
    /// Initializes references and sets default values.
    /// </summary>
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();

        _colliderBasicHeight = _collider.height;
        _movementForce = _basicMovementForce;

        _isCanMove = true;
    }

    /// <summary>
    /// Subscribes to input events for jumping and crouching.
    /// </summary>
    private void Start()
    {
        #region Adding Input Event Listeners

        PlayerInput.Instance.OnJump += Jump;

        switch (_crouchInputMode)
        {
            case InputMode.Press:
                PlayerInput.Instance.OnCrouchStarted += ChangeCrouch;
                break;
            case InputMode.Hold:
                PlayerInput.Instance.OnCrouchStarted += StartCrouch;
                PlayerInput.Instance.OnCrouchCanceled += CancelCrouch;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        #endregion

        StartCoroutine(SlowBackCoroutine());
    }

    private IEnumerator SlowBackCoroutine()
    {
        while (true)
        {
            _basicMovementForce -= _slowBack;
            _movementForce -= _slowBack;
            GameUIManager.Instance.UpdateSpeedSlider(_basicMovementForce);
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Unsubscribes from input events when disabled.
    /// </summary>
    private void OnDisable()
    {
        PlayerInput.Instance.OnJump -= Jump;

        switch (_crouchInputMode)
        {
            case InputMode.Press:
                PlayerInput.Instance.OnCrouchStarted -= ChangeCrouch;
                break;
            case InputMode.Hold:
                PlayerInput.Instance.OnCrouchStarted -= StartCrouch;
                PlayerInput.Instance.OnCrouchCanceled -= CancelCrouch;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion

    #region Movement

    /// <summary>
    /// Handles player movement based on input, including sliding and basic movement.
    /// </summary>
    /// <param name="input">The movement input.</param>
    public void MovementHandler(Vector2 input)
    {
        if (!_isCanMove) return;

        _speedLinesAlphaMap.value = _movementForce / _maxMovementForce;

        if (_isSliding)
            HandleSlidingMovement(input);
        else
            HandleBasicMovement(input);
    }

    /// <summary>
    /// Handles basic movement when not sliding or crouching.
    /// </summary>
    /// <param name="input">The movement input.</param>
    private void HandleBasicMovement(Vector2 input)
    {
        Vector3 forceDirection = Vector3.zero;
        forceDirection += transform.right * (input.x * _movementForce);
        forceDirection += transform.forward * (input.y * _movementForce);

        _rigidbody.AddForce(forceDirection * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    /// <summary>
    /// Handles sliding movement and speed reduction.
    /// </summary>
    /// <param name="input">The movement input.</param>
    private void HandleSlidingMovement(Vector2 input)
    {
        Vector3 forceDirection = Vector3.zero;
        forceDirection += transform.forward * (input.y * _movementForce * _slideSpeedScale);

        _slideSpeedScale -= _slideSlowBack * Time.fixedDeltaTime;

        if (_rigidbody.linearVelocity.magnitude < 0.1f)
            EndSlide();

        _rigidbody.AddForce(forceDirection * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    #endregion

    #region Gravity and Jumping

    /// <summary>
    /// Updates gravity for the player based on whether they are grounded or jumping.
    /// </summary>
    public void HandleGravity()
    {
        _currentGravityForce = _rigidbody.linearVelocity;

        if (IsGrounded())
            _currentGravityForce.y = 0f;
        else if (!IsJumping())
            _currentGravityForce.y += Physics.gravity.y * Time.fixedDeltaTime;

        _rigidbody.linearVelocity = _currentGravityForce;
    }

    /// <summary>
    /// Makes the player jump if they are grounded and not crouching.
    /// </summary>
    public void Jump()
    {
        if (IsGrounded() && !_isCrouching)
        {
            _rigidbody.AddForce(transform.up * _jumpForce);
        }
    }

    /// <summary>
    /// Checks if the player is currently jumping.
    /// </summary>
    /// <returns>True if the player is jumping, false otherwise.</returns>
    private bool IsJumping() => _rigidbody.linearVelocity.y > 0f;

    /// <summary>
    /// Checks if the player is grounded.
    /// </summary>
    /// <returns>True if the player is grounded, false otherwise.</returns>
    private bool IsGrounded() => Physics.Raycast(transform.position, Vector3.down, _collider.height / 2 + 0.01f, ~_notGroundLayers);

    #endregion

    #region Crouch

    /// <summary>
    /// Toggles crouch when the input mode is press-based.
    /// </summary>
    public void ChangeCrouch()
    {
        _isCrouching = !_isCrouching;
        if (_isCrouching)
            StartCrouch();
        else
            CancelCrouch();
    }

    /// <summary>
    /// Starts crouching and adjusts the collider height.
    /// </summary>
    public void StartCrouch()
    {
        _isCrouching = true;
        StartCoroutine(ColliderHeightChange(_colliderBasicHeight * _crouchColliderMultiplier));
        StartSlide();
    }

    /// <summary>
    /// Cancels crouch and resets collider height.
    /// </summary>
    public void CancelCrouch()
    {
        StartCoroutine(ColliderHeightChange(_colliderBasicHeight));
        SetEndCrouchValues();
    }

    /// <summary>
    /// Resets the crouching values when the player exits crouch.
    /// </summary>
    private void SetEndCrouchValues()
    {
        _movementForce = _basicMovementForce;
        _isCrouching = false;
        _isSliding = false;
    }

    /// <summary>
    /// Coroutine to smoothly change the collider height during crouch.
    /// </summary>
    /// <param name="endValue">The target height value.</param>
    private IEnumerator ColliderHeightChange(float endValue)
    {
        float startValue = _collider.height;
        float timeElapsed = 0;

        while (timeElapsed < _crouchSpeed)
        {
            _collider.height = Mathf.Lerp(startValue, endValue, Mathf.SmoothStep(0.0f, 1.0f, timeElapsed / _crouchSpeed));
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }

    #endregion

    #region Slide

    /// <summary>
    /// Starts the sliding state and increases movement speed.
    /// </summary>
    private void StartSlide()
    {
        _slideSpeedScale = 1;
        _movementForce *= _slideStartSpeedMultiplier;
        _isSliding = true;
    }

    /// <summary>
    /// Ends the sliding state and resets movement force.
    /// </summary>
    private void EndSlide()
    {
        _movementForce = _basicMovementForce * _crouchSpeedMultiplier;
        _isSliding = false;
    }

    #endregion

    public void MultiplyMovementForce(float multiplier)
    {
        _movementForce *= multiplier;
        _basicMovementForce *= multiplier;
        GameUIManager.Instance.UpdateSpeedSlider(_basicMovementForce);
    }
}
