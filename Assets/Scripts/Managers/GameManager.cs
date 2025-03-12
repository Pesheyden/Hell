using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the game state, including starting and ending the game, and handling enemy deaths.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance {  get; private set; }

    public bool IsGamePlaying;

    [SerializeField] private float _speedMultiplier = 1.1f;
    [SerializeField] private float _enemiesReviveTime = 2;
    [SerializeField] private GameObject _deathParticlesPrefab;

    private void Awake()
    {
        if (Instance)
        {
            Debug.LogError($"Two {name} could not exist");
            Destroy(this);
        }

        Instance = this;

        IsGamePlaying = true;
    }

    public void GameEnd()
    {
        if(!IsGamePlaying)
            return;
        
        IsGamePlaying = false;
        PlayerController.Instance.Disable();
    }

    public void EnemyDeath(GameObject enemy)
    {
        PlayerController.Instance.MultiplyMovementForce(_speedMultiplier);
        enemy.SetActive(false);
        var particles = Instantiate(_deathParticlesPrefab, enemy.transform.position, Quaternion.identity);
        StartCoroutine(EnemyDeathCoroutine(enemy, particles));
    }

    private IEnumerator EnemyDeathCoroutine(GameObject enemy, GameObject particles)
    {
        yield return new WaitForSeconds(_enemiesReviveTime);
        enemy.SetActive(true);
        Destroy(particles);
    }
}
