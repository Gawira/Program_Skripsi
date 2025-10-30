using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class AreaMusicTrigger : MonoBehaviour
{
    [Header("Music to play")]
    public AudioClip areaMusic;

    [Header("Trigger Settings (in-game only)")]
    public bool oneTimeOnly = false;
    public bool destroyAfterTrigger = false;
    private bool hasTriggered = false;

    [Header("Main Menu Mode")]
    [Tooltip("If ON, this object only plays music when the active scene is the Main Menu.")]
    public bool mainMenuOnly = false;

    [Tooltip("Scene name to treat as Main Menu.")]
    public string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        // Make sure collider is trigger for in-game use; harmless in menu.
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void Start()
    {
        // If restricted to Main Menu, auto-play once on scene load (no Player needed)
        if (mainMenuOnly)
        {
            string active = SceneManager.GetActiveScene().name;
            if (string.Equals(active, mainMenuSceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                if (AudioManager.Instance != null && areaMusic != null)
                {
                    AudioManager.Instance.PlayAreaMusic(areaMusic, true);
                }
                else
                {
                    Debug.LogWarning("[AreaMusicTrigger] Missing AudioManager or clip for Main Menu playback.");
                }
            }

            // In Main Menu mode we don't use trigger logic at all.
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Trigger logic is only for in-game use
        if (mainMenuOnly) return;
        if (!other.CompareTag("Player")) return;
        if (oneTimeOnly && hasTriggered) return;

        hasTriggered = true;

        if (AudioManager.Instance != null && areaMusic != null)
        {
            AudioManager.Instance.PlayAreaMusic(areaMusic, true);
        }
        else
        {
            Debug.LogWarning("[AreaMusicTrigger] No AudioManager or clip assigned.");
        }

        if (oneTimeOnly && destroyAfterTrigger)
            Destroy(gameObject);
    }
}
