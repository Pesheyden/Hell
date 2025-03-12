using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages player actions, including health, movement, shooting, and camera control.
/// </summary>
[RequireComponent(typeof(HealthSystem), typeof(ShootingHandler), typeof(PlayerMovementHandler))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance {  get; private set; }
    
    
    [HideInInspector] public HealthSystem HealthSystem;
    [HideInInspector] public ShootingHandler ShootingHandler;
    public PlayerCameraHandler PlayerCameraHandler;
    [HideInInspector] public PlayerMovementHandler MovementHandler;

    private bool _isDisabled;

    private void Awake()
    {
        if (Instance)
        {
            Debug.LogError($"Two {name} could not exist");
            Destroy(this);
        }

        Instance = this;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        HealthSystem = GetComponent<HealthSystem>();
        MovementHandler = GetComponent<PlayerMovementHandler>();
        ShootingHandler = GetComponent<ShootingHandler>();
    }

    private void FixedUpdate()
    {
        if(_isDisabled)
            return;
        
        PlayerCameraHandler.HandleRotation();
        MovementHandler.MovementHandler(PlayerInput.Instance.MovementInput);
        MovementHandler.HandleGravity();
    }

    public void Disable()
    {
        _isDisabled = true;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void TakeDamage(int amount)
    {
        HealthSystem.TakeDamage(amount);
    }

    public void AddHealth(int amount)
    {
        HealthSystem.AddHealth(amount);
    }

    public void AddMagazines(int amount)
    {
        ShootingHandler.AddMagazines(amount);
    }

    public void MultiplyMovementForce(float multiplier) => MovementHandler.MultiplyMovementForce(multiplier);
}
