using System.Collections;
using UnityEngine;

/*
    EnemySpawner.cs

    HOW TO USE IN UNITY
    1) Create an empty GameObject in your scene (for example, "EnemySpawner") and attach this script.
    2) Assign enemyPrefab to the enemy you want to spawn.
    3) Create one or more spawn point Transforms (empty GameObjects) and assign them to spawnPoints.
    4) Set spawnInterval to control how often spawning is attempted.
    5) Set maxEnemiesAlive to cap how many spawned enemies can exist at once.

    NOTES
    - Spawning uses a coroutine that runs during gameplay.
    - The script counts only enemies spawned by this spawner.
*/

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [Tooltip("Enemy prefab to spawn repeatedly.")]
    public GameObject enemyPrefab;

    [Tooltip("Possible locations where enemies can spawn.")]
    public Transform[] spawnPoints;

    [Tooltip("Time in seconds between spawn attempts.")]
    public float spawnInterval = 2f;

    [Tooltip("Maximum number of spawned enemies alive at the same time.")]
    public int maxEnemiesAlive = 10;

    private int enemiesAlive;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (!CanSpawn())
            {
                continue;
            }

            SpawnEnemy();
        }
    }

    private bool CanSpawn()
    {
        if (enemyPrefab == null)
        {
            return false;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return false;
        }

        return enemiesAlive < maxEnemiesAlive;
    }

    private void SpawnEnemy()
    {
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        if (point == null)
        {
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab, point.position, point.rotation);
        enemiesAlive++;

        SpawnedEnemyTracker tracker = enemy.AddComponent<SpawnedEnemyTracker>();
        tracker.Initialize(this);
    }

    private void NotifyEnemyDestroyed()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    /// <summary>
    /// Tracks spawned enemy destruction so the spawner can maintain an alive count.
    /// </summary>
    private class SpawnedEnemyTracker : MonoBehaviour
    {
        private EnemySpawner owner;

        public void Initialize(EnemySpawner spawner)
        {
            owner = spawner;
        }

        private void OnDestroy()
        {
            if (owner != null)
            {
                owner.NotifyEnemyDestroyed();
            }
        }
    }
}
