using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MetricsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI currencyUI;
    [SerializeField] TextMeshProUGUI waveUI;
    [SerializeField] TextMeshProUGUI livesUI;

    private void Update()
    {
        // Atualiza a UI com as informações do GameState
        currencyUI.text = "Money: " + GameState.Instance.PlayerCoins.ToString();
        waveUI.text = "Wave: " + GameState.Instance.CurrentWave.ToString();
        livesUI.text = "Lives: " + GameState.Instance.PlayerHealth.ToString();
    }
}
