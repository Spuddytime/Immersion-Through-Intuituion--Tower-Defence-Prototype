using UnityEngine;

[System.Serializable]
public class EnemySpawnOption
{
    public GameObject enemyPrefab;
    public int spawnWeight = 10;
    public int unlockWave = 1;
}