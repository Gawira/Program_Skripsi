using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AmbientLoop : MonoBehaviour
{
    [Header("Clip")]
    public AudioClip loopClip;

    [Header("Type")]
    [Tooltip("3D = positional (e.g. waterfall). 2D = global ambience (e.g. rain).")]
    public bool is3DSound = true;

    [Header("Volume")]
    [Range(0f, 1f)] public float volume = 1f;   // local multiplier for this loop

    [Header("Follow SFX Slider")]
    [Tooltip("If ON, this loop is scaled by AudioManager.sfxVolume.")]
    public bool useSFXVolume = true;

    [Header("3D Settings (if is3DSound = true)")]
    [Tooltip("How far the sound can be heard at full volume.")]
    public float minDistance = 2f;
    [Tooltip("Beyond this distance it fades out to 0.")]
    public float maxDistance = 25f;
    [Tooltip("Linear = nice for ambience. Logarithmic = default Unity falloff.")]
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;

    private AudioSource src;

    private void Awake()
    {
        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        Apply3DSettings();
    }

    private void Start()
    {
        if (loopClip != null)
        {
            src.clip = loopClip;
            UpdateVolumeFromAudioManager();
            src.Play();
        }
        else
        {
            Debug.LogWarning($"[AmbientLoop] No clip assigned on {name}.");
        }
    }

    private void Update()
    {
        // Continuously follow AudioManager SFX volume in real time
        UpdateVolumeFromAudioManager();
    }

    private void OnValidate()
    {
        if (src == null) src = GetComponent<AudioSource>();
        Apply3DSettings();
        UpdateVolumeFromAudioManager();
    }

    private void Apply3DSettings()
    {
        if (src == null) return;

        if (is3DSound)
        {
            src.spatialBlend = 1f; // 3D
            src.minDistance = minDistance;
            src.maxDistance = maxDistance;
            src.rolloffMode = rolloffMode;
        }
        else
        {
            src.spatialBlend = 0f; // 2D
        }
    }

    private void UpdateVolumeFromAudioManager()
    {
        if (src == null) return;

        float sfx = 1f;
        if (useSFXVolume && AudioManager.Instance != null)
        {
            sfx = Mathf.Clamp01(AudioManager.Instance.sfxVolume);
        }

        // Final output = local volume * global SFX volume
        src.volume = Mathf.Clamp01(volume * sfx);
    }
}
