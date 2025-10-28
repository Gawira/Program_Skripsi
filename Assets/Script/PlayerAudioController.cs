using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class PlayerAudioController : MonoBehaviour
{
    [Header("Movement Loop")]
    [Tooltip("Looped movement sound (footsteps / cloth / armor while walking).")]
    public AudioClip walkLoopClip;

    [Header("Attack SFX Pool")]
    [Tooltip("All possible sword swing / slash sounds. One will be chosen randomly each attack.")]
    public AudioClip[] attackClips;

    [Header("Hurt SFX Pool")]
    [Tooltip("All possible hurt / damage grunt sounds. One will be chosen randomly each time you get hit.")]
    public AudioClip[] hurtClips;

    [Header("Walk Loop Settings")]
    [Tooltip("Minimum horizontal speed before we consider the player 'moving'.")]
    public float moveSpeedThreshold = 0.2f;

    [Tooltip("How fast to fade in/out the walk loop volume.")]
    public float walkFadeSpeed = 8f;

    private TPCharacter character;        // to read grounded & velocity
    private TPUserControl controller;     // to check dash/dashing state
    private Rigidbody rb;

    // private looping source just for movement
    private AudioSource walkSource;

    private void Awake()
    {
        controller = GetComponent<TPUserControl>();
        rb = GetComponent<Rigidbody>();

        // Make a child just to hold our looped walking audio source
        GameObject walkAudioObj = new GameObject("WalkLoop_AudioSource");
        walkAudioObj.transform.SetParent(transform);
        walkAudioObj.transform.localPosition = Vector3.zero;

        walkSource = walkAudioObj.AddComponent<AudioSource>();
        walkSource.loop = true;
        walkSource.playOnAwake = false;
        walkSource.spatialBlend = 0f; // 0 = 2D (UI style). Set 1f if you want 3D positional.
        walkSource.volume = 0f;       // we'll fade manually
    }

    private void Start()
    {
        if (walkLoopClip != null)
        {
            walkSource.clip = walkLoopClip;
        }
    }

    private void Update()
    {
        HandleWalkLoop();
    }

    private void HandleWalkLoop()
    {
        if (walkSource == null || rb == null) return;
        if (walkLoopClip == null) return;
        if (AudioManager.Instance == null) return;

        // read horizontal speed (ignore vertical so jumps don't count)
        Vector3 vel = rb.velocity;
        vel.y = 0f;
        float horizontalSpeed = vel.magnitude;

        bool isMoving = horizontalSpeed > moveSpeedThreshold;
        bool isDashing = (controller != null && controller.isDashing);

        // we only want the loop if we're moving and not dashing
        bool shouldPlay = isMoving && !isDashing;

        // make sure volume follows global SFX volume
        walkSource.volume = AudioManager.Instance.sfxVolume;

        if (shouldPlay)
        {
            if (!walkSource.isPlaying)
                walkSource.Play();
        }
        else
        {
            if (walkSource.isPlaying)
                walkSource.Stop();
        }
    }

    // -------------------------
    // Helpers to pick a random clip
    // -------------------------
    private AudioClip GetRandomFromArray(AudioClip[] pool)
    {
        if (pool == null || pool.Length == 0) return null;
        int i = Random.Range(0, pool.Length);
        return pool[i];
    }

    // -------------------------
    // Public calls for other scripts
    // -------------------------

    // Call this in TPCharacter when you attack
    public void PlayAttackSFX()
    {
        if (AudioManager.Instance == null) return;

        AudioClip clip = GetRandomFromArray(attackClips);
        if (clip == null) return;

        AudioManager.Instance.PlaySFX(clip);
    }

    // Call this in PlayerManager.TakeDamage() when you actually take damage
    public void PlayHurtSFX()
    {
        if (AudioManager.Instance == null) return;

        AudioClip clip = GetRandomFromArray(hurtClips);
        if (clip == null) return;

        AudioManager.Instance.PlaySFX(clip);
    }
}
