using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class PlayerAudioController : MonoBehaviour
{
    [Header("Movement Loop")]
    public AudioClip walkLoopClip;

    [Header("Attack SFX Pool")]
    public AudioClip[] attackClips;

    [Header("Hurt SFX Pool")]
    public AudioClip[] hurtClips;

    [Header("Skill SFX")]
    [Tooltip("Played once when the skill gauge becomes full/ready.")]
    public AudioClip skillReadyClip;
    [Tooltip("Played when the player activates the skill.")]
    public AudioClip skillUseClip;

    [Header("Walk Loop Settings")]
    public float moveSpeedThreshold = 0.2f;
    public float walkFadeSpeed = 8f;

    private TPCharacter character;
    private TPUserControl controller;
    private Rigidbody rb;

    private AudioSource walkSource;

    private void Awake()
    {
        controller = GetComponent<TPUserControl>();
        rb = GetComponent<Rigidbody>();

        GameObject walkAudioObj = new GameObject("WalkLoop_AudioSource");
        walkAudioObj.transform.SetParent(transform);
        walkAudioObj.transform.localPosition = Vector3.zero;

        walkSource = walkAudioObj.AddComponent<AudioSource>();
        walkSource.loop = true;
        walkSource.playOnAwake = false;
        walkSource.spatialBlend = 0f;
        walkSource.volume = 0f;
    }

    private void Start()
    {
        if (walkLoopClip != null) walkSource.clip = walkLoopClip;
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

        Vector3 vel = rb.velocity; vel.y = 0f;
        bool isMoving = vel.magnitude > moveSpeedThreshold;
        bool isDashing = (controller != null && controller.isDashing);

        bool shouldPlay = isMoving && !isDashing;

        walkSource.volume = AudioManager.Instance.sfxVolume;

        if (shouldPlay)
        {
            if (!walkSource.isPlaying) walkSource.Play();
        }
        else
        {
            if (walkSource.isPlaying) walkSource.Stop();
        }
    }

    private AudioClip GetRandomFromArray(AudioClip[] pool)
    {
        if (pool == null || pool.Length == 0) return null;
        int i = Random.Range(0, pool.Length);
        return pool[i];
    }

    // ------------ Public API ------------
    public void PlayAttackSFX()
    {
        if (AudioManager.Instance == null) return;
        var clip = GetRandomFromArray(attackClips);
        if (clip != null) AudioManager.Instance.PlaySFX(clip);
    }

    public void PlayHurtSFX()
    {
        if (AudioManager.Instance == null) return;
        var clip = GetRandomFromArray(hurtClips);
        if (clip != null) AudioManager.Instance.PlaySFX(clip);
    }

    public void PlaySkillReadySFX()
    {
        if (AudioManager.Instance == null || skillReadyClip == null) return;
        AudioManager.Instance.PlaySFX(skillReadyClip);
    }

    public void PlaySkillUseSFX()
    {
        if (AudioManager.Instance == null || skillUseClip == null) return;
        AudioManager.Instance.PlaySFX(skillUseClip);
    }
}
