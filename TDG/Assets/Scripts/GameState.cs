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
    public float GameSpeed = 1f;

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

    private void Update()
    {
        //Cheat do money
        if (Input.GetKeyDown(KeyCode.W))
        {
            AddCoins(100);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SpendCoins(100);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            GameSpeed += 0.5f;
            Time.timeScale = GameSpeed;

            if(PlayerHealth <= 0) GameSpeed = 1f;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            GameSpeed -= 0.5f;
            Time.timeScale = GameSpeed;

            if (PlayerHealth <= 0) GameSpeed = 1f;
        }
    }

    // Métodos para manipular as ondas
    public void IncrementWave()
    {
        CurrentWave++;
    }

    public void ResetWave()
    {
        CurrentWave = 1;
    }

    // Métodos para manipular a saúde do jogador
    public void DecreaseHealth(int amount)
    {
        PlayerHealth -= amount;
        if (PlayerHealth <= 0)
        {
            PlayerHealth = 0;
            Debug.LogWarning("Game Over");
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

    // Métodos para manipular as moedas do jogador
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

    // Métodos para manipular o número total de inimigos derrotados
    public void IncrementEnemiesDefeated()
    {
        TotalEnemiesDefeated++;
    }

    public void ResetEnemiesDefeated()
    {
        TotalEnemiesDefeated = 0;
    }
}
