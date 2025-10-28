using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BossMusicTrigger : MonoBehaviour
{
    [Header("Boss Theme")]
    public AudioClip bossMusic;

    [Header("Behavior")]
    [Tooltip("If true, this trigger can only fire once (normal for boss arenas).")]
    public bool oneTimeOnly = true;

    // Optional: we can also tell the boss to activate here, if you want later
    // public BossAI bossAI;
    // public BossManager bossManager;

    private bool hasTriggered = false;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (oneTimeOnly && hasTriggered) return;

        hasTriggered = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StartBossMusic(bossMusic, true);
        }
        else
        {
            Debug.LogWarning("[BossMusicTrigger] No AudioManager in scene.");
        }

        // If you want to start boss logic here later:
        // bossAI?.ActivateBoss(other.transform);
        // bossManager?.ActivateBossUI();

        if (oneTimeOnly)
        {
            // You can optionally destroy this trigger so it doesn't retrigger.
            // Destroy(gameObject);
        }
    }
}
