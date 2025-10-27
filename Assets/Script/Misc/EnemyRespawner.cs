using UnityEngine;

public class EnemyRespawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [Tooltip("The enemy prefab to spawn (e.g. Varga2)")]
    public GameObject enemyPrefab;

    [Tooltip("Spawn point transform inside this prefab (e.g. SpawnPointVarga)")]
    public Transform spawnPoint;

    [Tooltip("Delay before respawning enemy after death (seconds)")]
    public float respawnDelay = 3f;

    [Header("Runtime Info")]
    [SerializeField] private GameObject currentEnemy;
    private bool isRespawning = false;

    void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"EnemyRespawner on {name} has no enemyPrefab assigned!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning($"EnemyRespawner on {name} has no spawnPoint assigned. Using self position.");
            spawnPoint = transform;
        }

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        // Prevent double spawning
        if (currentEnemy != null)
        {
            Debug.LogWarning($"EnemyRespawner on {name}: Tried to spawn but enemy already exists.");
            return;
        }

        // Spawn enemy prefab at spawn point
        currentEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        // Listen for enemy death
        EnemyManager enemyManager = currentEnemy.GetComponent<EnemyManager>();
        if (enemyManager != null)
        {
            enemyManager.OnEnemyDied += HandleEnemyDeath;
        }
    }

    private void HandleEnemyDeath(EnemyManager deadEnemy)
    {
        if (isRespawning) return;
        isRespawning = true;

        // Unsubscribe from the dead enemy
        if (deadEnemy != null)
            deadEnemy.OnEnemyDied -= HandleEnemyDeath;

        // Clear the current enemy reference
        currentEnemy = null;

    }

    public void RespawnEnemy()
    {
        isRespawning = false;

        // Double safety check — don’t respawn if an enemy already exists
        if (currentEnemy != null)
        {
            Debug.LogWarning($"EnemyRespawner on {name}: Tried to respawn but current enemy still alive!");
            return;
        }

        SpawnEnemy();
    }

    // Optional: for external scripts (e.g. checkpoint)
    public bool HasAliveEnemy()
    {
        return currentEnemy != null;
    }
}
