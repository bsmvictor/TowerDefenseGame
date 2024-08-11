using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    public int CurrentWave { get; private set; } = 1;
    public int TotalScore { get; private set; } = 0;
    public int PlayerHealth { get; private set; } = 100;
    public int PlayerCoins { get; private set; } = 50;
    public int TotalEnemiesDefeated { get; private set; } = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // M�todos para manipular as ondas
    public void IncrementWave()
    {
        CurrentWave++;
    }

    public void ResetWave()
    {
        CurrentWave = 1;
    }

    // M�todos para manipular a sa�de do jogador
    public void DecreaseHealth(int amount)
    {
        PlayerHealth -= amount;
        if (PlayerHealth <= 0)
        {
            PlayerHealth = 0;
            Debug.Log("Game Over");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void IncreaseHealth(int amount)
    {
        PlayerHealth += amount;
    }

    public void ResetHealth(int health)
    {
        PlayerHealth = health;
    }

    // M�todos para manipular as moedas do jogador
    public void AddCoins(int amount)
    {
        PlayerCoins += amount;
    }

    public bool SpendCoins(int amount)
    {
        if (PlayerCoins >= amount)
        {
            PlayerCoins -= amount;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ResetCoins(int coins)
    {
        PlayerCoins = coins;
    }

    // M�todos para manipular o n�mero total de inimigos derrotados
    public void IncrementEnemiesDefeated()
    {
        TotalEnemiesDefeated++;
    }

    public void ResetEnemiesDefeated()
    {
        TotalEnemiesDefeated = 0;
    }
}
