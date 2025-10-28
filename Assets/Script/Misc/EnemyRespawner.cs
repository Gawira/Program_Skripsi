using UnityEngine;

public class EnemyRespawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [Tooltip("The enemy prefab to spawn (e.g. Varga2)")]
    public GameObject enemyPrefab;

    [Tooltip("Where the enemy should appear. This should be an EMPTY child under this respawner that never moves.")]
    public Transform spawnPoint;

    [Tooltip("Delay before respawning enemy after death (seconds)")]
    public float respawnDelay = 3f;

    [Header("Runtime Info")]
    [SerializeField] private GameObject currentEnemy;
    private bool isRespawning = false;

    // We cache the ORIGINAL spawn transform on Start so it never drifts
    private Vector3 fixedSpawnPos;
    private Quaternion fixedSpawnRot;

    private void Start()
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

        // Cache the intended spawn location at startup
        fixedSpawnPos = spawnPoint.position;
        fixedSpawnRot = spawnPoint.rotation;

        SpawnEnemy(); // first spawn
    }

    private void SpawnEnemy()
    {
        // Safety: don't double-spawn
        if (currentEnemy != null)
        {
            Debug.LogWarning($"EnemyRespawner on {name}: Tried to spawn but enemy already exists.");
            return;
        }

        // Spawn at the cached original position, not at enemy's current body
        currentEnemy = Instantiate(enemyPrefab, fixedSpawnPos, fixedSpawnRot);

        // Hook death event
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

        // Clean up subscription
        if (deadEnemy != null)
            deadEnemy.OnEnemyDied -= HandleEnemyDeath;

        currentEnemy = null;
        // We are *not* immediately respawning here.
        // Respawn actually happens when checkpoint calls ForceRespawnNow(),
        // OR you could invoke a delayed spawn coroutine here if you want
        // Souls-like "enemy comes back after rest only".
    }

    public void RespawnEnemy()
    {
        // Old softer respawn (only if enemy was dead)
        isRespawning = false;

        if (currentEnemy != null)
        {
            Debug.LogWarning($"EnemyRespawner on {name}: Tried to respawn but current enemy still alive!");
            return;
        }

        SpawnEnemy();
    }

    // >>> This is the bonfire reset <<<
    public void ForceRespawnNow()
    {
        // If there's still an enemy alive, wipe it
        if (currentEnemy != null)
        {
            // Unhook its death event before destroying it, to avoid dangling listeners
            EnemyManager mgr = currentEnemy.GetComponent<EnemyManager>();
            if (mgr != null)
            {
                mgr.OnEnemyDied -= HandleEnemyDeath;
            }

            Destroy(currentEnemy);
            currentEnemy = null;
        }

        isRespawning = false;

        // Always spawn a fresh one at the ORIGINAL ground position
        SpawnEnemy();
    }

    // Optional helper
    public bool HasAliveEnemy()
    {
        return currentEnemy != null;
    }
}
