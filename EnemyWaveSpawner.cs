using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    EnemyWaveSpawner.cs

    HOW TO USE
    1) Attach this script to an empty GameObject in your scene.
    2) Assign one or more enemy prefabs to enemyPrefabs in the Inspector.
    3) Assign one or more spawn point transforms to spawnPoints.
    4) Set enemiesPerWave and timeBetweenWaves to tune pacing.

    BEHAVIOR
    - Starts at wave 1.
    - Each new wave spawns more enemies than the last.
    - Enemy types are chosen randomly from enemyPrefabs.
    - Spawn locations are chosen randomly from spawnPoints.
    - Once all enemies in the current wave are destroyed, the spawner waits
      timeBetweenWaves seconds and starts the next wave.
*/

public class EnemyWaveSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    [Tooltip("Enemy prefabs that can be randomly selected for each spawn.")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    [Tooltip("Possible spawn locations for enemies.")]
    public Transform[] spawnPoints;

    [Tooltip("Base number of enemies spawned in wave 1.")]
    public int enemiesPerWave = 5;

    [Tooltip("Delay in seconds before the next wave begins after a wave is cleared.")]
    public float timeBetweenWaves = 10f;

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private int currentWave = 0;
    private bool waitingForNextWave;

    private void Start()
    {
        StartNextWave();
    }

    private void Update()
    {
        CleanupDestroyedEnemies();

        if (currentWave <= 0 || waitingForNextWave)
        {
            return;
        }

        if (aliveEnemies.Count == 0)
        {
            Debug.Log("Wave cleared", this);
            StartCoroutine(BeginNextWaveAfterDelay());
        }
    }

    private void StartNextWave()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("EnemyWaveSpawner: No enemy prefabs assigned.", this);
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("EnemyWaveSpawner: No spawn points assigned.", this);
            return;
        }

        currentWave++;
        int enemiesThisWave = enemiesPerWave + (currentWave - 1);

        Debug.Log($"Wave {currentWave} started", this);

        for (int i = 0; i < enemiesThisWave; i++)
        {
            SpawnRandomEnemy();
        }
    }

    private void SpawnRandomEnemy()
    {
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        Transform spawnPoint = GetRandomSpawnPoint();

        if (enemyPrefab == null || spawnPoint == null)
        {
            return;
        }

        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        aliveEnemies.Add(spawnedEnemy);
    }

    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform candidate = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (candidate != null)
            {
                return candidate;
            }
        }

        return null;
    }

    private IEnumerator BeginNextWaveAfterDelay()
    {
        waitingForNextWave = true;
        yield return new WaitForSeconds(timeBetweenWaves);
        waitingForNextWave = false;
        StartNextWave();
    }

    private void CleanupDestroyedEnemies()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
            }
        }
    }
}
