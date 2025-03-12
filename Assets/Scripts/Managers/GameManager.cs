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

    }
}
