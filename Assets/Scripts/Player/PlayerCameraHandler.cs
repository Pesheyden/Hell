using System;
using UnityEngine;

/// <summary>
/// Controls the player camera's rotation based on mouse input, with configurable sensitivity and vertical rotation limits.
/// </summary>
public class PlayerCameraHandler : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("How sensitive horizontal mouse movement will be")]
    [SerializeField] private float _sensX = 22f;

    [Tooltip("How sensitive vertical mouse movement will be")]
    [SerializeField] private float _sensY = 18f;

    [Tooltip("Borders of camera vertical movement rotation")]
    [SerializeField] private Vector2 _verticalBorders;

    [Space]
    [Header("Other")]
    [Tooltip("Player transform with rigidbody")]
    [SerializeField] private Transform _playerTransform;

    private Vector2 _normalizedVerticalBorders;

    #region Unity Callbacks

    /// <summary>
    /// Initializes camera settings, including normalizing vertical rotation borders.
    /// </summary>
    private void Awake()
    {
        _normalizedVerticalBorders = _verticalBorders;
        _normalizedVerticalBorders.y = 360 - _normalizedVerticalBorders.y;
    }
    

    #endregion

    #region Rotation Handling

    /// <summary>
    /// Handles both horizontal and vertical camera rotation based on player input.
    /// </summary>
    public void HandleRotation()
    {
        // Get input for mouse movement (x and y axis)
        float mouseX = PlayerInput.Instance.CameraMovementInput.x * Time.deltaTime * _sensX;
        float mouseY = PlayerInput.Instance.CameraMovementInput.y * Time.deltaTime * _sensY;

        // Handle horizontal rotation (around the Y axis)
        _playerTransform.Rotate(0, mouseX, 0);

        // Handle vertical rotation (around the X axis)
        float xRotation = -mouseY;
        float xAngle = transform.eulerAngles.x;
        float zAngle = transform.eulerAngles.z;

        bool canRotate = false;

        // Logic for controlling vertical rotation boundaries
        switch (xRotation)
        {
            // When rotating upwards
            case < 0:
                canRotate = HandleUpwardRotation(xAngle, zAngle);
                break;

            // When rotating downwards
            case > 0:
                canRotate = HandleDownwardRotation(xAngle, zAngle);
                break;
        }

        // Apply the vertical rotation if allowed
        if (canRotate)
            transform.Rotate(xRotation, 0, 0);
    }

    /// <summary>
    /// Determines whether the camera can rotate upwards based on its current angle.
    /// </summary>
    /// <param name="xAngle">Current X rotation angle of the camera.</param>
    /// <param name="zAngle">Current Z rotation angle of the camera.</param>
    /// <returns>True if rotation is allowed, otherwise false.</returns>
    private bool HandleUpwardRotation(float xAngle, float zAngle)
    {
        bool canRotate = false;

        // Case when the camera is approximately upside down (Z angle around 180 degrees)
        if (Mathf.Approximately(zAngle, 180))
        {
            // 2 and 3 quarters rotation
            if (xAngle >= 0 && xAngle <= 90)
                canRotate = true;
        }
        else
        {
            // 1 and 4 quarters rotation
            canRotate = true;
            if (xAngle >= 270 && xAngle <= _normalizedVerticalBorders.y)
                canRotate = false;
        }

        return canRotate;
    }

    /// <summary>
    /// Determines whether the camera can rotate downwards based on its current angle.
    /// </summary>
    /// <param name="xAngle">Current X rotation angle of the camera.</param>
    /// <param name="zAngle">Current Z rotation angle of the camera.</param>
    /// <returns>True if rotation is allowed, otherwise false.</returns>
    private bool HandleDownwardRotation(float xAngle, float zAngle)
    {
        bool canRotate = false;

        // Case when the camera is approximately upside down (Z angle around 180 degrees)
        if (Mathf.Approximately(zAngle, 180))
        {
            // 2 and 3 quarters rotation
            if (xAngle >= 270 && xAngle <= 360)
                canRotate = true;
        }
        else
        {
            // 1 and 4 quarters rotation
            canRotate = true;
            if (xAngle > _normalizedVerticalBorders.x && xAngle <= 90)
                canRotate = false;
        }

        return canRotate;
    }

    #endregion
}
