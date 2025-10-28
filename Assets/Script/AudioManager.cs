using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // -------------------------
    // Singleton (global access)
    // -------------------------
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("This source is used for background music / area themes / boss themes.")]
    public AudioSource musicSource;

    [Tooltip("This source is used for one-shot sound effects.")]
    public AudioSource sfxSource;

    [Header("Volume (0-1)")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Current Music State (debug)")]
    [SerializeField] private AudioClip currentMusicClip;
    [SerializeField] private bool bossMusicActive = false;

    // we remember the last "area" music so we can go back to it after boss
    private AudioClip lastAreaMusicClip;
    private bool lastAreaMusicLoop = true;

    private void Awake()
    {
        // basic singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // safety: if sources are not assigned in inspector, auto-create them
        if (musicSource == null)
        {
            GameObject m = new GameObject("MusicSource");
            m.transform.SetParent(transform);
            musicSource = m.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        if (sfxSource == null)
        {
            GameObject s = new GameObject("SFXSource");
            s.transform.SetParent(transform);
            sfxSource = s.AddComponent<AudioSource>();
            sfxSource.loop = false;
        }

        ApplyVolume(); // sync initial volume
    }

    private void Update()
    {
        // safety: make sure volume applies in realtime
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume;

        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    // =========================================================
    // =============  PUBLIC API: VOLUME SLIDERS  ==============
    // =========================================================
    //
    // Hook these into your UI sliders:
    // - OnValueChanged(float) ? drag AudioManager in ? choose SetMusicVolume
    // - OnValueChanged(float) ? drag AudioManager in ? choose SetSFXVolume

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        ApplyVolume();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        ApplyVolume();
    }

    // =========================================================
    // =============  PUBLIC API: MUSIC CONTROL  ===============
    // =========================================================
    //
    // You call these from:
    // - trigger collider scripts (OnTriggerEnter)
    // - Timeline / Animation Events
    // - boss start / boss end
    //
    // Example:
    //   AudioManager.Instance.PlayAreaMusic(area1Clip, true);
    //   AudioManager.Instance.StartBossMusic(bossClip);
    //   AudioManager.Instance.StopBossMusic();  // go back to area music

    /// <summary>
    /// Play / switch the normal area / ambient / exploration music.
    /// Call this when player enters a new zone.
    /// </summary>
    public void PlayAreaMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        lastAreaMusicClip = clip;
        lastAreaMusicLoop = loop;

        bossMusicActive = false; // we're in normal mode again

        PlayMusicInternal(clip, loop);
    }

    /// <summary>
    /// Call this when a boss fight starts.
    /// Overrides the current area music.
    /// </summary>
    public void StartBossMusic(AudioClip bossClip, bool loop = true)
    {
        if (bossClip == null) return;

        bossMusicActive = true;
        PlayMusicInternal(bossClip, loop);
    }

    /// <summary>
    /// Call this when the boss fight ends.
    /// Goes back to whatever area track was playing before.
    /// </summary>
    public void StopBossMusic()
    {
        bossMusicActive = false;

        // if we have a remembered area music, go back to that
        if (lastAreaMusicClip != null)
        {
            PlayMusicInternal(lastAreaMusicClip, lastAreaMusicLoop);
        }
        else
        {
            // optional: stop all music if no fallback
            StopMusic();
        }
    }

    /// <summary>
    /// Immediately stop any music.
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            currentMusicClip = null;
        }
    }

    // --- internal helper ---
    private void PlayMusicInternal(AudioClip clip, bool loop)
    {
        if (musicSource == null) return;

        // if it's already playing that clip, do nothing
        if (currentMusicClip == clip && musicSource.isPlaying)
            return;

        currentMusicClip = clip;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    // =========================================================
    // =============  PUBLIC API: SOUND EFFECTS  ===============
    // =========================================================
    //
    // You call this for:
    // - attack sounds
    // - UI click / menu select
    // - item pickup
    // - door open
    //
    // YOU: Just drag AudioManager into OnClick(), then choose PlaySFX
    // and assign the clip param in the Inspector event.

    /// <summary>
    /// Play a 2D UI / global sound effect (no positional audio).
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary>
    /// Play a 3D sound at a world position (like enemy roar, hit impact in world).
    /// This does not reuse the main sfxSource, it spawns a temp AudioSource.
    /// </summary>
    public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float spatialBlend = 1f)
    {
        if (clip == null) return;

        GameObject temp = new GameObject("SFX_OneShot");
        temp.transform.position = position;

        AudioSource a = temp.AddComponent<AudioSource>();
        a.clip = clip;
        a.volume = sfxVolume;
        a.spatialBlend = Mathf.Clamp01(spatialBlend); // 0 = 2D, 1 = 3D
        a.minDistance = 1f;
        a.maxDistance = 25f;
        a.rolloffMode = AudioRolloffMode.Linear;
        a.Play();

        // destroy when finished
        Destroy(temp, clip.length + 0.1f);
    }

    // =========================================================
    // =============  QUALITY OF LIFE HELPERS  =================
    // =========================================================

    /// <summary>
    /// Call this from a trigger collider (OnTriggerEnter) to change area music.
    /// Example usage in your trigger script:
    ///     AudioManager.Instance.PlayAreaMusic(areaClip, true);
    /// </summary>
    public void TriggerAreaMusic(AudioClip newAreaClip)
    {
        PlayAreaMusic(newAreaClip, true);
    }

    /// <summary>
    /// Call this exactly when boss starts.
    /// </summary>
    public void TriggerBossStart(AudioClip bossClip)
    {
        StartBossMusic(bossClip, true);
    }

    /// <summary>
    /// Call this exactly when boss ends / dies.
    /// </summary>
    public void TriggerBossEnd()
    {
        StopBossMusic();
    }
}
