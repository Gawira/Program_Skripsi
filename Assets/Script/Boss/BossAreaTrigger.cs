using UnityEngine;

public class BossAreaTrigger : MonoBehaviour
{
    [Header("Boss References")]
    public BossManager bossManager;
    public BossAI bossAI;

    [Header("Player Reference")]
    public string playerTag = "Player";
    public PlayerManager playerManager;

    [Header("Arena Barrier")]
    public GameObject arenaBarrier;  // Drag your wall / box collider GameObject here in Inspector
    public GameObject fullArena;

    private bool bossActive = false;
    private bool fightLocked = false;
    private Transform playerTransform;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (fightLocked) return;

        playerTransform = other.transform;

        bossActive = true;
        fightLocked = true;

        // Activate boss AI
        if (bossAI != null)
            bossAI.ActivateBoss(playerTransform);

        if (bossManager != null)
            bossManager.ActivateBossUI();

        // Activate the barrier
        if (arenaBarrier != null)
            arenaBarrier.SetActive(true);

        Debug.Log("Boss fight started!");
    }

    private void Update()
    {
        

        bool bossDead = bossManager != null && bossManager.currentHealth <= 0;
        bool playerDead = playerManager != null && playerManager.currentHealth <= 0;

        if (bossDead)
        {
            EndBossFight();
        }

        if (playerDead)
        {
            if (arenaBarrier != null)
                arenaBarrier.SetActive(false);
            bossActive = false;
            fightLocked = false;
        }
    }

    private void EndBossFight()
    {
        bossActive = false;
        fightLocked = false;

        if (bossAI != null)
            bossAI.DeactivateBoss();

        if (bossManager != null)
            bossManager.DeactivateBossUI();

        // Deactivate the barrier so player can leave
        if (fullArena != null)
            fullArena.SetActive(false);

        Debug.Log("Boss fight ended!");
    }
}
