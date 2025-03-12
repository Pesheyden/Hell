using System;
using UnityEngine;

/// <summary>
/// Represents a bullet object in the game, including its behavior when shot, colliding, and ricocheting.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    private ShootingHandler _shootingHandler;
    private float _damage;
    private bool _canRicochet;
    private float _ricochetDamageReduction;
    private int _maxRicochetAmount;
    private int _currentRicochetAmount;
    private float _force;
    private Vector3 _lastVelocity;
    private Team _team;

    private float _bulletLifeTime = 5f;
    private float _currentDamage;

    private Rigidbody _rigidbody;

    /// <summary>
    /// Sets up the bullet with the provided configuration data.
    /// </summary>
    /// <param name="data">Bullet setup data containing all necessary properties like damage, ricochet, etc.</param>
    public void SetUp(BulletSetUpData data)
    {
        _shootingHandler = data.ShootingHandler;
        _damage = data.Damage;
        _canRicochet = data.CanRicochet;
        _ricochetDamageReduction = data.RicochetDamageReduction;
        _maxRicochetAmount = data.RicochetAmount;
        _force = data.Force;
        _team = data.Team;

        _rigidbody = GetComponent<Rigidbody>();
        
        Deactivate();
    }

    private void LateUpdate()
    {
        _lastVelocity = _rigidbody.linearVelocity;
    }

    /// <summary>
    /// Activates the bullet, enabling it to be shot and applying force.
    /// </summary>
    public void Activate()
    {
        gameObject.SetActive(true);
        _currentRicochetAmount = _maxRicochetAmount;
        _rigidbody.linearVelocity = transform.forward * _force;
        _currentDamage = _damage;
        
        Invoke(nameof(Deactivate), _bulletLifeTime);
    }

    /// <summary>
    /// Handles damage to health systems and ricochet logic.
    /// </summary>
    private void OnCollisionEnter(Collision other)
    {

        // Check for a HealthSystem to apply damage.
        if (other.gameObject.TryGetComponent<HealthSystem>(out var healthSystem))
        {
            if (healthSystem.EntityTeam != _team)
            {
                Debug.Log(_currentDamage);
                healthSystem.TakeDamage(_currentDamage);
                Deactivate();
                return;
            }
        }
        HandleRicochet(other);
    }

    private void HandleRicochet(Collision other)
    {
        if (!_canRicochet || _currentRicochetAmount == 0)
        {
            Deactivate();
            return;
        }

        var currentSpeed = _lastVelocity.magnitude;
        var direction = Vector3.Reflect(_lastVelocity.normalized, other.contacts[0].normal);
        _rigidbody.linearVelocity = direction * Mathf.Max(currentSpeed, 0);
            
        _currentRicochetAmount--;
        _currentDamage *= _ricochetDamageReduction;
    }

    /// <summary>
    /// Deactivates the bullet, returning it to the pool and resetting its properties.
    /// </summary>
    private void Deactivate()
    {
        if(!gameObject.activeSelf)
            return;
        _shootingHandler.ReturnBullet(this);
        gameObject.SetActive(false);
        _rigidbody.linearVelocity = Vector3.zero;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }
}

/// <summary>
/// Contains the setup data for the bullet, including damage, ricochet properties, and force.
/// </summary>
public class BulletSetUpData
{
    public readonly ShootingHandler ShootingHandler;
    public readonly float Damage;
    public readonly bool CanRicochet;
    public readonly float RicochetDamageReduction;
    public readonly int RicochetAmount;
    public readonly float Force;
    public readonly Team Team;

    /// <summary>
    /// Initializes the setup data for the bullet.
    /// </summary>
    /// <param name="shootingHandler">The shooting handler that owns this bullet.</param>
    /// <param name="damage">The damage dealt by the bullet.</param>
    /// <param name="canRicochet">Whether the bullet can ricochet.</param>
    /// <param name="ricochetDamageReduction">Damage reduction per ricochet.</param>
    /// <param name="ricochetAmount">Maximum ricochet count before deactivating.</param>
    /// <param name="force">The initial force applied to the bullet.</param>
    public BulletSetUpData(ShootingHandler shootingHandler, float damage, bool canRicochet, float ricochetDamageReduction, int ricochetAmount, float force, Team team)
    {
        ShootingHandler = shootingHandler;
        Damage = damage;
        CanRicochet = canRicochet;
        RicochetDamageReduction = ricochetDamageReduction;
        RicochetAmount = ricochetAmount;
        Force = force;
        Team = team;
    }
}
