using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] enemyPrefabs; // Prefabs dos inimigos

    [Header("Attributes")]
    [SerializeField] private float enemiesPerSecond; // Frequência de spawn dos inimigos
    [SerializeField] private float timeBetweenWaves; // Tempo de espera entre as ondas

    [Header("Events")]
    public static UnityEvent onEnemyDestroy = new UnityEvent(); // Evento chamado quando um inimigo é destruído

    private float timeSinceLastSpawn; // Tempo desde o último spawn de inimigo
    private int enemiesAlive; // Contagem de inimigos vivos
    private int enemiesLeftToSpawn; // Contagem de inimigos restantes para spawnar na onda atual
    private bool isSpawning = false; // Indica se a onda está em andamento

    private Dictionary<int, List<int>> waveData; // Dados de configuração das waves

    private void Awake()
    {
        onEnemyDestroy.AddListener(EnemyDestroyed); // Inscreve o método EnemyDestroyed ao evento onEnemyDestroy
        InitializeWaveData(); // Inicializa manualmente as waves
    }

    private void Start()
    {
        enemiesPerSecond = 0.5f; // Define a frequência inicial de spawn
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
        // Verifica se é hora de spawnar o próximo inimigo
        return timeSinceLastSpawn >= (1f / enemiesPerSecond) && enemiesLeftToSpawn > 0;
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
            foreach (int count in waveData[currentWave])
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
        List<int> enemiesForWave = waveData[currentWave];

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
    // Configura manualmente os dados das waves
    waveData = new Dictionary<int, List<int>>()
{
    { 1, new List<int> { 10, 0, 0 } },       // Wave 1: 10 inimigos do tipo 0
    { 2, new List<int> { 15, 5, 0 } },       // Wave 2: 15 inimigos do tipo 0, 5 do tipo 1
    { 3, new List<int> { 20, 10, 5 } },      // Wave 3: 20 inimigos do tipo 0, 10 do tipo 1, 5 do tipo 2
    { 4, new List<int> { 25, 15, 10 } },     // Wave 4: 25 inimigos do tipo 0, 15 do tipo 1, 10 do tipo 2
    { 5, new List<int> { 30, 20, 15 } },     // Wave 5: 30 inimigos do tipo 0, 20 do tipo 1, 15 do tipo 2
    { 6, new List<int> { 35, 25, 20 } },     // Wave 6: 35 inimigos do tipo 0, 25 do tipo 1, 20 do tipo 2
    { 7, new List<int> { 40, 30, 25 } },     // Wave 7: 40 inimigos do tipo 0, 30 do tipo 1, 25 do tipo 2
    { 8, new List<int> { 45, 35, 30 } },     // Wave 8: 45 inimigos do tipo 0, 35 do tipo 1, 30 do tipo 2
    { 9, new List<int> { 50, 40, 35 } },     // Wave 9: 50 inimigos do tipo 0, 40 do tipo 1, 35 do tipo 2
    { 10, new List<int> { 55, 45, 40 } },    // Wave 10: 55 inimigos do tipo 0, 45 do tipo 1, 40 do tipo 2
    { 11, new List<int> { 60, 50, 45 } },    // Wave 11: 60 inimigos do tipo 0, 50 do tipo 1, 45 do tipo 2
    { 12, new List<int> { 65, 55, 50 } },    // Wave 12: 65 inimigos do tipo 0, 55 do tipo 1, 50 do tipo 2
    { 13, new List<int> { 70, 60, 55 } },    // Wave 13: 70 inimigos do tipo 0, 60 do tipo 1, 55 do tipo 2
    { 14, new List<int> { 75, 65, 60 } },    // Wave 14: 75 inimigos do tipo 0, 65 do tipo 1, 60 do tipo 2
    { 15, new List<int> { 80, 70, 65 } },    // Wave 15: 80 inimigos do tipo 0, 70 do tipo 1, 65 do tipo 2
    { 16, new List<int> { 85, 75, 70 } },    // Wave 16: 85 inimigos do tipo 0, 75 do tipo 1, 70 do tipo 2
    { 17, new List<int> { 90, 80, 75 } },    // Wave 17: 90 inimigos do tipo 0, 80 do tipo 1, 75 do tipo 2
    { 18, new List<int> { 95, 85, 80 } },    // Wave 18: 95 inimigos do tipo 0, 85 do tipo 1, 80 do tipo 2
    { 19, new List<int> { 100, 90, 85 } },   // Wave 19: 100 inimigos do tipo 0, 90 do tipo 1, 85 do tipo 2
    { 20, new List<int> { 110, 100, 90 } }   // Wave 20: 110 inimigos do tipo 0, 100 do tipo 1, 90 do tipo 2
};

    }
}
