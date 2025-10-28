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
    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("3D Settings (used only if is3DSound = true)")]
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

        // make sure we don't accidentally fight with AudioManager
        src.playOnAwake = false;
        src.loop = true;
    }

    private void Start()
    {
        if (loopClip != null)
        {
            SetupSource();
            src.Play();
        }
        else
        {
            Debug.LogWarning($"[AmbientLoop] No clip assigned on {name}.");
        }
    }

    private void OnValidate()
    {
        // This runs in editor too, so you can preview settings live.
        if (src == null)
            src = GetComponent<AudioSource>();

        SetupSource();
    }

    private void SetupSource()
    {
        if (src == null) return;

        src.clip = loopClip;
        src.volume = volume;

        if (is3DSound)
        {
            // positional / spatial sound
            src.spatialBlend = 1f; // 1 = 3D
            src.minDistance = minDistance;
            src.maxDistance = maxDistance;
            src.rolloffMode = rolloffMode;
        }
        else
        {
            // global / screen-space sound
            src.spatialBlend = 0f; // 0 = 2D
        }
    }
}
