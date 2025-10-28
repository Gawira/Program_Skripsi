using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AreaMusicTrigger : MonoBehaviour
{
    [Header("Music to play in this area")]
    public AudioClip areaMusic;

    [Header("Settings")]
    [Tooltip("Should this trigger only work once? (good for intro areas)")]
    public bool oneTimeOnly = false;

    [Tooltip("If true, this trigger destroys itself after playing once.")]
    public bool destroyAfterTrigger = false;

    private bool hasTriggered = false;

    private void Reset()
    {
        // Auto-set collider to trigger when you add this script
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
            AudioManager.Instance.PlayAreaMusic(areaMusic, true);
        }
        else
        {
            Debug.LogWarning("[AreaMusicTrigger] No AudioManager in scene.");
        }

        if (oneTimeOnly && destroyAfterTrigger)
        {
            Destroy(gameObject);
        }
    }
}
