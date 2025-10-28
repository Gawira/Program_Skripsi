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
    public GameObject arenaBarrier;  // wall/door that locks you in
    public GameObject fullArena;     // full arena block, disabled on boss death

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

        // wake the boss AI
        if (bossAI != null)
            bossAI.ActivateBoss(playerTransform);

        // show boss HP UI
        if (bossManager != null)
            bossManager.ActivateBossUI();

        // close arena
        if (arenaBarrier != null)
            arenaBarrier.SetActive(true);

        Debug.Log("Boss fight started!");
    }

    private void Update()
    {
        if (bossManager == null || playerManager == null)
            return;

        bool bossDead = bossManager.currentHealth <= 0;
        bool playerDead = playerManager.currentHealth <= 0;

        // Boss defeated -> end fight, unlock arena, hide UI, drop handled in BossManager
        if (bossDead)
        {
            EndBossFight();
        }

        // Player died -> open barrier so player can leave / respawn logic,
        // turn off UI so it's not stuck on screen,
        // unlock fight so they can re-trigger it after respawn.
        if (playerDead)
        {
            if (fullArena != null)
                fullArena.SetActive(true);

            if (arenaBarrier != null)
                arenaBarrier.SetActive(false);

            // hide HP UI when player is dead
            if (bossManager != null)
                bossManager.ForceHideUI();

            bossActive = false;
            fightLocked = false;

            if (bossAI != null)
                bossAI.DeactivateBoss();

            bossManager.currentHealth = bossManager.maxHealth;
        }
    }

    private void EndBossFight()
    {
        bossActive = false;
        fightLocked = false;

        // shut down boss AI brain
        if (bossAI != null)
            bossAI.DeactivateBoss();

        // hide boss UI
        if (bossManager != null)
            bossManager.DeactivateBossUI();

        // open / break arena so player can leave
        if (fullArena != null)
            fullArena.SetActive(false);

        // just in case: make sure main barrier also opens
        if (arenaBarrier != null)
            arenaBarrier.SetActive(false);

        AudioManager.Instance.StopBossMusic();
        AudioManager.Instance.StopMusic();

        Debug.Log("Boss fight ended!");
    }
}
