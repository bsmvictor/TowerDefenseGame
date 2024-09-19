using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Classe para armazenar os dados de cada onda (quantidade de inimigos e intervalo de spawn)
public class WaveData
{
    public List<int> EnemyCounts { get; private set; } // Quantidade de inimigos por tipo
    public float SpawnInterval { get; private set; }   // Intervalo de spawn entre inimigos

    public WaveData(List<int> enemyCounts, float spawnInterval)
    {
        EnemyCounts = enemyCounts;
        SpawnInterval = spawnInterval;
    }
}

public class EnemySpawner : MonoBehaviour
{
    // Singleton Instance
    public static EnemySpawner Instance { get; private set; }

    [Header("References")]
    [SerializeField] public GameObject[] enemyPrefabs; // Prefabs dos inimigos

    [Header("Attributes")]
    [SerializeField] private float timeBetweenWaves; // Tempo de espera entre as ondas

    [Header("Events")]
    public static UnityEvent onEnemyDestroy = new UnityEvent(); // Evento chamado quando um inimigo é destruído

    private float timeSinceLastSpawn; // Tempo desde o último spawn de inimigo
    private int enemiesAlive; // Contagem de inimigos vivos
    private int enemiesLeftToSpawn; // Contagem de inimigos restantes para spawnar na onda atual
    private bool isSpawning = false; // Indica se a onda está em andamento

    private Dictionary<int, WaveData> waveData; // Dados de configuração das waves

    private void Awake()
    {
        // Implementação do Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Garante que não haja mais de uma instância do singleton
            return;
        }

        // Garante que o singleton persista entre as cenas, se necessário
        DontDestroyOnLoad(gameObject);

        onEnemyDestroy.AddListener(EnemyDestroyed); // Inscreve o método EnemyDestroyed ao evento onEnemyDestroy
        InitializeWaveData(); // Inicializa manualmente as waves
    }

    private void Start()
    {
        timeBetweenWaves = 5f; // Define o tempo entre as ondas

        StartCoroutine(StartWave()); // Inicia o ciclo de waves
    }

    private void Update()
    {
        if (isSpawning)
        {
            HandleSpawning(); // Gerencia o spawn dos inimigos durante a onda
        }
    }

    private void HandleSpawning()
    {
        timeSinceLastSpawn += Time.deltaTime; // Incrementa o tempo desde o último spawn

        if (ShouldSpawnNextEnemy())
        {
            SpawnEnemy(); // Spawna o próximo inimigo se as condições forem atendidas
        }

        if (WaveComplete())
        {
            EndWave(); // Finaliza a onda se todos os inimigos tiverem sido spawados e destruídos
        }
    }

    private bool ShouldSpawnNextEnemy()
    {
        // Verifica se é hora de spawnar o próximo inimigo com base no intervalo específico da onda
        int currentWave = GameState.Instance.CurrentWave;
        float spawnInterval = waveData[currentWave].SpawnInterval;
        return timeSinceLastSpawn >= spawnInterval && enemiesLeftToSpawn > 0;
    }

    private void SpawnEnemy()
    {
        int index = GetEnemyTypeToSpawn(); // Determina o tipo de inimigo a ser spawnado
        GameObject prefabToSpawn = enemyPrefabs[index]; // Seleciona o prefab com base no tipo
        Instantiate(prefabToSpawn, GameManager.main.startPoint.position, Quaternion.identity); // Spawna o inimigo
        enemiesLeftToSpawn--; // Decrementa a contagem de inimigos restantes para spawnar
        enemiesAlive++; // Incrementa a contagem de inimigos vivos
        timeSinceLastSpawn = 0f; // Reseta o tempo desde o último spawn
    }

    private IEnumerator StartWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves); // Espera pelo tempo definido entre as ondas
        BeginWave(); // Inicia a nova onda
    }

    private void BeginWave()
    {
        isSpawning = true; // Marca que a onda está em andamento
        enemiesLeftToSpawn = CalculateEnemiesForCurrentWave(); // Calcula quantos inimigos spawnar nesta onda
        timeSinceLastSpawn = 0f; // Reseta o tempo desde o último spawn
    }

    private void EndWave()
    {
        isSpawning = false; // Marca que a onda terminou
        GameState.Instance.IncrementWave(); // Avança para a próxima onda
        StartCoroutine(StartWave()); // Inicia o ciclo da próxima onda
    }

    private int CalculateEnemiesForCurrentWave()
    {
        // Calcula o número total de inimigos para a onda atual
        int currentWave = GameState.Instance.CurrentWave;
        if (waveData.ContainsKey(currentWave))
        {
            int totalEnemies = 0;
            foreach (int count in waveData[currentWave].EnemyCounts)
            {
                totalEnemies += count;
            }
            return totalEnemies;
        }
        else
        {
            Debug.LogWarning("Wave não encontrada! Nenhum inimigo será gerado.");
            return 0;
        }
    }

    private int GetEnemyTypeToSpawn()
    {
        int currentWave = GameState.Instance.CurrentWave;
        List<int> enemiesForWave = waveData[currentWave].EnemyCounts;

        // Identifica e retorna o tipo de inimigo a ser spawnado
        for (int i = 0; i < enemiesForWave.Count; i++)
        {
            if (enemiesForWave[i] > 0)
            {
                enemiesForWave[i]--; // Decrementa a contagem do tipo selecionado
                return i; // Retorna o índice do tipo de inimigo
            }
        }

        return 0; // Retorna o tipo padrão se nenhum for encontrado
    }

    private bool WaveComplete()
    {
        // Verifica se a onda está completa (todos os inimigos foram spawnados e destruídos)
        return enemiesAlive == 0 && enemiesLeftToSpawn == 0;
    }

    private void EnemyDestroyed()
    {
        enemiesAlive--; // Decrementa a contagem de inimigos vivos quando um inimigo é destruído
    }

    private void InitializeWaveData()
    {
        waveData = new Dictionary<int, WaveData>()
    {
        { 1, new WaveData(new List<int> { 10, 0, 0 }, 1.0f) },    // Wave 1: 10 inimigos do tipo 0, intervalo de 1.0s entre eles
        { 2, new WaveData(new List<int> { 15, 5, 0 }, 0.9f) },    // Wave 2: 15 inimigos do tipo 0, 5 do tipo 1, intervalo de 0.9s
        { 3, new WaveData(new List<int> { 20, 10, 5 }, 0.8f) },   // Wave 3: 20 inimigos do tipo 0, 10 do tipo 1, 5 do tipo 2, intervalo de 0.8s
        { 4, new WaveData(new List<int> { 25, 15, 10 }, 0.75f) }, // Wave 4: 25 inimigos do tipo 0, 15 do tipo 1, 10 do tipo 2, intervalo de 0.75s
        { 5, new WaveData(new List<int> { 30, 20, 15 }, 0.7f) },  // Wave 5: 30 inimigos do tipo 0, 20 do tipo 1, 15 do tipo 2, intervalo de 0.7s
        { 6, new WaveData(new List<int> { 35, 25, 20 }, 0.65f) }, // Wave 6: 35 inimigos do tipo 0, 25 do tipo 1, 20 do tipo 2, intervalo de 0.65s
        { 7, new WaveData(new List<int> { 40, 30, 25 }, 0.6f) },  // Wave 7: 40 inimigos do tipo 0, 30 do tipo 1, 25 do tipo 2, intervalo de 0.6s
        { 8, new WaveData(new List<int> { 45, 35, 30 }, 0.55f) }, // Wave 8: 45 inimigos do tipo 0, 35 do tipo 1, 30 do tipo 2, intervalo de 0.55s
        { 9, new WaveData(new List<int> { 50, 40, 35 }, 0.5f) },  // Wave 9: 50 inimigos do tipo 0, 40 do tipo 1, 35 do tipo 2, intervalo de 0.5s
        { 10, new WaveData(new List<int> { 55, 45, 40 }, 0.45f) },// Wave 10: 55 inimigos do tipo 0, 45 do tipo 1, 40 do tipo 2, intervalo de 0.45s
        { 11, new WaveData(new List<int> { 60, 50, 45 }, 0.4f) }, // Wave 11: 60 inimigos do tipo 0, 50 do tipo 1, 45 do tipo 2, intervalo de 0.4s
        { 12, new WaveData(new List<int> { 65, 55, 50 }, 0.35f) },// Wave 12: 65 inimigos do tipo 0, 55 do tipo 1, 50 do tipo 2, intervalo de 0.35s
        { 13, new WaveData(new List<int> { 70, 60, 55 }, 0.3f) }, // Wave 13: 70 inimigos do tipo 0, 60 do tipo 1, 55 do tipo 2, intervalo de 0.3s
        { 14, new WaveData(new List<int> { 75, 65, 60 }, 0.25f) },// Wave 14: 75 inimigos do tipo 0, 65 do tipo 1, 60 do tipo 2, intervalo de 0.25s
        { 15, new WaveData(new List<int> { 80, 70, 65 }, 0.2f) }, // Wave 15: 80 inimigos do tipo 0, 70 do tipo 1, 65 do tipo 2, intervalo de 0.2s
        { 16, new WaveData(new List<int> { 85, 75, 70 }, 0.18f) },// Wave 16: 85 inimigos do tipo 0, 75 do tipo 1, 70 do tipo 2, intervalo de 0.18s
        { 17, new WaveData(new List<int> { 90, 80, 75 }, 0.16f) },// Wave 17: 90 inimigos do tipo 0, 80 do tipo 1, 75 do tipo 2, intervalo de 0.16s
        { 18, new WaveData(new List<int> { 95, 85, 80 }, 0.14f) },// Wave 18: 95 inimigos do tipo 0, 85 do tipo 1, 80 do tipo 2, intervalo de 0.14s
        { 19, new WaveData(new List<int> { 100, 90, 85 }, 0.12f) },// Wave 19: 100 inimigos do tipo 0, 90 do tipo 1, 85 do tipo 2, intervalo de 0.12s
        { 20, new WaveData(new List<int> { 110, 100, 90 }, 0.1f) } // Wave 20: 110 inimigos do tipo 0, 100 do tipo 1, 90 do tipo 2, intervalo de 0.1s
    };
    }
}
