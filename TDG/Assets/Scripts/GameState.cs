using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    public int CurrentWave { get; private set; } = 1;
    public int TotalScore { get; private set; } = 0;
    public int PlayerHealth { get; private set; } = 10;
    public int PlayerCoins { get; private set; } = 300;
    public int TotalEnemiesDefeated { get; private set; } = 0;
    public float gameSpeed = 1f;

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
            gameSpeed += 0.5f;
            Time.timeScale = gameSpeed;

            if (PlayerHealth <= 0) gameSpeed = 1f;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            gameSpeed -= 0.5f;
            Time.timeScale = gameSpeed;

            if (PlayerHealth <= 0) gameSpeed = 1f;
        }
    }

    // Método para reiniciar todo o estado do jogo
    public void ResetGameState()
    {
        ResetWave();
        ResetHealth(10);  // Reinicia a saúde para 100 ou o valor que você preferir
        ResetCoins(300);    // Reinicia as moedas para 50 ou o valor inicial que você preferir
        ResetEnemiesDefeated();
        gameSpeed = 1f;    // Reinicia a velocidade do jogo para o padrão
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
            // Reinicia o estado do jogo antes de carregar a cena
            ResetGameState();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reinicia a cena atual
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
