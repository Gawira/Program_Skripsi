using UnityEngine;

public class BossAreaTrigger : MonoBehaviour
{
    [Header("Boss Reference")]
    public BossManager bossManager;

    public BossAI bossAI;

    [Header("Player Tag")]
    public string playerTag = "Player";
    private bool bossActive = false;
    private Transform playerTransform;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerTransform = other.transform;

        // Toggle boss state
        bossActive = !bossActive;

        if (bossActive)
        {
            // Activate boss fight
            if (bossAI != null)
                bossAI.ActivateBoss(playerTransform);

            if (bossManager != null)
                bossManager.ActivateBossUI();

            Debug.Log("Boss activated!");
        }
        else
        {
            // Deactivate boss fight
            if (bossAI != null)
                bossAI.DeactivateBoss();  // You can implement this method in BossAI

            if (bossManager != null)
                bossManager.DeactivateBossUI();

            Debug.Log("Boss deactivated!");
        }
    }
}
