using System.Collections.Generic;
using UnityEngine;

// Spawns a single enemy using the current valid path
public class EnemySpawner : MonoBehaviour
{
    public PathTester pathTester;

    public void SpawnEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null || pathTester == null)
            return;

        List<GridNode> path = pathTester.GetCurrentPath();

        if (path == null || path.Count == 0)
        {
            Debug.Log("Cannot spawn enemy - no valid path.");
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab);
        EnemyMover mover = enemy.GetComponent<EnemyMover>();

        if (mover != null)
        {
            mover.goalMarker = pathTester.goalMarker;
            mover.SetPath(path);
        }
    }
}