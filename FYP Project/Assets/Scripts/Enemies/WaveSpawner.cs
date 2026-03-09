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

    private bool isSpawning = false;
    private int currentWave = 0;

    void Update()
    {
        // Press E to start the next wave
        if (Input.GetKeyDown(KeyCode.E) && !isSpawning)
        {
            StartCoroutine(SpawnWave());
        }
    }

    IEnumerator SpawnWave()
    {
        if (enemySpawner == null)
            yield break;

        currentWave++;
        isSpawning = true;

        int enemiesThisWave = startingEnemiesPerWave + (currentWave - 1) * additionalEnemiesPerWave;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateWave(currentWave);
        }

        Debug.Log("Wave " + currentWave + " started with " + enemiesThisWave + " enemies.");

        for (int i = 0; i < enemiesThisWave; i++)
        {
            GameObject enemyToSpawn = GetWeightedRandomEnemy();

            if (enemyToSpawn != null)
            {
                enemySpawner.SpawnEnemy(enemyToSpawn);
            }

            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        Debug.Log("Wave " + currentWave + " finished spawning.");
        isSpawning = false;
    }

    GameObject GetWeightedRandomEnemy()
    {
        List<EnemySpawnOption> validOptions = new List<EnemySpawnOption>();

        foreach (EnemySpawnOption option in enemyOptions)
        {
            if (option.enemyPrefab != null && currentWave >= option.unlockWave)
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