using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Attributes")]
    [SerializeField] private int baseEnemies;
    [SerializeField] private float enemiesPerSecond; // Frequência inicial de spawn
    [SerializeField] private float timeBetweenWaves;   // Tempo entre ondas
    [SerializeField] private float difficultyScalingFactor; // Escala de dificuldade para ajustar o spawn

    [Header("Events")]
    public static UnityEvent onEnemyDestroy = new UnityEvent();

    private float timeSinceLastSpawn;
    private int enemiesAlive;
    private int enemiesLeftToSpawn;
    private bool isSpawning = false;

    private void Awake()
    {
        onEnemyDestroy.AddListener(EnemyDestroyed);
    }

    private void Start()
    {
        baseEnemies = 20;
        enemiesPerSecond = 0.5f;
        timeBetweenWaves = 5f;
        difficultyScalingFactor = 1.2f;

        StartCoroutine(StartWave());
    }

    private void Update()
    {
        if (isSpawning)
        {
            HandleSpawning();
        }
    }

    private void HandleSpawning()
    {
        timeSinceLastSpawn += Time.deltaTime;

        if (ShouldSpawnNextEnemy())
        {
            SpawnEnemy();
        }

        if (WaveComplete())
        {
            EndWave();
        }
    }

    private bool ShouldSpawnNextEnemy()
    {
        return timeSinceLastSpawn >= (1f / enemiesPerSecond) && enemiesLeftToSpawn > 0;
    }

    private void SpawnEnemy()
    {
        int index = Random.Range(0, enemyPrefabs.Length);
        GameObject prefabToSpawn = enemyPrefabs[index];
        Instantiate(prefabToSpawn, GameManager.main.startPoint.position, Quaternion.identity);
        enemiesLeftToSpawn--;
        enemiesAlive++;
        timeSinceLastSpawn = 0f;
    }

    private IEnumerator StartWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        BeginWave();
    }

    private void BeginWave()
    {
        isSpawning = true;
        enemiesLeftToSpawn = CalculateEnemiesPerWave();
        timeSinceLastSpawn = 0f;

        // Escala a frequência de spawn com o fator de dificuldade
        enemiesPerSecond *= difficultyScalingFactor;
    }

    private void EndWave()
    {
        isSpawning = false;
        GameState.Instance.IncrementWave();
        StartCoroutine(StartWave());
    }

    private int CalculateEnemiesPerWave()
    {
        return Mathf.RoundToInt(baseEnemies * Mathf.Pow(GameState.Instance.CurrentWave, difficultyScalingFactor));
    }

    private bool WaveComplete()
    {
        return enemiesAlive == 0 && enemiesLeftToSpawn == 0;
    }

    private void EnemyDestroyed()
    {
        enemiesAlive--;
    }
}
