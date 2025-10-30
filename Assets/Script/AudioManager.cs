using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Volume (0-1)")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Current Music State (debug)")]
    [SerializeField] private AudioClip currentMusicClip;
    [SerializeField] private bool bossMusicActive = false;

    private AudioClip lastAreaMusicClip;
    private bool lastAreaMusicLoop = true;

    // ---------- Scene auto-mute ----------
    [Header("Scene Auto-Mute")]
    [Tooltip("If scene name matches any of these (by contains or exact), music will be stopped and clip cleared.")]
    public string[] muteScenes = new[] { "Intro", "Outro", "Cutscene" };

    [Tooltip("If true, uses 'contains' (case-insensitive). If false, requires exact match.")]
    public bool matchByContains = true;

    [Tooltip("Also mute the SFX source while in a muted scene.")]
    public bool muteSFXInMuteScenes = false;

    // Events for UI binders (scene sliders)
    public event Action<float> OnMusicVolumeChanged;
    public event Action<float> OnSFXVolumeChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
        {
            var m = new GameObject("MusicSource");
            m.transform.SetParent(transform);
            musicSource = m.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        if (sfxSource == null)
        {
            var s = new GameObject("SFXSource");
            s.transform.SetParent(transform);
            sfxSource = s.AddComponent<AudioSource>();
            sfxSource.loop = false;
        }

        ApplyVolume();

        // Auto-bind sliders & handle scene-based muting
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Bind any sliders present in this scene
        var binders = FindObjectsOfType<AudioSliderBinder>(true);
        foreach (var b in binders) b.TryBind();

        // Auto-mute logic for cutscene-like scenes
        bool shouldMute = IsMuteScene(scene.name);
        if (shouldMute)
        {
            StopMusic();                    // stops playback
            if (musicSource) musicSource.clip = null; // clear current clip
            currentMusicClip = null;

            if (muteSFXInMuteScenes && sfxSource)
                sfxSource.mute = true;
        }
        else
        {
            // ensure SFX is audible again when leaving muted scenes
            if (sfxSource) sfxSource.mute = false;
        }
    }

    private bool IsMuteScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName) || muteScenes == null) return false;

        foreach (var key in muteScenes)
        {
            if (string.IsNullOrEmpty(key)) continue;

            if (matchByContains)
            {
                if (sceneName.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            else
            {
                if (string.Equals(sceneName, key, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        return false;
    }

    private void ApplyVolume()
    {
        if (musicSource) musicSource.volume = musicVolume;
        if (sfxSource) sfxSource.volume = sfxVolume;
    }

    // -------- Slider APIs --------
    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        if (musicSource) musicSource.volume = musicVolume;
        OnMusicVolumeChanged?.Invoke(musicVolume);
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        if (sfxSource) sfxSource.volume = sfxVolume;
        OnSFXVolumeChanged?.Invoke(sfxVolume);
    }

    // -------- Music control --------
    public void PlayAreaMusic(AudioClip clip, bool loop = true)
    {
        if (!clip) return;
        lastAreaMusicClip = clip; lastAreaMusicLoop = loop;
        bossMusicActive = false;
        PlayMusicInternal(clip, loop);
    }
    public void StartBossMusic(AudioClip bossClip, bool loop = true)
    {
        if (!bossClip) return;
        bossMusicActive = true;
        PlayMusicInternal(bossClip, loop);
    }
    public void StopBossMusic()
    {
        bossMusicActive = false;
        if (lastAreaMusicClip) PlayMusicInternal(lastAreaMusicClip, lastAreaMusicLoop);
        else StopMusic();
    }
    public void StopMusic()
    {
        if (musicSource)
        {
            musicSource.Stop();
            currentMusicClip = null;
        }
    }
    private void PlayMusicInternal(AudioClip clip, bool loop)
    {
        if (!musicSource) return;
        if (currentMusicClip == clip && musicSource.isPlaying) return;
        currentMusicClip = clip;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    // -------- SFX helpers --------
    public void PlaySFX(AudioClip clip)
    {
        if (!clip || !sfxSource) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
    public void PlaySFXAtPoint(AudioClip clip, Vector3 pos, float spatialBlend = 1f)
    {
        if (!clip) return;
        var go = new GameObject("SFX_OneShot");
        go.transform.position = pos;
        var a = go.AddComponent<AudioSource>();
        a.clip = clip; a.volume = sfxVolume; a.spatialBlend = Mathf.Clamp01(spatialBlend);
        a.minDistance = 1f; a.maxDistance = 25f; a.rolloffMode = AudioRolloffMode.Linear;
        a.Play();
        Destroy(go, clip.length + 0.1f);
    }

    // QoL triggers
    public void TriggerAreaMusic(AudioClip clip) => PlayAreaMusic(clip, true);
    public void TriggerBossStart(AudioClip clip) => StartBossMusic(clip, true);
    public void TriggerBossEnd() => StopBossMusic();
}
