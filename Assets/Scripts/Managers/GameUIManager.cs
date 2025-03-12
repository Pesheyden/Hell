using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the game UI, including health, score, bullets, magazines, and game end UI.
/// </summary>
public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private TextMeshProUGUI _scoreValueText;
    [SerializeField] private TextMeshProUGUI _bulletsValueText;
    [SerializeField] private TextMeshProUGUI _magazinesValueText;
    [SerializeField] private GameObject _gameEndUiContainer;
    [SerializeField] private TextMeshProUGUI _gameEndScoreText;
    [SerializeField] private Slider _speedSlider;

    private void Awake()
    {
        if (Instance)
        {
            Debug.LogError($"Two {name} instances cannot exist.");
            Destroy(this);
        }

        Instance = this;
    }

    public void SetUpHealthSlider(int maxValue)
    {
        //_healthSlider.maxValue = maxValue;
        //_healthSlider.value = maxValue;
    }

    public void UpdateHealthSliderValue(float newValue)
    {
        //_healthSlider.value = newValue;
    }

    public void UpdateScoreTextValues(string newValue)
    {
        //_scoreValueText.text = newValue;
    }

    public void UpdateBulletsText(string newValue)
    {
        //_bulletsValueText.text = newValue;
    }
    public void UpdateMagazinesText(string newValue)
    {
        //_magazinesValueText.text = newValue;
    }

    public void ActivateGameEndUI(string score)
    {
        //_gameEndUiContainer.SetActive(true);
        //_gameEndScoreText.text += score;
    }

    public void UpdateSpeedSlider(float newValue) => _speedSlider.value = newValue;
}