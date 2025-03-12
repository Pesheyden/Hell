using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The HealthSystem class manages the health of an entity, including health regeneration and damage handling.
/// It provides functionality for adding health, taking damage, and automatically regenerating health over time.
/// The class is designed to handle different types of entities, such as Player and Enemy, and provides UI updates through a health slider.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    /// <summary>
    /// The team that the entity belongs to (e.g., Player or Enemy).
    /// </summary>
    public Team EntityTeam;

    [SerializeField] 
    private int _maxHealth;

    [Tooltip("Health points per 1 second regeneration.")]
    [SerializeField] 
    private float _regeneration;

    [Tooltip("After n seconds after taking damage, regeneration will start.")]
    [SerializeField] 
    private float _regenerationDelay;

    /// <summary>
    /// The current health of the entity.
    /// </summary>
    public float CurrentHealth { get; private set; }

    private WaitForSeconds _regenerationWait;
    private Coroutine _currentCoroutine;
    
    // Initializes the health system, setting current health to max health and preparing for regeneration.
    private void Awake()
    {
        CurrentHealth = _maxHealth;
        _regenerationWait = new WaitForSeconds(_regenerationDelay);
        _currentCoroutine = null;
    }
    
    // Sets up the health slider for the player entity at the start.
    private void Start()
    {
        if(EntityTeam == Team.Player)
            GameUIManager.Instance.SetUpHealthSlider(_maxHealth);
    }

    /// <summary>
    /// Adds health to the entity, ensuring it doesn't exceed the maximum health.
    /// </summary>
    /// <param name="amount">The amount of health to add.</param>
    public void AddHealth(int amount)
    {
        if (CurrentHealth + amount >= _maxHealth)
            CurrentHealth = _maxHealth;
        else
            CurrentHealth += amount;
        
        GameUIManager.Instance.UpdateHealthSliderValue(CurrentHealth);
    }

    /// <summary>
    /// Reduces the entity's health by the specified damage amount. If health drops to zero or below for the player, the game ends.
    /// </summary>
    /// <param name="damage">The amount of damage taken.</param>
    public void TakeDamage(float damage)
    {

        CurrentHealth -= damage;
        Debug.Log("damage taken " + damage + " " + gameObject + " " + CurrentHealth);
        switch (EntityTeam)
        {
            case Team.Enemy:
                if (CurrentHealth <= 0)
                {
                    GameManager.Instance.EnemyDeath(gameObject);
                    return;
                }
                break;
            case Team.Player:
                if (CurrentHealth <= 0)
                {
                    GameManager.Instance.GameEnd();
                    return;
                }
                GameUIManager.Instance.UpdateHealthSliderValue(CurrentHealth);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        // Stop any ongoing regeneration and start a new regeneration coroutine
        if(_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);
        
        _currentCoroutine = StartCoroutine(RegenerationCoroutine());
    }

    /// <summary>
    /// Coroutine that handles health regeneration after a delay, restoring health over time.
    /// </summary>
    private IEnumerator RegenerationCoroutine()
    {
        yield return _regenerationWait;
        while (CurrentHealth < _maxHealth)
        {
            CurrentHealth += _regeneration;
            if (CurrentHealth > _maxHealth)
                CurrentHealth = _maxHealth;
            
            if(EntityTeam == Team.Player)
                GameUIManager.Instance.UpdateHealthSliderValue(CurrentHealth);
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Resets entity health and if player updates health slider
    /// </summary>
    public void ResetHealth()
    {
        CurrentHealth = _maxHealth;
        if(EntityTeam == Team.Player)
            GameUIManager.Instance.UpdateHealthSliderValue(CurrentHealth);
    }
}
