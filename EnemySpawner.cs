using System.Collections;
using System.Collections.Generic;
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
    - If no spawn points are assigned, it will spawn at the spawner's own transform.
    - The script tracks only enemies spawned by this spawner.
*/

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [Tooltip("Enemy prefab to spawn repeatedly.")]
    public GameObject enemyPrefab;

    [Tooltip("Possible locations where enemies can spawn. If empty, spawns at this object.")]
    public Transform[] spawnPoints;

    [Min(0.05f)]
    [Tooltip("Time in seconds between spawn attempts.")]
    public float spawnInterval = 2f;

    [Min(1)]
    [Tooltip("Maximum number of spawned enemies alive at the same time.")]
    public int maxEnemiesAlive = 10;

    private readonly List<GameObject> activeEnemies = new List<GameObject>();

    private void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: enemyPrefab is not assigned.", this);
        }

        StartCoroutine(SpawnLoop());
    }

    private void OnValidate()
    {
        if (spawnInterval < 0.05f)
        {
            spawnInterval = 0.05f;
        }

        if (maxEnemiesAlive < 1)
        {
            maxEnemiesAlive = 1;
        }
    }

    private IEnumerator SpawnLoop()
    {
        // Spawn immediately when gameplay starts (if possible), then continue on interval.
        TrySpawn();

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            TrySpawn();
        }
    }

    private void TrySpawn()
    {
        CleanupDestroyedEnemies();

        if (enemyPrefab == null)
        {
            return;
        }

        if (activeEnemies.Count >= maxEnemiesAlive)
        {
            return;
        }

        Transform spawnPoint = GetRandomSpawnPoint();
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        activeEnemies.Add(enemy);
    }

    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return transform;
        }

        // Try a few random picks to avoid null entries in the array.
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform candidate = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (candidate != null)
            {
                return candidate;
            }
        }

        Debug.LogWarning("EnemySpawner: spawnPoints contains only null entries. Using spawner transform.", this);
        return transform;
    }

    private void CleanupDestroyedEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i);
            }
        }
    }
}
