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

    [Header("Fade")]
    public bool useFade = true;
    [Tooltip("Seconds to fade out the current music.")]
    public float fadeOut = 0.5f;
    [Tooltip("Seconds to fade in the new music.")]
    public float fadeIn = 0.5f;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void Start()
    {
        if (mainMenuOnly)
        {
            string active = SceneManager.GetActiveScene().name;
            if (string.Equals(active, mainMenuSceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                Play(areaMusic);
            }
            // In Main Menu mode we don't use trigger logic at all.
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (mainMenuOnly) return;
        if (!other.CompareTag("Player")) return;
        if (oneTimeOnly && hasTriggered) return;

        hasTriggered = true;
        Play(areaMusic);

        if (oneTimeOnly && destroyAfterTrigger)
            Destroy(gameObject);
    }

    private void Play(AudioClip clip)
    {
        if (AudioManager.Instance == null || clip == null)
        {
            Debug.LogWarning("[AreaMusicTrigger] Missing AudioManager or clip.");
            return;
        }

        if (useFade)
            AudioManager.Instance.PlayAreaMusicWithFade(clip, fadeOut, fadeIn, true);
        else
            AudioManager.Instance.PlayAreaMusic(clip, true);
    }
}
