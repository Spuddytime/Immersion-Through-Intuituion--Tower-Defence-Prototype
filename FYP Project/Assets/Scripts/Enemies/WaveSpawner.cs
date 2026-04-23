using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls enemy waves with scaling difficulty and weighted enemy types
public class WaveSpawner : MonoBehaviour
{
    public EnemySpawner enemySpawner;

    [Header("Wave Scaling")]
    public int startingEnemiesPerWave = 5;
    public int additionalEnemiesPerWave = 2;
    public float timeBetweenSpawns = 1f;

    [Header("Enemy Types")]
    public List<EnemySpawnOption> enemyOptions = new List<EnemySpawnOption>();

    [SerializeField] private bool isSpawning = false;
    private int currentWave = 0;

    public bool IsSpawning => isSpawning;
    public int CurrentWave => currentWave;

    void Awake()
    {
        Debug.Log($"WaveSpawner Awake -> {gameObject.name} | ID: {GetInstanceID()}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartNextWave();
        }
    }

    public void StartNextWave()
    {
        Debug.Log($"StartNextWave called on {gameObject.name} | ID: {GetInstanceID()}");

        if (isSpawning)
        {
            Debug.Log($"Wave already spawning on {gameObject.name} | ID: {GetInstanceID()}");
            return;
        }

        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        if (enemySpawner == null)
        {
            Debug.LogWarning($"EnemySpawner missing on {gameObject.name} | ID: {GetInstanceID()}");
            yield break;
        }

        currentWave++;
        isSpawning = true;

        int enemiesThisWave = startingEnemiesPerWave + (currentWave - 1) * additionalEnemiesPerWave;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateWave(currentWave);
        }

        Debug.Log($"Wave {currentWave} started with {enemiesThisWave} enemies on {gameObject.name} | ID: {GetInstanceID()}");

        for (int i = 0; i < enemiesThisWave; i++)
        {
            GameObject enemyToSpawn = GetWeightedRandomEnemy();

            if (enemyToSpawn != null)
            {
                Debug.Log($"Spawning {enemyToSpawn.name} on wave {currentWave} from {gameObject.name} | ID: {GetInstanceID()}");
                enemySpawner.SpawnEnemy(enemyToSpawn);
            }
            else
            {
                Debug.LogWarning($"No valid enemy found for wave {currentWave} on {gameObject.name} | ID: {GetInstanceID()}");
            }

            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        Debug.Log($"Wave {currentWave} finished spawning on {gameObject.name} | ID: {GetInstanceID()}");
        isSpawning = false;
    }

    GameObject GetWeightedRandomEnemy()
    {
        List<EnemySpawnOption> validOptions = new List<EnemySpawnOption>();

        foreach (EnemySpawnOption option in enemyOptions)
        {
            string enemyName = option.enemyPrefab != null ? option.enemyPrefab.name : "NULL";

            Debug.Log($"Checking option {enemyName} | unlock={option.unlockWave} | weight={option.spawnWeight} | currentWave={currentWave} | spawner={gameObject.name} | ID={GetInstanceID()}");

            if (option.enemyPrefab != null && currentWave >= option.unlockWave && option.spawnWeight > 0)
            {
                validOptions.Add(option);
            }
        }

        if (validOptions.Count == 0)
            return null;

        int totalWeight = 0;

        foreach (EnemySpawnOption option in validOptions)
        {
            totalWeight += option.spawnWeight;
        }

        int randomValue = Random.Range(0, totalWeight);
        int runningWeight = 0;

        foreach (EnemySpawnOption option in validOptions)
        {
            runningWeight += option.spawnWeight;

            if (randomValue < runningWeight)
            {
                return option.enemyPrefab;
            }
        }

        return validOptions[0].enemyPrefab;
    }
}