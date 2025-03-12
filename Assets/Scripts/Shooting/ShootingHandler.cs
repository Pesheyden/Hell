using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the shooting mechanics for the player, including shooting, reloading, and bullet management.
/// </summary>
[RequireComponent(typeof(HealthSystem))]
public class ShootingHandler : MonoBehaviour
{
    [Header("Weapon Settings")]
    [Tooltip("Weapon data to define the shooting behavior.")]
    [SerializeField] private WeaponSO _weapon;

    [Tooltip("Container to hold the bullets.")]
    [SerializeField] private Transform _bulletsContainer;

    [Tooltip("Pivot point where the bullets will be shot from.")]
    [SerializeField] private Transform _shotPivot;

    private int _magazinesAmount;
    private int _bulletsLeft;
    private Stack<Bullet> _bulletsStack = new Stack<Bullet>();
    
    private bool _isCanShoot;
    private bool _hasShotTimePassed;
    private Coroutine _shootingCoroutine;
    private bool _isReloading;
    private BulletSetUpData _bulletSetUpData;

    private Team _team;

    #region Unity Callbacks

    /// <summary>
    /// Initializes the shooting handler, setting up bullet data and available ammo.
    /// </summary>
    private void Awake()
    {
        _team = GetComponent<HealthSystem>().EntityTeam;
        
        _bulletSetUpData = new BulletSetUpData
        (
            this, 
            _weapon.Damage, 
            _weapon.CanBulletRicochet,
            _weapon.BulletRicochetDamageReduction, 
            _weapon.RicochetMaxAmount,
            _weapon.BulletForce,
            _team
        );

        _bulletsLeft = _weapon.MagazineSize;

        if (!_weapon.AreMagazinesUnlimited)
            _magazinesAmount = _weapon.MagazinesStartAmount;

        _isReloading = false;
        _isCanShoot = true;
        _hasShotTimePassed = true;
        

    }

    /// <summary>
    /// Initializes the bullets stack and subscribes to player input events.
    /// </summary>
    private void Start()
    {
        int bulletsToCreate = (int)Mathf.Clamp(_weapon.MagazineSize / _weapon.TimeBetweenShots * 2, 10, 100) ;

        for (int i = 0; i < bulletsToCreate; i++)
        {
            CreateBullet();
        }

        if (_team== Team.Player)
        {
            PlayerInput.Instance.OnShootStarted += OnShootStarted;
            PlayerInput.Instance.OnShootCanceled += OnShootCanceled;
            PlayerInput.Instance.OnReload += OnReload;
            
            GameUIManager.Instance.UpdateBulletsText(_bulletsLeft.ToString());
            GameUIManager.Instance.UpdateMagazinesText(_magazinesAmount.ToString());
        }
        
    }

    #endregion

    #region Bullet Management

    /// <summary>
    /// Creates a new bullet and adds it to the bullets stack.
    /// </summary>
    private void CreateBullet()
    {
        Bullet bullet;
        
        if(_bulletsContainer)
            bullet = Instantiate(_weapon.BulletPrefab, _bulletsContainer).GetComponent<Bullet>();
        else
            bullet = Instantiate(_weapon.BulletPrefab).GetComponent<Bullet>();
        
        bullet.SetUp(_bulletSetUpData);
        _bulletsStack.Push(bullet);
    }

    /// <summary>
    /// Handles the shooting logic when the shoot button is pressed or held.
    /// </summary>
    public void OnShootStarted()
    {
        switch (_weapon.ShootInputMode)
        {
            case InputMode.Press:
                Shoot();
                break;
            case InputMode.Hold:
                _shootingCoroutine = StartCoroutine(ShootingCoroutine());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Coroutine to handle continuous shooting when the shoot button is held.
    /// </summary>
    private IEnumerator ShootingCoroutine()
    {
        while (true)
        {
            Shoot();
            yield return new WaitForSeconds(_weapon.TimeBetweenShots);
        }
    }

    /// <summary>
    /// Handles the cancellation of shooting when the shoot button is released.
    /// </summary>
    public void OnShootCanceled()
    {
        switch (_weapon.ShootInputMode)
        {
            case InputMode.Press:
                break;
            case InputMode.Hold:
                StopCoroutine(_shootingCoroutine);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Executes the shooting logic, reducing the number of bullets left and activating the bullet.
    /// </summary>
    public void Shoot()
    {
        if (_bulletsLeft <= 0)
        {
            switch (_team)
            {
                case Team.Enemy:
                    OnReload();
                    break;
                case Team.Player:
                    return;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        if (!_isCanShoot || !_hasShotTimePassed || _isReloading)
            return;

        if (_bulletsStack.Count == 0)
        {
            CreateBullet();
            Shoot();
            return;
        }

        var bullet = _bulletsStack.Pop();

        bullet.transform.position = _shotPivot.position;
        bullet.transform.rotation = _shotPivot.rotation;
        bullet.Activate();

        _bulletsLeft--;

        StartCoroutine(ShotDelayCoroutine());

        switch (_team)
        {
            case Team.Enemy:
                break;
            case Team.Player:
                GameUIManager.Instance.UpdateBulletsText(_bulletsLeft.ToString());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Delays the next shot until the time between shots has passed.
    /// </summary>
    private IEnumerator ShotDelayCoroutine()
    {
        _hasShotTimePassed = false;
        yield return new WaitForSeconds(_weapon.TimeBetweenShots);
        _hasShotTimePassed = true;
    }

    #endregion

    #region Reloading

    /// <summary>
    /// Triggers the reload action when the reload button is pressed.
    /// </summary>
    public void OnReload()
    {
        if (_isReloading)
            return;

        if (_bulletsLeft == _weapon.MagazineSize)
            return;

        if (!_weapon.AreMagazinesUnlimited && _magazinesAmount == 0)
            return;

        StartCoroutine(ReloadingCoroutine());
    }

    /// <summary>
    /// Coroutine to handle the reloading process, including updating the ammo count.
    /// </summary>
    private IEnumerator ReloadingCoroutine()
    {
        _isReloading = true;

        GameUIManager.Instance.UpdateBulletsText("Reloading");
        if (!_weapon.AreMagazinesUnlimited)
            _magazinesAmount--;

        yield return new WaitForSeconds(_weapon.ReloadTime);
        _bulletsLeft = _weapon.MagazineSize;

        _isReloading = false;

        if (_team == Team.Player)
        {
            GameUIManager.Instance.UpdateBulletsText(_bulletsLeft.ToString());
            GameUIManager.Instance.UpdateMagazinesText(_magazinesAmount.ToString());
        }
    }

    #endregion

    #region Ammo Management

    /// <summary>
    /// Returns a bullet to the pool of available bullets.
    /// </summary>
    /// <param name="bullet">The bullet to return.</param>
    public void ReturnBullet(Bullet bullet)
    {
        _bulletsStack.Push(bullet);
    }

    /// <summary>
    /// Adds a specified amount of magazines to the player's inventory.
    /// </summary>
    /// <param name="amount">The amount of magazines to add.</param>
    public void AddMagazines(int amount)
    {
        if (_magazinesAmount + amount >= _weapon.MagazinesMaxAmount)
            _magazinesAmount = _weapon.MagazinesMaxAmount;
        else
            _magazinesAmount += amount;
        
        GameUIManager.Instance.UpdateMagazinesText(_magazinesAmount.ToString());
    }

    #endregion
}
