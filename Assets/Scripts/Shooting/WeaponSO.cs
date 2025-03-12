using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a weapon configuration for the entity. This ScriptableObject contains the parameters
/// used to define a weapon's behavior, such as damage, magazine size, reload time, shooting mode, and bullet properties.
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/Weapon", fileName = "Weapon")]
public class WeaponSO : ScriptableObject
{
    public int Damage;
    
    public int MagazineSize;
    
    public bool AreMagazinesUnlimited;
    
    [Tooltip("Skip if using unlimited magazines")]
    public int MagazinesStartAmount;
    
    [Tooltip("Skip if using unlimited magazines")]
    public int MagazinesMaxAmount;
    
    public float TimeBetweenShots;
    
    public float ReloadTime;
    
    public InputMode ShootInputMode;
    
    public GameObject BulletPrefab;
    
    public float BulletForce;
    
    public bool CanBulletRicochet;
    
    public float BulletRicochetDamageReduction;
    
    public int RicochetMaxAmount;
}
